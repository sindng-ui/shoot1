using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using HappyShoot.Domain.Entities;
using HappyShoot.Domain.Events;
using HappyShoot.Domain.Leveling;
using HappyShoot.View.Player;
using HappyShoot.View.Utils;

namespace HappyShoot.View.UI
{
    /// <summary>
    /// Unity UI View that manages the large 3-choice skill selection popup with 80x80 icons and Korean typography.
    /// Supports full procedural UI building with crisp layout scaling.
    /// </summary>
    public class LevelUpUiView : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private PlayerView _playerView;
        [SerializeField] private GameObject _panelRoot;
        [SerializeField] private Transform _cardsContainer;
        [SerializeField] private GameObject _rewardCardPrefab;

        private LevelSystem _levelSystem;
        private SkillRewardManager _rewardManager;
        private List<SkillRewardOption> _currentOptions;
        private readonly List<GameObject> _cardObjects = new List<GameObject>(3);

        public LevelSystem LevelSystem => _levelSystem;
        public SkillRewardManager RewardManager => _rewardManager;

        public void Initialize(PlayerView playerView, LevelSystem levelSystem, SkillRewardManager rewardManager)
        {
            _playerView = playerView;
            _levelSystem = levelSystem;
            _rewardManager = rewardManager;

            if (_levelSystem != null)
            {
                _levelSystem.OnLevelUp += ShowLevelUpPopup;
            }

            EnsureUiElements();

            if (_panelRoot != null)
            {
                _panelRoot.SetActive(false);
            }
        }

        public void ShowLevelUpPopup(int newLevel)
        {
            if (_rewardManager == null || _playerView == null || _playerView.Entity == null)
                return;

            _currentOptions = _rewardManager.RollRewards(_playerView.Entity, count: 3);
            if (_currentOptions == null || _currentOptions.Count == 0)
                return;

            EnsureUiElements();
            PopulateCards();

            if (_panelRoot != null)
            {
                _panelRoot.SetActive(true);
            }

            // Publish sound event
            _playerView.EventBus?.Publish(new PlaySoundEvent(SoundEffectType.LevelUp));

            // Cancel any ongoing Hit-Stop micro-freeze and pause game
            HitStopManager.Instance?.CancelHitStop();
            Time.timeScale = 0f;

            Debug.Log($"[LevelUpUiView] Level Up to Lv.{newLevel}! Showing {_currentOptions.Count} reward cards.");
        }

        private void PopulateCards()
        {
            if (_cardsContainer == null || _currentOptions == null) return;

            // Clear previous cards
            for (int i = 0; i < _cardObjects.Count; i++)
            {
                if (_cardObjects[i] != null) Destroy(_cardObjects[i]);
            }
            _cardObjects.Clear();

            // Create cards for each option
            for (int i = 0; i < _currentOptions.Count; i++)
            {
                int index = i;
                var opt = _currentOptions[i];

                var cardGo = CreateCardObject(_cardsContainer, opt, index);
                _cardObjects.Add(cardGo);
            }
        }

