/*
 * Created by 11Hopem
 * Copyright (c) 2026 11Hopem. All rights reserved.
 */

using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using ElevenHopem.MoonSweepers.Core;
using ElevenHopem.MoonSweepers.Player;
using ElevenHopem.MoonSweepers.Tools;
using ElevenHopem.MoonSweepers.Environment;
using ElevenHopem.MoonSweepers.Economy;

namespace ElevenHopem.MoonSweepers.UI
{
    public class HUDController : MonoBehaviour
    {
        public static HUDController Instance { get; private set; }

        [SerializeField] private PlayerInventory playerInventory;
        [SerializeField] private PlayerController playerController;
        [SerializeField] private ToolController toolController;
        [SerializeField] private DepotHopper depotHopper;

        private Canvas rootCanvas;
        private Font uiFont;

        private Text creditsValueText;
        private Text upgradeReadyBadge;
        private Text cleanlinessLabel;
        private RectTransform cleanlinessFillRect;
        private Text activeToolHeaderText;
        private Text depotCompassText;

        private Text backpackLabelText;
        private RectTransform backpackFillRect;
        private Image backpackFillImage;

        private Text fuelLabelText;
        private RectTransform fuelFillRect;

        private Image crosshairDot;
        private readonly RectTransform[] crosshairTicks = new RectTransform[4];
        private readonly Image[] crosshairTickImages = new Image[4];
        private Text targetReadoutText;

        private GameObject bannerPanel;
        private Text bannerText;
        private Image bannerBgImage;
        private float toastTimer = 0f;
        private string activeToastMessage = "";

        private readonly Image[] slotBackgrounds = new Image[5];
        private readonly Text[] slotLabels = new Text[5];

        private GameObject pauseOverlay;

        private static readonly string[] ShortToolNames = new string[]
        {
            "Brush",
            "Broom",
            "Vacuum",
            "Electro",
            "Buggy"
        };

        public Canvas RootCanvas => rootCanvas;
        public Font SharedFont => GetOrLoadFont();

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;

            EnsureEventSystem();
            BuildHudCanvas();
        }

        private void Start()
        {
            BindSceneReferences();
            if (GameManager.Instance != null)
            {
                GameManager.Instance.OnToastNotification += HandleToast;
                GameManager.Instance.OnPauseStateChanged += HandlePauseChanged;
            }
        }

        private void OnDestroy()
        {
            if (GameManager.Instance != null)
            {
                GameManager.Instance.OnToastNotification -= HandleToast;
                GameManager.Instance.OnPauseStateChanged -= HandlePauseChanged;
            }
        }

        private void BindSceneReferences()
        {
            if (playerInventory == null) playerInventory = FindFirstObjectByType<PlayerInventory>();
            if (playerController == null) playerController = FindFirstObjectByType<PlayerController>();
            if (toolController == null) toolController = FindFirstObjectByType<ToolController>();
            if (depotHopper == null) depotHopper = FindFirstObjectByType<DepotHopper>();
        }

        private Font GetOrLoadFont()
        {
            if (uiFont != null) return uiFont;
            uiFont = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            if (uiFont == null)
            {
                uiFont = Font.CreateDynamicFontFromOSFont("Helvetica Neue", 16);
            }
            return uiFont;
        }

        private static void EnsureEventSystem()
        {
            if (FindFirstObjectByType<EventSystem>() != null) return;
            GameObject es = new GameObject("EventSystem");
            es.AddComponent<EventSystem>();
            es.AddComponent<StandaloneInputModule>();
        }

