using HappyShoot.Domain.Entities;

namespace HappyShoot.Domain.Progression
{
    /// <summary>
    /// Reads unlocked skill tree nodes and produces:
    /// 1) Augmented CharacterStats (core stat nodes)
    /// 2) PlayerProgressionFlags (elemental branch effects)
    /// Called once at game start after loading save data.
    /// </summary>
    public static class SkillTreeApplier
    {
        /// <summary>
        /// Applies all unlocked core stat nodes to the base character stats.
        /// Only applies nodes matching the given classType.
        /// </summary>
        public static CharacterStats ApplyStats(CharacterStats baseStats, SkillTreeManager mgr, CharacterClassType classType)
        {
            if (mgr == null) return baseStats;

            float bonusMaxHp = 0f;
            float bonusArmor = 0f;
            float bonusAtkSpeed = 0f;
            float bonusMoveSpeed = 0f;
            float bonusCritChance = 0f;
            float bonusProjSpeed = 0f;
            float bonusCdr = 0f;
            float bonusArea = 0f;
            float bonusAtkPower = 0f;

            foreach (var kvp in mgr.NodeDefs)
            {
                var def = kvp.Value;
                if (def.ClassType != classType) continue;
                if (def.Branch != BranchType.None) continue; // Only core nodes affect stats

                int level = mgr.GetNodeLevel(def.Id);
                if (level <= 0) continue;

                float value = def.EffectValue * level;

                switch (def.EffectType)
                {
                    case NodeEffectType.MaxHealth:
                        bonusMaxHp += value;
                        break;
                    case NodeEffectType.Armor:
                        bonusArmor += value;
                        break;
                    case NodeEffectType.AttackSpeed:
                        bonusAtkSpeed += value;
                        break;
                    case NodeEffectType.MoveSpeed:
                        bonusMoveSpeed += value;
                        break;
                    case NodeEffectType.CritChance:
                        bonusCritChance += value;
                        break;
                    case NodeEffectType.ProjectileSpeed:
                        bonusProjSpeed += value;
                        break;
                    case NodeEffectType.CooldownReduction:
                        bonusCdr += value;
                        break;
                    case NodeEffectType.AreaMultiplier:
                        bonusArea += value;
                        break;
                    case NodeEffectType.AttackPower:
                        bonusAtkPower += value;
                        break;
                    case NodeEffectType.ComboStatCdrArea:
                        // Mana Flow: +8% damage acts as attack power, +3% CDR
                        bonusAtkPower += value;
                        bonusCdr += 0.03f * level;
                        break;
                }
            }

            return new CharacterStats(
                maxHealth: baseStats.MaxHealth + bonusMaxHp,
                healthRegen: baseStats.HealthRegen,
                moveSpeed: baseStats.MoveSpeed * (1f + bonusMoveSpeed),
                attackPowerMultiplier: baseStats.AttackPowerMultiplier * (1f + bonusAtkPower),
                armor: baseStats.Armor + bonusArmor,
                critChance: baseStats.CritChance + bonusCritChance,
                critDamageMultiplier: baseStats.CritDamageMultiplier,
                cooldownReduction: baseStats.CooldownReduction + bonusCdr,
                areaMultiplier: baseStats.AreaMultiplier * (1f + bonusArea),
                projectileSpeedMultiplier: baseStats.ProjectileSpeedMultiplier * (1f + bonusProjSpeed),
                extraProjectiles: baseStats.ExtraProjectiles,
                pickupRadius: baseStats.PickupRadius
            );
        }

        /// <summary>
        /// Builds PlayerProgressionFlags from all unlocked branch nodes for the given class.
        /// </summary>
        public static PlayerProgressionFlags BuildFlags(SkillTreeManager mgr, CharacterClassType classType)
        {
            var flags = PlayerProgressionFlags.Empty;
            if (mgr == null) return flags;

            foreach (var kvp in mgr.NodeDefs)
            {
                var def = kvp.Value;
                if (def.ClassType != classType) continue;

                int level = mgr.GetNodeLevel(def.Id);
                if (level <= 0) continue;

                ApplyNodeToFlags(ref flags, def, level);
            }

            return flags;
        }

