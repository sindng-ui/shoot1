namespace HappyShoot.Domain.Forge
{
    /// <summary>
    /// Data injected into SkillContext when a rune is equipped on a skill slot.
    /// Zero-allocation struct — no heap pressure at runtime.
    /// </summary>
    public struct RuneModifiers
    {
        public string RuneId;
        public int RuneLevel;

        // ── Core multipliers (applied to every rune) ──
        public float DamageMultiplier;      // 1.0 = unchanged, 1.2 = +20%
        public float CooldownMultiplier;    // 1.0 = unchanged, 0.85 = -15%
        public float AreaMultiplier;        // 1.0 = unchanged, 1.25 = +25%

        // ── Special modifiers (0 = inactive) ──
        public int ExtraProjectiles;        // Split rune: extra projectile count
        public int ExtraPierceCount;        // Pierce rune: extra pierce count
        public float LifeStealPercent;      // Leech rune: 0.01 = 1% HP restore on hit
        public float ChainChance;           // Chain rune: 0.25 = 25% chain to nearby
        public float DeathExplosionPercent; // Detonate rune: 0.50 = 50% dmg explosion on kill
        public int FreecastEveryN;          // Tempo rune: 5 = free cast every 5th use
        public float ResonanceMultiplier;   // Resonance rune: 1.5x when 2+ same rune equipped
        public float ChaosMinMult;          // Chaos rune: minimum random multiplier
        public float ChaosMaxMult;          // Chaos rune: maximum random multiplier

        /// <summary>True when a rune is actually equipped (RuneId is set).</summary>
        public bool IsActive => !string.IsNullOrEmpty(RuneId);

        /// <summary>Identity/no-op modifiers (no rune equipped).</summary>
        public static RuneModifiers None => new RuneModifiers
        {
            RuneId = null,
            RuneLevel = 0,
            DamageMultiplier = 1f,
            CooldownMultiplier = 1f,
            AreaMultiplier = 1f
        };

        /// <summary>
        /// Applies damage modifier, factoring in chaos randomness if active.
        /// </summary>
        public float ApplyDamage(float baseDamage, System.Random random = null)
        {
            float mult = DamageMultiplier;
            if (ChaosMinMult > 0f && ChaosMaxMult > 0f && random != null)
            {
                float range = ChaosMaxMult - ChaosMinMult;
                mult *= ChaosMinMult + (float)random.NextDouble() * range;
            }
            return baseDamage * mult;
        }

        /// <summary>
        /// Applies area modifier, factoring in chaos randomness if active.
        /// </summary>
        public float ApplyArea(float baseArea, System.Random random = null)
        {
            float mult = AreaMultiplier;
            if (ChaosMinMult > 0f && ChaosMaxMult > 0f && random != null)
            {
                float range = ChaosMaxMult - ChaosMinMult;
                mult *= ChaosMinMult + (float)random.NextDouble() * range;
            }
            return baseArea * mult;
        }
    }
}
