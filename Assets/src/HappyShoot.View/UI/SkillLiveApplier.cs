using HappyShoot.Domain.Entities;
using HappyShoot.Domain.Skills;
using HappyShoot.Domain.Skills.Effects;
using HappyShoot.Domain.Skills.Triggers;

namespace HappyShoot.View.UI
{
    /// <summary>
    /// Live hot-reloading applier that syncs between PlayerEntity active skills and SkillConfigData.
    /// Strictly modular and under 500 lines.
    /// </summary>
    public static class SkillLiveApplier
    {
        public static void ApplyLive(PlayerEntity player, string skillId, SkillConfigData config, bool isInfiniteSpam)
        {
            if (player == null || config == null || string.IsNullOrEmpty(skillId)) return;
            var skill = player.GetSkill(skillId) as CompositeSkill;
            if (skill == null) return;

            float cooldown(float baseCd) => isInfiniteSpam ? 0.06f : baseCd;

            switch (skillId)
            {
                case "slash":
                    if (skill.Effect is GreatswordSlashEffect slash)
                    {
                        slash.BaseDamage = config.Slash.Damage;
                        slash.Radius = config.Slash.Radius;
                        slash.ArcAngleDegrees = config.Slash.ArcAngle;
                    }
                    if (skill.Trigger is CooldownTrigger cdSlash) cdSlash.Cooldown = cooldown(config.Slash.Cooldown);
                    break;
                case "ground_stomp":
                    if (skill.Effect is GroundStompEffect stomp)
                    {
                        stomp.BaseDamage = config.GroundStomp.Damage;
                        stomp.StompRadius = config.GroundStomp.Radius;
                    }
                    if (skill.Trigger is CooldownTrigger cdStomp) cdStomp.Cooldown = cooldown(config.GroundStomp.Cooldown);
                    break;
                case "whirlwind":
                    if (skill.Effect is WhirlwindEffect ww)
                    {
                        ww.BaseDamage = config.Whirlwind.Damage;
                        ww.Radius = config.Whirlwind.Radius;
                    }
                    if (skill.Trigger is CooldownTrigger cdWw) cdWw.Cooldown = cooldown(config.Whirlwind.Cooldown);
                    break;
                case "bow":
                    if (skill.Effect is PiercingArrowEffect bow)
                    {
                        bow.BaseDamage = config.Bow.Damage;
                        bow.Speed = config.Bow.Speed;
                        bow.ArrowCount = config.Bow.ArrowCount;
                        bow.SpreadAngleDeg = config.Bow.SpreadAngle;
                    }
                    if (skill.Trigger is CooldownTrigger cdBow) cdBow.Cooldown = cooldown(config.Bow.Cooldown);
                    break;
                case "glaive":
                    if (skill.Effect is WindGlaiveEffect glaive)
                    {
                        glaive.BaseDamage = config.Glaive.Damage;
                        glaive.Speed = config.Glaive.Speed;
                        glaive.MaxDistance = config.Glaive.Distance;
                        glaive.GlaiveCount = config.Glaive.GlaiveCount;
                    }
                    if (skill.Trigger is CooldownTrigger cdGlaive) cdGlaive.Cooldown = cooldown(config.Glaive.Cooldown);
                    break;
                case "arrow_rain":
                    if (skill.Effect is ArrowRainEffect ar)
                    {
                        ar.BaseDamage = config.ArrowRain.Damage;
                        ar.Radius = config.ArrowRain.Radius;
                        ar.Duration = config.ArrowRain.Duration;
                        ar.ArrowCount = config.ArrowRain.ArrowCount;
                    }
                    if (skill.Trigger is CooldownTrigger cdAr) cdAr.Cooldown = cooldown(config.ArrowRain.Cooldown);
                    break;
                case "fireball":
                    if (skill.Effect is FireballEffect fb)
                    {
                        fb.BaseDamage = config.Fireball.Damage;
                        fb.Radius = config.Fireball.Radius;
                        fb.Speed = config.Fireball.Speed;
                        fb.FireballCount = config.Fireball.FireballCount;
                    }
                    if (skill.Trigger is CooldownTrigger cdFb) cdFb.Cooldown = cooldown(config.Fireball.Cooldown);
                    break;
                case "frost_nova":
                    if (skill.Effect is FrostNovaEffect fn)
                    {
                        fn.BaseDamage = config.FrostNova.Damage;
                        fn.Radius = config.FrostNova.Radius;
                        fn.ChillDuration = config.FrostNova.ChillDuration;
                    }
                    if (skill.Trigger is CooldownTrigger cdFn) cdFn.Cooldown = cooldown(config.FrostNova.Cooldown);
                    break;
                case "chain_lightning":
                    if (skill.Effect is ChainLightningEffect cl)
                    {
                        cl.BaseDamage = config.ChainLightning.Damage;
                        cl.ChainCount = config.ChainLightning.ChainCount;
                        cl.JumpRadius = config.ChainLightning.JumpRadius;
                    }
                    if (skill.Trigger is CooldownTrigger cdCl) cdCl.Cooldown = cooldown(config.ChainLightning.Cooldown);
                    break;
                case "orbital":
                    if (skill.Effect is OrbitingBladesEffect orb)
                    {
                        orb.BaseDamage = config.Orbital.Damage;
                        orb.OrbitRadius = config.Orbital.Radius;
                        orb.RotationSpeed = config.Orbital.RotationSpeed;
                        orb.BladeCount = config.Orbital.BladeCount;
                    }
                    if (skill.Trigger is CooldownTrigger cdOrb) cdOrb.Cooldown = cooldown(config.Orbital.Cooldown);
                    break;
                case "blood_eater":
                    if (skill.Effect is BloodEaterEffect be)
                    {
                        be.BaseDamage = config.BloodEater.Damage;
                        be.Radius = config.BloodEater.Radius;
                        be.HealAmount = config.BloodEater.HealAmount;
                    }
                    if (skill.Trigger is CooldownTrigger cdBe) cdBe.Cooldown = cooldown(config.BloodEater.Cooldown);
                    break;
                case "storm_bow":
                    if (skill.Effect is StormArrowEffect sb)
                    {
                        sb.ArrowDamage = config.StormBow.ArrowDamage;
                        sb.ExplosionDamage = config.StormBow.ExplosionDamage;
                        sb.ExplosionRadius = config.StormBow.ExplosionRadius;
                        sb.Speed = config.StormBow.Speed;
                        sb.BaseArrowCount = config.StormBow.ArrowCount;
                        sb.SpreadAngleDeg = config.StormBow.SpreadAngle;
                    }
                    if (skill.Trigger is CooldownTrigger cdSb) cdSb.Cooldown = cooldown(config.StormBow.Cooldown);
                    break;
                case "meteor_strike":
                    if (skill.Effect is MeteorStrikeEffect ms)
                    {
                        ms.BaseDamage = config.MeteorStrike.Damage;
                        ms.ExplosionRadius = config.MeteorStrike.Radius;
                    }
                    if (skill.Trigger is CooldownTrigger cdMs) cdMs.Cooldown = cooldown(config.MeteorStrike.Cooldown);
                    break;
                case "tempest_whirlwind":
                    if (skill.Effect is TempestWhirlwindEffect tw)
                    {
                        tw.BaseDamage = config.TempestWhirlwind.Damage;
                        tw.Radius = config.TempestWhirlwind.Radius;
                        tw.SlashWaveCount = config.TempestWhirlwind.SlashWaveCount;
                    }
                    if (skill.Trigger is CooldownTrigger cdTw) cdTw.Cooldown = cooldown(config.TempestWhirlwind.Cooldown);
                    break;
                case "earthshaker":
                    if (skill.Effect is EarthshakerEffect es)
                    {
                        es.BaseDamage = config.Earthshaker.Damage;
                        es.Radius = config.Earthshaker.Radius;
                        es.FissureCount = config.Earthshaker.FissureCount;
                    }
                    if (skill.Trigger is CooldownTrigger cdEs) cdEs.Cooldown = cooldown(config.Earthshaker.Cooldown);
                    break;
                case "phantom_glaive":
                    if (skill.Effect is PhantomGlaiveEffect pg)
                    {
                        pg.BaseDamage = config.PhantomGlaive.Damage;
                        pg.MaxDistance = config.PhantomGlaive.Distance;
                        pg.Speed = config.PhantomGlaive.Speed;
                        pg.PhantomCount = config.PhantomGlaive.PhantomCount;
                    }
                    if (skill.Trigger is CooldownTrigger cdPg) cdPg.Cooldown = cooldown(config.PhantomGlaive.Cooldown);
                    break;
                case "stellar_rain":
                    if (skill.Effect is StellarRainEffect sr)
                    {
                        sr.BaseDamage = config.StellarRain.Damage;
                        sr.Radius = config.StellarRain.Radius;
                        sr.ArrowCount = config.StellarRain.ArrowCount;
                    }
                    if (skill.Trigger is CooldownTrigger cdSr) cdSr.Cooldown = cooldown(config.StellarRain.Cooldown);
                    break;
                case "gigastorm_lightning":
                    if (skill.Effect is GigastormLightningEffect gl)
                    {
                        gl.BaseDamage = config.GigastormLightning.Damage;
                        gl.ChainCount = config.GigastormLightning.ChainCount;
                        gl.ChainRange = config.GigastormLightning.JumpRadius;
                        gl.SparkRadius = config.GigastormLightning.SparkRadius;
                    }
                    if (skill.Trigger is CooldownTrigger cdGl) cdGl.Cooldown = cooldown(config.GigastormLightning.Cooldown);
                    break;
                case "blizzard_nova":
                    if (skill.Effect is BlizzardNovaEffect bn)
                    {
                        bn.BaseDamage = config.BlizzardNova.Damage;
                        bn.Radius = config.BlizzardNova.Radius;
                        bn.ShardCount = config.BlizzardNova.ShardCount;
                    }
                    if (skill.Trigger is CooldownTrigger cdBn) cdBn.Cooldown = cooldown(config.BlizzardNova.Cooldown);
                    break;
            }
        }