        private GameObject CreateCardObject(Transform parent, SkillRewardOption option, int index)
        {
            var cardGo = new GameObject($"Card_{index}_{option.Id}");
            cardGo.transform.SetParent(parent, false);

            var rt = cardGo.AddComponent<RectTransform>();
            rt.sizeDelta = new Vector2(320f, 460f);

            var le = cardGo.AddComponent<LayoutElement>();
            le.preferredWidth = 320f;
            le.preferredHeight = 460f;
            le.minWidth = 320f;
            le.minHeight = 460f;

            var img = cardGo.AddComponent<Image>();
            img.sprite = SpriteHelper.GetOrCreateWhiteSprite();
            img.color = GetCardColor(option.Category);

            var btn = cardGo.AddComponent<Button>();
            btn.targetGraphic = img;
            var colors = btn.colors;
            colors.highlightedColor = new Color(1.2f, 1.2f, 1.2f, 1f);
            colors.pressedColor = new Color(0.7f, 0.7f, 0.7f, 1f);
            btn.colors = colors;
            btn.onClick.AddListener(() => SelectOption(index));

            // Outline border
            var outline = cardGo.AddComponent<Outline>();
            outline.effectColor = GetCategoryBorderColor(option.Category);
            outline.effectDistance = new Vector2(3.5f, -3.5f);

            Font koreanFont = FontHelper.GetKoreanFont();

            // 1. Top Category Badge
            var badgeGo = new GameObject("BadgeText");
            badgeGo.transform.SetParent(cardGo.transform, false);
            var badgeRt = badgeGo.AddComponent<RectTransform>();
            badgeRt.anchorMin = new Vector2(0f, 1f);
            badgeRt.anchorMax = new Vector2(1f, 1f);
            badgeRt.pivot = new Vector2(0.5f, 1f);
            badgeRt.anchoredPosition = new Vector2(0f, -14f);
            badgeRt.sizeDelta = new Vector2(-20f, 26f);
            var badgeTxt = badgeGo.AddComponent<Text>();
            badgeTxt.text = GetCategoryBadgeText(option);
            badgeTxt.fontSize = 14;
            badgeTxt.fontStyle = FontStyle.Bold;
            badgeTxt.alignment = TextAnchor.MiddleCenter;
            badgeTxt.color = GetCategoryBorderColor(option.Category);
            badgeTxt.font = koreanFont;

            // 2. 80x80 Icon Frame & Image
            var iconBgGo = new GameObject("IconFrame");
            iconBgGo.transform.SetParent(cardGo.transform, false);
            var iconBgRt = iconBgGo.AddComponent<RectTransform>();
            iconBgRt.anchorMin = new Vector2(0.5f, 1f);
            iconBgRt.anchorMax = new Vector2(0.5f, 1f);
            iconBgRt.pivot = new Vector2(0.5f, 1f);
            iconBgRt.anchoredPosition = new Vector2(0f, -48f);
            iconBgRt.sizeDelta = new Vector2(88f, 88f);
            var iconBgImg = iconBgGo.AddComponent<Image>();
            iconBgImg.sprite = SpriteHelper.GetOrCreateWhiteSprite();
            iconBgImg.color = new Color(0.06f, 0.08f, 0.12f, 0.95f);
            var iconOutline = iconBgGo.AddComponent<Outline>();
            iconOutline.effectColor = GetCategoryBorderColor(option.Category) * 0.8f;
            iconOutline.effectDistance = new Vector2(2f, -2f);

            var iconImgGo = new GameObject("IconImage");
            iconImgGo.transform.SetParent(iconBgGo.transform, false);
            var iconImgRt = iconImgGo.AddComponent<RectTransform>();
            iconImgRt.anchorMin = Vector2.zero;
            iconImgRt.anchorMax = Vector2.one;
            iconImgRt.sizeDelta = new Vector2(-8f, -8f);
            var iconImg = iconImgGo.AddComponent<Image>();
            iconImg.sprite = RewardIconHelper.GetOrCreateRewardIcon(option.Id, size: 80);
            iconImg.preserveAspect = true;

            // 3. Title Text (Bold)
            var titleGo = new GameObject("TitleText");
            titleGo.transform.SetParent(cardGo.transform, false);
            var titleRt = titleGo.AddComponent<RectTransform>();
            titleRt.anchorMin = new Vector2(0f, 1f);
            titleRt.anchorMax = new Vector2(1f, 1f);
            titleRt.pivot = new Vector2(0.5f, 1f);
            titleRt.anchoredPosition = new Vector2(0f, -146f);
            titleRt.sizeDelta = new Vector2(-24f, 36f);
            var titleTxt = titleGo.AddComponent<Text>();
            titleTxt.text = option.Title;
            titleTxt.fontSize = 20;
            titleTxt.fontStyle = FontStyle.Bold;
            titleTxt.alignment = TextAnchor.MiddleCenter;
            titleTxt.color = Color.white;
            titleTxt.font = koreanFont;

            // 4. Description Text
            var descGo = new GameObject("DescText");
            descGo.transform.SetParent(cardGo.transform, false);
            var descRt = descGo.AddComponent<RectTransform>();
            descRt.anchorMin = new Vector2(0f, 0f);
            descRt.anchorMax = new Vector2(1f, 1f);
            descRt.pivot = new Vector2(0.5f, 0.5f);
            descRt.anchoredPosition = new Vector2(0f, -30f);
            descRt.sizeDelta = new Vector2(-36f, -260f);
            var descTxt = descGo.AddComponent<Text>();
            descTxt.text = option.Description;
            descTxt.fontSize = 15;
            descTxt.alignment = TextAnchor.MiddleCenter;
            descTxt.color = new Color(0.92f, 0.95f, 1.0f, 1f);
            descTxt.font = koreanFont;

            // 5. Select Button Background & text at bottom
            var selectBtnBg = new GameObject("SelectBtnBg");
            selectBtnBg.transform.SetParent(cardGo.transform, false);
            var sBgRt = selectBtnBg.AddComponent<RectTransform>();
            sBgRt.anchorMin = new Vector2(0f, 0f);
            sBgRt.anchorMax = new Vector2(1f, 0f);
            sBgRt.pivot = new Vector2(0.5f, 0f);
            sBgRt.anchoredPosition = new Vector2(0f, 18f);
            sBgRt.sizeDelta = new Vector2(-48f, 44f);
            var sBgImg = selectBtnBg.AddComponent<Image>();
            sBgImg.sprite = SpriteHelper.GetOrCreateWhiteSprite();
            sBgImg.color = new Color(0.12f, 0.14f, 0.20f, 0.95f);
            var btnOutline = selectBtnBg.AddComponent<Outline>();
            btnOutline.effectColor = new Color(1f, 0.85f, 0.2f, 0.8f);
            btnOutline.effectDistance = new Vector2(1.5f, -1.5f);

            var sTxt = CreateTextChild(selectBtnBg.transform, "선 택 하 기", 17, Color.yellow, koreanFont);

            cardGo.SetActive(true);
            return cardGo;
        }

