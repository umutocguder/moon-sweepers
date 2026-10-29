/*
 * Created by 11Hopem
 * Copyright (c) 2026 11Hopem. All rights reserved.
 */

using System.Collections.Generic;
using UnityEngine;
using ElevenHopem.MoonSweepers.Core;

namespace ElevenHopem.MoonSweepers.Environment
{
    public class LunarSurfaceGenerator : MonoBehaviour
    {
        private struct CraterSpec
        {
            public Vector2 center;
            public float radius;
            public float depth;

            public CraterSpec(float x, float z, float r, float d)
            {
                center = new Vector2(x, z);
                radius = r;
                depth = d;
            }
        }

        [SerializeField] private int dustPatchCount = 92;
        [SerializeField] private float spawnRadiusMin = 6.5f;
        [SerializeField] private float spawnRadiusMax = 78.0f;
        [SerializeField] private Material dustPatchMaterial;
        [SerializeField] private Material craterRockMaterial;
        [SerializeField] private Transform landingBaseCenter;

        private static CraterSpec[] craters;
        private Transform radarDishPivot;
        private Transform planetEarthTransform;
        private bool generated = false;

        private static void EnsureCraterField()
        {
            if (craters != null) return;

            System.Random rng = new System.Random(11092026);
            List<CraterSpec> list = new List<CraterSpec>
            {
                new CraterSpec(-18f, 24f, 11.5f, 2.3f),
                new CraterSpec(22f, 20f, 9.5f, 1.9f),
                new CraterSpec(16f, -14f, 8.0f, 1.5f),
                new CraterSpec(-22f, -16f, 10.5f, 2.1f),
                new CraterSpec(0f, 36f, 14.0f, 2.8f)
            };

            for (int i = 0; i < 24; i++)
            {
                float ang = (float)(rng.NextDouble() * Mathf.PI * 2.0);
                float dist = 24f + (float)rng.NextDouble() * 68f;
                float r = 5.5f + (float)rng.NextDouble() * 14.5f;
                float d = r * (0.16f + (float)rng.NextDouble() * 0.06f);
                list.Add(new CraterSpec(Mathf.Cos(ang) * dist, Mathf.Sin(ang) * dist, r, d));
            }

            craters = list.ToArray();
        }

        private static float SmoothStepRange(float edge0, float edge1, float x)
        {
            return Mathf.SmoothStep(0f, 1f, Mathf.InverseLerp(edge0, edge1, x));
        }

        public static float SampleHeight(float x, float z)
        {
            EnsureCraterField();

            float distFromOrigin = Mathf.Sqrt(x * x + z * z);
            float baseFlatBlend = SmoothStepRange(13.5f, 23.0f, distFromOrigin);

            if (baseFlatBlend <= 0.0001f)
            {
                return 0f;
            }

            float hills = (LunarMaterialLibrary.Fbm(x * 0.016f + 11.3f, z * 0.016f + 7.9f, 4) - 0.48f) * 5.2f;
            float ripples = (Mathf.PerlinNoise(x * 0.085f + 3.1f, z * 0.085f + 5.7f) - 0.5f) * 0.42f;
            float horizonRim = SmoothStepRange(72f, 114f, distFromOrigin) * (8.5f + Mathf.PerlinNoise(x * 0.04f + 2f, z * 0.04f + 2f) * 7.5f);

            float craterDelta = 0f;
            for (int i = 0; i < craters.Length; i++)
            {
                CraterSpec c = craters[i];
                float dx = x - c.center.x;
                float dz = z - c.center.y;
                float d = Mathf.Sqrt(dx * dx + dz * dz);
                float norm = d / c.radius;

                if (norm < 1.0f)
                {
                    float bowl = (norm * norm - 1.0f) * c.depth;
                    float innerRim = Mathf.Pow(norm, 4f) * c.depth * 0.34f;
                    craterDelta += bowl + innerRim;
                }
                else if (norm < 1.65f)
                {
                    float ejectaT = 1f - (norm - 1.0f) / 0.65f;
                    craterDelta += ejectaT * ejectaT * c.depth * 0.34f;
                }
            }

            return (hills + ripples + craterDelta + horizonRim) * baseFlatBlend;
        }

        public static Vector3 SampleNormal(float x, float z)
        {
            float eps = 0.5f;
            float hL = SampleHeight(x - eps, z);
            float hR = SampleHeight(x + eps, z);
            float hD = SampleHeight(x, z - eps);
            float hU = SampleHeight(x, z + eps);
            return new Vector3(hL - hR, 2f * eps, hD - hU).normalized;
        }

