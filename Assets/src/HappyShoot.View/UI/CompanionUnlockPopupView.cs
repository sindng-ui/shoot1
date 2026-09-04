using System;
using UnityEngine;
using UnityEngine.UI;
using HappyShoot.Domain.Entities;
using HappyShoot.Domain.Events;
using HappyShoot.View.Audio;
using HappyShoot.View.Utils;

namespace HappyShoot.View.UI
{
    /// <summary>
    /// Exclusive celebratory modal popup displayed when an AI Companion (Warrior or Ranger)
    /// is permanently unlocked upon defeating the final boss (Arch-Lich Malakar).
    /// Strictly modular and under 500 lines.
    /// </summary>
    public class CompanionUnlockPopupView : MonoBehaviour
    {
        private EventBus _eventBus;
        private Action _onClosedCallback;

        private GameObject _popupRoot;
        private Image _avatarImage;
        private Text _badgeText;
        private Text _companionTitleText;
        private Text _roleDescriptionText;
        private Text _synergyNoticeText;
        private Outline _dialogOutline;
        private Image _confirmBtnImage;

        public void Initialize(EventBus eventBus, Transform parentCanvasTf)
        {
            _eventBus = eventBus;
            if (parentCanvasTf != null)
            {
                transform.SetParent(parentCanvasTf, false);
            }

            BuildUi();
            if (_popupRoot != null) _popupRoot.SetActive(false);
        }

        public void Show(CompanionType type, Action onClosed)
        {
            _onClosedCallback = onClosed;

            if (_popupRoot == null)
            {
                BuildUi();
            }

            ConfigureContent(type);

            if (_popupRoot != null)
            {
                _popupRoot.SetActive(true);
                _popupRoot.transform.SetAsLastSibling();
            }

            // Play triumphant weapon evolution fanfare!
            _eventBus?.Publish(new PlaySoundEvent(SoundEffectType.WeaponEvolve, 1.0f));

            Time.timeScale = 0f;
        }

        private void ConfigureContent(CompanionType type)
        {
            bool isWarrior = type == CompanionType.Warrior;
            var classType = isWarrior ? CharacterClassType.Warrior : CharacterClassType.Ranger;

            // Avatar Sprite (High-res 32px pixel art front sprite)
            if (_avatarImage != null)
            {
                _avatarImage.sprite = HeroSpriteHelper.GetHeroSprite(classType, HeroSpriteHelper.ViewDirection.Front, 32);
                _avatarImage.color = Color.white;
            }

            // Theme colors
            Color themeColor = isWarrior
                ? new Color(1.0f, 0.65f, 0.20f, 1.0f) // Warm Amber Gold
                : new Color(0.25f, 0.90f, 0.60f, 1.0f); // Emerald Teal

            if (_dialogOutline != null)
            {
                _dialogOutline.effectColor = new Color(themeColor.r, themeColor.g, themeColor.b, 0.85f);
            }

            if (_badgeText != null)
            {
                _badgeText.text = isWarrior ? "🛡️ 1회차 정복 보상: 신규 동료 영입!" : "🏹 2회차 정복 보상: 신규 동료 영입!";
                _badgeText.color = themeColor;
            }

            if (_companionTitleText != null)
            {
                _companionTitleText.text = isWarrior
                    ? "호위 전사 (Warrior)"
                    : "지원 궁수 (Ranger)";
            }

            if (_roleDescriptionText != null)
            {
                _roleDescriptionText.text = isWarrior
                    ? "⚔️ 대검을 휘둘러 몬스터를 베어 넘기며 마법사를 철통 호위합니다!\n적들이 접근하지 못하도록 근접 공격으로 최전선을 방어합니다."
                    : "🏹 강력한 관통 화살로 마법사의 후방과 사각지대를 엄호 저격합니다!\n원거리에서 적들을 정밀 타격하여 마법사의 전투를 지원합니다.";
            }

            if (_synergyNoticeText != null)
            {
                _synergyNoticeText.text = "💡 [원정대 시너지]: 마법사의 모든 스탯 및 레벨업 성장 특전이\n1/3 비율로 동료에게 실시간 자동 반영됩니다!";
            }

            if (_confirmBtnImage != null)
            {
                _confirmBtnImage.color = isWarrior
                    ? new Color(0.85f, 0.50f, 0.15f, 1.0f)
                    : new Color(0.18f, 0.65f, 0.45f, 1.0f);
            }
        }

