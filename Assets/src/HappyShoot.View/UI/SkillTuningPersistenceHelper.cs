using UnityEngine;
using HappyShoot.Domain.Entities;
using HappyShoot.Domain.Leveling;
using HappyShoot.Domain.Gems;
using HappyShoot.Domain.Skills;
using HappyShoot.View.Config;

namespace HappyShoot.View.UI
{
    /// <summary>
    /// Helper responsible for Save, Restore, and Reset persistence operations for Sandbox Tuning.
    /// Strictly modular and under 500 lines.
    /// </summary>
    public static class SkillTuningPersistenceHelper
    {
        public static bool SaveConfig(PlayerEntity player, string selectedSkillId, SkillConfigData config)
        {
            if (player != null)
            {
                if (selectedSkillId != "exp_tuning" && selectedSkillId != "monster_tuning" && selectedSkillId != "crit_tuning")
                {
                    var activeSkill = player.GetSkill(selectedSkillId);
                    if (activeSkill != null && activeSkill.Level == 1)
                        SkillLiveApplier.PullSkillStatsToConfig(player, selectedSkillId, config);
                }

                if (config.CritStat == null) config.CritStat = new CritStatConfig();
                var pStat = player.Stats;
                config.CritStat.CritChance = pStat.CritChance;
                config.CritStat.CritDamageMultiplier = pStat.CritDamageMultiplier;
                config.CritStat.AttackPowerMultiplier = pStat.AttackPowerMultiplier;
                config.CritStat.MoveSpeed = pStat.MoveSpeed;
                config.CritStat.Armor = pStat.Armor;
                config.CritStat.CooldownReduction = pStat.CooldownReduction;
                config.CritStat.IsCustom = true;
            }

            SkillTuningMemoryCache.ExportToConfig(config);
            return SkillConfigRepository.Instance.Save(config);
        }

        public static SkillConfigData ResetConfig(PlayerEntity player, LevelSystem levelSystem, GemManager gemManager)
        {
            var config = SkillConfigRepository.Instance.ReloadFromFileOrDefaults();
            SkillTuningMemoryCache.ImportFromConfig(config);

            if (levelSystem != null) levelSystem.Config = config.Exp;
            if (gemManager != null) gemManager.Config = config.Exp;

            if (config.CritStat != null && config.CritStat.IsCustom && player != null)
            {
                var s = player.Stats;
                var c = config.CritStat;
                player.Stats = new CharacterStats(s.MaxHealth, s.HealthRegen, c.MoveSpeed, c.AttackPowerMultiplier, c.Armor, c.CritChance, c.CritDamageMultiplier, c.CooldownReduction, s.AreaMultiplier, s.ProjectileSpeedMultiplier, s.ExtraProjectiles, s.PickupRadius);
            }

            return config;
        }
    }
}
