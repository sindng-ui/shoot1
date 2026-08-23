using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using HappyShoot.View.Utils;

namespace HappyShoot.View.UI
{
    /// <summary>
    /// Builder helper that constructs UI elements for Combat & Balance Sandbox UI (SkillTuningUiView).
    /// Strictly modular and under 500 lines.
    /// </summary>
    public static class SkillTuningUiBuilder
    {
        public static (Image switchImg, Image addImg) CreateSelectionModeButtons(
            Transform contentBox,
            Action<bool> onModeChanged)
        {
            float modeY = -42f;

            var switchGo = new GameObject("Btn_SwitchMode");
            switchGo.transform.SetParent(contentBox, false);
            var sRt = switchGo.AddComponent<RectTransform>();
            sRt.anchorMin = new Vector2(0.5f, 1f);
            sRt.anchorMax = new Vector2(0.5f, 1f);
            sRt.pivot = new Vector2(0.5f, 1f);
            sRt.anchoredPosition = new Vector2(-115f, modeY);
            sRt.sizeDelta = new Vector2(220f, 26f);

            var switchImg = switchGo.AddComponent<Image>();
            switchImg.sprite = SpriteHelper.GetOrCreateWhiteSprite();
            switchImg.color = new Color(0.15f, 0.60f, 0.85f, 1f);

            var sBtn = switchGo.AddComponent<Button>();
            sBtn.targetGraphic = switchImg;
            sBtn.onClick.AddListener(() => onModeChanged?.Invoke(false));
            SkillTuningSliderFactory.CreateText(switchGo.transform, "Txt", "🔁 단독 교체 (Switch Mode)", 12, TextAnchor.MiddleCenter, Vector2.zero, Vector2.one, new Vector2(0.5f, 0.5f), Vector2.zero, Vector2.zero, Color.white);

            var addGo = new GameObject("Btn_AddMode");
            addGo.transform.SetParent(contentBox, false);
            var aRt = addGo.AddComponent<RectTransform>();
            aRt.anchorMin = new Vector2(0.5f, 1f);
            aRt.anchorMax = new Vector2(0.5f, 1f);
            aRt.pivot = new Vector2(0.5f, 1f);
            aRt.anchoredPosition = new Vector2(115f, modeY);
            aRt.sizeDelta = new Vector2(220f, 26f);

            var addImg = addGo.AddComponent<Image>();
            addImg.sprite = SpriteHelper.GetOrCreateWhiteSprite();
            addImg.color = new Color(0.18f, 0.24f, 0.32f, 0.95f);

            var aBtn = addGo.AddComponent<Button>();
            aBtn.targetGraphic = addImg;
            aBtn.onClick.AddListener(() => onModeChanged?.Invoke(true));
            SkillTuningSliderFactory.CreateText(addGo.transform, "Txt", "➕ 누적 추가 (Add Mode)", 12, TextAnchor.MiddleCenter, Vector2.zero, Vector2.one, new Vector2(0.5f, 0.5f), Vector2.zero, Vector2.zero, Color.white);

            return (switchImg, addImg);
        }

