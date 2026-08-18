using HappyShoot.Domain.Entities;

namespace HappyShoot.Domain.Meta
{
    /// <summary>
    /// Computes augmented CharacterStats by combining base stats with permanent meta upgrades.
    /// </summary>
    public static class MetaUpgradeApplier
    {
        public const string UpgradeHealth = "meta_health";
        public const string UpgradeRegen = "meta_regen";
        public const string UpgradeArmor = "meta_armor";
        public const string UpgradeDamage = "meta_damage";
        public const string UpgradeCritChance = "meta_crit";
        public const string UpgradeSpeed = "meta_speed";
        public const string UpgradeMagnet = "meta_magnet";
        public const string UpgradeExtraProjectile = "meta_projectile";

        public static CharacterStats ApplyUpgrades(CharacterStats baseStats, MetaUpgradeSaveData saveData)
        {
            if (saveData == null) return baseStats;

            int hpLvl = saveData.GetLevel(UpgradeHealth);
            int regenLvl = saveData.GetLevel(UpgradeRegen);
            int armorLvl = saveData.GetLevel(UpgradeArmor);
            int dmgLvl = saveData.GetLevel(UpgradeDamage);
            int critLvl = saveData.GetLevel(UpgradeCritChance);
            int speedLvl = saveData.GetLevel(UpgradeSpeed);
            int magnetLvl = saveData.GetLevel(UpgradeMagnet);
            int projLvl = saveData.GetLevel(UpgradeExtraProjectile);

            return new CharacterStats(
                maxHealth: baseStats.MaxHealth + (hpLvl * 10f),
                healthRegen: baseStats.HealthRegen + (regenLvl * 0.2f),
                moveSpeed: baseStats.MoveSpeed * (1.0f + (speedLvl * 0.05f)),
                attackPowerMultiplier: baseStats.AttackPowerMultiplier * (1.0f + (dmgLvl * 0.05f)),
                armor: baseStats.Armor + (armorLvl * 2.0f),
                critChance: baseStats.CritChance + (critLvl * 0.02f),
                critDamageMultiplier: baseStats.CritDamageMultiplier,
                cooldownReduction: baseStats.CooldownReduction,
                areaMultiplier: baseStats.AreaMultiplier,
                projectileSpeedMultiplier: baseStats.ProjectileSpeedMultiplier,
                extraProjectiles: baseStats.ExtraProjectiles + projLvl,
                pickupRadius: baseStats.PickupRadius + (magnetLvl * 0.5f)
            );
        }
    }
}
