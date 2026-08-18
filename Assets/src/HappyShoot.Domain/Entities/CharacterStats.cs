using System;

namespace HappyShoot.Domain.Entities
{
    /// <summary>
    /// Lightweight, zero-allocation value type containing all combat and movement statistics.
    /// </summary>
    public readonly struct CharacterStats : IEquatable<CharacterStats>
    {
        public readonly float MaxHealth;
        public readonly float HealthRegen;
        public readonly float MoveSpeed;
        public readonly float AttackPowerMultiplier;
        public readonly float Armor;
        public readonly float CritChance;
        public readonly float CritDamageMultiplier;
        public readonly float CooldownReduction;
        public readonly float AreaMultiplier;
        public readonly float ProjectileSpeedMultiplier;
        public readonly int ExtraProjectiles;
        public readonly float PickupRadius;

        public static readonly CharacterStats Default = new CharacterStats(
            maxHealth: 100f,
            healthRegen: 0f,
            moveSpeed: 5.0f,
            attackPowerMultiplier: 1.0f,
            armor: 0f,
            critChance: 0.05f,
            critDamageMultiplier: 1.5f,
            cooldownReduction: 0f,
            areaMultiplier: 1.0f,
            projectileSpeedMultiplier: 1.0f,
            extraProjectiles: 0,
            pickupRadius: 2.0f
        );

        public CharacterStats(
            float maxHealth,
            float healthRegen,
            float moveSpeed,
            float attackPowerMultiplier,
            float armor,
            float critChance,
            float critDamageMultiplier,
            float cooldownReduction,
            float areaMultiplier,
            float projectileSpeedMultiplier,
            int extraProjectiles,
            float pickupRadius)
        {
            MaxHealth = Math.Max(1f, maxHealth);
            HealthRegen = Math.Max(0f, healthRegen);
            MoveSpeed = Math.Max(0.1f, moveSpeed);
            AttackPowerMultiplier = Math.Max(0.1f, attackPowerMultiplier);
            Armor = Math.Max(0f, armor);
            CritChance = Math.Max(0f, Math.Min(1f, critChance));
            CritDamageMultiplier = Math.Max(1.0f, critDamageMultiplier);
            CooldownReduction = Math.Max(0f, Math.Min(0.75f, cooldownReduction)); // Max 75% CDR
            AreaMultiplier = Math.Max(0.1f, areaMultiplier);
            ProjectileSpeedMultiplier = Math.Max(0.1f, projectileSpeedMultiplier);
            ExtraProjectiles = Math.Max(0, extraProjectiles);
            PickupRadius = Math.Max(0.5f, pickupRadius);
        }

        /// <summary>
        /// Calculates mitigated damage based on Armor using standard formula: Raw * (100 / (100 + Armor)).
        /// </summary>
        public float CalculateMitigatedDamage(float rawDamage)
        {
            if (rawDamage <= 0f) return 0f;
            return rawDamage * (100f / (100f + Armor));
        }

        public static CharacterStats operator +(CharacterStats a, CharacterStats b)
        {
            return new CharacterStats(
                maxHealth: a.MaxHealth + b.MaxHealth - Default.MaxHealth,
                healthRegen: a.HealthRegen + b.HealthRegen,
                moveSpeed: a.MoveSpeed + b.MoveSpeed - Default.MoveSpeed,
                attackPowerMultiplier: a.AttackPowerMultiplier + b.AttackPowerMultiplier - 1.0f,
                armor: a.Armor + b.Armor,
                critChance: a.CritChance + b.CritChance,
                critDamageMultiplier: a.CritDamageMultiplier + b.CritDamageMultiplier - 1.5f,
                cooldownReduction: a.CooldownReduction + b.CooldownReduction,
                areaMultiplier: a.AreaMultiplier + b.AreaMultiplier - 1.0f,
                projectileSpeedMultiplier: a.ProjectileSpeedMultiplier + b.ProjectileSpeedMultiplier - 1.0f,
                extraProjectiles: a.ExtraProjectiles + b.ExtraProjectiles,
                pickupRadius: a.PickupRadius + b.PickupRadius - Default.PickupRadius
            );
        }

        public bool Equals(CharacterStats other)
        {
            return Math.Abs(MaxHealth - other.MaxHealth) < 1e-4f &&
                   Math.Abs(MoveSpeed - other.MoveSpeed) < 1e-4f &&
                   Math.Abs(Armor - other.Armor) < 1e-4f &&
                   Math.Abs(CritChance - other.CritChance) < 1e-4f;
        }

        public override bool Equals(object obj) => obj is CharacterStats other && Equals(other);
        public override int GetHashCode()
        {
            unchecked
            {
                int hash = 17;
                hash = hash * 23 + MaxHealth.GetHashCode();
                hash = hash * 23 + MoveSpeed.GetHashCode();
                hash = hash * 23 + Armor.GetHashCode();
                hash = hash * 23 + CritChance.GetHashCode();
                return hash;
            }
        }
    }
}
