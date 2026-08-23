using System;
using System.Collections.Generic;

namespace HappyShoot.Domain.Skills
{
    [Serializable]
    public class SkillLevelCustomData
    {
        public string SkillId;
        public int Level;
        public float Damage;
        public float Radius;
        public float Cooldown;
        public float Speed;
        public int Count;
        public float Duration;
        public float ExtraParam1;
        public bool HasCustomValues;
    }

    [Serializable]
    public class SkillConfigData
    {
        // 1. Warrior
        public SlashConfig Slash = new SlashConfig();
        public GroundStompConfig GroundStomp = new GroundStompConfig();
        public WhirlwindConfig Whirlwind = new WhirlwindConfig();

        // 2. Ranger
        public BowConfig Bow = new BowConfig();
        public GlaiveConfig Glaive = new GlaiveConfig();
        public ArrowRainConfig ArrowRain = new ArrowRainConfig();

        // 3. Wizard
        public FireballConfig Fireball = new FireballConfig();
        public FrostNovaConfig FrostNova = new FrostNovaConfig();
        public ChainLightningConfig ChainLightning = new ChainLightningConfig();

        // 4. Shared
        public OrbitalConfig Orbital = new OrbitalConfig();

        // 5. Ultimate Evolutions (9 Total)
        public BloodEaterConfig BloodEater = new BloodEaterConfig();
        public TempestWhirlwindConfig TempestWhirlwind = new TempestWhirlwindConfig();
        public EarthshakerConfig Earthshaker = new EarthshakerConfig();
        public StormBowConfig StormBow = new StormBowConfig();
        public PhantomGlaiveConfig PhantomGlaive = new PhantomGlaiveConfig();
        public StellarRainConfig StellarRain = new StellarRainConfig();
        public MeteorStrikeConfig MeteorStrike = new MeteorStrikeConfig();
        public GigastormLightningConfig GigastormLightning = new GigastormLightningConfig();
        public BlizzardNovaConfig BlizzardNova = new BlizzardNovaConfig();

        // 6. Experience & Leveling Tuning
        public ExpConfig Exp = new ExpConfig();

        // 7. Monster Stats Tuning
        public HappyShoot.Domain.Entities.MonsterTuningConfigData Monsters = new HappyShoot.Domain.Entities.MonsterTuningConfigData();

        // 8. Player Critical & Core Stats Tuning
        public CritStatConfig CritStat = new CritStatConfig();

        // 9. Explicit Level-by-Level Tunings (L1~L5)
        public List<SkillLevelCustomData> LevelTunings = new List<SkillLevelCustomData>();
    }

    [Serializable]
    public class CritStatConfig
    {
        public float CritChance = 0.10f;
        public float CritDamageMultiplier = 1.50f;
        public float AttackPowerMultiplier = 1.00f;
        public float MoveSpeed = 5.00f;
        public float Armor = 0f;
        public float CooldownReduction = 0f;
        public bool IsCustom = false;
    }

    [Serializable]
    public class SlashConfig
    {
        public float Damage = 35f;
        public float Radius = 2.5f;
        public float ArcAngle = 150f;
        public float Cooldown = 1.2f;
        public bool EnableCameraShake = false;
    }

    [Serializable]
    public class GroundStompConfig
    {
        public float Damage = 32f;
        public float Radius = 2.2f;
        public float Cooldown = 1.4f;
        public bool EnableCameraShake = false;
    }

    [Serializable]
    public class WhirlwindConfig
    {
        public float Damage = 30f;
        public float Radius = 2.2f;
        public float Cooldown = 1.8f;
        public bool EnableCameraShake = false;
    }

    [Serializable]
    public class BowConfig
    {
        public float Damage = 25f;
        public float Speed = 16f;
        public int ArrowCount = 1;
        public float SpreadAngle = 28f;
        public float Cooldown = 0.8f;
        public bool EnableCameraShake = false;
    }

    [Serializable]
    public class GlaiveConfig
    {
        public float Damage = 28f;
        public float Speed = 15f;
        public float Distance = 8.5f;
        public int GlaiveCount = 1;
        public float Cooldown = 2.0f;
        public bool EnableCameraShake = false;
    }

    [Serializable]
    public class ArrowRainConfig
    {
        public float Damage = 24f;
        public float Radius = 2.0f;
        public float Duration = 1.5f;
        public int ArrowCount = 20;
        public float Cooldown = 2.2f;
        public bool EnableCameraShake = false;
    }