        private void Start()
        {
            GenerateSurfaceFeatures();
        }

        private void Update()
        {
            if (radarDishPivot != null)
            {
                radarDishPivot.Rotate(0f, 18f * Time.deltaTime, 0f, Space.Self);
            }
            if (planetEarthTransform != null)
            {
                planetEarthTransform.Rotate(0f, 1.4f * Time.deltaTime, 0f, Space.Self);
            }
        }

        public void GenerateSurfaceFeatures()
        {
            if (generated) return;
            generated = true;

            BuildSculptedLunarTerrain();
            BuildCelestialSkyAndLighting();
            BuildLunarBaseArchitecture();
            BuildLandingPerimeterBeacons();
            SpawnRegolithPatchesAndBoulders();
        }

        private void BuildSculptedLunarTerrain()
        {
            GameObject oldSurface = GameObject.Find("LunarSurface");
            if (oldSurface != null)
            {
                oldSurface.SetActive(false);
            }

            GameObject terrainObj = new GameObject("SculptedLunarTerrain");
            terrainObj.transform.SetParent(transform, false);
            terrainObj.transform.position = Vector3.zero;

            int grid = 160;
            float worldSize = 230f;
            float half = worldSize * 0.5f;
            int vertSide = grid + 1;

            Vector3[] verts = new Vector3[vertSide * vertSide];
            Vector2[] uvs = new Vector2[vertSide * vertSide];
            Color[] colors = new Color[vertSide * vertSide];
            int[] tris = new int[grid * grid * 6];

            for (int z = 0; z < vertSide; z++)
            {
                float wz = -half + (z / (float)grid) * worldSize;
                for (int x = 0; x < vertSide; x++)
                {
                    float wx = -half + (x / (float)grid) * worldSize;
                    float wy = SampleHeight(wx, wz);
                    int idx = z * vertSide + x;

                    verts[idx] = new Vector3(wx, wy, wz);
                    uvs[idx] = new Vector2(x / (float)grid, z / (float)grid);

                    float shade = Mathf.Clamp(0.84f + wy * 0.045f, 0.58f, 1.08f);
                    colors[idx] = new Color(shade, shade, shade * 1.02f, 1f);
                }
            }

            int t = 0;
            for (int z = 0; z < grid; z++)
            {
                for (int x = 0; x < grid; x++)
                {
                    int i0 = z * vertSide + x;
                    int i1 = i0 + 1;
                    int i2 = i0 + vertSide;
                    int i3 = i2 + 1;

                    tris[t++] = i0;
                    tris[t++] = i2;
                    tris[t++] = i1;

                    tris[t++] = i1;
                    tris[t++] = i2;
                    tris[t++] = i3;
                }
            }

            Mesh mesh = new Mesh();
            mesh.name = "ProceduralLunarBasin";
            mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
            mesh.vertices = verts;
            mesh.uv = uvs;
            mesh.colors = colors;
            mesh.triangles = tris;
            mesh.RecalculateNormals();
            mesh.RecalculateTangents();
            mesh.RecalculateBounds();

            MeshFilter mf = terrainObj.AddComponent<MeshFilter>();
            mf.sharedMesh = mesh;

            MeshRenderer mr = terrainObj.AddComponent<MeshRenderer>();
            mr.sharedMaterial = craterRockMaterial != null ? craterRockMaterial : LunarMaterialLibrary.GetRegolithTerrainMaterial();
            mr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.On;
            mr.receiveShadows = true;

            MeshCollider mc = terrainObj.AddComponent<MeshCollider>();
            mc.sharedMesh = mesh;
        }

