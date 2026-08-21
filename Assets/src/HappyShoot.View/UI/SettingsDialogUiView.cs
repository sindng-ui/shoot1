using System;
using UnityEngine;
using UnityEngine.UI;
using HappyShoot.Domain.Settings;
using HappyShoot.View.Utils;

namespace HappyShoot.View.UI
{
    /// <summary>
    /// Master Settings Dialog UI View managing Gameplay, Audio, and Display preferences.
    /// Provides a modern 3-tab modal dialog with InputSystem UI support and instant PlayerPrefs sync.
    /// </summary>
    public class SettingsDialogUiView : MonoBehaviour
    {
        private enum SettingsTab { Gameplay, Sound, Display }

        private GameObject _dialogRoot;
        private SettingsTab _currentTab = SettingsTab.Gameplay;

        // Tab Buttons
        private Button _btnTabGameplay;
        private Button _btnTabSound;
        private Button _btnTabDisplay;
        private Image _imgTabGameplay;
        private Image _imgTabSound;
        private Image _imgTabDisplay;

        // Content Containers
        private GameObject _panelGameplay;
        private GameObject _panelSound;
        private GameObject _panelDisplay;

        // Gameplay UI Elements
        private Text _txtAutoTargetStatus;
        private Text _txtShowDamageStatus;

        // Sound UI Elements
        private Text _txtMuteStatus;
        private Text _txtBgmValue;
        private Text _txtSfxValue;

        // Display UI Elements
        private Text _txtUiScaleStatus;
        private Text _txtScreenShakeStatus;
        private Text _txtFullscreenStatus;

        private Action _onCloseCallback;

        public void Initialize()
        {
            BuildUi();
            Hide();
        }

        public void Show(Action onClose = null)
        {
            _onCloseCallback = onClose;
            _dialogRoot.SetActive(true);
            RefreshAllValues();
            SwitchTab(SettingsTab.Gameplay);
        }

        public void Hide()
        {
            if (_dialogRoot != null)
            {
                _dialogRoot.SetActive(false);
            }
            _onCloseCallback?.Invoke();
            _onCloseCallback = null;
        }

