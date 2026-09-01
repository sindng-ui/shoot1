using UnityEngine;
using UnityEngine.UI;
using HappyShoot.View.SkillTree;
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
            public Text RubyText;
            public Text EmeraldText;
            public Text AmethystText;
            public RectTransform RubyIconRt;
            public RectTransform EmeraldIconRt;
            public RectTransform AmethystIconRt;
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

            var dashSlotGo = InGameSlotBuilder.CreateSlotElement(skillRowGo.transform, "Slot_Dash", new Vector2(startX - 62f, 0f), slotSize, isDash: true, out var dashIcon, out var dashMask, out _, out _);
            dashIcon.sprite = RewardIconHelper.GetOrCreateRewardIcon("passive_feather");
            res.DashIcon = dashIcon;
            res.DashCooldownMask = dashMask;

            // 6 Active Skill Slots
            for (int i = 0; i < MaxSkillSlots; i++)
            {
                var slotGo = InGameSlotBuilder.CreateSlotElement(skillRowGo.transform, $"Slot_{i}", new Vector2(startX + i * spacing, 0f), slotSize, isDash: false, out var icon, out var mask, out var lvTxt, out var countTxt);
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
            // TOP STATUS: Center Timer & Top-Left Unified 5-Resource Capsule (Gems + Gold + Kills)
            // =========================================================================
            res.TimerText = CreateText(canvasGo.transform, "TimerText", "00:00", 28, TextAnchor.MiddleCenter, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -24f), new Vector2(160f, 40f), new Color(1f, 0.95f, 0.7f, 1f));
            var timerOutline = res.TimerText.gameObject.AddComponent<Outline>();
            timerOutline.effectColor = new Color(0f, 0f, 0f, 0.8f);

            // Unified Top-Left Capsule (3 Gems + Gold + Kills) - Aligned with Left Passive Column
            var statsCapsuleGo = new GameObject("TopLeftUnifiedResourceCapsule");
            statsCapsuleGo.transform.SetParent(canvasGo.transform, false);
            var statsRt = statsCapsuleGo.AddComponent<RectTransform>();
            statsRt.anchorMin = new Vector2(0f, 1f);
            statsRt.anchorMax = new Vector2(0f, 1f);
            statsRt.pivot = new Vector2(0f, 1f);
            statsRt.anchoredPosition = new Vector2(20f, -16f);
            statsRt.sizeDelta = new Vector2(365f, 36f);

            var statsBg = statsCapsuleGo.AddComponent<Image>();
            statsBg.sprite = SpriteHelper.GetOrCreateWhiteSprite();
            statsBg.color = new Color(0.06f, 0.08f, 0.13f, 0.90f);

            var statsOutline = statsCapsuleGo.AddComponent<Outline>();
            statsOutline.effectColor = new Color(0.85f, 0.70f, 0.30f, 0.40f);
            statsOutline.effectDistance = new Vector2(1f, -1f);

            // 1. 3 Gem Slots (🔴 Ruby, 🟢 Emerald, 🟣 Amethyst)
            CreateResourceBadge(statsCapsuleGo.transform, "RubyBadge", 28f, GemSpriteHelper.GetOrCreateRubySprite(32), new Color(1.0f, 0.85f, 0.88f), 48f, out res.RubyIconRt, out res.RubyText);
            CreateResourceBadge(statsCapsuleGo.transform, "EmeraldBadge", 80f, GemSpriteHelper.GetOrCreateEmeraldSprite(32), new Color(0.85f, 1.0f, 0.90f), 48f, out res.EmeraldIconRt, out res.EmeraldText);
            CreateResourceBadge(statsCapsuleGo.transform, "AmethystBadge", 132f, GemSpriteHelper.GetOrCreateAmethystSprite(32), new Color(0.94f, 0.85f, 1.0f), 48f, out res.AmethystIconRt, out res.AmethystText);

            // Divider Line
            var divGo = new GameObject("ResourceDivider");
            divGo.transform.SetParent(statsCapsuleGo.transform, false);
            var divRt = divGo.AddComponent<RectTransform>();
            divRt.anchorMin = new Vector2(0f, 0.5f);
            divRt.anchorMax = new Vector2(0f, 0.5f);
            divRt.pivot = new Vector2(0.5f, 0.5f);
            divRt.anchoredPosition = new Vector2(162f, 0f);
            divRt.sizeDelta = new Vector2(1.5f, 20f);
            var divImg = divGo.AddComponent<Image>();
            divImg.sprite = SpriteHelper.GetOrCreateWhiteSprite();
            divImg.color = new Color(0.85f, 0.70f, 0.30f, 0.35f);
            divImg.raycastTarget = false;

            // 2. Gold & Kills Slots (💰 Gold, 💀 Kills)
            CreateResourceBadge(statsCapsuleGo.transform, "GoldBadge", 218f, HudSpriteHelper.GetOrCreateCoinIcon(24), new Color(1.0f, 0.88f, 0.30f), 80f, out _, out res.GoldText);
            CreateResourceBadge(statsCapsuleGo.transform, "KillsBadge", 305f, HudSpriteHelper.GetOrCreateSkullIcon(24), new Color(1.0f, 0.65f, 0.65f), 65f, out _, out res.KillCountText);

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
                var slotGo = InGameSlotBuilder.CreatePassiveSlotElement(passiveColGo.transform, $"PassiveSlot_{i}", new Vector2(0f, pStartY - i * pSpacing), pSlotSize, out var icon, out var lvTxt, out var valTxt);
                res.PassiveSlotRoots[i] = slotGo;
                res.PassiveSlotIcons[i] = icon;
                res.PassiveSlotLevelTexts[i] = lvTxt;
                res.PassiveSlotValueTexts[i] = valTxt;
                slotGo.SetActive(false);
            }

            return res;
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

        private static void CreateResourceBadge(Transform parent, string name, float centerX, Sprite iconSprite, Color textColor, float width, out RectTransform iconRt, out Text textComponent)
        {
            var badgeGo = new GameObject(name);
            badgeGo.transform.SetParent(parent, false);
            var badgeRt = badgeGo.AddComponent<RectTransform>();
            badgeRt.anchorMin = new Vector2(0f, 0.5f);
            badgeRt.anchorMax = new Vector2(0f, 0.5f);
            badgeRt.pivot = new Vector2(0.5f, 0.5f);
            badgeRt.anchoredPosition = new Vector2(centerX, 0f);
            badgeRt.sizeDelta = new Vector2(width, 30f);

            // Icon Image
            var iconGo = new GameObject("Icon");
            iconGo.transform.SetParent(badgeGo.transform, false);
            iconRt = iconGo.AddComponent<RectTransform>();
            iconRt.anchorMin = new Vector2(0f, 0.5f);
            iconRt.anchorMax = new Vector2(0f, 0.5f);
            iconRt.pivot = new Vector2(0.5f, 0.5f);
            iconRt.anchoredPosition = new Vector2(12f, 0f);
            iconRt.sizeDelta = new Vector2(22f, 22f);

            var iconImg = iconGo.AddComponent<Image>();
            iconImg.sprite = iconSprite;
            iconImg.color = Color.white;
            iconImg.preserveAspect = true;
            iconImg.raycastTarget = false;

            // Value Text
            var textGo = new GameObject("ValueText");
            textGo.transform.SetParent(badgeGo.transform, false);
            var textRt = textGo.AddComponent<RectTransform>();
            textRt.anchorMin = new Vector2(0f, 0.5f);
            textRt.anchorMax = new Vector2(0f, 0.5f);
            textRt.pivot = new Vector2(0f, 0.5f);
            textRt.anchoredPosition = new Vector2(26f, 0f);
            textRt.sizeDelta = new Vector2(width - 26f, 22f);

            textComponent = textGo.AddComponent<Text>();
            textComponent.font = FontHelper.GetKoreanFont();
            textComponent.fontSize = 15;
            textComponent.fontStyle = FontStyle.Bold;
            textComponent.alignment = TextAnchor.MiddleLeft;
            textComponent.color = textColor;
            textComponent.text = "0";
            textComponent.raycastTarget = false;

            var outline = textGo.AddComponent<Outline>();
            outline.effectColor = new Color(0f, 0f, 0f, 0.95f);
            outline.effectDistance = new Vector2(1.2f, -1.2f);
        }
    }
}
