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

        // 5. Ultimate Evolutions
        public BloodEaterConfig BloodEater = new BloodEaterConfig();
        public StormBowConfig StormBow = new StormBowConfig();
        public MeteorStrikeConfig MeteorStrike = new MeteorStrikeConfig();

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
        public float Damage = 120f;
        public float Radius = 3.0f;
        public float Cooldown = 1.2f;
        public bool EnableCameraShake = false;
    }

    [Serializable]
    public class ExpConfig
    {
        public float GemExpMultiplier = 1.0f; // 경험치 알 하나당 획득 경험치 배율
        public int BaseRequiredExp = 4;       // 1레벨업 기본 필요 경험치 (기존 5 -> 4)
        public float ExpGrowthFactor = 0.85f; // 레벨별 필요 경험치 증가율 (기존 1.0 -> 0.85로 더 자주 레벨업)
    }
}