        private void BuildUi()
        {
            var canvas = gameObject.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 999; // Topmost modal

            var scaler = gameObject.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            scaler.matchWidthOrHeight = 0.5f;

            gameObject.AddComponent<GraphicRaycaster>();
            
            if (FindFirstObjectByType<UnityEngine.EventSystems.EventSystem>() == null)
            {
                var esGo = new GameObject("EventSystem");
                esGo.AddComponent<UnityEngine.EventSystems.EventSystem>();
                esGo.AddComponent<UnityEngine.InputSystem.UI.InputSystemUIInputModule>();
            }

            _dialogRoot = new GameObject("SettingsModalRoot");
            _dialogRoot.transform.SetParent(transform, false);

            var rootRect = _dialogRoot.AddComponent<RectTransform>();
            rootRect.anchorMin = Vector2.zero;
            rootRect.anchorMax = Vector2.one;
            rootRect.sizeDelta = Vector2.zero;

            // Dim Background
            var dimImg = _dialogRoot.AddComponent<Image>();
            dimImg.color = new Color(0f, 0f, 0f, 0.75f);
            var dimBtn = _dialogRoot.AddComponent<Button>();
            dimBtn.onClick.AddListener(Hide);

            // Center Panel (680 x 620)
            var panelGo = new GameObject("SettingsCard");
            panelGo.transform.SetParent(_dialogRoot.transform, false);
            var panelRect = panelGo.AddComponent<RectTransform>();
            panelRect.sizeDelta = new Vector2(720f, 640f);
            panelRect.anchoredPosition = Vector2.zero;

            var panelImg = panelGo.AddComponent<Image>();
            panelImg.color = new Color(0.10f, 0.12f, 0.16f, 0.98f);
            panelGo.AddComponent<Button>(); // Block raycast clicks from closing

            // Header Title
            var titleGo = new GameObject("TitleText");
            titleGo.transform.SetParent(panelGo.transform, false);
            var titleRect = titleGo.AddComponent<RectTransform>();
            titleRect.anchoredPosition = new Vector2(0f, 270f);
            titleRect.sizeDelta = new Vector2(600f, 60f);

            var titleTxt = titleGo.AddComponent<Text>();
            titleTxt.font = FontHelper.GetKoreanFont();
            titleTxt.fontSize = 32;
            titleTxt.fontStyle = FontStyle.Bold;
            titleTxt.alignment = TextAnchor.MiddleCenter;
            titleTxt.color = new Color(1.0f, 0.85f, 0.3f, 1f);
            titleTxt.text = "⚙️ 게임 환경 설정 (SETTINGS)";

            // Tab Bar (y = 205f)
            CreateTabBar(panelGo.transform);

            // Tab Content Area (y = -10f, size 640 x 360)
            CreateGameplayTab(panelGo.transform);
            CreateSoundTab(panelGo.transform);
            CreateDisplayTab(panelGo.transform);

            // Bottom Close Button (y = -260f)
            var closeBtnGo = new GameObject("BtnClose");
            closeBtnGo.transform.SetParent(panelGo.transform, false);
            var closeBtnRect = closeBtnGo.AddComponent<RectTransform>();
            closeBtnRect.anchoredPosition = new Vector2(0f, -265f);
            closeBtnRect.sizeDelta = new Vector2(260f, 56f);

            var closeBtnImg = closeBtnGo.AddComponent<Image>();
            closeBtnImg.color = new Color(0.2f, 0.65f, 0.35f, 1f);
            var closeBtn = closeBtnGo.AddComponent<Button>();
            closeBtn.onClick.AddListener(Hide);

            var closeTxtGo = new GameObject("Text");
            closeTxtGo.transform.SetParent(closeBtnGo.transform, false);
            var closeTxtRect = closeTxtGo.AddComponent<RectTransform>();
            closeTxtRect.sizeDelta = closeBtnRect.sizeDelta;
            var closeTxt = closeTxtGo.AddComponent<Text>();
            closeTxt.font = FontHelper.GetKoreanFont();
            closeTxt.fontSize = 24;
            closeTxt.fontStyle = FontStyle.Bold;
            closeTxt.alignment = TextAnchor.MiddleCenter;
            closeTxt.color = Color.white;
            closeTxt.text = "💾 설정 저장 & 닫기";

            // Version info footer
            var verGo = new GameObject("VerText");
            verGo.transform.SetParent(panelGo.transform, false);
            var verRect = verGo.AddComponent<RectTransform>();
            verRect.anchoredPosition = new Vector2(0f, -305f);
            verRect.sizeDelta = new Vector2(600f, 20f);
            var verTxt = verGo.AddComponent<Text>();
            verTxt.font = FontHelper.GetKoreanFont();
            verTxt.fontSize = 12;
            verTxt.alignment = TextAnchor.MiddleCenter;
            verTxt.color = new Color(0.5f, 0.6f, 0.7f, 0.6f);
            verTxt.text = HappyShoot.Domain.Common.AppVersion.FullVersionText;
        }

        private void CreateTabBar(Transform parent)
        {
            var tabRoot = new GameObject("TabBar");
            tabRoot.transform.SetParent(parent, false);
            var tabRect = tabRoot.AddComponent<RectTransform>();
            tabRect.anchoredPosition = new Vector2(0f, 205f);
            tabRect.sizeDelta = new Vector2(640f, 48f);

            _btnTabGameplay = CreateTabButton(tabRoot.transform, "🎮 게임플레이", -215f, () => SwitchTab(SettingsTab.Gameplay), out _imgTabGameplay);
            _btnTabSound = CreateTabButton(tabRoot.transform, "🎵 사운드", 0f, () => SwitchTab(SettingsTab.Sound), out _imgTabSound);
            _btnTabDisplay = CreateTabButton(tabRoot.transform, "🖥️ 디스플레이", 215f, () => SwitchTab(SettingsTab.Display), out _imgTabDisplay);
        }

        private Button CreateTabButton(Transform parent, string label, float posX, Action onClick, out Image outImg)
        {
            var btnGo = new GameObject($"Tab_{label}");
            btnGo.transform.SetParent(parent, false);
            var rect = btnGo.AddComponent<RectTransform>();
            rect.anchoredPosition = new Vector2(posX, 0f);
            rect.sizeDelta = new Vector2(205f, 44f);

            outImg = btnGo.AddComponent<Image>();
            outImg.color = new Color(0.18f, 0.22f, 0.28f, 1f);

            var btn = btnGo.AddComponent<Button>();
            btn.onClick.AddListener(() => onClick?.Invoke());

            var txtGo = new GameObject("Text");
            txtGo.transform.SetParent(btnGo.transform, false);
            var txtRect = txtGo.AddComponent<RectTransform>();
            txtRect.sizeDelta = rect.sizeDelta;
            var txt = txtGo.AddComponent<Text>();
            txt.font = FontHelper.GetKoreanFont();
            txt.fontSize = 20;
            txt.fontStyle = FontStyle.Bold;
            txt.alignment = TextAnchor.MiddleCenter;
            txt.color = Color.white;
            txt.text = label;

            return btn;
        }

