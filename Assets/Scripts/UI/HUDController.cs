using UnityEngine;
using ElevenHopem.MoonSweepers.Core;
using ElevenHopem.MoonSweepers.Player;
using ElevenHopem.MoonSweepers.Tools;
using ElevenHopem.MoonSweepers.Environment;

namespace ElevenHopem.MoonSweepers.UI
{
    /// <summary>
    /// Dual HUD controller: Supports both High-Performance OnGUI immediate rendering
    /// and Canvas UI, guaranteeing instant visual rendering on macOS and Unity Editor
    /// with zero missing font/prefab dependency.
    /// Brand: 11Hopem (https://11hopem.com)
    /// </summary>
    public class HUDController : MonoBehaviour
    {
        public static HUDController Instance { get; private set; }

        [Header("References")]
        [SerializeField] private PlayerInventory playerInventory;
        [SerializeField] private PlayerController playerController;
        [SerializeField] private ToolController toolController;
        [SerializeField] private DepotHopper depotHopper;

        private GUIStyle headerStyle;
        private GUIStyle creditsStyle;
        private GUIStyle labelStyle;
        private GUIStyle promptStyle;
        private GUIStyle brandStyle;
        private Texture2D barBgTexture;
        private Texture2D barFillTexture;
        private Texture2D fuelFillTexture;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;

            CreateTextures();
        }

        private void Start()
        {
            if (playerInventory == null) playerInventory = FindFirstObjectByType<PlayerInventory>();
            if (playerController == null) playerController = FindFirstObjectByType<PlayerController>();
            if (toolController == null) toolController = FindFirstObjectByType<ToolController>();
            if (depotHopper == null) depotHopper = FindFirstObjectByType<DepotHopper>();
        }

        private void CreateTextures()
        {
            barBgTexture = MakeSolidTexture(2, 2, new Color(0.12f, 0.14f, 0.18f, 0.85f));
            barFillTexture = MakeSolidTexture(2, 2, new Color(0.35f, 0.75f, 1.0f, 0.95f));
            fuelFillTexture = MakeSolidTexture(2, 2, new Color(1.0f, 0.65f, 0.15f, 0.95f));
        }

        private Texture2D MakeSolidTexture(int width, int height, Color col)
        {
            Color[] pix = new Color[width * height];
            for (int i = 0; i < pix.Length; ++i) pix[i] = col;
            Texture2D result = new Texture2D(width, height);
            result.SetPixels(pix);
            result.Apply();
            return result;
        }

        private void InitStyles()
        {
            if (headerStyle != null) return;

            headerStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 18,
                fontStyle = FontStyle.Bold,
                normal = { textColor = Color.white }
            };

            creditsStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 24,
                fontStyle = FontStyle.Bold,
                normal = { textColor = new Color(0.3f, 1f, 0.5f) }
            };

            labelStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 14,
                fontStyle = FontStyle.Bold,
                normal = { textColor = new Color(0.9f, 0.9f, 0.9f) }
            };

            promptStyle = new GUIStyle(GUI.skin.box)
            {
                fontSize = 16,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleCenter,
                normal = { textColor = Color.yellow }
            };

            brandStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 12,
                fontStyle = FontStyle.Normal,
                normal = { textColor = new Color(0.7f, 0.7f, 0.7f, 0.8f) }
            };
        }

        private void OnGUI()
        {
            InitStyles();

            // Brand Watermark (Top Left)
            GUI.Label(new Rect(20, 15, 300, 25), "11Hopem Moon Sweepers", headerStyle);
            if (GUI.Button(new Rect(20, 40, 160, 22), "11hopem.com"))
            {
                Application.OpenURL("https://11hopem.com");
            }

            // Lunar Credits (Top Right)
            long credits = GameManager.Instance != null ? GameManager.Instance.LunarCredits : 0;
            string creditStr = $"LC ${credits:N0}";
            GUI.Label(new Rect(Screen.width - 240, 15, 220, 35), creditStr, creditsStyle);

            // Active Tool Display (Top Center)
            ToolData currentTool = toolController != null ? toolController.CurrentTool : null;
            if (currentTool != null)
            {
                string toolText = $"[Active Tool: {currentTool.toolName} (Tier {currentTool.tier})]";
                GUI.Label(new Rect(Screen.width / 2 - 150, 15, 300, 25), toolText, headerStyle);
            }

            // Crosshair in screen center
            float crosshairSize = 6f;
            GUI.Box(new Rect(Screen.width / 2 - crosshairSize / 2, Screen.height / 2 - crosshairSize / 2, crosshairSize, crosshairSize), GUIContent.none);

            // Bottom Left: Backpack Capacity Gauge
            float currentDust = playerInventory != null ? playerInventory.CurrentDustKg : 0f;
            float maxDust = playerInventory != null ? playerInventory.MaxBackpackCapacityKg : 30f;
            float fillRatio = Mathf.Clamp01(currentDust / maxDust);

            float gaugeY = Screen.height - 85;
            GUI.Label(new Rect(25, gaugeY - 24, 300, 22), $"Backpack: {currentDust:F1} / {maxDust:F1} kg ({(fillRatio * 100f):F0}%)", labelStyle);

            // Backpack Bar
            GUI.DrawTexture(new Rect(25, gaugeY, 260, 20), barBgTexture);
            GUI.DrawTexture(new Rect(25, gaugeY, 260 * fillRatio, 20), barFillTexture);

            // Jetpack Thruster Fuel Gauge
            float currentFuel = playerController != null ? playerController.CurrentFuel : 100f;
            float maxFuel = playerController != null ? playerController.MaxFuel : 100f;
            float fuelRatio = Mathf.Clamp01(currentFuel / maxFuel);

            float fuelY = Screen.height - 40;
            GUI.Label(new Rect(25, fuelY - 20, 260, 20), $"Thruster Fuel: {currentFuel:F0}%", labelStyle);
            GUI.DrawTexture(new Rect(25, fuelY, 260, 14), barBgTexture);
            GUI.DrawTexture(new Rect(25, fuelY, 260 * fuelRatio, 14), fuelFillTexture);

            // Bottom Right: Key Controls Info
            string controls = "[L-Click] Sweep / Vacuum\n[R-Click/Shift] Thruster Boost\n[Space] Low-G Jump\n[1-5] Switch Tool\n[TAB] Upgrade Hangar";
            GUI.Label(new Rect(Screen.width - 240, Screen.height - 110, 220, 95), controls, labelStyle);

            // Interaction Prompts (Center Bottom)
            if (depotHopper != null && depotHopper.PlayerInRange)
            {
                GUI.Box(new Rect(Screen.width / 2 - 180, Screen.height - 130, 360, 38), "Press [E] to Deposit Regolith Dust -> Convert to LC", promptStyle);
            }
            else if (playerInventory != null && playerInventory.IsFull)
            {
                GUI.Box(new Rect(Screen.width / 2 - 180, Screen.height - 130, 360, 38), "BACKPACK FULL! Return to Base Depot Hopper", promptStyle);
            }
        }
    }
}
