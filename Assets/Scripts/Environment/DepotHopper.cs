/*
 * Created by 11Hopem
 * Copyright (c) 2026 11Hopem. All rights reserved.
 */

using System;
using System.Collections.Generic;
using UnityEngine;
using ElevenHopem.MoonSweepers.Core;
using ElevenHopem.MoonSweepers.Player;

namespace ElevenHopem.MoonSweepers.Environment
{
    public class DepotHopper : MonoBehaviour
    {
        [SerializeField] private float depositRadius = 4.6f;
        [SerializeField] private Light depotBeaconLight;
        [SerializeField] private ParticleSystem unloadParticles;

        private bool playerInRange = false;
        private PlayerInventory nearbyInventory;
        private Transform crusherRollerA;
        private Transform crusherRollerB;
        private Transform beaconSpinner;
        private Renderer holoRingRenderer;
        private Material ringIdleMat;
        private Material ringActiveMat;

        public bool PlayerInRange => playerInRange;

        public event Action<float, long> OnDustDeposited;

        private void Awake()
        {
            gameObject.tag = "Depot";

            MeshRenderer rootMr = GetComponent<MeshRenderer>();
            if (rootMr != null) Destroy(rootMr);
            MeshFilter rootMf = GetComponent<MeshFilter>();
            if (rootMf != null) Destroy(rootMf);

            Collider[] existingCols = GetComponents<Collider>();
            for (int i = 0; i < existingCols.Length; i++)
            {
                if (!(existingCols[i] is SphereCollider))
                {
                    Destroy(existingCols[i]);
                }
            }

            transform.localScale = Vector3.one;
            transform.position = new Vector3(0f, 0f, 8.0f);

            SphereCollider col = GetComponent<SphereCollider>() ?? gameObject.AddComponent<SphereCollider>();
            col.isTrigger = true;
            col.center = new Vector3(0f, 1.0f, 0f);
            col.radius = depositRadius;

            BuildIndustrialDepotArchitecture();
        }

