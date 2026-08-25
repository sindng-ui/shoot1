using System;
using UnityEngine;
using UnityEngine.UI;
using HappyShoot.View.Utils;

namespace HappyShoot.View.UI
{
    /// <summary>
    /// Professional Developer UI Factory for Tuning Sliders and Input Controls.
    /// Clean, aligned layout with centered controls and wide buttons.
    /// Strictly modular and under 500 lines.
    /// </summary>
    public static class SkillTuningSliderFactory
    {
        public static GameObject CreateSliderRow(
            Transform parent,
            string title,
            float curVal,
            float min,
            float max,
            float step,
            Action<float> onChanged,
            ref float yOffset,
            bool isInt = false)
        {
            var rowGo = new GameObject($"Row_{title}");
            rowGo.transform.SetParent(parent, false);
            var rowRt = rowGo.AddComponent<RectTransform>();
            rowRt.anchorMin = new Vector2(0f, 1f);
            rowRt.anchorMax = new Vector2(1f, 1f);
            rowRt.pivot = new Vector2(0.5f, 1f);
            rowRt.anchoredPosition = new Vector2(0f, yOffset);
            rowRt.sizeDelta = new Vector2(0f, 34f);

            var bgCard = rowGo.AddComponent<Image>();
            bgCard.sprite = SpriteHelper.GetOrCreateWhiteSprite();
            bgCard.color = new Color(0.08f, 0.12f, 0.18f, 0.92f);

            var outline = rowGo.AddComponent<Outline>();
            outline.effectColor = new Color(0.18f, 0.28f, 0.42f, 0.6f);
            outline.effectDistance = new Vector2(1f, -1f);

            // 1. Title (Left)
            CreateText(rowGo.transform, "Title", title, 11, TextAnchor.MiddleLeft,
                new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(0f, 0.5f),
                new Vector2(8f, 0f), new Vector2(210f, 30f), new Color(0.92f, 0.96f, 1f));

            // 2. Minus Button
            float btnW = 24f;
            float btnH = 22f;
            var minusBtnGo = CreateButton(rowGo.transform, "BtnMinus", "-",
                new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(0f, 0.5f),
                new Vector2(224f, 0f), new Vector2(btnW, btnH), new Color(0.20f, 0.28f, 0.42f, 1f));

            // 3. Slider
            var sliderGo = new GameObject("Slider");
            sliderGo.transform.SetParent(rowGo.transform, false);
            var sliderRt = sliderGo.AddComponent<RectTransform>();
            sliderRt.anchorMin = new Vector2(0f, 0.5f);
            sliderRt.anchorMax = new Vector2(0f, 0.5f);
            sliderRt.pivot = new Vector2(0f, 0.5f);
            sliderRt.anchoredPosition = new Vector2(254f, 0f);
            sliderRt.sizeDelta = new Vector2(120f, 14f);

            var bgGo = new GameObject("Background");
            bgGo.transform.SetParent(sliderGo.transform, false);
            var bgRt = bgGo.AddComponent<RectTransform>();
            bgRt.anchorMin = new Vector2(0f, 0.25f);
            bgRt.anchorMax = new Vector2(1f, 0.75f);
            bgRt.offsetMin = Vector2.zero;
            bgRt.offsetMax = Vector2.zero;
            var bgImg = bgGo.AddComponent<Image>();
            bgImg.sprite = SpriteHelper.GetOrCreateWhiteSprite();
            bgImg.color = new Color(0.04f, 0.06f, 0.10f, 0.98f);

            var fillAreaGo = new GameObject("Fill Area");
            fillAreaGo.transform.SetParent(sliderGo.transform, false);
            var fillAreaRt = fillAreaGo.AddComponent<RectTransform>();
            fillAreaRt.anchorMin = new Vector2(0f, 0f);
            fillAreaRt.anchorMax = new Vector2(1f, 1f);
            fillAreaRt.offsetMin = new Vector2(3f, 0f);
            fillAreaRt.offsetMax = new Vector2(-3f, 0f);

            var fillGo = new GameObject("Fill");
            fillGo.transform.SetParent(fillAreaGo.transform, false);
            var fillRt = fillGo.AddComponent<RectTransform>();
            fillRt.anchorMin = new Vector2(0f, 0.25f);
            fillRt.anchorMax = new Vector2(0f, 0.75f);
            fillRt.offsetMin = Vector2.zero;
            fillRt.offsetMax = Vector2.zero;
            var fillImg = fillGo.AddComponent<Image>();
            fillImg.sprite = SpriteHelper.GetOrCreateWhiteSprite();
            fillImg.color = new Color(0.15f, 0.75f, 0.95f, 1f);

            var handleAreaGo = new GameObject("Handle Slide Area");
            handleAreaGo.transform.SetParent(sliderGo.transform, false);
            var handleAreaRt = handleAreaGo.AddComponent<RectTransform>();
            handleAreaRt.anchorMin = new Vector2(0f, 0f);
            handleAreaRt.anchorMax = new Vector2(1f, 1f);
            handleAreaRt.offsetMin = new Vector2(6f, 0f);
            handleAreaRt.offsetMax = new Vector2(-6f, 0f);

            var handleGo = new GameObject("Handle");
            handleGo.transform.SetParent(handleAreaGo.transform, false);
            var handleRt = handleGo.AddComponent<RectTransform>();
            handleRt.anchorMin = new Vector2(0f, 0.5f);
            handleRt.anchorMax = new Vector2(0f, 0.5f);
            handleRt.sizeDelta = new Vector2(10f, 16f);
            var handleImg = handleGo.AddComponent<Image>();
            handleImg.sprite = SpriteHelper.GetOrCreateWhiteSprite();
            handleImg.color = Color.white;

            var slider = sliderGo.AddComponent<Slider>();
            slider.fillRect = fillRt;
            slider.handleRect = handleRt;
            slider.targetGraphic = handleImg;
            slider.direction = Slider.Direction.LeftToRight;
            slider.minValue = min;
            slider.maxValue = max;
            slider.wholeNumbers = isInt;
            slider.value = curVal;

            // 4. Plus Button
            var plusBtnGo = CreateButton(rowGo.transform, "BtnPlus", "+",
                new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(0f, 0.5f),
                new Vector2(380f, 0f), new Vector2(btnW, btnH), new Color(0.20f, 0.28f, 0.42f, 1f));

            // 5. Interactive Direct Number InputField (Right)
            var inputGo = new GameObject("InputField");
            inputGo.transform.SetParent(rowGo.transform, false);
            var inputRt = inputGo.AddComponent<RectTransform>();
            inputRt.anchorMin = new Vector2(1f, 0.5f);
            inputRt.anchorMax = new Vector2(1f, 0.5f);
            inputRt.pivot = new Vector2(1f, 0.5f);
            inputRt.anchoredPosition = new Vector2(-6f, 0f);
            inputRt.sizeDelta = new Vector2(56f, 22f);

            var inputBg = inputGo.AddComponent<Image>();
            inputBg.sprite = SpriteHelper.GetOrCreateWhiteSprite();
            inputBg.color = new Color(0.04f, 0.08f, 0.14f, 0.95f);

            var inputOutline = inputGo.AddComponent<Outline>();
            inputOutline.effectColor = new Color(0.25f, 0.60f, 0.90f, 0.85f);
            inputOutline.effectDistance = new Vector2(1f, -1f);

            var textComponent = CreateText(inputGo.transform, "Text", isInt ? $"{Mathf.RoundToInt(curVal)}" : $"{curVal:F2}", 11, TextAnchor.MiddleCenter,
                Vector2.zero, Vector2.one, new Vector2(0.5f, 0.5f), Vector2.zero, Vector2.zero, new Color(0.35f, 1f, 0.65f));

            var inputField = inputGo.AddComponent<InputField>();
            inputField.textComponent = textComponent;
            inputField.contentType = isInt ? InputField.ContentType.IntegerNumber : InputField.ContentType.DecimalNumber;
            inputField.lineType = InputField.LineType.SingleLine;
            inputField.caretColor = Color.cyan;
            inputField.selectionColor = new Color(0.18f, 0.55f, 0.85f, 0.5f);
            inputField.text = isInt ? $"{Mathf.RoundToInt(curVal)}" : $"{curVal:F2}";

            // Sync Slider -> InputField
            slider.onValueChanged.AddListener(v =>
            {
                if (!inputField.isFocused)
                {
                    inputField.text = isInt ? $"{Mathf.RoundToInt(v)}" : $"{v:F2}";
                }
                onChanged?.Invoke(v);
            });

            // Sync InputField -> Slider & Logic
            inputField.onEndEdit.AddListener(str =>
            {
                if (float.TryParse(str, System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out float parsed))
                {
                    float clamped = Mathf.Clamp(parsed, min, max);
                    slider.value = clamped;
                    inputField.text = isInt ? $"{Mathf.RoundToInt(clamped)}" : $"{clamped:F2}";
                    onChanged?.Invoke(clamped);
                }
                else
                {
                    inputField.text = isInt ? $"{Mathf.RoundToInt(slider.value)}" : $"{slider.value:F2}";
                }
            });

            minusBtnGo.GetComponent<Button>().onClick.AddListener(() =>
            {
                slider.value = Mathf.Max(min, slider.value - step);
                inputField.text = isInt ? $"{Mathf.RoundToInt(slider.value)}" : $"{slider.value:F2}";
            });

            plusBtnGo.GetComponent<Button>().onClick.AddListener(() =>
            {
                slider.value = Mathf.Min(max, slider.value + step);
                inputField.text = isInt ? $"{Mathf.RoundToInt(slider.value)}" : $"{slider.value:F2}";
            });

            yOffset -= 38f;
            if (parent is RectTransform pRt)
            {
                pRt.sizeDelta = new Vector2(pRt.sizeDelta.x, Mathf.Max(pRt.sizeDelta.y, Mathf.Abs(yOffset) + 10f));
            }
            return rowGo;
        }

