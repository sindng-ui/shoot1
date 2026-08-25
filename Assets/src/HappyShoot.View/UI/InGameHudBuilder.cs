using UnityEngine;
using UnityEngine.UI;
using HappyShoot.View.Utils;

namespace HappyShoot.View.UI
{
    /// <summary>
    /// Procedural UI Builder that constructs Soulstone Survivors-style bottom 3-layer HUD elements.
    /// Strictly modular and under 500 lines.
    /// </summary>
    public static class InGameHudBuilder
    {
        public const int MaxSkillSlots = 6;
        public const int MaxPassiveSlots = 9;

        public struct HudComponents
        {
            public Canvas Canvas;
            public CanvasScaler Scaler;
            public Slider ExpSlider;
            public Text LevelText;
            public Text ExpText;
            public Slider HealthSlider;
            public Text HealthText;
            public Text TimerText;
            public Text KillCountText;
            public Text GoldText;
            public Image[] SkillSlotIcons;
            public Image[] SkillSlotCooldownMasks;
            public Text[] SkillSlotLevelTexts;
            public Text[] SkillSlotCountTexts;
            public GameObject[] SkillSlotRoots;
            public Image[] PassiveSlotIcons;
            public Text[] PassiveSlotLevelTexts;
            public Text[] PassiveSlotValueTexts;
            public GameObject[] PassiveSlotRoots;
            public Image DashIcon;
            public Image DashCooldownMask;
        }