        private void BuildHudCanvas()
        {
            Font font = GetOrLoadFont();

            GameObject canvasObj = new GameObject("LunarHUDCanvas");
            canvasObj.transform.SetParent(transform, false);
            rootCanvas = canvasObj.AddComponent<Canvas>();
            rootCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
            rootCanvas.sortingOrder = 10;

            CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.matchWidthOrHeight = 0.5f;

            canvasObj.AddComponent<GraphicRaycaster>();

            Sprite cardSprite = LunarMaterialLibrary.GetRoundedSprite(
                "HudCardBg", 64, 12, 2,
                new Color(0.04f, 0.07f, 0.13f, 0.84f),
                new Color(0.20f, 0.65f, 0.95f, 0.42f)
            );
            Sprite barBgSprite = LunarMaterialLibrary.GetRoundedSprite(
                "HudBarBg", 32, 6, 1,
                new Color(0.08f, 0.12f, 0.19f, 0.95f),
                new Color(0.25f, 0.40f, 0.55f, 0.45f)
            );
            Sprite barFillSprite = LunarMaterialLibrary.GetRoundedSprite(
                "HudBarFill", 32, 6, 0,
                Color.white,
                Color.white
            );
            Sprite pillSprite = LunarMaterialLibrary.GetRoundedSprite(
                "HudPillBtn", 48, 10, 1,
                new Color(0.10f, 0.24f, 0.40f, 0.90f),
                new Color(0.30f, 0.80f, 1.0f, 0.65f)
            );

            RectTransform topLeft = CreatePanel("TopLeftBrandCard", canvasObj.transform, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(24f, -22f), new Vector2(396f, 108f), cardSprite);
            CreateLabel("BrandTitle", topLeft, font, "11HOPEM MOON SWEEPERS", 16, FontStyle.Bold, new Color(0.92f, 0.97f, 1f), TextAnchor.MiddleLeft, new Vector2(16f, -12f), new Vector2(262f, 26f));

            Button brandBtn = CreateStyledButton("BrandSiteBtn", topLeft, font, "11hopem.com", 12, pillSprite, new Vector2(282f, -11f), new Vector2(98f, 26f));
            brandBtn.onClick.AddListener(() => Application.OpenURL("https://11hopem.com"));

            cleanlinessLabel = CreateLabel("CleanlinessText", topLeft, font, "SECTOR CLEANLINESS • 0.0%", 12, FontStyle.Bold, new Color(0.58f, 0.84f, 0.98f), TextAnchor.MiddleLeft, new Vector2(16f, -48f), new Vector2(364f, 20f));
            RectTransform cleanTrack = CreateBarTrack("CleanBarTrack", topLeft, new Vector2(16f, -74f), new Vector2(364f, 14f), barBgSprite);
            cleanlinessFillRect = CreateBarFill("CleanBarFill", cleanTrack, barFillSprite, new Color(0.22f, 0.88f, 0.98f), out _);

            RectTransform topCenter = CreatePanel("TopCenterNavCard", canvasObj.transform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -22f), new Vector2(470f, 74f), cardSprite);
            depotCompassText = CreateLabel("DepotCompass", topCenter, font, "DEPOT HOPPER • 13.5m AHEAD", 13, FontStyle.Bold, new Color(0.35f, 0.98f, 0.72f), TextAnchor.MiddleCenter, new Vector2(0f, -10f), new Vector2(440f, 22f), true);
            activeToolHeaderText = CreateLabel("ActiveToolTitle", topCenter, font, "TIER 1 • STANDARD TOOTHBRUSH", 16, FontStyle.Bold, Color.white, TextAnchor.MiddleCenter, new Vector2(0f, -36f), new Vector2(440f, 26f), true);

            RectTransform topRight = CreatePanel("TopRightCreditsCard", canvasObj.transform, new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(-24f, -22f), new Vector2(310f, 94f), cardSprite);
            CreateLabel("CreditsCaption", topRight, font, "LUNAR CREDITS BALANCE", 11, FontStyle.Bold, new Color(0.62f, 0.76f, 0.90f), TextAnchor.MiddleRight, new Vector2(-18f, -12f), new Vector2(274f, 18f), false, true);
            creditsValueText = CreateLabel("CreditsValue", topRight, font, "LC 0", 28, FontStyle.Bold, new Color(0.28f, 1.0f, 0.58f), TextAnchor.MiddleRight, new Vector2(-18f, -32f), new Vector2(274f, 36f), false, true);
            upgradeReadyBadge = CreateLabel("UpgradeHint", topRight, font, "PRESS [TAB] FOR HANGAR UPGRADES", 11, FontStyle.Bold, new Color(0.65f, 0.82f, 0.95f), TextAnchor.MiddleRight, new Vector2(-18f, -68f), new Vector2(274f, 18f), false, true);

