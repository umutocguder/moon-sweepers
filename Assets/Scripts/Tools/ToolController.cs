/*
 * Created by 11Hopem
 * Copyright (c) 2026 11Hopem. All rights reserved.
 */

using System;
using System.Collections.Generic;
using UnityEngine;
using ElevenHopem.MoonSweepers.Core;
using ElevenHopem.MoonSweepers.Player;
using ElevenHopem.MoonSweepers.Environment;

namespace ElevenHopem.MoonSweepers.Tools
{
    public class ToolController : MonoBehaviour
    {
        private struct FlyingDustMote
        {
            public Transform transform;
            public Vector3 startWorld;
            public float progress;
            public float speed;
        }

        [SerializeField] private Camera playerCamera;
        [SerializeField] private PlayerInventory inventory;
        [SerializeField] private Transform toolHolder;
        [SerializeField] private List<ToolData> availableTools = new List<ToolData>();
        [SerializeField] private int activeToolIndex = 0;

        private bool[] unlockedTools = new bool[] { true, false, false, false, false };
        private PlayerController playerController;
        private float nextUseTime = 0f;
        private bool isVacuuming = false;
        private bool isActivelyCleaning = false;
        private float scrubAnimPhase = 0f;
        private float recoilImpulse = 0f;
        private Vector3 swayOffset = Vector3.zero;

        private GameObject activeToolVisual;
        private Transform nozzlePoint;
        private Transform spinningPartA;
        private Transform spinningPartB;
        private GameObject activeBeamEffect;
        private ParticleSystem groundBurstParticles;
        private readonly List<FlyingDustMote> activeMotes = new List<FlyingDustMote>();

        public ToolData CurrentTool => (availableTools != null && activeToolIndex >= 0 && activeToolIndex < availableTools.Count)
            ? availableTools[activeToolIndex]
            : null;

        public int ActiveToolIndex => activeToolIndex;
        public int ToolCount => availableTools != null ? availableTools.Count : 0;
        public bool IsActivelyCleaning => isActivelyCleaning || isVacuuming;
        public RegolithDustPatch HoveredDustPatch { get; private set; }
        public float HoveredDistance { get; private set; }

        public event Action<ToolData> OnToolChanged;

        private void Awake()
        {
            if (inventory == null)
            {
                inventory = GetComponentInParent<PlayerInventory>() ?? GetComponent<PlayerInventory>();
            }

            playerController = GetComponentInParent<PlayerController>() ?? GetComponent<PlayerController>();

            if (playerCamera == null)
            {
                playerCamera = GetComponentInChildren<Camera>() ?? Camera.main;
            }

            EnsureToolHolder();
            InitializeDefaultTools();
            EnsureGroundBurstParticles();
        }

        private void Start()
        {
            EnsureToolHolder();
            EquipTool(activeToolIndex, true);
        }

        private void EnsureToolHolder()
        {
            if (playerCamera == null)
            {
                playerCamera = GetComponentInChildren<Camera>() ?? Camera.main;
            }
            if (toolHolder == null && playerCamera != null)
            {
                Transform found = playerCamera.transform.Find("ToolHolder");
                if (found != null)
                {
                    toolHolder = found;
                }
                else
                {
                    GameObject holderObj = new GameObject("ToolHolder");
                    holderObj.transform.SetParent(playerCamera.transform, false);
                    toolHolder = holderObj.transform;
                }
            }
            if (toolHolder != null)
            {
                toolHolder.localPosition = Vector3.zero;
                toolHolder.localRotation = Quaternion.identity;
            }
        }

        private void EnsureGroundBurstParticles()
        {
            if (groundBurstParticles != null) return;

            GameObject burstObj = new GameObject("RegolithScrubParticles");
            burstObj.transform.SetParent(transform, false);
            groundBurstParticles = burstObj.AddComponent<ParticleSystem>();
            var main = groundBurstParticles.main;
            main.loop = false;
            main.playOnAwake = false;
            main.simulationSpace = ParticleSystemSimulationSpace.World;
            main.startLifetime = 0.55f;
            main.startSpeed = 2.6f;
            main.startSize = 0.16f;
            main.gravityModifier = 0.35f;
            main.startColor = new Color(0.52f, 0.54f, 0.58f, 0.85f);
            var emission = groundBurstParticles.emission;
            emission.enabled = false;
            var shape = groundBurstParticles.shape;
            shape.shapeType = ParticleSystemShapeType.Hemisphere;
            shape.radius = 0.25f;

            ParticleSystemRenderer psr = burstObj.GetComponent<ParticleSystemRenderer>();
            psr.sharedMaterial = LunarMaterialLibrary.GetParticleMaterial();
            psr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        }

