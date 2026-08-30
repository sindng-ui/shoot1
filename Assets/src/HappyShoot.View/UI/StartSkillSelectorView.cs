using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using HappyShoot.View.Utils;

namespace HappyShoot.View.UI
{
    /// <summary>
    /// UI Component dedicated to selecting the Wizard's starting magic skill.
    /// Supports Fireball, Frost Nova, and Chain Lightning with real-time descriptions.
    /// Strictly modular and under 250 lines.
    /// </summary>
    public class StartSkillSelectorView : MonoBehaviour
    {
        private const string PrefsKey = "WizardSelectedStartSkill";

        public struct SkillOption
        {
            public string Id;
            public string Name;
            public string IconEmoji;
            public string ShortDesc;
            public string StatSummary;

            public SkillOption(string id, string name, string iconEmoji, string shortDesc, string statSummary)
            {
                Id = id;
                Name = name;
                IconEmoji = iconEmoji;
                ShortDesc = shortDesc;
                StatSummary = statSummary;
            }
        }

        private static readonly SkillOption[] AvailableSkills = new[]
        {
            new SkillOption("fireball", "화염구", "🔥",
                "목표 지점에 고속 화염 혜성을 발사하여 강력한 폭발 피해",
                "사거리 9.0 | 쿨다운 1.2s | 위력 35 (광역 폭발)"),
            new SkillOption("frost_nova", "서리 폭발", "❄️",
                "플레이어 주변 360°로 빙결 파동을 방출하여 적들을 얼리고 감속",
                "범위 4.0 | 쿨다운 2.5s | 위력 25 (전방위 동결)"),
            new SkillOption("chain_lightning", "연쇄 번개", "⚡",
                "적에게 번갯불을 쏘아 주변 최대 4마리까지 순차 전이 감전",
                "사거리 8.0 | 쿨다운 1.5s | 위력 28 (4체 연쇄)")
        };

        private string _selectedSkillId = "fireball";
        public string SelectedSkillId => _selectedSkillId;

        public event Action<string> OnStartSkillChanged;

        private readonly List<Image> _buttonBgImages = new List<Image>(3);
        private readonly List<Outline> _buttonOutlines = new List<Outline>(3);
        private Text _descTitleText;
        private Text _descBodyText;
        private Text _descStatText;

        public void Initialize(Transform parent, Vector2 anchoredPosition)
        {
            _selectedSkillId = PlayerPrefs.GetString(PrefsKey, "fireball");
            EnsureValidSelection();

            var containerGo = new GameObject("StartSkillSelector");
            containerGo.transform.SetParent(parent, false);
            var rt = containerGo.AddComponent<RectTransform>();
            rt.anchorMin = new Vector2(0.5f, 1f);
            rt.anchorMax = new Vector2(0.5f, 1f);
            rt.pivot = new Vector2(0.5f, 1f);
            rt.anchoredPosition = anchoredPosition;
            rt.sizeDelta = new Vector2(380f, 175f);

            // Title: 시작 기본 마법 선택
            var titleTxt = CreateText(containerGo.transform, "Title", "🔮 시작 기본 마법 선택", 15,
                TextAnchor.MiddleCenter, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f),
                new Vector2(0f, 0f), new Vector2(360f, 22f), new Color(1f, 0.85f, 0.4f, 1f));

            // Buttons Row
            var buttonRowGo = new GameObject("ButtonRow");
            buttonRowGo.transform.SetParent(containerGo.transform, false);
            var rowRt = buttonRowGo.AddComponent<RectTransform>();
            rowRt.anchorMin = new Vector2(0.5f, 1f);
            rowRt.anchorMax = new Vector2(0.5f, 1f);
            rowRt.pivot = new Vector2(0.5f, 1f);
            rowRt.anchoredPosition = new Vector2(0f, -26f);
            rowRt.sizeDelta = new Vector2(360f, 54f);

            float btnWidth = 112f;
            float btnGap = 8f;
            float startX = -(btnWidth + btnGap);

