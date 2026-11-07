/*
 * Created by 11Hopem
 * Copyright (c) 2026 11Hopem. All rights reserved.
 */

using UnityEngine;
using ElevenHopem.MoonSweepers.Core;

namespace ElevenHopem.MoonSweepers.Economy
{
    public enum ContractObjectiveType
    {
        CleanRegolithKg,
        ExtractMinerals,
        DepositRegolithKg,
        HarvestMeteorite
    }

    public class ContractManager : MonoBehaviour
    {
        public static ContractManager Instance { get; private set; }

        private static readonly string[] RankTitles = new string[]
        {
            "LSD Cadet Sweeper",
            "Crater Technician",
            "Regolith Specialist",
            "Isotope Engineer",
            "Lunar Fleet Commander",
            "Director of Tranquility"
        };

        private int contractIndex = 0;
        private float currentProgress = 0f;
        private float targetAmount = 120f;
        private long rewardCredits = 220;
        private string contractTitle = "";
        private ContractObjectiveType objectiveType = ContractObjectiveType.CleanRegolithKg;

        public int CurrentContractIndex => contractIndex;
        public float CurrentProgress => currentProgress;
        public float TargetAmount => targetAmount;
        public long RewardCredits => rewardCredits;
        public string ContractTitle => contractTitle;
        public float FillRatio => targetAmount > 0f ? Mathf.Clamp01(currentProgress / targetAmount) : 0f;

        public string CurrentRankTitle
        {
            get
            {
                int rIdx = Mathf.Clamp(contractIndex / 2, 0, RankTitles.Length - 1);
                return RankTitles[rIdx];
            }
        }

        public string ProgressStatusText
        {
            get
            {
                if (objectiveType == ContractObjectiveType.ExtractMinerals || objectiveType == ContractObjectiveType.HarvestMeteorite)
                {
                    return $"{Mathf.FloorToInt(currentProgress)} / {Mathf.RoundToInt(targetAmount)}   (+LC {rewardCredits:N0})";
                }
                return $"{currentProgress:F0} / {targetAmount:F0} kg   (+LC {rewardCredits:N0})";
            }
        }

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            SetupContractForIndex(0);
        }

        private void Start()
        {
            if (GameManager.Instance != null)
            {
                GameManager.Instance.OnDustCleanedGlobal += HandleDustCleaned;
                GameManager.Instance.OnDustDepositedGlobal += HandleDustDeposited;
                GameManager.Instance.OnMineralExtractedGlobal += HandleMineralExtracted;
                GameManager.Instance.OnMeteorCoreHarvested += HandleMeteorHarvested;
            }
        }

        private void OnDestroy()
        {
            if (GameManager.Instance != null)
            {
                GameManager.Instance.OnDustCleanedGlobal -= HandleDustCleaned;
                GameManager.Instance.OnDustDepositedGlobal -= HandleDustDeposited;
                GameManager.Instance.OnMineralExtractedGlobal -= HandleMineralExtracted;
                GameManager.Instance.OnMeteorCoreHarvested -= HandleMeteorHarvested;
            }
        }

        public void RestoreContractState(int savedIndex, float savedProgress)
        {
            contractIndex = Mathf.Max(0, savedIndex);
            SetupContractForIndex(contractIndex);
            currentProgress = Mathf.Clamp(savedProgress, 0f, targetAmount * 0.99f);
        }

        private void SetupContractForIndex(int idx)
        {
            int tierScale = 1 + idx / 3;
            int mod = idx % 4;

            if (mod == 0)
            {
                objectiveType = ContractObjectiveType.CleanRegolithKg;
                targetAmount = 140f * tierScale + idx * 85f;
                rewardCredits = 240 + idx * 320;
                contractTitle = $"OP #{idx + 1}: Sweep {targetAmount:F0} kg Lunar Regolith";
            }
            else if (mod == 1)
            {
                objectiveType = ContractObjectiveType.ExtractMinerals;
                targetAmount = 2 + idx;
                rewardCredits = 380 + idx * 360;
                contractTitle = $"OP #{idx + 1}: Extract {targetAmount:F0} Rare Mineral Cores";
            }
            else if (mod == 2)
            {
                objectiveType = ContractObjectiveType.DepositRegolithKg;
                targetAmount = 220f * tierScale + idx * 110f;
                rewardCredits = 520 + idx * 420;
                contractTitle = $"OP #{idx + 1}: Process {targetAmount:F0} kg at Depot";
            }
            else
            {
                objectiveType = ContractObjectiveType.HarvestMeteorite;
                targetAmount = 1 + idx / 4;
                rewardCredits = 850 + idx * 500;
                contractTitle = $"OP #{idx + 1}: Harvest {targetAmount:F0} Fallen Meteorite";
            }
        }

        private void HandleDustCleaned(float kg)
        {
            if (objectiveType == ContractObjectiveType.CleanRegolithKg)
            {
                AdvanceProgress(kg);
            }
        }

        private void HandleDustDeposited(float kg)
        {
            if (objectiveType == ContractObjectiveType.DepositRegolithKg)
            {
                AdvanceProgress(kg);
            }
        }

        private void HandleMineralExtracted(int count)
        {
            if (objectiveType == ContractObjectiveType.ExtractMinerals)
            {
                AdvanceProgress(count);
            }
        }

        private void HandleMeteorHarvested()
        {
            if (objectiveType == ContractObjectiveType.HarvestMeteorite)
            {
                AdvanceProgress(1f);
            }
        }

        private void AdvanceProgress(float delta)
        {
            if (delta <= 0f) return;
            currentProgress += delta;
            if (currentProgress >= targetAmount)
            {
                CompleteCurrentContract();
            }
        }

        private void CompleteCurrentContract()
        {
            long payout = rewardCredits;
            int completedNum = contractIndex + 1;
            contractIndex++;
            currentProgress = 0f;
            SetupContractForIndex(contractIndex);

            AudioManager.Instance?.PlayContractComplete();
            GameManager.Instance?.NotifyToast($"LSD CONTRACT #{completedNum} COMPLETE! Bonus +LC {payout:N0} Awarded!");
            GameManager.Instance?.AddCredits(payout);
        }
    }
}
