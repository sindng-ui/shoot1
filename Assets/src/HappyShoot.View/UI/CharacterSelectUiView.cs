using System;
using UnityEngine;
using UnityEngine.UI;
using HappyShoot.Domain.Entities;
using HappyShoot.View.Utils;

namespace HappyShoot.View.UI
{
    /// <summary>
    /// Master Character Selection Screen displayed before game launch.
    /// Allows players to select between Warrior (Melee) and Ranger (Bow) heroes.
    /// </summary>
    public class CharacterSelectUiView : MonoBehaviour
    {
        private GameObject _panelRoot;
        private Action<CharacterClassType> _onSelectedCallback;
        private SettingsDialogUiView _settingsDialog;

        public void SetSettingsDialog(SettingsDialogUiView dialog)
        {
            _settingsDialog = dialog;
        }

        public void Initialize(Action<CharacterClassType> onSelectedCallback)
        {
            _onSelectedCallback = onSelectedCallback;
            EnsureUiElements();
            ShowSelectScreen();
        }

        public void ShowSelectScreen()
        {
            if (_panelRoot != null)
            {
                _panelRoot.SetActive(true);
                Time.timeScale = 0f;
            }
        }

        public void HideSelectScreen()
        {
            if (_panelRoot != null)
            {
                _panelRoot.SetActive(false);
                Time.timeScale = 1f;
            }
        }

        private void SelectClass(CharacterClassType classType)
        {
            PlayerPrefs.SetInt("SelectedHeroClass", (int)classType);
            PlayerPrefs.Save();
            HideSelectScreen();
            _onSelectedCallback?.Invoke(classType);
        }

        private void EnsureUiElements()
        {
            if (_panelRoot != null) return;

            // Ensure EventSystem with New Input System Module
            if (FindFirstObjectByType<UnityEngine.EventSystems.EventSystem>() == null)
            {
                var eventSystemGo = new GameObject("EventSystem");
                eventSystemGo.AddComponent<UnityEngine.EventSystems.EventSystem>();
                eventSystemGo.AddComponent<UnityEngine.InputSystem.UI.InputSystemUIInputModule>();
            }

            var canvasGo = new GameObject("CharacterSelectCanvas");
            var canvas = canvasGo.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 85;

            var scaler = canvasGo.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            scaler.matchWidthOrHeight = 0.5f;
            canvasGo.AddComponent<GraphicRaycaster>();

            // Dark Slate Backdrop
            _panelRoot = new GameObject("CharacterSelectPanel");
            _panelRoot.transform.SetParent(canvasGo.transform, false);
            var panelRt = _panelRoot.AddComponent<RectTransform>();
            panelRt.anchorMin = Vector2.zero;
            panelRt.anchorMax = Vector2.one;
            panelRt.sizeDelta = Vector2.zero;
            _panelRoot.AddComponent<Image>().color = new Color(0.06f, 0.08f, 0.12f, 0.95f);

            // Title Banner
            CreateText(_panelRoot.transform, "Title", "⚔️ 출격할 영웅을 선택하세요 ⚔️", 36, TextAnchor.MiddleCenter, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -80f), new Vector2(800f, 60f), new Color(1f, 0.88f, 0.35f, 1f));
            CreateText(_panelRoot.transform, "Subtitle", "각 영웅은 고유한 능력치와 전용 무기를 보유하고 있습니다", 18, TextAnchor.MiddleCenter, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -135f), new Vector2(800f, 30f), new Color(0.75f, 0.82f, 0.90f, 0.8f));

            // Card Container
            var cardsContainer = new GameObject("CardsContainer");
            cardsContainer.transform.SetParent(_panelRoot.transform, false);
            var containerRt = cardsContainer.AddComponent<RectTransform>();
            containerRt.anchorMin = new Vector2(0.5f, 0.5f);
            containerRt.anchorMax = new Vector2(0.5f, 0.5f);
            containerRt.pivot = new Vector2(0.5f, 0.5f);
            containerRt.anchoredPosition = new Vector2(0f, -20f);
            containerRt.sizeDelta = new Vector2(800f, 520f);