        private void BuildUi()
        {
            var canvasGo = new GameObject("CompanionUnlockCanvas");
            canvasGo.transform.SetParent(transform, false);
            var canvas = canvasGo.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 130; // Above StageVictoryUI (120)
            var scaler = canvasGo.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1440, 810);
            scaler.matchWidthOrHeight = 1.0f;
            canvasGo.AddComponent<GraphicRaycaster>();

            _popupRoot = new GameObject("PopupRoot");
            _popupRoot.transform.SetParent(canvasGo.transform, false);

            var rt = _popupRoot.AddComponent<RectTransform>();
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;

            // Dark Cinematic Backdrop (No blur, lightweight dark tint)
            var bgImg = _popupRoot.AddComponent<Image>();
            bgImg.color = new Color(0.02f, 0.04f, 0.09f, 0.95f);

            // Center Dialog Card
            var dialogGo = new GameObject("DialogFrame");
            dialogGo.transform.SetParent(_popupRoot.transform, false);
            var dialogRt = dialogGo.AddComponent<RectTransform>();
            dialogRt.sizeDelta = new Vector2(620f, 560f);

            var dialogImg = dialogGo.AddComponent<Image>();
            dialogImg.color = new Color(0.08f, 0.11f, 0.18f, 0.98f);

            _dialogOutline = dialogGo.AddComponent<Outline>();
            _dialogOutline.effectColor = new Color(1.0f, 0.8f, 0.3f, 0.85f);
            _dialogOutline.effectDistance = new Vector2(2f, -2f);

            var font = FontHelper.GetKoreanFont();

            // 1. Badge Top Tag
            var badgeGo = new GameObject("BadgeText");
            badgeGo.transform.SetParent(dialogGo.transform, false);
            var badgeRt = badgeGo.AddComponent<RectTransform>();
            badgeRt.anchoredPosition = new Vector2(0f, 235f);
            badgeRt.sizeDelta = new Vector2(560f, 32f);
            _badgeText = badgeGo.AddComponent<Text>();
            _badgeText.font = font;
            _badgeText.fontSize = 18;
            _badgeText.fontStyle = FontStyle.Bold;
            _badgeText.alignment = TextAnchor.MiddleCenter;
            _badgeText.color = new Color(1.0f, 0.85f, 0.3f);

            // 2. Main Title Banner
            var titleGo = new GameObject("MainTitle");
            titleGo.transform.SetParent(dialogGo.transform, false);
            var titleRt = titleGo.AddComponent<RectTransform>();
            titleRt.anchoredPosition = new Vector2(0f, 195f);
            titleRt.sizeDelta = new Vector2(560f, 44f);
            var titleText = titleGo.AddComponent<Text>();
            titleText.font = font;
            titleText.text = "✨ 새로운 동료가 합류했습니다! ✨";
            titleText.fontSize = 26;
            titleText.fontStyle = FontStyle.Bold;
            titleText.alignment = TextAnchor.MiddleCenter;
            titleText.color = Color.white;

            // 3. Avatar Frame (Large 140x140 Sprite Box)
            var avatarFrameGo = new GameObject("AvatarFrame");
            avatarFrameGo.transform.SetParent(dialogGo.transform, false);
            var frameRt = avatarFrameGo.AddComponent<RectTransform>();
            frameRt.anchoredPosition = new Vector2(0f, 95f);
            frameRt.sizeDelta = new Vector2(144f, 144f);
            var frameImg = avatarFrameGo.AddComponent<Image>();
            frameImg.color = new Color(0.14f, 0.18f, 0.28f, 0.95f);
            var frameOutline = avatarFrameGo.AddComponent<Outline>();
            frameOutline.effectColor = new Color(1f, 1f, 1f, 0.35f);
            frameOutline.effectDistance = new Vector2(1.5f, -1.5f);

