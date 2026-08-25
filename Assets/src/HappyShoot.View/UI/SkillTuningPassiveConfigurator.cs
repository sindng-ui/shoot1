using System;
using HappyShoot.Domain.Entities;
using HappyShoot.Domain.Skills;

namespace HappyShoot.View.UI
{
    /// <summary>
    /// Configurator for 9 Passive Skills sandbox tuning sliders.
    /// Strictly modular and under 500 lines.
    /// </summary>
    public static class SkillTuningPassiveConfigurator
    {
        public static void ConfigurePassiveRows(
            string skillId,
            PlayerEntity player,
            SkillConfigData config,
            Action<string, float, float, float, float, Action<float>, bool> addRow)
        {
            if (config == null) return;
            if (config.Passives == null) config.Passives = new PassiveConfigData();
            var p = config.Passives;

            void Sync() => SkillLiveApplier.ApplyPassivesLive(player, config);

            switch (skillId)
            {
                case "passive_fang":
                    addRow("⚔️ 레벨당 공격력 증가 (%: 1~50%)", p.FangAttackPowerPercent, 1f, 50f, 1f, v => { p.FangAttackPowerPercent = v; Sync(); }, true);
                    break;

                case "passive_feather":
                    addRow("🏃 레벨당 이동속도 (m/s: 0.1~2.0)", p.FeatherMoveSpeed, 0.1f, 2.0f, 0.05f, v => { p.FeatherMoveSpeed = v; Sync(); }, false);
                    addRow("🚀 레벨당 투사체 속도 (%: 1~50%)", p.FeatherProjSpeedPercent, 1f, 50f, 1f, v => { p.FeatherProjSpeedPercent = v; Sync(); }, true);
                    break;

                case "passive_rune":
                    addRow("⏱️ 레벨당 쿨타임 감소 (%: 1~25%)", p.RuneCooldownReductionPercent, 1f, 25f, 1f, v => { p.RuneCooldownReductionPercent = v; Sync(); }, true);
                    addRow("📏 레벨당 공격 범위 (%: 1~50%)", p.RuneAreaMultiplierPercent, 1f, 50f, 1f, v => { p.RuneAreaMultiplierPercent = v; Sync(); }, true);
                    break;

                case "passive_armor":
                    addRow("🛡️ 레벨당 방어력 증가 (1~25)", p.ArmorAmount, 1f, 25f, 1f, v => { p.ArmorAmount = v; Sync(); }, true);
                    break;

                case "passive_ring":
                    addRow("🧲 레벨당 자석 흡수 반경 (m: 0.5~5.0)", p.RingPickupRadius, 0.5f, 5.0f, 0.1f, v => { p.RingPickupRadius = v; Sync(); }, false);
                    break;

                case "passive_heart":
                    addRow("❤️ 레벨당 최대 체력 (HP: 5~100)", p.HeartMaxHp, 5f, 100f, 5f, v => { p.HeartMaxHp = v; Sync(); }, true);
                    addRow("💖 레벨당 초당 체력 재생 (HP/s: 0.2~10.0)", p.HeartHpRegen, 0.2f, 10.0f, 0.2f, v => { p.HeartHpRegen = v; Sync(); }, false);
                    break;

                case "passive_ignition":
                    addRow("🔥 레벨당 공격력 증가 (%: 1~50%)", p.IgnitionAttackPowerPercent, 1f, 50f, 1f, v => { p.IgnitionAttackPowerPercent = v; Sync(); }, true);
                    break;

                case "passive_overcharge":
                    addRow("⚡ 레벨당 쿨타임 감소 (%: 1~20%)", p.OverchargeCooldownReductionPercent, 1f, 20f, 1f, v => { p.OverchargeCooldownReductionPercent = v; Sync(); }, true);
                    break;

                case "passive_crit":
                    addRow("🎯 레벨당 크리티컬 확률 (%: 1~25%)", p.CritEyeChancePercent, 1f, 25f, 1f, v => { p.CritEyeChancePercent = v; Sync(); }, true);
                    addRow("💥 레벨당 크리티컬 데미지 배율 (%: 1~30%)", p.CritEyeDamageMultiplierPercent, 1f, 30f, 1f, v => { p.CritEyeDamageMultiplierPercent = v; Sync(); }, true);
                    break;
            }
        }
    }
}
