/*
 * Created by 11Hopem
 * Copyright (c) 2026 11Hopem. All rights reserved.
 */

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using ElevenHopem.MoonSweepers.Core;
using ElevenHopem.MoonSweepers.Economy;

namespace ElevenHopem.MoonSweepers.UI
{
    public class ShopUIController : MonoBehaviour
    {
        private class UpgradeCardWidgets
        {
            public ShopUpgradeItem item;
            public Text categoryText;
            public Text titleText;
            public Text descText;
            public Text priceText;
            public Button buyButton;
            public Image buyButtonImage;
            public Text buyButtonLabel;
        }

        private GameObject shopOverlay;
        private Text balanceText;
        private readonly List<UpgradeCardWidgets> cardWidgets = new List<UpgradeCardWidgets>();
        private bool built = false;

        private void Start()
        {
            EnsureBuilt();
            if (GameManager.Instance != null)
            {
                GameManager.Instance.OnShopStateChanged += HandleShopVisibility;
                GameManager.Instance.OnCreditsChanged += HandleCreditsUpdated;
            }
            if (ShopManager.Instance != null)
            {
                ShopManager.Instance.OnUpgradePurchased += _ => RefreshCards();
            }
            HandleShopVisibility(GameManager.Instance != null && GameManager.Instance.IsShopOpen);
        }

        private void OnDestroy()
        {
            if (GameManager.Instance != null)
            {
                GameManager.Instance.OnShopStateChanged -= HandleShopVisibility;
                GameManager.Instance.OnCreditsChanged -= HandleCreditsUpdated;
            }
        }

        private void HandleShopVisibility(bool isOpen)
        {
            EnsureBuilt();
            if (shopOverlay != null)
            {
                shopOverlay.SetActive(isOpen);
                if (isOpen)
                {
                    RefreshCards();
                }
            }
        }

        private void HandleCreditsUpdated(long _)
        {
            if (shopOverlay != null && shopOverlay.activeSelf)
            {
                RefreshCards();
            }
        }