        public static HudComponents BuildHud(Transform rootParent)
        {
            var res = new HudComponents
            {
                SkillSlotIcons = new Image[MaxSkillSlots],
                SkillSlotCooldownMasks = new Image[MaxSkillSlots],
                SkillSlotLevelTexts = new Text[MaxSkillSlots],
                SkillSlotCountTexts = new Text[MaxSkillSlots],
                SkillSlotRoots = new GameObject[MaxSkillSlots],
                PassiveSlotIcons = new Image[MaxPassiveSlots],
                PassiveSlotLevelTexts = new Text[MaxPassiveSlots],
                PassiveSlotValueTexts = new Text[MaxPassiveSlots],
                PassiveSlotRoots = new GameObject[MaxPassiveSlots]
            };

            // 1. Master Overlay Canvas
            var canvasGo = new GameObject("InGameHudCanvas");
            if (rootParent != null) canvasGo.transform.SetParent(rootParent, false);

            var canvas = canvasGo.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 10;
            res.Canvas = canvas;

            var scaler = canvasGo.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight = 0.5f;
            res.Scaler = scaler;

            canvasGo.AddComponent<GraphicRaycaster>();

            // =========================================================================
            // LAYER 1 (Bottom): 10-Segmented EXP Progress Bar + Diamond Level Badge
            // =========================================================================
            float expBarWidth = 1200f;
            float expBarHeight = 18f;

            var expRootGo = new GameObject("ExpBarRoot");
            expRootGo.transform.SetParent(canvasGo.transform, false);
            var expRootRt = expRootGo.AddComponent<RectTransform>();
            expRootRt.anchorMin = new Vector2(0.5f, 0f);
            expRootRt.anchorMax = new Vector2(0.5f, 0f);
            expRootRt.pivot = new Vector2(0.5f, 0f);
            expRootRt.anchoredPosition = new Vector2(0f, 6f);
            expRootRt.sizeDelta = new Vector2(expBarWidth, expBarHeight);

            // 10-Segment Frame
            var expBgImg = expRootGo.AddComponent<Image>();
            expBgImg.sprite = HudSpriteHelper.GetOrCreateExpBar10SegmentSprite();
            expBgImg.type = Image.Type.Sliced;

            // Fill Area & Fill Image
            var expFillAreaGo = new GameObject("ExpFillArea");
            expFillAreaGo.transform.SetParent(expRootGo.transform, false);
            var expFillAreaRt = expFillAreaGo.AddComponent<RectTransform>();
            expFillAreaRt.anchorMin = Vector2.zero;
            expFillAreaRt.anchorMax = Vector2.one;
            expFillAreaRt.offsetMin = new Vector2(3f, 2f);
            expFillAreaRt.offsetMax = new Vector2(-3f, -2f);

            var expFillGo = new GameObject("ExpFill");
            expFillGo.transform.SetParent(expFillAreaGo.transform, false);
            var expFillRt = expFillGo.AddComponent<RectTransform>();
            expFillRt.anchorMin = new Vector2(0f, 0f);
            expFillRt.anchorMax = new Vector2(0f, 1f);
            expFillRt.offsetMin = Vector2.zero;
            expFillRt.offsetMax = Vector2.zero;

            var expFillImg = expFillGo.AddComponent<Image>();
            expFillImg.sprite = SpriteHelper.GetOrCreateWhiteSprite();
            expFillImg.color = new Color(0.95f, 0.82f, 0.25f, 0.95f); // Lustrous Gold

            var expSlider = expRootGo.AddComponent<Slider>();
            expSlider.fillRect = expFillRt;
            expSlider.minValue = 0f;
            expSlider.maxValue = 1f;
            expSlider.value = 0f;
            res.ExpSlider = expSlider;

            // Diamond Level Badge (Left of Exp Bar)
            var badgeGo = new GameObject("LevelBadge");
            badgeGo.transform.SetParent(expRootGo.transform, false);
            var badgeRt = badgeGo.AddComponent<RectTransform>();
            badgeRt.anchorMin = new Vector2(0f, 0.5f);
            badgeRt.anchorMax = new Vector2(0f, 0.5f);
            badgeRt.pivot = new Vector2(1f, 0.5f);
            badgeRt.anchoredPosition = new Vector2(-8f, 0f);
            badgeRt.sizeDelta = new Vector2(38f, 38f);

            var badgeImg = badgeGo.AddComponent<Image>();
            badgeImg.sprite = HudSpriteHelper.GetOrCreateLevelBadgeSprite();

            var levelText = CreateText(badgeGo.transform, "LvText", "1", 16, TextAnchor.MiddleCenter, Vector2.zero, Vector2.one, new Vector2(0.5f, 0.5f), Vector2.zero, Vector2.zero, new Color(1f, 0.95f, 0.70f, 1f));
            res.LevelText = levelText;

            // Exp Progress Text (Center of Exp Bar)
            var expText = CreateText(expRootGo.transform, "ExpText", "EXP 0 / 12 (0%)", 11, TextAnchor.MiddleCenter, Vector2.zero, Vector2.one, new Vector2(0.5f, 0.5f), Vector2.zero, Vector2.zero, Color.white);
            var expTextOutline = expText.gameObject.AddComponent<Outline>();
            expTextOutline.effectColor = Color.black;
            expTextOutline.effectDistance = new Vector2(1f, -1f);
            res.ExpText = expText;

            // =========================================================================
            // LAYER 2 (Middle): 6 Active Skill Slots + Dash Slot + Radial 360 Cooldown
            // =========================================================================
            var skillRowGo = new GameObject("SkillSlotsRow");
            skillRowGo.transform.SetParent(canvasGo.transform, false);
            var skillRowRt = skillRowGo.AddComponent<RectTransform>();
            skillRowRt.anchorMin = new Vector2(0.5f, 0f);
            skillRowRt.anchorMax = new Vector2(0.5f, 0f);
            skillRowRt.pivot = new Vector2(0.5f, 0f);
            skillRowRt.anchoredPosition = new Vector2(0f, 32f);
            skillRowRt.sizeDelta = new Vector2(460f, 54f);

            // Dash Slot (Leftmost, with Space badge)
            float slotSize = 48f;
            float spacing = 54f;
            float startX = -((MaxSkillSlots * spacing) * 0.5f) + 27f;

            var dashSlotGo = CreateSlotElement(skillRowGo.transform, "Slot_Dash", new Vector2(startX - 62f, 0f), slotSize, isDash: true, out var dashIcon, out var dashMask, out _, out _);
            dashIcon.sprite = RewardIconHelper.GetOrCreateRewardIcon("passive_feather");
            res.DashIcon = dashIcon;
            res.DashCooldownMask = dashMask;

            // 6 Active Skill Slots
            for (int i = 0; i < MaxSkillSlots; i++)
            {
                var slotGo = CreateSlotElement(skillRowGo.transform, $"Slot_{i}", new Vector2(startX + i * spacing, 0f), slotSize, isDash: false, out var icon, out var mask, out var lvTxt, out var countTxt);
                res.SkillSlotRoots[i] = slotGo;
                res.SkillSlotIcons[i] = icon;
                res.SkillSlotCooldownMasks[i] = mask;
                res.SkillSlotLevelTexts[i] = lvTxt;
                res.SkillSlotCountTexts[i] = countTxt;
                slotGo.SetActive(false);
            }

            // =========================================================================
            // LAYER 3 (Top): Helmet Emblem Frame + Wide Health Bar
            // =========================================================================
            float hpWidth = 480f;
            float hpHeight = 22f;

            var hpRootGo = new GameObject("HealthBarRoot");
            hpRootGo.transform.SetParent(canvasGo.transform, false);
            var hpRootRt = hpRootGo.AddComponent<RectTransform>();
            hpRootRt.anchorMin = new Vector2(0.5f, 0f);
            hpRootRt.anchorMax = new Vector2(0.5f, 0f);
            hpRootRt.pivot = new Vector2(0.5f, 0f);
            hpRootRt.anchoredPosition = new Vector2(0f, 92f);
            hpRootRt.sizeDelta = new Vector2(hpWidth, hpHeight);

            // Health Bar Background Track
            var hpBgGo = new GameObject("HpBackground");
            hpBgGo.transform.SetParent(hpRootGo.transform, false);
            var hpBgRt = hpBgGo.AddComponent<RectTransform>();
            hpBgRt.anchorMin = Vector2.zero;
            hpBgRt.anchorMax = Vector2.one;
            hpBgRt.offsetMin = Vector2.zero;
            hpBgRt.offsetMax = Vector2.zero;

            var hpBgImg = hpBgGo.AddComponent<Image>();
            hpBgImg.sprite = SpriteHelper.GetOrCreateWhiteSprite();
            hpBgImg.color = new Color(0.12f, 0.12f, 0.15f, 0.90f);

            // Health Bar Fill Area
            var hpFillAreaGo = new GameObject("HpFillArea");
            hpFillAreaGo.transform.SetParent(hpRootGo.transform, false);
            var hpFillAreaRt = hpFillAreaGo.AddComponent<RectTransform>();
            hpFillAreaRt.anchorMin = Vector2.zero;
            hpFillAreaRt.anchorMax = Vector2.one;
            hpFillAreaRt.offsetMin = new Vector2(2f, 2f);
            hpFillAreaRt.offsetMax = new Vector2(-2f, -2f);

            var hpFillGo = new GameObject("HpFill");
            hpFillGo.transform.SetParent(hpFillAreaGo.transform, false);
            var hpFillRt = hpFillGo.AddComponent<RectTransform>();
            hpFillRt.anchorMin = new Vector2(0f, 0f);
            hpFillRt.anchorMax = new Vector2(0f, 1f);
            hpFillRt.offsetMin = Vector2.zero;
            hpFillRt.offsetMax = Vector2.zero;

            var hpFillImg = hpFillGo.AddComponent<Image>();
            hpFillImg.sprite = SpriteHelper.GetOrCreateWhiteSprite();
            hpFillImg.color = new Color(0.92f, 0.18f, 0.22f, 0.95f); // Vibrant Ruby Red

            var healthSlider = hpRootGo.AddComponent<Slider>();
            healthSlider.fillRect = hpFillRt;
            healthSlider.minValue = 0f;
            healthSlider.maxValue = 1f;
            healthSlider.value = 1f;
            res.HealthSlider = healthSlider;

            // Helmet Emblem Center Crest
            var emblemGo = new GameObject("HelmetEmblem");
            emblemGo.transform.SetParent(hpRootGo.transform, false);
            var emblemRt = emblemGo.AddComponent<RectTransform>();
            emblemRt.anchorMin = new Vector2(0.5f, 1f);
            emblemRt.anchorMax = new Vector2(0.5f, 1f);
            emblemRt.pivot = new Vector2(0.5f, 0.5f);
            emblemRt.anchoredPosition = new Vector2(0f, 6f);
            emblemRt.sizeDelta = new Vector2(52f, 40f);

            var emblemImg = emblemGo.AddComponent<Image>();
            emblemImg.sprite = HudSpriteHelper.GetOrCreateHelmetEmblemSprite();
            emblemImg.raycastTarget = false;

            // HP Text
            var healthText = CreateText(hpRootGo.transform, "HpText", "100 / 100", 12, TextAnchor.MiddleCenter, Vector2.zero, Vector2.one, new Vector2(0.5f, 0.5f), Vector2.zero, Vector2.zero, Color.white);
            var hpTextOutline = healthText.gameObject.AddComponent<Outline>();
            hpTextOutline.effectColor = Color.black;
            hpTextOutline.effectDistance = new Vector2(1f, -1f);
            res.HealthText = healthText;

            // =========================================================================
            // TOP STATUS: Timer (Center), Kills & Gold (Right)
            // =========================================================================
            res.TimerText = CreateText(canvasGo.transform, "TimerText", "00:00", 28, TextAnchor.MiddleCenter, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -24f), new Vector2(160f, 40f), new Color(1f, 0.95f, 0.7f, 1f));
            var timerOutline = res.TimerText.gameObject.AddComponent<Outline>();
            timerOutline.effectColor = new Color(0f, 0f, 0f, 0.8f);