            BuildCenterReticle(canvasObj.transform, font);

            RectTransform bottomLeft = CreatePanel("BottomLeftGauges", canvasObj.transform, new Vector2(0f, 0f), new Vector2(0f, 0f), new Vector2(0f, 0f), new Vector2(24f, 24f), new Vector2(370f, 124f), cardSprite);
            backpackLabelText = CreateLabel("BackpackLabel", bottomLeft, font, "BACKPACK REGOLITH • 0.0 / 30.0 kg", 13, FontStyle.Bold, Color.white, TextAnchor.MiddleLeft, new Vector2(16f, -14f), new Vector2(338f, 20f));
            RectTransform bpTrack = CreateBarTrack("BackpackTrack", bottomLeft, new Vector2(16f, -38f), new Vector2(338f, 18f), barBgSprite);
            backpackFillRect = CreateBarFill("BackpackFill", bpTrack, barFillSprite, new Color(0.22f, 0.78f, 1.0f), out backpackFillImage);

            fuelLabelText = CreateLabel("FuelLabel", bottomLeft, font, "THRUSTER HYDRAZINE • 100%", 13, FontStyle.Bold, new Color(0.95f, 0.88f, 0.72f), TextAnchor.MiddleLeft, new Vector2(16f, -68f), new Vector2(338f, 20f));
            RectTransform fuelTrack = CreateBarTrack("FuelTrack", bottomLeft, new Vector2(16f, -92f), new Vector2(338f, 14f), barBgSprite);
            fuelFillRect = CreateBarFill("FuelFill", fuelTrack, barFillSprite, new Color(1.0f, 0.68f, 0.16f), out _);

            BuildBottomHotbarAndBanner(canvasObj.transform, font, cardSprite, pillSprite);

            RectTransform bottomRight = CreatePanel("TopRightControls", canvasObj.transform, new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(-24f, -128f), new Vector2(310f, 136f), cardSprite);
            string controlsCopy =
                "WASD + SHIFT   Move / Sprint\n" +
                "SPACE / RMB    Jump / Jetpack\n" +
                "LEFT CLICK     Scrub / Vacuum\n" +
                "1 - 5 / WHEEL  Switch Tool\n" +
                "TAB / H        Hangar / Base\n" +
                "ESC / [ ]      Menu / Mouse Sens";
            Text ctrlText = CreateLabel("ControlsText", bottomRight, font, controlsCopy, 12, FontStyle.Bold, new Color(0.82f, 0.90f, 0.98f), TextAnchor.UpperLeft, new Vector2(16f, -13f), new Vector2(278f, 114f));
            ctrlText.lineSpacing = 1.18f;

