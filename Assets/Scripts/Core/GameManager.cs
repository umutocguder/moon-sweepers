using System;
using UnityEngine;

namespace ElevenHopem.MoonSweepers.Core
{
    /// <summary>
    /// Central game state manager for 11Hopem Moon Sweepers.
    /// Controls overall lifecycle, economic balances, low-gravity simulation, and pause states.
    /// Brand: 11Hopem (https://11hopem.com)
    /// </summary>
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }

        [Header("Lunar Physics Settings")]
        [Tooltip("Lunar gravity in m/s^2 (Standard Moon gravity is approx -1.62 m/s^2)")]
        [SerializeField] private float lunarGravity = -1.62f;

        [Header("Economy & Progression")]
        [SerializeField] private long lunarCredits = 0;
        [SerializeField] private float baseDustValuePerKg = 2.5f; // LC per kg

        [Header("State")]
        [SerializeField] private bool isGamePaused = false;
        [SerializeField] private bool isShopOpen = false;

        public float LunarGravity => lunarGravity;
        public long LunarCredits => lunarCredits;
        public float BaseDustValuePerKg => baseDustValuePerKg;
        public bool IsGamePaused => isGamePaused;
        public bool IsShopOpen => isShopOpen;

        public event Action<long> OnCreditsChanged;
        public event Action<bool> OnPauseStateChanged;
        public event Action<bool> OnShopStateChanged;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);

            // Apply global physics gravity
            Physics.gravity = new Vector3(0f, lunarGravity, 0f);
        }

        private void Start()
        {
            // Initial notification
            OnCreditsChanged?.Invoke(lunarCredits);
        }

        private void Update()
        {
            // Keyboard shortcuts
            if (Input.GetKeyDown(KeyCode.Tab) || Input.GetKeyDown(KeyCode.B))
            {
                ToggleShop();
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
        }

        public void AddCredits(long amount)
        {
            if (amount <= 0) return;
            lunarCredits += amount;
            OnCreditsChanged?.Invoke(lunarCredits);
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
            Cursor.lockState = open ? CursorLockMode.None : CursorLockMode.Locked;
            Cursor.visible = open;
            OnShopStateChanged?.Invoke(open);
        }

        public void TogglePause()
        {
            isGamePaused = !isGamePaused;
            Time.timeScale = isGamePaused ? 0f : 1f;
            Cursor.lockState = isGamePaused ? CursorLockMode.None : CursorLockMode.Locked;
            Cursor.visible = isGamePaused;
            OnPauseStateChanged?.Invoke(isGamePaused);
        }
    }
}
