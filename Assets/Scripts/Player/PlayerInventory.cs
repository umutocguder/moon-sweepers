using System;
using System.Collections.Generic;
using UnityEngine;

namespace ElevenHopem.MoonSweepers.Player
{
    [Serializable]
    public class MineralItem
    {
        public string mineralName;
        public int count;
        public float valueMultiplier;

        public MineralItem(string name, int count, float multiplier)
        {
            this.mineralName = name;
            this.count = count;
            this.valueMultiplier = multiplier;
        }
    }

    public class PlayerInventory : MonoBehaviour
    {
        [Header("Regolith Dust Capacity")]
        [SerializeField] private float currentDustKg = 0f;
        [SerializeField] private float maxBackpackCapacityKg = 30f;

        [Header("Rare Minerals")]
        [SerializeField] private List<MineralItem> collectedMinerals = new List<MineralItem>();

        public float CurrentDustKg => currentDustKg;
        public float MaxBackpackCapacityKg => maxBackpackCapacityKg;
        public bool IsFull => currentDustKg >= maxBackpackCapacityKg;
        public float FillRatio => Mathf.Clamp01(currentDustKg / maxBackpackCapacityKg);

        public event Action<float, float> OnInventoryUpdated;
        public event Action<string, int> OnMineralCollected;

        private void Start()
        {
            OnInventoryUpdated?.Invoke(currentDustKg, maxBackpackCapacityKg);
        }

        public bool AddDust(float amountKg)
        {
            if (amountKg <= 0 || IsFull) return false;

            float availableSpace = maxBackpackCapacityKg - currentDustKg;
            float added = Mathf.Min(amountKg, availableSpace);

            currentDustKg += added;
            OnInventoryUpdated?.Invoke(currentDustKg, maxBackpackCapacityKg);

            return added > 0;
        }

        public void AddMineral(string name, int quantity, float multiplier)
        {
            var existing = collectedMinerals.Find(m => m.mineralName == name);
            if (existing != null)
            {
                existing.count += quantity;
            }
            else
            {
                collectedMinerals.Add(new MineralItem(name, quantity, multiplier));
            }

            OnMineralCollected?.Invoke(name, quantity);
        }

        public float UnloadAllDust()
        {
            float unloaded = currentDustKg;
            currentDustKg = 0f;
            OnInventoryUpdated?.Invoke(currentDustKg, maxBackpackCapacityKg);
            return unloaded;
        }

        public List<MineralItem> UnloadAllMinerals()
        {
            var minerals = new List<MineralItem>(collectedMinerals);
            collectedMinerals.Clear();
            return minerals;
        }

        public void UpgradeCapacity(float addedKg)
        {
            maxBackpackCapacityKg += addedKg;
            OnInventoryUpdated?.Invoke(currentDustKg, maxBackpackCapacityKg);
        }
    }
}
