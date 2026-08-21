using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using HappyShoot.Domain.Events;
using HappyShoot.Domain.Session;

namespace HappyShoot.View.UI
{
    /// <summary>
    /// Pause Menu popup activated by ESC key or pause button.
    /// Manages Resume, Restart, and Quit flows with zero-garbage UI state handling.
    /// </summary>
    public class PauseMenuUiView : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private GameObject _panelRoot;
        [SerializeField] private Button _resumeButton;
        [SerializeField] private Button _restartButton;
        [SerializeField] private Button _quitButton;

        private GameSessionEntity _gameSession;
        private EventBus _eventBus;

        public void Initialize(GameSessionEntity gameSession, EventBus eventBus)
        {
            _gameSession = gameSession;
            _eventBus = eventBus;

            EnsureUiElements();

            if (_eventBus != null)
            {
                _eventBus.Subscribe<GameStateChangedEvent>(OnGameStateChanged);
            }

            if (_panelRoot != null)
            {
                _panelRoot.SetActive(false);
            }
        }

        private void Update()
        {
            // Toggle pause on ESC key press
            if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
            {
                if (_gameSession != null)
                {
                    if (_gameSession.IsPlaying)
                    {
                        _gameSession.Pause();
                    }
                    else if (_gameSession.IsPaused)
                    {
                        _gameSession.Resume();
                    }
                }
            }
        }

        private void OnGameStateChanged(GameStateChangedEvent evt)
        {
            if (evt.NewState == GameState.Paused)
            {
                ShowPauseMenu();
            }
            else if (evt.PreviousState == GameState.Paused && evt.NewState == GameState.Playing)
            {
                HidePauseMenu();
            }
        }

        public void ShowPauseMenu()
        {
            if (_panelRoot != null)
            {
                _panelRoot.SetActive(true);
            }
            Time.timeScale = 0f;
        }

        public void HidePauseMenu()
        {
            if (_panelRoot != null)
            {
                _panelRoot.SetActive(false);
            }
            Time.timeScale = 1f;
        }

        public void OnResumeClicked()
        {
            if (_gameSession != null)
            {
                _gameSession.Resume();
            }
            else
            {
                HidePauseMenu();
            }
        }

        public void OnRestartClicked()
        {
            Time.timeScale = 1f;
            var currentScene = SceneManager.GetActiveScene();
            if (!string.IsNullOrEmpty(currentScene.name))
            {
                SceneManager.LoadScene(currentScene.name);
            }
            else if (currentScene.buildIndex >= 0)
            {
                SceneManager.LoadScene(currentScene.buildIndex);
            }
            else
            {
                SceneManager.LoadScene(0);
            }
        }

        public void OnQuitClicked()
        {
            Time.timeScale = 1f;
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }

        private SettingsDialogUiView _settingsDialog;

        public void SetSettingsDialog(SettingsDialogUiView dialog)
        {
            _settingsDialog = dialog;
        }

        private void EnsureUiElements()
        {
            if (_panelRoot != null) return;

            // Ensure EventSystem exists in scene
            if (FindFirstObjectByType<UnityEngine.EventSystems.EventSystem>() == null)
            {
                var eventSystemGo = new GameObject("EventSystem");
                eventSystemGo.AddComponent<UnityEngine.EventSystems.EventSystem>();
                eventSystemGo.AddComponent<UnityEngine.InputSystem.UI.InputSystemUIInputModule>();
            }

            var canvasGo = new GameObject("PauseMenuCanvas");
            var canvas = canvasGo.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 50;
            var scaler = canvasGo.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            scaler.matchWidthOrHeight = 0.5f;
            canvasGo.AddComponent<GraphicRaycaster>();

            // Dark backdrop
            _panelRoot = new GameObject("PausePanel");
            _panelRoot.transform.SetParent(canvasGo.transform, false);
            var panelRt = _panelRoot.AddComponent<RectTransform>();
            panelRt.anchorMin = Vector2.zero;
            panelRt.anchorMax = Vector2.one;
            panelRt.sizeDelta = Vector2.zero;
            var panelImg = _panelRoot.AddComponent<Image>();
            panelImg.color = new Color(0.05f, 0.06f, 0.09f, 0.85f); // Dark translucent slate

            // Dialog container
            var dialogGo = new GameObject("DialogBox");
            dialogGo.transform.SetParent(_panelRoot.transform, false);
            var dialogRt = dialogGo.AddComponent<RectTransform>();
            dialogRt.anchorMin = new Vector2(0.5f, 0.5f);
            dialogRt.anchorMax = new Vector2(0.5f, 0.5f);
            dialogRt.sizeDelta = new Vector2(380f, 440f);
            var dialogImg = dialogGo.AddComponent<Image>();
            dialogImg.color = new Color(0.12f, 0.15f, 0.22f, 0.95f);

            // Title
            CreateText(dialogGo.transform, "Title", "⏸️ 게임 일시정지", 26, TextAnchor.MiddleCenter, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -40f), new Vector2(300f, 40f), Color.white);

            // Buttons
            _resumeButton = CreateButton(dialogGo.transform, "ResumeBtn", "▶ 계속하기", new Vector2(0f, 55f), new Color(0.2f, 0.6f, 0.9f, 1f), OnResumeClicked);
            CreateButton(dialogGo.transform, "SettingsBtn", "⚙️ 환경 설정", new Vector2(0f, -10f), new Color(0.32f, 0.42f, 0.60f, 1f), () => _settingsDialog?.Show());
            _restartButton = CreateButton(dialogGo.transform, "RestartBtn", "🔄 다시 시작", new Vector2(0f, -75f), new Color(0.3f, 0.7f, 0.4f, 1f), OnRestartClicked);
            _quitButton = CreateButton(dialogGo.transform, "QuitBtn", "🚪 게임 종료", new Vector2(0f, -140f), new Color(0.85f, 0.25f, 0.25f, 1f), OnQuitClicked);
        }

        private Button CreateButton(Transform parent, string name, string label, Vector2 anchoredPos, Color btnColor, UnityEngine.Events.UnityAction onClick)
        {
            var btnGo = new GameObject(name);
            btnGo.transform.SetParent(parent, false);
            var rt = btnGo.AddComponent<RectTransform>();
            rt.anchorMin = new Vector2(0.5f, 0.5f);
            rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.anchoredPosition = anchoredPos;
            rt.sizeDelta = new Vector2(240f, 50f);

            var img = btnGo.AddComponent<Image>();
            img.sprite = Utils.SpriteHelper.GetOrCreateWhiteSprite();
            img.color = btnColor;

            var btn = btnGo.AddComponent<Button>();
            btn.targetGraphic = img;
            btn.onClick.AddListener(onClick);

            CreateText(btnGo.transform, "Label", label, 18, TextAnchor.MiddleCenter, Vector2.zero, Vector2.one, new Vector2(0.5f, 0.5f), Vector2.zero, Vector2.zero, Color.white);

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
            txt.raycastTarget = false;
            txt.font = Utils.FontHelper.GetKoreanFont();
            return txt;
        }
    }
}
