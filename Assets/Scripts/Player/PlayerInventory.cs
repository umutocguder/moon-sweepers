/*
 * Created by 11Hopem
 * Copyright (c) 2026 11Hopem. All rights reserved.
 */

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
        [SerializeField] private float currentDustKg = 0f;
        [SerializeField] private float maxBackpackCapacityKg = 30f;
        [SerializeField] private List<MineralItem> collectedMinerals = new List<MineralItem>();

        public float CurrentDustKg => currentDustKg;
        public float MaxBackpackCapacityKg => maxBackpackCapacityKg;
        public bool IsFull => currentDustKg >= maxBackpackCapacityKg - 0.01f;
        public float FillRatio => maxBackpackCapacityKg > 0f ? Mathf.Clamp01(currentDustKg / maxBackpackCapacityKg) : 0f;
        public IReadOnlyList<MineralItem> CollectedMinerals => collectedMinerals;

        public int TotalMineralCount
        {
            get
            {
                int sum = 0;
                for (int i = 0; i < collectedMinerals.Count; i++)
                {
                    sum += collectedMinerals[i].count;
                }
                return sum;
            }
        }

        public event Action<float, float> OnInventoryUpdated;
        public event Action<string, int> OnMineralCollected;

        private void Start()
        {
            OnInventoryUpdated?.Invoke(currentDustKg, maxBackpackCapacityKg);
        }

        public bool AddDust(float amountKg)
        {
            if (amountKg <= 0f || IsFull) return false;

            float availableSpace = maxBackpackCapacityKg - currentDustKg;
            float added = Mathf.Min(amountKg, availableSpace);

            currentDustKg += added;
            OnInventoryUpdated?.Invoke(currentDustKg, maxBackpackCapacityKg);
            return added > 0f;
        }

        public void AddMineral(string mineralName, int quantity, float multiplier)
        {
            var existing = collectedMinerals.Find(m => m.mineralName == mineralName);
            if (existing != null)
            {
                existing.count += quantity;
            }
            else
            {
                collectedMinerals.Add(new MineralItem(mineralName, quantity, multiplier));
            }

            OnMineralCollected?.Invoke(mineralName, quantity);
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

        public void SetMaxCapacity(float capacityKg)
        {
            maxBackpackCapacityKg = Mathf.Max(30f, capacityKg);
            OnInventoryUpdated?.Invoke(currentDustKg, maxBackpackCapacityKg);
        }
    }
}
