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
        /// Maps each Wizard node to its organic constellation position on circular orbital rings.
        /// Core nodes form a central orbital web (r = 110, 180), and 3 elemental branches radiate outward at 90°, 210°, 330°.
        /// </summary>
        public static Vector2 GetNodePosition(SkillTreeNodeDef def)
        {
            // 1. Core Stat Nodes (Central Mana Ring: r = 110, 180)
            if (def.Branch == BranchType.None)
            {
                return GetWizardCorePosition(def.Id);
            }

            // 2. Elemental Branches (Radiating outward at 120° intervals)
            float baseAngle = 90f; // Default Fire: Top
            switch (def.Branch)
            {
                case BranchType.Fire:      baseAngle = 90f;  break; // Top (🔥 Fire)
                case BranchType.Ice:       baseAngle = 210f; break; // Bottom-Left (❄️ Ice)
                case BranchType.Lightning: baseAngle = 330f; break; // Bottom-Right (⚡ Lightning)
            }

            int step = GetBranchStep(def.Id); // 0, 1, 2, 3
            float radius = 250f + (step * 70f); // r = 250, 320, 390, 460

            return PolarToCartesian(radius, baseAngle);
        }

        private static Vector2 GetWizardCorePosition(string id)
        {
            // Tier 1 inner ring (r = 110)
            if (id == "m_cdr1") return PolarToCartesian(110f, 90f);
            if (id == "m_area1") return PolarToCartesian(110f, 210f);
            if (id == "m_ap1") return PolarToCartesian(110f, 330f);

            // Tier 2 outer core ring (r = 180)
            if (id == "m_cdr2") return PolarToCartesian(180f, 90f);
            if (id == "m_area2") return PolarToCartesian(180f, 210f);
            if (id == "m_mana") return PolarToCartesian(180f, 330f);

            return Vector2.zero;
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
