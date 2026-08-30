using System;
using UnityEngine;
using UnityEngine.UI;
using HappyShoot.Domain.Entities;
using HappyShoot.Domain.Progression;
using HappyShoot.View.SkillTree;
using HappyShoot.View.Utils;

namespace HappyShoot.View.UI
{
    /// <summary>
    /// Wizard-Only Main Menu Screen.
    /// Displays a large wizard avatar preview (with equipped gear),
    /// a single "Game Start" button, and a "Magic Forge" entry button.
    /// Dev tools (Dev Mode, Sandbox, Settings, Quit) remain at the bottom.
    /// </summary>
    public class CharacterSelectUiView : MonoBehaviour
    {
        private GameObject _panelRoot;
        private Action<CharacterClassType, bool, bool, int, string> _onSelectedCallback;
        private StartSkillSelectorView _startSkillSelector;
        private SettingsDialogUiView _settingsDialog;
        private SkillTreeUiView _skillTreeUiView;
        private bool _isDevMode = false;
        private bool _isSkillTestMode = false;
        private int _startPhase = 1;
        private Text _devModeBtnText;
        private Image _devModeBtnImg;
        private Text _skillTestBtnText;
        private Image _skillTestBtnImg;
        private GameObject _phaseBtnGo;
        private Text _phaseBtnText;

        // Magic Forge UI callback (connected by GameBootstrap when Phase 2 is ready)
        private Action _onMagicForgeCallback;
        private SkillTreeManager _skillTreeManager;

        public void SetSettingsDialog(SettingsDialogUiView dialog) { _settingsDialog = dialog; }
        public void SetSkillTreeUiView(SkillTreeUiView treeView) { _skillTreeUiView = treeView; }
        public void SetSkillTreeManager(SkillTreeManager manager) { _skillTreeManager = manager; }
        public void SetMagicForgeCallback(Action callback) { _onMagicForgeCallback = callback; }

        public void Initialize(Action<CharacterClassType, bool, bool, int, string> onSelectedCallback)
        {
            _onSelectedCallback = onSelectedCallback;
            EnsureUiElements();
            ShowSelectScreen();
        }

        public void Initialize(Action<CharacterClassType, bool, bool, int> onSelectedCallback)
        {
            Initialize((classType, isDev, isTest, startPhase, startSkillId) =>
                onSelectedCallback?.Invoke(classType, isDev, isTest, startPhase));
        }

        public void Initialize(Action<CharacterClassType, bool, bool> onSelectedCallback)
        {
            Initialize((classType, isDev, isTest, startPhase, startSkillId) =>
                onSelectedCallback?.Invoke(classType, isDev, isTest));
        }

        public void Initialize(Action<CharacterClassType, bool> onSelectedCallback)
        {
            Initialize((classType, isDev, isTest, startPhase, startSkillId) =>
                onSelectedCallback?.Invoke(classType, isDev));
        }

        public void Initialize(Action<CharacterClassType> onSelectedCallback)
        {
            Initialize((classType, isDev, isTest, startPhase, startSkillId) =>
                onSelectedCallback?.Invoke(classType));
        }

        public void ShowSelectScreen()
        {
            if (_panelRoot != null)
            {
                _panelRoot.SetActive(true);
                Time.timeScale = 0f;
            }
        }

        public void HideSelectScreen()
        {
            if (_panelRoot != null)
            {
                _panelRoot.SetActive(false);
                Time.timeScale = 1f;
            }
        }

        private void StartGame()
        {
            PlayerPrefs.SetInt("SelectedHeroClass", (int)CharacterClassType.Wizard);
            PlayerPrefs.Save();
            HideSelectScreen();

            string startSkillId = _startSkillSelector != null ? _startSkillSelector.SelectedSkillId : "fireball";
            _onSelectedCallback?.Invoke(CharacterClassType.Wizard, _isDevMode, _isSkillTestMode, _startPhase, startSkillId);
        }

        private void OpenMagicForge()
        {
            if (_onMagicForgeCallback != null)
            {
                _onMagicForgeCallback.Invoke();
            }
            else
            {
                Debug.Log("[CharacterSelectUiView] Magic Forge coming soon! (Phase 2)");
            }
        }

