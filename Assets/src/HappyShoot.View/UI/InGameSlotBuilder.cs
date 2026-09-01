using UnityEngine;
using UnityEngine.UI;
using HappyShoot.View.Utils;

namespace HappyShoot.View.UI
{
    /// <summary>
    /// Factory for creating Skill Slots, Dash Slots, and Passive Slots for InGame HUD.
    /// Strictly modular and under 500 lines (500-line architecture rule).
    /// </summary>
    public static class InGameSlotBuilder
    {
        public static GameObject CreatePassiveSlotElement(Transform parent, string name, Vector2 pos, float size, out Image icon, out Text lvlTxt, out Text valTxt)
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
            lvlTxt = InGameHudBuilder.CreateText(frameGo.transform, "LvBadge", "1", 10, TextAnchor.LowerRight, Vector2.zero, Vector2.one, new Vector2(1f, 0f), new Vector2(-2f, 2f), Vector2.zero, new Color(1f, 0.90f, 0.30f, 1f));
            var lvOutline = lvlTxt.gameObject.AddComponent<Outline>();
            lvOutline.effectColor = Color.black;
            lvOutline.effectDistance = new Vector2(1f, -1f);

            // Value / Stat Label (Right of icon)
            valTxt = InGameHudBuilder.CreateText(root.transform, "ValueText", "+15% ATK", 11, TextAnchor.MiddleLeft, new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(size + 6f, 0f), new Vector2(100f, 20f), new Color(0.9f, 0.95f, 1f, 1f));
            var valOutline = valTxt.gameObject.AddComponent<Outline>();
            valOutline.effectColor = Color.black;
            valOutline.effectDistance = new Vector2(1f, -1f);

            return root;
        }

        public static GameObject CreateSlotElement(Transform parent, string name, Vector2 pos, float size, bool isDash, out Image icon, out Image cooldownMask, out Text lvlTxt, out Text countTxt)
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
            cooldownMask.fillAmount = 0f;
            cooldownMask.raycastTarget = false;

            // Badge / Key Label
            if (isDash)
            {
                lvlTxt = null;
                countTxt = null;
                var keyTxt = InGameHudBuilder.CreateText(root.transform, "KeyBadge", "Space", 10, TextAnchor.LowerCenter, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0f, -6f), new Vector2(42f, 16f), new Color(0.4f, 0.9f, 1f, 1f));
                var keyOutline = keyTxt.gameObject.AddComponent<Outline>();
                keyOutline.effectColor = Color.black;
            }
            else
            {
                lvlTxt = InGameHudBuilder.CreateText(root.transform, "LvBadge", "Lv.1", 11, TextAnchor.MiddleCenter, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0f, -6f), new Vector2(36f, 16f), new Color(1f, 0.90f, 0.30f, 1f));
                var lvOutline = lvlTxt.gameObject.AddComponent<Outline>();
                lvOutline.effectColor = Color.black;

                countTxt = InGameHudBuilder.CreateText(root.transform, "CountBadge", "", 12, TextAnchor.UpperRight, new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(-4f, -4f), new Vector2(24f, 16f), new Color(0.35f, 0.95f, 1f, 1f));
                var countOutline = countTxt.gameObject.AddComponent<Outline>();
                countOutline.effectColor = Color.black;
                countOutline.effectDistance = new Vector2(1f, -1f);
            }

            return root;
        }
    }
}
