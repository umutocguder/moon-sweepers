/*
 * Created by 11Hopem
 * Copyright (c) 2026 11Hopem. All rights reserved.
 */

using System.Collections.Generic;
using UnityEngine;
using ElevenHopem.MoonSweepers.Core;

namespace ElevenHopem.MoonSweepers.Environment
{
    public class MeteorShowerManager : MonoBehaviour
    {
        private class FallingMeteor
        {
            public Transform root;
            public Vector3 startPos;
            public Vector3 impactPos;
            public float progress;
            public float duration;
            public int seed;
        }

        private class ImpactShockwave
        {
            public Transform ring;
            public float progress;
        }

        public static MeteorShowerManager Instance { get; private set; }

        private readonly List<FallingMeteor> fallingMeteors = new List<FallingMeteor>();
        private readonly List<ImpactShockwave> shockwaves = new List<ImpactShockwave>();
        private float nextMeteorTimer = 14f;
        private int meteorCounter = 0;

        public float SecondsUntilNextMeteor => Mathf.Max(0f, nextMeteorTimer);

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        private void Update()
        {
            if (GameManager.Instance != null && GameManager.Instance.IsGamePaused) return;

            nextMeteorTimer -= Time.deltaTime;
            if (nextMeteorTimer <= 0f)
            {
                nextMeteorTimer = Random.Range(38f, 54f);
                LaunchMeteorite();
            }

            UpdateFallingMeteors();
            UpdateShockwaves();
        }

        public void LaunchMeteorite()
        {
            float ang = Random.Range(0f, Mathf.PI * 2f);
            float dist = Random.Range(19f, 68f);
            float tx = Mathf.Cos(ang) * dist;
            float tz = Mathf.Sin(ang) * dist;
            float ty = LunarSurfaceGenerator.SampleHeight(tx, tz);
            Vector3 impact = new Vector3(tx, ty, tz);

            Vector3 skyOffset = new Vector3(Random.Range(-45f, 45f), 115f, Random.Range(35f, 75f));
            Vector3 start = impact + skyOffset;

            GameObject meteorObj = new GameObject($"FallingMeteor_{meteorCounter}");
            meteorObj.transform.position = start;

            GameObject rock = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            Destroy(rock.GetComponent<Collider>());
            rock.transform.SetParent(meteorObj.transform, false);
            rock.transform.localScale = Vector3.one * 1.35f;
            rock.GetComponent<Renderer>().sharedMaterial = LunarMaterialLibrary.GetEmissiveMaterial(
                "MeteorFireball",
                new Color(1.0f, 0.55f, 0.15f),
                new Color(3.2f, 1.4f, 0.25f)
            );

            GameObject tail = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            Destroy(tail.GetComponent<Collider>());
            tail.transform.SetParent(meteorObj.transform, false);
            Vector3 flightDir = (impact - start).normalized;
            meteorObj.transform.rotation = Quaternion.LookRotation(flightDir, Vector3.up);
            tail.transform.localPosition = new Vector3(0f, 0f, -6.5f);
            tail.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
            tail.transform.localScale = new Vector3(0.85f, 6.5f, 0.85f);
            tail.GetComponent<Renderer>().sharedMaterial = LunarMaterialLibrary.GetTransparentMaterial(
                "MeteorPlasmaTail",
                new Color(1.0f, 0.60f, 0.18f, 0.42f),
                new Color(2.4f, 0.95f, 0.2f)
            );

            Light flash = meteorObj.AddComponent<Light>();
            flash.type = LightType.Point;
            flash.color = new Color(1.0f, 0.65f, 0.25f);
            flash.range = 32f;
            flash.intensity = 4.0f;

            fallingMeteors.Add(new FallingMeteor
            {
                root = meteorObj.transform,
                startPos = start,
                impactPos = impact,
                progress = 0f,
                duration = 1.65f,
                seed = 800 + (++meteorCounter) * 19
            });
        }

        private void UpdateFallingMeteors()
        {
            for (int i = fallingMeteors.Count - 1; i >= 0; i--)
            {
                FallingMeteor m = fallingMeteors[i];
                if (m.root == null)
                {
                    fallingMeteors.RemoveAt(i);
                    continue;
                }

                m.progress += Time.deltaTime / m.duration;
                if (m.progress >= 1f)
                {
                    SpawnMeteorImpactNode(m.impactPos, m.seed);
                    Destroy(m.root.gameObject);
                    fallingMeteors.RemoveAt(i);
                    continue;
                }

                m.root.position = Vector3.Lerp(m.startPos, m.impactPos, m.progress);
                fallingMeteors[i] = m;
            }
        }

        private void SpawnMeteorImpactNode(Vector3 pos, int seed)
        {
            AudioManager.Instance?.PlayMeteorImpact();
            GameManager.Instance?.NotifyToast("METEORITE IMPACT DETECTED! Check Lunar Radar (Gold Blip) for Chondrite Core!");

            Vector3 norm = LunarSurfaceGenerator.SampleNormal(pos.x, pos.z);
            GameObject patchObj = new GameObject($"MeteoriteNode_{seed}");
            patchObj.transform.position = pos;
            patchObj.transform.rotation = Quaternion.FromToRotation(Vector3.up, norm);

            RegolithDustPatch patch = patchObj.AddComponent<RegolithDustPatch>();
            patch.ConfigureAsMeteorite(seed, 32f);

            GameObject ring = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            ring.name = "ImpactShockwave";
            Destroy(ring.GetComponent<Collider>());
            ring.transform.position = pos + Vector3.up * 0.15f;
            ring.transform.localScale = new Vector3(1f, 0.04f, 1f);
            ring.GetComponent<Renderer>().sharedMaterial = LunarMaterialLibrary.GetTransparentMaterial(
                "ShockwaveAmber",
                new Color(1.0f, 0.65f, 0.2f, 0.45f),
                new Color(2.0f, 0.9f, 0.2f)
            );
            shockwaves.Add(new ImpactShockwave { ring = ring.transform, progress = 0f });
        }

        private void UpdateShockwaves()
        {
            for (int i = shockwaves.Count - 1; i >= 0; i--)
            {
                ImpactShockwave sw = shockwaves[i];
                if (sw.ring == null)
                {
                    shockwaves.RemoveAt(i);
                    continue;
                }

                sw.progress += Time.deltaTime * 1.4f;
                if (sw.progress >= 1f)
                {
                    Destroy(sw.ring.gameObject);
                    shockwaves.RemoveAt(i);
                    continue;
                }

                float r = Mathf.Lerp(1.2f, 14.0f, sw.progress);
                sw.ring.localScale = new Vector3(r, 0.03f * (1f - sw.progress), r);
                shockwaves[i] = sw;
            }
        }
    }
}
