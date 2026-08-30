using UnityEngine;
using UnityEngine.UI;
using HappyShoot.Domain.Events;
using HappyShoot.Domain.Progression;
using HappyShoot.View.Utils;

namespace HappyShoot.View.UI
{
    /// <summary>
    /// Displays real-time run currency (Gold + Ruby, Emerald, Amethyst) at the top of the HUD.
    /// Tracks all loot collected during the current run for end-of-run saving and player clarity.
    /// Strictly modular and under 500 lines.
    /// </summary>
    public class InGameGemCounterHudView : MonoBehaviour
    {
        private EventBus _eventBus;
        private Text _lootText;

        public int RunGoldCount { get; private set; }
        public int RunRubyCount { get; private set; }
        public int RunEmeraldCount { get; private set; }
        public int RunAmethystCount { get; private set; }

        public void Initialize(EventBus eventBus, Transform parent)
        {
            _eventBus = eventBus;
            if (_eventBus != null)
            {
                _eventBus.Subscribe<GemStoneCollectedEvent>(OnGemCollected);
                _eventBus.Subscribe<GoldGainedEvent>(OnGoldGained);
            }

            BuildUi(parent);
            UpdateText();
        }

        private void OnDestroy()
        {
            if (_eventBus != null)
            {
                _eventBus.Unsubscribe<GemStoneCollectedEvent>(OnGemCollected);
                _eventBus.Unsubscribe<GoldGainedEvent>(OnGoldGained);
            }
        }

        private void BuildUi(Transform parent)
        {
            Transform targetParent = parent;
            if (parent != null)
            {
                var canvas = parent.GetComponent<Canvas>() ?? parent.GetComponentInChildren<Canvas>();
                if (canvas != null)
                {
                    targetParent = canvas.transform;
                }
            }

            // Container Capsule Bar (Top Center, below Timer)
            var go = new GameObject("RunLootCounterHud");
            go.transform.SetParent(targetParent, false);
            var rt = go.AddComponent<RectTransform>();
            rt.anchorMin = new Vector2(0.5f, 1f);
            rt.anchorMax = new Vector2(0.5f, 1f);
            rt.pivot = new Vector2(0.5f, 1f);
            rt.anchoredPosition = new Vector2(0f, -62f);
            rt.sizeDelta = new Vector2(430f, 32f);

            // Capsule Background
            var bgImg = go.AddComponent<Image>();
            bgImg.sprite = SpriteHelper.GetOrCreateWhiteSprite();
            bgImg.color = new Color(0.06f, 0.08f, 0.14f, 0.85f);

            var outline = go.AddComponent<Outline>();
            outline.effectColor = new Color(0.85f, 0.70f, 0.30f, 0.35f);
            outline.effectDistance = new Vector2(1.5f, -1.5f);

            // Currency Text
            var textGo = new GameObject("LootText");
            textGo.transform.SetParent(go.transform, false);
            var textRt = textGo.AddComponent<RectTransform>();
            textRt.anchorMin = Vector2.zero;
            textRt.anchorMax = Vector2.one;
            textRt.offsetMin = new Vector2(10f, 0f);
            textRt.offsetMax = new Vector2(-10f, 0f);

            _lootText = textGo.AddComponent<Text>();
            _lootText.font = FontHelper.GetKoreanFont();
            _lootText.fontSize = 16;
            _lootText.fontStyle = FontStyle.Bold;
            _lootText.alignment = TextAnchor.MiddleCenter;
            _lootText.color = Color.white;
            _lootText.raycastTarget = false;
        }

        private void OnGemCollected(GemStoneCollectedEvent evt)
        {
            switch (evt.GemType)
            {
                case GemType.Ruby: RunRubyCount++; break;
                case GemType.Emerald: RunEmeraldCount++; break;
                case GemType.Amethyst: RunAmethystCount++; break;
            }

            UpdateText();
        }

        private void OnGoldGained(GoldGainedEvent evt)
        {
            RunGoldCount = evt.TotalGold;
            UpdateText();
        }

        private void UpdateText()
        {
            if (_lootText != null)
            {
                _lootText.text = $"💰 {RunGoldCount:N0} G   │   🔴 {RunRubyCount}   🟢 {RunEmeraldCount}   🟣 {RunAmethystCount}";
            }
        }

        public void ResetRun()
        {
            RunGoldCount = 0;
            RunRubyCount = 0;
            RunEmeraldCount = 0;
            RunAmethystCount = 0;
            UpdateText();
        }
    }
}
