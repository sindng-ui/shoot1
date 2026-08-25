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

        public static (string id, string name, string cat, bool isUltimate)[] AllSkillDefinitions = new (string id, string name, string cat, bool isUltimate)[]
        {
            // 1. Warrior
            ("slash", "대검 베기", "warrior", false),
            ("ground_stomp", "지면 강타", "warrior", false),
            ("whirlwind", "휠윈드", "warrior", false),
            ("blood_eater", "🩸 블러드 이터", "warrior", true),
            ("tempest_whirlwind", "🌪️ 템페스트 휠윈드", "warrior", true),
            ("earthshaker", "🌋 어스셰이커 파쇄", "warrior", true),

            // 2. Ranger
            ("bow", "관통 화살", "ranger", false),
            ("glaive", "칼바람 글레이브", "ranger", false),
            ("arrow_rain", "화살비", "ranger", false),
            ("storm_bow", "⚡ 폭풍의 활", "ranger", true),
            ("phantom_glaive", "🪃 팬텀 글레이브", "ranger", true),
            ("stellar_rain", "🌟 스텔라 레인", "ranger", true),

            // 3. Wizard
            ("fireball", "화염구", "wizard", false),
            ("frost_nova", "서리 폭발", "wizard", false),
            ("chain_lightning", "연쇄 번개", "wizard", false),
            ("meteor_strike", "☄️ 메테오 스트라이크", "wizard", true),
            ("blizzard_nova", "❄️ 블리자드 노바", "wizard", true),
            ("gigastorm_lightning", "⚡ 기가스톰 체인", "wizard", true),

            // 4. Passives (9 Total)
            ("passive_fang", "🧛 흡혈귀의 이빨", "passive", false),
            ("passive_feather", "🪶 바람의 깃털", "passive", false),
            ("passive_rune", "🔮 마나 룬", "passive", false),
            ("passive_armor", "🛡️ 강철 갑옷", "passive", false),
            ("passive_ring", "💍 황금 반지", "passive", false),
            ("passive_heart", "💖 생명의 펜던트", "passive", false),
            ("passive_ignition", "🔥 발화의 불꽃", "passive", true),
            ("passive_overcharge", "⚡ 과전류의 핵", "passive", true),
            ("passive_crit", "🎯 치명타의 눈", "passive", true),

            // 5. Common & Stats
            ("orbital", "⚔️ 수호의 검 (오비탈)", "common", false),
            ("crit_tuning", "🎯 치명/코어스탯", "common", true),

            // 6. System
            ("exp_tuning", "💎 경험치/레벨 튜닝", "system", false),
            ("monster_tuning", "👾 몬스터 스탯 튜닝", "system", true)
        };

        public static (string catId, string catName)[] Categories = new (string, string)[]
        {
            ("warrior", "⚔️ 전사"),
            ("ranger", "🏹 궁수"),
            ("wizard", "🧙 마법사"),
            ("passive", "🧬 패시브"),
            ("common", "🛡️ 공통"),
            ("system", "⚙️ 시스템")
        };

        public static void CreateCategoryTabs(
            Transform contentBox,
            Dictionary<string, Image> categoryTabImgs,
            Action<string> onSelectCategory)
        {
            float startX = -195f;
            float spacingX = 78f;
            float tabY = -72f;

            for (int i = 0; i < Categories.Length; i++)
            {
                var (catId, catName) = Categories[i];
                var catGo = new GameObject($"BtnCat_{catId}");
                catGo.transform.SetParent(contentBox, false);
                var rt = catGo.AddComponent<RectTransform>();
                rt.anchorMin = new Vector2(0.5f, 1f);
                rt.anchorMax = new Vector2(0.5f, 1f);
                rt.pivot = new Vector2(0.5f, 1f);
                rt.anchoredPosition = new Vector2(startX + i * spacingX, tabY);
                rt.sizeDelta = new Vector2(74f, 24f);

                var img = catGo.AddComponent<Image>();
                img.sprite = SpriteHelper.GetOrCreateWhiteSprite();
                img.color = (i == 0) ? new Color(0.18f, 0.55f, 0.85f, 1f) : new Color(0.12f, 0.16f, 0.22f, 0.95f);
                categoryTabImgs[catId] = img;

                var btn = catGo.AddComponent<Button>();
                btn.targetGraphic = img;
                string capturedCat = catId;
                btn.onClick.AddListener(() => onSelectCategory?.Invoke(capturedCat));

                SkillTuningSliderFactory.CreateText(catGo.transform, "Txt", catName, 11, TextAnchor.MiddleCenter, Vector2.zero, Vector2.one, new Vector2(0.5f, 0.5f), Vector2.zero, Vector2.zero, Color.white);
            }
        }

        public static void CreateSkillSelectButtons(
            Transform contentBox,
            Dictionary<string, (GameObject go, Image img)> skillTabButtons,
            Action<string> onSelectSkill,
            Action<string> onUnequipSkill)
        {
            // Create sub-buttons for all skills (grouped by category)
            foreach (var (catId, _) in Categories)
            {
                var skillsInCat = new List<(string id, string name, string cat, bool isUltimate)>();
                foreach (var def in AllSkillDefinitions)
                {
                    if (def.cat == catId) skillsInCat.Add(def);
                }

                // Special 3x3 Grid for Passives (9 items)
                if (catId == "passive")
                {
                    float pWidth = 145f;
                    float pHeight = 22f;
                    for (int i = 0; i < skillsInCat.Count; i++)
                    {
                        var def = skillsInCat[i];
                        int col = i % 3;
                        int row = i / 3;
                        float xPos = -150f + col * 150f;
                        float yPos = -98f - row * 24f;

                        var btnGo = CreateSubSkillButton(contentBox, def.id, def.name, new Vector2(xPos, yPos), new Vector2(pWidth, pHeight), def.isUltimate, onSelectSkill, onUnequipSkill);
                        skillTabButtons[def.id] = (btnGo, btnGo.GetComponent<Image>());
                    }
                    continue;
                }

                // 2 rows layout per standard category (Row 0: Base skills, Row 1: Ultimates/Special)
                var baseSkills = skillsInCat.FindAll(s => !s.isUltimate);
                var ultSkills = skillsInCat.FindAll(s => s.isUltimate);

                // Row 0: Base skills
                float btnWidth = 145f;
                float btnHeight = 23f;
                float startY = -100f;

                for (int i = 0; i < baseSkills.Count; i++)
                {
                    var def = baseSkills[i];
                    float xPos = -150f + i * 150f;
                    if (baseSkills.Count == 1) xPos = 0f;
                    else if (baseSkills.Count == 2) xPos = -75f + i * 150f;

                    var btnGo = CreateSubSkillButton(contentBox, def.id, def.name, new Vector2(xPos, startY), new Vector2(btnWidth, btnHeight), false, onSelectSkill, onUnequipSkill);
                    skillTabButtons[def.id] = (btnGo, btnGo.GetComponent<Image>());
                }

                // Row 1: Ultimates
                float ultY = -126f;
                for (int i = 0; i < ultSkills.Count; i++)
                {
                    var def = ultSkills[i];
                    float xPos = -150f + i * 150f;
                    if (ultSkills.Count == 1) xPos = 0f;
                    else if (ultSkills.Count == 2) xPos = -75f + i * 150f;

                    var btnGo = CreateSubSkillButton(contentBox, def.id, def.name, new Vector2(xPos, ultY), new Vector2(btnWidth, btnHeight), true, onSelectSkill, onUnequipSkill);
                    skillTabButtons[def.id] = (btnGo, btnGo.GetComponent<Image>());
                }
            }
        }

        private static GameObject CreateSubSkillButton(
            Transform parent,
            string id,
            string label,
            Vector2 pos,
            Vector2 size,
            bool isUltimate,
            Action<string> onSelectSkill,
            Action<string> onUnequipSkill)
        {
            var btnGo = new GameObject($"BtnTab_{id}");
            btnGo.transform.SetParent(parent, false);
            var rt = btnGo.AddComponent<RectTransform>();
            rt.anchorMin = new Vector2(0.5f, 1f);
            rt.anchorMax = new Vector2(0.5f, 1f);
            rt.pivot = new Vector2(0.5f, 1f);
            rt.anchoredPosition = pos;
            rt.sizeDelta = size;

            var img = btnGo.AddComponent<Image>();
            img.sprite = SpriteHelper.GetOrCreateWhiteSprite();
            img.color = isUltimate ? new Color(0.24f, 0.18f, 0.32f, 0.95f) : new Color(0.14f, 0.18f, 0.26f, 0.95f);

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

            Color txtColor = isUltimate ? new Color(1.0f, 0.88f, 0.40f, 1f) : Color.white;
            SkillTuningSliderFactory.CreateText(btnGo.transform, "Txt", label, 11, TextAnchor.MiddleCenter, Vector2.zero, Vector2.one, new Vector2(0.5f, 0.5f), Vector2.zero, Vector2.zero, txtColor);

            return btnGo;
        }

        public static Transform CreateSlidersScrollView(Transform contentBox)
        {
            var scrollGo = new GameObject("SlidersScrollView");
            scrollGo.transform.SetParent(contentBox, false);
            var scrollRt = scrollGo.AddComponent<RectTransform>();
            scrollRt.anchorMin = new Vector2(0f, 0f);
            scrollRt.anchorMax = new Vector2(1f, 1f);
            scrollRt.pivot = new Vector2(0.5f, 0.5f);
            scrollRt.offsetMin = new Vector2(12f, 105f); // 105px above bottom buttons
            scrollRt.offsetMax = new Vector2(-12f, -228f); // 228px below top toolbar & level selectors

            var scrollImg = scrollGo.AddComponent<Image>();
            scrollImg.sprite = SpriteHelper.GetOrCreateWhiteSprite();
            scrollImg.color = new Color(0.04f, 0.06f, 0.09f, 0.88f);

            // 🛡️ RectMask2D prevents any slider from overflowing outside viewport
            scrollGo.AddComponent<RectMask2D>();

            var contentObj = new GameObject("ScrollContent");
            contentObj.transform.SetParent(scrollGo.transform, false);
            var contentRect = contentObj.AddComponent<RectTransform>();
            contentRect.anchorMin = new Vector2(0f, 1f);
            contentRect.anchorMax = new Vector2(1f, 1f);
            contentRect.pivot = new Vector2(0f, 1f);
            contentRect.anchoredPosition = Vector2.zero;
            contentRect.sizeDelta = new Vector2(0f, 400f);

            var scrollRect = scrollGo.AddComponent<ScrollRect>();
            scrollRect.horizontal = false;
            scrollRect.vertical = true;
            scrollRect.scrollSensitivity = 25f;
            scrollRect.movementType = ScrollRect.MovementType.Clamped;
            scrollRect.viewport = scrollRt;
            scrollRect.content = contentRect;

            return contentObj.transform;
        }

        public static (Image spamImg, Text spamTxt) CreateUtilityToolbar(
            Transform contentBox,
            Action onToggleSpam,
            Action onResetDummies,
            Action onAddBatDummies)
        {
            float toolY = -174f;

            var spamGo = new GameObject("Btn_InfiniteSpam");
            spamGo.transform.SetParent(contentBox, false);
            var spRt = spamGo.AddComponent<RectTransform>();
            spRt.anchorMin = new Vector2(0.5f, 1f);
            spRt.anchorMax = new Vector2(0.5f, 1f);
            spRt.pivot = new Vector2(0.5f, 1f);
            spRt.anchoredPosition = new Vector2(-155f, toolY);
            spRt.sizeDelta = new Vector2(140f, 23f);

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
            dRt.sizeDelta = new Vector2(145f, 23f);

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
            bRt.sizeDelta = new Vector2(140f, 23f);

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
            float lvY = -199f;
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
                rt.sizeDelta = new Vector2(88f, 23f);

                var img = btnGo.AddComponent<Image>();
                img.sprite = SpriteHelper.GetOrCreateWhiteSprite();
                img.color = (lv == 1) ? new Color(0.2f, 0.8f, 0.4f, 1f) : new Color(0.18f, 0.24f, 0.35f, 1f);
                levelButtonImgs.Add(img);

                var btn = btnGo.AddComponent<Button>();
                btn.targetGraphic = img;
                btn.onClick.AddListener(() => onSelectLevel?.Invoke(lv));

                SkillTuningSliderFactory.CreateText(btnGo.transform, "Txt", $"Lv.{lv}", 11, TextAnchor.MiddleCenter, Vector2.zero, Vector2.one, new Vector2(0.5f, 0.5f), Vector2.zero, Vector2.zero, Color.white);
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
            float y = -184f;

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
                rt.sizeDelta = new Vector2(55f, 23f);

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