            res.KillCountText = CreateText(canvasGo.transform, "KillsText", "💀 0", 18, TextAnchor.MiddleRight, new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(-40f, -24f), new Vector2(140f, 30f), new Color(1f, 0.45f, 0.45f, 1f));
            var killOutline = res.KillCountText.gameObject.AddComponent<Outline>();
            killOutline.effectColor = Color.black;

            res.GoldText = CreateText(canvasGo.transform, "GoldText", "💰 0", 18, TextAnchor.MiddleRight, new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(-40f, -54f), new Vector2(140f, 30f), new Color(1f, 0.85f, 0.25f, 1f));
            var goldOutline = res.GoldText.gameObject.AddComponent<Outline>();
            goldOutline.effectColor = Color.black;

            // =========================================================================
            // LAYER 4 (Left Column): 9 Passive Buff Slots with Level & Value Display
            // =========================================================================
            var passiveColGo = new GameObject("PassiveSlotsColumn");
            passiveColGo.transform.SetParent(canvasGo.transform, false);
            var passiveColRt = passiveColGo.AddComponent<RectTransform>();
            passiveColRt.anchorMin = new Vector2(0f, 0.5f);
            passiveColRt.anchorMax = new Vector2(0f, 0.5f);
            passiveColRt.pivot = new Vector2(0f, 0.5f);
            passiveColRt.anchoredPosition = new Vector2(24f, 20f);
            passiveColRt.sizeDelta = new Vector2(160f, 420f);

