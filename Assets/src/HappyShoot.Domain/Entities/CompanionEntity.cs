using System;
using System.Collections.Generic;
using HappyShoot.Domain.Spatial;

namespace HappyShoot.Domain.Entities
{
    public enum CompanionType
    {
        Warrior,
        Ranger
    }

    /// <summary>
    /// Pure domain entity representing an AI companion (Warrior or Ranger) escorting the Wizard.
    /// Synchronizes stats and skills with the player entity:
    /// - Gains companion class skills when player learns new active skills.
    /// - Levels up a random active skill when player upgrades an active skill.
    /// - Receives exactly 1/3 (0.333x) of player passive bonuses.
    /// - Deals exactly 1/3 (0.333x) final damage scale on top of sandbox base skill stats.
    /// Strictly modular and under 500 lines.
    /// </summary>
    public class CompanionEntity
    {
        public float FinalDamageScale { get; set; } = 0.3333333f; // Configurable via Sandbox
        public float PassiveScale { get; set; } = 0.3333333f;     // Configurable via Sandbox

        public CompanionType Type { get; }
        public PlayerEntity Owner { get; }
        public Vector2D Position { get; set; }

        private readonly List<CompanionSkillInstance> _skills = new List<CompanionSkillInstance>();
        public IReadOnlyList<CompanionSkillInstance> Skills => _skills;

        // Class-specific full active skill pools
        public static readonly string[] WarriorSkillPool = { "slash", "ground_stomp", "whirlwind" };
        public static readonly string[] RangerSkillPool = { "bow", "glaive", "arrow_rain" };

        public CompanionEntity(CompanionType type, PlayerEntity owner, Vector2D startPos)
        {
            Type = type;
            Owner = owner ?? throw new ArgumentNullException(nameof(owner));
            Position = startPos;

            // Initialize default starting skill (Level 1)
            string startingSkillId = (type == CompanionType.Warrior) ? "slash" : "bow";
            _skills.Add(new CompanionSkillInstance(startingSkillId, 1));
        }

        public void Update(float deltaTime)
        {
            for (int i = 0; i < _skills.Count; i++)
            {
                _skills[i].Tick(deltaTime);
            }
        }

        public CompanionSkillInstance GetSkill(string skillId)
        {
            for (int i = 0; i < _skills.Count; i++)
            {
                if (_skills[i].SkillId == skillId) return _skills[i];
            }
            return null;
        }

        public bool HasSkill(string skillId) => GetSkill(skillId) != null;

        /// <summary>
        /// Learns a random unlearned active skill for this companion's class.
        /// If all 3 skills are already learned, levels up a random non-max skill.
        /// </summary>
        public bool LearnNewSkillRandomly(Random random = null)
        {
            random ??= new Random();
            var pool = (Type == CompanionType.Warrior) ? WarriorSkillPool : RangerSkillPool;

            var unlearned = new List<string>();
            for (int i = 0; i < pool.Length; i++)
            {
                if (!HasSkill(pool[i])) unlearned.Add(pool[i]);
            }

            if (unlearned.Count > 0)
            {
                string chosenSkillId = unlearned[random.Next(unlearned.Count)];
                _skills.Add(new CompanionSkillInstance(chosenSkillId, 1));
                return true;
            }

            // Fallback: If all skills already unlocked, upgrade one of them
            return LevelUpRandomSkill(random);
        }

        /// <summary>
        /// Levels up a random active skill that is not yet at max level (Lv.5).
        /// </summary>
        public bool LevelUpRandomSkill(Random random = null)
        {
            random ??= new Random();
            var upgradeable = new List<CompanionSkillInstance>();
            for (int i = 0; i < _skills.Count; i++)
            {
                if (!_skills[i].IsMaxLevel) upgradeable.Add(_skills[i]);
            }

            if (upgradeable.Count == 0) return false;

            var chosen = upgradeable[random.Next(upgradeable.Count)];
            return chosen.LevelUp();
        }

        // ==========================================
        // 1/3 Scaled Passive Stat Helpers
        // ==========================================

        /// <summary>
        /// Gets the effective Attack Power Multiplier with 1/3 passive effect.
        /// </summary>
        public float GetEffectiveAttackPowerMultiplier()
        {
            if (Owner == null) return 1.0f;
            float ownerBonusAp = Owner.Stats.AttackPowerMultiplier - 1.0f;
            return Math.Max(0.2f, 1.0f + (ownerBonusAp * PassiveScale));
        }

        /// <summary>
        /// Gets the effective Cooldown Reduction with passive scale effect.
        /// </summary>
        public float GetEffectiveCooldownReduction()
        {
            if (Owner == null) return 0f;
            float ownerCdr = Owner.Stats.CooldownReduction;
            return Math.Min(0.75f, Math.Max(0f, ownerCdr * PassiveScale));
        }

        /// <summary>
        /// Gets the effective Area Multiplier with passive scale effect.
        /// </summary>
        public float GetEffectiveAreaMultiplier()
        {
            if (Owner == null) return 1.0f;
            float ownerBonusArea = Owner.Stats.AreaMultiplier - 1.0f;
            return Math.Max(0.5f, 1.0f + (ownerBonusArea * PassiveScale));
        }

        /// <summary>
        /// Calculates final damage: [Sandbox Base Damage] * [Effective AP (passive scale)] * [FinalDamageScale].
        /// </summary>
        public float CalculateFinalDamage(float baseDamage)
        {
            float ap = GetEffectiveAttackPowerMultiplier();
            return baseDamage * ap * FinalDamageScale;
        }

        /// <summary>
        /// Calculates the final cooldown for a skill: [Sandbox Base Cooldown] * (1 - Effective CDR).
        /// </summary>
        public float CalculateEffectiveCooldown(float baseCooldown)
        {
            float cdr = GetEffectiveCooldownReduction();
            return Math.Max(0.2f, baseCooldown * (1f - cdr));
        }
    }
}
