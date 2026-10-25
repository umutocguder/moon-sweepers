using System;
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
        public string title;
        public string description;
        public long priceCredits;
        public bool isPurchased;
        public int tierRequirement;
    }

    public class ShopManager : MonoBehaviour
    {
        public static ShopManager Instance { get; private set; }

        [Header("Player References")]
        [SerializeField] private PlayerInventory playerInventory;
        [SerializeField] private PlayerController playerController;
        [SerializeField] private ToolController toolController;

        [Header("Upgrades Available")]
        [SerializeField] private ShopUpgradeItem[] upgrades;

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
                new ShopUpgradeItem { id = "tool_broom", title = "Lunar Hand Broom", description = "Tier 2 tool. Sweeps 3.5 kg/strike with 1.2m radius arc.", priceCredits = 120, isPurchased = false, tierRequirement = 1 },
                new ShopUpgradeItem { id = "tool_vacuum", title = "Cyclone Vacuum Mk-I", description = "Tier 3 tool. Continuous suction at 12 kg/sec.", priceCredits = 550, isPurchased = false, tierRequirement = 2 },
                new ShopUpgradeItem { id = "tool_electro", title = "Electromagnetic Sweeper", description = "Tier 4 tool. Magnetic field pulls 30 kg/sec.", priceCredits = 1800, isPurchased = false, tierRequirement = 3 },
                new ShopUpgradeItem { id = "tool_buggy", title = "Lunar Sweeper Buggy", description = "Tier 5 heavy vehicle sweeper. Devours 100 kg/sec.", priceCredits = 5000, isPurchased = false, tierRequirement = 4 },
                new ShopUpgradeItem { id = "bp_1", title = "Reinforced Lunar Backpack", description = "Expands backpack capacity by +30 kg.", priceCredits = 150, isPurchased = false, tierRequirement = 1 },
                new ShopUpgradeItem { id = "bp_2", title = "Titanium Heavy Hauler Sack", description = "Expands backpack capacity by +100 kg.", priceCredits = 650, isPurchased = false, tierRequirement = 2 },
                new ShopUpgradeItem { id = "fuel_1", title = "Auxiliary Jetpack Fuel Cells", description = "Adds +50 maximum jetpack thruster fuel.", priceCredits = 250, isPurchased = false, tierRequirement = 1 },
                new ShopUpgradeItem { id = "conveyor", title = "Regolith Conveyor Line", description = "Directs nearby dust directly into base hopper.", priceCredits = 900, isPurchased = false, tierRequirement = 2 },
                new ShopUpgradeItem { id = "drone", title = "Autonomous Cleaner Drone", description = "AI drone sweeps lunar surface automatically.", priceCredits = 2500, isPurchased = false, tierRequirement = 3 }
            };
        }

        public ShopUpgradeItem[] GetAllUpgrades() => upgrades;

        public bool PurchaseUpgrade(string upgradeId)
        {
            ShopUpgradeItem item = Array.Find(upgrades, u => u.id == upgradeId);
            if (item == null || item.isPurchased) return false;

            if (GameManager.Instance == null || !GameManager.Instance.TrySpendCredits(item.priceCredits))
            {
                Debug.LogWarning("[ShopManager] Insufficient funds!");
                return false;
            }

            item.isPurchased = true;
            ApplyUpgradeEffects(item);

            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlayUpgradeFanfare();
            }

            OnUpgradePurchased?.Invoke(item);
            Debug.Log($"[ShopManager] Purchased {item.title}!");
            return true;
        }

        private void ApplyUpgradeEffects(ShopUpgradeItem item)
        {
            if (playerInventory == null) playerInventory = FindFirstObjectByType<PlayerInventory>();
            if (playerController == null) playerController = FindFirstObjectByType<PlayerController>();
            if (toolController == null) toolController = FindFirstObjectByType<ToolController>();

            switch (item.id)
            {
                case "tool_broom":
                    toolController?.EquipTool(1);
                    break;
                case "tool_vacuum":
                    toolController?.EquipTool(2);
                    break;
                case "tool_electro":
                    toolController?.EquipTool(3);
                    break;
                case "tool_buggy":
                    toolController?.EquipTool(4);
                    break;
                case "bp_1":
                    playerInventory?.UpgradeCapacity(30f);
                    break;
                case "bp_2":
                    playerInventory?.UpgradeCapacity(100f);
                    break;
                case "fuel_1":
                    playerController?.UpgradeFuelCapacity(50f);
                    break;
                case "conveyor":
                case "drone":
                    SpawnAutomationStation(item.id);
                    break;
            }
        }

        private void SpawnAutomationStation(string type)
        {
            GameObject stationObj = new GameObject($"Automation_{type}");
            stationObj.transform.position = new Vector3(UnityEngine.Random.Range(-10f, 10f), 0.5f, UnityEngine.Random.Range(5f, 20f));
            AutomationStation station = stationObj.AddComponent<AutomationStation>();
            station.stationType = type;
        }
    }
}
