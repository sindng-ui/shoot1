using UnityEngine;
using HappyShoot.Domain.Events;

namespace HappyShoot.View.UI
{
    /// <summary>
    /// Provides vibrant, high-contrast graphic color definitions for floating damage numbers.
    /// Distinguishes elemental sources: Fire (Orange), Ice (Cyan), Lightning (Electric Yellow), Default (White),
    /// with golden highlight for critical strikes.
    /// </summary>
    public static class DamageColorPalette
    {
        // 1. Normal (Default / Physical / Piercing Arrow / Blade)
        public static readonly Color Normal = new Color(1.0f, 1.0f, 1.0f, 1.0f); // #FFFFFF Pure Crisp White

        // 2. Fire & Burn Dot (Fiery Magma Orange)
        public static readonly Color Fire = new Color(1.0f, 0.45f, 0.10f, 1.0f); // #FF731A Vibrant Fire Orange

        // 3. Ice & Frost Nova (Glacial Cyan / Frost Sky Blue)
        public static readonly Color Ice = new Color(0.15f, 0.88f, 1.0f, 1.0f); // #26E0FF Bright Glacial Cyan

        // 4. Lightning & Shock Dot (Neon Zapping Electric Yellow)
        public static readonly Color Lightning = new Color(1.0f, 0.95f, 0.15f, 1.0f); // #FFF226 Neon Electric Yellow

        // 5. Critical Strike Highlight (Golden Amber / Intense Flare)
        public static readonly Color CriticalDefault = new Color(1.0f, 0.85f, 0.12f, 1.0f); // #FFD91F Radiant Gold
        public static readonly Color CriticalFire = new Color(1.0f, 0.32f, 0.05f, 1.0f);    // Deep Fiery Crimson-Orange
        public static readonly Color CriticalIce = new Color(0.40f, 0.95f, 1.0f, 1.0f);     // Diamond Ice White-Cyan
        public static readonly Color CriticalLightning = new Color(1.0f, 1.0f, 0.30f, 1.0f); // Ultra Neon Spark Yellow

        /// <summary>
        /// Resolves the optimal graphic text color based on damage type and critical status.
        /// </summary>
        public static Color GetColor(DamageType damageType, bool isCritical)
        {
            if (isCritical)
            {
                switch (damageType)
                {
                    case DamageType.Fireball:
                    case DamageType.BurnDot:
                        return CriticalFire;

                    case DamageType.Ice:
                        return CriticalIce;

                    case DamageType.Lightning:
                    case DamageType.ShockDot:
                        return CriticalLightning;

                    default:
                        return CriticalDefault;
                }
            }

            switch (damageType)
            {
                case DamageType.Fireball:
                case DamageType.BurnDot:
                    return Fire;

                case DamageType.Ice:
                    return Ice;

                case DamageType.Lightning:
                case DamageType.ShockDot:
                    return Lightning;

                default:
                    return Normal;
            }
        }
    }
}
