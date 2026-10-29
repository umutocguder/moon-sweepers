/*
 * Created by 11Hopem
 * Copyright (c) 2026 11Hopem. All rights reserved.
 */

using UnityEngine;
using ElevenHopem.MoonSweepers.Core;
using ElevenHopem.MoonSweepers.Environment;

namespace ElevenHopem.MoonSweepers.Economy
{
    public class AutomationStation : MonoBehaviour
    {
        public string stationType = "conveyor";
        public int unitIndex = 0;

        [SerializeField] private float cleanRadius = 22.0f;
        [SerializeField] private float harvestRateKgPerSec = 6.5f;
        [SerializeField] private float patrolSpeed = 1.6f;

        private float nextTick = 0f;
        private Vector3 anchorPosition;
        private readonly Transform[] rotors = new Transform[4];
        private Transform droneTransform;

        private void Start()
        {
            anchorPosition = transform.position;
            CreateVisualRepresentation();
        }

        private void CreateVisualRepresentation()
        {
            Material darkMetal = LunarMaterialLibrary.GetSurfaceMaterial("AutoDarkMetal", new Color(0.20f, 0.23f, 0.28f), 0.85f, 0.65f);
            Material cyanGlow = LunarMaterialLibrary.GetEmissiveMaterial("AutoCyan", new Color(0.18f, 0.92f, 1.0f), new Color(0.25f, 1.6f, 2.1f));
            Material hazardMat = LunarMaterialLibrary.GetHazardMaterial();

            if (stationType == "drone")
            {
                GameObject droneRoot = new GameObject("QuadDroneChassis");
                droneRoot.transform.SetParent(transform, false);
                droneTransform = droneRoot.transform;

                AddPart(PrimitiveType.Sphere, "CentralPod", droneTransform, Vector3.zero, new Vector3(1.1f, 0.42f, 1.1f), darkMetal);
                AddPart(PrimitiveType.Sphere, "SensorEye", droneTransform, new Vector3(0f, -0.12f, 0.42f), new Vector3(0.28f, 0.22f, 0.28f), cyanGlow);

                for (int i = 0; i < 4; i++)
                {
                    float ang = (i * 90f + 45f) * Mathf.Deg2Rad;
                    Vector3 armDir = new Vector3(Mathf.Cos(ang), 0f, Mathf.Sin(ang));
                    GameObject arm = AddPart(PrimitiveType.Cube, $"DroneArm_{i}", droneTransform, armDir * 0.62f, new Vector3(0.10f, 0.06f, 0.85f), darkMetal);
                    arm.transform.localRotation = Quaternion.LookRotation(armDir, Vector3.up);

                    GameObject rotorPivot = new GameObject($"RotorPivot_{i}");
                    rotorPivot.transform.SetParent(droneTransform, false);
                    rotorPivot.transform.localPosition = armDir * 1.02f + Vector3.up * 0.08f;
                    rotors[i] = rotorPivot.transform;

                    AddPart(PrimitiveType.Cube, "Blade", rotorPivot.transform, Vector3.zero, new Vector3(0.62f, 0.015f, 0.07f), cyanGlow);
                }

                GameObject lightObj = new GameObject("DroneSearchlight");
                lightObj.transform.SetParent(droneTransform, false);
                lightObj.transform.localPosition = new Vector3(0f, -0.2f, 0f);
                lightObj.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
                Light spot = lightObj.AddComponent<Light>();
                spot.type = LightType.Spot;
                spot.range = 14f;
                spot.spotAngle = 48f;
                spot.intensity = 2.2f;
                spot.color = new Color(0.2f, 0.95f, 1.0f);
            }
            else
            {
                AddPart(PrimitiveType.Cube, "ConveyorFrame", transform, new Vector3(0f, 0.24f, 0f), new Vector3(1.55f, 0.24f, 7.6f), hazardMat);
                AddPart(PrimitiveType.Cube, "BeltSurface", transform, new Vector3(0f, 0.38f, 0f), new Vector3(1.18f, 0.06f, 7.4f), darkMetal);

                for (int r = -3; r <= 3; r++)
                {
                    GameObject roller = AddPart(PrimitiveType.Cylinder, $"ConveyorRoller_{r}", transform, new Vector3(0f, 0.41f, r * 1.05f), new Vector3(0.14f, 0.58f, 0.14f), cyanGlow);
                    roller.transform.localRotation = Quaternion.Euler(0f, 0f, 90f);
                }
            }
        }

        private static GameObject AddPart(PrimitiveType type, string name, Transform parent, Vector3 pos, Vector3 scale, Material mat)
        {
            GameObject go = GameObject.CreatePrimitive(type);
            go.name = name;
            Collider c = go.GetComponent<Collider>();
            if (c != null) Destroy(c);
            go.transform.SetParent(parent, false);
            go.transform.localPosition = pos;
            go.transform.localScale = scale;
            Renderer r = go.GetComponent<Renderer>();
            if (r != null)
            {
                r.sharedMaterial = mat;
                r.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.On;
            }
            return go;
        }

        private void Update()
        {
            if (stationType == "drone" && droneTransform != null)
            {
                float orbitRadius = 15f + unitIndex * 7.5f;
                float angle = Time.time * (patrolSpeed * 0.25f) + unitIndex * 1.7f;
                float wx = anchorPosition.x + Mathf.Cos(angle) * orbitRadius;
                float wz = anchorPosition.z + Mathf.Sin(angle) * orbitRadius;
                float groundY = LunarSurfaceGenerator.SampleHeight(wx, wz);

                Vector3 nextPos = new Vector3(wx, groundY + 3.4f + Mathf.Sin(Time.time * 2.5f) * 0.25f, wz);
                Vector3 tangent = new Vector3(-Mathf.Sin(angle), 0f, Mathf.Cos(angle));
                transform.position = nextPos;
                if (tangent.sqrMagnitude > 0.001f)
                {
                    droneTransform.rotation = Quaternion.LookRotation(tangent, Vector3.up) * Quaternion.Euler(10f, 0f, 0f);
                }

                for (int i = 0; i < rotors.Length; i++)
                {
                    if (rotors[i] != null)
                    {
                        rotors[i].Rotate(0f, 1200f * Time.deltaTime, 0f, Space.Self);
                    }
                }
            }

            if (Time.time >= nextTick)
            {
                nextTick = Time.time + 1.0f;
                HarvestNearbyDust();
            }
        }

        private void HarvestNearbyDust()
        {
            float searchRadius = stationType == "drone" ? cleanRadius : 34f;
            Collider[] hits = Physics.OverlapSphere(transform.position, searchRadius);
            for (int i = 0; i < hits.Length; i++)
            {
                RegolithDustPatch patch = hits[i].GetComponent<RegolithDustPatch>();
                if (patch != null && !patch.IsFullyCleaned)
                {
                    float harvested = patch.CleanDust(harvestRateKgPerSec);
                    if (harvested > 0f)
                    {
                        float rate = GameManager.Instance != null ? GameManager.Instance.BaseDustValuePerKg : 3.5f;
                        long earned = (long)Mathf.Max(1f, Mathf.Round(harvested * rate));
                        GameManager.Instance?.AddCredits(earned);
                        break;
                    }
                }
            }
        }
    }
}