            var avatarGo = new GameObject("Avatar");
            avatarGo.transform.SetParent(avatarFrameGo.transform, false);
            var avatarRt = avatarGo.AddComponent<RectTransform>();
            avatarRt.sizeDelta = new Vector2(132f, 132f);
            _avatarImage = avatarGo.AddComponent<Image>();
            _avatarImage.preserveAspect = true;

            // 4. Companion Name Title
            var nameGo = new GameObject("CompanionName");
            nameGo.transform.SetParent(dialogGo.transform, false);
            var nameRt = nameGo.AddComponent<RectTransform>();
            nameRt.anchoredPosition = new Vector2(0f, -4f);
            nameRt.sizeDelta = new Vector2(560f, 38f);
            _companionTitleText = nameGo.AddComponent<Text>();
            _companionTitleText.font = font;
            _companionTitleText.fontSize = 24;
            _companionTitleText.fontStyle = FontStyle.Bold;
            _companionTitleText.alignment = TextAnchor.MiddleCenter;
            _companionTitleText.color = new Color(1.0f, 0.95f, 0.75f);

            // 5. Role & Skill Description Box
            var descBoxGo = new GameObject("DescBox");
            descBoxGo.transform.SetParent(dialogGo.transform, false);
            var descBoxRt = descBoxGo.AddComponent<RectTransform>();
            descBoxRt.anchoredPosition = new Vector2(0f, -65f);
            descBoxRt.sizeDelta = new Vector2(540f, 68f);
            _roleDescriptionText = descBoxGo.AddComponent<Text>();
            _roleDescriptionText.font = font;
            _roleDescriptionText.fontSize = 16;
            _roleDescriptionText.alignment = TextAnchor.MiddleCenter;
            _roleDescriptionText.color = new Color(0.85f, 0.90f, 0.98f);
            _roleDescriptionText.lineSpacing = 1.15f;

            // 6. Synergy Info Box
            var synGo = new GameObject("SynergyBox");
            synGo.transform.SetParent(dialogGo.transform, false);
            var synRt = synGo.AddComponent<RectTransform>();
            synRt.anchoredPosition = new Vector2(0f, -125f);
            synRt.sizeDelta = new Vector2(540f, 44f);
            _synergyNoticeText = synGo.AddComponent<Text>();
            _synergyNoticeText.font = font;
            _synergyNoticeText.fontSize = 14;
            _synergyNoticeText.alignment = TextAnchor.MiddleCenter;
            _synergyNoticeText.color = new Color(0.55f, 0.90f, 0.70f);

            // 7. Confirm / Join Expedition Button
            var btnGo = new GameObject("BtnJoinExpedition");
            btnGo.transform.SetParent(dialogGo.transform, false);
            var btnRt = btnGo.AddComponent<RectTransform>();
            btnRt.anchoredPosition = new Vector2(0f, -195f);
            btnRt.sizeDelta = new Vector2(440f, 54f);
            _confirmBtnImage = btnGo.AddComponent<Image>();
            _confirmBtnImage.color = new Color(0.85f, 0.55f, 0.15f, 1.0f);

            var btn = btnGo.AddComponent<Button>();
            btn.onClick.AddListener(OnConfirmClicked);

            var btnTextGo = new GameObject("Text");
            btnTextGo.transform.SetParent(btnGo.transform, false);
            var btnTextRt = btnTextGo.AddComponent<RectTransform>();
            btnTextRt.sizeDelta = btnRt.sizeDelta;
            var btnText = btnTextGo.AddComponent<Text>();
            btnText.font = font;
            btnText.text = "⚔️ 마법 원정대에 합류시키기";
            btnText.fontSize = 20;
            btnText.fontStyle = FontStyle.Bold;
            btnText.alignment = TextAnchor.MiddleCenter;
            btnText.color = Color.white;
        }

        private void OnConfirmClicked()
        {
            // Play confirm chime
            _eventBus?.Publish(new PlaySoundEvent(SoundEffectType.ChestOpen, 0.8f));

            if (_popupRoot != null)
            {
                _popupRoot.SetActive(false);
            }

            var cb = _onClosedCallback;
            _onClosedCallback = null;
            cb?.Invoke();
        }
    }
}