        private void SwitchTab(SettingsTab tab)
        {
            _currentTab = tab;
            _panelGameplay.SetActive(tab == SettingsTab.Gameplay);
            _panelSound.SetActive(tab == SettingsTab.Sound);
            _panelDisplay.SetActive(tab == SettingsTab.Display);

            Color activeColor = new Color(0.28f, 0.45f, 0.70f, 1f);
            Color inactiveColor = new Color(0.16f, 0.20f, 0.26f, 1f);

            _imgTabGameplay.color = (tab == SettingsTab.Gameplay) ? activeColor : inactiveColor;
            _imgTabSound.color = (tab == SettingsTab.Sound) ? activeColor : inactiveColor;
            _imgTabDisplay.color = (tab == SettingsTab.Display) ? activeColor : inactiveColor;
        }

        private void CreateGameplayTab(Transform parent)
        {
            _panelGameplay = new GameObject("PanelGameplay");
            _panelGameplay.transform.SetParent(parent, false);
            var rect = _panelGameplay.AddComponent<RectTransform>();
            rect.anchoredPosition = new Vector2(0f, -10f);
            rect.sizeDelta = new Vector2(640f, 350f);

            // Row 1: Auto Targeting
            CreateOptionRow(_panelGameplay.transform, "🎯 기본스킬 자동 타겟팅", 90f,
                "ON: 가까운 적 자동 조준 / OFF: 마우스 방향 직접 사격",
                out _txtAutoTargetStatus, () =>
                {
                    GameSettings.SetAutoTargeting(!GameSettings.AutoTargeting);
                    RefreshAllValues();
                });

            // Row 2: Damage Numbers
            CreateOptionRow(_panelGameplay.transform, "💥 데미지 텍스트 표시", -40f,
                "타격 시 수치 플로팅 텍스트 렌더링 여부",
                out _txtShowDamageStatus, () =>
                {
                    GameSettings.SetShowDamageText(!GameSettings.ShowDamageText);
                    RefreshAllValues();
                });
        }

        private void CreateSoundTab(Transform parent)
        {
            _panelSound = new GameObject("PanelSound");
            _panelSound.transform.SetParent(parent, false);
            var rect = _panelSound.AddComponent<RectTransform>();
            rect.anchoredPosition = new Vector2(0f, -10f);
            rect.sizeDelta = new Vector2(640f, 350f);

            // Row 1: Master Mute
            CreateOptionRow(_panelSound.transform, "🔇 전체 음소거 (Mute)", 110f,
                "모든 사운드 및 BGM 즉시 음소거",
                out _txtMuteStatus, () =>
                {
                    GameSettings.SetMuted(!GameSettings.IsMuted);
                    RefreshAllValues();
                });

            // Row 2: BGM Volume Slider
            CreateSliderRow(_panelSound.transform, "🎼 배경음악 (BGM)", 20f, out _txtBgmValue,
                onMinus: () => { GameSettings.SetBgmVolume(GameSettings.BgmVolume - 0.1f); RefreshAllValues(); },
                onPlus: () => { GameSettings.SetBgmVolume(GameSettings.BgmVolume + 0.1f); RefreshAllValues(); });

            // Row 3: SFX Volume Slider
            CreateSliderRow(_panelSound.transform, "⚔️ 효과음 (SFX)", -70f, out _txtSfxValue,
                onMinus: () => { GameSettings.SetSfxVolume(GameSettings.SfxVolume - 0.1f); RefreshAllValues(); },
                onPlus: () => { GameSettings.SetSfxVolume(GameSettings.SfxVolume + 0.1f); RefreshAllValues(); });
        }

