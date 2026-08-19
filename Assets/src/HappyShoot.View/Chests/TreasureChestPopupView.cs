using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using HappyShoot.Domain.Events;
using HappyShoot.Domain.Leveling;

namespace HappyShoot.View.Chests
{
    /// <summary>
    /// Glorious celebratory popup opened when a player collects a treasure chest.
    /// Displays awarded skills and bonus gold with instant resume flow.
    /// </summary>
    public class TreasureChestPopupView : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private GameObject _panelRoot;
        [SerializeField] private Text _bonusGoldText;
        [SerializeField] private Text _rewardListText;
        [SerializeField] private Button _confirmButton;

        private EventBus _eventBus;

        public void Initialize(EventBus eventBus)
        {
            _eventBus = eventBus;
            EnsureUiElements();

            if (_eventBus != null)
            {
                _eventBus.Subscribe<TreasureChestOpenedEvent>(OnChestOpened);
            }

            if (_panelRoot != null)
            {
                _panelRoot.SetActive(false);
            }
        }

        private void OnChestOpened(TreasureChestOpenedEvent evt)
        {
            ShowChestPopup(evt.Rewards, evt.BonusGold);
        }

        public void ShowChestPopup(IReadOnlyList<SkillRewardOption> rewards, int bonusGold)
        {
            if (_panelRoot != null)
            {
                _panelRoot.SetActive(true);
            }

            if (_bonusGoldText != null)
            {
                _bonusGoldText.text = $"💰 +{bonusGold} GOLD COLLECTED!";
            }

            if (_rewardListText != null)
            {
                if (rewards != null && rewards.Count > 0)
                {
                    var lines = new List<string>(rewards.Count);
                    for (int i = 0; i < rewards.Count; i++)
                    {
                        lines.Add($"⭐ {rewards[i].Title}: {rewards[i].Description}");
                    }
                    _rewardListText.text = string.Join("\n\n", lines);
                }
                else
                {
                    _rewardListText.text = "⭐ Maximum Skills Reached! Bonus power empowered!";
                }
            }

            Time.timeScale = 0f;
        }

        public void OnConfirmClicked()
        {
            if (_panelRoot != null)
            {
                _panelRoot.SetActive(false);
            }
            Time.timeScale = 1f;
        }

        private void EnsureUiElements()
        {
            if (_panelRoot != null) return;

            var canvasGo = new GameObject("ChestPopupCanvas");
            var canvas = canvasGo.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 80;
            canvasGo.AddComponent<CanvasScaler>().uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            canvasGo.AddComponent<GraphicRaycaster>();

            // Dark golden translucent backdrop
            _panelRoot = new GameObject("ChestPopupPanel");
            _panelRoot.transform.SetParent(canvasGo.transform, false);
            var panelRt = _panelRoot.AddComponent<RectTransform>();
            panelRt.anchorMin = Vector2.zero;
            panelRt.anchorMax = Vector2.one;
            panelRt.sizeDelta = Vector2.zero;
            var panelImg = _panelRoot.AddComponent<Image>();
            panelImg.color = new Color(0.15f, 0.10f, 0.02f, 0.90f);

            // Dialog container
            var dialogGo = new GameObject("DialogBox");
            dialogGo.transform.SetParent(_panelRoot.transform, false);
            var dialogRt = dialogGo.AddComponent<RectTransform>();
            dialogRt.anchorMin = new Vector2(0.5f, 0.5f);
            dialogRt.anchorMax = new Vector2(0.5f, 0.5f);
            dialogRt.sizeDelta = new Vector2(440f, 460f);
            var dialogImg = dialogGo.AddComponent<Image>();
            dialogImg.color = new Color(0.22f, 0.18f, 0.10f, 0.98f);

            // Title
            CreateText(dialogGo.transform, "Title", "🎁 TREASURE CHEST OPENED! 🎁", 24, TextAnchor.MiddleCenter, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -40f), new Vector2(400f, 40f), new Color(1f, 0.88f, 0.25f, 1f));

            // Gold Text
            _bonusGoldText = CreateText(dialogGo.transform, "GoldText", "💰 +100 GOLD COLLECTED!", 20, TextAnchor.MiddleCenter, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -90f), new Vector2(380f, 30f), new Color(1f, 0.75f, 0.2f, 1f));

            // Reward Items Text
            _rewardListText = CreateText(dialogGo.transform, "RewardText", "⭐ Rewards Received!", 16, TextAnchor.MiddleLeft, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, 10f), new Vector2(380f, 180f), Color.white);

            // Confirm Button
            _confirmButton = CreateButton(dialogGo.transform, "ClaimBtn", "✨ CLAIM REWARDS ✨", new Vector2(0f, -145f), new Color(0.9f, 0.65f, 0.15f, 1f), OnConfirmClicked);
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
            rt.sizeDelta = new Vector2(280f, 52f);

            var img = btnGo.AddComponent<Image>();
            img.color = btnColor;

            var btn = btnGo.AddComponent<Button>();
            btn.targetGraphic = img;
            btn.onClick.AddListener(onClick);

            CreateText(btnGo.transform, "Label", label, 18, TextAnchor.MiddleCenter, Vector2.zero, Vector2.one, new Vector2(0.5f, 0.5f), Vector2.zero, Vector2.zero, Color.black);

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