        private void EnsureBuilt()
        {
            if (built) return;
            HUDController hud = HUDController.Instance ?? FindFirstObjectByType<HUDController>();
            if (hud == null || hud.RootCanvas == null) return;

            built = true;
            Font font = hud.SharedFont;

            Sprite modalSprite = LunarMaterialLibrary.GetRoundedSprite(
                "HangarModalBg", 64, 16, 2,
                new Color(0.03f, 0.06f, 0.11f, 0.95f),
                new Color(0.24f, 0.72f, 1.0f, 0.65f)
            );
            Sprite itemCardSprite = LunarMaterialLibrary.GetRoundedSprite(
                "HangarItemCard", 64, 12, 1,
                new Color(0.07f, 0.12f, 0.20f, 0.92f),
                new Color(0.20f, 0.48f, 0.72f, 0.50f)
            );
            Sprite btnSprite = LunarMaterialLibrary.GetRoundedSprite(
                "HangarActionBtn", 48, 10, 1,
                Color.white,
                new Color(1f, 1f, 1f, 0.35f)
            );

            shopOverlay = new GameObject("LunarHangarOverlay");
            shopOverlay.transform.SetParent(hud.RootCanvas.transform, false);
            RectTransform ort = shopOverlay.AddComponent<RectTransform>();
            ort.anchorMin = Vector2.zero;
            ort.anchorMax = Vector2.one;
            ort.offsetMin = Vector2.zero;
            ort.offsetMax = Vector2.zero;
            Image dim = shopOverlay.AddComponent<Image>();
            dim.color = new Color(0.01f, 0.02f, 0.05f, 0.78f);

            GameObject window = new GameObject("HangarWindow");
            window.transform.SetParent(shopOverlay.transform, false);
            RectTransform wrt = window.AddComponent<RectTransform>();
            wrt.anchorMin = new Vector2(0.5f, 0.5f);
            wrt.anchorMax = new Vector2(0.5f, 0.5f);
            wrt.sizeDelta = new Vector2(1140f, 740f);
            wrt.anchoredPosition = Vector2.zero;
            Image winImg = window.AddComponent<Image>();
            winImg.sprite = modalSprite;
            winImg.type = Image.Type.Sliced;

            CreateText("HeaderTitle", wrt, font, "11HOPEM LUNAR EQUIPMENT & AUTOMATION HANGAR", 22, FontStyle.Bold, new Color(0.35f, 0.90f, 1.0f), TextAnchor.MiddleLeft, new Vector2(30f, -20f), new Vector2(700f, 32f));
            balanceText = CreateText("BalanceReadout", wrt, font, "BALANCE: LC 0", 22, FontStyle.Bold, new Color(0.28f, 1.0f, 0.58f), TextAnchor.MiddleRight, new Vector2(760f, -20f), new Vector2(350f, 32f));
            CreateText("SubHeader", wrt, font, "Unlock industrial sweepers, suit silos, conveyor lines, and autonomous lunar cleaner drones.", 13, FontStyle.Normal, new Color(0.68f, 0.80f, 0.92f), TextAnchor.MiddleLeft, new Vector2(30f, -52f), new Vector2(900f, 22f));

            GameObject gridObj = new GameObject("UpgradesGrid");
            gridObj.transform.SetParent(wrt, false);
            RectTransform grt = gridObj.AddComponent<RectTransform>();
            grt.anchorMin = new Vector2(0.5f, 1f);
            grt.anchorMax = new Vector2(0.5f, 1f);
            grt.pivot = new Vector2(0.5f, 1f);
            grt.anchoredPosition = new Vector2(0f, -88f);
            grt.sizeDelta = new Vector2(1080f, 560f);

            GridLayoutGroup grid = gridObj.AddComponent<GridLayoutGroup>();
            grid.cellSize = new Vector2(530f, 128f);
            grid.spacing = new Vector2(20f, 14f);
            grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            grid.constraintCount = 2;

            ShopUpgradeItem[] items = ShopManager.Instance != null ? ShopManager.Instance.GetAllUpgrades() : new ShopUpgradeItem[0];
            for (int i = 0; i < items.Length; i++)
            {
                ShopUpgradeItem item = items[i];
                GameObject card = new GameObject($"Card_{item.id}");
                card.transform.SetParent(grt, false);
                Image cImg = card.AddComponent<Image>();
                cImg.sprite = itemCardSprite;
                cImg.type = Image.Type.Sliced;

                RectTransform crt = card.GetComponent<RectTransform>();
                Text catTxt = CreateText("Category", crt, font, item.category, 11, FontStyle.Bold, new Color(0.38f, 0.84f, 1.0f), TextAnchor.MiddleLeft, new Vector2(18f, -12f), new Vector2(340f, 16f));
                Text titleTxt = CreateText("Title", crt, font, item.title, 16, FontStyle.Bold, Color.white, TextAnchor.MiddleLeft, new Vector2(18f, -31f), new Vector2(355f, 24f));
                Text descTxt = CreateText("Description", crt, font, item.description, 12, FontStyle.Normal, new Color(0.75f, 0.83f, 0.92f), TextAnchor.UpperLeft, new Vector2(18f, -58f), new Vector2(355f, 56f));
                Text priceTxt = CreateText("Price", crt, font, $"LC {item.priceCredits:N0}", 16, FontStyle.Bold, new Color(0.30f, 1.0f, 0.58f), TextAnchor.MiddleRight, new Vector2(375f, -24f), new Vector2(136f, 24f));

                GameObject btnObj = new GameObject("UpgradeBtn");
                btnObj.transform.SetParent(crt, false);
                RectTransform brt = btnObj.AddComponent<RectTransform>();
                brt.anchorMin = new Vector2(0f, 1f);
                brt.anchorMax = new Vector2(0f, 1f);
                brt.pivot = new Vector2(0f, 1f);
                brt.anchoredPosition = new Vector2(382f, -64f);
                brt.sizeDelta = new Vector2(130f, 42f);

                Image bImg = btnObj.AddComponent<Image>();
                bImg.sprite = btnSprite;
                bImg.type = Image.Type.Sliced;

                Button btn = btnObj.AddComponent<Button>();
                btn.targetGraphic = bImg;
                string capturedId = item.id;
                btn.onClick.AddListener(() => ShopManager.Instance?.PurchaseUpgrade(capturedId));

                Text btnLbl = CreateText("BtnLabel", brt, font, "UPGRADE", 13, FontStyle.Bold, Color.white, TextAnchor.MiddleCenter, Vector2.zero, new Vector2(130f, 42f));
                btnLbl.rectTransform.anchorMin = Vector2.zero;
                btnLbl.rectTransform.anchorMax = Vector2.one;
                btnLbl.rectTransform.offsetMin = Vector2.zero;
                btnLbl.rectTransform.offsetMax = Vector2.zero;

                cardWidgets.Add(new UpgradeCardWidgets
                {
                    item = item,
                    categoryText = catTxt,
                    titleText = titleTxt,
                    descText = descTxt,
                    priceText = priceTxt,
                    buyButton = btn,
                    buyButtonImage = bImg,
                    buyButtonLabel = btnLbl
                });
            }

            CreateFooterButton("SiteBtn", wrt, font, "11hopem.com", btnSprite, new Vector2(30f, -674f), new Vector2(170f, 42f), new Color(0.14f, 0.30f, 0.48f, 0.95f))
                .onClick.AddListener(() => Application.OpenURL("https://11hopem.com"));

            CreateFooterButton("CloseHangarBtn", wrt, font, "RETURN TO SURFACE [TAB / ESC]", btnSprite, new Vector2(820f, -674f), new Vector2(290f, 42f), new Color(0.18f, 0.56f, 0.86f, 0.95f))
                .onClick.AddListener(() => GameManager.Instance?.SetShopOpen(false));

            shopOverlay.SetActive(false);
        }