        public static GameObject CreateButton(Transform parent, string name, string label, Vector2 anchorMin, Vector2 anchorMax, Vector2 pivot, Vector2 anchoredPos, Vector2 size, Color color)
        {
            var btnGo = new GameObject(name);
            btnGo.transform.SetParent(parent, false);
            var rt = btnGo.AddComponent<RectTransform>();
            rt.anchorMin = anchorMin;
            rt.anchorMax = anchorMax;
            rt.pivot = pivot;
            rt.anchoredPosition = anchoredPos;
            rt.sizeDelta = size;

            var img = btnGo.AddComponent<Image>();
            img.sprite = SpriteHelper.GetOrCreateWhiteSprite();
            img.color = color;

            var btn = btnGo.AddComponent<Button>();
            btn.targetGraphic = img;

            CreateText(btnGo.transform, "Txt", label, 16, TextAnchor.MiddleCenter, Vector2.zero, Vector2.one, new Vector2(0.5f, 0.5f), Vector2.zero, Vector2.zero, Color.white);
            return btnGo;
        }

        public static GameObject CreateWideButton(Transform parent, string name, string label, Vector2 pos, Color color, Action onClick)
        {
            var btnGo = new GameObject(name);
            btnGo.transform.SetParent(parent, false);
            var rt = btnGo.AddComponent<RectTransform>();
            rt.anchorMin = new Vector2(0.5f, 0f);
            rt.anchorMax = new Vector2(0.5f, 0f);
            rt.pivot = new Vector2(0.5f, 0f);
            rt.anchoredPosition = pos;
            rt.sizeDelta = new Vector2(460f, 34f);

            var img = btnGo.AddComponent<Image>();
            img.sprite = SpriteHelper.GetOrCreateWhiteSprite();
            img.color = color;

            var btn = btnGo.AddComponent<Button>();
            btn.targetGraphic = img;
            btn.onClick.AddListener(() => onClick?.Invoke());

            CreateText(btnGo.transform, "Label", label, 13, TextAnchor.MiddleCenter, Vector2.zero, Vector2.one, new Vector2(0.5f, 0.5f), Vector2.zero, Vector2.zero, Color.white);
            return btnGo;
        }

        public static Text CreateText(Transform parent, string name, string text, int fontSize, TextAnchor alignment, Vector2 anchorMin, Vector2 anchorMax, Vector2 pivot, Vector2 pos, Vector2 size, Color color)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            var rt = go.AddComponent<RectTransform>();
            rt.anchorMin = anchorMin;
            rt.anchorMax = anchorMax;
            rt.pivot = pivot;
            rt.anchoredPosition = pos;
            rt.sizeDelta = size;

            var txt = go.AddComponent<Text>();
            txt.text = text;
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