        private void BuildIndustrialDepotArchitecture()
        {
            Transform existing = transform.Find("DepotStructure");
            if (existing != null) Destroy(existing.gameObject);

            GameObject root = new GameObject("DepotStructure");
            root.transform.SetParent(transform, false);

            Material steelDark = LunarMaterialLibrary.GetSurfaceMaterial("DepotSteelDark", new Color(0.18f, 0.21f, 0.26f), 0.82f, 0.62f, true);
            Material steelLight = LunarMaterialLibrary.GetSurfaceMaterial("DepotSteelLight", new Color(0.68f, 0.72f, 0.78f), 0.75f, 0.68f, true);
            Material hazardMat = LunarMaterialLibrary.GetHazardMaterial();
            ringIdleMat = LunarMaterialLibrary.GetEmissiveMaterial("DepotRingCyan", new Color(0.1f, 0.85f, 1f), new Color(0.15f, 1.35f, 1.8f));
            ringActiveMat = LunarMaterialLibrary.GetEmissiveMaterial("DepotRingGreen", new Color(0.15f, 1f, 0.45f), new Color(0.2f, 1.8f, 0.65f));
            Material coreGlass = LunarMaterialLibrary.GetTransparentMaterial("SiloGlass", new Color(0.15f, 0.75f, 0.95f, 0.35f), new Color(0.1f, 0.65f, 0.9f));

            GameObject deck = CreatePart(PrimitiveType.Cylinder, "HexPadDeck", root.transform, new Vector3(0f, 0.08f, 0f), new Vector3(6.6f, 0.08f, 6.6f), steelDark, true);
            CreatePart(PrimitiveType.Cylinder, "HazardTrimOuter", root.transform, new Vector3(0f, 0.05f, 0f), new Vector3(7.1f, 0.05f, 7.1f), hazardMat, true);

            GameObject ring = CreatePart(PrimitiveType.Cylinder, "ProximityHoloRing", root.transform, new Vector3(0f, 0.165f, 0f), new Vector3(5.8f, 0.015f, 5.8f), ringIdleMat, false);
            holoRingRenderer = ring.GetComponent<Renderer>();
            CreatePart(PrimitiveType.Cylinder, "InnerDeckMask", root.transform, new Vector3(0f, 0.175f, 0f), new Vector3(5.35f, 0.015f, 5.35f), steelDark, false);

            for (int side = -1; side <= 1; side += 2)
            {
                CreatePart(PrimitiveType.Cube, $"HopperWallX_{side}", root.transform, new Vector3(side * 1.45f, 0.45f, -0.3f), new Vector3(0.28f, 0.58f, 2.4f), hazardMat, true);
                CreatePart(PrimitiveType.Cube, $"HopperWallZ_{side}", root.transform, new Vector3(0f, 0.45f, -0.3f + side * 1.15f), new Vector3(3.15f, 0.58f, 0.28f), steelLight, true);
            }

            GameObject rollA = CreatePart(PrimitiveType.Cylinder, "CrusherDrumA", root.transform, new Vector3(-0.42f, 0.38f, -0.3f), new Vector3(0.42f, 0.98f, 0.42f), steelLight, false);
            rollA.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
            crusherRollerA = rollA.transform;

            GameObject rollB = CreatePart(PrimitiveType.Cylinder, "CrusherDrumB", root.transform, new Vector3(0.42f, 0.38f, -0.3f), new Vector3(0.42f, 0.98f, 0.42f), steelLight, false);
            rollB.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
            crusherRollerB = rollB.transform;

            CreatePart(PrimitiveType.Cylinder, "SiloBase", root.transform, new Vector3(0f, 0.9f, 2.15f), new Vector3(2.1f, 0.75f, 2.1f), steelDark, true);
            CreatePart(PrimitiveType.Cylinder, "SiloGlassChamber", root.transform, new Vector3(0f, 2.55f, 2.15f), new Vector3(1.65f, 1.15f, 1.65f), coreGlass, false);
            CreatePart(PrimitiveType.Cylinder, "SiloEnergyCore", root.transform, new Vector3(0f, 2.55f, 2.15f), new Vector3(0.65f, 1.05f, 0.65f), ringIdleMat, false);
            CreatePart(PrimitiveType.Cylinder, "SiloCap", root.transform, new Vector3(0f, 3.85f, 2.15f), new Vector3(2.0f, 0.22f, 2.0f), steelLight, true);

            for (int p = 0; p < 4; p++)
            {
                float ang = (p * 90f + 45f) * Mathf.Deg2Rad;
                Vector3 strutPos = new Vector3(Mathf.Cos(ang) * 0.92f, 2.55f, 2.15f + Mathf.Sin(ang) * 0.92f);
                CreatePart(PrimitiveType.Cylinder, $"SiloStrut_{p}", root.transform, strutPos, new Vector3(0.11f, 1.25f, 0.11f), steelDark, false);
            }

            GameObject spinner = new GameObject("BeaconSpinner");
            spinner.transform.SetParent(root.transform, false);
            spinner.transform.localPosition = new Vector3(0f, 4.35f, 2.15f);
            beaconSpinner = spinner.transform;
            CreatePart(PrimitiveType.Sphere, "BeaconDome", spinner.transform, Vector3.zero, new Vector3(0.55f, 0.55f, 0.55f), ringIdleMat, false);

            if (depotBeaconLight == null)
            {
                Transform oldLight = transform.Find("BeaconLight");
                if (oldLight != null)
                {
                    depotBeaconLight = oldLight.GetComponent<Light>();
                }
            }
            if (depotBeaconLight == null)
            {
                GameObject lightObj = new GameObject("DepotPointLight");
                lightObj.transform.SetParent(root.transform, false);
                lightObj.transform.localPosition = new Vector3(0f, 4.6f, 1.5f);
                depotBeaconLight = lightObj.AddComponent<Light>();
                depotBeaconLight.type = LightType.Point;
            }
            depotBeaconLight.color = new Color(0.2f, 0.9f, 1.0f);
            depotBeaconLight.range = 20f;
            depotBeaconLight.intensity = 2.4f;

            GameObject pObj = new GameObject("UnloadBurstParticles");
            pObj.transform.SetParent(root.transform, false);
            pObj.transform.localPosition = new Vector3(0f, 0.65f, -0.3f);
            pObj.transform.localRotation = Quaternion.Euler(-90f, 0f, 0f);
            unloadParticles = pObj.AddComponent<ParticleSystem>();
            var main = unloadParticles.main;
            main.loop = false;
            main.playOnAwake = false;
            main.duration = 0.6f;
            main.startLifetime = 0.85f;
            main.startSpeed = 5.5f;
            main.startSize = 0.24f;
            main.startColor = new Color(0.25f, 1.0f, 0.65f, 0.95f);
            var emission = unloadParticles.emission;
            emission.rateOverTime = 0f;
            emission.SetBursts(new ParticleSystem.Burst[] { new ParticleSystem.Burst(0f, 36) });
            var shape = unloadParticles.shape;
            shape.shapeType = ParticleSystemShapeType.Cone;
            shape.angle = 26f;
            shape.radius = 0.65f;
            ParticleSystemRenderer psr = pObj.GetComponent<ParticleSystemRenderer>();
            psr.sharedMaterial = LunarMaterialLibrary.GetParticleMaterial();
        }

