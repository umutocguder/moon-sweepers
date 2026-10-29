/*
 * Created by 11Hopem
 * Copyright (c) 2026 11Hopem. All rights reserved.
 */

using System;
using System.Collections.Generic;
using UnityEngine;
using ElevenHopem.MoonSweepers.Core;
using ElevenHopem.MoonSweepers.Player;
using ElevenHopem.MoonSweepers.Tools;

namespace ElevenHopem.MoonSweepers.Economy
{
    [Serializable]
    public class ShopUpgradeItem
    {
        public string id;
        public string category;
        public string title;
        public string description;
        public long priceCredits;
        public bool isPurchased;
        public bool repeatable;
        public int purchaseCount;
        public int maxPurchases;
    }

    public class ShopManager : MonoBehaviour
    {
        public static ShopManager Instance { get; private set; }

        [SerializeField] private PlayerInventory playerInventory;
        [SerializeField] private PlayerController playerController;
        [SerializeField] private ToolController toolController;
        [SerializeField] private ShopUpgradeItem[] upgrades;

        private int conveyorCount = 0;
        private int droneCount = 0;

        public int ConveyorCount => conveyorCount;
        public int DroneCount => droneCount;

        public event Action<ShopUpgradeItem> OnUpgradePurchased;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            InitializeUpgrades();
        }

        private void InitializeUpgrades()
        {
            upgrades = new ShopUpgradeItem[]
            {
                new ShopUpgradeItem
                {
                    id = "tool_broom",
                    category = "SWEEPING TOOL • TIER 2",
                    title = "Lunar Hand Broom",
                    description = "Wide carbon-fiber push broom. Sweeps 5.5 kg/stroke across a 1.85m arc.",
                    priceCredits = 110,
                    isPurchased = false,
                    repeatable = false,
                    maxPurchases = 1
                },
                new ShopUpgradeItem
                {
                    id = "tool_vacuum",
                    category = "SWEEPING TOOL • TIER 3",
                    title = "Cyclone Vacuum Mk-I",
                    description = "Continuous turbine suction gun pulling 24 kg/sec from 7.8m range.",
                    priceCredits = 420,
                    isPurchased = false,
                    repeatable = false,
                    maxPurchases = 1
                },
                new ShopUpgradeItem
                {
                    id = "tool_electro",
                    category = "SWEEPING TOOL • TIER 4",
                    title = "Electromagnetic Sweeper",
                    description = "Dual-rail plasma field magnetizes 58 kg/sec within a 3.8m radius.",
                    priceCredits = 1350,
                    isPurchased = false,
                    repeatable = false,
                    maxPurchases = 1
                },
                new ShopUpgradeItem
                {
                    id = "tool_buggy",
                    category = "HEAVY VEHICLE • TIER 5",
                    title = "Lunar Sweeper Buggy",
                    description = "Armored rover cockpit with twin rotary sweepers (145 kg/sec & +65% speed).",
                    priceCredits = 3800,
                    isPurchased = false,
                    repeatable = false,
                    maxPurchases = 1
                },
                new ShopUpgradeItem
                {
                    id = "bp_1",
                    category = "SUIT RIG • CAPACITY",
                    title = "Reinforced Regolith Silo Pack",
                    description = "Expands astronaut backpack capacity by +45 kg per module.",
                    priceCredits = 125,
                    isPurchased = false,
                    repeatable = true,
                    maxPurchases = 5
                },
                new ShopUpgradeItem
                {
                    id = "fuel_1",
                    category = "THRUSTER • MOBILITY",
                    title = "Cryo-Hydrazine Jetpack Cells",
                    description = "Adds +60 maximum thruster fuel for extended low-gravity flight.",
                    priceCredits = 190,
                    isPurchased = false,
                    repeatable = true,
                    maxPurchases = 4
                },
                new ShopUpgradeItem
                {
                    id = "conveyor",
                    category = "AUTOMATION • BASE INFRASTRUCTURE",
                    title = "Regolith Conveyor Belt Line",
                    description = "Automated surface intake conveyor transferring nearby dust straight into LC.",
                    priceCredits = 650,
                    isPurchased = false,
                    repeatable = true,
                    maxPurchases = 3
                },
                new ShopUpgradeItem
                {
                    id = "drone",
                    category = "AUTOMATION • AUTONOMOUS FLEET",
                    title = "Quad-Rotor Lunar Sweeper Drone",
                    description = "AI patrol drone orbits the crater basin and vacuums regolith into LC.",
                    priceCredits = 1600,
                    isPurchased = false,
                    repeatable = true,
                    maxPurchases = 4
                }
            };
        }

