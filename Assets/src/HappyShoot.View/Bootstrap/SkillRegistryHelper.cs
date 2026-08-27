using HappyShoot.Domain.Entities;
using HappyShoot.Domain.Leveling;
using HappyShoot.Domain.Skills;
using HappyShoot.Domain.Skills.Effects;
using HappyShoot.Domain.Skills.Evolution;
using HappyShoot.Domain.Skills.Targeters;
using HappyShoot.Domain.Skills.Triggers;
using HappyShoot.View.Config;

namespace HappyShoot.View.Bootstrap
{
    /// <summary>
    /// Helper responsible for registering all Character Skills, Passives, and Evolution Recipes.
    /// Strictly modular and under 500 lines.
    /// </summary>
    public static class SkillRegistryHelper
    {
        public static void RegisterAllSkills(SkillRewardManager rewardManager)
        {
            // 1. Warrior Exclusive Skills
            rewardManager.RegisterSkill("slash", "대검 베기", "전방 150도 궤적의 적들을 시원하게 베어버리는 근접 광역 물리 공격",
                () => {
                    var cfg = SkillConfigRepository.Instance.GetConfig();
                    return new CompositeSkill("slash", "대검 베기", new CooldownTrigger(cfg.Slash.Cooldown), new ClosestEnemyTargeter(), new GreatswordSlashEffect(cfg.Slash.Damage, cfg.Slash.Radius, cfg.Slash.ArcAngle), range: cfg.Slash.Radius + 0.3f);
                },
                new[] { CharacterClassType.Warrior });

            rewardManager.RegisterSkill("ground_stomp", "지면 강타", "전방으로 지면을 강하게 내리쳐 연쇄 지진 충격파를 파파팍 솟구쳐 적들을 강타 (레벨업 시 줄기 수 및 사거리 증가)",
                () => {
                    var cfg = SkillConfigRepository.Instance.GetConfig();
                    return new CompositeSkill("ground_stomp", "지면 강타", new CooldownTrigger(cfg.GroundStomp.Cooldown), new ClosestEnemyTargeter(), new GroundStompEffect(cfg.GroundStomp.Damage, cfg.GroundStomp.Length, cfg.GroundStomp.Radius, cfg.GroundStomp.LineCount), range: cfg.GroundStomp.Length);
                },
                new[] { CharacterClassType.Warrior });

            rewardManager.RegisterSkill("whirlwind", "휠윈드", "플레이어 주변 360도 전방위로 회전 검기 충격파를 날려 접근하는 적들을 일제 타격 (레벨업 시 범위 대폭 확장)",
                () => {
                    var cfg = SkillConfigRepository.Instance.GetConfig();
                    return new CompositeSkill("whirlwind", "휠윈드", new CooldownTrigger(cfg.Whirlwind.Cooldown), new ClosestEnemyTargeter(), new WhirlwindEffect(cfg.Whirlwind.Damage, cfg.Whirlwind.Radius), range: cfg.Whirlwind.Radius + 0.3f);
                },
                new[] { CharacterClassType.Warrior });

            // 2. Ranger Exclusive Skills
            rewardManager.RegisterSkill("bow", "관통 화살", "가장 가까운 적을 향해 고속으로 날아가 화면 끝까지 적들을 무제한 꿰뚫는 원거리 관통 사격 (레벨업 시 화살 수 추가)",
                () => {
                    var cfg = SkillConfigRepository.Instance.GetConfig();
                    return new CompositeSkill("bow", "관통 화살", new CooldownTrigger(cfg.Bow.Cooldown), new ClosestEnemyTargeter(), new PiercingArrowEffect(cfg.Bow.Damage, cfg.Bow.Speed, cfg.Bow.ArrowCount, 999, cfg.Bow.SpreadAngle));
                },
                new[] { CharacterClassType.Ranger });

            rewardManager.RegisterSkill("glaive", "칼바람 글레이브", "전방으로 회전하는 풍인을 던져 적들을 관통하고 되돌아오며 2중 타격하는 사냥꾼의 부메랑 무기 (레벨업 시 글레이브 수 증가)",
                () => {
                    var cfg = SkillConfigRepository.Instance.GetConfig();
                    return new CompositeSkill("glaive", "칼바람 글레이브", new CooldownTrigger(cfg.Glaive.Cooldown), new ClosestEnemyTargeter(), new WindGlaiveEffect(cfg.Glaive.Damage, cfg.Glaive.Distance, cfg.Glaive.Speed, cfg.Glaive.GlaiveCount), range: cfg.Glaive.Distance);
                },
                new[] { CharacterClassType.Ranger });

            rewardManager.RegisterSkill("arrow_rain", "화살비", "하늘 높이 화살을 쏘아 올려 지정된 전장에 쏟아붓는 원거리 광역 물리 폭격",
                () => {
                    var cfg = SkillConfigRepository.Instance.GetConfig();
                    return new CompositeSkill("arrow_rain", "화살비", new CooldownTrigger(cfg.ArrowRain.Cooldown), new ClosestEnemyTargeter(), new ArrowRainEffect(cfg.ArrowRain.Damage, cfg.ArrowRain.Radius, cfg.ArrowRain.Duration, cfg.ArrowRain.ArrowCount), range: 8.0f);
                },
                new[] { CharacterClassType.Ranger });

            // 3. Wizard Exclusive Skills
            rewardManager.RegisterSkill("fireball", "화염구", "적에게 날아가는 고속 혜성 화염구를 발사하여 목표 지점에 강력한 화염 폭발을 일으키는 마법 공격 (레벨업 시 화염구 추가)",
                () => {
                    var cfg = SkillConfigRepository.Instance.GetConfig();
                    return new CompositeSkill("fireball", "화염구", new CooldownTrigger(cfg.Fireball.Cooldown), new ClosestEnemyTargeter(), new FireballEffect(cfg.Fireball.Damage, cfg.Fireball.Radius, cfg.Fireball.Speed, cfg.Fireball.FireballCount), range: 9.0f);
                },
                new[] { CharacterClassType.Wizard });

            rewardManager.RegisterSkill("frost_nova", "서리 폭발", "플레이어 주변 360도로 얼어붙는 한기 파동을 방출하여 모든 적을 얼리고 감속시키는 빙결 마법",
                () => {
                    var cfg = SkillConfigRepository.Instance.GetConfig();
                    return new CompositeSkill("frost_nova", "서리 폭발", new CooldownTrigger(cfg.FrostNova.Cooldown), new ClosestEnemyTargeter(), new FrostNovaEffect(cfg.FrostNova.Damage, cfg.FrostNova.Radius, cfg.FrostNova.ChillDuration), range: cfg.FrostNova.Radius);
                },
                new[] { CharacterClassType.Wizard });

            rewardManager.RegisterSkill("chain_lightning", "연쇄 번개", "가장 가까운 적으로부터 주변 4마리의 적에게 번갯불이 순차적으로 전이되며 감전시키는 전격 마법",
                () => {
                    var cfg = SkillConfigRepository.Instance.GetConfig();
                    return new CompositeSkill("chain_lightning", "연쇄 번개", new CooldownTrigger(cfg.ChainLightning.Cooldown), new ClosestEnemyTargeter(), new ChainLightningEffect(cfg.ChainLightning.Damage, cfg.ChainLightning.ChainCount, cfg.ChainLightning.JumpRadius), range: 8.0f);
                },
                new[] { CharacterClassType.Wizard });

            // 4. Common Skills
            rewardManager.RegisterSkill("orbital", "수호의 검", "플레이어 주위를 공전하며 접촉하는 적들을 연속으로 베어버리는 회전 검날",
                () => {
                    var cfg = SkillConfigRepository.Instance.GetConfig();
                    return new CompositeSkill("orbital", "수호의 검", new CooldownTrigger(cfg.Orbital.Cooldown), new SelfTargeter(), new OrbitingBladesEffect(cfg.Orbital.Damage, cfg.Orbital.Radius, cfg.Orbital.RotationSpeed, cfg.Orbital.BladeCount), range: 0f);
                },
                new[] { CharacterClassType.Warrior, CharacterClassType.Ranger, CharacterClassType.Wizard });
        }