        private void ToggleDevMode()
        {
            _isDevMode = !_isDevMode;
            if (_devModeBtnText != null)
                _devModeBtnText.text = _isDevMode ? "🛠️ 개발자 모드: ON" : "🛠️ 개발자 모드: OFF";
            if (_devModeBtnImg != null)
                _devModeBtnImg.color = _isDevMode ? new Color(0.85f, 0.45f, 0.15f, 1f) : new Color(0.25f, 0.28f, 0.35f, 0.95f);
            if (_phaseBtnGo != null)
                _phaseBtnGo.SetActive(_isDevMode);
        }

        private void ToggleStartPhase()
        {
            _startPhase = _startPhase switch { 1 => 2, 2 => 3, _ => 1 };
            if (_phaseBtnText != null)
            {
                _phaseBtnText.text = _startPhase switch
                {
                    2 => "🚀 시작: Phase 2 (독거미/흑기사)",
                    3 => "🚀 시작: Phase 3 (망령/3보스)",
                    _ => "🚀 시작: Phase 1 (기본 슬라임)"
                };
            }
        }

        private void ToggleSkillTestMode()
        {
            _isSkillTestMode = !_isSkillTestMode;
            if (_skillTestBtnText != null)
                _skillTestBtnText.text = _isSkillTestMode ? "🧪 밸런스 샌드박스: ON" : "🧪 밸런스 샌드박스: OFF";
            if (_skillTestBtnImg != null)
                _skillTestBtnImg.color = _isSkillTestMode ? new Color(0.20f, 0.75f, 0.45f, 1f) : new Color(0.18f, 0.26f, 0.32f, 0.95f);
        }

        private void QuitGame()
        {
            Debug.Log("[CharacterSelectUiView] Quitting Application...");
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }

        // ════════════════════════════════════════════════════════
        //  UI Construction
        // ════════════════════════════════════════════════════════

        private void EnsureUiElements()
        {
            if (_panelRoot != null) return;

            // EventSystem
            if (FindFirstObjectByType<UnityEngine.EventSystems.EventSystem>() == null)
            {
                var eventSystemGo = new GameObject("EventSystem");
                eventSystemGo.AddComponent<UnityEngine.EventSystems.EventSystem>();
                eventSystemGo.AddComponent<UnityEngine.InputSystem.UI.InputSystemUIInputModule>();
            }

            var canvasGo = new GameObject("CharacterSelectCanvas");
            var canvas = canvasGo.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 85;
            var scaler = canvasGo.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            scaler.matchWidthOrHeight = 0.5f;
            canvasGo.AddComponent<GraphicRaycaster>();

            // Full-screen dark backdrop
            _panelRoot = new GameObject("WizardMainMenuPanel");
            _panelRoot.transform.SetParent(canvasGo.transform, false);
            var panelRt = _panelRoot.AddComponent<RectTransform>();
            panelRt.anchorMin = Vector2.zero;
            panelRt.anchorMax = Vector2.one;
            panelRt.sizeDelta = Vector2.zero;
            _panelRoot.AddComponent<Image>().color = new Color(0.04f, 0.05f, 0.10f, 0.97f);

            BuildTitleSection();
            BuildWizardPreview();
            CompanionSelectPreviewHelper.CreateCompanionPreviewCards(_panelRoot.transform, _skillTreeManager);
            BuildMainButtons();
            BuildDevToolsRow();
            BuildSkillTreeButton();
            BuildVersionLabel();
        }

        // ── Title Section ──
        private void BuildTitleSection()
        {
            CreateText(_panelRoot.transform, "GameTitle", "🧙‍♂️ HappyShoot", 48,
                TextAnchor.MiddleCenter,
                new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f),
                new Vector2(0f, -50f), new Vector2(800f, 70f),
                new Color(0.85f, 0.72f, 1f, 1f));

