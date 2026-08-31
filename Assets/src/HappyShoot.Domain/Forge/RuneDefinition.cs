using System;

namespace HappyShoot.Domain.Forge
{
    /// <summary>Rune quality grade, determines unlock cost and power tier.</summary>
    public enum RuneGrade
    {
        Common = 0,    // 🟢 Green border
        Rare = 1,      // 🔵 Blue border
        Legendary = 2  // 🟣 Purple border
    }

    /// <summary>
    /// Immutable definition for a single rune type.
    /// 12 rune types total: 4 Common + 4 Rare + 4 Legendary.
    /// </summary>
    public sealed class RuneDefinition
    {
        public readonly string Id;
        public readonly string Name;
        public readonly string Description;
        public readonly RuneGrade Grade;

        // ── Unlock cost (one-time, in gems) ──
        public readonly int UnlockRubyCost;
        public readonly int UnlockEmeraldCost;
        public readonly int UnlockAmethystCost;

        // ── Base modifiers at Lv.1 ──
        public readonly float BaseDamageMultiplier;
        public readonly float BaseCooldownMultiplier;
        public readonly float BaseAreaMultiplier;

        // ── Per-level increments ──
        public readonly float DamagePerLevel;     // +0.01 = +1%p per level
        public readonly float CooldownPerLevel;   // -0.01 = cooldown shrinks per level
        public readonly float AreaPerLevel;        // +0.01 = area grows per level

        // ── Special effect base values (0 = not applicable) ──
        public readonly int BaseExtraProjectiles;
        public readonly int BaseExtraPierceCount;
        public readonly float BaseLifeStealPercent;
        public readonly float BaseChainChance;
        public readonly float BaseDeathExplosionPercent;
        public readonly int FreecastEveryN;
        public readonly float ResonanceMultiplier;
        public readonly float ChaosMinMult;
        public readonly float ChaosMaxMult;

        // ── Primary gem type for upgrade cost ──
        public readonly Progression.GemType PrimaryGem;

        public RuneDefinition(
            string id, string name, string description, RuneGrade grade,
            int unlockRuby, int unlockEmerald, int unlockAmethyst,
            float baseDmg, float baseCd, float baseArea,
            float dmgPerLv, float cdPerLv, float areaPerLv,
            Progression.GemType primaryGem,
            int extraProj = 0, int extraPierce = 0,
            float lifeSteal = 0f, float chainChance = 0f,
            float deathExplosion = 0f, int freecastN = 0,
            float resonance = 0f, float chaosMin = 0f, float chaosMax = 0f)
        {
            Id = id;
            Name = name;
            Description = description;
            Grade = grade;
            UnlockRubyCost = unlockRuby;
            UnlockEmeraldCost = unlockEmerald;
            UnlockAmethystCost = unlockAmethyst;
            BaseDamageMultiplier = baseDmg;
            BaseCooldownMultiplier = baseCd;
            BaseAreaMultiplier = baseArea;
            DamagePerLevel = dmgPerLv;
            CooldownPerLevel = cdPerLv;
            AreaPerLevel = areaPerLv;
            PrimaryGem = primaryGem;
            BaseExtraProjectiles = extraProj;
            BaseExtraPierceCount = extraPierce;
            BaseLifeStealPercent = lifeSteal;
            BaseChainChance = chainChance;
            BaseDeathExplosionPercent = deathExplosion;
            FreecastEveryN = freecastN;
            ResonanceMultiplier = resonance;
            ChaosMinMult = chaosMin;
            ChaosMaxMult = chaosMax;
        }

        /// <summary>
        /// Calculate the gem cost to upgrade from current level to next.
        /// Formula: floor(level * 1.1 + 1)
        /// </summary>
        public int GetUpgradeCost(int currentLevel)
        {
            return (int)Math.Floor(currentLevel * 1.1 + 1);
        }

        /// <summary>
        /// Build RuneModifiers for a given rune level.
        /// Cooldown multiplier is clamped to minimum 0.25 (75% CDR cap).
        /// </summary>
        public RuneModifiers CalculateModifiers(int level)
        {
            if (level <= 0) return RuneModifiers.None;

            int bonusLevels = level - 1; // Lv.1 = base, Lv.2 = base + 1 bonus
            float dmgMult = BaseDamageMultiplier + DamagePerLevel * bonusLevels;
            float cdMult = BaseCooldownMultiplier + CooldownPerLevel * bonusLevels;
            float areaMult = BaseAreaMultiplier + AreaPerLevel * bonusLevels;

            // Clamp cooldown multiplier: minimum 0.25 (75% CDR cap)
            if (cdMult < 0.25f) cdMult = 0.25f;

            return new RuneModifiers
            {
                RuneId = Id,
                RuneLevel = level,
                DamageMultiplier = dmgMult,
                CooldownMultiplier = cdMult,
                AreaMultiplier = areaMult,
                ExtraProjectiles = BaseExtraProjectiles,
                ExtraPierceCount = BaseExtraPierceCount,
                LifeStealPercent = BaseLifeStealPercent,
                ChainChance = BaseChainChance,
                DeathExplosionPercent = BaseDeathExplosionPercent,
                FreecastEveryN = FreecastEveryN,
                ResonanceMultiplier = ResonanceMultiplier,
                ChaosMinMult = ChaosMinMult,
                ChaosMaxMult = ChaosMaxMult
            };
        }
    }
}