            // Card 1: Warrior
            CreateCharacterCard(
                parent: cardsContainer.transform,
                name: "WarriorCard",
                title: "🛡️ 전사 (Warrior)",
                heroSprite: SpriteHelper.GetOrCreateWarriorSprite(),
                statsDesc: "❤️ 최대 체력: 125 (+25%)\n🛡️ 기본 방어력: 15\n⚔️ 공격력: +10%",
                weaponDesc: "🗡️ 시작 스킬: [대검 베기]\n전방 150도 광역 근접 물리 베기",
                accentColor: new Color(0.95f, 0.35f, 0.35f, 1f),
                btnColor: new Color(0.85f, 0.25f, 0.25f, 1f),
                anchoredPos: new Vector2(-200f, 0f),
                classType: CharacterClassType.Warrior
            );

            // Card 2: Ranger
            CreateCharacterCard(
                parent: cardsContainer.transform,
                name: "RangerCard",
                title: "🏹 궁수 (Ranger)",
                heroSprite: SpriteHelper.GetOrCreateRangerSprite(),
                statsDesc: "👟 이동 속도: 6.0 (+20%)\n🎯 치명타율: 15% (치명타 특화)\n🏹 투사체 속도: +30%",
                weaponDesc: "🏹 시작 스킬: [관통 화살]\n적들을 관통하는 고속 원거리 사격",
                accentColor: new Color(0.25f, 0.85f, 0.45f, 1f),
                btnColor: new Color(0.18f, 0.70f, 0.35f, 1f),
                anchoredPos: new Vector2(200f, 0f),
                classType: CharacterClassType.Ranger
            );

            // Bottom Settings Button (Clear text button for easy user access)
            var settingsBtnGo = new GameObject("BtnOpenSettings");
            settingsBtnGo.transform.SetParent(_panelRoot.transform, false);
            var settingsRt = settingsBtnGo.AddComponent<RectTransform>();
            settingsRt.anchorMin = new Vector2(0.5f, 0f);
            settingsRt.anchorMax = new Vector2(0.5f, 0f);
            settingsRt.pivot = new Vector2(0.5f, 0f);
            settingsRt.anchoredPosition = new Vector2(0f, 35f);
            settingsRt.sizeDelta = new Vector2(300f, 52f);

            var settingsImg = settingsBtnGo.AddComponent<Image>();
            settingsImg.sprite = SpriteHelper.GetOrCreateWhiteSprite();
            settingsImg.color = new Color(0.20f, 0.26f, 0.38f, 0.95f);

            var settingsBtn = settingsBtnGo.AddComponent<Button>();
            settingsBtn.targetGraphic = settingsImg;
            settingsBtn.onClick.AddListener(() => _settingsDialog?.Show());

            CreateText(settingsBtnGo.transform, "BtnText", "⚙️ 게임 환경 설정 (SETTINGS)", 18, TextAnchor.MiddleCenter, Vector2.zero, Vector2.one, new Vector2(0.5f, 0.5f), Vector2.zero, Vector2.zero, Color.white);