        private static void ApplyNodeToFlags(ref PlayerProgressionFlags f, SkillTreeNodeDef def, int level)
        {
            float v = def.EffectValue;

            switch (def.EffectType)
            {
                // ── Dodge (Ranger core) ──
                case NodeEffectType.DodgeChance:
                    f.HasDodgeChance = true;
                    f.DodgeChance = v * level;
                    break;

                // ══ Warrior Fire ══
                case NodeEffectType.FireBurnOnHit:
                    f.WFireBurnOnHit = true;
                    f.WFireBurnDuration = v;
                    break;
                case NodeEffectType.FireDeathExplosion:
                    f.WFireDeathExplosion = true;
                    f.WFireExplosionRadius = v;
                    break;
                case NodeEffectType.FireWhirlwindAura:
                    f.WFireWhirlwindAura = true;
                    break;
                case NodeEffectType.FireGroundLava:
                    f.WFireGroundLava = true;
                    break;

                // ══ Warrior Ice ══
                case NodeEffectType.IceChillOnHit:
                    f.WIceChillOnHit = true;
                    f.WIceChillSlowFactor = v;
                    break;
                case NodeEffectType.IceShatterExecute:
                    f.WIceShatterExecute = true;
                    f.WIceShatterChance = v;
                    break;
                case NodeEffectType.IceStompFreeze:
                    f.WIceStompFreeze = true;
                    f.WIceFreezeDuration = v;
                    break;
                case NodeEffectType.IceFrostCounter:
                    f.WIceFrostCounter = true;
                    f.WIceCounterChance = v;
                    break;

                // ══ Warrior Lightning ══
                case NodeEffectType.LightningShockOnHit:
                    f.WElecShockOnHit = true;
                    f.WElecShockAmplify = v;
                    break;
                case NodeEffectType.LightningStormOverload:
                    f.WElecStormOverload = true;
                    break;
                case NodeEffectType.LightningWhirlwindDischarge:
                    f.WElecWhirlwindDischarge = true;
                    break;
                case NodeEffectType.LightningThunderStrike:
                    f.WElecThunderStrike = true;
                    f.WElecBonusAttackSpeed = v;
                    break;

                // ══ Ranger Fire ══
                case NodeEffectType.FireArrowBurn:
                    f.RFireBurnOnHit = true;
                    f.RFireBurnDuration = v;
                    break;
                case NodeEffectType.FireCritExplosion:
                    f.RFireCritExplosion = true;
                    break;
                case NodeEffectType.FireMeteorRain:
                    f.RFireMeteorRain = true;
                    break;
                case NodeEffectType.FirePhoenixSummon:
                    f.RFirePhoenixSummon = true;
                    f.RFirePhoenixHitThreshold = 5;
                    break;

                // ══ Ranger Ice ══
                case NodeEffectType.IceArrowChill:
                    f.RIceChillOnHit = true;
                    f.RIceChillSlowFactor = v;
                    break;
                case NodeEffectType.IceShardBurst:
                    f.RIceShardBurst = true;
                    break;
                case NodeEffectType.IceGlaiveFrost:
                    f.RIceGlaiveFrost = true;
                    f.RIceGlaiveFreezeDuration = v;
                    break;
                case NodeEffectType.IceAutoTurret:
                    f.RIceAutoTurret = true;
                    f.RIceAutoTurretInterval = v;
                    break;

                // ══ Ranger Lightning ══
                case NodeEffectType.LightningArrowShock:
                    f.RElecShockOnHit = true;
                    f.RElecShockAmplify = v;
                    break;
                case NodeEffectType.LightningChainJump:
                    f.RElecChainJump = true;
                    f.RElecChainCount = (int)v;
                    break;
                case NodeEffectType.LightningCritThunder:
                    f.RElecCritThunder = true;
                    break;
                case NodeEffectType.LightningFullPierce:
                    f.RElecFullPierce = true;
                    break;

                // ══ Wizard Fire ══
                case NodeEffectType.FireballDotBoost:
                    f.MFireDotBoost = true;
                    f.MFireDotMultiplier = v;
                    break;
                case NodeEffectType.FireballAreaBoost:
                    f.MFireAreaBoost = true;
                    f.MFireAreaMultiplier = v;
                    break;
                case NodeEffectType.FireAutoMeteor:
                    f.MFireAutoMeteor = true;
                    f.MFireAutoMeteorInterval = v;
                    break;
                case NodeEffectType.FireChainExplosion:
                    f.MFireChainExplosion = true;
                    break;

                // ══ Wizard Ice ══
                case NodeEffectType.IceNovaSlowBoost:
                    f.MIceSlowBoost = true;
                    f.MIceSlowFactor = v;
                    break;
                case NodeEffectType.IceShardOnThaw:
                    f.MIceShardOnThaw = true;
                    break;
                case NodeEffectType.IceChanceFreeze:
                    f.MIceChanceFreeze = true;
                    f.MIceFreezeChance = v;
                    break;
                case NodeEffectType.IceFrostAura:
                    f.MIceFrostAura = true;
                    break;

                // ══ Wizard Lightning ══
                case NodeEffectType.LightningChainCountBoost:
                    f.MElecChainCountBoost = true;
                    f.MElecExtraChainCount = (int)v;
                    break;
                case NodeEffectType.LightningChainOnKill:
                    f.MElecChainOnKill = true;
                    break;
                case NodeEffectType.LightningChainOnHit:
                    f.MElecChainOnHit = true;
                    f.MElecChainOnHitChance = v;
                    break;
                case NodeEffectType.LightningShockShield:
                    f.MElecShockShield = true;
                    f.MElecShockShieldChance = v;
                    break;
            }
        }
    }
}