    [Serializable]
    public class FireballConfig
    {
        public float Damage = 35f;
        public float Radius = 1.6f;
        public float Speed = 14f;
        public int FireballCount = 1;
        public float Cooldown = 1.2f;
        public bool EnableCameraShake = false;
    }

    [Serializable]
    public class FrostNovaConfig
    {
        public float Damage = 28f;
        public float Radius = 2.8f;
        public float ChillDuration = 3.5f;
        public float Cooldown = 1.8f;
        public bool EnableCameraShake = false;
    }

    [Serializable]
    public class ChainLightningConfig
    {
        public float Damage = 30f;
        public int ChainCount = 4;
        public float JumpRadius = 4.0f;
        public float Cooldown = 2.0f;
        public bool EnableCameraShake = false;
    }

    [Serializable]
    public class OrbitalConfig
    {
        public float Damage = 25f;
        public float Radius = 2.0f;
        public float RotationSpeed = 4.18879f;
        public int BladeCount = 2;
        public float Cooldown = 0.20f;
        public bool EnableCameraShake = false;
    }

    [Serializable]
    public class BloodEaterConfig
    {
        public float Damage = 85f;
        public float Radius = 4.8f;
        public float HealAmount = 2.0f;
        public float Cooldown = 0.85f;
        public bool EnableCameraShake = false;
    }

    [Serializable]
    public class StormBowConfig
    {
        public float ArrowDamage = 65f;
        public float ExplosionDamage = 45f;
        public float ExplosionRadius = 1.6f;
        public float Speed = 20f;
        public int ArrowCount = 5;
        public float SpreadAngle = 36f;
        public float Cooldown = 1.6f;
        public bool EnableCameraShake = false;
    }

    [Serializable]
    public class MeteorStrikeConfig
    {
        public float Damage = 220f;
        public float Radius = 6.0f;
        public float Cooldown = 1.0f;
        public bool EnableCameraShake = false;
    }

    [Serializable]
    public class TempestWhirlwindConfig
    {
        public float Damage = 75f;
        public float Radius = 4.2f;
        public int SlashWaveCount = 4;
        public float Cooldown = 1.1f;
        public bool EnableCameraShake = false;
    }

    [Serializable]
    public class EarthshakerConfig
    {
        public float Damage = 80f;
        public float Radius = 4.8f;
        public int FissureCount = 4;
        public float Cooldown = 1.6f;
        public bool EnableCameraShake = false;
    }

    [Serializable]
    public class PhantomGlaiveConfig
    {
        public float Damage = 60f;
        public float Distance = 11.0f;
        public float Speed = 17.0f;
        public int PhantomCount = 2;
        public float Cooldown = 1.3f;
        public bool EnableCameraShake = false;
    }

    [Serializable]
    public class StellarRainConfig
    {
        public float Damage = 75f;
        public float Radius = 5.0f;
        public int ArrowCount = 60;
        public float Cooldown = 2.2f;
        public bool EnableCameraShake = false;
    }

    [Serializable]
    public class GigastormLightningConfig
    {
        public float Damage = 85f;
        public int ChainCount = 10;
        public float JumpRadius = 7.5f;
        public float SparkRadius = 2.2f;
        public float Cooldown = 1.2f;
        public bool EnableCameraShake = false;
    }

    [Serializable]
    public class BlizzardNovaConfig
    {
        public float Damage = 70f;
        public float Radius = 5.2f;
        public int ShardCount = 8;
        public float Cooldown = 1.8f;
        public bool EnableCameraShake = false;
    }

    [Serializable]
    public class ExpConfig
    {
        public float GemExpMultiplier = 1.0f; // 경험치 알 하나당 획득 경험치 배율
        public int BaseRequiredExp = 4;       // 1레벨업 기본 필요 경험치
        public float ExpGrowthFactor = 0.85f; // 레벨별 필요 경험치 증가율
        public bool EnableLevelExpScaling = true; // 레벨업 경험치 증가율 비례 최대 몹 수 증가 활성화
        public float MobScalingRatio = 0.30f;     // 경험치 증가량 중 몹 수 반영 비율 (30%)
        public int MaxMonsterCapLimit = 1200; // 최대 몬스터 수 상한선 (기본 1200)
    }
}
