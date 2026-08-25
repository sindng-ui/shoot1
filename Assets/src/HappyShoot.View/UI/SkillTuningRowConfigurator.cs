using System;
using UnityEngine;
using HappyShoot.Domain.Entities;
using HappyShoot.Domain.Gems;
using HappyShoot.Domain.Leveling;
using HappyShoot.Domain.Skills;
using HappyShoot.Domain.Skills.Effects;
using HappyShoot.Domain.Skills.Triggers;

namespace HappyShoot.View.UI
{
    /// <summary>
    /// Configurator that sets up parameter slider rows directly bound to active skill instance, EXP tuning,
    /// monster stats tuning, and per-skill camera shake toggles.
    /// Strictly modular and under 500 lines.
    /// </summary>
    public static class SkillTuningRowConfigurator
    {
        public static void ConfigureRows(
            PlayerEntity player,
            string skillId,
            Transform container,
            bool isInfiniteSpam,
            SkillConfigData config,
            LevelSystem levelSystem,
            GemManager gemManager,
            Action<GameObject> onRowCreated)
        {
            if (container == null || string.IsNullOrEmpty(skillId)) return;

            float yOffset = 0f;

            void AddRow(string title, float curVal, float min, float max, float step, Action<float> onChanged, bool isInt = false)
            {
                var row = SkillTuningSliderFactory.CreateSliderRow(container, title, curVal, min, max, step, onChanged, ref yOffset, isInt);
                onRowCreated?.Invoke(row);
            }

            void AddShakeRow(string title, bool curVal, Action<bool> onToggle)
            {
                AddRow(title, curVal ? 1f : 0f, 0f, 1f, 1f, v => onToggle?.Invoke(v >= 0.5f), isInt: true);
            }

            if (skillId == "crit_tuning")
            {
                if (player == null) return;
                if (config != null && config.CritStat == null) config.CritStat = new HappyShoot.Domain.Skills.CritStatConfig();

                var cStat = config?.CritStat;
                if (cStat != null && cStat.IsCustom)
                {
                    var sInit = player.Stats;
                    player.Stats = new CharacterStats(sInit.MaxHealth, sInit.HealthRegen, cStat.MoveSpeed, cStat.AttackPowerMultiplier, cStat.Armor, cStat.CritChance, cStat.CritDamageMultiplier, cStat.CooldownReduction, sInit.AreaMultiplier, sInit.ProjectileSpeedMultiplier, sInit.ExtraProjectiles, sInit.PickupRadius);
                }

                var s = player.Stats;

                AddRow("🎯 크리티컬 확률 (Crit Chance: 0~100%)", s.CritChance * 100f, 0f, 100f, 1f, v =>
                {
                    var cur = player.Stats;
                    player.Stats = new CharacterStats(cur.MaxHealth, cur.HealthRegen, cur.MoveSpeed, cur.AttackPowerMultiplier, cur.Armor, v / 100f, cur.CritDamageMultiplier, cur.CooldownReduction, cur.AreaMultiplier, cur.ProjectileSpeedMultiplier, cur.ExtraProjectiles, cur.PickupRadius);
                    if (cStat != null) { cStat.CritChance = v / 100f; cStat.IsCustom = true; }
                }, isInt: true);

                AddRow("💥 크리티컬 데미지 배율 (Crit Multiplier)", s.CritDamageMultiplier, 1.0f, 5.0f, 0.05f, v =>
                {
                    var cur = player.Stats;
                    player.Stats = new CharacterStats(cur.MaxHealth, cur.HealthRegen, cur.MoveSpeed, cur.AttackPowerMultiplier, cur.Armor, cur.CritChance, v, cur.CooldownReduction, cur.AreaMultiplier, cur.ProjectileSpeedMultiplier, cur.ExtraProjectiles, cur.PickupRadius);
                    if (cStat != null) { cStat.CritDamageMultiplier = v; cStat.IsCustom = true; }
                });

                AddRow("⚔️ 기본 공격력 배율 (Attack Power)", s.AttackPowerMultiplier, 0.2f, 5.0f, 0.1f, v =>
                {
                    var cur = player.Stats;
                    player.Stats = new CharacterStats(cur.MaxHealth, cur.HealthRegen, cur.MoveSpeed, v, cur.Armor, cur.CritChance, cur.CritDamageMultiplier, cur.CooldownReduction, cur.AreaMultiplier, cur.ProjectileSpeedMultiplier, cur.ExtraProjectiles, cur.PickupRadius);
                    if (cStat != null) { cStat.AttackPowerMultiplier = v; cStat.IsCustom = true; }
                });

                AddRow("🏃 이동 속도 (Move Speed)", s.MoveSpeed, 2.0f, 12.0f, 0.2f, v =>
                {
                    var cur = player.Stats;
                    player.Stats = new CharacterStats(cur.MaxHealth, cur.HealthRegen, v, cur.AttackPowerMultiplier, cur.Armor, cur.CritChance, cur.CritDamageMultiplier, cur.CooldownReduction, cur.AreaMultiplier, cur.ProjectileSpeedMultiplier, cur.ExtraProjectiles, cur.PickupRadius);
                    if (cStat != null) { cStat.MoveSpeed = v; cStat.IsCustom = true; }
                });

                AddRow("🛡️ 방어력 (Armor)", s.Armor, 0f, 100f, 1f, v =>
                {
                    var cur = player.Stats;
                    player.Stats = new CharacterStats(cur.MaxHealth, cur.HealthRegen, cur.MoveSpeed, cur.AttackPowerMultiplier, v, cur.CritChance, cur.CritDamageMultiplier, cur.CooldownReduction, cur.AreaMultiplier, cur.ProjectileSpeedMultiplier, cur.ExtraProjectiles, cur.PickupRadius);
                    if (cStat != null) { cStat.Armor = v; cStat.IsCustom = true; }
                }, isInt: true);

                AddRow("⏱️ 쿨타임 감소율 (CDR: 0~75%)", s.CooldownReduction * 100f, 0f, 75f, 1f, v =>
                {
                    var cur = player.Stats;
                    player.Stats = new CharacterStats(cur.MaxHealth, cur.HealthRegen, cur.MoveSpeed, cur.AttackPowerMultiplier, cur.Armor, cur.CritChance, cur.CritDamageMultiplier, v / 100f, cur.AreaMultiplier, cur.ProjectileSpeedMultiplier, cur.ExtraProjectiles, cur.PickupRadius);
                    if (cStat != null) { cStat.CooldownReduction = v / 100f; cStat.IsCustom = true; }
                }, isInt: true);

                return;
            }

            if (skillId == "exp_tuning")
            {
                if (config?.Exp == null) return;
                var exp = config.Exp;

                AddRow("💎 알당 경험치 배율 (Gem Exp)", exp.GemExpMultiplier, 0.5f, 5.0f, 0.2f, v => { exp.GemExpMultiplier = v; if (gemManager != null) gemManager.Config = exp; });
                AddRow("📈 1Lv 기본 필요 경험치 (Base Exp)", exp.BaseRequiredExp, 1f, 20f, 1f, v => { exp.BaseRequiredExp = (int)v; if (levelSystem != null) levelSystem.Config = exp; }, isInt: true);
                AddRow("🚀 레벨별 경험치 증가율 (Growth)", exp.ExpGrowthFactor, 0.2f, 2.5f, 0.05f, v => { exp.ExpGrowthFactor = v; if (levelSystem != null) levelSystem.Config = exp; });
                AddShakeRow("👾 경험치 비례 몹 수 증가 (Exp->Mob)", exp.EnableLevelExpScaling, on => exp.EnableLevelExpScaling = on);
                AddRow("📊 경험치 증가분 대비 몹 젠 비율 (%)", exp.MobScalingRatio * 100f, 0f, 100f, 1f, v => exp.MobScalingRatio = v / 100f, isInt: true);
                AddRow("❤️ 경험치 비례 몹 체력 감쇠율 (%: 0~50%)", exp.MobHpScalingRatio * 100f, 0f, 50f, 1f, v => exp.MobHpScalingRatio = v / 100f, isInt: true);
                AddRow("🛡️ 몹 최대 체력 배율 상한선 (1.0x~10.0x)", exp.MobHpMaxCapMultiplier, 1.0f, 10.0f, 0.5f, v => exp.MobHpMaxCapMultiplier = v);
                AddRow("🛑 최대 몹 수 상한선 (Max Mob Cap)", exp.MaxMonsterCapLimit, 200f, 1200f, 50f, v => exp.MaxMonsterCapLimit = (int)v, isInt: true);
                AddShakeRow("🥊 타격 역경직 활성화 (Hit-Stop)", exp.EnableHitStop, on => exp.EnableHitStop = on);
                AddRow("⏱️ 역경직 지속 시간 (Hit-Stop Duration: s)", exp.HitStopDuration, 0.00f, 0.15f, 0.01f, v => exp.HitStopDuration = v);
                AddRow("🧊 역경직 슬로우 강도 (SlowScale: 0~0.5)", exp.HitStopSlowScale, 0.00f, 0.50f, 0.02f, v => exp.HitStopSlowScale = v);
                AddRow("📳 마스터 카메라 셰이크 강도 (%: 0~100%)", exp.MasterCameraShakeScale, 0f, 100f, 5f, v => exp.MasterCameraShakeScale = v, isInt: true);
                return;
            }

            if (player == null) return;
            var skill = player.GetSkill(skillId) as CompositeSkill;
            if (skill == null) return;

            int level = skill.Level;
            var mem = SkillTuningMemoryCache.GetOrCreate(skillId, level);
            var cdTrigger = skill.Trigger as CooldownTrigger;

            switch (skillId)
            {
                case "slash":
                    if (skill.Effect is GreatswordSlashEffect slash)
                    {
                        AddRow("⚔️ 공격력 (Damage)", slash.BaseDamage, 10f, 300f, 5f, v => { slash.BaseDamage = v; mem.Damage = v; });
                        AddRow("📏 베기 반경 (Radius)", slash.Radius, 1.0f, 8.0f, 0.2f, v => { slash.Radius = v; mem.Radius = v; });
                        AddRow("📐 부채꼴 각도 (ArcAngle)", slash.ArcAngleDegrees, 30f, 360f, 15f, v => { slash.ArcAngleDegrees = v; mem.ExtraParam1 = v; });
                    }
                    if (cdTrigger != null)
                        AddRow("⏱️ 쿨다운 (Cooldown)", cdTrigger.Cooldown, 0.1f, 4.0f, 0.1f, v => { cdTrigger.Cooldown = isInfiniteSpam ? 0.06f : v; mem.Cooldown = v; });
                    if (config?.Slash != null)
                        AddRow("📳 카메라 셰이크 (%: 0~100%)", config.Slash.CameraShakeScale, 0f, 100f, 5f, v => config.Slash.CameraShakeScale = v, isInt: true);
                    break;

                case "ground_stomp":
                    if (skill.Effect is GroundStompEffect stomp)
                    {
                        AddRow("💥 공격력 (Damage)", stomp.BaseDamage, 10f, 300f, 5f, v => { stomp.BaseDamage = v; mem.Damage = v; });
                        AddRow("📏 지진 반경 (Radius)", stomp.StompRadius, 1.0f, 8.0f, 0.2f, v => { stomp.StompRadius = v; mem.Radius = v; });
                    }
                    if (cdTrigger != null)
                        AddRow("⏱️ 쿨다운 (Cooldown)", cdTrigger.Cooldown, 0.2f, 4.0f, 0.1f, v => { cdTrigger.Cooldown = isInfiniteSpam ? 0.06f : v; mem.Cooldown = v; });
                    if (config?.GroundStomp != null)
                        AddRow("📳 카메라 셰이크 (%: 0~100%)", config.GroundStomp.CameraShakeScale, 0f, 100f, 5f, v => config.GroundStomp.CameraShakeScale = v, isInt: true);
                    break;

                case "whirlwind":
                    if (skill.Effect is WhirlwindEffect ww)
                    {
                        AddRow("🌀 공격력 (Damage)", ww.BaseDamage, 10f, 300f, 5f, v => { ww.BaseDamage = v; mem.Damage = v; });
                        AddRow("📏 회전 반경 (Radius)", ww.Radius, 1.0f, 8.0f, 0.2f, v => { ww.Radius = v; mem.Radius = v; });
                    }
                    if (cdTrigger != null)
                        AddRow("⏱️ 쿨다운 (Cooldown)", cdTrigger.Cooldown, 0.2f, 4.0f, 0.1f, v => { cdTrigger.Cooldown = isInfiniteSpam ? 0.06f : v; mem.Cooldown = v; });
                    if (config?.Whirlwind != null)
                        AddRow("📳 카메라 셰이크 (%: 0~100%)", config.Whirlwind.CameraShakeScale, 0f, 100f, 5f, v => config.Whirlwind.CameraShakeScale = v, isInt: true);
                    break;

                case "bow":
                    if (skill.Effect is PiercingArrowEffect bow)
                    {
                        AddRow("🏹 공격력 (Damage)", bow.BaseDamage, 10f, 300f, 5f, v => { bow.BaseDamage = v; mem.Damage = v; });
                        AddRow("🚀 화살 속도 (Speed)", bow.Speed, 5.0f, 35.0f, 1f, v => { bow.Speed = v; mem.Speed = v; });
                        AddRow("🔢 화살 개수 (ArrowCount)", bow.ArrowCount, 1f, 10f, 1f, v => { bow.ArrowCount = (int)v; mem.Count = (int)v; }, isInt: true);
                        AddRow("📐 확산 각도 (SpreadAngle)", bow.SpreadAngleDeg, 10f, 120f, 5f, v => { bow.SpreadAngleDeg = v; mem.ExtraParam1 = v; });
                    }
                    if (cdTrigger != null)
                        AddRow("⏱️ 쿨다운 (Cooldown)", cdTrigger.Cooldown, 0.1f, 3.0f, 0.1f, v => { cdTrigger.Cooldown = isInfiniteSpam ? 0.06f : v; mem.Cooldown = v; });
                    if (config?.Bow != null)
                        AddRow("📳 카메라 셰이크 (%: 0~100%)", config.Bow.CameraShakeScale, 0f, 100f, 5f, v => config.Bow.CameraShakeScale = v, isInt: true);
                    break;

                case "glaive":
                    if (skill.Effect is WindGlaiveEffect glaive)
                    {
                        AddRow("🪓 공격력 (Damage)", glaive.BaseDamage, 10f, 300f, 5f, v => { glaive.BaseDamage = v; mem.Damage = v; });
                        AddRow("📏 사거리 (Distance)", glaive.MaxDistance, 4.0f, 18.0f, 0.5f, v => { glaive.MaxDistance = v; mem.Radius = v; });
                        AddRow("🚀 비행 속도 (Speed)", glaive.Speed, 5.0f, 30.0f, 1f, v => { glaive.Speed = v; mem.Speed = v; });
                        AddRow("🔢 글레이브 수 (Count)", glaive.GlaiveCount, 1f, 6f, 1f, v => { glaive.GlaiveCount = (int)v; mem.Count = (int)v; }, isInt: true);
                    }
                    if (cdTrigger != null)
                        AddRow("⏱️ 쿨다운 (Cooldown)", cdTrigger.Cooldown, 0.2f, 4.0f, 0.1f, v => { cdTrigger.Cooldown = isInfiniteSpam ? 0.06f : v; mem.Cooldown = v; });
                    if (config?.Glaive != null)
                        AddRow("📳 카메라 셰이크 (%: 0~100%)", config.Glaive.CameraShakeScale, 0f, 100f, 5f, v => config.Glaive.CameraShakeScale = v, isInt: true);
                    break;

                case "arrow_rain":
                    if (skill.Effect is ArrowRainEffect ar)
                    {
                        AddRow("🌧️ 공격력 (Damage)", ar.BaseDamage, 10f, 300f, 5f, v => { ar.BaseDamage = v; mem.Damage = v; });
                        AddRow("📏 폭격 반경 (Radius)", ar.Radius, 1.0f, 8.0f, 0.2f, v => { ar.Radius = v; mem.Radius = v; });
                        AddRow("⏳ 지속 시간 (Duration)", ar.Duration, 0.5f, 6.0f, 0.5f, v => { ar.Duration = v; mem.Duration = v; });
                        AddRow("🔢 쏟아지는 화살 (Count)", ar.ArrowCount, 10f, 80f, 5f, v => { ar.ArrowCount = (int)v; mem.Count = (int)v; }, isInt: true);
                    }
                    if (cdTrigger != null)
                        AddRow("⏱️ 쿨다운 (Cooldown)", cdTrigger.Cooldown, 0.5f, 5.0f, 0.2f, v => { cdTrigger.Cooldown = isInfiniteSpam ? 0.06f : v; mem.Cooldown = v; });
                    if (config?.ArrowRain != null)
                        AddRow("📳 카메라 셰이크 (%: 0~100%)", config.ArrowRain.CameraShakeScale, 0f, 100f, 5f, v => config.ArrowRain.CameraShakeScale = v, isInt: true);
                    break;

                case "fireball":
                    if (skill.Effect is FireballEffect fb)
                    {
                        AddRow("🔥 공격력 (Damage)", fb.BaseDamage, 10f, 300f, 5f, v => { fb.BaseDamage = v; mem.Damage = v; });
                        AddRow("📏 폭발 반경 (Radius)", fb.Radius, 1.0f, 6.0f, 0.2f, v => { fb.Radius = v; mem.Radius = v; });
                        AddRow("🚀 투사체 속도 (Speed)", fb.Speed, 5.0f, 30.0f, 1f, v => { fb.Speed = v; mem.Speed = v; });
                        AddRow("🔢 화염구 수 (Count)", fb.FireballCount, 1f, 5f, 1f, v => { fb.FireballCount = (int)v; mem.Count = (int)v; }, isInt: true);
                    }
                    if (cdTrigger != null)
                        AddRow("⏱️ 쿨다운 (Cooldown)", cdTrigger.Cooldown, 0.2f, 4.0f, 0.1f, v => { cdTrigger.Cooldown = isInfiniteSpam ? 0.06f : v; mem.Cooldown = v; });
                    if (config?.Fireball != null)
                        AddRow("📳 카메라 셰이크 (%: 0~100%)", config.Fireball.CameraShakeScale, 0f, 100f, 5f, v => config.Fireball.CameraShakeScale = v, isInt: true);
                    break;

                case "frost_nova":
                    if (skill.Effect is FrostNovaEffect fn)
                    {
                        AddRow("❄️ 공격력 (Damage)", fn.BaseDamage, 10f, 300f, 5f, v => { fn.BaseDamage = v; mem.Damage = v; });
                        AddRow("📏 냉기 반경 (Radius)", fn.Radius, 1.5f, 9.0f, 0.3f, v => { fn.Radius = v; mem.Radius = v; });
                        AddRow("⏳ 오한 시간 (ChillDuration)", fn.ChillDuration, 1.0f, 10.0f, 0.5f, v => { fn.ChillDuration = v; mem.Duration = v; });
                    }
                    if (cdTrigger != null)
                        AddRow("⏱️ 쿨다운 (Cooldown)", cdTrigger.Cooldown, 0.2f, 4.0f, 0.1f, v => { cdTrigger.Cooldown = isInfiniteSpam ? 0.06f : v; mem.Cooldown = v; });
                    if (config?.FrostNova != null)
                        AddRow("📳 카메라 셰이크 (%: 0~100%)", config.FrostNova.CameraShakeScale, 0f, 100f, 5f, v => config.FrostNova.CameraShakeScale = v, isInt: true);
                    break;

                case "chain_lightning":
                    if (skill.Effect is ChainLightningEffect cl)
                    {
                        AddRow("⚡ 공격력 (Damage)", cl.BaseDamage, 10f, 300f, 5f, v => { cl.BaseDamage = v; mem.Damage = v; });
                        AddRow("🔢 연쇄 횟수 (ChainCount)", cl.ChainCount, 1f, 16f, 1f, v => { cl.ChainCount = (int)v; mem.Count = (int)v; }, isInt: true);
                        AddRow("📏 점프 거리 (JumpRadius)", cl.JumpRadius, 2.0f, 10.0f, 0.5f, v => { cl.JumpRadius = v; mem.Radius = v; });
                    }
                    if (cdTrigger != null)
                        AddRow("⏱️ 쿨다운 (Cooldown)", cdTrigger.Cooldown, 0.2f, 4.0f, 0.1f, v => { cdTrigger.Cooldown = isInfiniteSpam ? 0.06f : v; mem.Cooldown = v; });
                    if (config?.ChainLightning != null)
                        AddRow("📳 카메라 셰이크 (%: 0~100%)", config.ChainLightning.CameraShakeScale, 0f, 100f, 5f, v => config.ChainLightning.CameraShakeScale = v, isInt: true);
                    break;

                case "orbital":
                    if (skill.Effect is OrbitingBladesEffect orb)
                    {
                        AddRow("⚔️ 공격력 (Damage)", orb.BaseDamage, 10f, 300f, 5f, v => { orb.BaseDamage = v; mem.Damage = v; });
                        AddRow("📏 회전 반경 (Radius)", orb.OrbitRadius, 1.0f, 6.0f, 0.2f, v => { orb.OrbitRadius = v; mem.Radius = v; });
                        AddRow("🚀 회전 속도 (Speed)", orb.RotationSpeed, 1.0f, 12.0f, 0.5f, v => { orb.RotationSpeed = v; mem.Speed = v; });
                        AddRow("🔢 칼날 개수 (BladeCount)", orb.BladeCount, 1f, 8f, 1f, v => { orb.BladeCount = (int)v; mem.Count = (int)v; }, isInt: true);
                    }
                    if (cdTrigger != null)
                        AddRow("⏱️ 틱 쿨다운 (Cooldown)", cdTrigger.Cooldown, 0.05f, 1.0f, 0.05f, v => { cdTrigger.Cooldown = isInfiniteSpam ? 0.06f : v; mem.Cooldown = v; });
                    if (config?.Orbital != null)
                        AddRow("📳 카메라 셰이크 (%: 0~100%)", config.Orbital.CameraShakeScale, 0f, 100f, 5f, v => config.Orbital.CameraShakeScale = v, isInt: true);
                    break;

                case "blood_eater":
                    if (skill.Effect is BloodEaterEffect be)
                    {
                        AddRow("🩸 공격력 (Damage)", be.BaseDamage, 20f, 400f, 5f, v => { be.BaseDamage = v; mem.Damage = v; });
                        AddRow("📏 흡혈 반경 (Radius)", be.Radius, 2.0f, 10.0f, 0.2f, v => { be.Radius = v; mem.Radius = v; });
                        AddRow("📐 부채꼴 각도 (ArcAngle)", be.ArcAngleDegrees, 30f, 360f, 15f, v => { be.ArcAngleDegrees = v; mem.ExtraParam2 = v; if (config?.BloodEater != null) config.BloodEater.ArcAngle = v; });
                        AddRow("💖 회복량 (HealAmount)", be.HealAmount, 0.5f, 10.0f, 0.5f, v => { be.HealAmount = v; mem.ExtraParam1 = v; });
                    }
                    if (cdTrigger != null)
                        AddRow("⏱️ 쿨다운 (Cooldown)", cdTrigger.Cooldown, 0.2f, 4.0f, 0.1f, v => { cdTrigger.Cooldown = isInfiniteSpam ? 0.06f : v; mem.Cooldown = v; });
                    if (config?.BloodEater != null)
                        AddRow("📳 카메라 셰이크 (%: 0~100%)", config.BloodEater.CameraShakeScale, 0f, 100f, 5f, v => config.BloodEater.CameraShakeScale = v, isInt: true);
                    break;

                case "storm_bow":
                    if (skill.Effect is StormArrowEffect sb)
                    {
                        AddRow("🏹 화살 대미지 (ArrowDmg)", sb.ArrowDamage, 20f, 400f, 5f, v => { sb.ArrowDamage = v; mem.Damage = v; });
                        AddRow("💥 폭풍 폭발 대미지 (ExplosionDmg)", sb.ExplosionDamage, 10f, 300f, 5f, v => { sb.ExplosionDamage = v; mem.ExtraParam1 = v; });
                        AddRow("📏 폭발 반경 (Radius)", sb.ExplosionRadius, 0.8f, 5.0f, 0.2f, v => { sb.ExplosionRadius = v; mem.Radius = v; });
                        AddRow("🚀 화살 속도 (Speed)", sb.Speed, 5.0f, 40.0f, 1f, v => { sb.Speed = v; mem.Speed = v; });
                        AddRow("🔢 화살 개수 (ArrowCount)", sb.BaseArrowCount, 1f, 12f, 1f, v => { sb.BaseArrowCount = (int)v; mem.Count = (int)v; }, isInt: true);
                    }
                    if (cdTrigger != null)
                        AddRow("⏱️ 쿨다운 (Cooldown)", cdTrigger.Cooldown, 0.2f, 4.0f, 0.1f, v => { cdTrigger.Cooldown = isInfiniteSpam ? 0.06f : v; mem.Cooldown = v; });
                    if (config?.StormBow != null)
                        AddRow("📳 카메라 셰이크 (%: 0~100%)", config.StormBow.CameraShakeScale, 0f, 100f, 5f, v => config.StormBow.CameraShakeScale = v, isInt: true);
                    break;

                case "meteor_strike":
                    if (skill.Effect is MeteorStrikeEffect ms)
                    {
                        AddRow("☄️ 공격력 (Damage)", ms.BaseDamage, 30f, 500f, 10f, v => { ms.BaseDamage = v; mem.Damage = v; if (config?.MeteorStrike != null) config.MeteorStrike.Damage = v; });
                        AddRow("📏 폭발 반경 (Radius)", ms.ExplosionRadius, 1.0f, 12.0f, 0.2f, v => { ms.ExplosionRadius = v; mem.Radius = v; if (config?.MeteorStrike != null) config.MeteorStrike.Radius = v; });
                    }
                    if (cdTrigger != null)
                        AddRow("⏱️ 쿨다운 (Cooldown)", cdTrigger.Cooldown, 0.2f, 4.0f, 0.1f, v => { cdTrigger.Cooldown = isInfiniteSpam ? 0.06f : v; mem.Cooldown = v; if (config?.MeteorStrike != null) config.MeteorStrike.Cooldown = v; });
                    if (config?.MeteorStrike != null)
                        AddRow("📳 카메라 셰이크 (%: 0~100%)", config.MeteorStrike.CameraShakeScale, 0f, 100f, 5f, v => config.MeteorStrike.CameraShakeScale = v, isInt: true);
                    break;

                case "tempest_whirlwind":
                    if (skill.Effect is TempestWhirlwindEffect tw)
                    {
                        AddRow("🌪️ 공격력 (Damage)", tw.BaseDamage, 20f, 400f, 5f, v => { tw.BaseDamage = v; mem.Damage = v; });
                        AddRow("📏 회전 반경 (Radius)", tw.Radius, 1.5f, 9.0f, 0.2f, v => { tw.Radius = v; mem.Radius = v; });
                        AddRow("⚔️ 참격 파동 수 (Waves)", tw.SlashWaveCount, 1f, 8f, 1f, v => { tw.SlashWaveCount = (int)v; mem.Count = (int)v; }, isInt: true);
                    }
                    if (cdTrigger != null)
                        AddRow("⏱️ 쿨다운 (Cooldown)", cdTrigger.Cooldown, 0.2f, 4.0f, 0.1f, v => { cdTrigger.Cooldown = isInfiniteSpam ? 0.06f : v; mem.Cooldown = v; });
                    if (config?.TempestWhirlwind != null)
                        AddRow("📳 카메라 셰이크 (%: 0~100%)", config.TempestWhirlwind.CameraShakeScale, 0f, 100f, 5f, v => config.TempestWhirlwind.CameraShakeScale = v, isInt: true);
                    break;

                case "earthshaker":
                    if (skill.Effect is EarthshakerEffect es)
                    {
                        AddRow("🌋 공격력 (Damage)", es.BaseDamage, 20f, 400f, 5f, v => { es.BaseDamage = v; mem.Damage = v; });
                        AddRow("📏 지진 반경 (Radius)", es.Radius, 2.0f, 10.0f, 0.2f, v => { es.Radius = v; mem.Radius = v; });
                        AddRow("⚡ 균열 방향 수 (Fissures)", es.FissureCount, 2f, 8f, 1f, v => { es.FissureCount = (int)v; mem.Count = (int)v; }, isInt: true);
                    }
                    if (cdTrigger != null)
                        AddRow("⏱️ 쿨다운 (Cooldown)", cdTrigger.Cooldown, 0.2f, 4.0f, 0.1f, v => { cdTrigger.Cooldown = isInfiniteSpam ? 0.06f : v; mem.Cooldown = v; });
                    if (config?.Earthshaker != null)
                        AddRow("📳 카메라 셰이크 (%: 0~100%)", config.Earthshaker.CameraShakeScale, 0f, 100f, 5f, v => config.Earthshaker.CameraShakeScale = v, isInt: true);
                    break;

                case "phantom_glaive":
                    if (skill.Effect is PhantomGlaiveEffect pg)
                    {
                        AddRow("🪃 공격력 (Damage)", pg.BaseDamage, 20f, 400f, 5f, v => { pg.BaseDamage = v; mem.Damage = v; });
                        AddRow("📏 사거리 (Distance)", pg.MaxDistance, 4.0f, 20.0f, 0.5f, v => { pg.MaxDistance = v; mem.Radius = v; });
                        AddRow("🚀 비행 속도 (Speed)", pg.Speed, 5.0f, 35.0f, 1f, v => { pg.Speed = v; mem.Speed = v; });
                        AddRow("📏 칼날 크기 (BladeScale)", pg.BladeScale, 0.5f, 3.0f, 0.1f, v => { pg.BladeScale = v; mem.ExtraParam1 = v; });
                        AddRow("👻 환영 글레이브 (PhantomCount)", pg.PhantomCount, 1f, 7f, 1f, v => { pg.PhantomCount = (int)v; mem.Count = (int)v; }, isInt: true);
                    }
                    if (cdTrigger != null)
                        AddRow("⏱️ 쿨다운 (Cooldown)", cdTrigger.Cooldown, 0.2f, 4.0f, 0.1f, v => { cdTrigger.Cooldown = isInfiniteSpam ? 0.06f : v; mem.Cooldown = v; });
                    if (config?.PhantomGlaive != null)
                        AddRow("📳 카메라 셰이크 (%: 0~100%)", config.PhantomGlaive.CameraShakeScale, 0f, 100f, 5f, v => config.PhantomGlaive.CameraShakeScale = v, isInt: true);
                    break;

                case "stellar_rain":
                    if (skill.Effect is StellarRainEffect sr)
                    {
                        AddRow("🌟 공격력 (Damage)", sr.BaseDamage, 20f, 400f, 5f, v => { sr.BaseDamage = v; mem.Damage = v; });
                        AddRow("📏 폭격 반경 (Radius)", sr.Radius, 2.0f, 10.0f, 0.2f, v => { sr.Radius = v; mem.Radius = v; });
                        AddRow("⏳ 지속 시간 (Duration)", sr.Duration, 0.5f, 8.0f, 0.5f, v => { sr.Duration = v; mem.Duration = v; });
                        AddRow("🏹 유성 화살 수 (ArrowCount)", sr.ArrowCount, 20f, 120f, 5f, v => { sr.ArrowCount = (int)v; mem.Count = (int)v; }, isInt: true);
                    }
                    if (cdTrigger != null)
                        AddRow("⏱️ 쿨다운 (Cooldown)", cdTrigger.Cooldown, 0.5f, 5.0f, 0.2f, v => { cdTrigger.Cooldown = isInfiniteSpam ? 0.06f : v; mem.Cooldown = v; });
                    if (config?.StellarRain != null)
                        AddRow("📳 카메라 셰이크 (%: 0~100%)", config.StellarRain.CameraShakeScale, 0f, 100f, 5f, v => config.StellarRain.CameraShakeScale = v, isInt: true);
                    break;

                case "gigastorm_lightning":
                    if (skill.Effect is GigastormLightningEffect gl)
                    {
                        AddRow("⚡ 공격력 (Damage)", gl.BaseDamage, 20f, 400f, 5f, v => { gl.BaseDamage = v; mem.Damage = v; });
                        AddRow("🔢 연쇄 횟수 (ChainCount)", gl.ChainCount, 2f, 16f, 1f, v => { gl.ChainCount = (int)v; mem.Count = (int)v; }, isInt: true);
                        AddRow("📏 점프 반경 (ChainRange)", gl.ChainRange, 2.0f, 12.0f, 0.5f, v => { gl.ChainRange = v; mem.Radius = v; });
                        AddRow("💥 뇌격 폭발 반경 (SparkRadius)", gl.SparkRadius, 0.8f, 4.0f, 0.2f, v => { gl.SparkRadius = v; mem.ExtraParam1 = v; });
                    }
                    if (cdTrigger != null)
                        AddRow("⏱️ 쿨다운 (Cooldown)", cdTrigger.Cooldown, 0.2f, 4.0f, 0.1f, v => { cdTrigger.Cooldown = isInfiniteSpam ? 0.06f : v; mem.Cooldown = v; });
                    if (config?.GigastormLightning != null)
                        AddRow("📳 카메라 셰이크 (%: 0~100%)", config.GigastormLightning.CameraShakeScale, 0f, 100f, 5f, v => config.GigastormLightning.CameraShakeScale = v, isInt: true);
                    break;

                case "blizzard_nova":
                    if (skill.Effect is BlizzardNovaEffect bn)
                    {
                        AddRow("❄️ 공격력 (Damage)", bn.BaseDamage, 20f, 400f, 5f, v => { bn.BaseDamage = v; mem.Damage = v; });
                        AddRow("📏 냉기 반경 (Radius)", bn.Radius, 2.0f, 12.0f, 0.3f, v => { bn.Radius = v; mem.Radius = v; });
                        AddRow("🧊 빙하 파편 수 (ShardCount)", bn.ShardCount, 4f, 16f, 1f, v => { bn.ShardCount = (int)v; mem.Count = (int)v; }, isInt: true);
                    }
                    if (cdTrigger != null)
                        AddRow("⏱️ 쿨다운 (Cooldown)", cdTrigger.Cooldown, 0.2f, 4.0f, 0.1f, v => { cdTrigger.Cooldown = isInfiniteSpam ? 0.06f : v; mem.Cooldown = v; });
                    if (config?.BlizzardNova != null)
                        AddRow("📳 카메라 셰이크 (%: 0~100%)", config.BlizzardNova.CameraShakeScale, 0f, 100f, 5f, v => config.BlizzardNova.CameraShakeScale = v, isInt: true);
                    break;
            }
        }