        private static GameObject CreatePart(PrimitiveType type, string name, Transform parent, Vector3 pos, Vector3 scale, Material mat, bool keepCollider)
        {
            GameObject go = GameObject.CreatePrimitive(type);
            go.name = name;
            go.transform.SetParent(parent, false);
            go.transform.localPosition = pos;
            go.transform.localScale = scale;
            if (!keepCollider)
            {
                Collider c = go.GetComponent<Collider>();
                if (c != null) Destroy(c);
            }
            Renderer r = go.GetComponent<Renderer>();
            if (r != null)
            {
                r.sharedMaterial = mat;
                r.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.On;
                r.receiveShadows = true;
            }
            return go;
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Player"))
            {
                playerInRange = true;
                nearbyInventory = other.GetComponent<PlayerInventory>() ?? other.GetComponentInParent<PlayerInventory>();
                if (depotBeaconLight != null) depotBeaconLight.color = new Color(0.25f, 1f, 0.55f);
                if (holoRingRenderer != null) holoRingRenderer.sharedMaterial = ringActiveMat;
            }
        }

        private void OnTriggerExit(Collider other)
        {
            if (other.CompareTag("Player"))
            {
                playerInRange = false;
                nearbyInventory = null;
                if (depotBeaconLight != null) depotBeaconLight.color = new Color(0.2f, 0.9f, 1.0f);
                if (holoRingRenderer != null) holoRingRenderer.sharedMaterial = ringIdleMat;
            }
        }

        private void Update()
        {
            if (crusherRollerA != null) crusherRollerA.Rotate(0f, 140f * Time.deltaTime, 0f, Space.Self);
            if (crusherRollerB != null) crusherRollerB.Rotate(0f, -140f * Time.deltaTime, 0f, Space.Self);
            if (beaconSpinner != null) beaconSpinner.Rotate(0f, 95f * Time.deltaTime, 0f, Space.Self);

            if (nearbyInventory == null)
            {
                PlayerInventory inv = FindFirstObjectByType<PlayerInventory>();
                if (inv != null)
                {
                    float dist = Vector3.Distance(inv.transform.position, transform.position);
                    playerInRange = dist <= depositRadius;
                    if (playerInRange)
                    {
                        nearbyInventory = inv;
                        if (holoRingRenderer != null) holoRingRenderer.sharedMaterial = ringActiveMat;
                    }
                    else if (holoRingRenderer != null)
                    {
                        holoRingRenderer.sharedMaterial = ringIdleMat;
                    }
                }
            }

            if (playerInRange && nearbyInventory != null)
            {
                if (Input.GetKeyDown(KeyCode.E) || nearbyInventory.CurrentDustKg > 0.05f || nearbyInventory.TotalMineralCount > 0)
                {
                    ExecuteDeposit();
                }
            }
        }

        public void ExecuteDeposit()
        {
            if (nearbyInventory == null) return;

            float dustAmount = nearbyInventory.UnloadAllDust();
            List<MineralItem> minerals = nearbyInventory.UnloadAllMinerals();

            if (dustAmount <= 0.01f && minerals.Count == 0) return;

            float baseRate = GameManager.Instance != null ? GameManager.Instance.BaseDustValuePerKg : 3.5f;
            long dustCredits = (long)Mathf.Max(1f, Mathf.Round(dustAmount * baseRate));
            if (dustAmount <= 0.01f) dustCredits = 0;

            long mineralCredits = 0;
            foreach (var m in minerals)
            {
                mineralCredits += (long)Mathf.Round(m.count * m.valueMultiplier);
            }

            long totalCredits = dustCredits + mineralCredits;
            GameManager.Instance?.AddCredits(totalCredits);
            AudioManager.Instance?.PlayCoinChime();

            if (unloadParticles != null)
            {
                unloadParticles.Play();
            }

            string bonusNote = mineralCredits > 0 ? $" (incl. +{mineralCredits} LC minerals)" : "";
            GameManager.Instance?.NotifyToast($"Deposited {dustAmount:F1} kg Regolith -> +{totalCredits:N0} LC{bonusNote}");
            OnDustDeposited?.Invoke(dustAmount, totalCredits);
        }
    }
}