        private void RefreshCards()
        {
            long credits = GameManager.Instance != null ? GameManager.Instance.LunarCredits : 0;
            if (balanceText != null)
            {
                balanceText.text = $"BALANCE: LC {credits:N0}";
            }

            for (int i = 0; i < cardWidgets.Count; i++)
            {
                UpgradeCardWidgets w = cardWidgets[i];
                ShopUpgradeItem item = w.item;

                if (item.repeatable)
                {
                    w.titleText.text = $"{item.title} ({item.purchaseCount}/{item.maxPurchases})";
                }
                else
                {
                    w.titleText.text = item.title;
                }

                if (item.isPurchased)
                {
                    w.priceText.text = "UNLOCKED";
                    w.priceText.color = new Color(0.35f, 0.92f, 1.0f);
                    w.buyButton.interactable = false;
                    w.buyButtonImage.color = new Color(0.14f, 0.24f, 0.34f, 0.85f);
                    w.buyButtonLabel.text = "OWNED";
                    w.buyButtonLabel.color = new Color(0.62f, 0.78f, 0.90f);
                }
                else
                {
                    w.priceText.text = $"LC {item.priceCredits:N0}";
                    bool affordable = credits >= item.priceCredits;
                    w.priceText.color = affordable ? new Color(0.30f, 1.0f, 0.58f) : new Color(0.95f, 0.52f, 0.45f);
                    w.buyButton.interactable = affordable;
                    w.buyButtonImage.color = affordable ? new Color(0.16f, 0.72f, 0.46f, 0.98f) : new Color(0.18f, 0.22f, 0.28f, 0.88f);
                    w.buyButtonLabel.text = affordable ? "UPGRADE" : "NEED LC";
                    w.buyButtonLabel.color = affordable ? Color.white : new Color(0.55f, 0.62f, 0.70f);
                }
            }
        }

        private static Text CreateText(string name, Transform parent, Font font, string text, int size, FontStyle style, Color col, TextAnchor align, Vector2 pos, Vector2 dims)
        {
            GameObject go = new GameObject(name);
            go.transform.SetParent(parent, false);
            RectTransform rt = go.AddComponent<RectTransform>();
            rt.anchorMin = new Vector2(0f, 1f);
            rt.anchorMax = new Vector2(0f, 1f);
            rt.pivot = new Vector2(0f, 1f);
            rt.anchoredPosition = pos;
            rt.sizeDelta = dims;

            Text t = go.AddComponent<Text>();
            t.font = font;
            t.text = text;
            t.fontSize = size;
            t.fontStyle = style;
            t.color = col;
            t.alignment = align;
            t.horizontalOverflow = HorizontalWrapMode.Wrap;
            t.verticalOverflow = VerticalWrapMode.Overflow;
            t.raycastTarget = false;
            return t;
        }

        private static Button CreateFooterButton(string name, Transform parent, Font font, string label, Sprite sprite, Vector2 pos, Vector2 dims, Color bgCol)
        {
            GameObject go = new GameObject(name);
            go.transform.SetParent(parent, false);
            RectTransform rt = go.AddComponent<RectTransform>();
            rt.anchorMin = new Vector2(0f, 1f);
            rt.anchorMax = new Vector2(0f, 1f);
            rt.pivot = new Vector2(0f, 1f);
            rt.anchoredPosition = pos;
            rt.sizeDelta = dims;

            Image img = go.AddComponent<Image>();
            img.sprite = sprite;
            img.type = Image.Type.Sliced;
            img.color = bgCol;

            Button btn = go.AddComponent<Button>();
            btn.targetGraphic = img;

            Text lbl = CreateText("Label", rt, font, label, 13, FontStyle.Bold, Color.white, TextAnchor.MiddleCenter, Vector2.zero, dims);
            lbl.rectTransform.anchorMin = Vector2.zero;
            lbl.rectTransform.anchorMax = Vector2.one;
            lbl.rectTransform.offsetMin = Vector2.zero;
            lbl.rectTransform.offsetMax = Vector2.zero;
            return btn;
        }
    }
}
