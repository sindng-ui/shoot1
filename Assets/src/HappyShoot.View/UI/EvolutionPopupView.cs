using UnityEngine;
using UnityEngine.UI;
using HappyShoot.Domain.Events;

namespace HappyShoot.View.UI
{
    /// <summary>
    /// Unity UI View that pops up a glorious banner whenever a weapon is synthesized / evolved.
    /// Supports automatic procedural UI creation and auto-dismiss.
    /// </summary>
    public class EvolutionPopupView : MonoBehaviour
    {
        [SerializeField] private GameObject _popupRoot;
        [SerializeField] private Text _evolvedTitleText;
        [SerializeField] private Text _evolvedDescriptionText;

        public void Initialize(EventBus eventBus)
        {
            EnsureUiElements();
            BindEventBus(eventBus);
        }

        public void BindEventBus(EventBus eventBus)
        {
            eventBus?.Subscribe<SkillEvolvedEvent>(OnSkillEvolved);

            if (_popupRoot != null)
            {
                _popupRoot.SetActive(false);
            }
        }

        private void OnSkillEvolved(SkillEvolvedEvent evt)
        {
            if (_popupRoot != null)
            {
                _popupRoot.SetActive(true);
            }

            if (_evolvedTitleText != null)
            {
                _evolvedTitleText.text = $"⚡ WEAPON EVOLVED: {evt.EvolvedSkillName} ⚡";
            }

            if (_evolvedDescriptionText != null)
            {
                _evolvedDescriptionText.text = $"Synthesized from {evt.OldSkillId} into supreme form!";
            }

            CancelInvoke(nameof(ClosePopup));
            Invoke(nameof(ClosePopup), 3.0f);
        }

        private void ClosePopup()
        {
            if (_popupRoot != null)
            {
                _popupRoot.SetActive(false);
            }
        }

        private void EnsureUiElements()
        {
            if (_popupRoot != null) return;

            var canvasGo = new GameObject("EvolutionCanvas");
            var canvas = canvasGo.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 90;
            var scaler = canvasGo.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            scaler.matchWidthOrHeight = 0.5f;
            canvasGo.AddComponent<GraphicRaycaster>();

            _popupRoot = new GameObject("EvolutionBanner");
            _popupRoot.transform.SetParent(canvasGo.transform, false);
            var bannerRt = _popupRoot.AddComponent<RectTransform>();
            bannerRt.anchorMin = new Vector2(0.5f, 0.75f);
            bannerRt.anchorMax = new Vector2(0.5f, 0.75f);
            bannerRt.pivot = new Vector2(0.5f, 0.5f);
            bannerRt.anchoredPosition = Vector2.zero;
            bannerRt.sizeDelta = new Vector2(500f, 100f);

            var img = _popupRoot.AddComponent<Image>();
            img.sprite = Utils.SpriteHelper.GetOrCreateWhiteSprite();
            img.color = new Color(0.12f, 0.08f, 0.25f, 0.95f);

            _evolvedTitleText = CreateText(_popupRoot.transform, "Title", "⚡ 궁극 무기 진화 완료 ⚡", 20, TextAnchor.MiddleCenter, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -25f), new Vector2(480f, 30f), new Color(1f, 0.9f, 0.2f, 1f));
            _evolvedDescriptionText = CreateText(_popupRoot.transform, "Desc", "최고 등급의 궁극 무기로 각성했습니다!", 15, TextAnchor.MiddleCenter, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0f, 25f), new Vector2(480f, 25f), Color.white);
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
            txt.font = Utils.FontHelper.GetKoreanFont();
            return txt;
        }
    }
}