        public static void CreateSkillSelectButtons(
            Transform contentBox,
            Dictionary<string, Image> tabButtons,
            Action<string> onSelectSkill,
            Action<string> onUnequipSkill)
        {
            string[] skillIds = {
                "slash", "ground_stomp", "whirlwind", "bow", "glaive",
                "arrow_rain", "fireball", "frost_nova", "chain_lightning", "orbital",
                "blood_eater", "storm_bow", "meteor_strike", "crit_tuning", "exp_tuning",
                "monster_tuning"
            };
            string[] skillNames = {
                "대검", "강타", "휠윈드", "활", "글레이브",
                "화살비", "화염구", "서리폭발", "번개", "오비탈",
                "🩸블러드", "⚡폭풍활", "☄️메테오", "🎯치명/스탯", "💎경험치",
                "👾몬스터"
            };

            float startX = -188f;
            float startY = -74f;

            for (int i = 0; i < skillIds.Length; i++)
            {
                int row = i / 5;
                int col = i % 5;
                string id = skillIds[i];
                string label = skillNames[i];

                Vector2 pos = new Vector2(startX + col * 94f, startY - row * 27f);
                float btnWidth = 90f;

                var btnGo = new GameObject($"BtnTab_{id}");
                btnGo.transform.SetParent(contentBox, false);
                var rt = btnGo.AddComponent<RectTransform>();
                rt.anchorMin = new Vector2(0.5f, 1f);
                rt.anchorMax = new Vector2(0.5f, 1f);
                rt.pivot = new Vector2(0.5f, 1f);
                rt.anchoredPosition = pos;
                rt.sizeDelta = new Vector2(btnWidth, 24f);

                var img = btnGo.AddComponent<Image>();
                img.sprite = SpriteHelper.GetOrCreateWhiteSprite();
                img.color = new Color(0.14f, 0.18f, 0.26f, 0.95f);
                tabButtons[id] = img;

                var btn = btnGo.AddComponent<Button>();
                btn.targetGraphic = img;

                string capturedId = id;
                var trigger = btnGo.AddComponent<EventTrigger>();
                var clickEntry = new EventTrigger.Entry { eventID = EventTriggerType.PointerClick };
                clickEntry.callback.AddListener((data) =>
                {
                    var pointerData = (PointerEventData)data;
                    if (pointerData.button == PointerEventData.InputButton.Right)
                    {
                        onUnequipSkill?.Invoke(capturedId);
                    }
                    else
                    {
                        onSelectSkill?.Invoke(capturedId);
                    }
                });
                trigger.triggers.Add(clickEntry);

                SkillTuningSliderFactory.CreateText(btnGo.transform, "Txt", label, 12, TextAnchor.MiddleCenter, Vector2.zero, Vector2.one, new Vector2(0.5f, 0.5f), Vector2.zero, Vector2.zero, Color.white);
            }
        }

        public static (Image spamImg, Text spamTxt) CreateUtilityToolbar(
            Transform contentBox,
            Action onToggleSpam,
            Action onResetDummies,
            Action onAddBatDummies)
        {
            float toolY = -160f;

            var spamGo = new GameObject("Btn_InfiniteSpam");
            spamGo.transform.SetParent(contentBox, false);
            var spRt = spamGo.AddComponent<RectTransform>();
            spRt.anchorMin = new Vector2(0.5f, 1f);
            spRt.anchorMax = new Vector2(0.5f, 1f);
            spRt.pivot = new Vector2(0.5f, 1f);
            spRt.anchoredPosition = new Vector2(-155f, toolY);
            spRt.sizeDelta = new Vector2(140f, 26f);

            var spamImg = spamGo.AddComponent<Image>();
            spamImg.sprite = SpriteHelper.GetOrCreateWhiteSprite();
            spamImg.color = new Color(0.25f, 0.28f, 0.35f, 1f);

            var spBtn = spamGo.AddComponent<Button>();
            spBtn.targetGraphic = spamImg;
            spBtn.onClick.AddListener(() => onToggleSpam?.Invoke());
            var spamTxt = SkillTuningSliderFactory.CreateText(spamGo.transform, "Txt", "⚡ 난사 모드: OFF", 11, TextAnchor.MiddleCenter, Vector2.zero, Vector2.one, new Vector2(0.5f, 0.5f), Vector2.zero, Vector2.zero, Color.white);

            var dummyGo = new GameObject("Btn_ResetDummies");
            dummyGo.transform.SetParent(contentBox, false);
            var dRt = dummyGo.AddComponent<RectTransform>();
            dRt.anchorMin = new Vector2(0.5f, 1f);
            dRt.anchorMax = new Vector2(0.5f, 1f);
            dRt.pivot = new Vector2(0.5f, 1f);
            dRt.anchoredPosition = new Vector2(0f, toolY);
            dRt.sizeDelta = new Vector2(150f, 26f);

            var dImg = dummyGo.AddComponent<Image>();
            dImg.sprite = SpriteHelper.GetOrCreateWhiteSprite();
            dImg.color = new Color(0.18f, 0.45f, 0.32f, 1f);

            var dBtn = dummyGo.AddComponent<Button>();
            dBtn.targetGraphic = dImg;
            dBtn.onClick.AddListener(() => onResetDummies?.Invoke());
            SkillTuningSliderFactory.CreateText(dummyGo.transform, "Txt", "🎯 허수아비 5마리", 11, TextAnchor.MiddleCenter, Vector2.zero, Vector2.one, new Vector2(0.5f, 0.5f), Vector2.zero, Vector2.zero, Color.white);

            var batGo = new GameObject("Btn_BatDummies");
            batGo.transform.SetParent(contentBox, false);
            var bRt = batGo.AddComponent<RectTransform>();
            bRt.anchorMin = new Vector2(0.5f, 1f);
            bRt.anchorMax = new Vector2(0.5f, 1f);
            bRt.pivot = new Vector2(0.5f, 1f);
            bRt.anchoredPosition = new Vector2(155f, toolY);
            bRt.sizeDelta = new Vector2(140f, 26f);

            var bImg = batGo.AddComponent<Image>();
            bImg.sprite = SpriteHelper.GetOrCreateWhiteSprite();
            bImg.color = new Color(0.35f, 0.18f, 0.40f, 1f);

            var bBtn = batGo.AddComponent<Button>();
            bBtn.targetGraphic = bImg;
            bBtn.onClick.AddListener(() => onAddBatDummies?.Invoke());
            SkillTuningSliderFactory.CreateText(batGo.transform, "Txt", "🦇 박쥐 더미 +20", 11, TextAnchor.MiddleCenter, Vector2.zero, Vector2.one, new Vector2(0.5f, 0.5f), Vector2.zero, Vector2.zero, Color.white);

            return (spamImg, spamTxt);
        }

