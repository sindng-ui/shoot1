using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using HappyShoot.Domain.Events;
using HappyShoot.Domain.Meta;
using HappyShoot.Domain.Progression;
using HappyShoot.Domain.Session;
using HappyShoot.View.Shop;
using HappyShoot.View.SkillTree;

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
        private SkillTreeManager _skillTreeManager;
        private SkillTreeUiView _skillTreeUiView;
        private InGameGemCounterHudView _gemCounter;
        private Text _gemsEarnedText;

        public void Initialize(
            GameSessionEntity gameSession,
            EventBus eventBus,
            MetaShopManager shopManager = null,
            MetaShopUiView shopView = null,
            SkillTreeManager skillTreeManager = null,
            SkillTreeUiView skillTreeUiView = null,
            InGameGemCounterHudView gemCounter = null)
        {
            _gameSession = gameSession;
            _eventBus = eventBus;
            _shopManager = shopManager;
            _shopView = shopView;
            _skillTreeManager = skillTreeManager;
            _skillTreeUiView = skillTreeUiView;
            _gemCounter = gemCounter;

            if (_shopView != null)
            {
                _shopView.OnShopClosed += OnShopClosed;
            }

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

        private void OnDestroy()
        {
            if (_shopView != null)
            {
                _shopView.OnShopClosed -= OnShopClosed;
            }
        }

        private void OnShopClosed()
        {
            if (_gameSession != null && _gameSession.IsGameOver)
            {
                if (_panelRoot != null)
                {
                    _panelRoot.SetActive(true);
                }
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
                    _goldEarnedText.text = $"💰 획득한 골드: +{_gameSession.GoldEarned}";
                }

                // Settle gems into permanent SkillTree storage
                if (_gemCounter != null && _skillTreeManager != null)
                {
                    int r = _gemCounter.RunRubyCount;
                    int e = _gemCounter.RunEmeraldCount;
                    int a = _gemCounter.RunAmethystCount;

                    if (r > 0) _skillTreeManager.AddGems(HappyShoot.Domain.Progression.GemType.Ruby, r);
                    if (e > 0) _skillTreeManager.AddGems(HappyShoot.Domain.Progression.GemType.Emerald, e);
                    if (a > 0) _skillTreeManager.AddGems(HappyShoot.Domain.Progression.GemType.Amethyst, a);

                    if (_gemsEarnedText != null)
                    {
                        _gemsEarnedText.text = $"💎 획득 보석: 🔴+{r}  🟢+{e}  🟣+{a}";
                    }
                }

                // Settle gold into permanent storage
                if (_shopManager != null && _gameSession.GoldEarned > 0)
                {
                    _shopManager.AddGold(_gameSession.GoldEarned);
                }
            }

            Utils.HitStopManager.Instance?.CancelHitStop();
            Time.timeScale = 0f;
        }

        public void OnRetryClicked()
        {
            Debug.Log("[GameOverResultUiView] OnRetryClicked! Reloading active scene...");
            Time.timeScale = 1f;

            try
            {
                var currentScene = SceneManager.GetActiveScene();
                if (currentScene.buildIndex >= 0)
                {
                    SceneManager.LoadScene(currentScene.buildIndex);
                }
                else if (!string.IsNullOrEmpty(currentScene.name))
                {
                    SceneManager.LoadScene(currentScene.name);
                }
                else
                {
                    SceneManager.LoadScene(0);
                }
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"[GameOverResultUiView] Scene load failed, fallback to reload: {ex.Message}");
                SceneManager.LoadScene(SceneManager.GetActiveScene().name);
            }
        }

        public void OnOpenShopClicked()
        {
            if (_skillTreeUiView != null)
            {
                _skillTreeUiView.Show();
                return;
            }

            if (_shopView != null)
            {
                if (_panelRoot != null)
                {
                    _panelRoot.SetActive(false);
                }
                _shopView.ShowShop();
            }
        }

        private void EnsureUiElements()
        {
            if (_panelRoot != null) return;

            // Ensure EventSystem with InputSystemUIInputModule
            var eventSystem = FindFirstObjectByType<UnityEngine.EventSystems.EventSystem>();
            if (eventSystem == null)
            {
                var eventSystemGo = new GameObject("EventSystem");
                eventSystemGo.AddComponent<UnityEngine.EventSystems.EventSystem>();
                eventSystemGo.AddComponent<UnityEngine.InputSystem.UI.InputSystemUIInputModule>();
            }
            else
            {
                var legacy = eventSystem.GetComponent<UnityEngine.EventSystems.StandaloneInputModule>();
                if (legacy != null) DestroyImmediate(legacy);
                if (eventSystem.GetComponent<UnityEngine.InputSystem.UI.InputSystemUIInputModule>() == null)
                {
                    eventSystem.gameObject.AddComponent<UnityEngine.InputSystem.UI.InputSystemUIInputModule>();
                }
            }

            var canvasGo = new GameObject("GameOverCanvas");
            var canvas = canvasGo.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 100;
            var scaler = canvasGo.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            scaler.matchWidthOrHeight = 0.5f;
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
            dialogRt.sizeDelta = new Vector2(440f, 560f);
            dialogGo.AddComponent<Image>().color = new Color(0.18f, 0.12f, 0.15f, 0.98f);

            // Title
            CreateText(dialogGo.transform, "Title", "💀 게임 오버 💀", 32, TextAnchor.MiddleCenter, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -44f), new Vector2(360f, 50f), new Color(1f, 0.3f, 0.3f, 1f));

            // Stats items
            _survivalTimeText = CreateText(dialogGo.transform, "TimeText", "⏱️ 생존 시간: 00:00", 20, TextAnchor.MiddleLeft, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, 85f), new Vector2(340f, 32f), Color.white);
            _killCountText = CreateText(dialogGo.transform, "KillsText", "💀 처치한 적: 0 마리", 20, TextAnchor.MiddleLeft, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, 48f), new Vector2(340f, 32f), Color.white);
            _levelReachedText = CreateText(dialogGo.transform, "LevelText", "🌟 달성 레벨: Lv.1", 20, TextAnchor.MiddleLeft, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, 12f), new Vector2(340f, 32f), Color.white);
            _goldEarnedText = CreateText(dialogGo.transform, "GoldText", "💰 획득한 골드: +0 G", 20, TextAnchor.MiddleLeft, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, -24f), new Vector2(340f, 32f), new Color(1f, 0.85f, 0.3f, 1f));
            _gemsEarnedText = CreateText(dialogGo.transform, "GemsText", "💎 획득 보석: 🔴+0  🟢+0  🟣+0", 18, TextAnchor.MiddleLeft, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, -60f), new Vector2(340f, 32f), new Color(0.9f, 0.6f, 1f, 1f));

            // Buttons: Power Up Shop & Retry
            _openShopButton = CreateButton(dialogGo.transform, "ShopBtn", "💎 스킬 트리 (영구 성장)", new Vector2(0f, -125f), new Color(0.85f, 0.35f, 0.55f, 1f), OnOpenShopClicked);
            _retryButton = CreateButton(dialogGo.transform, "RetryBtn", "🔄 다시 도전하기", new Vector2(0f, -190f), new Color(0.2f, 0.7f, 0.4f, 1f), OnRetryClicked);
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
