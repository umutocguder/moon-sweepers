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
        [Header("References")]
        [SerializeField] private Camera playerCamera;
        [SerializeField] private PlayerInventory inventory;
        [SerializeField] private Transform toolHolder;

        [Header("Available Tools (Progression)")]
        [SerializeField] private List<ToolData> availableTools = new List<ToolData>();
        [SerializeField] private int activeToolIndex = 0;

        private float nextUseTime = 0f;
        private bool isVacuuming = false;
        private GameObject activeToolVisual;

        public ToolData CurrentTool => (availableTools != null && activeToolIndex >= 0 && activeToolIndex < availableTools.Count)
            ? availableTools[activeToolIndex]
            : null;

        public event Action<ToolData> OnToolChanged;

        private void Awake()
        {
            if (inventory == null)
            {
                inventory = GetComponentInParent<PlayerInventory>();
            }

            if (playerCamera == null)
            {
                playerCamera = Camera.main;
            }

            InitializeDefaultTools();
        }

        private void Start()
        {
            EquipTool(activeToolIndex);
        }

        private void InitializeDefaultTools()
        {
            if (availableTools.Count == 0)
            {
                // Tier 1: Toothbrush (Starter absurd tool)
                availableTools.Add(new ToolData(
                    "Standard Toothbrush", 1, ToolMode.ClickScrub,
                    power: 1.0f, range: 2.2f, radius: 0.6f, cooldown: 0.25f, cost: 0,
                    color: new Color(0.2f, 0.7f, 1f)));

                // Tier 2: Lunar Hand Broom
                availableTools.Add(new ToolData(
                    "Lunar Hand Broom", 2, ToolMode.ClickScrub,
                    power: 3.5f, range: 3.0f, radius: 1.2f, cooldown: 0.35f, cost: 120,
                    color: new Color(0.9f, 0.7f, 0.2f)));

                // Tier 3: Cyclone Vacuum Mk-I
                availableTools.Add(new ToolData(
                    "Cyclone Vacuum Mk-I", 3, ToolMode.ContinuousHold,
                    power: 12.0f, range: 4.5f, radius: 1.8f, cooldown: 0.05f, cost: 550,
                    color: new Color(1f, 0.3f, 0.2f)));

                // Tier 4: Electromagnetic Sweeper
                availableTools.Add(new ToolData(
                    "Electromagnetic Sweeper", 4, ToolMode.ContinuousHold,
                    power: 30.0f, range: 6.0f, radius: 3.0f, cooldown: 0.05f, cost: 1800,
                    color: new Color(0.6f, 0.2f, 1f)));

                // Tier 5: Lunar Buggy Sweeper
                availableTools.Add(new ToolData(
                    "Lunar Sweeper Buggy", 5, ToolMode.ContinuousHold,
                    power: 100.0f, range: 8.0f, radius: 5.0f, cooldown: 0.05f, cost: 5000,
                    color: new Color(0.1f, 0.9f, 0.5f)));
            }
        }

        private void Update()
        {
            if (GameManager.Instance != null && (GameManager.Instance.IsGamePaused || GameManager.Instance.IsShopOpen))
            {
                if (isVacuuming) StopVacuum();
                return;
            }

            // Quick Tool Switching via 1-5 keys
            for (int i = 0; i < availableTools.Count; i++)
            {
                if (Input.GetKeyDown(KeyCode.Alpha1 + i))
                {
                    EquipTool(i);
                }
            }

            HandleToolUsage();
            AnimateToolSway();
        }

        private void HandleToolUsage()
        {
            ToolData current = CurrentTool;
            if (current == null) return;

            if (current.mode == ToolMode.ClickScrub)
            {
                if (Input.GetButtonDown("Fire1") && Time.time >= nextUseTime)
                {
                    nextUseTime = Time.time + current.useCooldownSeconds;
                    PerformScrub(current);
                }
            }
            else if (current.mode == ToolMode.ContinuousHold)
            {
                if (Input.GetButton("Fire1"))
                {
                    if (!isVacuuming) StartVacuum();

                    if (Time.time >= nextUseTime)
                    {
                        nextUseTime = Time.time + current.useCooldownSeconds;
                        PerformContinuousClean(current);
                    }
                }
                else
                {
                    if (isVacuuming) StopVacuum();
                }
            }
        }

        private void PerformScrub(ToolData tool)
        {
            if (inventory.IsFull) return;

            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlayBrushScrape();
            }

            TriggerToolSwayImpulse();
            CleanInTargetZone(tool.sweepRangeMeters, tool.sweepRadiusMeters, tool.sweepPowerKg);
        }

        private void PerformContinuousClean(ToolData tool)
        {
            if (inventory.IsFull)
            {
                StopVacuum();
                return;
            }

            float frameCleanPower = tool.sweepPowerKg * tool.useCooldownSeconds;
            CleanInTargetZone(tool.sweepRangeMeters, tool.sweepRadiusMeters, frameCleanPower);
        }

        private void CleanInTargetZone(float range, float radius, float amountKg)
        {
            Ray ray = playerCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));
            if (Physics.Raycast(ray, out RaycastHit hit, range))
            {
                // Search for dust patches around hit point
                Collider[] hits = Physics.OverlapSphere(hit.point, radius);
                foreach (Collider col in hits)
                {
                    RegolithDustPatch patch = col.GetComponent<RegolithDustPatch>();
                    if (patch != null && !patch.IsFullyCleaned)
                    {
                        float gathered = patch.CleanDust(amountKg);
                        inventory.AddDust(gathered);

                        if (inventory.IsFull) break;
                    }
                }
            }
        }

        private void StartVacuum()
        {
            isVacuuming = true;
            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.SetVacuumActive(true);
            }
        }

        private void StopVacuum()
        {
            isVacuuming = false;
            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.SetVacuumActive(false);
            }
        }

        public void EquipTool(int index)
        {
            if (index < 0 || index >= availableTools.Count) return;

            if (isVacuuming) StopVacuum();

            activeToolIndex = index;
            CreateProceduralToolVisual(availableTools[activeToolIndex]);
            OnToolChanged?.Invoke(availableTools[activeToolIndex]);
        }

        private void CreateProceduralToolVisual(ToolData tool)
        {
            if (toolHolder == null) return;

            if (activeToolVisual != null)
            {
                Destroy(activeToolVisual);
            }

            // Procedural tool representation (Brush head, vacuum nozzle, etc.)
            activeToolVisual = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            activeToolVisual.transform.SetParent(toolHolder, false);
            activeToolVisual.transform.localPosition = new Vector3(0.25f, -0.2f, 0.45f);
            activeToolVisual.transform.localRotation = Quaternion.Euler(60f, 15f, -20f);
            activeToolVisual.transform.localScale = new Vector3(0.04f, 0.15f, 0.04f);

            // Remove primitive collider so it doesn't collide with player
            Collider c = activeToolVisual.GetComponent<Collider>();
            if (c != null) Destroy(c);

            Renderer r = activeToolVisual.GetComponent<Renderer>();
            if (r != null)
            {
                r.material = new Material(Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard"));
                r.material.color = tool.toolVisualColor;
            }
        }

        private void AnimateToolSway()
        {
            if (toolHolder == null) return;

            float moveX = Input.GetAxis("Mouse X") * 0.015f;
            float moveY = Input.GetAxis("Mouse Y") * 0.015f;

            Vector3 targetPos = new Vector3(-moveX, -moveY, 0f);
            toolHolder.localPosition = Vector3.Lerp(toolHolder.localPosition, targetPos, Time.deltaTime * 8f);
        }

        private void TriggerToolSwayImpulse()
        {
            if (toolHolder != null)
            {
                toolHolder.localPosition += new Vector3(0.02f, -0.04f, 0.05f);
            }
        }
    }
}
