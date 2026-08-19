using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using HappyShoot.Domain.Events;
using HappyShoot.Domain.Meta;
using HappyShoot.Domain.Session;
using HappyShoot.View.Shop;

namespace HappyShoot.View.UI
{
    /// <summary>
    /// Game Over summary popup shown when the player dies.
    /// Settles earned gold into permanent meta storage, displays stats, and provides replay/shop navigation.
    /// </summary>
    public class GameOverResultUiView : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private GameObject _panelRoot;
        [SerializeField] private Text _survivalTimeText;
        [SerializeField] private Text _killCountText;
        [SerializeField] private Text _levelReachedText;
        [SerializeField] private Text _goldEarnedText;
        [SerializeField] private Button _retryButton;
        [SerializeField] private Button _openShopButton;

        private GameSessionEntity _gameSession;
        private EventBus _eventBus;
        private MetaShopManager _shopManager;
        private MetaShopUiView _shopView;

        public void Initialize(GameSessionEntity gameSession, EventBus eventBus, MetaShopManager shopManager = null, MetaShopUiView shopView = null)
        {
            _gameSession = gameSession;
            _eventBus = eventBus;
            _shopManager = shopManager;
            _shopView = shopView;

            EnsureUiElements();

            if (_eventBus != null)
            {
                _eventBus.Subscribe<PlayerDiedEvent>(OnPlayerDied);
            }

            if (_panelRoot != null)
            {
                _panelRoot.SetActive(false);
            }
        }

        private void OnPlayerDied(PlayerDiedEvent evt)
        {
            ShowGameOverPopup();
        }

        public void ShowGameOverPopup()
        {
            if (_panelRoot != null)
            {
                _panelRoot.SetActive(true);
            }

            if (_gameSession != null)
            {
                if (_survivalTimeText != null)
                {
                    _survivalTimeText.text = $"⏱️ Survived: {_gameSession.GetFormattedTime()}";
                }

                if (_killCountText != null)
                {
                    _killCountText.text = $"💀 Enemies Defeated: {_gameSession.KillCount}";
                }

                if (_levelReachedText != null)
                {
                    _levelReachedText.text = $"🌟 Reached Level: Lv.{_gameSession.PlayerLevel}";
                }

                if (_goldEarnedText != null)
                {
                    _goldEarnedText.text = $"💰 Gold Collected: +{_gameSession.GoldEarned}";
                }

                // Settle gold into permanent storage
                if (_shopManager != null && _gameSession.GoldEarned > 0)
                {
                    _shopManager.AddGold(_gameSession.GoldEarned);
                }
            }

            Time.timeScale = 0f;
        }

        public void OnRetryClicked()
        {
            Time.timeScale = 1f;
            Scene currentScene = SceneManager.GetActiveScene();
            SceneManager.LoadScene(currentScene.buildIndex);
        }

        public void OnOpenShopClicked()
        {
            if (_shopView != null)
            {
                _shopView.ShowShop();
            }
        }

        private void EnsureUiElements()
        {
            if (_panelRoot != null) return;

            var canvasGo = new GameObject("GameOverCanvas");
            var canvas = canvasGo.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 100;
            canvasGo.AddComponent<CanvasScaler>().uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            canvasGo.AddComponent<GraphicRaycaster>();

            // Dark red-tinted backdrop
            _panelRoot = new GameObject("GameOverPanel");
            _panelRoot.transform.SetParent(canvasGo.transform, false);
            var panelRt = _panelRoot.AddComponent<RectTransform>();
            panelRt.anchorMin = Vector2.zero;
            panelRt.anchorMax = Vector2.one;
            panelRt.sizeDelta = Vector2.zero;
            var panelImg = _panelRoot.AddComponent<Image>();
            panelImg.color = new Color(0.12f, 0.02f, 0.02f, 0.9f);

            // Dialog container
            var dialogGo = new GameObject("StatsBox");
            dialogGo.transform.SetParent(_panelRoot.transform, false);
            var dialogRt = dialogGo.AddComponent<RectTransform>();
            dialogRt.anchorMin = new Vector2(0.5f, 0.5f);
            dialogRt.anchorMax = new Vector2(0.5f, 0.5f);
            dialogRt.sizeDelta = new Vector2(440f, 520f);
            dialogGo.AddComponent<Image>().color = new Color(0.18f, 0.12f, 0.15f, 0.98f);

            // Title
            CreateText(dialogGo.transform, "Title", "💀 YOU DIED 💀", 32, TextAnchor.MiddleCenter, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -44f), new Vector2(360f, 50f), new Color(1f, 0.3f, 0.3f, 1f));

            // Stats items
            _survivalTimeText = CreateText(dialogGo.transform, "TimeText", "⏱️ Survived: 00:00", 20, TextAnchor.MiddleLeft, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, 70f), new Vector2(340f, 32f), Color.white);
            _killCountText = CreateText(dialogGo.transform, "KillsText", "💀 Enemies Defeated: 0", 20, TextAnchor.MiddleLeft, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, 30f), new Vector2(340f, 32f), Color.white);
            _levelReachedText = CreateText(dialogGo.transform, "LevelText", "🌟 Reached Level: Lv.1", 20, TextAnchor.MiddleLeft, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, -10f), new Vector2(340f, 32f), Color.white);
            _goldEarnedText = CreateText(dialogGo.transform, "GoldText", "💰 Gold Collected: +0", 20, TextAnchor.MiddleLeft, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, -50f), new Vector2(340f, 32f), new Color(1f, 0.85f, 0.3f, 1f));

            // Buttons: Power Up Shop & Retry
            _openShopButton = CreateButton(dialogGo.transform, "ShopBtn", "🏛️ POWER UP SHOP", new Vector2(0f, -115f), new Color(0.85f, 0.65f, 0.15f, 1f), OnOpenShopClicked);
            _retryButton = CreateButton(dialogGo.transform, "RetryBtn", "🔄 PLAY AGAIN", new Vector2(0f, -180f), new Color(0.2f, 0.7f, 0.4f, 1f), OnRetryClicked);
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
            rt.sizeDelta = new Vector2(280f, 50f);

            var img = btnGo.AddComponent<Image>();
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
            txt.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            return txt;
        }
    }
}