        private void BuildCelestialSkyAndLighting()
        {
            RenderSettings.skybox = null;
            RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Trilight;
            RenderSettings.ambientSkyColor = new Color(0.22f, 0.26f, 0.36f);
            RenderSettings.ambientEquatorColor = new Color(0.16f, 0.18f, 0.24f);
            RenderSettings.ambientGroundColor = new Color(0.09f, 0.09f, 0.12f);
            RenderSettings.fog = false;

            Camera mainCam = Camera.main;
            if (mainCam != null)
            {
                mainCam.clearFlags = CameraClearFlags.SolidColor;
                mainCam.backgroundColor = new Color(0.005f, 0.008f, 0.016f, 1f);
                mainCam.allowMSAA = true;
            }

            GameObject sunObj = GameObject.Find("Directional Light (Sun)") ?? GameObject.Find("Directional Light");
            if (sunObj != null)
            {
                sunObj.transform.rotation = Quaternion.Euler(28f, -36f, 0f);
                Light sun = sunObj.GetComponent<Light>();
                if (sun != null)
                {
                    sun.color = new Color(1.0f, 0.98f, 0.94f);
                    sun.intensity = 1.95f;
                    sun.shadows = LightShadows.Soft;
                    sun.shadowStrength = 0.82f;
                    sun.shadowBias = 0.03f;
                    sun.shadowNormalBias = 0.35f;
                }
            }

            GameObject earthShineObj = new GameObject("EarthshineFillLight");
            earthShineObj.transform.SetParent(transform, false);
            earthShineObj.transform.rotation = Quaternion.Euler(40f, 145f, 0f);
            Light fill = earthShineObj.AddComponent<Light>();
            fill.type = LightType.Directional;
            fill.color = new Color(0.45f, 0.65f, 0.95f);
            fill.intensity = 0.35f;
            fill.shadows = LightShadows.None;

            GameObject skyRoot = new GameObject("CelestialDome");
            skyRoot.transform.SetParent(transform, false);

            GameObject starSystemObj = new GameObject("StarfieldParticles");
            starSystemObj.transform.SetParent(skyRoot.transform, false);
            ParticleSystem ps = starSystemObj.AddComponent<ParticleSystem>();
            var main = ps.main;
            main.loop = false;
            main.playOnAwake = false;
            main.maxParticles = 1600;
            main.simulationSpace = ParticleSystemSimulationSpace.Local;
            main.startLifetime = float.MaxValue;
            var emission = ps.emission;
            emission.enabled = false;
            var shape = ps.shape;
            shape.enabled = false;

            ParticleSystemRenderer psr = starSystemObj.GetComponent<ParticleSystemRenderer>();
            psr.sharedMaterial = LunarMaterialLibrary.GetParticleMaterial();
            psr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;

            ParticleSystem.Particle[] stars = new ParticleSystem.Particle[1500];
            System.Random starRng = new System.Random(4202026);
            for (int i = 0; i < stars.Length; i++)
            {
                float u = (float)starRng.NextDouble();
                float v = (float)starRng.NextDouble();
                float theta = u * Mathf.PI * 2f;
                float phi = Mathf.Acos(Mathf.Lerp(0.04f, 1.0f, v));
                float r = 420f;

                Vector3 dir = new Vector3(
                    Mathf.Sin(phi) * Mathf.Cos(theta),
                    Mathf.Cos(phi),
                    Mathf.Sin(phi) * Mathf.Sin(theta)
                );

                float bright = Mathf.Pow((float)starRng.NextDouble(), 2.2f);
                Color tint = (i % 5 == 0)
                    ? new Color(0.75f, 0.88f, 1f, 0.65f + bright * 0.35f)
                    : (i % 7 == 0 ? new Color(1f, 0.88f, 0.72f, 0.6f + bright * 0.4f) : new Color(0.95f, 0.97f, 1f, 0.55f + bright * 0.45f));

                stars[i].position = dir * r;
                stars[i].startColor = tint;
                stars[i].startSize = Mathf.Lerp(0.8f, 3.2f, bright);
                stars[i].remainingLifetime = 999999f;
                stars[i].startLifetime = 999999f;
            }
            ps.SetParticles(stars, stars.Length);

            GameObject earthObj = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            earthObj.name = "PlanetEarth";
            Destroy(earthObj.GetComponent<Collider>());
            earthObj.transform.SetParent(skyRoot.transform, false);
            earthObj.transform.position = new Vector3(-28f, 38f, 168f);
            earthObj.transform.localScale = Vector3.one * 36f;
            earthObj.transform.rotation = Quaternion.Euler(18f, 125f, -23.5f);
            Renderer earthRend = earthObj.GetComponent<Renderer>();
            earthRend.sharedMaterial = LunarMaterialLibrary.GetEarthMaterial();
            earthRend.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            earthRend.receiveShadows = false;
            planetEarthTransform = earthObj.transform;

            GameObject atmoObj = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            atmoObj.name = "EarthAtmosphereRim";
            Destroy(atmoObj.GetComponent<Collider>());
            atmoObj.transform.SetParent(earthObj.transform, false);
            atmoObj.transform.localPosition = Vector3.zero;
            atmoObj.transform.localScale = Vector3.one * 1.032f;
            Renderer atmoRend = atmoObj.GetComponent<Renderer>();
            atmoRend.sharedMaterial = LunarMaterialLibrary.GetTransparentMaterial(
                "EarthAtmoRim",
                new Color(0.22f, 0.68f, 1.0f, 0.08f),
                new Color(0.08f, 0.32f, 0.75f)
            );
            atmoRend.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;

            Vector3 sunDir = sunObj != null ? -sunObj.transform.forward : new Vector3(0.5f, 0.44f, -0.74f).normalized;
            GameObject sunDisc = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            sunDisc.name = "SolarDisc";
            Destroy(sunDisc.GetComponent<Collider>());
            sunDisc.transform.SetParent(skyRoot.transform, false);
            sunDisc.transform.position = sunDir * 390f;
            sunDisc.transform.localScale = Vector3.one * 18f;
            Renderer sunRend = sunDisc.GetComponent<Renderer>();
            sunRend.sharedMaterial = LunarMaterialLibrary.GetEmissiveMaterial("SunDiscMat", Color.white, new Color(3.0f, 2.8f, 2.3f));
            sunRend.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        }