        public static void RegisterAllPassives(SkillRewardManager rewardManager)
        {
            PassiveConfigData Cfg() => SkillConfigRepository.Instance?.GetConfig()?.Passives ?? new PassiveConfigData();

            rewardManager.RegisterPassive("passive_fang", "흡혈귀의 이빨", "공격력 증가 (대검 진화 재료)", 5, (p, lv) =>
            {
                var s = p.Stats;
                float inc = Cfg().FangAttackPowerPercent / 100f;
                p.Stats = new CharacterStats(s.MaxHealth, s.HealthRegen, s.MoveSpeed, s.AttackPowerMultiplier + inc, s.Armor, s.CritChance, s.CritDamageMultiplier, s.CooldownReduction, s.AreaMultiplier, s.ProjectileSpeedMultiplier, s.ExtraProjectiles, s.PickupRadius);
            });

            rewardManager.RegisterPassive("passive_feather", "바람의 깃털", "이동속도 & 투사체 속도 증가 (활/휠윈드 진화 재료)", 5, (p, lv) =>
            {
                var s = p.Stats;
                var cfg = Cfg();
                p.Stats = new CharacterStats(s.MaxHealth, s.HealthRegen, s.MoveSpeed + cfg.FeatherMoveSpeed, s.AttackPowerMultiplier, s.Armor, s.CritChance, s.CritDamageMultiplier, s.CooldownReduction, s.AreaMultiplier, s.ProjectileSpeedMultiplier + (cfg.FeatherProjSpeedPercent / 100f), s.ExtraProjectiles, s.PickupRadius);
            });

            rewardManager.RegisterPassive("passive_rune", "마나 룬", "쿨타임 감소 & 공격 범위 증가 (화염구 진화 재료)", 5, (p, lv) =>
            {
                var s = p.Stats;
                var cfg = Cfg();
                p.Stats = new CharacterStats(s.MaxHealth, s.HealthRegen, s.MoveSpeed, s.AttackPowerMultiplier, s.Armor, s.CritChance, s.CritDamageMultiplier, s.CooldownReduction + (cfg.RuneCooldownReductionPercent / 100f), s.AreaMultiplier + (cfg.RuneAreaMultiplierPercent / 100f), s.ProjectileSpeedMultiplier, s.ExtraProjectiles, s.PickupRadius);
            });

            rewardManager.RegisterPassive("passive_armor", "강철 갑옷", "방어력 증가 (지면강타 진화 재료)", 5, (p, lv) =>
            {
                var s = p.Stats;
                p.Stats = new CharacterStats(s.MaxHealth, s.HealthRegen, s.MoveSpeed, s.AttackPowerMultiplier, s.Armor + Cfg().ArmorAmount, s.CritChance, s.CritDamageMultiplier, s.CooldownReduction, s.AreaMultiplier, s.ProjectileSpeedMultiplier, s.ExtraProjectiles, s.PickupRadius);
            });

            rewardManager.RegisterPassive("passive_ring", "황금 반지", "자석 흡수 반경 증가 (화살비 진화 재료)", 5, (p, lv) =>
            {
                var s = p.Stats;
                p.Stats = new CharacterStats(s.MaxHealth, s.HealthRegen, s.MoveSpeed, s.AttackPowerMultiplier, s.Armor, s.CritChance, s.CritDamageMultiplier, s.CooldownReduction, s.AreaMultiplier, s.ProjectileSpeedMultiplier, s.ExtraProjectiles, s.PickupRadius + Cfg().RingPickupRadius);
            });

            rewardManager.RegisterPassive("passive_heart", "생명의 펜던트", "최대 체력 & 초당 체력 재생 증가 (서리폭발 진화 재료)", 5, (p, lv) =>
            {
                var s = p.Stats;
                var cfg = Cfg();
                p.Stats = new CharacterStats(s.MaxHealth + cfg.HeartMaxHp, s.HealthRegen + cfg.HeartHpRegen, s.MoveSpeed, s.AttackPowerMultiplier, s.Armor, s.CritChance, s.CritDamageMultiplier, s.CooldownReduction, s.AreaMultiplier, s.ProjectileSpeedMultiplier, s.ExtraProjectiles, s.PickupRadius);
            });

            rewardManager.RegisterPassive("passive_ignition", "발화의 불꽃", "화염 마법 공격 시 적을 7초간 불태우며 공격력 증가", 5, (p, lv) =>
            {
                var s = p.Stats;
                p.Stats = new CharacterStats(s.MaxHealth, s.HealthRegen, s.MoveSpeed, s.AttackPowerMultiplier + (Cfg().IgnitionAttackPowerPercent / 100f), s.Armor, s.CritChance, s.CritDamageMultiplier, s.CooldownReduction, s.AreaMultiplier, s.ProjectileSpeedMultiplier, s.ExtraProjectiles, s.PickupRadius);
            });

            rewardManager.RegisterPassive("passive_overcharge", "과전류의 핵", "전기 마법 공격 시 적을 7초간 감전시키며 쿨타임 추가 감소 (연쇄번개 진화 재료)", 5, (p, lv) =>
            {
                var s = p.Stats;
                p.Stats = new CharacterStats(s.MaxHealth, s.HealthRegen, s.MoveSpeed, s.AttackPowerMultiplier, s.Armor, s.CritChance, s.CritDamageMultiplier, s.CooldownReduction + (Cfg().OverchargeCooldownReductionPercent / 100f), s.AreaMultiplier, s.ProjectileSpeedMultiplier, s.ExtraProjectiles, s.PickupRadius);
            });

            rewardManager.RegisterPassive("passive_crit", "치명타의 눈", "크리티컬 확률 & 크리티컬 데미지 증가 (글레이브 진화 재료)", 5, (p, lv) =>
            {
                var s = p.Stats;
                var cfg = Cfg();
                p.Stats = new CharacterStats(s.MaxHealth, s.HealthRegen, s.MoveSpeed, s.AttackPowerMultiplier, s.Armor, s.CritChance + (cfg.CritEyeChancePercent / 100f), s.CritDamageMultiplier + (cfg.CritEyeDamageMultiplierPercent / 100f), s.CooldownReduction, s.AreaMultiplier, s.ProjectileSpeedMultiplier, s.ExtraProjectiles, s.PickupRadius);
            });
        }