            for (int i = 0; i < AvailableSkills.Length; i++)
            {
                int index = i;
                var opt = AvailableSkills[i];
                float posX = startX + (i * (btnWidth + btnGap));

                var btnGo = new GameObject($"BtnSkill_{opt.Id}");
                btnGo.transform.SetParent(buttonRowGo.transform, false);
                var btnRt = btnGo.AddComponent<RectTransform>();
                btnRt.anchorMin = new Vector2(0.5f, 0.5f);
                btnRt.anchorMax = new Vector2(0.5f, 0.5f);
                btnRt.pivot = new Vector2(0.5f, 0.5f);
                btnRt.anchoredPosition = new Vector2(posX, 0f);
                btnRt.sizeDelta = new Vector2(btnWidth, 50f);

                var bgImg = btnGo.AddComponent<Image>();
                bgImg.sprite = SpriteHelper.GetOrCreateWhiteSprite();
                var outline = btnGo.AddComponent<Outline>();
                outline.effectDistance = new Vector2(2f, -2f);

                _buttonBgImages.Add(bgImg);
                _buttonOutlines.Add(outline);

                var btn = btnGo.AddComponent<Button>();
                btn.targetGraphic = bgImg;
                btn.onClick.AddListener(() => SelectSkill(opt.Id));

                // Icon (Left)
                var iconGo = new GameObject("Icon");
                iconGo.transform.SetParent(btnGo.transform, false);
                var iconRt = iconGo.AddComponent<RectTransform>();
                iconRt.anchorMin = new Vector2(0f, 0.5f);
                iconRt.anchorMax = new Vector2(0f, 0.5f);
                iconRt.pivot = new Vector2(0f, 0.5f);
                iconRt.anchoredPosition = new Vector2(6f, 0f);
                iconRt.sizeDelta = new Vector2(38f, 38f);
                var iconImg = iconGo.AddComponent<Image>();
                iconImg.sprite = RewardIconHelper.GetOrCreateRewardIcon(opt.Id, 80);
                iconImg.preserveAspect = true;
                iconImg.raycastTarget = false;

                // Name label (Right)
                CreateText(btnGo.transform, "Label", $"{opt.IconEmoji} {opt.Name}", 13,
                    TextAnchor.MiddleLeft, new Vector2(0f, 0f), new Vector2(1f, 1f), new Vector2(0.5f, 0.5f),
                    new Vector2(25f, 0f), new Vector2(-48f, 0f), Color.white);
            }

            // Description Box Container
            var descBoxGo = new GameObject("DescBox");
            descBoxGo.transform.SetParent(containerGo.transform, false);
            var descRt = descBoxGo.AddComponent<RectTransform>();
            descRt.anchorMin = new Vector2(0.5f, 1f);
            descRt.anchorMax = new Vector2(0.5f, 1f);
            descRt.pivot = new Vector2(0.5f, 1f);
            descRt.anchoredPosition = new Vector2(0f, -86f);
            descRt.sizeDelta = new Vector2(360f, 82f);

            var descBg = descBoxGo.AddComponent<Image>();
            descBg.sprite = SpriteHelper.GetOrCreateWhiteSprite();
            descBg.color = new Color(0.04f, 0.03f, 0.08f, 0.85f);

            _descTitleText = CreateText(descBoxGo.transform, "DescTitle", "", 14,
                TextAnchor.MiddleLeft, new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(0f, 1f),
                new Vector2(10f, -8f), new Vector2(-20f, 20f), new Color(1f, 0.9f, 0.4f, 1f));

            _descBodyText = CreateText(descBoxGo.transform, "DescBody", "", 12,
                TextAnchor.UpperLeft, new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(0f, 1f),
                new Vector2(10f, -30f), new Vector2(-20f, 30f), new Color(0.9f, 0.93f, 0.98f, 0.95f));

            _descStatText = CreateText(descBoxGo.transform, "DescStat", "", 11,
                TextAnchor.LowerLeft, new Vector2(0f, 0f), new Vector2(1f, 0f), new Vector2(0f, 0f),
                new Vector2(10f, 6f), new Vector2(-20f, 18f), new Color(0.6f, 0.85f, 1f, 0.9f));

            RefreshVisuals();
        }

        public void SelectSkill(string skillId)
        {
            if (_selectedSkillId == skillId) return;

            _selectedSkillId = skillId;
            PlayerPrefs.SetString(PrefsKey, _selectedSkillId);
            PlayerPrefs.Save();

            RefreshVisuals();
            OnStartSkillChanged?.Invoke(_selectedSkillId);
        }

        private void EnsureValidSelection()
        {
            bool valid = false;
            for (int i = 0; i < AvailableSkills.Length; i++)
            {
                if (AvailableSkills[i].Id == _selectedSkillId)
                {
                    valid = true;
                    break;
                }
            }
            if (!valid) _selectedSkillId = "fireball";
        }

        private void RefreshVisuals()
        {
            for (int i = 0; i < AvailableSkills.Length; i++)
            {
                var opt = AvailableSkills[i];
                bool isSelected = opt.Id == _selectedSkillId;

                if (i < _buttonBgImages.Count)
                {
                    _buttonBgImages[i].color = isSelected
                        ? new Color(0.40f, 0.20f, 0.65f, 0.95f) // Glowing Purple
                        : new Color(0.12f, 0.12f, 0.18f, 0.70f); // Dim Slate
                }

                if (i < _buttonOutlines.Count)
                {
                    _buttonOutlines[i].enabled = isSelected;
                    _buttonOutlines[i].effectColor = isSelected
                        ? new Color(1.0f, 0.85f, 0.35f, 1.0f) // Golden accent rim
                        : Color.clear;
                }

                if (isSelected)
                {
                    if (_descTitleText != null) _descTitleText.text = $"{opt.IconEmoji} {opt.Name}";
                    if (_descBodyText != null) _descBodyText.text = opt.ShortDesc;
                    if (_descStatText != null) _descStatText.text = opt.StatSummary;
                }
            }
        }

        private Text CreateText(Transform parent, string name, string text, int fontSize,
            TextAnchor alignment, Vector2 anchorMin, Vector2 anchorMax, Vector2 pivot,
            Vector2 anchoredPos, Vector2 sizeDelta, Color color)
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