            CreateText(_panelRoot.transform, "Subtitle", "마법사의 모험이 시작됩니다", 20,
                TextAnchor.MiddleCenter,
                new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f),
                new Vector2(0f, -115f), new Vector2(600f, 30f),
                new Color(0.70f, 0.78f, 0.92f, 0.8f));
        }

        // ── Large Wizard Preview ──
        private void BuildWizardPreview()
        {
            // Wizard card container (centered, large)
            var cardGo = new GameObject("WizardPreviewCard");
            cardGo.transform.SetParent(_panelRoot.transform, false);
            var cardRt = cardGo.AddComponent<RectTransform>();
            cardRt.anchorMin = new Vector2(0.5f, 0.5f);
            cardRt.anchorMax = new Vector2(0.5f, 0.5f);
            cardRt.pivot = new Vector2(0.5f, 0.5f);
            cardRt.anchoredPosition = new Vector2(0f, 65f);
            cardRt.sizeDelta = new Vector2(440f, 570f);
            cardGo.AddComponent<Image>().color = new Color(0.10f, 0.08f, 0.18f, 0.85f);

            // Purple accent border
            var borderGo = new GameObject("AccentBorder");
            borderGo.transform.SetParent(cardGo.transform, false);
            var borderRt = borderGo.AddComponent<RectTransform>();
            borderRt.anchorMin = Vector2.zero;
            borderRt.anchorMax = Vector2.one;
            borderRt.sizeDelta = Vector2.zero;
            borderGo.AddComponent<Image>().color = new Color(0.70f, 0.40f, 1.0f, 0.20f);

            // Wizard avatar
            var avatarGo = new GameObject("WizardAvatar");
            avatarGo.transform.SetParent(cardGo.transform, false);
            var avatarRt = avatarGo.AddComponent<RectTransform>();
            avatarRt.anchorMin = new Vector2(0.5f, 1f);
            avatarRt.anchorMax = new Vector2(0.5f, 1f);
            avatarRt.pivot = new Vector2(0.5f, 1f);
            avatarRt.anchoredPosition = new Vector2(0f, -15f);
            avatarRt.sizeDelta = new Vector2(170f, 170f);

            var avatarBgGo = new GameObject("AvatarBg");
            avatarBgGo.transform.SetParent(avatarGo.transform, false);
            var avatarBgRt = avatarBgGo.AddComponent<RectTransform>();
            avatarBgRt.anchorMin = Vector2.zero;
            avatarBgRt.anchorMax = Vector2.one;
            avatarBgRt.sizeDelta = Vector2.zero;
            avatarBgGo.AddComponent<Image>().color = new Color(0.06f, 0.04f, 0.12f, 0.9f);

            var iconGo = new GameObject("WizardIcon");
            iconGo.transform.SetParent(avatarGo.transform, false);
            var iconRt = iconGo.AddComponent<RectTransform>();
            iconRt.anchorMin = new Vector2(0.5f, 0.5f);
            iconRt.anchorMax = new Vector2(0.5f, 0.5f);
            iconRt.pivot = new Vector2(0.5f, 0.5f);
            iconRt.sizeDelta = new Vector2(150f, 150f);
            var iconImg = iconGo.AddComponent<Image>();
            iconImg.sprite = HeroSpriteHelper.GetHeroSprite(CharacterClassType.Wizard, HeroSpriteHelper.ViewDirection.Front, 32);
            iconImg.preserveAspect = true;

            // Class title
            CreateText(cardGo.transform, "ClassTitle", "🧙 대마법사", 26,
                TextAnchor.MiddleCenter,
                new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f),
                new Vector2(0f, -195f), new Vector2(360f, 32f),
                new Color(0.85f, 0.72f, 1f, 1f));

            // Stats description (Compact 2-lines)
            CreateText(cardGo.transform, "StatsDesc",
                "⚡ 쿨타임 -15%  |  🔮 스킬 범위 +20%\n✨ 공격력 +25%  |  🧲 자석 반경 3.0",
                14, TextAnchor.MiddleCenter,
                new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f),
                new Vector2(0f, -232f), new Vector2(380f, 42f),
                new Color(0.88f, 0.92f, 0.98f, 0.95f));

            // Starting skill selector UI
            _startSkillSelector = cardGo.AddComponent<StartSkillSelectorView>();
            _startSkillSelector.Initialize(cardGo.transform, new Vector2(0f, -282f));
        }

        // ── Main Action Buttons ──
        private void BuildMainButtons()
        {
            // ── 🔥 Game Start Button (large, centered below card) ──
            var startBtnGo = CreateButton(_panelRoot.transform, "BtnGameStart",
                new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0.5f, 0f),
                new Vector2(100f, 155f), new Vector2(340f, 64f),
                new Color(0.55f, 0.25f, 0.85f, 1f));
            startBtnGo.GetComponent<Button>().onClick.AddListener(StartGame);
            CreateText(startBtnGo.transform, "BtnText", "🔥 게임 시작", 24,
                TextAnchor.MiddleCenter,
                Vector2.zero, Vector2.one, new Vector2(0.5f, 0.5f),
                Vector2.zero, Vector2.zero, Color.white);

            // ── ⚒️ Magic Forge Button (left of start) ──
            var forgeBtnGo = CreateButton(_panelRoot.transform, "BtnMagicForge",
                new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0.5f, 0f),
                new Vector2(-200f, 155f), new Vector2(240f, 64f),
                new Color(0.75f, 0.45f, 0.15f, 1f));
            forgeBtnGo.GetComponent<Button>().onClick.AddListener(OpenMagicForge);
            CreateText(forgeBtnGo.transform, "BtnText", "⚒️ 마법 대장간", 20,
                TextAnchor.MiddleCenter,
                Vector2.zero, Vector2.one, new Vector2(0.5f, 0.5f),
                Vector2.zero, Vector2.zero, Color.white);
        }

        // ── Developer Tools Row ──
        private void BuildDevToolsRow()
        {
            float y = 35f;
            float btnW = 220f;
            float btnH = 46f;
            float gap = 235f;

            // Dev Mode Toggle
            var devBtnGo = CreateButton(_panelRoot.transform, "BtnToggleDevMode",
                new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0.5f, 0f),
                new Vector2(-gap * 1.5f, y), new Vector2(btnW, btnH),
                new Color(0.25f, 0.28f, 0.35f, 0.95f));
            _devModeBtnImg = devBtnGo.GetComponent<Image>();
            devBtnGo.GetComponent<Button>().onClick.AddListener(ToggleDevMode);
            _devModeBtnText = CreateText(devBtnGo.transform, "BtnText", "🛠️ 개발자 모드: OFF", 14,
                TextAnchor.MiddleCenter, Vector2.zero, Vector2.one, new Vector2(0.5f, 0.5f),
                Vector2.zero, Vector2.zero, Color.white);

            // Phase Toggle (visible only in dev mode)
            _phaseBtnGo = CreateButton(_panelRoot.transform, "BtnToggleStartPhase",
                new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0.5f, 0f),
                new Vector2(-gap * 1.5f, y + 54f), new Vector2(btnW, 38f),
                new Color(0.35f, 0.18f, 0.52f, 0.95f));
            _phaseBtnGo.GetComponent<Button>().onClick.AddListener(ToggleStartPhase);
            _phaseBtnText = CreateText(_phaseBtnGo.transform, "BtnText", "🚀 시작: Phase 1 (기본 슬라임)", 12,
                TextAnchor.MiddleCenter, Vector2.zero, Vector2.one, new Vector2(0.5f, 0.5f),
                Vector2.zero, Vector2.zero, Color.white);
            _phaseBtnGo.SetActive(_isDevMode);

            // Sandbox Toggle
            var testBtnGo = CreateButton(_panelRoot.transform, "BtnToggleSkillTest",
                new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0.5f, 0f),
                new Vector2(-gap * 0.5f, y), new Vector2(btnW, btnH),
                new Color(0.18f, 0.26f, 0.32f, 0.95f));
            _skillTestBtnImg = testBtnGo.GetComponent<Image>();
            testBtnGo.GetComponent<Button>().onClick.AddListener(ToggleSkillTestMode);
            _skillTestBtnText = CreateText(testBtnGo.transform, "BtnText", "🧪 밸런스 샌드박스: OFF", 14,
                TextAnchor.MiddleCenter, Vector2.zero, Vector2.one, new Vector2(0.5f, 0.5f),
                Vector2.zero, Vector2.zero, Color.white);

            // Settings
            var settingsBtnGo = CreateButton(_panelRoot.transform, "BtnOpenSettings",
                new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0.5f, 0f),
                new Vector2(gap * 0.5f, y), new Vector2(btnW, btnH),
                new Color(0.20f, 0.26f, 0.38f, 0.95f));
            settingsBtnGo.GetComponent<Button>().onClick.AddListener(() => _settingsDialog?.Show());
            CreateText(settingsBtnGo.transform, "BtnText", "⚙️ 게임 환경 설정", 14,
                TextAnchor.MiddleCenter, Vector2.zero, Vector2.one, new Vector2(0.5f, 0.5f),
                Vector2.zero, Vector2.zero, Color.white);

            // Quit
            var quitBtnGo = CreateButton(_panelRoot.transform, "BtnQuitGame",
                new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0.5f, 0f),
                new Vector2(gap * 1.5f, y), new Vector2(btnW, btnH),
                new Color(0.45f, 0.16f, 0.20f, 0.95f));
            quitBtnGo.GetComponent<Button>().onClick.AddListener(QuitGame);
            CreateText(quitBtnGo.transform, "BtnText", "🚪 게임 종료", 14,
                TextAnchor.MiddleCenter, Vector2.zero, Vector2.one, new Vector2(0.5f, 0.5f),
                Vector2.zero, Vector2.zero, Color.white);
        }

        // ── Skill Tree Button (Top Right, Locked) ──
        private void BuildSkillTreeButton()
        {
            var treeBtnGo = CreateButton(_panelRoot.transform, "BtnOpenSkillTree",
                new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(1f, 1f),
                new Vector2(-40f, -40f), new Vector2(260f, 54f),
                new Color(0.25f, 0.25f, 0.30f, 0.85f));
            treeBtnGo.GetComponent<Button>().interactable = false;
            CreateText(treeBtnGo.transform, "BtnText", "🔒 3보스 클리어 시 영구성장 개방", 15,
                TextAnchor.MiddleCenter, Vector2.zero, Vector2.one, new Vector2(0.5f, 0.5f),
                Vector2.zero, Vector2.zero, new Color(0.75f, 0.75f, 0.75f, 0.9f));
        }

        // ── Version Label ──
        private void BuildVersionLabel()
        {
            CreateText(_panelRoot.transform, "VersionLabel",
                HappyShoot.Domain.Common.AppVersion.FullVersionText, 14,
                TextAnchor.LowerRight,
                new Vector2(1f, 0f), new Vector2(1f, 0f), new Vector2(1f, 0f),
                new Vector2(-24f, 15f), new Vector2(300f, 24f),
                new Color(0.6f, 0.7f, 0.8f, 0.6f));
        }

        // ════════════════════════════════════════════════════════
        //  UI Helpers
        // ════════════════════════════════════════════════════════

        private GameObject CreateButton(Transform parent, string name,
            Vector2 anchorMin, Vector2 anchorMax, Vector2 pivot,
            Vector2 anchoredPos, Vector2 sizeDelta, Color bgColor)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            var rt = go.AddComponent<RectTransform>();
            rt.anchorMin = anchorMin;
            rt.anchorMax = anchorMax;
            rt.pivot = pivot;
            rt.anchoredPosition = anchoredPos;
            rt.sizeDelta = sizeDelta;
            var img = go.AddComponent<Image>();
            img.sprite = SpriteHelper.GetOrCreateWhiteSprite();
            img.color = bgColor;
            var btn = go.AddComponent<Button>();
            btn.targetGraphic = img;
            return go;
        }

        private Text CreateText(Transform parent, string name, string defaultText,
            int fontSize, TextAnchor alignment,
            Vector2 anchorMin, Vector2 anchorMax, Vector2 pivot,
            Vector2 anchoredPos, Vector2 sizeDelta, Color color)
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
            txt.raycastTarget = false;
            txt.font = FontHelper.GetKoreanFont();
            return txt;
        }
    }
}