        private Text CreateTextChild(Transform parent, string text, int fontSize, Color color, Font font)
        {
            var go = new GameObject("Text");
            go.transform.SetParent(parent, false);
            var rt = go.AddComponent<RectTransform>();
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.sizeDelta = Vector2.zero;
            var txt = go.AddComponent<Text>();
            txt.text = text;
            txt.fontSize = fontSize;
            txt.fontStyle = FontStyle.Bold;
            txt.alignment = TextAnchor.MiddleCenter;
            txt.color = color;
            txt.font = font;
            return txt;
        }

        private Color GetCardColor(RewardCategory category)
        {
            switch (category)
            {
                case RewardCategory.EvolveSkill: return new Color(0.28f, 0.12f, 0.35f, 0.95f);
                case RewardCategory.NewActiveSkill: return new Color(0.12f, 0.22f, 0.35f, 0.95f);
                case RewardCategory.UpgradeActiveSkill: return new Color(0.14f, 0.26f, 0.24f, 0.95f);
                case RewardCategory.NewPassive:
                case RewardCategory.UpgradePassive: return new Color(0.25f, 0.22f, 0.12f, 0.95f);
                default: return new Color(0.18f, 0.18f, 0.22f, 0.95f);
            }
        }

        private Color GetCategoryBorderColor(RewardCategory category)
        {
            switch (category)
            {
                case RewardCategory.EvolveSkill: return new Color(1f, 0.4f, 0.9f, 1f);
                case RewardCategory.NewActiveSkill: return new Color(0.4f, 0.8f, 1f, 1f);
                case RewardCategory.UpgradeActiveSkill: return new Color(0.4f, 1f, 0.6f, 1f);
                case RewardCategory.NewPassive:
                case RewardCategory.UpgradePassive: return new Color(1f, 0.85f, 0.3f, 1f);
                default: return Color.white;
            }
        }

        private string GetCategoryBadgeText(SkillRewardOption opt)
        {
            switch (opt.Category)
            {
                case RewardCategory.EvolveSkill: return "★ 궁극 무기 진화 ★";
                case RewardCategory.NewActiveSkill: return "[ 신규 무기 획득 ]";
                case RewardCategory.UpgradeActiveSkill: return $"[ 무기 강화 Lv.{opt.CurrentLevel + 1} ]";
                case RewardCategory.NewPassive: return "[ 신규 패시브 획득 ]";
                case RewardCategory.UpgradePassive: return $"[ 패시브 강화 Lv.{opt.CurrentLevel + 1} ]";
                default: return "[ 능력 강화 ]";
            }
        }

