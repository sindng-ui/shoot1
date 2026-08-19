using System.Text;
using UnityEngine;
using UnityEngine.UI;
using HappyShoot.Domain.Entities;
using HappyShoot.Domain.Events;
using HappyShoot.Domain.Leveling;
using HappyShoot.Domain.Session;
using HappyShoot.View.Player;

namespace HappyShoot.View.UI
{
    /// <summary>
    /// Master in-game persistent HUD managing top EXP bar, player HP bar, survival timer, and kill/gold counters.
    /// Can automatically build its own UI procedurally if no Canvas is present!
    /// </summary>
    public class InGameHudView : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private Slider _expSlider;
        [SerializeField] private Text _levelText;
        [SerializeField] private Slider _healthSlider;
        [SerializeField] private Text _healthText;
        [SerializeField] private Text _timerText;
        [SerializeField] private Text _killCountText;
        [SerializeField] private Text _goldText;

        private PlayerView _playerView;
        private LevelSystem _levelSystem;
        private GameSessionEntity _gameSession;
        private EventBus _eventBus;

        private float _targetExpFill;
        private float _currentExpFill;
        private float _targetHpFill;
        private float _currentHpFill;

        private readonly StringBuilder _sb = new StringBuilder(32);
        private int _cachedKills = -1;
        private int _cachedGold = -1;
        private int _cachedLevel = -1;

        public void Initialize(PlayerView playerView, LevelSystem levelSystem, GameSessionEntity gameSession)
        {
            _playerView = playerView;
            _levelSystem = levelSystem;
            _gameSession = gameSession;

            if (_playerView != null)
            {
                _eventBus = _playerView.EventBus;
            }

            EnsureUiElements();
            SubscribeEvents();
            UpdateAllUi(immediate: true);
        }

        private void SubscribeEvents()
        {
            if (_eventBus == null) return;

            _eventBus.Subscribe<PlayerDamagedEvent>(OnPlayerDamaged);
            _eventBus.Subscribe<PlayerHealedEvent>(OnPlayerHealed);
            _eventBus.Subscribe<PlayerLevelUpEvent>(OnLevelUp);
            _eventBus.Subscribe<ExpGainedEvent>(OnExpGained);
            _eventBus.Subscribe<KillCountUpdatedEvent>(OnKillCountUpdated);
            _eventBus.Subscribe<GoldGainedEvent>(OnGoldGained);
        }

        private void Update()
        {
            // Smoothly interpolate Exp Bar and HP Bar
            if (_expSlider != null && Mathf.Abs(_currentExpFill - _targetExpFill) > 0.001f)
            {
                _currentExpFill = Mathf.Lerp(_currentExpFill, _targetExpFill, Time.unscaledDeltaTime * 10f);
                _expSlider.value = _currentExpFill;
            }

            if (_healthSlider != null && Mathf.Abs(_currentHpFill - _targetHpFill) > 0.001f)
            {
                _currentHpFill = Mathf.Lerp(_currentHpFill, _targetHpFill, Time.unscaledDeltaTime * 12f);
                _healthSlider.value = _currentHpFill;
            }

            // Update timer display from game session
            if (_timerText != null && _gameSession != null)
            {
                _timerText.text = _gameSession.GetFormattedTime();
            }
        }

        private void OnPlayerDamaged(PlayerDamagedEvent evt)
        {
            UpdateHpDisplay(evt.RemainingHealth, evt.MaxHealth);
        }

        private void OnPlayerHealed(PlayerHealedEvent evt)
        {
            UpdateHpDisplay(evt.CurrentHealth, evt.MaxHealth);
        }

        private void OnLevelUp(PlayerLevelUpEvent evt)
        {
            UpdateLevelDisplay(evt.NewLevel);
            if (_levelSystem != null)
            {
                _targetExpFill = (float)_levelSystem.CurrentExp / _levelSystem.RequiredExp;
            }
        }

        private void OnExpGained(ExpGainedEvent evt)
        {
            _targetExpFill = (float)evt.CurrentExp / evt.RequiredExp;
        }

        private void OnKillCountUpdated(KillCountUpdatedEvent evt)
        {
            UpdateKillCountDisplay(evt.TotalKills);
        }

        private void OnGoldGained(GoldGainedEvent evt)
        {
            UpdateGoldDisplay(evt.TotalGold);
        }

        public void UpdateAllUi(bool immediate = false)
        {
            if (_playerView != null && _playerView.Entity != null)
            {
                var entity = _playerView.Entity;
                UpdateHpDisplay(entity.CurrentHealth, entity.Stats.MaxHealth, immediate);
            }

            if (_levelSystem != null)
            {
                UpdateLevelDisplay(_levelSystem.Level);
                _targetExpFill = (float)_levelSystem.CurrentExp / _levelSystem.RequiredExp;
                if (immediate)
                {
                    _currentExpFill = _targetExpFill;
                    if (_expSlider != null) _expSlider.value = _currentExpFill;
                }
            }

            if (_gameSession != null)
            {
                UpdateKillCountDisplay(_gameSession.KillCount);
                UpdateGoldDisplay(_gameSession.GoldEarned);
            }
        }

