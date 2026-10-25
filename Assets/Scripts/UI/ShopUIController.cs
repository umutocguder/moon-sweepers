using UnityEngine;
using ElevenHopem.MoonSweepers.Core;
using ElevenHopem.MoonSweepers.Economy;

namespace ElevenHopem.MoonSweepers.UI
{
    public class ShopUIController : MonoBehaviour
    {
        private Vector2 scrollPos;
        private GUIStyle windowStyle;
        private GUIStyle titleStyle;
        private GUIStyle itemTitleStyle;
        private GUIStyle descStyle;
        private GUIStyle priceStyle;
        private GUIStyle buttonStyle;
        private GUIStyle disabledButtonStyle;

        private void InitStyles()
        {
            if (windowStyle != null) return;

            windowStyle = new GUIStyle(GUI.skin.window)
            {
                fontStyle = FontStyle.Bold
            };

            titleStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 20,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleCenter,
                normal = { textColor = new Color(0.35f, 0.8f, 1f) }
            };

            itemTitleStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 15,
                fontStyle = FontStyle.Bold,
                normal = { textColor = Color.white }
            };

            descStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 12,
                wordWrap = true,
                normal = { textColor = new Color(0.8f, 0.8f, 0.85f) }
            };

            priceStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 14,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleRight,
                normal = { textColor = new Color(0.3f, 1f, 0.5f) }
            };

            buttonStyle = new GUIStyle(GUI.skin.button)
            {
                fontSize = 13,
                fontStyle = FontStyle.Bold
            };

            disabledButtonStyle = new GUIStyle(GUI.skin.button)
            {
                fontSize = 13,
                normal = { textColor = Color.gray }
            };
        }

        private void OnGUI()
        {
            if (GameManager.Instance == null || !GameManager.Instance.IsShopOpen) return;

            InitStyles();

            float windowWidth = 620;
            float windowHeight = 480;
            float x = (Screen.width - windowWidth) / 2f;
            float y = (Screen.height - windowHeight) / 2f;

            GUI.Window(999, new Rect(x, y, windowWidth, windowHeight), DrawShopWindow, "11Hopem Lunar Hangar & Upgrades");
        }

        private void DrawShopWindow(int windowId)
        {
            long credits = GameManager.Instance != null ? GameManager.Instance.LunarCredits : 0;

            GUILayout.BeginVertical();
            GUILayout.Space(8);

            GUILayout.Label("LUNAR EQUIPMENT HANGAR", titleStyle);
            GUILayout.Label($"Your Balance: LC ${credits:N0}   |   Brand: 11Hopem", priceStyle);
            GUILayout.Space(6);

            scrollPos = GUILayout.BeginScrollView(scrollPos, GUILayout.Height(340));

            ShopUpgradeItem[] items = ShopManager.Instance != null ? ShopManager.Instance.GetAllUpgrades() : new ShopUpgradeItem[0];

            foreach (var item in items)
            {
                GUILayout.BeginHorizontal("box");

                GUILayout.BeginVertical(GUILayout.Width(380));
                GUILayout.Label(item.title, itemTitleStyle);
                GUILayout.Label(item.description, descStyle);
                GUILayout.EndVertical();

                GUILayout.FlexibleSpace();

                GUILayout.BeginVertical(GUILayout.Width(130));
                if (item.isPurchased)
                {
                    GUILayout.Label("PURCHASED", priceStyle);
                    GUI.enabled = false;
                    GUILayout.Button("Owned", disabledButtonStyle, GUILayout.Height(28));
                    GUI.enabled = true;
                }
                else
                {
                    GUILayout.Label($"LC ${item.priceCredits:N0}", priceStyle);
                    bool canAfford = credits >= item.priceCredits;
                    GUI.enabled = canAfford;
                    if (GUILayout.Button(canAfford ? "Upgrade" : "Too Expensive", buttonStyle, GUILayout.Height(28)))
                    {
                        ShopManager.Instance?.PurchaseUpgrade(item.id);
                    }
                    GUI.enabled = true;
                }
                GUILayout.EndVertical();

                GUILayout.EndHorizontal();
                GUILayout.Space(4);
            }

            GUILayout.EndScrollView();

            GUILayout.FlexibleSpace();

            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Visit 11hopem.com", GUILayout.Width(150), GUILayout.Height(30)))
            {
                Application.OpenURL("https://11hopem.com");
            }
            GUILayout.FlexibleSpace();
            if (GUILayout.Button("Close Hangar [TAB / ESC]", GUILayout.Width(180), GUILayout.Height(30)))
            {
                GameManager.Instance?.SetShopOpen(false);
            }
            GUILayout.EndHorizontal();

            GUILayout.EndVertical();
        }
    }
}