        public static void RegisterAllEvolutions(SkillEvolutionManager evolutionManager)
        {
            // Warrior Evolutions (3)
            evolutionManager.RegisterRecipe(new SkillEvolutionRecipe(
                "slash", "passive_fang", "blood_eater", "블러드 이터",
                () => {
                    var cfg = SkillConfigRepository.Instance.GetConfig();
                    return new CompositeSkill("blood_eater", "블러드 이터", new CooldownTrigger(cfg.BloodEater.Cooldown), new ClosestEnemyTargeter(), new BloodEaterEffect(cfg.BloodEater.Damage, cfg.BloodEater.Radius, cfg.BloodEater.HealAmount, cfg.BloodEater.ArcAngle));
                }));

            evolutionManager.RegisterRecipe(new SkillEvolutionRecipe(
                "whirlwind", "passive_feather", "tempest_whirlwind", "템페스트 휠윈드",
                () => {
                    var cfg = SkillConfigRepository.Instance.GetConfig();
                    return new CompositeSkill("tempest_whirlwind", "템페스트 휠윈드", new CooldownTrigger(cfg.TempestWhirlwind.Cooldown), new ClosestEnemyTargeter(), new TempestWhirlwindEffect(cfg.TempestWhirlwind.Damage, cfg.TempestWhirlwind.Radius, cfg.TempestWhirlwind.SlashWaveCount));
                }));

            evolutionManager.RegisterRecipe(new SkillEvolutionRecipe(
                "ground_stomp", "passive_armor", "earthshaker", "어스셰이커 파쇄",
                () => {
                    var cfg = SkillConfigRepository.Instance.GetConfig();
                    return new CompositeSkill("earthshaker", "어스셰이커 파쇄", new CooldownTrigger(cfg.Earthshaker.Cooldown), new ClosestEnemyTargeter(), new EarthshakerEffect(cfg.Earthshaker.Damage, cfg.Earthshaker.Radius, cfg.Earthshaker.FissureCount));
                }));

            // Ranger Evolutions (3)
            evolutionManager.RegisterRecipe(new SkillEvolutionRecipe(
                "bow", "passive_feather", "storm_bow", "폭풍의 활",
                () => {
                    var cfg = SkillConfigRepository.Instance.GetConfig();
                    return new CompositeSkill("storm_bow", "폭풍의 활", new CooldownTrigger(cfg.StormBow.Cooldown), new ClosestEnemyTargeter(), new StormArrowEffect(cfg.StormBow.ArrowDamage, cfg.StormBow.ExplosionDamage, cfg.StormBow.ExplosionRadius, cfg.StormBow.Speed, cfg.StormBow.ArrowCount, cfg.StormBow.SpreadAngle));
                }));

            evolutionManager.RegisterRecipe(new SkillEvolutionRecipe(
                "glaive", "passive_crit", "phantom_glaive", "팬텀 글레이브",
                () => {
                    var cfg = SkillConfigRepository.Instance.GetConfig();
                    return new CompositeSkill("phantom_glaive", "팬텀 글레이브", new CooldownTrigger(cfg.PhantomGlaive.Cooldown), new ClosestEnemyTargeter(), new PhantomGlaiveEffect(cfg.PhantomGlaive.Damage, cfg.PhantomGlaive.Distance, cfg.PhantomGlaive.Speed, cfg.PhantomGlaive.PhantomCount, cfg.PhantomGlaive.BladeScale), range: cfg.PhantomGlaive.Distance);
                }));

            evolutionManager.RegisterRecipe(new SkillEvolutionRecipe(
                "arrow_rain", "passive_ring", "stellar_rain", "스텔라 레인",
                () => {
                    var cfg = SkillConfigRepository.Instance.GetConfig();
                    return new CompositeSkill("stellar_rain", "스텔라 레인", new CooldownTrigger(cfg.StellarRain.Cooldown), new ClosestEnemyTargeter(), new StellarRainEffect(cfg.StellarRain.Damage, cfg.StellarRain.Radius, cfg.StellarRain.ArrowCount, cfg.StellarRain.Duration), range: 8.0f);
                }));

            // Wizard Evolutions (3)
            evolutionManager.RegisterRecipe(new SkillEvolutionRecipe(
                "fireball", "passive_rune", "meteor_strike", "인페르노 화염구",
                () => {
                    var cfg = SkillConfigRepository.Instance.GetConfig();
                    return new CompositeSkill("meteor_strike", "인페르노 화염구", new CooldownTrigger(cfg.MeteorStrike.Cooldown), new ClosestEnemyTargeter(), new MeteorStrikeEffect(cfg.MeteorStrike.Damage, cfg.MeteorStrike.Radius, cfg.MeteorStrike.Speed, cfg.MeteorStrike.FireballCount, cfg.MeteorStrike.PierceCount), range: 9.0f);
                }));

            evolutionManager.RegisterRecipe(new SkillEvolutionRecipe(
                "chain_lightning", "passive_overcharge", "gigastorm_lightning", "기가스톰 체인",
                () => {
                    var cfg = SkillConfigRepository.Instance.GetConfig();
                    return new CompositeSkill("gigastorm_lightning", "기가스톰 체인", new CooldownTrigger(cfg.GigastormLightning.Cooldown), new ClosestEnemyTargeter(), new GigastormLightningEffect(cfg.GigastormLightning.Damage, cfg.GigastormLightning.ChainCount, cfg.GigastormLightning.JumpRadius, cfg.GigastormLightning.SparkRadius, cfg.GigastormLightning.StreamCount), range: 8.5f);
                }));

            evolutionManager.RegisterRecipe(new SkillEvolutionRecipe(
                "frost_nova", "passive_heart", "blizzard_nova", "블리자드 노바",
                () => {
                    var cfg = SkillConfigRepository.Instance.GetConfig();
                    return new CompositeSkill("blizzard_nova", "블리자드 노바", new CooldownTrigger(cfg.BlizzardNova.Cooldown), new ClosestEnemyTargeter(), new BlizzardNovaEffect(cfg.BlizzardNova.Damage, cfg.BlizzardNova.Radius, cfg.BlizzardNova.ShardCount), range: cfg.BlizzardNova.Radius);
                }));
        }
    }
}