            float pSlotSize = 34f;
            float pSpacing = 42f;
            float pStartY = ((MaxPassiveSlots * pSpacing) * 0.5f) - 21f;

            for (int i = 0; i < MaxPassiveSlots; i++)
            {
                var slotGo = CreatePassiveSlotElement(passiveColGo.transform, $"PassiveSlot_{i}", new Vector2(0f, pStartY - i * pSpacing), pSlotSize, out var icon, out var lvTxt, out var valTxt);
                res.PassiveSlotRoots[i] = slotGo;
                res.PassiveSlotIcons[i] = icon;
                res.PassiveSlotLevelTexts[i] = lvTxt;
                res.PassiveSlotValueTexts[i] = valTxt;
                slotGo.SetActive(false);
            }

            return res;
        }

        private static GameObject CreatePassiveSlotElement(Transform parent, string name, Vector2 pos, float size, out Image icon, out Text lvlTxt, out Text valTxt)
        {
            var root = new GameObject(name);
            root.transform.SetParent(parent, false);
            var rt = root.AddComponent<RectTransform>();
            rt.anchorMin = new Vector2(0f, 0.5f);
            rt.anchorMax = new Vector2(0f, 0.5f);
            rt.pivot = new Vector2(0f, 0.5f);
            rt.anchoredPosition = pos;
            rt.sizeDelta = new Vector2(size + 110f, size);

            // Frame Image (Left-aligned)
            var frameGo = new GameObject("Frame");
            frameGo.transform.SetParent(root.transform, false);
            var frameRt = frameGo.AddComponent<RectTransform>();
            frameRt.anchorMin = new Vector2(0f, 0.5f);
            frameRt.anchorMax = new Vector2(0f, 0.5f);
            frameRt.pivot = new Vector2(0f, 0.5f);
            frameRt.anchoredPosition = Vector2.zero;
            frameRt.sizeDelta = new Vector2(size, size);

            var frameImg = frameGo.AddComponent<Image>();
            frameImg.sprite = HudSpriteHelper.GetOrCreateSkillSlotFrameSprite();
            frameImg.type = Image.Type.Sliced;

            // Icon Image
            var iconGo = new GameObject("Icon");
            iconGo.transform.SetParent(frameGo.transform, false);
            var iconRt = iconGo.AddComponent<RectTransform>();
            iconRt.anchorMin = Vector2.zero;
            iconRt.anchorMax = Vector2.one;
            iconRt.offsetMin = new Vector2(3f, 3f);
            iconRt.offsetMax = new Vector2(-3f, -3f);

            icon = iconGo.AddComponent<Image>();
            icon.raycastTarget = false;

            // Level Badge (Inside Icon bottom-right)
            lvlTxt = CreateText(frameGo.transform, "LvBadge", "1", 10, TextAnchor.LowerRight, Vector2.zero, Vector2.one, new Vector2(1f, 0f), new Vector2(-2f, 2f), Vector2.zero, new Color(1f, 0.90f, 0.30f, 1f));
            var lvOutline = lvlTxt.gameObject.AddComponent<Outline>();
            lvOutline.effectColor = Color.black;
            lvOutline.effectDistance = new Vector2(1f, -1f);

            // Value / Stat Label (Right of icon)
            valTxt = CreateText(root.transform, "ValueText", "+15% ATK", 11, TextAnchor.MiddleLeft, new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(size + 6f, 0f), new Vector2(100f, 20f), new Color(0.9f, 0.95f, 1f, 1f));
            var valOutline = valTxt.gameObject.AddComponent<Outline>();
            valOutline.effectColor = Color.black;
            valOutline.effectDistance = new Vector2(1f, -1f);

            return root;
        }

