using UnityEngine;
using UnityEngine.UI;
using HappyShoot.Domain.Events;
using HappyShoot.Domain.Progression;

namespace HappyShoot.View.UI
{
    /// <summary>
    /// Displays real-time run gem counts (Ruby, Emerald, Amethyst) at the top of the HUD.
    /// Tracks gems collected during the current run for end-of-run saving.
    /// Strictly modular and under 500 lines.
    /// </summary>
    public class InGameGemCounterHudView : MonoBehaviour
    {
        private EventBus _eventBus;
        private Text _gemText;

        public int RunRubyCount { get; private set; }
        public int RunEmeraldCount { get; private set; }
        public int RunAmethystCount { get; private set; }

        public void Initialize(EventBus eventBus, Transform parent)
        {
            _eventBus = eventBus;
            if (_eventBus != null)
            {
                _eventBus.Subscribe<GemStoneCollectedEvent>(OnGemCollected);
            }

            BuildUi(parent);
            UpdateText();
        }

        private void BuildUi(Transform parent)
        {
            var go = new GameObject("GemCounterHud");
            go.transform.SetParent(parent, false);
            var rt = go.AddComponent<RectTransform>();
            rt.anchorMin = new Vector2(0.5f, 1f);
            rt.anchorMax = new Vector2(0.5f, 1f);
            rt.pivot = new Vector2(0.5f, 1f);
            rt.anchoredPosition = new Vector2(240, -18);
            rt.sizeDelta = new Vector2(260, 32);

            _gemText = go.AddComponent<Text>();
            _gemText.font = Utils.FontHelper.GetKoreanFont();
            _gemText.fontSize = 17;
            _gemText.alignment = TextAnchor.MiddleLeft;
            _gemText.color = Color.white;
            _gemText.raycastTarget = false;
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

        private void UpdateText()
        {
            if (_gemText != null)
            {
                _gemText.text = $"🔴 {RunRubyCount}  🟢 {RunEmeraldCount}  🟣 {RunAmethystCount}";
            }
        }

        public void ResetRun()
        {
            RunRubyCount = 0;
            RunEmeraldCount = 0;
            RunAmethystCount = 0;
            UpdateText();
        }
    }
}