            // Version Label (Bottom Right)
            CreateText(_panelRoot.transform, "VersionLabel", HappyShoot.Domain.Common.AppVersion.FullVersionText, 14, TextAnchor.LowerRight, new Vector2(1f, 0f), new Vector2(1f, 0f), new Vector2(1f, 0f), new Vector2(-24f, 20f), new Vector2(300f, 24f), new Color(0.6f, 0.7f, 0.8f, 0.6f));
        }

        private void CreateCharacterCard(
            Transform parent,
            string name,
            string title,
            Sprite heroSprite,
            string statsDesc,
            string weaponDesc,
            Color accentColor,
            Color btnColor,
            Vector2 anchoredPos,
            CharacterClassType classType)
        {
            var cardGo = new GameObject(name);
            cardGo.transform.SetParent(parent, false);
            var cardRt = cardGo.AddComponent<RectTransform>();
            cardRt.anchorMin = new Vector2(0.5f, 0.5f);
            cardRt.anchorMax = new Vector2(0.5f, 0.5f);
            cardRt.pivot = new Vector2(0.5f, 0.5f);
            cardRt.anchoredPosition = anchoredPos;
            cardRt.sizeDelta = new Vector2(360f, 500f);

            // Card Background & Accent Border
            cardGo.AddComponent<Image>().color = new Color(0.12f, 0.15f, 0.20f, 0.98f);

            var borderGo = new GameObject("Border");
            borderGo.transform.SetParent(cardGo.transform, false);
            var borderRt = borderGo.AddComponent<RectTransform>();
            borderRt.anchorMin = Vector2.zero;
            borderRt.anchorMax = Vector2.one;
            borderRt.sizeDelta = Vector2.zero;
            var borderImg = borderGo.AddComponent<Image>();
            borderImg.sprite = SpriteHelper.GetOrCreateWhiteSprite();
            borderImg.color = accentColor * new Color(1f, 1f, 1f, 0.3f);

            // Hero Avatar Icon Box
            var avatarBoxGo = new GameObject("AvatarBox");
            avatarBoxGo.transform.SetParent(cardGo.transform, false);
            var avatarBoxRt = avatarBoxGo.AddComponent<RectTransform>();
            avatarBoxRt.anchorMin = new Vector2(0.5f, 1f);
            avatarBoxRt.anchorMax = new Vector2(0.5f, 1f);
            avatarBoxRt.pivot = new Vector2(0.5f, 1f);
            avatarBoxRt.anchoredPosition = new Vector2(0f, -24f);
            avatarBoxRt.sizeDelta = new Vector2(100f, 100f);
            avatarBoxGo.AddComponent<Image>().color = new Color(0.08f, 0.10f, 0.14f, 0.9f);

            var iconGo = new GameObject("HeroIcon");
            iconGo.transform.SetParent(avatarBoxGo.transform, false);
            var iconRt = iconGo.AddComponent<RectTransform>();
            iconRt.anchorMin = new Vector2(0.5f, 0.5f);
            iconRt.anchorMax = new Vector2(0.5f, 0.5f);
            iconRt.pivot = new Vector2(0.5f, 0.5f);
            iconRt.anchoredPosition = Vector2.zero;
            iconRt.sizeDelta = new Vector2(80f, 80f);
            var iconImg = iconGo.AddComponent<Image>();
            iconImg.sprite = heroSprite;

            // Class Title
            CreateText(cardGo.transform, "Title", title, 22, TextAnchor.MiddleCenter, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -140f), new Vector2(320f, 32f), accentColor);

            // Stats Description
            CreateText(cardGo.transform, "Stats", statsDesc, 15, TextAnchor.UpperLeft, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -185f), new Vector2(300f, 90f), new Color(0.92f, 0.95f, 0.98f, 1f));

            // Starting Skill Description
            CreateText(cardGo.transform, "Weapon", weaponDesc, 14, TextAnchor.UpperLeft, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -290f), new Vector2(300f, 80f), new Color(1f, 0.85f, 0.4f, 1f));

            // Select & Launch Button
            var btnGo = new GameObject("SelectBtn");
            btnGo.transform.SetParent(cardGo.transform, false);
            var btnRt = btnGo.AddComponent<RectTransform>();
            btnRt.anchorMin = new Vector2(0.5f, 0f);
            btnRt.anchorMax = new Vector2(0.5f, 0f);
            btnRt.pivot = new Vector2(0.5f, 0f);
            btnRt.anchoredPosition = new Vector2(0f, 24f);
            btnRt.sizeDelta = new Vector2(280f, 52f);

            var btnImg = btnGo.AddComponent<Image>();
            btnImg.sprite = SpriteHelper.GetOrCreateWhiteSprite();
            btnImg.color = btnColor;

            var btn = btnGo.AddComponent<Button>();
            btn.targetGraphic = btnImg;
            btn.onClick.AddListener(() => SelectClass(classType));

            CreateText(btnGo.transform, "BtnText", "⚔️ 이 영웅으로 출격", 18, TextAnchor.MiddleCenter, Vector2.zero, Vector2.one, new Vector2(0.5f, 0.5f), Vector2.zero, Vector2.zero, Color.white);
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
            txt.font = FontHelper.GetKoreanFont();
            return txt;
        }
    }
}