        private void InitializeDefaultTools()
        {
            availableTools.Clear();

            availableTools.Add(new ToolData(
                "Standard Toothbrush", 1, ToolMode.ClickScrub,
                power: 2.0f, range: 4.8f, radius: 1.05f, cooldown: 0.15f, cost: 0,
                color: new Color(0.18f, 0.68f, 1.0f)));

            availableTools.Add(new ToolData(
                "Lunar Hand Broom", 2, ToolMode.ClickScrub,
                power: 5.5f, range: 5.8f, radius: 1.85f, cooldown: 0.22f, cost: 110,
                color: new Color(0.95f, 0.72f, 0.16f)));

            availableTools.Add(new ToolData(
                "Cyclone Vacuum Mk-I", 3, ToolMode.ContinuousHold,
                power: 24.0f, range: 7.8f, radius: 2.6f, cooldown: 0.05f, cost: 420,
                color: new Color(0.95f, 0.26f, 0.20f)));

            availableTools.Add(new ToolData(
                "Electromagnetic Sweeper", 4, ToolMode.ContinuousHold,
                power: 58.0f, range: 10.5f, radius: 3.8f, cooldown: 0.05f, cost: 1350,
                color: new Color(0.65f, 0.25f, 1.0f)));

            availableTools.Add(new ToolData(
                "Lunar Sweeper Buggy", 5, ToolMode.ContinuousHold,
                power: 145.0f, range: 13.5f, radius: 5.6f, cooldown: 0.05f, cost: 3800,
                color: new Color(0.15f, 0.92f, 0.55f)));
        }

        public ToolData GetToolAt(int index)
        {
            if (availableTools == null || index < 0 || index >= availableTools.Count) return null;
            return availableTools[index];
        }

        public bool IsToolUnlocked(int index)
        {
            if (index == 0) return true;
            if (unlockedTools == null || index < 0 || index >= unlockedTools.Length) return false;
            return unlockedTools[index];
        }

        public bool[] GetUnlockedToolStates()
        {
            bool[] copy = new bool[5];
            for (int i = 0; i < 5; i++)
            {
                copy[i] = IsToolUnlocked(i);
            }
            return copy;
        }

        public void UnlockAndEquipTool(int index)
        {
            if (index < 0 || index >= unlockedTools.Length) return;
            unlockedTools[index] = true;
            EquipTool(index, true);
        }

        public void RestoreUnlockedTools(bool[] states, int equipIdx)
        {
            if (states != null)
            {
                for (int i = 0; i < Mathf.Min(states.Length, unlockedTools.Length); i++)
                {
                    unlockedTools[i] = states[i];
                }
            }
            unlockedTools[0] = true;
            int validIdx = IsToolUnlocked(equipIdx) ? equipIdx : 0;
            EquipTool(validIdx, true);
        }

        private void Update()
        {
            UpdateFlyingDustMotes();

            if (GameManager.Instance != null && (GameManager.Instance.IsGamePaused || GameManager.Instance.IsShopOpen))
            {
                if (isVacuuming) StopVacuum();
                isActivelyCleaning = false;
                return;
            }

            UpdateTargetDetection();
            HandleToolSwitching();
            HandleToolUsage();
            AnimateViewmodel();
        }

        private void UpdateTargetDetection()
        {
            HoveredDustPatch = null;
            HoveredDistance = 0f;

            ToolData current = CurrentTool;
            if (current == null || playerCamera == null) return;

            Ray ray = playerCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));

            if (Physics.SphereCast(ray, 0.42f, out RaycastHit sphereHit, current.sweepRangeMeters))
            {
                RegolithDustPatch patch = sphereHit.collider.GetComponent<RegolithDustPatch>() ?? sphereHit.collider.GetComponentInParent<RegolithDustPatch>();
                if (patch != null && !patch.IsFullyCleaned)
                {
                    HoveredDustPatch = patch;
                    HoveredDistance = sphereHit.distance;
                    return;
                }
            }