        public ShopUpgradeItem[] GetAllUpgrades() => upgrades;

        public bool CanPurchaseAnyUpgrade(long currentCredits)
        {
            if (upgrades == null) return false;
            for (int i = 0; i < upgrades.Length; i++)
            {
                if (!upgrades[i].isPurchased && currentCredits >= upgrades[i].priceCredits)
                {
                    return true;
                }
            }
            return false;
        }

        public bool PurchaseUpgrade(string upgradeId)
        {
            ShopUpgradeItem item = Array.Find(upgrades, u => u.id == upgradeId);
            if (item == null || item.isPurchased) return false;

            if (GameManager.Instance == null || !GameManager.Instance.TrySpendCredits(item.priceCredits))
            {
                return false;
            }

            item.purchaseCount++;
            if (!item.repeatable || item.purchaseCount >= item.maxPurchases)
            {
                item.isPurchased = true;
            }
            else
            {
                item.priceCredits = (long)Mathf.Round(item.priceCredits * 1.65f);
            }

            ApplyUpgradeEffects(item.id);
            AudioManager.Instance?.PlayUpgradeFanfare();
            GameManager.Instance?.NotifyToast($"Unlocked: {item.title}!");
            GameManager.Instance?.SaveCurrentProgress();
            OnUpgradePurchased?.Invoke(item);
            return true;
        }

        private void ApplyUpgradeEffects(string id)
        {
            if (playerInventory == null) playerInventory = FindFirstObjectByType<PlayerInventory>();
            if (playerController == null) playerController = FindFirstObjectByType<PlayerController>();
            if (toolController == null) toolController = FindFirstObjectByType<ToolController>();

            switch (id)
            {
                case "tool_broom":
                    toolController?.UnlockAndEquipTool(1);
                    break;
                case "tool_vacuum":
                    toolController?.UnlockAndEquipTool(2);
                    break;
                case "tool_electro":
                    toolController?.UnlockAndEquipTool(3);
                    break;
                case "tool_buggy":
                    toolController?.UnlockAndEquipTool(4);
                    break;
                case "bp_1":
                    playerInventory?.UpgradeCapacity(45f);
                    break;
                case "fuel_1":
                    playerController?.UpgradeFuelCapacity(60f);
                    break;
                case "conveyor":
                    conveyorCount++;
                    SpawnAutomationStation("conveyor", conveyorCount - 1);
                    break;
                case "drone":
                    droneCount++;
                    SpawnAutomationStation("drone", droneCount - 1);
                    break;
            }
        }

        public void RestorePurchasedState(string[] ownedIds, int savedConveyors, int savedDrones)
        {
            HashSet<string> set = new HashSet<string>(ownedIds ?? Array.Empty<string>());
            for (int i = 0; i < upgrades.Length; i++)
            {
                if (set.Contains(upgrades[i].id))
                {
                    upgrades[i].isPurchased = true;
                }
            }

            for (int c = 0; c < savedConveyors; c++)
            {
                conveyorCount++;
                SpawnAutomationStation("conveyor", c);
            }

            for (int d = 0; d < savedDrones; d++)
            {
                droneCount++;
                SpawnAutomationStation("drone", d);
            }
        }

        private void SpawnAutomationStation(string type, int index)
        {
            GameObject stationObj = new GameObject($"Automation_{type}_{index}");
            if (type == "conveyor")
            {
                float angleDeg = -55f + index * 55f;
                float rad = angleDeg * Mathf.Deg2Rad;
                Vector3 dir = new Vector3(Mathf.Sin(rad), 0f, Mathf.Cos(rad));
                stationObj.transform.position = new Vector3(0f, 0.05f, 8.0f) + dir * 7.2f;
                stationObj.transform.rotation = Quaternion.LookRotation(dir, Vector3.up);
            }
            else
            {
                stationObj.transform.position = new Vector3(0f, 0f, 8.0f);
            }

            AutomationStation station = stationObj.AddComponent<AutomationStation>();
            station.stationType = type;
            station.unitIndex = index;
        }
    }
}
