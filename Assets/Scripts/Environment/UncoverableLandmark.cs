/*
 * Created by 11Hopem
 * Copyright (c) 2026 11Hopem. All rights reserved.
 */

using System.Collections.Generic;
using UnityEngine;
using ElevenHopem.MoonSweepers.Core;

namespace ElevenHopem.MoonSweepers.Environment
{
    public enum LandmarkKind
    {
        CrashedSatellite,
        AncientMonolith
    }

    public class UncoverableLandmark : MonoBehaviour
    {
        public static readonly List<UncoverableLandmark> ActiveLandmarks = new List<UncoverableLandmark>();

        public LandmarkKind kind;
        private readonly List<RegolithDustPatch> coveringPatches = new List<RegolithDustPatch>();
        private Transform animatedPartA;
        private Transform animatedPartB;
        private GameObject awakenedBeam;
        private bool isUncovered = false;

        public bool IsUncovered => isUncovered;

        private void OnEnable()
        {
            if (!ActiveLandmarks.Contains(this))
            {
                ActiveLandmarks.Add(this);
            }
        }

        private void OnDisable()
        {
            ActiveLandmarks.Remove(this);
        }

        public void InitializeLandmark(LandmarkKind landmarkKind, int seedBase)
        {
            kind = landmarkKind;
            if (kind == LandmarkKind.CrashedSatellite)
            {
                BuildCrashedSatelliteMesh();
            }
            else
            {
                BuildAncientMonolithMesh();
            }

            int coverCount = 6;
            for (int i = 0; i < coverCount; i++)
            {
                float ang = (i / (float)coverCount) * Mathf.PI * 2f + 0.3f;
                float dist = 2.4f + (i % 2) * 0.9f;
                float wx = transform.position.x + Mathf.Cos(ang) * dist;
                float wz = transform.position.z + Mathf.Sin(ang) * dist;
                float wy = LunarSurfaceGenerator.SampleHeight(wx, wz);
                Vector3 norm = LunarSurfaceGenerator.SampleNormal(wx, wz);

                GameObject patchObj = new GameObject($"LandmarkCoverDust_{kind}_{i}");
                patchObj.transform.SetParent(transform.parent, true);
                patchObj.transform.position = new Vector3(wx, wy, wz);
                patchObj.transform.rotation = Quaternion.FromToRotation(Vector3.up, norm);

                RegolithDustPatch patch = patchObj.AddComponent<RegolithDustPatch>();
                patch.ConfigureVisuals(seedBase + i * 13, 2.25f, 24f, false);
                patch.OnPatchFullyCleaned += CheckUncoveredState;
                coveringPatches.Add(patch);
            }
        }

        private void BuildCrashedSatelliteMesh()
        {
            Material goldFoil = LunarMaterialLibrary.GetSurfaceMaterial("SatGoldFoil", new Color(0.96f, 0.74f, 0.16f), 0.90f, 0.72f, true);
            Material solarMat = LunarMaterialLibrary.GetEmissiveMaterial("SatSolarPanel", new Color(0.08f, 0.25f, 0.58f), new Color(0.06f, 0.20f, 0.48f));
            Material darkMetal = LunarMaterialLibrary.GetSurfaceMaterial("SatDarkFrame", new Color(0.22f, 0.25f, 0.30f), 0.82f, 0.60f);
            Material cyanBeamMat = LunarMaterialLibrary.GetTransparentMaterial("SatLaserBeam", new Color(0.20f, 0.95f, 1.0f, 0.32f), new Color(0.25f, 1.6f, 2.2f));
            Material beaconRed = LunarMaterialLibrary.GetEmissiveMaterial("SatDistressBeacon", new Color(1f, 0.3f, 0.2f), new Color(2.0f, 0.35f, 0.2f));

            GameObject chassis = new GameObject("SatelliteChassis");
            chassis.transform.SetParent(transform, false);
            chassis.transform.localPosition = new Vector3(0f, 0.9f, 0f);
            chassis.transform.localRotation = Quaternion.Euler(24f, -30f, 18f);

            AddPart(PrimitiveType.Cylinder, "GoldBusCore", chassis.transform, Vector3.zero, Quaternion.identity, new Vector3(1.8f, 1.35f, 1.8f), goldFoil, true);
            AddPart(PrimitiveType.Cube, "WingLeft", chassis.transform, new Vector3(-2.8f, 0.2f, 0f), Quaternion.Euler(14f, 0f, -8f), new Vector3(3.6f, 0.08f, 1.35f), solarMat, true);
            AddPart(PrimitiveType.Cube, "WingRight", chassis.transform, new Vector3(2.6f, -0.1f, 0f), Quaternion.Euler(-18f, 5f, 12f), new Vector3(3.2f, 0.08f, 1.35f), solarMat, true);

            GameObject dishPivot = new GameObject("TransponderPivot");
            dishPivot.transform.SetParent(chassis.transform, false);
            dishPivot.transform.localPosition = new Vector3(0f, 1.55f, 0f);
            animatedPartA = dishPivot.transform;

            AddPart(PrimitiveType.Sphere, "HighGainDish", dishPivot.transform, Vector3.zero, Quaternion.Euler(-20f, 0f, 0f), new Vector3(2.1f, 0.35f, 2.1f), darkMetal, false);
            AddPart(PrimitiveType.Sphere, "FeedBeacon", dishPivot.transform, new Vector3(0f, 0.55f, 0f), Quaternion.identity, new Vector3(0.32f, 0.32f, 0.32f), beaconRed, false);

            GameObject markerPole = AddPart(PrimitiveType.Cylinder, "SiteBeaconBeam", transform, new Vector3(0f, 18f, 0f), Quaternion.identity, new Vector3(0.45f, 18f, 0.45f), cyanBeamMat, false);
            markerPole.SetActive(false);
            awakenedBeam = markerPole;
        }