            BuildPauseOverlay(canvasObj.transform, font, cardSprite, pillSprite);
        }

        private void BuildCenterReticle(Transform parent, Font font)
        {
            GameObject centerRoot = new GameObject("CrosshairRoot");
            centerRoot.transform.SetParent(parent, false);
            RectTransform rt = centerRoot.AddComponent<RectTransform>();
            rt.anchorMin = new Vector2(0.5f, 0.5f);
            rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.sizeDelta = new Vector2(120f, 120f);
            rt.anchoredPosition = Vector2.zero;

            GameObject dotObj = new GameObject("CenterDot");
            dotObj.transform.SetParent(rt, false);
            RectTransform dotRt = dotObj.AddComponent<RectTransform>();
            dotRt.sizeDelta = new Vector2(5f, 5f);
            crosshairDot = dotObj.AddComponent<Image>();
            crosshairDot.color = new Color(1f, 1f, 1f, 0.92f);
            crosshairDot.raycastTarget = false;

            Vector2[] dirs = new Vector2[] { Vector2.up, Vector2.down, Vector2.left, Vector2.right };
            for (int i = 0; i < 4; i++)
            {
                GameObject tick = new GameObject($"ReticleTick_{i}");
                tick.transform.SetParent(rt, false);
                RectTransform trt = tick.AddComponent<RectTransform>();
                bool vertical = i < 2;
                trt.sizeDelta = vertical ? new Vector2(2.5f, 9f) : new Vector2(9f, 2.5f);
                trt.anchoredPosition = dirs[i] * 11f;
                Image img = tick.AddComponent<Image>();
                img.color = new Color(0.85f, 0.95f, 1f, 0.85f);
                img.raycastTarget = false;
                crosshairTicks[i] = trt;
                crosshairTickImages[i] = img;
            }

            targetReadoutText = CreateLabel("TargetReadout", rt, font, "", 13, FontStyle.Bold, new Color(0.35f, 1.0f, 0.72f), TextAnchor.UpperCenter, new Vector2(0f, -26f), new Vector2(360f, 40f), true);
            Shadow sh = targetReadoutText.gameObject.AddComponent<Shadow>();
            sh.effectColor = new Color(0f, 0f, 0f, 0.85f);
            sh.effectDistance = new Vector2(1.5f, -1.5f);
        }

        private void BuildBottomHotbarAndBanner(Transform parent, Font font, Sprite cardSprite, Sprite slotSprite)
        {
            RectTransform hotbar = CreatePanel("ToolHotbar", parent, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0f, 24f), new Vector2(560f, 68f), cardSprite);

            for (int i = 0; i < 5; i++)
            {
                float x = -216f + i * 108f;
                GameObject slotObj = new GameObject($"ToolSlot_{i + 1}");
                slotObj.transform.SetParent(hotbar, false);
                RectTransform srt = slotObj.AddComponent<RectTransform>();
                srt.anchorMin = new Vector2(0.5f, 0.5f);
                srt.anchorMax = new Vector2(0.5f, 0.5f);
                srt.sizeDelta = new Vector2(98f, 50f);
                srt.anchoredPosition = new Vector2(x, 0f);

                Image bg = slotObj.AddComponent<Image>();
                bg.sprite = slotSprite;
                bg.type = Image.Type.Sliced;
                bg.raycastTarget = false;
                slotBackgrounds[i] = bg;

                Text lbl = CreateLabel("SlotText", srt, font, $"[{i + 1}]\n{ShortToolNames[i]}", 12, FontStyle.Bold, Color.white, TextAnchor.MiddleCenter, Vector2.zero, new Vector2(92f, 46f), true);
                lbl.rectTransform.anchorMin = Vector2.zero;
                lbl.rectTransform.anchorMax = Vector2.one;
                lbl.rectTransform.offsetMin = Vector2.zero;
                lbl.rectTransform.offsetMax = Vector2.zero;
                slotLabels[i] = lbl;
            }

            RectTransform bannerRt = CreatePanel("StatusBanner", parent, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0f, 104f), new Vector2(560f, 42f), cardSprite);
            bannerPanel = bannerRt.gameObject;
            bannerBgImage = bannerPanel.GetComponent<Image>();
            bannerText = CreateLabel("BannerLabel", bannerRt, font, "", 14, FontStyle.Bold, new Color(0.95f, 0.98f, 1f), TextAnchor.MiddleCenter, new Vector2(0f, -8f), new Vector2(530f, 26f), true);
            bannerPanel.SetActive(false);
        }

        private void BuildPauseOverlay(Transform parent, Font font, Sprite cardSprite, Sprite btnSprite)
        {
            GameObject overlay = new GameObject("PauseMenuOverlay");
            overlay.transform.SetParent(parent, false);
            RectTransform ort = overlay.AddComponent<RectTransform>();
            ort.anchorMin = Vector2.zero;
            ort.anchorMax = Vector2.one;
            ort.offsetMin = Vector2.zero;
            ort.offsetMax = Vector2.zero;
            Image dim = overlay.AddComponent<Image>();
            dim.color = new Color(0.01f, 0.02f, 0.05f, 0.78f);

            RectTransform box = CreatePanel("PauseCard", overlay.transform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(440f, 420f), cardSprite);
            CreateLabel("PauseTitle", box, font, "MISSION PAUSED", 24, FontStyle.Bold, new Color(0.35f, 0.88f, 1f), TextAnchor.MiddleCenter, new Vector2(0f, -24f), new Vector2(380f, 34f), true);
            CreateLabel("PauseSub", box, font, "11Hopem Lunar Operations", 13, FontStyle.Normal, new Color(0.68f, 0.78f, 0.88f), TextAnchor.MiddleCenter, new Vector2(0f, -58f), new Vector2(380f, 22f), true);

            Button resumeBtn = CreateCenteredButton("ResumeBtn", box, font, "RESUME MISSION [ESC]", 15, btnSprite, new Vector2(0f, -108f), new Vector2(320f, 44f));
            resumeBtn.onClick.AddListener(() => GameManager.Instance?.SetPaused(false));

            Button shopBtn = CreateCenteredButton("OpenShopBtn", box, font, "OPEN UPGRADE HANGAR [TAB]", 15, btnSprite, new Vector2(0f, -162f), new Vector2(320f, 44f));
            shopBtn.onClick.AddListener(() =>
            {
                GameManager.Instance?.SetPaused(false);
                GameManager.Instance?.SetShopOpen(true);
            });

            Button fsBtn = CreateCenteredButton("FullscreenBtn", box, font, "TOGGLE FULLSCREEN [F11]", 15, btnSprite, new Vector2(0f, -216f), new Vector2(320f, 44f));
            fsBtn.onClick.AddListener(() => GameManager.Instance?.ToggleFullscreen());

            Button resetBtn = CreateCenteredButton("ResetSaveBtn", box, font, "RESET SAVE PROGRESS", 14, btnSprite, new Vector2(0f, -270f), new Vector2(320f, 42f));
            resetBtn.onClick.AddListener(() => GameManager.Instance?.ResetAllProgress());

            Button webBtn = CreateCenteredButton("WebLinkBtn", box, font, "VISIT 11HOPEM.COM", 14, btnSprite, new Vector2(0f, -322f), new Vector2(320f, 40f));
            webBtn.onClick.AddListener(() => Application.OpenURL("https://11hopem.com"));

            Button quitBtn = CreateCenteredButton("QuitBtn", box, font, "QUIT TO DESKTOP", 14, btnSprite, new Vector2(0f, -372f), new Vector2(320f, 40f));
            quitBtn.onClick.AddListener(() => Application.Quit());

            pauseOverlay = overlay;
            pauseOverlay.SetActive(false);
        }

        private void HandleToast(string msg)
        {
            activeToastMessage = msg;
            toastTimer = 3.2f;
        }

        private void HandlePauseChanged(bool paused)
        {
            if (pauseOverlay != null)
            {
                pauseOverlay.SetActive(paused);
            }
        }

        private void Update()
        {
            BindSceneReferences();

            long credits = GameManager.Instance != null ? GameManager.Instance.LunarCredits : 0;
            if (creditsValueText != null)
            {
                creditsValueText.text = $"LC {credits:N0}";
            }

            if (upgradeReadyBadge != null)
            {
                bool canBuy = ShopManager.Instance != null && ShopManager.Instance.CanPurchaseAnyUpgrade(credits);
                upgradeReadyBadge.text = canBuy ? "★ UPGRADE READY! PRESS [TAB]" : "PRESS [TAB] FOR HANGAR UPGRADES";
                upgradeReadyBadge.color = canBuy ? new Color(1.0f, 0.84f, 0.25f) : new Color(0.58f, 0.75f, 0.90f);
            }

            if (cleanlinessLabel != null && GameManager.Instance != null)
            {
                float pct = GameManager.Instance.SectorCleanlinessPercent;
                cleanlinessLabel.text = $"SECTOR CLEANED • {pct:F1}% ({GameManager.Instance.TotalDustCleanedKg:F0} kg)";
                SetBarFill(cleanlinessFillRect, Mathf.Clamp01(pct / 100f));
            }

            if (toolController != null && activeToolHeaderText != null && toolController.CurrentTool != null)
            {
                ToolData t = toolController.CurrentTool;
                activeToolHeaderText.text = $"TIER {t.tier} • {t.toolName.ToUpperInvariant()}";
            }

            if (depotCompassText != null && playerController != null && depotHopper != null)
            {
                Vector3 toDepot = depotHopper.transform.position - playerController.transform.position;
                toDepot.y = 0f;
                float dist = toDepot.magnitude;
                if (depotHopper.PlayerInRange)
                {
                    depotCompassText.text = "● INSIDE DEPOT HOPPER ZONE • AUTO-CONVERTING TO LC";
                    depotCompassText.color = new Color(0.30f, 1.0f, 0.58f);
                }
                else
                {
                    float signedAngle = Vector3.SignedAngle(playerController.transform.forward, toDepot.normalized, Vector3.up);
                    string arrow = Mathf.Abs(signedAngle) < 28f ? "▲" : (signedAngle > 0f ? "▶" : "◀");
                    depotCompassText.text = $"{arrow} BASE DEPOT HOPPER • {dist:F1}m";
                    depotCompassText.color = new Color(0.38f, 0.92f, 1.0f);
                }
            }

            if (playerInventory != null)
            {
                float cur = playerInventory.CurrentDustKg;
                float max = playerInventory.MaxBackpackCapacityKg;
                float ratio = playerInventory.FillRatio;
                int minCount = playerInventory.TotalMineralCount;
                string minTag = minCount > 0 ? $"  [+{minCount} Ore]" : "";
                backpackLabelText.text = $"BACKPACK • {cur:F1} / {max:F0} kg ({ratio * 100f:F0}%){minTag}";
                SetBarFill(backpackFillRect, ratio);

                if (backpackFillImage != null)
                {
                    backpackFillImage.color = playerInventory.IsFull
                        ? new Color(1.0f, 0.34f, 0.30f)
                        : (ratio > 0.8f ? new Color(1.0f, 0.75f, 0.20f) : new Color(0.22f, 0.78f, 1.0f));
                }
            }

            if (playerController != null)
            {
                float fRatio = Mathf.Clamp01(playerController.CurrentFuel / Mathf.Max(1f, playerController.MaxFuel));
                fuelLabelText.text = $"JETPACK THRUSTER • {fRatio * 100f:F0}%";
                SetBarFill(fuelFillRect, fRatio);
            }

            UpdateReticleAndSlots();
            UpdateStatusBanner();
        }

        private void UpdateReticleAndSlots()
        {
            bool menuOpen = GameManager.Instance != null && (GameManager.Instance.IsGamePaused || GameManager.Instance.IsShopOpen);
            if (crosshairDot != null)
            {
                crosshairDot.transform.parent.gameObject.SetActive(!menuOpen);
            }
            if (menuOpen) return;

            RegolithDustPatch hovered = toolController != null ? toolController.HoveredDustPatch : null;
            bool cleaning = toolController != null && toolController.IsActivelyCleaning;

            Color reticleCol = hovered != null
                ? new Color(0.25f, 1.0f, 0.65f, 0.98f)
                : new Color(0.88f, 0.95f, 1.0f, 0.78f);

            float spread = cleaning ? 16f + Mathf.Sin(Time.time * 32f) * 2.5f : (hovered != null ? 9.5f : 12.5f);
            Vector2[] dirs = new Vector2[] { Vector2.up, Vector2.down, Vector2.left, Vector2.right };
            for (int i = 0; i < 4; i++)
            {
                if (crosshairTicks[i] != null)
                {
                    crosshairTicks[i].anchoredPosition = Vector2.Lerp(crosshairTicks[i].anchoredPosition, dirs[i] * spread, Time.deltaTime * 18f);
                    crosshairTickImages[i].color = reticleCol;
                }
            }
            if (crosshairDot != null) crosshairDot.color = reticleCol;

            if (targetReadoutText != null)
            {
                if (hovered != null)
                {
                    string mineralHint = hovered.HasEmbeddedMineral ? "\n★ RARE MINERAL SIGNAL" : "";
                    targetReadoutText.text = $"REGOLITH PILE • {hovered.CurrentDustMassKg:F1} kg{mineralHint}";
                }
                else
                {
                    targetReadoutText.text = "";
                }
            }

            if (toolController != null)
            {
                for (int i = 0; i < 5; i++)
                {
                    bool unlocked = toolController.IsToolUnlocked(i);
                    bool active = toolController.ActiveToolIndex == i;

                    if (slotBackgrounds[i] != null)
                    {
                        slotBackgrounds[i].color = active
                            ? new Color(0.20f, 0.78f, 1.0f, 0.95f)
                            : (unlocked ? new Color(0.14f, 0.22f, 0.34f, 0.85f) : new Color(0.06f, 0.08f, 0.12f, 0.65f));
                    }
                    if (slotLabels[i] != null)
                    {
                        slotLabels[i].text = unlocked ? $"[{i + 1}]\n{ShortToolNames[i]}" : $"[{i + 1}]\nLocked";
                        slotLabels[i].color = active ? Color.white : (unlocked ? new Color(0.82f, 0.90f, 0.98f) : new Color(0.42f, 0.48f, 0.56f));
                    }
                }
            }
        }

        private void UpdateStatusBanner()
        {
            if (bannerPanel == null) return;

            if (toastTimer > 0f)
            {
                toastTimer -= Time.unscaledDeltaTime;
                bannerPanel.SetActive(true);
                bannerText.text = activeToastMessage;
                bannerBgImage.color = new Color(0.08f, 0.26f, 0.42f, 0.92f);
                return;
            }

            if (playerInventory != null && playerInventory.IsFull)
            {
                bannerPanel.SetActive(true);
                bannerText.text = "BACKPACK FULL! WALK ONTO THE GLOWING CYAN DEPOT PAD AHEAD TO CASH IN";
                bannerBgImage.color = new Color(0.55f, 0.16f, 0.16f, 0.92f);
                return;
            }

            bannerPanel.SetActive(false);
        }

        private static void SetBarFill(RectTransform fillRt, float ratio)
        {
            if (fillRt == null) return;
            fillRt.anchorMax = new Vector2(Mathf.Clamp01(ratio), 1f);
        }

        private static RectTransform CreatePanel(string name, Transform parent, Vector2 anchorMin, Vector2 anchorMax, Vector2 pivot, Vector2 pos, Vector2 size, Sprite sprite)
        {
            GameObject go = new GameObject(name);
            go.transform.SetParent(parent, false);
            RectTransform rt = go.AddComponent<RectTransform>();
            rt.anchorMin = anchorMin;
            rt.anchorMax = anchorMax;
            rt.pivot = pivot;
            rt.anchoredPosition = pos;
            rt.sizeDelta = size;

            Image img = go.AddComponent<Image>();
            img.sprite = sprite;
            img.type = Image.Type.Sliced;
            return rt;
        }

        private static RectTransform CreateBarTrack(string name, Transform parent, Vector2 pos, Vector2 size, Sprite bgSprite)
        {
            GameObject track = new GameObject(name);
            track.transform.SetParent(parent, false);
            RectTransform rt = track.AddComponent<RectTransform>();
            rt.anchorMin = new Vector2(0f, 1f);
            rt.anchorMax = new Vector2(0f, 1f);
            rt.pivot = new Vector2(0f, 1f);
            rt.anchoredPosition = pos;
            rt.sizeDelta = size;

            Image img = track.AddComponent<Image>();
            img.sprite = bgSprite;
            img.type = Image.Type.Sliced;
            img.raycastTarget = false;
            return rt;
        }

        private static RectTransform CreateBarFill(string name, Transform track, Sprite fillSprite, Color color, out Image fillImg)
        {
            GameObject fill = new GameObject(name);
            fill.transform.SetParent(track, false);
            RectTransform rt = fill.AddComponent<RectTransform>();
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = new Vector2(1f, 1f);
            rt.offsetMin = new Vector2(2f, 2f);
            rt.offsetMax = new Vector2(-2f, -2f);

            fillImg = fill.AddComponent<Image>();
            fillImg.sprite = fillSprite;
            fillImg.type = Image.Type.Sliced;
            fillImg.color = color;
            fillImg.raycastTarget = false;
            return rt;
        }

        private static Text CreateLabel(string name, Transform parent, Font font, string content, int fontSize, FontStyle style, Color color, TextAnchor align, Vector2 pos, Vector2 size, bool centerAnchor = false, bool rightAnchor = false)
        {
            GameObject go = new GameObject(name);
            go.transform.SetParent(parent, false);
            RectTransform rt = go.AddComponent<RectTransform>();

            if (centerAnchor)
            {
                rt.anchorMin = new Vector2(0.5f, 1f);
                rt.anchorMax = new Vector2(0.5f, 1f);
                rt.pivot = new Vector2(0.5f, 1f);
            }
            else if (rightAnchor)
            {
                rt.anchorMin = new Vector2(1f, 1f);
                rt.anchorMax = new Vector2(1f, 1f);
                rt.pivot = new Vector2(1f, 1f);
            }
            else
            {
                rt.anchorMin = new Vector2(0f, 1f);
                rt.anchorMax = new Vector2(0f, 1f);
                rt.pivot = new Vector2(0f, 1f);
            }

            rt.anchoredPosition = pos;
            rt.sizeDelta = size;

            Text txt = go.AddComponent<Text>();
            txt.font = font;
            txt.text = content;
            txt.fontSize = fontSize;
            txt.fontStyle = style;
            txt.color = color;
            txt.alignment = align;
            txt.horizontalOverflow = HorizontalWrapMode.Wrap;
            txt.verticalOverflow = VerticalWrapMode.Overflow;
            txt.raycastTarget = false;
            return txt;
        }

        private static Button CreateStyledButton(string name, Transform parent, Font font, string label, int fontSize, Sprite sprite, Vector2 pos, Vector2 size)
        {
            GameObject go = new GameObject(name);
            go.transform.SetParent(parent, false);
            RectTransform rt = go.AddComponent<RectTransform>();
            rt.anchorMin = new Vector2(0f, 1f);
            rt.anchorMax = new Vector2(0f, 1f);
            rt.pivot = new Vector2(0f, 1f);
            rt.anchoredPosition = pos;
            rt.sizeDelta = size;

            Image img = go.AddComponent<Image>();
            img.sprite = sprite;
            img.type = Image.Type.Sliced;

            Button btn = go.AddComponent<Button>();
            btn.targetGraphic = img;

            Text txt = CreateLabel("Label", rt, font, label, fontSize, FontStyle.Bold, new Color(0.75f, 0.95f, 1f), TextAnchor.MiddleCenter, Vector2.zero, size, true);
            txt.rectTransform.anchorMin = Vector2.zero;
            txt.rectTransform.anchorMax = Vector2.one;
            txt.rectTransform.offsetMin = Vector2.zero;
            txt.rectTransform.offsetMax = Vector2.zero;
            return btn;
        }

        private static Button CreateCenteredButton(string name, Transform parent, Font font, string label, int fontSize, Sprite sprite, Vector2 pos, Vector2 size)
        {
            GameObject go = new GameObject(name);
            go.transform.SetParent(parent, false);
            RectTransform rt = go.AddComponent<RectTransform>();
            rt.anchorMin = new Vector2(0.5f, 1f);
            rt.anchorMax = new Vector2(0.5f, 1f);
            rt.pivot = new Vector2(0.5f, 1f);
            rt.anchoredPosition = pos;
            rt.sizeDelta = size;

            Image img = go.AddComponent<Image>();
            img.sprite = sprite;
            img.type = Image.Type.Sliced;

            Button btn = go.AddComponent<Button>();
            btn.targetGraphic = img;

            Text txt = CreateLabel("Label", rt, font, label, fontSize, FontStyle.Bold, Color.white, TextAnchor.MiddleCenter, Vector2.zero, size, true);
            txt.rectTransform.anchorMin = Vector2.zero;
            txt.rectTransform.anchorMax = Vector2.one;
            txt.rectTransform.offsetMin = Vector2.zero;
            txt.rectTransform.offsetMax = Vector2.zero;
            return btn;
        }
    }
}
