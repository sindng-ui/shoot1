using System;
using UnityEngine;
using UnityEngine.UI;
using HappyShoot.Domain.Forge;
using HappyShoot.Domain.Progression;

namespace HappyShoot.View.Forge
{
    /// <summary>
    /// Magic Forge main UI popup.
    /// Provides 3 tabs (Rune Inscription, Crystal Workbench, Skill Reforge),
    /// wallet balance display, and close handling.
    /// Strictly modular and under 500 lines.
    /// </summary>
    public class MagicForgeUiView : MonoBehaviour
    {
        private RuneManager _runeManager;
        private SkillTreeManager _walletManager;
        private JsonForgeStorage _forgeStorage;

        private GameObject _canvasGo;
        private GameObject _panelRoot;
        private RuneInscriptionTabView _runeTabView;
        private Text _walletGemsText;
        private Text _walletGoldText;

        public bool IsOpen => _canvasGo != null && _canvasGo.activeSelf;

        public void Initialize(RuneManager runeManager, SkillTreeManager walletManager, JsonForgeStorage forgeStorage)
        {
            _runeManager = runeManager;
            _walletManager = walletManager;
            _forgeStorage = forgeStorage;

            BuildUI();
            Hide();
        }

        private void BuildUI()
        {
            if (_canvasGo != null) return;

            // ── Canvas Setup (ScreenSpaceOverlay, sortingOrder: 90) ──
            _canvasGo = new GameObject("MagicForgeCanvas");
            _canvasGo.transform.SetParent(transform, false);

            var canvas = _canvasGo.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 90; // Above CharacterSelect (85), below SkillTree (200)

            var scaler = _canvasGo.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            scaler.matchWidthOrHeight = 0.5f;

            _canvasGo.AddComponent<GraphicRaycaster>();

            // Fullscreen translucent backdrop (click to close)
            var backdrop = CreatePanel(_canvasGo.transform, "Backdrop", Vector2.zero, Vector2.zero);
            var backRt = backdrop.GetComponent<RectTransform>();
            backRt.anchorMin = Vector2.zero;
            backRt.anchorMax = Vector2.one;
            backRt.offsetMin = Vector2.zero;
            backRt.offsetMax = Vector2.zero;
            backdrop.GetComponent<Image>().color = new Color(0f, 0f, 0f, 0.7f);

            var backBtn = backdrop.AddComponent<Button>();
            backBtn.onClick.AddListener(Hide);

            // Centered Modal Dialog Panel (1600x920 - Expansive Mobile-Ready Layout)
            _panelRoot = CreatePanel(_canvasGo.transform, "MagicForgePanel", Vector2.zero, new Vector2(1600f, 920f));
            var img = _panelRoot.GetComponent<Image>();
            img.color = new Color(0.06f, 0.07f, 0.10f, 0.98f);

            // ── Header (Title & Close button) ──
            var headerGo = CreatePanel(_panelRoot.transform, "Header", new Vector2(0f, 420f), new Vector2(1560f, 56f));
            headerGo.GetComponent<Image>().color = new Color(0.12f, 0.14f, 0.20f, 0.95f);

            CreateText(headerGo.transform, "Title", "⚒️ 마법 대장간 (Magic Forge)", 24, FontStyle.Bold,
                new Vector2(-300f, 0f), new Vector2(600f, 44f), new Color(1f, 0.85f, 0.35f));

            var closeBtn = CreateButton(headerGo.transform, "CloseBtn", new Vector2(740f, 0f), new Vector2(48f, 44f),
                new Color(0.7f, 0.2f, 0.2f, 1f));
            closeBtn.GetComponent<Button>().onClick.AddListener(Hide);
            CreateText(closeBtn.transform, "Label", "✕", 22, FontStyle.Bold, Vector2.zero, new Vector2(48f, 44f), Color.white);

            // ── Tab Buttons Row ──
            var tabRow = CreatePanel(_panelRoot.transform, "TabRow", new Vector2(0f, 360f), new Vector2(1560f, 46f));
            tabRow.GetComponent<Image>().color = Color.clear;

            // Tab 1: Rune Inscription (Active)
            var tab1Btn = CreateButton(tabRow.transform, "Tab1", new Vector2(-380f, 0f), new Vector2(360f, 44f),
                new Color(0.25f, 0.45f, 0.85f, 1f));
            CreateText(tab1Btn.transform, "Label", "🔮 룬 각인소 (1단계)", 18, FontStyle.Bold, Vector2.zero, new Vector2(360f, 44f), Color.white);

            // Tab 2: Crystal Workbench (Phase 2 Coming Soon)
            var tab2Btn = CreateButton(tabRow.transform, "Tab2", new Vector2(0f, 0f), new Vector2(360f, 44f),
                new Color(0.20f, 0.22f, 0.30f, 0.8f));
            CreateText(tab2Btn.transform, "Label", "💎 마법 결정체 (2단계)", 16, FontStyle.Normal, Vector2.zero, new Vector2(360f, 44f), new Color(0.7f, 0.7f, 0.8f));

            // Tab 3: Skill Reforge (Phase 3 Coming Soon)
            var tab3Btn = CreateButton(tabRow.transform, "Tab3", new Vector2(380f, 0f), new Vector2(360f, 44f),
                new Color(0.20f, 0.22f, 0.30f, 0.8f));
            CreateText(tab3Btn.transform, "Label", "🔥 스킬 재련 (3단계)", 16, FontStyle.Normal, Vector2.zero, new Vector2(360f, 44f), new Color(0.7f, 0.7f, 0.8f));

            // ── Content Area (Spacious 1560x680) ──
            var contentArea = CreatePanel(_panelRoot.transform, "ContentArea", new Vector2(0f, -15f), new Vector2(1560f, 680f));
            contentArea.GetComponent<Image>().color = Color.clear;

            // Instantiate Tab 1 (Rune Inscription)
            var runeTabGo = new GameObject("RuneInscriptionTab", typeof(RectTransform));
            runeTabGo.transform.SetParent(contentArea.transform, false);
            _runeTabView = runeTabGo.AddComponent<RuneInscriptionTabView>();
            _runeTabView.Initialize(_runeManager, _walletManager, OnDataChanged);

            // ── Bottom Wallet Footer ──
            var footerGo = CreatePanel(_panelRoot.transform, "Footer", new Vector2(0f, -420f), new Vector2(1560f, 54f));
            footerGo.GetComponent<Image>().color = new Color(0.10f, 0.11f, 0.16f, 0.95f);

            _walletGemsText = CreateText(footerGo.transform, "Gems", "💎 보유 보석: 루비 0개  에메랄드 0개  아메시스트 0개", 18, FontStyle.Bold,
                new Vector2(-260f, 0f), new Vector2(880f, 40f), Color.white);

            _walletGoldText = CreateText(footerGo.transform, "Gold", "💰 보유 골드: 0 G", 18, FontStyle.Bold,
                new Vector2(480f, 0f), new Vector2(360f, 40f), new Color(1.0f, 0.85f, 0.3f));
        }