        private void EnsureUiElements()
        {
            if (_panelRoot != null) return;

            // Ensure EventSystem exists for UI button interaction
            if (FindFirstObjectByType<UnityEngine.EventSystems.EventSystem>() == null)
            {
                var eventSystemGo = new GameObject("EventSystem");
                eventSystemGo.AddComponent<UnityEngine.EventSystems.EventSystem>();
                eventSystemGo.AddComponent<UnityEngine.InputSystem.UI.InputSystemUIInputModule>();
            }

            // Create Canvas
            var canvasGo = new GameObject("LevelUpCanvas");
            var canvas = canvasGo.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 20; // Above HUD
            var scaler = canvasGo.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            scaler.matchWidthOrHeight = 0.5f;
            canvasGo.AddComponent<GraphicRaycaster>();

            _panelRoot = new GameObject("LevelUpPanelRoot");
            _panelRoot.transform.SetParent(canvasGo.transform, false);

            var panelRt = _panelRoot.AddComponent<RectTransform>();
            panelRt.anchorMin = Vector2.zero;
            panelRt.anchorMax = Vector2.one;
            panelRt.sizeDelta = Vector2.zero;

            // Dim Background with white sprite
            var bgImg = _panelRoot.AddComponent<Image>();
            bgImg.sprite = SpriteHelper.GetOrCreateWhiteSprite();
            bgImg.color = new Color(0.04f, 0.05f, 0.08f, 0.88f);

            // Header Title
            var titleGo = new GameObject("LevelUpHeader");
            titleGo.transform.SetParent(_panelRoot.transform, false);
            var titleRt = titleGo.AddComponent<RectTransform>();
            titleRt.anchorMin = new Vector2(0.5f, 1f);
            titleRt.anchorMax = new Vector2(0.5f, 1f);
            titleRt.pivot = new Vector2(0.5f, 1f);
            titleRt.anchoredPosition = new Vector2(0f, -60f);
            titleRt.sizeDelta = new Vector2(700f, 60f);
            var titleTxt = titleGo.AddComponent<Text>();
            titleTxt.text = "★ 레벨 업! 보상을 선택하세요 ★";
            titleTxt.fontSize = 32;
            titleTxt.fontStyle = FontStyle.Bold;
            titleTxt.alignment = TextAnchor.MiddleCenter;
            titleTxt.color = new Color(1f, 0.9f, 0.2f, 1f);
            titleTxt.font = FontHelper.GetKoreanFont();

            // Cards Horizontal Layout Container
            var containerGo = new GameObject("CardsContainer");
            containerGo.transform.SetParent(_panelRoot.transform, false);
            var cRt = containerGo.AddComponent<RectTransform>();
            cRt.anchorMin = new Vector2(0.5f, 0.5f);
            cRt.anchorMax = new Vector2(0.5f, 0.5f);
            cRt.pivot = new Vector2(0.5f, 0.5f);
            cRt.anchoredPosition = new Vector2(0f, -25f);
            cRt.sizeDelta = new Vector2(1080f, 500f);

            var hlg = containerGo.AddComponent<HorizontalLayoutGroup>();
            hlg.spacing = 35f;
            hlg.childAlignment = TextAnchor.MiddleCenter;
            hlg.childControlWidth = false;
            hlg.childControlHeight = false;
            hlg.childForceExpandWidth = false;
            hlg.childForceExpandHeight = false;

            _cardsContainer = containerGo.transform;
        }

        /// <summary>
        /// Called when the player selects a card option (0, 1, or 2).
        /// </summary>
        public void SelectOption(int optionIndex)
        {
            if (_currentOptions == null || optionIndex < 0 || optionIndex >= _currentOptions.Count)
                return;

            var selected = _currentOptions[optionIndex];
            _rewardManager.ApplyReward(_playerView.Entity, selected);

            Debug.Log($"[LevelUpUiView] Selected reward: {selected.Title}");

            // Close UI and resume game
            if (_panelRoot != null)
            {
                _panelRoot.SetActive(false);
            }

            HitStopManager.Instance?.CancelHitStop();
            Time.timeScale = 1f;
        }

        private void OnDestroy()
        {
            if (_levelSystem != null)
            {
                _levelSystem.OnLevelUp -= ShowLevelUpPopup;
            }
        }
    }
}
