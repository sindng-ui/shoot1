using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using HappyShoot.Domain.Events;
using HappyShoot.Domain.Leveling;
using HappyShoot.View.Utils;

namespace HappyShoot.View.Chests
{
    /// <summary>
    /// Glorious celebratory popup opened when a player collects a treasure chest from boss/elites.
    /// Displays awarded skills with large 80x80 icons, clear readable Korean descriptions,
    /// bonus gold, and instant resume on Space/Enter/Click.
    /// Strictly modular and under 500 lines.
    /// </summary>
    public class TreasureChestPopupView : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private GameObject _panelRoot;
        [SerializeField] private Text _bonusGoldText;
        [SerializeField] private Transform _rewardCardsContainer;
        [SerializeField] private Button _confirmButton;

        private readonly List<GameObject> _spawnedCardObjects = new List<GameObject>(4);
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
            EnsureUiElements();

            if (_panelRoot != null)
            {
                _panelRoot.SetActive(true);
            }

            if (_bonusGoldText != null)
            {
                _bonusGoldText.text = $"💰 +{bonusGold} GOLD 획득!";
            }

            PopulateRewardCards(rewards);

            HitStopManager.Instance?.CancelHitStop();
            Time.timeScale = 0f;
        }

        private void PopulateRewardCards(IReadOnlyList<SkillRewardOption> rewards)
        {
            if (_rewardCardsContainer == null) return;

            // Clear previous cards
            for (int i = 0; i < _spawnedCardObjects.Count; i++)
            {
                if (_spawnedCardObjects[i] != null) Destroy(_spawnedCardObjects[i]);
            }
            _spawnedCardObjects.Clear();

            Font koreanFont = FontHelper.GetKoreanFont();

            if (rewards != null && rewards.Count > 0)
            {
                for (int i = 0; i < rewards.Count; i++)
                {
                    var opt = rewards[i];
                    var cardGo = CreateRewardCardItem(_rewardCardsContainer, opt, koreanFont);
                    _spawnedCardObjects.Add(cardGo);
                }
            }
            else
            {
                // Max skills fallback notice
                var noticeGo = new GameObject("MaxSkillNotice");
                noticeGo.transform.SetParent(_rewardCardsContainer, false);
                var noticeRt = noticeGo.AddComponent<RectTransform>();
                noticeRt.sizeDelta = new Vector2(580f, 100f);

                var txt = noticeGo.AddComponent<Text>();
                txt.text = "⭐ 모든 스킬이 최고 레벨에 도달했습니다!\n추가 골드와 경험치가 지급되었습니다.";
                txt.fontSize = 20;
                txt.fontStyle = FontStyle.Bold;
                txt.alignment = TextAnchor.MiddleCenter;
                txt.color = new Color(1f, 0.9f, 0.4f, 1f);
                txt.font = koreanFont;
                _spawnedCardObjects.Add(noticeGo);
            }
        }

        private GameObject CreateRewardCardItem(Transform parent, SkillRewardOption opt, Font font)
        {
            var cardGo = new GameObject($"RewardCard_{opt.Id}");
            cardGo.transform.SetParent(parent, false);

            var rt = cardGo.AddComponent<RectTransform>();
            rt.sizeDelta = new Vector2(580f, 110f);

            var le = cardGo.AddComponent<LayoutElement>();
            le.preferredWidth = 580f;
            le.preferredHeight = 110f;
            le.minWidth = 580f;
            le.minHeight = 110f;

            var bgImg = cardGo.AddComponent<Image>();
            bgImg.sprite = SpriteHelper.GetOrCreateWhiteSprite();
            bgImg.color = new Color(0.12f, 0.14f, 0.20f, 0.95f);

            var outline = cardGo.AddComponent<Outline>();
            outline.effectColor = new Color(1f, 0.75f, 0.2f, 0.85f);
            outline.effectDistance = new Vector2(2f, -2f);

            // 1. Icon Frame (Left)
            var iconFrameGo = new GameObject("IconFrame");
            iconFrameGo.transform.SetParent(cardGo.transform, false);
            var iconFrameRt = iconFrameGo.AddComponent<RectTransform>();
            iconFrameRt.anchorMin = new Vector2(0f, 0.5f);
            iconFrameRt.anchorMax = new Vector2(0f, 0.5f);
            iconFrameRt.pivot = new Vector2(0f, 0.5f);
            iconFrameRt.anchoredPosition = new Vector2(16f, 0f);
            iconFrameRt.sizeDelta = new Vector2(76f, 76f);

            var iconFrameImg = iconFrameGo.AddComponent<Image>();
            iconFrameImg.sprite = SpriteHelper.GetOrCreateWhiteSprite();
            iconFrameImg.color = new Color(0.06f, 0.08f, 0.12f, 0.95f);

            var iconOutline = iconFrameGo.AddComponent<Outline>();
            iconOutline.effectColor = new Color(1f, 0.85f, 0.25f, 0.9f);
            iconOutline.effectDistance = new Vector2(1.5f, -1.5f);

            var iconImgGo = new GameObject("IconImage");
            iconImgGo.transform.SetParent(iconFrameGo.transform, false);
            var iconImgRt = iconImgGo.AddComponent<RectTransform>();
            iconImgRt.anchorMin = Vector2.zero;
            iconImgRt.anchorMax = Vector2.one;
            iconImgRt.sizeDelta = new Vector2(-6f, -6f);
            var iconImg = iconImgGo.AddComponent<Image>();
            iconImg.sprite = RewardIconHelper.GetOrCreateRewardIcon(opt.Id, size: 80);
            iconImg.preserveAspect = true;

            // 2. Title Text (Top Right)
            var titleGo = new GameObject("TitleText");
            titleGo.transform.SetParent(cardGo.transform, false);
            var titleRt = titleGo.AddComponent<RectTransform>();
            titleRt.anchorMin = new Vector2(0f, 1f);
            titleRt.anchorMax = new Vector2(1f, 1f);
            titleRt.pivot = new Vector2(0f, 1f);
            titleRt.anchoredPosition = new Vector2(108f, -14f);
            titleRt.sizeDelta = new Vector2(-124f, 30f);

            var titleTxt = titleGo.AddComponent<Text>();
            titleTxt.text = opt.Title;
            titleTxt.fontSize = 21;
            titleTxt.fontStyle = FontStyle.Bold;
            titleTxt.alignment = TextAnchor.MiddleLeft;
            titleTxt.color = new Color(1.0f, 0.88f, 0.25f, 1.0f);
            titleTxt.font = font;

            // 3. Description Text (Bottom Right - Large and clear)
            var descGo = new GameObject("DescText");
            descGo.transform.SetParent(cardGo.transform, false);
            var descRt = descGo.AddComponent<RectTransform>();
            descRt.anchorMin = new Vector2(0f, 0f);
            descRt.anchorMax = new Vector2(1f, 1f);
            descRt.pivot = new Vector2(0f, 0f);
            descRt.anchoredPosition = new Vector2(108f, -10f);
            descRt.sizeDelta = new Vector2(-124f, -48f);

            var descTxt = descGo.AddComponent<Text>();
            descTxt.text = opt.Description;
            descTxt.fontSize = 17;
            descTxt.fontStyle = FontStyle.Normal;
            descTxt.alignment = TextAnchor.UpperLeft;
            descTxt.color = new Color(0.92f, 0.94f, 0.98f, 1.0f);
            descTxt.font = font;
            descTxt.lineSpacing = 1.15f;

            cardGo.SetActive(true);
            return cardGo;
        }

        public void OnConfirmClicked()
        {
            if (_panelRoot != null)
            {
                _panelRoot.SetActive(false);
            }
            HitStopManager.Instance?.CancelHitStop();
            Time.timeScale = 1f;
        }

        private void Update()
        {
            if (_panelRoot == null || !_panelRoot.activeSelf) return;

            bool confirm = false;
            var kb = Keyboard.current;
            if (kb != null)
            {
                if (kb.spaceKey.wasPressedThisFrame || kb.enterKey.wasPressedThisFrame ||
                    kb.numpadEnterKey.wasPressedThisFrame || kb.escapeKey.wasPressedThisFrame ||
                    kb.digit1Key.wasPressedThisFrame || kb.digit2Key.wasPressedThisFrame || kb.digit3Key.wasPressedThisFrame)
                {
                    confirm = true;
                }
            }

            try
            {
                if (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.Return) ||
                    Input.GetKeyDown(KeyCode.KeypadEnter) || Input.GetKeyDown(KeyCode.Escape) ||
                    Input.GetKeyDown(KeyCode.Alpha1) || Input.GetKeyDown(KeyCode.Alpha2) || Input.GetKeyDown(KeyCode.Alpha3))
                {
                    confirm = true;
                }
            }
            catch { /* Ignore if legacy input disabled */ }

            if (confirm)
            {
                OnConfirmClicked();
            }
        }

        private void EnsureUiElements()
        {
            if (_panelRoot != null) return;

            var canvasGo = new GameObject("ChestPopupCanvas");
            var canvas = canvasGo.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 80;
            var scaler = canvasGo.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1440, 810);
            scaler.matchWidthOrHeight = 1.0f;
            canvasGo.AddComponent<GraphicRaycaster>();

            // Dark golden translucent backdrop
            _panelRoot = new GameObject("ChestPopupPanel");
            _panelRoot.transform.SetParent(canvasGo.transform, false);
            var panelRt = _panelRoot.AddComponent<RectTransform>();
            panelRt.anchorMin = Vector2.zero;
            panelRt.anchorMax = Vector2.one;
            panelRt.sizeDelta = Vector2.zero;
            var panelImg = _panelRoot.AddComponent<Image>();
            panelImg.color = new Color(0.08f, 0.06f, 0.02f, 0.92f);

            // Dialog container (Spacious 640x580)
            var dialogGo = new GameObject("DialogBox");
            dialogGo.transform.SetParent(_panelRoot.transform, false);
            var dialogRt = dialogGo.AddComponent<RectTransform>();
            dialogRt.anchorMin = new Vector2(0.5f, 0.5f);
            dialogRt.anchorMax = new Vector2(0.5f, 0.5f);
            dialogRt.sizeDelta = new Vector2(640f, 580f);
            var dialogImg = dialogGo.AddComponent<Image>();
            dialogImg.sprite = SpriteHelper.GetOrCreateWhiteSprite();
            dialogImg.color = new Color(0.16f, 0.13f, 0.08f, 0.98f);
            var dialogOutline = dialogGo.AddComponent<Outline>();
            dialogOutline.effectColor = new Color(1.0f, 0.80f, 0.25f, 0.90f);
            dialogOutline.effectDistance = new Vector2(3f, -3f);

            // Title
            CreateText(dialogGo.transform, "Title", "🎁 보물 상자 개봉! 🎁", 26, TextAnchor.MiddleCenter, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -36f), new Vector2(580f, 40f), new Color(1f, 0.88f, 0.25f, 1f));

            // Gold Text
            _bonusGoldText = CreateText(dialogGo.transform, "GoldText", "💰 +100 GOLD 획득!", 21, TextAnchor.MiddleCenter, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -80f), new Vector2(580f, 32f), new Color(1f, 0.75f, 0.2f, 1f));

            // Reward Cards Container
            var containerGo = new GameObject("CardsContainer");
            containerGo.transform.SetParent(dialogGo.transform, false);
            var cRt = containerGo.AddComponent<RectTransform>();
            cRt.anchorMin = new Vector2(0.5f, 0.5f);
            cRt.anchorMax = new Vector2(0.5f, 0.5f);
            cRt.pivot = new Vector2(0.5f, 0.5f);
            cRt.anchoredPosition = new Vector2(0f, 10f);
            cRt.sizeDelta = new Vector2(600f, 360f);

            var vlg = containerGo.AddComponent<VerticalLayoutGroup>();
            vlg.spacing = 14f;
            vlg.childAlignment = TextAnchor.MiddleCenter;
            vlg.childControlWidth = false;
            vlg.childControlHeight = false;
            vlg.childForceExpandWidth = false;
            vlg.childForceExpandHeight = false;

            _rewardCardsContainer = containerGo.transform;

            // Confirm Button with Space/Enter guide
            _confirmButton = CreateButton(dialogGo.transform, "ClaimBtn", "✨ [ 스페이스 / 클릭 ] 확인 ✨", new Vector2(0f, -240f), new Color(0.95f, 0.70f, 0.15f, 1f), OnConfirmClicked);
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
            rt.sizeDelta = new Vector2(340f, 54f);

            var img = btnGo.AddComponent<Image>();
            img.sprite = SpriteHelper.GetOrCreateWhiteSprite();
            img.color = btnColor;

            var btnOutline = btnGo.AddComponent<Outline>();
            btnOutline.effectColor = new Color(0.4f, 0.25f, 0.05f, 0.9f);
            btnOutline.effectDistance = new Vector2(2f, -2f);

            var btn = btnGo.AddComponent<Button>();
            btn.targetGraphic = img;
            btn.onClick.AddListener(onClick);

            CreateText(btnGo.transform, "Label", label, 20, TextAnchor.MiddleCenter, Vector2.zero, Vector2.one, new Vector2(0.5f, 0.5f), Vector2.zero, Vector2.zero, new Color(0.12f, 0.08f, 0.02f, 1.0f));

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
            txt.font = FontHelper.GetKoreanFont();
            return txt;
        }
    }
}