        public static void ConfigureMonsterRows(
            int monsterIdx,
            Transform container,
            MonsterTuningConfigData mConfig,
            Action<GameObject> onRowCreated)
        {
            if (container == null || mConfig == null) return;
            float yOffset = 0f;

            void AddRow(string title, float curVal, float min, float max, float step, Action<float> onChanged, bool isInt = false)
            {
                var row = SkillTuningSliderFactory.CreateSliderRow(container, title, curVal, min, max, step, onChanged, ref yOffset, isInt);
                onRowCreated?.Invoke(row);
            }

            switch (monsterIdx)
            {
                case 0: AddCommonMonsterRows("🟢 슬라임", mConfig.Slime, AddRow); break;
                case 1: AddCommonMonsterRows("🦇 박쥐", mConfig.Bat, AddRow); break;
                case 2:
                    AddCommonMonsterRows("💀 해골 궁수", mConfig.Skeleton, AddRow);
                    AddRow("🚀 뼈다귀 투사체 속도 (Speed)", mConfig.Skeleton.ProjectileSpeed, 1.0f, 10.0f, 0.25f, v => mConfig.Skeleton.ProjectileSpeed = v);
                    AddRow("🏹 뼈다귀 투사체 대미지 (Damage)", mConfig.Skeleton.ProjectileDamage, 2.0f, 60.0f, 1f, v => mConfig.Skeleton.ProjectileDamage = v);
                    break;
                case 3: AddCommonMonsterRows("🗿 골렘", mConfig.Golem, AddRow); break;
                case 4: AddCommonMonsterRows("🔥 화염 임프", mConfig.FireImp, AddRow); break;
                case 5: AddCommonMonsterRows("🕷️ 독 거미", mConfig.ToxicSpider, AddRow); break;
                case 6: AddCommonMonsterRows("⚔️ 흑기사", mConfig.DarkKnight, AddRow); break;
                case 7:
                    AddCommonMonsterRows("👹 고블린 킹 (보스)", mConfig.Boss, AddRow);
                    AddRow("⏱️ 광선 주기 (Laser Interval)", mConfig.Boss.LaserInterval, 3.0f, 20.0f, 1f, v => mConfig.Boss.LaserInterval = v);
                    AddRow("☄️ 광선 초당 피해 (Laser DPS)", mConfig.Boss.LaserDamage, 5.0f, 100.0f, 5f, v => mConfig.Boss.LaserDamage = v);
                    break;
            }
        }

        private static void AddCommonMonsterRows(
            string prefix,
            MonsterStatConfig stat,
            Action<string, float, float, float, float, Action<float>, bool> addRow)
        {
            if (stat == null) return;
            addRow($"❤️ {prefix} 최대 체력 (HP)", stat.MaxHealth, 5f, 2000f, 5f, v => stat.MaxHealth = v, false);
            addRow($"👟 {prefix} 이동 속도 (Speed)", stat.MoveSpeed, 0.5f, 10.0f, 0.1f, v => stat.MoveSpeed = v, false);
            addRow($"💥 {prefix} 접촉 공격력 (Damage)", stat.ContactDamage, 1f, 150f, 1f, v => stat.ContactDamage = v, false);
            addRow($"💎 {prefix} 처치 경험치 (Exp)", stat.ExpValue, 1f, 100f, 1f, v => stat.ExpValue = (int)v, true);
            addRow($"💰 {prefix} 처치 골드 (Gold)", stat.GoldValue, 1f, 200f, 1f, v => stat.GoldValue = (int)v, true);
        }
    }
}
