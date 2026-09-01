using UnityEngine;
using UnityEngine.UI;
using HappyShoot.Domain.Events;
using HappyShoot.Domain.Progression;

namespace HappyShoot.View.UI
{
    /// <summary>
    /// Manages real-time run progression gem stone counters (Ruby, Emerald, Amethyst)
    /// within the unified Top-Right Resource HUD, driving sparkling loot punch animations.
    /// Strictly modular and under 500 lines (500-line architecture rule).
    /// </summary>
    public class InGameGemCounterHudView : MonoBehaviour
    {
        private EventBus _eventBus;

        private RectTransform _rubyIconRt;
        private RectTransform _emeraldIconRt;
        private RectTransform _amethystIconRt;

        private Text _rubyText;
        private Text _emeraldText;
        private Text _amethystText;

        private float _rubyPunchScale = 1.0f;
        private float _emeraldPunchScale = 1.0f;
        private float _amethystPunchScale = 1.0f;

        public int RunGoldCount { get; private set; }
        public int RunRubyCount { get; private set; }
        public int RunEmeraldCount { get; private set; }
        public int RunAmethystCount { get; private set; }

        public void Initialize(EventBus eventBus, InGameHudBuilder.HudComponents hud)
        {
            _eventBus = eventBus;
            if (_eventBus != null)
            {
                _eventBus.Subscribe<GemStoneCollectedEvent>(OnGemCollected);
                _eventBus.Subscribe<GoldGainedEvent>(OnGoldGained);
            }

            _rubyIconRt = hud.RubyIconRt;
            _emeraldIconRt = hud.EmeraldIconRt;
            _amethystIconRt = hud.AmethystIconRt;

            _rubyText = hud.RubyText;
            _emeraldText = hud.EmeraldText;
            _amethystText = hud.AmethystText;

            UpdateCounters();
        }

        public void Initialize(EventBus eventBus, Transform parent)
        {
            _eventBus = eventBus;
            if (_eventBus != null)
            {
                _eventBus.Subscribe<GemStoneCollectedEvent>(OnGemCollected);
                _eventBus.Subscribe<GoldGainedEvent>(OnGoldGained);
            }

            var canvas = parent != null ? (parent.GetComponent<Canvas>() ?? parent.GetComponentInChildren<Canvas>()) : null;
            if (canvas != null)
            {
                var rubySlot = canvas.transform.Find("TopLeftUnifiedResourceCapsule/RubyBadge");
                if (rubySlot != null)
                {
                    _rubyIconRt = rubySlot.Find("Icon") as RectTransform;
                    _rubyText = rubySlot.Find("ValueText")?.GetComponent<Text>();
                }
                var emeraldSlot = canvas.transform.Find("TopLeftUnifiedResourceCapsule/EmeraldBadge");
                if (emeraldSlot != null)
                {
                    _emeraldIconRt = emeraldSlot.Find("Icon") as RectTransform;
                    _emeraldText = emeraldSlot.Find("ValueText")?.GetComponent<Text>();
                }
                var amethystSlot = canvas.transform.Find("TopLeftUnifiedResourceCapsule/AmethystBadge");
                if (amethystSlot != null)
                {
                    _amethystIconRt = amethystSlot.Find("Icon") as RectTransform;
                    _amethystText = amethystSlot.Find("ValueText")?.GetComponent<Text>();
                }
            }

            UpdateCounters();
        }

        private void OnDestroy()
        {
            if (_eventBus != null)
            {
                _eventBus.Unsubscribe<GemStoneCollectedEvent>(OnGemCollected);
                _eventBus.Unsubscribe<GoldGainedEvent>(OnGoldGained);
            }
        }

        private void Update()
        {
            // Smoothly decay punch scale animations
            if (_rubyPunchScale > 1.001f && _rubyIconRt != null)
            {
                _rubyPunchScale = Mathf.MoveTowards(_rubyPunchScale, 1.0f, Time.unscaledDeltaTime * 2.5f);
                _rubyIconRt.localScale = Vector3.one * _rubyPunchScale;
            }

            if (_emeraldPunchScale > 1.001f && _emeraldIconRt != null)
            {
                _emeraldPunchScale = Mathf.MoveTowards(_emeraldPunchScale, 1.0f, Time.unscaledDeltaTime * 2.5f);
                _emeraldIconRt.localScale = Vector3.one * _emeraldPunchScale;
            }

            if (_amethystPunchScale > 1.001f && _amethystIconRt != null)
            {
                _amethystPunchScale = Mathf.MoveTowards(_amethystPunchScale, 1.0f, Time.unscaledDeltaTime * 2.5f);
                _amethystIconRt.localScale = Vector3.one * _amethystPunchScale;
            }
        }

        private void OnGemCollected(GemStoneCollectedEvent evt)
        {
            switch (evt.GemType)
            {
                case GemType.Ruby:
                    RunRubyCount++;
                    _rubyPunchScale = 1.40f;
                    break;
                case GemType.Emerald:
                    RunEmeraldCount++;
                    _emeraldPunchScale = 1.40f;
                    break;
                case GemType.Amethyst:
                    RunAmethystCount++;
                    _amethystPunchScale = 1.40f;
                    break;
            }

            UpdateCounters();
        }

        private void OnGoldGained(GoldGainedEvent evt)
        {
            RunGoldCount = evt.TotalGold;
        }

        private void UpdateCounters()
        {
            if (_rubyText != null) _rubyText.text = RunRubyCount.ToString();
            if (_emeraldText != null) _emeraldText.text = RunEmeraldCount.ToString();
            if (_amethystText != null) _amethystText.text = RunAmethystCount.ToString();
        }

        public void ResetRun()
        {
            RunGoldCount = 0;
            RunRubyCount = 0;
            RunEmeraldCount = 0;
            RunAmethystCount = 0;
            _rubyPunchScale = 1f;
            _emeraldPunchScale = 1f;
            _amethystPunchScale = 1f;
            UpdateCounters();
        }
    }
}