        private void BuildLunarBaseArchitecture()
        {
            GameObject baseRoot = new GameObject("LunarOutpostAlpha");
            baseRoot.transform.SetParent(transform, false);
            baseRoot.transform.position = new Vector3(-11.5f, 0f, 13.0f);
            baseRoot.transform.rotation = Quaternion.Euler(0f, 22f, 0f);

            Material hullWhite = LunarMaterialLibrary.GetSurfaceMaterial("HabitatWhite", new Color(0.90f, 0.92f, 0.96f), 0.22f, 0.62f, true);
            Material darkTi = LunarMaterialLibrary.GetSurfaceMaterial("DarkTitanium", new Color(0.22f, 0.25f, 0.30f), 0.78f, 0.58f, true);
            Material solarBlue = LunarMaterialLibrary.GetEmissiveMaterial("SolarPanelBlue", new Color(0.08f, 0.24f, 0.56f), new Color(0.05f, 0.18f, 0.45f));
            Material windowAmber = LunarMaterialLibrary.GetEmissiveMaterial("HabWindowAmber", new Color(1f, 0.78f, 0.35f), new Color(1.6f, 1.1f, 0.35f));
            Material cyanGlow = LunarMaterialLibrary.GetEmissiveMaterial("BaseCyanStrip", new Color(0.15f, 0.85f, 1f), new Color(0.2f, 1.3f, 1.6f));

            CreateArchPart(PrimitiveType.Cylinder, "FoundationPlinth", baseRoot.transform, new Vector3(0f, 0.25f, 0f), new Vector3(9.4f, 0.25f, 9.4f), darkTi);
            CreateArchPart(PrimitiveType.Sphere, "MainHabitatDome", baseRoot.transform, new Vector3(0f, 0.85f, 0f), new Vector3(8.4f, 7.2f, 8.4f), hullWhite);
            CreateArchPart(PrimitiveType.Cylinder, "DomeCollarRing", baseRoot.transform, new Vector3(0f, 0.65f, 0f), new Vector3(8.5f, 0.22f, 8.5f), darkTi);
            CreateArchPart(PrimitiveType.Cylinder, "DomeGlowTrim", baseRoot.transform, new Vector3(0f, 0.90f, 0f), new Vector3(8.35f, 0.05f, 8.35f), cyanGlow);

            CreateArchPart(PrimitiveType.Cube, "AirlockCorridor", baseRoot.transform, new Vector3(0f, 1.35f, -4.8f), new Vector3(2.5f, 2.2f, 3.2f), hullWhite);
            CreateArchPart(PrimitiveType.Cube, "AirlockFrame", baseRoot.transform, new Vector3(0f, 1.35f, -6.45f), new Vector3(2.7f, 2.4f, 0.25f), darkTi);
            CreateArchPart(PrimitiveType.Cube, "AirlockViewport", baseRoot.transform, new Vector3(0f, 1.55f, -6.58f), new Vector3(1.2f, 0.9f, 0.08f), windowAmber);

            for (int i = -1; i <= 1; i += 2)
            {
                GameObject win = CreateArchPart(PrimitiveType.Cube, $"DomeWindow_{i}", baseRoot.transform, new Vector3(i * 2.9f, 2.1f, -2.5f), new Vector3(1.25f, 0.58f, 0.2f), windowAmber);
                win.transform.localRotation = Quaternion.Euler(-14f, i * -40f, 0f);
            }

            CreateArchPart(PrimitiveType.Cylinder, "CommsMast", baseRoot.transform, new Vector3(0f, 4.1f, 0f), new Vector3(0.28f, 1.2f, 0.28f), darkTi);
            GameObject dishPivot = new GameObject("RadarDishPivot");
            dishPivot.transform.SetParent(baseRoot.transform, false);
            dishPivot.transform.localPosition = new Vector3(0f, 5.3f, 0f);
            radarDishPivot = dishPivot.transform;

            GameObject dish = CreateArchPart(PrimitiveType.Sphere, "RadarDish", dishPivot.transform, Vector3.zero, new Vector3(2.2f, 0.35f, 2.2f), hullWhite);
            dish.transform.localRotation = Quaternion.Euler(35f, 0f, 0f);
            CreateArchPart(PrimitiveType.Sphere, "RadarFeedHorn", dishPivot.transform, new Vector3(0f, 0.55f, -0.35f), new Vector3(0.24f, 0.24f, 0.24f), cyanGlow);

            for (int side = -1; side <= 1; side += 2)
            {
                CreateArchPart(PrimitiveType.Cylinder, $"SolarMast_{side}", baseRoot.transform, new Vector3(side * 6.8f, 1.4f, 1.2f), new Vector3(0.22f, 1.4f, 0.22f), darkTi);
                GameObject wing = CreateArchPart(PrimitiveType.Cube, $"SolarWing_{side}", baseRoot.transform, new Vector3(side * 6.8f, 2.65f, 1.2f), new Vector3(3.8f, 0.08f, 2.2f), solarBlue);
                wing.transform.localRotation = Quaternion.Euler(24f, 0f, 0f);
            }

            GameObject baseLightObj = new GameObject("HabitatWarmLight");
            baseLightObj.transform.SetParent(baseRoot.transform, false);
            baseLightObj.transform.localPosition = new Vector3(0f, 3.2f, -7.8f);
            Light warmLight = baseLightObj.AddComponent<Light>();
            warmLight.type = LightType.Point;
            warmLight.color = new Color(1.0f, 0.82f, 0.55f);
            warmLight.range = 16f;
            warmLight.intensity = 1.6f;
        }

