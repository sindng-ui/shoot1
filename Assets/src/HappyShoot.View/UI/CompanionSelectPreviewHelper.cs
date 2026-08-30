using UnityEngine;
using UnityEngine.UI;
using HappyShoot.Domain.Entities;
using HappyShoot.Domain.Progression;
using HappyShoot.View.Utils;

namespace HappyShoot.View.UI
{
    /// <summary>
    /// Renders companion preview cards on the character selection screen (Main Menu).
    /// Displays unlocked companions flanking the Wizard or lock silhouettes with unlock requirements.
    /// Strictly modular and under 500 lines.
    /// </summary>
    public static class CompanionSelectPreviewHelper
    {
        public static void CreateCompanionPreviewCards(Transform parent, SkillTreeManager skillTreeManager)
        {
            bool warriorUnlocked = skillTreeManager != null && skillTreeManager.IsWarriorUnlocked;
            bool rangerUnlocked = skillTreeManager != null && skillTreeManager.IsRangerUnlocked;

            // 1. Warrior Card (Left: x = -340, y = 65)
            CreateCompanionCard(
                parent,
                "WarriorCompanionCard",
                new Vector2(-340f, 65f),
                CharacterClassType.Warrior,
                "🛡️ 호위 전사",
                "근접 대검 베기 호위\n(본체 공격력 1/3 연동)",
                "🔒 1회차 클리어 시\n전사 동료 해금",
                warriorUnlocked);

            // 2. Ranger Card (Right: x = 340, y = 65)
            CreateCompanionCard(
                parent,
                "RangerCompanionCard",
                new Vector2(340f, 65f),
                CharacterClassType.Ranger,
                "🏹 지원 궁수",
                "원거리 관통 화살 지원\n(본체 공격력 1/3 연동)",
                "🔒 2회차 클리어 시\n궁수 동료 해금",
                rangerUnlocked);
        }

        private static void CreateCompanionCard(
            Transform parent,
            string name,
            Vector2 pos,
            CharacterClassType classType,
            string title,
            string unlockedDesc,
            string lockedDesc,
            bool isUnlocked)
        {
            var cardGo = new GameObject(name);
            cardGo.transform.SetParent(parent, false);
            var cardRt = cardGo.AddComponent<RectTransform>();
            cardRt.anchorMin = new Vector2(0.5f, 0.5f);
            cardRt.anchorMax = new Vector2(0.5f, 0.5f);
            cardRt.pivot = new Vector2(0.5f, 0.5f);
            cardRt.anchoredPosition = pos;
            cardRt.sizeDelta = new Vector2(210f, 310f);

            var bg = cardGo.AddComponent<Image>();
            bg.sprite = SpriteHelper.GetOrCreateWhiteSprite();
            bg.color = isUnlocked
                ? new Color(0.12f, 0.14f, 0.22f, 0.88f)
                : new Color(0.06f, 0.07f, 0.10f, 0.75f);

            var outline = cardGo.AddComponent<Outline>();
            outline.effectColor = isUnlocked
                ? (classType == CharacterClassType.Warrior ? new Color(0.85f, 0.45f, 0.35f, 0.6f) : new Color(0.35f, 0.85f, 0.55f, 0.6f))
                : new Color(0.3f, 0.3f, 0.35f, 0.3f);
            outline.effectDistance = new Vector2(1.5f, -1.5f);

            // Avatar Icon
            var avatarGo = new GameObject("Avatar");
            avatarGo.transform.SetParent(cardGo.transform, false);
            var avatarRt = avatarGo.AddComponent<RectTransform>();
            avatarRt.anchorMin = new Vector2(0.5f, 1f);
            avatarRt.anchorMax = new Vector2(0.5f, 1f);
            avatarRt.pivot = new Vector2(0.5f, 1f);
            avatarRt.anchoredPosition = new Vector2(0f, -20f);
            avatarRt.sizeDelta = new Vector2(96f, 96f);

            var iconImg = avatarGo.AddComponent<Image>();
            iconImg.sprite = HeroSpriteHelper.GetHeroSprite(classType, HeroSpriteHelper.ViewDirection.Front, 32);
            iconImg.color = isUnlocked ? Color.white : new Color(0.2f, 0.2f, 0.25f, 0.6f); // Silhouette if locked
            iconImg.preserveAspect = true;

            // Title
            CreateCardText(cardGo.transform, title, new Vector2(0f, -135f), 16,
                isUnlocked ? Color.white : Color.gray);

            // Subtitle / Description
            CreateCardText(cardGo.transform, isUnlocked ? unlockedDesc : lockedDesc, new Vector2(0f, -210f), 12,
                isUnlocked ? new Color(0.75f, 0.85f, 0.95f) : new Color(0.9f, 0.5f, 0.4f));
        }

        private static Text CreateCardText(Transform parent, string text, Vector2 pos, int fontSize, Color color)
        {
            var go = new GameObject("Text");
            go.transform.SetParent(parent, false);
            var rt = go.AddComponent<RectTransform>();
            rt.anchorMin = new Vector2(0.5f, 1f);
            rt.anchorMax = new Vector2(0.5f, 1f);
            rt.pivot = new Vector2(0.5f, 1f);
            rt.anchoredPosition = pos;
            rt.sizeDelta = new Vector2(190f, 56f);

            var txt = go.AddComponent<Text>();
            txt.font = FontHelper.GetKoreanFont();
            txt.text = text;
            txt.fontSize = fontSize;
            txt.fontStyle = FontStyle.Bold;
            txt.alignment = TextAnchor.MiddleCenter;
            txt.color = color;
            txt.raycastTarget = false;
            return txt;
        }
    }
}