        private void CreateDisplayTab(Transform parent)
        {
            _panelDisplay = new GameObject("PanelDisplay");
            _panelDisplay.transform.SetParent(parent, false);
            var rect = _panelDisplay.AddComponent<RectTransform>();
            rect.anchoredPosition = new Vector2(0f, -10f);
            rect.sizeDelta = new Vector2(640f, 350f);

            // Row 1: UI Scale
            CreateOptionRow(_panelDisplay.transform, "🔍 UI 크기 조절", 100f,
                "80% (미니) -> 100% (표준) -> 120% (크게)",
                out _txtUiScaleStatus, () =>
                {
                    float nextScale = GameSettings.UiScale < 0.95f ? 1.0f : (GameSettings.UiScale < 1.15f ? 1.20f : 0.80f);
                    GameSettings.SetUiScale(nextScale);
                    RefreshAllValues();
                });

            // Row 2: Screen Shake
            CreateOptionRow(_panelDisplay.transform, "📳 화면 진동 (Screen Shake)", -10f,
                "폭발 및 피격 시 타격감 화면 흔들림 효과",
                out _txtScreenShakeStatus, () =>
                {
                    GameSettings.SetScreenShake(!GameSettings.ScreenShake);
                    RefreshAllValues();
                });

            // Row 3: Fullscreen
            CreateOptionRow(_panelDisplay.transform, "🖥️ 화면 모드 (Display)", -120f,
                "전체화면 모드 및 창모드 전환",
                out _txtFullscreenStatus, () =>
                {
                    GameSettings.SetFullscreen(!GameSettings.IsFullscreen);
                    RefreshAllValues();
                });
        }

        private void CreateOptionRow(Transform parent, string title, float posY, string desc, out Text statusTxt, Action onToggle)
        {
            var rowGo = new GameObject($"Row_{title}");
            rowGo.transform.SetParent(parent, false);
            var rowRect = rowGo.AddComponent<RectTransform>();
            rowRect.anchoredPosition = new Vector2(0f, posY);
            rowRect.sizeDelta = new Vector2(620f, 75f);

            var bgImg = rowGo.AddComponent<Image>();
            bgImg.color = new Color(0.14f, 0.17f, 0.22f, 0.9f);

            // Title & Desc
            var txtGo = new GameObject("Title");
            txtGo.transform.SetParent(rowGo.transform, false);
            var txtRect = txtGo.AddComponent<RectTransform>();
            txtRect.anchoredPosition = new Vector2(-120f, 0f);
            txtRect.sizeDelta = new Vector2(340f, 65f);

            var t = txtGo.AddComponent<Text>();
            t.font = FontHelper.GetKoreanFont();
            t.fontSize = 20;
            t.fontStyle = FontStyle.Bold;
            t.alignment = TextAnchor.MiddleLeft;
            t.color = Color.white;
            t.text = $"{title}\n<size=14><color=#A0A8B4>{desc}</color></size>";

            // Toggle Button
            var btnGo = new GameObject("BtnToggle");
            btnGo.transform.SetParent(rowGo.transform, false);
            var btnRect = btnGo.AddComponent<RectTransform>();
            btnRect.anchoredPosition = new Vector2(210f, 0f);
            btnRect.sizeDelta = new Vector2(160f, 48f);
            btnGo.AddComponent<Image>().color = new Color(0.24f, 0.32f, 0.44f, 1f);
            var btn = btnGo.AddComponent<Button>();
            btn.onClick.AddListener(() => onToggle?.Invoke());

            var statGo = new GameObject("StatusText");
            statGo.transform.SetParent(btnGo.transform, false);
            var statRect = statGo.AddComponent<RectTransform>();
            statRect.sizeDelta = btnRect.sizeDelta;
            statusTxt = statGo.AddComponent<Text>();
            statusTxt.font = FontHelper.GetKoreanFont();
            statusTxt.fontSize = 20;
            statusTxt.fontStyle = FontStyle.Bold;
            statusTxt.alignment = TextAnchor.MiddleCenter;
            statusTxt.color = Color.white;
        }

        private void CreateSliderRow(Transform parent, string title, float posY, out Text valueTxt, Action onMinus, Action onPlus)
        {
            var rowGo = new GameObject($"SliderRow_{title}");
            rowGo.transform.SetParent(parent, false);
            var rowRect = rowGo.AddComponent<RectTransform>();
            rowRect.anchoredPosition = new Vector2(0f, posY);
            rowRect.sizeDelta = new Vector2(620f, 70f);
            rowGo.AddComponent<Image>().color = new Color(0.14f, 0.17f, 0.22f, 0.9f);

            var txtGo = new GameObject("Title");
            txtGo.transform.SetParent(rowGo.transform, false);
            var txtRect = txtGo.AddComponent<RectTransform>();
            txtRect.anchoredPosition = new Vector2(-150f, 0f);
            txtRect.sizeDelta = new Vector2(280f, 60f);
            var t = txtGo.AddComponent<Text>();
            t.font = FontHelper.GetKoreanFont();
            t.fontSize = 20;
            t.fontStyle = FontStyle.Bold;
            t.alignment = TextAnchor.MiddleLeft;
            t.color = Color.white;
            t.text = title;

            CreateMiniButton(rowGo.transform, "◀", 60f, onMinus);

            var valGo = new GameObject("ValueText");
            valGo.transform.SetParent(rowGo.transform, false);
            var valRect = valGo.AddComponent<RectTransform>();
            valRect.anchoredPosition = new Vector2(170f, 0f);
            valRect.sizeDelta = new Vector2(140f, 44f);
            valueTxt = valGo.AddComponent<Text>();
            valueTxt.font = FontHelper.GetKoreanFont();
            valueTxt.fontSize = 20;
            valueTxt.fontStyle = FontStyle.Bold;
            valueTxt.alignment = TextAnchor.MiddleCenter;
            valueTxt.color = new Color(0.4f, 0.9f, 0.6f, 1f);

            CreateMiniButton(rowGo.transform, "▶", 265f, onPlus);
        }