        private static GameObject CreateSlotElement(Transform parent, string name, Vector2 pos, float size, bool isDash, out Image icon, out Image cooldownMask, out Text lvlTxt, out Text countTxt)
        {
            var root = new GameObject(name);
            root.transform.SetParent(parent, false);
            var rt = root.AddComponent<RectTransform>();
            rt.anchorMin = new Vector2(0.5f, 0.5f);
            rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.anchoredPosition = pos;
            rt.sizeDelta = new Vector2(size, size);

            // Frame Image
            var frameImg = root.AddComponent<Image>();
            frameImg.sprite = HudSpriteHelper.GetOrCreateSkillSlotFrameSprite();
            frameImg.type = Image.Type.Sliced;

            // Icon Image
            var iconGo = new GameObject("Icon");
            iconGo.transform.SetParent(root.transform, false);
            var iconRt = iconGo.AddComponent<RectTransform>();
            iconRt.anchorMin = Vector2.zero;
            iconRt.anchorMax = Vector2.one;
            iconRt.offsetMin = new Vector2(4f, 4f);
            iconRt.offsetMax = new Vector2(-4f, -4f);

            icon = iconGo.AddComponent<Image>();
            icon.raycastTarget = false;

            // Radial 360 Cooldown Dark Mask
            var cdGo = new GameObject("CooldownMask");
            cdGo.transform.SetParent(root.transform, false);
            var cdRt = cdGo.AddComponent<RectTransform>();
            cdRt.anchorMin = Vector2.zero;
            cdRt.anchorMax = Vector2.one;
            cdRt.offsetMin = new Vector2(4f, 4f);
            cdRt.offsetMax = new Vector2(-4f, -4f);

            cooldownMask = cdGo.AddComponent<Image>();
            cooldownMask.sprite = SpriteHelper.GetOrCreateWhiteSprite();
            cooldownMask.color = new Color(0f, 0f, 0f, 0.68f);
            cooldownMask.type = Image.Type.Filled;
            cooldownMask.fillMethod = Image.FillMethod.Radial360;
            cooldownMask.fillOrigin = (int)Image.Origin360.Top;
            cooldownMask.fillClockwise = true;
            cooldownMask.fillAmount = 0f; // 0 = ready, 1 = on cooldown
            cooldownMask.raycastTarget = false;

            // Badge / Key Label
            if (isDash)
            {
                lvlTxt = null;
                countTxt = null;
                var keyTxt = CreateText(root.transform, "KeyBadge", "Space", 10, TextAnchor.LowerCenter, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0f, -6f), new Vector2(42f, 16f), new Color(0.4f, 0.9f, 1f, 1f));
                var keyOutline = keyTxt.gameObject.AddComponent<Outline>();
                keyOutline.effectColor = Color.black;
            }
            else
            {
                lvlTxt = CreateText(root.transform, "LvBadge", "Lv.1", 11, TextAnchor.MiddleCenter, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0f, -6f), new Vector2(36f, 16f), new Color(1f, 0.90f, 0.30f, 1f));
                var lvOutline = lvlTxt.gameObject.AddComponent<Outline>();
                lvOutline.effectColor = Color.black;

                // Projectile / Sub-Count Badge (Top-Right of Slot)
                countTxt = CreateText(root.transform, "CountBadge", "", 12, TextAnchor.UpperRight, new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(-4f, -4f), new Vector2(24f, 16f), new Color(0.35f, 0.95f, 1f, 1f));
                var countOutline = countTxt.gameObject.AddComponent<Outline>();
                countOutline.effectColor = Color.black;
                countOutline.effectDistance = new Vector2(1f, -1f);
            }

            return root;
        }

        public static Text CreateText(Transform parent, string name, string defaultText, int fontSize, TextAnchor alignment, Vector2 anchorMin, Vector2 anchorMax, Vector2 pivot, Vector2 anchoredPos, Vector2 sizeDelta, Color color)
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