        public void Show()
        {
            if (_canvasGo != null)
            {
                _canvasGo.SetActive(true);
                RefreshWallet();
                _runeTabView?.Refresh();
            }
        }

        public void Hide()
        {
            if (_canvasGo != null)
            {
                _canvasGo.SetActive(false);
            }
        }

        private void OnDataChanged()
        {
            if (_forgeStorage != null && _runeManager?.SaveData != null)
            {
                _forgeStorage.Save(_runeManager.SaveData);
            }
            RefreshWallet();
        }

        private void RefreshWallet()
        {
            if (_walletManager == null) return;

            var wallet = _walletManager.SaveData;
            int r = wallet.GetGems(GemType.Ruby);
            int e = wallet.GetGems(GemType.Emerald);
            int a = wallet.GetGems(GemType.Amethyst);
            int g = _walletManager.GetGoldCount();

            if (_walletGemsText != null)
            {
                _walletGemsText.text = $"💎 <color=#AAAAAA>보유 보석:</color>  <color=#FF5555>◆ 루비 {r}개</color>   <color=#44FF77>◆ 에메랄드 {e}개</color>   <color=#D477FF>◆ 아메시스트 {a}개</color>";
            }
            if (_walletGoldText != null)
            {
                _walletGoldText.text = $"💰 <color=#AAAAAA>보유 골드:</color> <color=#FFDD44>{g:N0} G</color>";
            }
        }

        // ── Helper Canvas UI Factories ──

        private GameObject CreatePanel(Transform parent, string name, Vector2 pos, Vector2 size)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            go.transform.SetParent(parent, false);
            var rt = go.GetComponent<RectTransform>();
            rt.anchoredPosition = pos;
            rt.sizeDelta = size;
            return go;
        }

        private GameObject CreateButton(Transform parent, string name, Vector2 pos, Vector2 size, Color color)
        {
            var go = CreatePanel(parent, name, pos, size);
            go.GetComponent<Image>().color = color;
            var btn = go.AddComponent<Button>();
            btn.targetGraphic = go.GetComponent<Image>();
            return go;
        }

        private Text CreateText(Transform parent, string name, string text, int fontSize, FontStyle style,
            Vector2 pos, Vector2 size, Color color)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Text));
            go.transform.SetParent(parent, false);
            var rt = go.GetComponent<RectTransform>();
            rt.anchoredPosition = pos;
            rt.sizeDelta = size;

            var t = go.GetComponent<Text>();
            t.text = text;
            t.font = Utils.FontHelper.GetKoreanFont();
            t.fontSize = fontSize;
            t.fontStyle = style;
            t.alignment = TextAnchor.MiddleCenter;
            t.color = color;
            t.supportRichText = true;
            return t;
        }
    }
}