        public static void PullSkillStatsToConfig(PlayerEntity player, string skillId, SkillConfigData config)
        {
            if (player == null || config == null || string.IsNullOrEmpty(skillId)) return;
            var skill = player.GetSkill(skillId) as CompositeSkill;
            if (skill == null) return;

            switch (skillId)
            {
                case "slash":
                    if (skill.Effect is GreatswordSlashEffect slash)
                    {
                        config.Slash.Damage = slash.BaseDamage;
                        config.Slash.Radius = slash.Radius;
                        config.Slash.ArcAngle = slash.ArcAngleDegrees;
                    }
                    if (skill.Trigger is CooldownTrigger cdSlash) config.Slash.Cooldown = cdSlash.Cooldown;
                    break;
                case "ground_stomp":
                    if (skill.Effect is GroundStompEffect stomp)
                    {
                        config.GroundStomp.Damage = stomp.BaseDamage;
                        config.GroundStomp.Radius = stomp.StompRadius;
                    }
                    if (skill.Trigger is CooldownTrigger cdStomp) config.GroundStomp.Cooldown = cdStomp.Cooldown;
                    break;
                case "whirlwind":
                    if (skill.Effect is WhirlwindEffect ww)
                    {
                        config.Whirlwind.Damage = ww.BaseDamage;
                        config.Whirlwind.Radius = ww.Radius;
                    }
                    if (skill.Trigger is CooldownTrigger cdWw) config.Whirlwind.Cooldown = cdWw.Cooldown;
                    break;
                case "bow":
                    if (skill.Effect is PiercingArrowEffect bow)
                    {
                        config.Bow.Damage = bow.BaseDamage;
                        config.Bow.Speed = bow.Speed;
                        config.Bow.ArrowCount = bow.ArrowCount;
                        config.Bow.SpreadAngle = bow.SpreadAngleDeg;
                    }
                    if (skill.Trigger is CooldownTrigger cdBow) config.Bow.Cooldown = cdBow.Cooldown;
                    break;
                case "glaive":
                    if (skill.Effect is WindGlaiveEffect glaive)
                    {
                        config.Glaive.Damage = glaive.BaseDamage;
                        config.Glaive.Speed = glaive.Speed;
                        config.Glaive.Distance = glaive.MaxDistance;
                        config.Glaive.GlaiveCount = glaive.GlaiveCount;
                    }
                    if (skill.Trigger is CooldownTrigger cdGlaive) config.Glaive.Cooldown = cdGlaive.Cooldown;
                    break;
                case "arrow_rain":
                    if (skill.Effect is ArrowRainEffect ar)
                    {
                        config.ArrowRain.Damage = ar.BaseDamage;
                        config.ArrowRain.Radius = ar.Radius;
                        config.ArrowRain.Duration = ar.Duration;
                        config.ArrowRain.ArrowCount = ar.ArrowCount;
                    }
                    if (skill.Trigger is CooldownTrigger cdAr) config.ArrowRain.Cooldown = cdAr.Cooldown;
                    break;
                case "fireball":
                    if (skill.Effect is FireballEffect fb)
                    {
                        config.Fireball.Damage = fb.BaseDamage;
                        config.Fireball.Radius = fb.Radius;
                        config.Fireball.Speed = fb.Speed;
                        config.Fireball.FireballCount = fb.FireballCount;
                    }
                    if (skill.Trigger is CooldownTrigger cdFb) config.Fireball.Cooldown = cdFb.Cooldown;
                    break;
                case "frost_nova":
                    if (skill.Effect is FrostNovaEffect fn)
                    {
                        config.FrostNova.Damage = fn.BaseDamage;
                        config.FrostNova.Radius = fn.Radius;
                        config.FrostNova.ChillDuration = fn.ChillDuration;
                    }
                    if (skill.Trigger is CooldownTrigger cdFn) config.FrostNova.Cooldown = cdFn.Cooldown;
                    break;
                case "chain_lightning":
                    if (skill.Effect is ChainLightningEffect cl)
                    {
                        config.ChainLightning.Damage = cl.BaseDamage;
                        config.ChainLightning.ChainCount = cl.ChainCount;
                        config.ChainLightning.JumpRadius = cl.JumpRadius;
                    }
                    if (skill.Trigger is CooldownTrigger cdCl) config.ChainLightning.Cooldown = cdCl.Cooldown;
                    break;
                case "orbital":
                    if (skill.Effect is OrbitingBladesEffect orb)
                    {
                        config.Orbital.Damage = orb.BaseDamage;
                        config.Orbital.Radius = orb.OrbitRadius;
                        config.Orbital.RotationSpeed = orb.RotationSpeed;
                        config.Orbital.BladeCount = orb.BladeCount;
                    }
                    if (skill.Trigger is CooldownTrigger cdOrb) config.Orbital.Cooldown = cdOrb.Cooldown;
                    break;
                case "blood_eater":
                    if (skill.Effect is BloodEaterEffect be)
                    {
                        config.BloodEater.Damage = be.BaseDamage;
                        config.BloodEater.Radius = be.Radius;
                        config.BloodEater.HealAmount = be.HealAmount;
                    }
                    if (skill.Trigger is CooldownTrigger cdBePull) config.BloodEater.Cooldown = cdBePull.Cooldown;
                    break;
                case "storm_bow":
                    if (skill.Effect is StormArrowEffect sb)
                    {
                        config.StormBow.ArrowDamage = sb.ArrowDamage;
                        config.StormBow.ExplosionDamage = sb.ExplosionDamage;
                        config.StormBow.ExplosionRadius = sb.ExplosionRadius;
                        config.StormBow.Speed = sb.Speed;
                        config.StormBow.ArrowCount = sb.BaseArrowCount;
                        config.StormBow.SpreadAngle = sb.SpreadAngleDeg;
                    }
                    if (skill.Trigger is CooldownTrigger cdSbPull) config.StormBow.Cooldown = cdSbPull.Cooldown;
                    break;
                case "meteor_strike":
                    if (skill.Effect is MeteorStrikeEffect ms)
                    {
                        config.MeteorStrike.Damage = ms.BaseDamage;
                        config.MeteorStrike.Radius = ms.ExplosionRadius;
                    }
                    if (skill.Trigger is CooldownTrigger cdMsPull) config.MeteorStrike.Cooldown = cdMsPull.Cooldown;
                    break;
                case "tempest_whirlwind":
                    if (skill.Effect is TempestWhirlwindEffect tw)
                    {
                        config.TempestWhirlwind.Damage = tw.BaseDamage;
                        config.TempestWhirlwind.Radius = tw.Radius;
                        config.TempestWhirlwind.SlashWaveCount = tw.SlashWaveCount;
                    }
                    if (skill.Trigger is CooldownTrigger cdTwPull) config.TempestWhirlwind.Cooldown = cdTwPull.Cooldown;
                    break;
                case "earthshaker":
                    if (skill.Effect is EarthshakerEffect es)
                    {
                        config.Earthshaker.Damage = es.BaseDamage;
                        config.Earthshaker.Radius = es.Radius;
                        config.Earthshaker.FissureCount = es.FissureCount;
                    }
                    if (skill.Trigger is CooldownTrigger cdEsPull) config.Earthshaker.Cooldown = cdEsPull.Cooldown;
                    break;
                case "phantom_glaive":
                    if (skill.Effect is PhantomGlaiveEffect pg)
                    {
                        config.PhantomGlaive.Damage = pg.BaseDamage;
                        config.PhantomGlaive.Distance = pg.MaxDistance;
                        config.PhantomGlaive.Speed = pg.Speed;
                        config.PhantomGlaive.PhantomCount = pg.PhantomCount;
                    }
                    if (skill.Trigger is CooldownTrigger cdPgPull) config.PhantomGlaive.Cooldown = cdPgPull.Cooldown;
                    break;
                case "stellar_rain":
                    if (skill.Effect is StellarRainEffect sr)
                    {
                        config.StellarRain.Damage = sr.BaseDamage;
                        config.StellarRain.Radius = sr.Radius;
                        config.StellarRain.ArrowCount = sr.ArrowCount;
                    }
                    if (skill.Trigger is CooldownTrigger cdSrPull) config.StellarRain.Cooldown = cdSrPull.Cooldown;
                    break;
                case "gigastorm_lightning":
                    if (skill.Effect is GigastormLightningEffect gl)
                    {
                        config.GigastormLightning.Damage = gl.BaseDamage;
                        config.GigastormLightning.ChainCount = gl.ChainCount;
                        config.GigastormLightning.JumpRadius = gl.ChainRange;
                        config.GigastormLightning.SparkRadius = gl.SparkRadius;
                    }
                    if (skill.Trigger is CooldownTrigger cdGlPull) config.GigastormLightning.Cooldown = cdGlPull.Cooldown;
                    break;
                case "blizzard_nova":
                    if (skill.Effect is BlizzardNovaEffect bn)
                    {
                        config.BlizzardNova.Damage = bn.BaseDamage;
                        config.BlizzardNova.Radius = bn.Radius;
                        config.BlizzardNova.ShardCount = bn.ShardCount;
                    }
                    if (skill.Trigger is CooldownTrigger cdBnPull) config.BlizzardNova.Cooldown = cdBnPull.Cooldown;
                    break;
            }
        }
    }
}
