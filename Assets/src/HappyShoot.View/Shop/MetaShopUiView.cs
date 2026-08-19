using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using HappyShoot.Domain.Meta;

namespace HappyShoot.View.Shop
{
    /// <summary>
    /// PlayerPrefs storage implementation for saving meta progression data in Unity.
    /// </summary>
    public class JsonPlayerPrefsStorage : ISaveStorage
    {
        private const string SaveKey = "HappyShoot_MetaSave";

        public MetaUpgradeSaveData Load()
        {
            if (PlayerPrefs.HasKey(SaveKey))
            {
                string json = PlayerPrefs.GetString(SaveKey);
                try
                {
                    return JsonUtility.FromJson<MetaUpgradeSaveData>(json);
                }
                catch
                {
                    // Fallback to fresh data if corrupted
                }
            }
            return new MetaUpgradeSaveData();
        }

        public void Save(MetaUpgradeSaveData data)
        {
            if (data == null) return;
            string json = JsonUtility.ToJson(data);
            PlayerPrefs.SetString(SaveKey, json);
            PlayerPrefs.Save();
        }
    }

    /// <summary>
    /// Master Lobby & Pause Meta Shop UI View for purchasing permanent stat upgrades with 100% refund support.
    /// </summary>
    public class MetaShopUiView : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private GameObject _panelRoot;
        [SerializeField] private Text _totalGoldText;
        [SerializeField] private Button _refundAllButton;
        [SerializeField] private Button _closeButton;

        private MetaShopManager _shopManager;
        private readonly List<(string id, Text descText, Text costText, Button buyBtn)> _cardViews = new List<(string, Text, Text, Button)>();

        public MetaShopManager ShopManager => _shopManager;

        public void Initialize(MetaShopManager shopManager = null)
        {
            _shopManager = shopManager ?? new MetaShopManager(new JsonPlayerPrefsStorage());
            _shopManager.OnShopStateChanged += RefreshUI;

            EnsureUiElements();
            RefreshUI();

            if (_panelRoot != null)
            {
                _panelRoot.SetActive(false);
            }
        }

        public void ShowShop()
        {
            if (_panelRoot != null)
            {
                _panelRoot.SetActive(true);
            }
            RefreshUI();
        }

        public void HideShop()
        {
            if (_panelRoot != null)
            {
                _panelRoot.SetActive(false);
            }
        }

        public void RefreshUI()
        {
            if (_shopManager == null) return;

            if (_totalGoldText != null)
            {
                _totalGoldText.text = $"💰 AVAILABLE GOLD: {_shopManager.TotalGold:N0}";
            }

            for (int i = 0; i < _cardViews.Count; i++)
            {
                var (id, descText, costText, buyBtn) = _cardViews[i];
                if (_shopManager.Definitions.TryGetValue(id, out var def))
                {
                    int currentLevel = _shopManager.SaveData.GetLevel(id);
                    bool isMax = currentLevel >= def.MaxLevel;
                    int cost = isMax ? 0 : def.GetCostForLevel(currentLevel);

                    if (descText != null)
                    {
                        descText.text = isMax
                            ? $"{def.Name} (MAX Lv.{def.MaxLevel})\n{def.Description}"
                            : $"{def.Name} (Lv.{currentLevel}/{def.MaxLevel})\n{def.Description}";
                    }

                    if (costText != null)
                    {
                        costText.text = isMax ? "MAXED" : $"💰 {cost}";
                    }

                    if (buyBtn != null)
                    {
                        buyBtn.interactable = !isMax && _shopManager.TotalGold >= cost;
                    }
                }
            }
        }

        private void TryBuyUpgrade(string upgradeId)
        {
            if (_shopManager != null)
            {
                _shopManager.TryPurchaseUpgrade(upgradeId);
            }
        }

        private void OnRefundAllClicked()
        {
            if (_shopManager != null)
            {
                _shopManager.RefundAll();
            }
        }

        private void EnsureUiElements()
        {
            if (_panelRoot != null) return;

            var canvasGo = new GameObject("MetaShopCanvas");
            var canvas = canvasGo.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 95;
            canvasGo.AddComponent<CanvasScaler>().uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            canvasGo.AddComponent<GraphicRaycaster>();

            _panelRoot = new GameObject("MetaShopPanel");
            _panelRoot.transform.SetParent(canvasGo.transform, false);
            var panelRt = _panelRoot.AddComponent<RectTransform>();
            panelRt.anchorMin = Vector2.zero;
            panelRt.anchorMax = Vector2.one;
            panelRt.sizeDelta = Vector2.zero;
            _panelRoot.AddComponent<Image>().color = new Color(0.08f, 0.09f, 0.14f, 0.96f);

            var dialogGo = new GameObject("ShopDialog");
            dialogGo.transform.SetParent(_panelRoot.transform, false);
            var dialogRt = dialogGo.AddComponent<RectTransform>();
            dialogRt.anchorMin = new Vector2(0.5f, 0.5f);
            dialogRt.anchorMax = new Vector2(0.5f, 0.5f);
            dialogRt.sizeDelta = new Vector2(720f, 620f);
            dialogGo.AddComponent<Image>().color = new Color(0.14f, 0.16f, 0.22f, 0.98f);

            // Title
            CreateText(dialogGo.transform, "Title", "🏛️ PERMANENT POWER UP SHOP 🏛️", 22, TextAnchor.MiddleCenter, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -30f), new Vector2(500f, 32f), new Color(1f, 0.85f, 0.3f, 1f));

