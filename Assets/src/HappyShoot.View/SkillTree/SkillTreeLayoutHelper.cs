using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using HappyShoot.Domain.Entities;
using HappyShoot.Domain.Progression;
using HappyShoot.View.Utils;

namespace HappyShoot.View.SkillTree
{
    /// <summary>
    /// Computes organic celestial constellation coordinates (3 classes × 120° sectors)
    /// on concentric orbital rings, and renders glowing constellation paths and sector dividers.
    /// Strictly modular and under 500 lines.
    /// </summary>
    public static class SkillTreeLayoutHelper
    {
        // ── Sector Center Angles ──
        public const float WarriorBaseAngle = 90f;   // Top (30° ~ 150°)
        public const float RangerBaseAngle = 330f;   // Bottom-Right (270° ~ 30°)
        public const float WizardBaseAngle = 210f;   // Bottom-Left (150° ~ 270°)

        /// <summary>
        /// Maps each node to its organic constellation position on circular orbital rings.
        /// </summary>
        public static Vector2 GetNodePosition(SkillTreeNodeDef def)
        {
            float baseAngle = GetClassBaseAngle(def.ClassType);

            // 1. Core Stat Nodes (Inner Rings: r = 95, 160, 225)
            if (def.Branch == BranchType.None)
            {
                return GetCoreRadialPosition(def.Id, baseAngle);
            }

            // 2. Elemental Branches (Outer Rings: r = 290, 350, 410, 470)
            float branchAngleOffset = 0f;
            switch (def.Branch)
            {
                case BranchType.Fire:      branchAngleOffset = -26f; break; // Left wing of sector
                case BranchType.Ice:       branchAngleOffset = 0f;   break; // Center spire
                case BranchType.Lightning: branchAngleOffset = 26f;  break; // Right wing of sector
            }

            float finalAngle = baseAngle + branchAngleOffset;
            int step = GetBranchStep(def.Id); // 0, 1, 2, 3
            float radius = 290f + (step * 60f);

            return PolarToCartesian(radius, finalAngle);
        }

        public static float GetClassBaseAngle(CharacterClassType classType)
        {
            switch (classType)
            {
                case CharacterClassType.Warrior: return WarriorBaseAngle;
                case CharacterClassType.Ranger:  return RangerBaseAngle;
                case CharacterClassType.Wizard:  return WizardBaseAngle;
                default: return WarriorBaseAngle;
            }
        }

        private static Vector2 GetCoreRadialPosition(string id, float baseAngle)
        {
            float angleOffset = 0f;
            float radius = 95f;

            // Tier 1 (r = 95) - Dual Star Gates
            if (id == "w_hp1" || id == "r_spd1" || id == "m_cdr1")
            {
                angleOffset = -11f;
                radius = 95f;
            }
            else if (id == "w_armor1" || id == "r_crit1" || id == "m_area1")
            {
                angleOffset = 11f;
                radius = 95f;
            }
            // Tier 2 (r = 160) - Expanded Web
            else if (id == "w_hp2" || id == "r_spd2" || id == "m_cdr2")
            {
                angleOffset = -16f;
                radius = 160f;
            }
            else if (id == "w_armor2" || id == "r_crit2" || id == "m_area2")
            {
                angleOffset = 16f;
                radius = 160f;
            }
            // Tier 3 (r = 225) - Pre-Awakening Trident Base
            else if (id == "w_hp3" || id == "r_projspd" || id == "m_mana")
            {
                angleOffset = -18f;
                radius = 225f;
            }
            else if (id == "w_atkspd" || id == "r_dodge" || id == "m_ap1")
            {
                angleOffset = 18f;
                radius = 225f;
            }

            return PolarToCartesian(radius, baseAngle + angleOffset);
        }

        private static int GetBranchStep(string id)
        {
            if (id.EndsWith("1")) return 0;
            if (id.EndsWith("2")) return 1;
            if (id.EndsWith("3")) return 2;
            if (id.EndsWith("4")) return 3;
            return 0;
        }

        public static Vector2 PolarToCartesian(float radius, float angleDeg)
        {
            float rad = angleDeg * Mathf.Deg2Rad;
            return new Vector2(radius * Mathf.Cos(rad), radius * Mathf.Sin(rad));
        }

        /// <summary>
        /// Creates a connecting line between two nodes in the radial constellation.
        /// </summary>
        public static GameObject CreateConnectionLine(
            Transform parent,
            Vector2 fromPos,
            Vector2 toPos,
            bool isUnlocked,
            bool isBlocked)
        {
            var lineGo = new GameObject("TreeLine");
            lineGo.transform.SetParent(parent, false);

            var img = lineGo.AddComponent<Image>();
            img.sprite = SpriteHelper.GetOrCreateWhiteSprite();
            img.raycastTarget = false;

            Color lineColor;
            if (isBlocked)
                lineColor = new Color(0.40f, 0.12f, 0.12f, 0.35f);
            else if (isUnlocked)
                lineColor = new Color(1.0f, 0.85f, 0.25f, 0.95f); // Radiant Gold Laser
            else
                lineColor = new Color(0.25f, 0.32f, 0.45f, 0.60f); // Engraved silver groove

            img.color = lineColor;

            var rt = (RectTransform)lineGo.transform;
            rt.anchorMin = new Vector2(0.5f, 0.5f);
            rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.pivot = new Vector2(0.5f, 0.5f);

            Vector2 diff = toPos - fromPos;
            float distance = diff.magnitude;
            float angle = Mathf.Atan2(diff.y, diff.x) * Mathf.Rad2Deg;

            rt.sizeDelta = new Vector2(distance, isUnlocked ? 3.5f : 2.0f);
            rt.anchoredPosition = fromPos + (diff * 0.5f);
            rt.localRotation = Quaternion.Euler(0, 0, angle);

            return lineGo;
        }

        /// <summary>
        /// Creates the 120-degree divider rays separating the 3 class sectors (at 30°, 150°, 270°).
        /// </summary>
        public static void CreateSectorDividers(Transform parent)
        {
            float[] dividerAngles = new float[] { 30f, 150f, 270f };
            for (int i = 0; i < dividerAngles.Length; i++)
            {
                float ang = dividerAngles[i];
                var divGo = new GameObject($"SectorDivider_{i}");
                divGo.transform.SetParent(parent, false);

                var img = divGo.AddComponent<Image>();
                img.sprite = SpriteHelper.GetOrCreateWhiteSprite();
                img.color = new Color(0.30f, 0.38f, 0.55f, 0.30f);
                img.raycastTarget = false;

                var rt = (RectTransform)divGo.transform;
                rt.anchorMin = new Vector2(0.5f, 0.5f);
                rt.anchorMax = new Vector2(0.5f, 0.5f);
                rt.pivot = new Vector2(0f, 0.5f);
                rt.sizeDelta = new Vector2(500f, 1.5f);
                rt.anchoredPosition = Vector2.zero;
                rt.localRotation = Quaternion.Euler(0, 0, ang);
            }
        }
    }
}