        public static List<Image> CreateLevelSelectors(
            Transform contentBox,
            Action<int> onSelectLevel)
        {
            var levelButtonImgs = new List<Image>();
            float lvY = -194f;
            float startX = -188f;

            for (int i = 0; i < 5; i++)
            {
                int lv = i + 1;
                var btnGo = new GameObject($"BtnLv_{lv}");
                btnGo.transform.SetParent(contentBox, false);
                var rt = btnGo.AddComponent<RectTransform>();
                rt.anchorMin = new Vector2(0.5f, 1f);
                rt.anchorMax = new Vector2(0.5f, 1f);
                rt.pivot = new Vector2(0.5f, 1f);
                rt.anchoredPosition = new Vector2(startX + i * 94f, lvY);
                rt.sizeDelta = new Vector2(88f, 24f);

                var img = btnGo.AddComponent<Image>();
                img.sprite = SpriteHelper.GetOrCreateWhiteSprite();
                img.color = (lv == 1) ? new Color(0.2f, 0.8f, 0.4f, 1f) : new Color(0.18f, 0.24f, 0.35f, 1f);
                levelButtonImgs.Add(img);

                var btn = btnGo.AddComponent<Button>();
                btn.targetGraphic = img;
                btn.onClick.AddListener(() => onSelectLevel?.Invoke(lv));

                SkillTuningSliderFactory.CreateText(btnGo.transform, "Txt", $"Lv.{lv}", 12, TextAnchor.MiddleCenter, Vector2.zero, Vector2.one, new Vector2(0.5f, 0.5f), Vector2.zero, Vector2.zero, Color.white);
            }

            return levelButtonImgs;
        }

        public static List<Image> CreateMonsterSubTabs(
            Transform contentBox,
            Action<int> onSelectMonsterType)
        {
            string[] names = { "슬라임", "박쥐", "해골", "골렘", "화염임프", "독거미", "흑기사", "보스" };
            var images = new List<Image>();
            float startX = -205f;
            float y = -194f;

            for (int i = 0; i < names.Length; i++)
            {
                int captured = i;
                var btnGo = new GameObject($"BtnMon_{i}");
                btnGo.transform.SetParent(contentBox, false);
                var rt = btnGo.AddComponent<RectTransform>();
                rt.anchorMin = new Vector2(0.5f, 1f);
                rt.anchorMax = new Vector2(0.5f, 1f);
                rt.pivot = new Vector2(0.5f, 1f);
                rt.anchoredPosition = new Vector2(startX + i * 58f, y);
                rt.sizeDelta = new Vector2(55f, 24f);

                var img = btnGo.AddComponent<Image>();
                img.sprite = SpriteHelper.GetOrCreateWhiteSprite();
                img.color = (i == 0) ? new Color(0.8f, 0.3f, 0.2f, 1f) : new Color(0.18f, 0.24f, 0.35f, 1f);
                images.Add(img);

                var btn = btnGo.AddComponent<Button>();
                btn.targetGraphic = img;
                btn.onClick.AddListener(() => onSelectMonsterType?.Invoke(captured));

                SkillTuningSliderFactory.CreateText(btnGo.transform, "Txt", names[i], 10, TextAnchor.MiddleCenter, Vector2.zero, Vector2.one, new Vector2(0.5f, 0.5f), Vector2.zero, Vector2.zero, Color.white);
            }

            return images;
        }
    }
}