            // Gold Text
            _totalGoldText = CreateText(dialogGo.transform, "GoldText", "💰 AVAILABLE GOLD: 0", 18, TextAnchor.MiddleCenter, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -65f), new Vector2(400f, 26f), Color.white);

            // Grid Container for 8 cards
            var gridGo = new GameObject("GridContainer");
            gridGo.transform.SetParent(dialogGo.transform, false);
            var gridRt = gridGo.AddComponent<RectTransform>();
            gridRt.anchorMin = new Vector2(0.5f, 0.5f);
            gridRt.anchorMax = new Vector2(0.5f, 0.5f);
            gridRt.anchoredPosition = new Vector2(0f, 10f);
            gridRt.sizeDelta = new Vector2(660f, 400f);

            var gridLayout = gridGo.AddComponent<GridLayoutGroup>();
            gridLayout.cellSize = new Vector2(310f, 90f);
            gridLayout.spacing = new Vector2(20f, 10f);
            gridLayout.childAlignment = TextAnchor.MiddleCenter;

            // Create cards for each registered upgrade
            foreach (var kvp in _shopManager.Definitions)
            {
                string id = kvp.Key;
                var def = kvp.Value;
                CreateUpgradeCard(gridGo.transform, id, def);
            }

            // Bottom Buttons: Refund All & Close
            _refundAllButton = CreateButton(dialogGo.transform, "RefundBtn", "🔄 REFUND ALL (100%)", new Vector2(-150f, -260f), new Vector2(240f, 45f), new Color(0.8f, 0.25f, 0.25f, 1f), OnRefundAllClicked);
            _closeButton = CreateButton(dialogGo.transform, "CloseBtn", "❌ CLOSE", new Vector2(150f, -260f), new Vector2(200f, 45f), new Color(0.35f, 0.4f, 0.5f, 1f), HideShop);
        }

        private void CreateUpgradeCard(Transform parent, string id, MetaUpgradeDefinition def)
        {
            var cardGo = new GameObject($"Card_{id}");
            cardGo.transform.SetParent(parent, false);
            cardGo.AddComponent<Image>().color = new Color(0.20f, 0.22f, 0.30f, 0.95f);

            var desc = CreateText(cardGo.transform, "Desc", $"{def.Name}\n{def.Description}", 13, TextAnchor.MiddleLeft, new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(12f, 0f), new Vector2(180f, 70f), Color.white);

            var buyBtn = CreateButton(cardGo.transform, "BuyBtn", "UPGRADE", new Vector2(95f, 0f), new Vector2(95f, 45f), new Color(0.2f, 0.65f, 0.35f, 1f), () => TryBuyUpgrade(id));
            var costTxt = CreateText(buyBtn.transform, "Cost", "💰 100", 12, TextAnchor.MiddleCenter, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0f, 8f), new Vector2(90f, 16f), new Color(1f, 0.85f, 0.3f, 1f));

            _cardViews.Add((id, desc, costTxt, buyBtn));
        }

        private Button CreateButton(Transform parent, string name, string label, Vector2 anchoredPos, Vector2 sizeDelta, Color btnColor, UnityEngine.Events.UnityAction onClick)
        {
            var btnGo = new GameObject(name);
            btnGo.transform.SetParent(parent, false);
            var rt = btnGo.AddComponent<RectTransform>();
            rt.anchorMin = new Vector2(0.5f, 0.5f);
            rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.anchoredPosition = anchoredPos;
            rt.sizeDelta = sizeDelta;

            var img = btnGo.AddComponent<Image>();
            img.color = btnColor;

            var btn = btnGo.AddComponent<Button>();
            btn.targetGraphic = img;
            btn.onClick.AddListener(onClick);

            CreateText(btnGo.transform, "Label", label, 14, TextAnchor.MiddleCenter, Vector2.zero, Vector2.one, new Vector2(0.5f, 0.5f), Vector2.zero, Vector2.zero, Color.white);

            return btn;
        }

        private Text CreateText(Transform parent, string name, string defaultText, int fontSize, TextAnchor alignment, Vector2 anchorMin, Vector2 anchorMax, Vector2 pivot, Vector2 anchoredPos, Vector2 sizeDelta, Color color)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            var rt = go.AddComponent<RectTransform>();
            rt.anchorMin = anchorMin;
            rt.anchorMax = anchorMax;
            rt.pivot = pivot;
            rt.anchoredPosition = anchoredPos;
            rt.sizeDelta = sizeDelta;
            var txt = go.AddComponent<Text>();
            txt.text = defaultText;
            txt.fontSize = fontSize;
            txt.fontStyle = FontStyle.Bold;
            txt.alignment = alignment;
            txt.color = color;
            txt.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            return txt;
        }

        private void OnDestroy()
        {
            if (_shopManager != null)
            {
                _shopManager.OnShopStateChanged -= RefreshUI;
            }
        }
    }
}