        private void BuildLandingPerimeterBeacons()
        {
            GameObject ringRoot = new GameObject("BasePerimeterMarkers");
            ringRoot.transform.SetParent(transform, false);

            Material postMat = LunarMaterialLibrary.GetSurfaceMaterial("BeaconPost", new Color(0.3f, 0.34f, 0.4f), 0.7f, 0.6f);
            Material glowMat = LunarMaterialLibrary.GetEmissiveMaterial("PerimeterCyan", new Color(0.2f, 0.9f, 1f), new Color(0.2f, 1.4f, 1.8f));

            int count = 12;
            float radius = 13.0f;
            for (int i = 0; i < count; i++)
            {
                float ang = (i / (float)count) * Mathf.PI * 2f;
                Vector3 pos = new Vector3(Mathf.Cos(ang) * radius, 0f, Mathf.Sin(ang) * radius);
                pos.y = SampleHeight(pos.x, pos.z);

                CreateArchPart(PrimitiveType.Cylinder, $"MarkerPost_{i}", ringRoot.transform, pos + new Vector3(0f, 0.28f, 0f), new Vector3(0.16f, 0.28f, 0.16f), postMat);
                GameObject cap = CreateArchPart(PrimitiveType.Sphere, $"MarkerGlow_{i}", ringRoot.transform, pos + new Vector3(0f, 0.60f, 0f), new Vector3(0.22f, 0.22f, 0.22f), glowMat);
                Destroy(cap.GetComponent<Collider>());
            }
        }

