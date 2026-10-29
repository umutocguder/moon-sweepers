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
    public class RegolithDustPatch : MonoBehaviour
    {
        [SerializeField] private float maxDustMassKg = 14f;
        [SerializeField] private float currentDustMassKg = 14f;
        [SerializeField] private bool canDropMinerals = true;
        [SerializeField] private float mineralDropChance = 0.28f;
        [SerializeField] private bool autoRespawn = true;
        [SerializeField] private float respawnCooldownSeconds = 55f;

        private Transform visualClusterRoot;
        private readonly List<Renderer> clusterRenderers = new List<Renderer>();
        private Transform embeddedCrystal;
        private SphereCollider hitCollider;
        private Vector3 baseClusterScale = Vector3.one;
        private float respawnTimer = 0f;
        private int mineralTier = 0;
        private bool registeredWithManager = false;

        public float CurrentDustMassKg => currentDustMassKg;
        public float MaxDustMassKg => maxDustMassKg;
        public bool IsFullyCleaned => currentDustMassKg <= 0.05f;
        public bool HasEmbeddedMineral => mineralTier > 0;

        public event Action<float> OnDustCleaned;

        private void Awake()
        {
            gameObject.tag = "DustPatch";
            hitCollider = GetComponent<SphereCollider>();
            if (hitCollider == null)
            {
                Collider existing = GetComponent<Collider>();
                if (existing != null)
                {
                    Destroy(existing);
                }
                hitCollider = gameObject.AddComponent<SphereCollider>();
            }
            hitCollider.isTrigger = true;
            hitCollider.radius = 1.05f;
            hitCollider.center = new Vector3(0f, 0.22f, 0f);
        }

        private void Start()
        {
            if (!registeredWithManager && GameManager.Instance != null)
            {
                GameManager.Instance.RegisterDustPatch();
                registeredWithManager = true;
            }

            if (visualClusterRoot == null)
            {
                ConfigureVisuals(transform.GetSiblingIndex() + 1, 1.8f, 14f);
            }
        }

        public void ConfigureVisuals(int seed, float radiusScale, float massKg)
        {
            maxDustMassKg = massKg;
            currentDustMassKg = massKg;

            MeshRenderer rootMr = GetComponent<MeshRenderer>();
            if (rootMr != null) Destroy(rootMr);
            MeshFilter rootMf = GetComponent<MeshFilter>();
            if (rootMf != null) Destroy(rootMf);

            if (visualClusterRoot != null)
            {
                Destroy(visualClusterRoot.gameObject);
            }
            clusterRenderers.Clear();

            GameObject root = new GameObject("RegolithCluster");
            root.transform.SetParent(transform, false);
            visualClusterRoot = root.transform;
            baseClusterScale = new Vector3(radiusScale, Mathf.Lerp(0.85f, 1.35f, (radiusScale - 1.1f) * 0.5f), radiusScale);
            visualClusterRoot.localScale = baseClusterScale;

            Material dustMat = LunarMaterialLibrary.GetDustMoundMaterial();

            GameObject mainMound = new GameObject("PrimaryMound");
            mainMound.transform.SetParent(visualClusterRoot, false);
            MeshFilter mf = mainMound.AddComponent<MeshFilter>();
            mf.sharedMesh = LunarMaterialLibrary.CreateDustMoundMesh(seed);
            MeshRenderer mr = mainMound.AddComponent<MeshRenderer>();
            mr.sharedMaterial = dustMat;
            mr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.On;
            mr.receiveShadows = true;
            clusterRenderers.Add(mr);

            int subCount = 2 + (seed % 2);
            for (int i = 0; i < subCount; i++)
            {
                float ang = (i / (float)subCount) * Mathf.PI * 2f + seed * 0.7f;
                float dist = 0.52f + (i % 2) * 0.16f;
                GameObject sub = new GameObject($"SubMound_{i}");
                sub.transform.SetParent(visualClusterRoot, false);
                sub.transform.localPosition = new Vector3(Mathf.Cos(ang) * dist, -0.01f, Mathf.Sin(ang) * dist);
                sub.transform.localRotation = Quaternion.Euler(0f, seed * 37f + i * 65f, 0f);
                float s = 0.48f + ((seed + i) % 3) * 0.11f;
                sub.transform.localScale = new Vector3(s, s * 0.85f, s);

                MeshFilter smf = sub.AddComponent<MeshFilter>();
                smf.sharedMesh = LunarMaterialLibrary.CreateDustMoundMesh(seed * 7 + i + 3);
                MeshRenderer smr = sub.AddComponent<MeshRenderer>();
                smr.sharedMaterial = dustMat;
                smr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.On;
                smr.receiveShadows = true;
                clusterRenderers.Add(smr);
            }

            float roll = Mathf.Abs(Mathf.Sin(seed * 12.9898f) * 43758.5453f) % 1f;
            if (canDropMinerals && roll < mineralDropChance)
            {
                if (roll < 0.05f) mineralTier = 3;
                else if (roll < 0.14f) mineralTier = 2;
                else mineralTier = 1;

                GameObject crystal = GameObject.CreatePrimitive(PrimitiveType.Cube);
                crystal.name = "EmbeddedMineral";
                Collider cCol = crystal.GetComponent<Collider>();
                if (cCol != null) Destroy(cCol);

                crystal.transform.SetParent(visualClusterRoot, false);
                crystal.transform.localPosition = new Vector3(0.12f, 0.24f, -0.08f);
                crystal.transform.localRotation = Quaternion.Euler(32f, seed * 29f, 41f);
                crystal.transform.localScale = new Vector3(0.16f, 0.28f, 0.16f);

                Renderer cr = crystal.GetComponent<Renderer>();
                if (mineralTier == 3)
                {
                    cr.sharedMaterial = LunarMaterialLibrary.GetEmissiveMaterial("RelicGold", new Color(1f, 0.82f, 0.2f), new Color(1.4f, 0.95f, 0.15f));
                }
                else if (mineralTier == 2)
                {
                    cr.sharedMaterial = LunarMaterialLibrary.GetEmissiveMaterial("HeliumCrystal", new Color(0.2f, 0.9f, 1f), new Color(0.15f, 1.25f, 1.5f));
                }
                else
                {
                    cr.sharedMaterial = LunarMaterialLibrary.GetSurfaceMaterial("TitaniumChunkMat", new Color(0.78f, 0.83f, 0.90f), 0.88f, 0.78f);
                }
                clusterRenderers.Add(cr);
                embeddedCrystal = crystal.transform;
            }

            if (hitCollider != null)
            {
                hitCollider.radius = Mathf.Max(0.85f, radiusScale * 0.92f);
            }
        }

        private void Update()
        {
            if (embeddedCrystal != null && !IsFullyCleaned)
            {
                embeddedCrystal.Rotate(Vector3.up, 28f * Time.deltaTime, Space.World);
            }

            if (IsFullyCleaned && autoRespawn)
            {
                respawnTimer += Time.deltaTime;
                if (respawnTimer >= respawnCooldownSeconds)
                {
                    Respawn();
                }
            }
        }

        public float CleanDust(float amountKg)
        {
            if (IsFullyCleaned || amountKg <= 0f) return 0f;

            float cleaned = Mathf.Min(amountKg, currentDustMassKg);
            currentDustMassKg -= cleaned;
            GameManager.Instance?.RecordDustCleaned(cleaned);

            float ratio = Mathf.Clamp01(currentDustMassKg / maxDustMassKg);
            if (visualClusterRoot != null)
            {
                visualClusterRoot.localScale = new Vector3(
                    baseClusterScale.x * (0.28f + 0.72f * ratio),
                    baseClusterScale.y * Mathf.Max(0.08f, ratio),
                    baseClusterScale.z * (0.28f + 0.72f * ratio)
                );
            }

            OnDustCleaned?.Invoke(cleaned);

            if (IsFullyCleaned)
            {
                OnFullyCleaned();
            }

            return cleaned;
        }

        private void OnFullyCleaned()
        {
            if (mineralTier > 0 || (canDropMinerals && UnityEngine.Random.value < 0.12f))
            {
                AwardRareMineral();
            }

            SetClusterVisible(false);
            if (hitCollider != null) hitCollider.enabled = false;
            respawnTimer = 0f;
            GameManager.Instance?.ReportPatchCleanedState(true);
        }

        private void AwardRareMineral()
        {
            PlayerInventory inventory = FindFirstObjectByType<PlayerInventory>();
            if (inventory == null) return;

            int tier = mineralTier > 0 ? mineralTier : 1;
            if (tier == 3)
            {
                inventory.AddMineral("Apollo Titanium Relic", 1, 140f);
                GameManager.Instance?.NotifyToast("Rare Discovery: Apollo Titanium Relic (+140 LC value)!");
            }
            else if (tier == 2)
            {
                inventory.AddMineral("Helium-3 Isotope Cell", 1, 55f);
                GameManager.Instance?.NotifyToast("Rare Mineral: Helium-3 Isotope Cell (+55 LC value)!");
            }
            else
            {
                inventory.AddMineral("Titanium Ore Chunk", 1, 22f);
                GameManager.Instance?.NotifyToast("Extracted: Titanium Ore Chunk (+22 LC value)");
            }

            AudioManager.Instance?.PlayMineralPickup();
        }

        public void Respawn()
        {
            bool wasCleaned = IsFullyCleaned;
            currentDustMassKg = maxDustMassKg;
            if (visualClusterRoot != null)
            {
                visualClusterRoot.localScale = baseClusterScale;
            }
            SetClusterVisible(true);
            if (hitCollider != null) hitCollider.enabled = true;
            respawnTimer = 0f;

            if (wasCleaned)
            {
                GameManager.Instance?.ReportPatchCleanedState(false);
            }
        }

        private void SetClusterVisible(bool visible)
        {
            for (int i = 0; i < clusterRenderers.Count; i++)
            {
                if (clusterRenderers[i] != null)
                {
                    clusterRenderers[i].enabled = visible;
                }
            }
        }
    }
}
