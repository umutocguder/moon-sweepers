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

        public float LunarGravity => lunarGravity;
        public long LunarCredits => lunarCredits;
        public float BaseDustValuePerKg => baseDustValuePerKg;
        public bool IsGamePaused => isGamePaused;
        public bool IsShopOpen => isShopOpen;
        public float TotalDustCleanedKg => totalDustCleanedKg;
        public float SectorCleanlinessPercent => totalPatches > 0 ? Mathf.Clamp01((float)cleanedPatches / totalPatches) * 100f : 0f;

        public event Action<long> OnCreditsChanged;
        public event Action<bool> OnPauseStateChanged;
        public event Action<bool> OnShopStateChanged;
        public event Action<string> OnToastNotification;

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

        public void ReportPatchCleanedState(bool isCleaned)
        {
            cleanedPatches = Mathf.Clamp(cleanedPatches + (isCleaned ? 1 : -1), 0, Mathf.Max(1, totalPatches));
        }

        public void RecordDustCleaned(float amountKg)
        {
            if (amountKg > 0f)
            {
                totalDustCleanedKg += amountKg;
            }
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

            GameSaveData data = new GameSaveData
            {
                lunarCredits = lunarCredits,
                totalDustCleanedKg = totalDustCleanedKg,
                backpackCapacity = inv != null ? inv.MaxBackpackCapacityKg : 30f,
                jetpackMaxFuel = pc != null ? pc.MaxFuel : 100f,
                currentToolIndex = tc != null ? tc.ActiveToolIndex : 0,
                unlockedTools = tc != null ? tc.GetUnlockedToolStates() : new bool[] { true, false, false, false, false }
            };

            if (ShopManager.Instance != null)
            {
                List<string> owned = new List<string>();
                foreach (var upg in ShopManager.Instance.GetAllUpgrades())
                {
                    if (upg.isPurchased)
                    {
                        owned.Add(upg.id);
                    }
                }
                data.purchasedUpgradeIds = owned.ToArray();
                data.conveyorLevel = ShopManager.Instance.ConveyorCount;
                data.droneLevel = ShopManager.Instance.DroneCount;
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

            if (ShopManager.Instance != null && data.purchasedUpgradeIds != null)
            {
                ShopManager.Instance.RestorePurchasedState(data.purchasedUpgradeIds, data.conveyorLevel, data.droneLevel);
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