        private void SpawnRegolithPatchesAndBoulders()
        {
            System.Random rng = new System.Random(20261109);
            Vector3 depotPos = new Vector3(0f, 0f, 8.0f);
            Vector3 habPos = new Vector3(-13.5f, 0f, 13.5f);

            Vector2[] starterSpots = new Vector2[]
            {
                new Vector2(-2.4f, 0.8f),
                new Vector2(2.6f, 0.4f),
                new Vector2(0.2f, 2.2f),
                new Vector2(-4.2f, 3.6f),
                new Vector2(4.4f, 3.8f),
                new Vector2(6.5f, -1.5f),
                new Vector2(-6.2f, -1.8f)
            };

            int idx = 0;
            for (int i = 0; i < starterSpots.Length; i++)
            {
                SpawnSingleDustPatch(starterSpots[i].x, starterSpots[i].y, idx++, 1.45f, 10f);
            }

            int safety = 0;
            while (idx < dustPatchCount && safety < 600)
            {
                safety++;
                float ang = (float)(rng.NextDouble() * Mathf.PI * 2.0);
                float t = Mathf.Pow((float)rng.NextDouble(), 0.72f);
                float dist = Mathf.Lerp(spawnRadiusMin, spawnRadiusMax, t);
                float px = Mathf.Cos(ang) * dist;
                float pz = Mathf.Sin(ang) * dist;

                if (Vector2.Distance(new Vector2(px, pz), new Vector2(depotPos.x, depotPos.z)) < 5.8f) continue;
                if (Vector2.Distance(new Vector2(px, pz), new Vector2(habPos.x, habPos.z)) < 8.2f) continue;
                if (Vector2.Distance(new Vector2(px, pz), new Vector2(0f, -5.5f)) < 3.2f) continue;

                float radiusScale = Mathf.Lerp(1.25f, 2.45f, (float)rng.NextDouble());
                float massKg = Mathf.Round(Mathf.Lerp(9f, 28f, (radiusScale - 1.25f) / 1.2f));
                SpawnSingleDustPatch(px, pz, idx++, radiusScale, massKg);
            }

            Material boulderMat = craterRockMaterial != null ? craterRockMaterial : LunarMaterialLibrary.GetBoulderMaterial();
            for (int i = 0; i < 48; i++)
            {
                float ang = (float)(rng.NextDouble() * Mathf.PI * 2.0);
                float dist = Mathf.Lerp(14.5f, 92f, (float)rng.NextDouble());
                float bx = Mathf.Cos(ang) * dist;
                float bz = Mathf.Sin(ang) * dist;

                if (Vector2.Distance(new Vector2(bx, bz), new Vector2(habPos.x, habPos.z)) < 9.0f) continue;

                float by = SampleHeight(bx, bz);
                GameObject rock = new GameObject($"LunarBoulder_{i}");
                rock.transform.SetParent(transform, false);
                rock.transform.position = new Vector3(bx, by - 0.12f, bz);
                rock.transform.rotation = Quaternion.Euler(
                    (float)rng.NextDouble() * 25f,
                    (float)rng.NextDouble() * 360f,
                    (float)rng.NextDouble() * 25f
                );

                float s = Mathf.Lerp(0.75f, 3.1f, Mathf.Pow((float)rng.NextDouble(), 1.4f));
                rock.transform.localScale = new Vector3(s * 1.15f, s * 0.78f, s);

                Mesh rockMesh = LunarMaterialLibrary.CreateBoulderMesh(i + 101);
                MeshFilter mf = rock.AddComponent<MeshFilter>();
                mf.sharedMesh = rockMesh;

                MeshRenderer mr = rock.AddComponent<MeshRenderer>();
                mr.sharedMaterial = boulderMat;
                mr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.On;
                mr.receiveShadows = true;

                SphereCollider sc = rock.AddComponent<SphereCollider>();
                sc.radius = 0.42f;
            }
        }

        private void SpawnSingleDustPatch(float x, float z, int index, float radiusScale, float massKg)
        {
            float y = SampleHeight(x, z);
            Vector3 normal = SampleNormal(x, z);

            GameObject patchObj = new GameObject($"DustPatch_{index}");
            patchObj.transform.SetParent(transform, false);
            patchObj.transform.position = new Vector3(x, y, z);
            patchObj.transform.rotation = Quaternion.FromToRotation(Vector3.up, normal) * Quaternion.Euler(0f, (index * 53f) % 360f, 0f);

            RegolithDustPatch patch = patchObj.AddComponent<RegolithDustPatch>();
            patch.ConfigureVisuals(index + 17, radiusScale, massKg);
        }

        private static GameObject CreateArchPart(PrimitiveType type, string partName, Transform parent, Vector3 localPos, Vector3 localScale, Material mat)
        {
            GameObject go = GameObject.CreatePrimitive(type);
            go.name = partName;
            go.transform.SetParent(parent, false);
            go.transform.localPosition = localPos;
            go.transform.localScale = localScale;
            Renderer r = go.GetComponent<Renderer>();
            if (r != null)
            {
                r.sharedMaterial = mat;
                r.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.On;
                r.receiveShadows = true;
            }
            return go;
        }
    }
}