            if (Physics.Raycast(ray, out RaycastHit groundHit, current.sweepRangeMeters))
            {
                Collider[] nearby = Physics.OverlapSphere(groundHit.point, current.sweepRadiusMeters * 0.85f);
                float bestDist = float.MaxValue;
                for (int i = 0; i < nearby.Length; i++)
                {
                    RegolithDustPatch p = nearby[i].GetComponent<RegolithDustPatch>();
                    if (p != null && !p.IsFullyCleaned)
                    {
                        float d = Vector3.Distance(groundHit.point, p.transform.position);
                        if (d < bestDist)
                        {
                            bestDist = d;
                            HoveredDustPatch = p;
                            HoveredDistance = groundHit.distance;
                        }
                    }
                }
            }
        }

        private void HandleToolSwitching()
        {
            for (int i = 0; i < availableTools.Count; i++)
            {
                if (Input.GetKeyDown(KeyCode.Alpha1 + i))
                {
                    if (IsToolUnlocked(i))
                    {
                        EquipTool(i, false);
                    }
                    else
                    {
                        GameManager.Instance?.NotifyToast($"Locked: {availableTools[i].toolName} — Press [TAB] to unlock in Hangar!");
                    }
                }
            }

            float scroll = Input.GetAxis("Mouse ScrollWheel");
            if (Mathf.Abs(scroll) > 0.02f)
            {
                int dir = scroll > 0f ? 1 : -1;
                for (int step = 1; step <= availableTools.Count; step++)
                {
                    int candidate = (activeToolIndex + dir * step + availableTools.Count * 2) % availableTools.Count;
                    if (IsToolUnlocked(candidate))
                    {
                        if (candidate != activeToolIndex)
                        {
                            EquipTool(candidate, false);
                        }
                        break;
                    }
                }
            }
        }

        private void HandleToolUsage()
        {
            ToolData current = CurrentTool;
            if (current == null || Cursor.lockState != CursorLockMode.Locked)
            {
                if (isVacuuming) StopVacuum();
                isActivelyCleaning = false;
                return;
            }

            bool fireHeld = Input.GetMouseButton(0);
            bool fireDown = Input.GetMouseButtonDown(0);

            if (current.mode == ToolMode.ClickScrub)
            {
                isActivelyCleaning = fireHeld;
                if ((fireDown || fireHeld) && Time.time >= nextUseTime)
                {
                    float cd = fireDown ? current.useCooldownSeconds * 0.7f : current.useCooldownSeconds;
                    nextUseTime = Time.time + cd;
                    PerformScrub(current);
                }
            }
            else
            {
                if (fireHeld)
                {
                    if (inventory != null && inventory.IsFull)
                    {
                        if (isVacuuming) StopVacuum();
                        isActivelyCleaning = false;
                        return;
                    }

                    if (!isVacuuming) StartVacuum(current.tier);
                    isActivelyCleaning = true;

                    if (Time.time >= nextUseTime)
                    {
                        nextUseTime = Time.time + current.useCooldownSeconds;
                        PerformContinuousClean(current);
                    }
                }
                else
                {
                    isActivelyCleaning = false;
                    if (isVacuuming) StopVacuum();
                }
            }
        }

        private void PerformScrub(ToolData tool)
        {
            recoilImpulse = 1.0f;
            AudioManager.Instance?.PlayBrushScrape();

            if (inventory != null && inventory.IsFull)
            {
                GameManager.Instance?.NotifyToast("Backpack Full! Walk onto the glowing Depot Pad ahead to cash in!");
                return;
            }

            CleanInTargetZone(tool.sweepRangeMeters, tool.sweepRadiusMeters, tool.sweepPowerKg);
        }

        private void PerformContinuousClean(ToolData tool)
        {
            float frameCleanPower = tool.sweepPowerKg * tool.useCooldownSeconds;
            CleanInTargetZone(tool.sweepRangeMeters, tool.sweepRadiusMeters, frameCleanPower);
        }

        private void CleanInTargetZone(float range, float radius, float amountKg)
        {
            if (playerCamera == null || inventory == null) return;

            Ray ray = playerCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));
            Vector3 centerPoint;
            bool foundSurface = false;

            if (Physics.SphereCast(ray, 0.36f, out RaycastHit hit, range))
            {
                centerPoint = hit.point;
                foundSurface = true;
            }
            else if (Physics.Raycast(ray, out RaycastHit rayHit, range))
            {
                centerPoint = rayHit.point;
                foundSurface = true;
            }
            else
            {
                centerPoint = ray.GetPoint(range * 0.75f);
            }

            Collider[] hits = Physics.OverlapSphere(centerPoint, radius);
            bool cleanedAny = false;

            for (int i = 0; i < hits.Length; i++)
            {
                RegolithDustPatch patch = hits[i].GetComponent<RegolithDustPatch>() ?? hits[i].GetComponentInParent<RegolithDustPatch>();
                if (patch != null && !patch.IsFullyCleaned)
                {
                    float gathered = patch.CleanDust(amountKg);
                    if (gathered > 0f)
                    {
                        inventory.AddDust(gathered);
                        cleanedAny = true;
                        SpawnCleaningVisualFeedback(patch.transform.position + Vector3.up * 0.15f);
                    }

                    if (inventory.IsFull) break;
                }
            }

            if (!cleanedAny && foundSurface && HoveredDustPatch != null && !HoveredDustPatch.IsFullyCleaned)
            {
                float gathered = HoveredDustPatch.CleanDust(amountKg);
                if (gathered > 0f)
                {
                    inventory.AddDust(gathered);
                    SpawnCleaningVisualFeedback(HoveredDustPatch.transform.position + Vector3.up * 0.15f);
                }
            }
        }

        private void SpawnCleaningVisualFeedback(Vector3 worldPos)
        {
            if (groundBurstParticles != null)
            {
                groundBurstParticles.transform.position = worldPos;
                groundBurstParticles.Emit(6);
            }

            if (nozzlePoint != null && activeMotes.Count < 28)
            {
                int spawnCount = CurrentTool != null && CurrentTool.tier >= 3 ? 2 : 1;
                Material moteMat = LunarMaterialLibrary.GetDustMoundMaterial();

                for (int i = 0; i < spawnCount; i++)
                {
                    GameObject mote = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                    mote.name = "SuctionDustMote";
                    Destroy(mote.GetComponent<Collider>());
                    Vector3 jitter = UnityEngine.Random.insideUnitSphere * 0.28f;
                    jitter.y = Mathf.Abs(jitter.y);
                    mote.transform.position = worldPos + jitter;
                    mote.transform.localScale = Vector3.one * UnityEngine.Random.Range(0.045f, 0.095f);
                    Renderer r = mote.GetComponent<Renderer>();
                    r.sharedMaterial = moteMat;
                    r.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;

                    activeMotes.Add(new FlyingDustMote
                    {
                        transform = mote.transform,
                        startWorld = mote.transform.position,
                        progress = 0f,
                        speed = UnityEngine.Random.Range(3.6f, 5.4f)
                    });
                }
            }
        }

        private void UpdateFlyingDustMotes()
        {
            for (int i = activeMotes.Count - 1; i >= 0; i--)
            {
                FlyingDustMote m = activeMotes[i];
                if (m.transform == null || nozzlePoint == null)
                {
                    if (m.transform != null) Destroy(m.transform.gameObject);
                    activeMotes.RemoveAt(i);
                    continue;
                }

                m.progress += Time.deltaTime * m.speed;
                if (m.progress >= 1f)
                {
                    Destroy(m.transform.gameObject);
                    activeMotes.RemoveAt(i);
                    continue;
                }

                Vector3 target = nozzlePoint.position;
                Vector3 arcPos = Vector3.Lerp(m.startWorld, target, m.progress);
                arcPos.y += Mathf.Sin(m.progress * Mathf.PI) * 0.22f;
                m.transform.position = arcPos;
                m.transform.localScale = Vector3.one * Mathf.Lerp(0.08f, 0.015f, m.progress);
                activeMotes[i] = m;
            }
        }

        private void StartVacuum(int tier)
        {
            isVacuuming = true;
            float pitch = tier == 3 ? 1.0f : (tier == 4 ? 1.28f : 0.82f);
            AudioManager.Instance?.SetVacuumActive(true, pitch);
            if (activeBeamEffect != null)
            {
                activeBeamEffect.SetActive(true);
            }
        }

        private void StopVacuum()
        {
            isVacuuming = false;
            AudioManager.Instance?.SetVacuumActive(false);
            if (activeBeamEffect != null)
            {
                activeBeamEffect.SetActive(false);
            }
        }

        public void EquipTool(int index)
        {
            EquipTool(index, false);
        }

        public void EquipTool(int index, bool force)
        {
            if (index < 0 || index >= availableTools.Count) return;
            if (!force && !IsToolUnlocked(index)) return;

            if (isVacuuming) StopVacuum();

            activeToolIndex = index;
            ToolData selected = availableTools[activeToolIndex];

            if (playerController == null)
            {
                playerController = GetComponentInParent<PlayerController>() ?? GetComponent<PlayerController>();
            }
            if (playerController != null)
            {
                playerController.SetVehicleSpeedMultiplier(selected.tier == 5 ? 1.65f : 1.0f);
            }

            BuildDetailedViewmodel(selected);
            OnToolChanged?.Invoke(selected);
        }

        private void BuildDetailedViewmodel(ToolData tool)
        {
            EnsureToolHolder();
            if (toolHolder == null) return;

            if (activeToolVisual != null)
            {
                Destroy(activeToolVisual);
            }
            spinningPartA = null;
            spinningPartB = null;
            activeBeamEffect = null;
            nozzlePoint = null;

            activeToolVisual = new GameObject($"Viewmodel_Tier{tool.tier}");
            activeToolVisual.transform.SetParent(toolHolder, false);

            Material suitWhite = LunarMaterialLibrary.GetSurfaceMaterial("SuitFabricWhite", new Color(0.92f, 0.94f, 0.97f), 0.1f, 0.38f);
            Material cuffAnodized = LunarMaterialLibrary.GetSurfaceMaterial("WristCuffBlue", new Color(0.16f, 0.45f, 0.82f), 0.8f, 0.72f);
            Material gloveGrey = LunarMaterialLibrary.GetSurfaceMaterial("SuitGloveGrey", new Color(0.32f, 0.35f, 0.40f), 0.25f, 0.45f);
            Material gunmetal = LunarMaterialLibrary.GetSurfaceMaterial("ToolGunmetal", new Color(0.20f, 0.23f, 0.27f), 0.85f, 0.68f);
            Material chrome = LunarMaterialLibrary.GetSurfaceMaterial("ToolChrome", new Color(0.82f, 0.86f, 0.92f), 0.92f, 0.86f);

            if (tool.tier < 5)
            {
                GameObject armRoot = new GameObject("AstronautRightArm");
                armRoot.transform.SetParent(activeToolVisual.transform, false);
                armRoot.transform.localPosition = new Vector3(0.30f, -0.27f, 0.34f);
                armRoot.transform.localRotation = Quaternion.Euler(14f, -14f, 8f);

                AddViewPart(PrimitiveType.Cylinder, "ForearmSleeve", armRoot.transform, new Vector3(0.04f, -0.06f, -0.18f), Quaternion.Euler(78f, 8f, 0f), new Vector3(0.105f, 0.19f, 0.105f), suitWhite);
                AddViewPart(PrimitiveType.Cylinder, "WristPressureRing", armRoot.transform, new Vector3(0.01f, -0.015f, -0.02f), Quaternion.Euler(78f, 8f, 0f), new Vector3(0.112f, 0.024f, 0.112f), cuffAnodized);
                AddViewPart(PrimitiveType.Sphere, "GlovePalm", armRoot.transform, new Vector3(0f, 0f, 0.05f), Quaternion.identity, new Vector3(0.095f, 0.082f, 0.115f), gloveGrey);
                AddViewPart(PrimitiveType.Capsule, "GloveThumb", armRoot.transform, new Vector3(-0.048f, 0.022f, 0.065f), Quaternion.Euler(25f, -30f, 40f), new Vector3(0.032f, 0.038f, 0.032f), suitWhite);
            }

            if (tool.tier == 1)
            {
                BuildToothbrushModel(activeToolVisual.transform, suitWhite, chrome);
            }
            else if (tool.tier == 2)
            {
                BuildHandBroomModel(activeToolVisual.transform, gunmetal, chrome);
            }
            else if (tool.tier == 3)
            {
                BuildCycloneVacuumModel(activeToolVisual.transform, gunmetal, chrome);
            }
            else if (tool.tier == 4)
            {
                BuildElectroSweeperModel(activeToolVisual.transform, gunmetal, chrome);
            }
            else
            {
                BuildBuggyCockpitModel(activeToolVisual.transform, gunmetal, chrome);
            }
        }

        private void BuildToothbrushModel(Transform root, Material whitePlastic, Material trimMat)
        {
            Material blueGrip = LunarMaterialLibrary.GetSurfaceMaterial("ToothbrushCyan", new Color(0.12f, 0.65f, 0.98f), 0.15f, 0.78f);
            Material bristleCyan = LunarMaterialLibrary.GetEmissiveMaterial("BristleTipCyan", new Color(0.65f, 0.95f, 1f), new Color(0.1f, 0.35f, 0.5f));

            GameObject tb = new GameObject("ToothbrushAssembly");
            tb.transform.SetParent(root, false);
            tb.transform.localPosition = new Vector3(0.28f, -0.21f, 0.46f);
            tb.transform.localRotation = Quaternion.Euler(18f, -18f, 10f);

            AddViewPart(PrimitiveType.Cube, "HandleGrip", tb.transform, new Vector3(0f, 0f, 0f), Quaternion.identity, new Vector3(0.022f, 0.018f, 0.20f), blueGrip);
            AddViewPart(PrimitiveType.Cube, "HandleInlay", tb.transform, new Vector3(0f, 0.006f, 0.01f), Quaternion.identity, new Vector3(0.014f, 0.010f, 0.16f), whitePlastic);
            AddViewPart(PrimitiveType.Cube, "AngledNeck", tb.transform, new Vector3(0f, 0.012f, 0.135f), Quaternion.Euler(-10f, 0f, 0f), new Vector3(0.015f, 0.012f, 0.09f), whitePlastic);
            GameObject head = AddViewPart(PrimitiveType.Cube, "BrushHeadPlate", tb.transform, new Vector3(0f, 0.022f, 0.205f), Quaternion.Euler(-4f, 0f, 0f), new Vector3(0.026f, 0.012f, 0.068f), whitePlastic);

            for (int r = 0; r < 7; r++)
            {
                for (int c = 0; c < 4; c++)
                {
                    float bz = -0.026f + r * 0.0086f;
                    float bx = -0.0082f + c * 0.0055f;
                    Material bMat = (r == 0 || r == 6) ? bristleCyan : whitePlastic;
                    AddViewPart(PrimitiveType.Cylinder, $"Bristle_{r}_{c}", tb.transform,
                        new Vector3(bx, 0.038f, 0.205f + bz),
                        Quaternion.identity,
                        new Vector3(0.0068f, 0.014f, 0.0068f),
                        bMat);
                }
            }

            nozzlePoint = head.transform;
        }

        private void BuildHandBroomModel(Transform root, Material darkCarbon, Material chrome)
        {
            Material goldAnodized = LunarMaterialLibrary.GetSurfaceMaterial("BroomGold", new Color(0.95f, 0.70f, 0.14f), 0.72f, 0.65f);
            Material darkBristle = LunarMaterialLibrary.GetSurfaceMaterial("BroomBristles", new Color(0.14f, 0.16f, 0.20f), 0.05f, 0.2f);

            GameObject broom = new GameObject("HandBroomAssembly");
            broom.transform.SetParent(root, false);
            broom.transform.localPosition = new Vector3(0.28f, -0.22f, 0.52f);
            broom.transform.localRotation = Quaternion.Euler(28f, -20f, 12f);

            AddViewPart(PrimitiveType.Cylinder, "CarbonShaft", broom.transform, new Vector3(0f, 0f, 0.05f), Quaternion.Euler(90f, 0f, 0f), new Vector3(0.028f, 0.28f, 0.028f), darkCarbon);
            AddViewPart(PrimitiveType.Cylinder, "ShaftCollar", broom.transform, new Vector3(0f, 0f, 0.26f), Quaternion.Euler(90f, 0f, 0f), new Vector3(0.042f, 0.03f, 0.042f), chrome);
            GameObject head = AddViewPart(PrimitiveType.Cube, "WideSweepBar", broom.transform, new Vector3(0f, -0.01f, 0.34f), Quaternion.Euler(15f, 0f, 0f), new Vector3(0.30f, 0.038f, 0.055f), goldAnodized);

            for (int i = -6; i <= 6; i++)
            {
                AddViewPart(PrimitiveType.Cube, $"BroomTuft_{i}", broom.transform,
                    new Vector3(i * 0.021f, -0.052f, 0.365f),
                    Quaternion.Euler(22f, 0f, 0f),
                    new Vector3(0.017f, 0.065f, 0.036f),
                    darkBristle);
            }

            nozzlePoint = head.transform;
        }

        private void BuildCycloneVacuumModel(Transform root, Material gunmetal, Material chrome)
        {
            Material crimsonShell = LunarMaterialLibrary.GetSurfaceMaterial("VacCrimson", new Color(0.88f, 0.18f, 0.16f), 0.65f, 0.76f);
            Material glassChamber = LunarMaterialLibrary.GetTransparentMaterial("CycloneGlass", new Color(0.18f, 0.82f, 1.0f, 0.36f), new Color(0.08f, 0.45f, 0.7f));
            Material cyanTurbine = LunarMaterialLibrary.GetEmissiveMaterial("VacCyanCore", new Color(0.2f, 0.92f, 1f), new Color(0.25f, 1.6f, 2.2f));
            Material coneMat = LunarMaterialLibrary.GetTransparentMaterial("SuctionVortexCone", new Color(0.35f, 0.9f, 1f, 0.20f), new Color(0.15f, 0.65f, 0.95f));

            GameObject vac = new GameObject("CycloneVacuumAssembly");
            vac.transform.SetParent(root, false);
            vac.transform.localPosition = new Vector3(0.29f, -0.23f, 0.48f);
            vac.transform.localRotation = Quaternion.Euler(8f, -10f, 0f);

            AddViewPart(PrimitiveType.Cube, "MotorHousing", vac.transform, new Vector3(0f, 0f, -0.04f), Quaternion.identity, new Vector3(0.12f, 0.13f, 0.20f), crimsonShell);
            AddViewPart(PrimitiveType.Cube, "TopCarryRail", vac.transform, new Vector3(0f, 0.085f, 0.02f), Quaternion.identity, new Vector3(0.035f, 0.025f, 0.24f), gunmetal);
            AddViewPart(PrimitiveType.Cylinder, "CycloneGlassDrum", vac.transform, new Vector3(0f, 0.005f, 0.14f), Quaternion.Euler(90f, 0f, 0f), new Vector3(0.115f, 0.085f, 0.115f), glassChamber);

            GameObject rotor = new GameObject("TurbineRotorPivot");
            rotor.transform.SetParent(vac.transform, false);
            rotor.transform.localPosition = new Vector3(0f, 0.005f, 0.14f);
            spinningPartA = rotor.transform;
            AddViewPart(PrimitiveType.Cube, "RotorBlade1", rotor.transform, Vector3.zero, Quaternion.identity, new Vector3(0.088f, 0.015f, 0.12f), cyanTurbine);
            AddViewPart(PrimitiveType.Cube, "RotorBlade2", rotor.transform, Vector3.zero, Quaternion.Euler(0f, 0f, 90f), new Vector3(0.088f, 0.015f, 0.12f), cyanTurbine);

            AddViewPart(PrimitiveType.Cylinder, "IntakeBarrel", vac.transform, new Vector3(0f, -0.01f, 0.32f), Quaternion.Euler(90f, 0f, 0f), new Vector3(0.058f, 0.11f, 0.058f), gunmetal);
            GameObject nozzle = AddViewPart(PrimitiveType.Cube, "WideFlaredNozzle", vac.transform, new Vector3(0f, -0.025f, 0.45f), Quaternion.Euler(14f, 0f, 0f), new Vector3(0.19f, 0.048f, 0.09f), chrome);
            AddViewPart(PrimitiveType.Cube, "NozzleGlowLip", vac.transform, new Vector3(0f, -0.025f, 0.495f), Quaternion.Euler(14f, 0f, 0f), new Vector3(0.195f, 0.014f, 0.015f), cyanTurbine);

            GameObject cone = AddViewPart(PrimitiveType.Cylinder, "SuctionAirVortex", vac.transform, new Vector3(0f, -0.08f, 0.88f), Quaternion.Euler(96f, 0f, 0f), new Vector3(0.42f, 0.38f, 0.42f), coneMat);
            cone.SetActive(false);
            activeBeamEffect = cone;
            nozzlePoint = nozzle.transform;
        }

        private void BuildElectroSweeperModel(Transform root, Material gunmetal, Material chrome)
        {
            Material violetCore = LunarMaterialLibrary.GetEmissiveMaterial("ElectroViolet", new Color(0.72f, 0.28f, 1.0f), new Color(1.4f, 0.45f, 2.4f));
            Material arcField = LunarMaterialLibrary.GetTransparentMaterial("ElectroArcBeam", new Color(0.65f, 0.35f, 1.0f, 0.26f), new Color(0.85f, 0.35f, 1.8f));

            GameObject electro = new GameObject("ElectroSweeperAssembly");
            electro.transform.SetParent(root, false);
            electro.transform.localPosition = new Vector3(0.30f, -0.23f, 0.48f);
            electro.transform.localRotation = Quaternion.Euler(6f, -9f, 0f);

            AddViewPart(PrimitiveType.Cube, "ChassisBody", electro.transform, new Vector3(0f, 0f, 0f), Quaternion.identity, new Vector3(0.13f, 0.11f, 0.28f), gunmetal);
            AddViewPart(PrimitiveType.Cylinder, "PlasmaCoreTube", electro.transform, new Vector3(0f, 0.02f, 0.14f), Quaternion.Euler(90f, 0f, 0f), new Vector3(0.065f, 0.16f, 0.065f), violetCore);

            AddViewPart(PrimitiveType.Cube, "LeftRailProng", electro.transform, new Vector3(-0.065f, 0.0f, 0.32f), Quaternion.identity, new Vector3(0.022f, 0.042f, 0.26f), chrome);
            AddViewPart(PrimitiveType.Cube, "RightRailProng", electro.transform, new Vector3(0.065f, 0.0f, 0.32f), Quaternion.identity, new Vector3(0.022f, 0.042f, 0.26f), chrome);

            GameObject ringSpin = new GameObject("MagneticCoilSpinner");
            ringSpin.transform.SetParent(electro.transform, false);
            ringSpin.transform.localPosition = new Vector3(0f, 0.01f, 0.36f);
            spinningPartA = ringSpin.transform;
            AddViewPart(PrimitiveType.Cylinder, "CoilRingA", ringSpin.transform, Vector3.zero, Quaternion.Euler(90f, 0f, 0f), new Vector3(0.11f, 0.012f, 0.11f), violetCore);

            GameObject muzzle = new GameObject("ElectroNozzleAnchor");
            muzzle.transform.SetParent(electro.transform, false);
            muzzle.transform.localPosition = new Vector3(0f, 0f, 0.46f);
            nozzlePoint = muzzle.transform;

            GameObject beam = AddViewPart(PrimitiveType.Cylinder, "MagneticPulseCone", electro.transform, new Vector3(0f, -0.05f, 1.05f), Quaternion.Euler(94f, 0f, 0f), new Vector3(0.62f, 0.55f, 0.62f), arcField);
            beam.SetActive(false);
            activeBeamEffect = beam;
        }

        private void BuildBuggyCockpitModel(Transform root, Material gunmetal, Material chrome)
        {
            Material roverGreen = LunarMaterialLibrary.GetSurfaceMaterial("BuggyEmerald", new Color(0.12f, 0.72f, 0.46f), 0.68f, 0.72f);
            Material brushYellow = LunarMaterialLibrary.GetEmissiveMaterial("BuggyBrushGlow", new Color(1.0f, 0.78f, 0.18f), new Color(0.45f, 0.32f, 0.05f));
            Material dashGlow = LunarMaterialLibrary.GetEmissiveMaterial("BuggyDashCyan", new Color(0.15f, 0.95f, 0.75f), new Color(0.25f, 1.6f, 1.2f));

            GameObject cockpit = new GameObject("LunarBuggyCockpit");
            cockpit.transform.SetParent(root, false);
            cockpit.transform.localPosition = Vector3.zero;
            cockpit.transform.localRotation = Quaternion.identity;

            AddViewPart(PrimitiveType.Cube, "DashboardConsole", cockpit.transform, new Vector3(0f, -0.40f, 0.62f), Quaternion.Euler(-18f, 0f, 0f), new Vector3(0.95f, 0.16f, 0.28f), gunmetal);
            AddViewPart(PrimitiveType.Cube, "HoodArmor", cockpit.transform, new Vector3(0f, -0.44f, 0.95f), Quaternion.Euler(6f, 0f, 0f), new Vector3(0.88f, 0.08f, 0.45f), roverGreen);
            AddViewPart(PrimitiveType.Cube, "TelemetryScreen", cockpit.transform, new Vector3(0f, -0.31f, 0.56f), Quaternion.Euler(-22f, 0f, 0f), new Vector3(0.32f, 0.08f, 0.02f), dashGlow);

            AddViewPart(PrimitiveType.Cylinder, "RollCageLeft", cockpit.transform, new Vector3(-0.56f, 0.05f, 0.58f), Quaternion.Euler(-22f, 0f, -10f), new Vector3(0.036f, 0.52f, 0.036f), gunmetal);
            AddViewPart(PrimitiveType.Cylinder, "RollCageRight", cockpit.transform, new Vector3(0.56f, 0.05f, 0.58f), Quaternion.Euler(-22f, 0f, 10f), new Vector3(0.036f, 0.52f, 0.036f), gunmetal);
            AddViewPart(PrimitiveType.Cylinder, "RollCageTopBar", cockpit.transform, new Vector3(0f, 0.44f, 0.46f), Quaternion.Euler(0f, 0f, 90f), new Vector3(0.032f, 0.58f, 0.032f), gunmetal);

            GameObject brushL = new GameObject("RotarySweeperLeft");
            brushL.transform.SetParent(cockpit.transform, false);
            brushL.transform.localPosition = new Vector3(-0.36f, -0.50f, 1.28f);
            spinningPartA = brushL.transform;
            AddViewPart(PrimitiveType.Cylinder, "DrumL", brushL.transform, Vector3.zero, Quaternion.identity, new Vector3(0.38f, 0.06f, 0.38f), brushYellow);
            AddViewPart(PrimitiveType.Cube, "CrossL", brushL.transform, new Vector3(0f, 0.04f, 0f), Quaternion.identity, new Vector3(0.34f, 0.02f, 0.05f), gunmetal);

            GameObject brushR = new GameObject("RotarySweeperRight");
            brushR.transform.SetParent(cockpit.transform, false);
            brushR.transform.localPosition = new Vector3(0.36f, -0.50f, 1.28f);
            spinningPartB = brushR.transform;
            AddViewPart(PrimitiveType.Cylinder, "DrumR", brushR.transform, Vector3.zero, Quaternion.identity, new Vector3(0.38f, 0.06f, 0.38f), brushYellow);
            AddViewPart(PrimitiveType.Cube, "CrossR", brushR.transform, new Vector3(0f, 0.04f, 0f), Quaternion.identity, new Vector3(0.34f, 0.02f, 0.05f), gunmetal);

            for (int side = -1; side <= 1; side += 2)
            {
                GameObject hl = new GameObject($"BuggyHeadlight_{side}");
                hl.transform.SetParent(cockpit.transform, false);
                hl.transform.localPosition = new Vector3(side * 0.40f, -0.36f, 1.15f);
                hl.transform.localRotation = Quaternion.Euler(12f, 0f, 0f);
                Light spot = hl.AddComponent<Light>();
                spot.type = LightType.Spot;
                spot.spotAngle = 58f;
                spot.range = 28f;
                spot.intensity = 2.5f;
                spot.color = new Color(0.92f, 0.98f, 1.0f);
            }

            GameObject intake = new GameObject("BuggyIntakeCenter");
            intake.transform.SetParent(cockpit.transform, false);
            intake.transform.localPosition = new Vector3(0f, -0.48f, 1.25f);
            nozzlePoint = intake.transform;
        }

        private static GameObject AddViewPart(PrimitiveType type, string name, Transform parent, Vector3 pos, Quaternion rot, Vector3 scale, Material mat)
        {
            GameObject go = GameObject.CreatePrimitive(type);
            go.name = name;
            Collider c = go.GetComponent<Collider>();
            if (c != null) Destroy(c);

            go.transform.SetParent(parent, false);
            go.transform.localPosition = pos;
            go.transform.localRotation = rot;
            go.transform.localScale = scale;

            Renderer r = go.GetComponent<Renderer>();
            if (r != null)
            {
                r.sharedMaterial = mat;
                r.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
                r.receiveShadows = false;
            }
            return go;
        }

        private void AnimateViewmodel()
        {
            if (toolHolder == null) return;

            float mouseX = Input.GetAxisRaw("Mouse X");
            float mouseY = Input.GetAxisRaw("Mouse Y");

            Vector3 targetSway = new Vector3(
                Mathf.Clamp(-mouseX * 0.008f, -0.04f, 0.04f),
                Mathf.Clamp(-mouseY * 0.008f, -0.04f, 0.04f),
                0f
            );
            swayOffset = Vector3.Lerp(swayOffset, targetSway, Time.deltaTime * 12f);

            recoilImpulse = Mathf.MoveTowards(recoilImpulse, 0f, Time.deltaTime * 6.5f);

            Vector3 animPos = swayOffset;
            Quaternion animRot = Quaternion.identity;

            ToolData tool = CurrentTool;
            if (tool != null)
            {
                if (tool.tier == 1 && (isActivelyCleaning || recoilImpulse > 0.01f))
                {
                    scrubAnimPhase += Time.deltaTime * 28f;
                    float scrubZ = Mathf.Sin(scrubAnimPhase) * 0.048f;
                    float scrubX = Mathf.Cos(scrubAnimPhase * 0.5f) * 0.018f;
                    animPos += new Vector3(scrubX, -0.01f, scrubZ);
                    animRot = Quaternion.Euler(Mathf.Sin(scrubAnimPhase) * 8f, Mathf.Cos(scrubAnimPhase) * 6f, 0f);
                }
                else if (tool.tier == 2 && (isActivelyCleaning || recoilImpulse > 0.01f))
                {
                    scrubAnimPhase += Time.deltaTime * 18f;
                    float sweepArc = Mathf.Sin(scrubAnimPhase);
                    animPos += new Vector3(sweepArc * 0.085f, -Mathf.Abs(sweepArc) * 0.02f, 0.03f);
                    animRot = Quaternion.Euler(6f, sweepArc * 18f, -sweepArc * 10f);
                }
                else if (tool.tier >= 3 && tool.tier <= 4 && isVacuuming)
                {
                    float vibX = Mathf.Sin(Time.time * 55f) * 0.0035f;
                    float vibY = Mathf.Cos(Time.time * 63f) * 0.0035f;
                    animPos += new Vector3(vibX, vibY, -0.018f);
                }
            }

            toolHolder.localPosition = Vector3.Lerp(toolHolder.localPosition, animPos, Time.deltaTime * 18f);
            toolHolder.localRotation = Quaternion.Slerp(toolHolder.localRotation, animRot, Time.deltaTime * 18f);

            float spinSpeed = isVacuuming ? 920f : 160f;
            if (tool != null && tool.tier == 5)
            {
                float roverSpin = isVacuuming ? 540f : 180f;
                if (spinningPartA != null) spinningPartA.Rotate(0f, roverSpin * Time.deltaTime, 0f, Space.Self);
                if (spinningPartB != null) spinningPartB.Rotate(0f, -roverSpin * Time.deltaTime, 0f, Space.Self);
            }
            else if (spinningPartA != null)
            {
                spinningPartA.Rotate(0f, 0f, spinSpeed * Time.deltaTime, Space.Self);
            }
        }
    }
}
