using System;
using UnityEngine;
using UnityEngine.UI;
using HappyShoot.Domain.Progression;
using HappyShoot.View.Utils;

namespace HappyShoot.View.SkillTree
{
    /// <summary>
    /// Modal dialog for 2:1 gem exchange between Ruby, Emerald, and Amethyst.
    /// Strictly modular and under 500 lines.
    /// </summary>
    public class SkillTreeExchangePopupView : MonoBehaviour
    {
        private SkillTreeManager _manager;
        private Action _onExchangeDone;

        public void Initialize(SkillTreeManager manager, Transform parent, Action onExchangeDone)
        {
            _manager = manager;
            _onExchangeDone = onExchangeDone;

            BuildUi(parent);
            gameObject.SetActive(false);
        }

        private void BuildUi(Transform parent)
        {
            transform.SetParent(parent, false);
            var rt = gameObject.AddComponent<RectTransform>();
            rt.anchorMin = new Vector2(0.5f, 0.5f);
            rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.anchoredPosition = Vector2.zero;
            rt.sizeDelta = new Vector2(500, 440);

            var bg = gameObject.AddComponent<Image>();
            bg.sprite = SpriteHelper.GetOrCreateWhiteSprite();
            bg.color = new Color(0.08f, 0.10f, 0.16f, 1.0f); // 100% Solid

            CreateText("💎 보석 2:1 교환소", new Vector2(0, 180), 22, Color.white);
            CreateText("보석 2개를 소모하여 원하는 다른 보석 1개로 교환합니다.", new Vector2(0, 140), 14, Color.gray);

            float y = 80f;
            CreateExchangeOption(GemType.Ruby, GemType.Emerald, ref y);
            CreateExchangeOption(GemType.Ruby, GemType.Amethyst, ref y);
            CreateExchangeOption(GemType.Emerald, GemType.Ruby, ref y);
            CreateExchangeOption(GemType.Emerald, GemType.Amethyst, ref y);
            CreateExchangeOption(GemType.Amethyst, GemType.Ruby, ref y);
            CreateExchangeOption(GemType.Amethyst, GemType.Emerald, ref y);

            CreateButton("닫기 (ESC)", new Vector2(0, -185), new Vector2(140, 38), () => gameObject.SetActive(false));
        }

        private void CreateExchangeOption(GemType from, GemType to, ref float y)
        {
            string label = $"{from.GetDisplayName()} 2개  ➔  {to.GetDisplayName()} 1개";
            CreateButton(label, new Vector2(0, y), new Vector2(380, 34), () =>
            {
                if (_manager != null && _manager.TryExchangeGems(from, to))
                {
                    _onExchangeDone?.Invoke();
                }
            });
            y -= 42f;
        }

        private Text CreateText(string text, Vector2 pos, int fontSize, Color color)
        {
            var go = new GameObject("Text");
            go.transform.SetParent(transform, false);
            var rt = go.AddComponent<RectTransform>();
            rt.anchoredPosition = pos;
            rt.sizeDelta = new Vector2(460, 36);
            var txt = go.AddComponent<Text>();
            txt.font = FontHelper.GetKoreanFont();
            txt.text = text;
            txt.fontSize = fontSize;
            txt.color = color;
            txt.alignment = TextAnchor.MiddleCenter;
            txt.raycastTarget = false;
            return txt;
        }

        private Button CreateButton(string text, Vector2 pos, Vector2 size, Action onClick)
        {
            var go = new GameObject("Btn");
            go.transform.SetParent(transform, false);
            var rt = go.AddComponent<RectTransform>();
            rt.anchoredPosition = pos;
            rt.sizeDelta = size;
            var img = go.AddComponent<Image>();
            img.sprite = SpriteHelper.GetOrCreateWhiteSprite();
            img.color = new Color(0.20f, 0.26f, 0.38f, 1.0f);
            var btn = go.AddComponent<Button>();
            btn.targetGraphic = img;
            btn.onClick.AddListener(() => onClick?.Invoke());

            var txt = CreateText(text, Vector2.zero, 15, Color.white);
            txt.transform.SetParent(go.transform, false);
            txt.rectTransform.sizeDelta = size;
            return btn;
        }
    }
}