        private void BuildAncientMonolithMesh()
        {
            Material obsidian = LunarMaterialLibrary.GetSurfaceMaterial("MonolithObsidian", new Color(0.07f, 0.08f, 0.11f), 0.92f, 0.92f);
            Material goldRune = LunarMaterialLibrary.GetEmissiveMaterial("MonolithGold", new Color(1.0f, 0.78f, 0.22f), new Color(1.8f, 1.25f, 0.25f));
            Material violetBeam = LunarMaterialLibrary.GetTransparentMaterial("MonolithColumn", new Color(0.75f, 0.45f, 1.0f, 0.28f), new Color(1.1f, 0.55f, 1.9f));

            AddPart(PrimitiveType.Cylinder, "Pedestal", transform, new Vector3(0f, 0.2f, 0f), Quaternion.identity, new Vector3(4.2f, 0.25f, 4.2f), obsidian, true);
            AddPart(PrimitiveType.Cube, "ObsidianSlab", transform, new Vector3(0f, 2.6f, 0f), Quaternion.Euler(0f, 15f, 0f), new Vector3(1.45f, 4.8f, 0.42f), obsidian, true);
            AddPart(PrimitiveType.Cube, "GoldSpine", transform, new Vector3(0f, 2.6f, 0f), Quaternion.Euler(0f, 15f, 0f), new Vector3(0.18f, 4.5f, 0.46f), goldRune, false);

            GameObject ringA = new GameObject("HarmonicRingA");
            ringA.transform.SetParent(transform, false);
            ringA.transform.localPosition = new Vector3(0f, 5.8f, 0f);
            animatedPartA = ringA.transform;
            AddPart(PrimitiveType.Cylinder, "GoldHaloA", ringA.transform, Vector3.zero, Quaternion.Euler(35f, 0f, 0f), new Vector3(2.2f, 0.06f, 2.2f), goldRune, false);

            GameObject ringB = new GameObject("HarmonicRingB");
            ringB.transform.SetParent(transform, false);
            ringB.transform.localPosition = new Vector3(0f, 5.8f, 0f);
            animatedPartB = ringB.transform;
            AddPart(PrimitiveType.Cylinder, "GoldHaloB", ringB.transform, Vector3.zero, Quaternion.Euler(-35f, 45f, 0f), new Vector3(1.6f, 0.06f, 1.6f), goldRune, false);

            GameObject beam = AddPart(PrimitiveType.Cylinder, "MonolithSkyBeam", transform, new Vector3(0f, 22f, 0f), Quaternion.identity, new Vector3(0.6f, 22f, 0.6f), violetBeam, false);
            beam.SetActive(false);
            awakenedBeam = beam;
        }

        private void CheckUncoveredState()
        {
            if (isUncovered) return;

            for (int i = 0; i < coveringPatches.Count; i++)
            {
                if (coveringPatches[i] != null && !coveringPatches[i].IsFullyCleaned)
                {
                    return;
                }
            }

            isUncovered = true;
            if (awakenedBeam != null)
            {
                awakenedBeam.SetActive(true);
            }

            if (kind == LandmarkKind.CrashedSatellite)
            {
                GameManager.Instance?.GrantSatelliteTelemetryBonus();
            }
            else
            {
                GameManager.Instance?.GrantMonolithOverclockBonus();
            }
        }

        private void Update()
        {
            float speed = isUncovered ? 95f : 12f;
            if (animatedPartA != null)
            {
                animatedPartA.Rotate(0f, speed * Time.deltaTime, 0f, Space.Self);
            }
            if (animatedPartB != null)
            {
                animatedPartB.Rotate(speed * 0.8f * Time.deltaTime, -speed * Time.deltaTime, 0f, Space.Self);
            }
        }

        private static GameObject AddPart(PrimitiveType type, string name, Transform parent, Vector3 pos, Quaternion rot, Vector3 scale, Material mat, bool keepCollider)
        {
            GameObject go = GameObject.CreatePrimitive(type);
            go.name = name;
            if (!keepCollider)
            {
                Collider c = go.GetComponent<Collider>();
                if (c != null) Destroy(c);
            }
            go.transform.SetParent(parent, false);
            go.transform.localPosition = pos;
            go.transform.localRotation = rot;
            go.transform.localScale = scale;
            Renderer r = go.GetComponent<Renderer>();
            if (r != null)
            {
                r.sharedMaterial = mat;
                r.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.On;
            }
            return go;
        }
    }
}