        private Button CreateSettingButton(Transform parent, string label, float posX, float width, Action onClick)
        {
            var btnGo = new GameObject($"Btn_{label}");
            btnGo.transform.SetParent(parent, false);
            var rect = btnGo.AddComponent<RectTransform>();
            rect.anchoredPosition = new Vector2(posX, 0f);
            rect.sizeDelta = new Vector2(width, 44f);
            btnGo.AddComponent<Image>().color = new Color(0.24f, 0.32f, 0.44f, 1f);
            var btn = btnGo.AddComponent<Button>();
            btn.onClick.AddListener(() => onClick?.Invoke());

            if (!string.IsNullOrEmpty(label))
            {
                var tGo = new GameObject("Text");
                tGo.transform.SetParent(btnGo.transform, false);
                var tRect = tGo.AddComponent<RectTransform>();
                tRect.sizeDelta = rect.sizeDelta;
                var txt = tGo.AddComponent<Text>();
                txt.font = FontHelper.GetKoreanFont();
                txt.fontSize = 18;
                txt.fontStyle = FontStyle.Bold;
                txt.alignment = TextAnchor.MiddleCenter;
                txt.color = Color.white;
                txt.text = label;
            }
            return btn;
        }

        private Button CreateMiniButton(Transform parent, string label, float posX, Action onClick)
        {
            return CreateSettingButton(parent, label, posX, 48f, onClick);
        }

        private void RefreshAllValues()
        {
            // Gameplay
            if (_txtAutoTargetStatus != null)
            {
                _txtAutoTargetStatus.text = GameSettings.AutoTargeting ? "🟢 자동 (ON)" : "🔵 마우스 (OFF)";
                _txtAutoTargetStatus.color = GameSettings.AutoTargeting ? new Color(0.4f, 1f, 0.5f) : new Color(0.5f, 0.8f, 1f);
            }

            if (_txtShowDamageStatus != null)
            {
                _txtShowDamageStatus.text = GameSettings.ShowDamageText ? "🟢 켜짐 (ON)" : "🔴 꺼짐 (OFF)";
                _txtShowDamageStatus.color = GameSettings.ShowDamageText ? new Color(0.4f, 1f, 0.5f) : new Color(1f, 0.5f, 0.5f);
            }

            // Sound
            if (_txtMuteStatus != null)
            {
                _txtMuteStatus.text = GameSettings.IsMuted ? "🔴 음소거 ON" : "🟢 소리 켬 (OFF)";
                _txtMuteStatus.color = GameSettings.IsMuted ? new Color(1f, 0.5f, 0.5f) : new Color(0.4f, 1f, 0.5f);
            }

            if (_txtBgmValue != null)
            {
                _txtBgmValue.text = $"{Mathf.RoundToInt(GameSettings.BgmVolume * 100)}%";
            }

            if (_txtSfxValue != null)
            {
                _txtSfxValue.text = $"{Mathf.RoundToInt(GameSettings.SfxVolume * 100)}%";
            }

            // Display
            if (_txtUiScaleStatus != null)
            {
                _txtUiScaleStatus.text = $"{Mathf.RoundToInt(GameSettings.UiScale * 100)}%";
            }

            if (_txtScreenShakeStatus != null)
            {
                _txtScreenShakeStatus.text = GameSettings.ScreenShake ? "🟢 켜짐 (ON)" : "🔴 꺼짐 (OFF)";
                _txtScreenShakeStatus.color = GameSettings.ScreenShake ? new Color(0.4f, 1f, 0.5f) : new Color(1f, 0.5f, 0.5f);
            }

            if (_txtFullscreenStatus != null)
            {
                _txtFullscreenStatus.text = GameSettings.IsFullscreen ? "🖥️ 전체화면" : "🪟 창모드";
            }
        }
    }
}
