/*
 * Created by 11Hopem
 * Copyright (c) 2026 11Hopem. All rights reserved.
 */

using System;
using System.Collections.Generic;
using UnityEngine;
using ElevenHopem.MoonSweepers.Player;
using ElevenHopem.MoonSweepers.Tools;
using ElevenHopem.MoonSweepers.Economy;

namespace ElevenHopem.MoonSweepers.Core
{
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }

        [SerializeField] private float lunarGravity = -1.62f;
        [SerializeField] private long lunarCredits = 0;
        [SerializeField] private float baseDustValuePerKg = 3.5f;
        [SerializeField] private bool isGamePaused = false;
        [SerializeField] private bool isShopOpen = false;

        private float totalDustCleanedKg = 0f;
        private int totalPatches = 0;
        private int cleanedPatches = 0;
        private float overchargeHeat = 0f;
        private float satelliteValueBonus = 0f;
        private float monolithPowerBonus = 0f;
        private int refineryLevel = 0;
        private bool massDriverUnlocked = false;
        private float autoBeamDustAccumulator = 0f;

        public float LunarGravity => lunarGravity;
        public long LunarCredits => lunarCredits;
        public float BaseDustValuePerKg => baseDustValuePerKg * TotalValueMultiplier;
        public float TotalValueMultiplier => 1.0f + refineryLevel * 0.40f + satelliteValueBonus;
        public float OverchargeMultiplier => 1.0f + Mathf.Clamp01(overchargeHeat) * 0.75f;
        public float TotalSweepPowerMultiplier => (1.0f + monolithPowerBonus) * OverchargeMultiplier;
        public float OverchargeRatio => Mathf.Clamp01(overchargeHeat);
        public bool HasMassDriver => massDriverUnlocked;
        public int RefineryLevel => refineryLevel;
        public bool IsGamePaused => isGamePaused;
        public bool IsShopOpen => isShopOpen;
        public float TotalDustCleanedKg => totalDustCleanedKg;
        public float SectorCleanlinessPercent => totalPatches > 0 ? Mathf.Clamp01((float)cleanedPatches / totalPatches) * 100f : 0f;

        public event Action<long> OnCreditsChanged;
        public event Action<bool> OnPauseStateChanged;
        public event Action<bool> OnShopStateChanged;
        public event Action<string> OnToastNotification;
        public event Action<float> OnDustCleanedGlobal;
        public event Action<float> OnDustDepositedGlobal;
        public event Action<int> OnMineralExtractedGlobal;
        public event Action OnMeteorCoreHarvested;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            Application.targetFrameRate = 120;
            QualitySettings.vSyncCount = 1;
            QualitySettings.antiAliasing = 4;
            Physics.gravity = new Vector3(0f, lunarGravity, 0f);

            int targetW = Display.main.systemWidth;
            int targetH = Display.main.systemHeight;
            Screen.SetResolution(targetW, targetH, FullScreenMode.FullScreenWindow);
        }

        private void Start()
        {
            LockGameplayCursor();
            LoadSavedProgress();
            OnCreditsChanged?.Invoke(lunarCredits);
        }

        private void Update()
        {
            if (overchargeHeat > 0f && !isGamePaused)
            {
                overchargeHeat = Mathf.Max(0f, overchargeHeat - Time.deltaTime * 0.22f);
            }

            if (Input.GetKeyDown(KeyCode.Tab) || Input.GetKeyDown(KeyCode.B))
            {
                if (!isGamePaused)
                {
                    ToggleShop();
                }
            }

            if (Input.GetKeyDown(KeyCode.Escape))
            {
                if (isShopOpen)
                {
                    SetShopOpen(false);
                }
                else
                {
                    TogglePause();
                }
            }

            if (Input.GetKeyDown(KeyCode.F11))
            {
                ToggleFullscreen();
            }

            if (!isGamePaused && !isShopOpen && Cursor.lockState != CursorLockMode.Locked)
            {
                if (Input.GetMouseButtonDown(0) || Input.GetMouseButtonDown(1))
                {
                    LockGameplayCursor();
                }
            }
        }

        public void FeedOvercharge(float delta)
        {
            overchargeHeat = Mathf.Clamp01(overchargeHeat + delta);
        }

        public void SetRefineryLevel(int level)
        {
            refineryLevel = Mathf.Max(refineryLevel, level);
        }

        public void SetMassDriverUnlocked(bool unlocked)
        {
            massDriverUnlocked = unlocked;
        }

        public void GrantSatelliteTelemetryBonus()
        {
            satelliteValueBonus = 0.25f;
            NotifyToast("LANDMARK RESTORED: Orbital Satellite Online! Permanent +25% LC Value!");
            AudioManager.Instance?.PlayContractComplete();
        }

        public void GrantMonolithOverclockBonus()
        {
            monolithPowerBonus = 0.25f;
            NotifyToast("MONOLITH AWAKENED: Harmonic Field Active! Permanent +25% Sweep Power!");
            AudioManager.Instance?.PlayContractComplete();
        }

        public float ApplyMassDriverAutoTransfer(float gatheredKg)
        {
            if (!massDriverUnlocked || gatheredKg <= 0f) return gatheredKg;

            float beamedKg = gatheredKg * 0.40f;
            float remainingKg = gatheredKg - beamedKg;
            autoBeamDustAccumulator += beamedKg;

            if (autoBeamDustAccumulator >= 3.0f)
            {
                long earned = (long)Mathf.Max(1f, Mathf.Round(autoBeamDustAccumulator * BaseDustValuePerKg));
                OnDustDepositedGlobal?.Invoke(autoBeamDustAccumulator);
                autoBeamDustAccumulator = 0f;
                lunarCredits += earned;
                OnCreditsChanged?.Invoke(lunarCredits);
            }

            return remainingKg;
        }

        public void LockGameplayCursor()
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }

        public void ToggleFullscreen()
        {
            if (Screen.fullScreenMode == FullScreenMode.Windowed)
            {
                Resolution res = Screen.currentResolution;
                Screen.SetResolution(res.width, res.height, FullScreenMode.FullScreenWindow);
                NotifyToast("Display: Borderless Fullscreen");
            }
            else
            {
                Screen.SetResolution(1680, 1020, FullScreenMode.Windowed);
                NotifyToast("Display: Windowed (1680x1020)");
            }
        }

        public void RegisterDustPatch()
        {
            totalPatches++;
        }

        public void UnregisterDustPatch(bool wasCleaned)
        {
            totalPatches = Mathf.Max(0, totalPatches - 1);
            if (wasCleaned)
            {
                cleanedPatches = Mathf.Max(0, cleanedPatches - 1);
            }
        }

        public void ReportPatchCleanedState(bool isCleaned)
        {
            cleanedPatches = Mathf.Clamp(cleanedPatches + (isCleaned ? 1 : -1), 0, Mathf.Max(1, totalPatches));
        }

        public void RecordDustCleaned(float amountKg)
        {
            if (amountKg > 0f)
            {
                totalDustCleanedKg += amountKg;
                OnDustCleanedGlobal?.Invoke(amountKg);
            }
        }

        public void RecordDustDeposited(float amountKg)
        {
            if (amountKg > 0f)
            {
                OnDustDepositedGlobal?.Invoke(amountKg);
            }
        }

        public void RecordMineralExtracted(int count)
        {
            if (count > 0)
            {
                OnMineralExtractedGlobal?.Invoke(count);
            }
        }

        public void RecordMeteorHarvested()
        {
            OnMeteorCoreHarvested?.Invoke();
        }

        public void NotifyToast(string message)
        {
            if (!string.IsNullOrEmpty(message))
            {
                OnToastNotification?.Invoke(message);
            }
        }

        public void AddCredits(long amount)
        {
            if (amount <= 0) return;
            lunarCredits += amount;
            OnCreditsChanged?.Invoke(lunarCredits);
            SaveCurrentProgress();
        }

        public bool TrySpendCredits(long amount)
        {
            if (amount <= 0) return false;
            if (lunarCredits >= amount)
            {
                lunarCredits -= amount;
                OnCreditsChanged?.Invoke(lunarCredits);
                return true;
            }
            return false;
        }

        public void ToggleShop()
        {
            SetShopOpen(!isShopOpen);
        }

        public void SetShopOpen(bool open)
        {
            isShopOpen = open;
            if (open)
            {
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
            }
            else if (!isGamePaused)
            {
                LockGameplayCursor();
            }
            OnShopStateChanged?.Invoke(open);
        }

        public void TogglePause()
        {
            SetPaused(!isGamePaused);
        }

        public void SetPaused(bool paused)
        {
            isGamePaused = paused;
            Time.timeScale = isGamePaused ? 0f : 1f;
            if (isGamePaused)
            {
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
            }
            else if (!isShopOpen)
            {
                LockGameplayCursor();
            }
            OnPauseStateChanged?.Invoke(isGamePaused);
        }

        public void SaveCurrentProgress()
        {
            if (SaveManager.Instance == null) return;

            PlayerInventory inv = FindFirstObjectByType<PlayerInventory>();
            PlayerController pc = FindFirstObjectByType<PlayerController>();
            ToolController tc = FindFirstObjectByType<ToolController>();
            ContractManager cm = FindFirstObjectByType<ContractManager>();

            GameSaveData data = new GameSaveData
            {
                lunarCredits = lunarCredits,
                totalDustCleanedKg = totalDustCleanedKg,
                backpackCapacity = inv != null ? inv.MaxBackpackCapacityKg : 30f,
                jetpackMaxFuel = pc != null ? pc.MaxFuel : 100f,
                currentToolIndex = tc != null ? tc.ActiveToolIndex : 0,
                unlockedTools = tc != null ? tc.GetUnlockedToolStates() : new bool[] { true, false, false, false, false, false },
                refineryLevel = refineryLevel,
                massDriverUnlocked = massDriverUnlocked,
                contractIndex = cm != null ? cm.CurrentContractIndex : 0,
                contractProgress = cm != null ? cm.CurrentProgress : 0f
            };

            if (ShopManager.Instance != null)
            {
                ShopUpgradeItem[] all = ShopManager.Instance.GetAllUpgrades();
                List<string> owned = new List<string>();
                int[] counts = new int[all.Length];
                for (int i = 0; i < all.Length; i++)
                {
                    counts[i] = all[i].purchaseCount;
                    if (all[i].isPurchased)
                    {
                        owned.Add(all[i].id);
                    }
                }
                data.purchasedUpgradeIds = owned.ToArray();
                data.upgradePurchaseCounts = counts;
                data.conveyorLevel = ShopManager.Instance.ConveyorCount;
                data.droneLevel = ShopManager.Instance.DroneCount;
                data.roverBotLevel = ShopManager.Instance.RoverBotCount;
            }

            SaveManager.Instance.SaveGame(data);
        }

        private void LoadSavedProgress()
        {
            if (SaveManager.Instance == null) return;
            GameSaveData data = SaveManager.Instance.LoadGame();
            if (data == null) return;

            lunarCredits = data.lunarCredits;
            totalDustCleanedKg = data.totalDustCleanedKg;
            refineryLevel = data.refineryLevel;
            massDriverUnlocked = data.massDriverUnlocked;

            PlayerInventory inv = FindFirstObjectByType<PlayerInventory>();
            if (inv != null && data.backpackCapacity > 30f)
            {
                inv.SetMaxCapacity(data.backpackCapacity);
            }

            PlayerController pc = FindFirstObjectByType<PlayerController>();
            if (pc != null && data.jetpackMaxFuel > 100f)
            {
                pc.SetMaxFuel(data.jetpackMaxFuel);
            }

            ToolController tc = FindFirstObjectByType<ToolController>();
            if (tc != null && data.unlockedTools != null)
            {
                tc.RestoreUnlockedTools(data.unlockedTools, data.currentToolIndex);
            }

            ContractManager cm = FindFirstObjectByType<ContractManager>();
            if (cm != null)
            {
                cm.RestoreContractState(data.contractIndex, data.contractProgress);
            }

            if (ShopManager.Instance != null)
            {
                ShopManager.Instance.RestorePurchasedState(
                    data.purchasedUpgradeIds,
                    data.upgradePurchaseCounts,
                    data.conveyorLevel,
                    data.droneLevel,
                    data.roverBotLevel,
                    data.refineryLevel,
                    data.massDriverUnlocked
                );
            }
        }

        public void ResetAllProgress()
        {
            SaveManager.Instance?.ResetSave();
            Time.timeScale = 1f;
            isGamePaused = false;
            isShopOpen = false;
            UnityEngine.SceneManagement.SceneManager.LoadScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex);
        }
    }
}