        private void UpdateHpDisplay(float currentHp, float maxHp, bool immediate = false)
        {
            float fill = maxHp > 0f ? Mathf.Clamp01(currentHp / maxHp) : 0f;
            _targetHpFill = fill;
            if (immediate)
            {
                _currentHpFill = fill;
                if (_healthSlider != null) _healthSlider.value = fill;
            }

            if (_healthText != null)
            {
                _sb.Clear();
                _sb.Append("HP ").Append((int)currentHp).Append(" / ").Append((int)maxHp);
                _healthText.text = _sb.ToString();
            }
        }

        private void UpdateLevelDisplay(int level)
        {
            if (_cachedLevel == level) return;
            _cachedLevel = level;

            if (_levelText != null)
            {
                _sb.Clear();
                _sb.Append("LV. ").Append(level);
                _levelText.text = _sb.ToString();
            }
        }

        private void UpdateKillCountDisplay(int kills)
        {
            if (_cachedKills == kills) return;
            _cachedKills = kills;

            if (_killCountText != null)
            {
                _sb.Clear();
                _sb.Append("💀 ").Append(kills);
                _killCountText.text = _sb.ToString();
            }
        }

        private void UpdateGoldDisplay(int gold)
        {
            if (_cachedGold == gold) return;
            _cachedGold = gold;

            if (_goldText != null)
            {
                _sb.Clear();
                _sb.Append("💰 ").Append(gold);
                _goldText.text = _sb.ToString();
            }
        }

        private void EnsureUiElements()
        {
            if (_expSlider != null && _healthSlider != null) return;

            // Create Canvas if not already present
            var canvasGo = new GameObject("InGameHudCanvas");
            var canvas = canvasGo.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 10;
            canvasGo.AddComponent<CanvasScaler>().uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            canvasGo.AddComponent<GraphicRaycaster>();

            // 1. Top Exp Bar
            var expBarGo = CreateUiPanel(canvasGo.transform, "ExpBarBackground", new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -12f), new Vector2(0f, 24f), new Color(0.1f, 0.12f, 0.18f, 0.9f));
            
            var expFillGo = CreateUiPanel(expBarGo.transform, "ExpFill", new Vector2(0f, 0f), new Vector2(1f, 1f), new Vector2(0f, 0.5f), Vector2.zero, Vector2.zero, new Color(0.2f, 0.8f, 0.4f, 1f));
            _expSlider = expBarGo.AddComponent<Slider>();
            _expSlider.targetGraphic = expBarGo.GetComponent<Image>();
            _expSlider.fillRect = expFillGo.GetComponent<RectTransform>();
            _expSlider.minValue = 0f;
            _expSlider.maxValue = 1f;
            _expSlider.value = 0f;

            // Level Text
            _levelText = CreateText(expBarGo.transform, "LevelText", "LV. 1", 16, TextAnchor.MiddleLeft, new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(16f, 0f), new Vector2(100f, 24f), Color.white);

            // 2. Top Center Timer
            _timerText = CreateText(canvasGo.transform, "TimerText", "00:00", 28, TextAnchor.MiddleCenter, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -48f), new Vector2(160f, 40f), new Color(1f, 0.95f, 0.7f, 1f));

            // 3. Top Right Kills & Gold
            _killCountText = CreateText(canvasGo.transform, "KillsText", "💀 0", 20, TextAnchor.MiddleRight, new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(-24f, -44f), new Vector2(140f, 30f), new Color(1f, 0.45f, 0.45f, 1f));
            _goldText = CreateText(canvasGo.transform, "GoldText", "💰 0", 20, TextAnchor.MiddleRight, new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(-24f, -74f), new Vector2(140f, 30f), new Color(1f, 0.85f, 0.25f, 1f));

            // 4. Top Left Player HP Bar
            var hpBgGo = CreateUiPanel(canvasGo.transform, "HpBarBackground", new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(20f, -48f), new Vector2(220f, 28f), new Color(0.18f, 0.05f, 0.05f, 0.85f));
            var hpFillGo = CreateUiPanel(hpBgGo.transform, "HpFill", new Vector2(0f, 0f), new Vector2(1f, 1f), new Vector2(0f, 0.5f), Vector2.zero, Vector2.zero, new Color(0.9f, 0.2f, 0.2f, 1f));
            
            _healthSlider = hpBgGo.AddComponent<Slider>();
            _healthSlider.targetGraphic = hpBgGo.GetComponent<Image>();
            _healthSlider.fillRect = hpFillGo.GetComponent<RectTransform>();
            _healthSlider.minValue = 0f;
            _healthSlider.maxValue = 1f;
            _healthSlider.value = 1f;

            _healthText = CreateText(hpBgGo.transform, "HpText", "HP 100 / 100", 14, TextAnchor.MiddleCenter, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(200f, 24f), Color.white);
        }

        private GameObject CreateUiPanel(Transform parent, string name, Vector2 anchorMin, Vector2 anchorMax, Vector2 pivot, Vector2 anchoredPos, Vector2 sizeDelta, Color color)
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
            img.color = color;
            return go;
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
            txt.alignment = alignment;
            txt.color = color;
            txt.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            return txt;
        }
    }
}
