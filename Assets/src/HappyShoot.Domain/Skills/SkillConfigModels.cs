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
        public float ExtraParam2;
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

        // 6. Passive Skills Tuning (9 Total)
        public PassiveConfigData Passives = new PassiveConfigData();

        // 7. Experience & Leveling Tuning
        public ExpConfig Exp = new ExpConfig();

        // 8. Monster Stats Tuning
        public HappyShoot.Domain.Entities.MonsterTuningConfigData Monsters = new HappyShoot.Domain.Entities.MonsterTuningConfigData();

        // 9. Player Critical & Core Stats Tuning
        public CritStatConfig CritStat = new CritStatConfig();

        // 10. Explicit Level-by-Level Tunings (L1~L5)
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
        public float CameraShakeScale = 0f;
        public bool EnableCameraShake { get => CameraShakeScale > 0.001f; set => CameraShakeScale = value ? 100f : 0f; }
    }

    [Serializable]
    public class GroundStompConfig
    {
        public float Damage = 32f;
        public float Radius = 2.2f;
        public float Cooldown = 1.4f;
        public float CameraShakeScale = 0f;
        public bool EnableCameraShake { get => CameraShakeScale > 0.001f; set => CameraShakeScale = value ? 100f : 0f; }
    }

    [Serializable]
    public class WhirlwindConfig
    {
        public float Damage = 30f;
        public float Radius = 2.2f;
        public float Cooldown = 1.8f;
        public float CameraShakeScale = 0f;
        public bool EnableCameraShake { get => CameraShakeScale > 0.001f; set => CameraShakeScale = value ? 100f : 0f; }
    }

    [Serializable]
    public class BowConfig
    {
        public float Damage = 25f;
        public float Speed = 16f;
        public int ArrowCount = 1;
        public float SpreadAngle = 28f;
        public float Cooldown = 0.8f;
        public float CameraShakeScale = 0f;
        public bool EnableCameraShake { get => CameraShakeScale > 0.001f; set => CameraShakeScale = value ? 100f : 0f; }
    }

    [Serializable]
    public class GlaiveConfig
    {
        public float Damage = 28f;
        public float Speed = 15f;
        public float Distance = 8.5f;
        public int GlaiveCount = 1;
        public float Cooldown = 2.0f;
        public float CameraShakeScale = 0f;
        public bool EnableCameraShake { get => CameraShakeScale > 0.001f; set => CameraShakeScale = value ? 100f : 0f; }
    }

    [Serializable]
    public class ArrowRainConfig
    {
        public float Damage = 24f;
        public float Radius = 2.0f;
        public float Duration = 1.5f;
        public int ArrowCount = 20;
        public float Cooldown = 2.2f;
        public float CameraShakeScale = 0f;
        public bool EnableCameraShake { get => CameraShakeScale > 0.001f; set => CameraShakeScale = value ? 100f : 0f; }
    }

    [Serializable]
    public class FireballConfig
    {
        public float Damage = 35f;
        public float Radius = 1.6f;
        public float Speed = 14f;
        public int FireballCount = 1;
        public float Cooldown = 1.2f;
        public float CameraShakeScale = 0f;
        public bool EnableCameraShake { get => CameraShakeScale > 0.001f; set => CameraShakeScale = value ? 100f : 0f; }
    }

    [Serializable]
    public class FrostNovaConfig
    {
        public float Damage = 28f;
        public float Radius = 2.8f;
        public float ChillDuration = 3.5f;
        public float Cooldown = 1.8f;
        public float CameraShakeScale = 0f;
        public bool EnableCameraShake { get => CameraShakeScale > 0.001f; set => CameraShakeScale = value ? 100f : 0f; }
    }

    [Serializable]
    public class ChainLightningConfig
    {
        public float Damage = 30f;
        public int ChainCount = 4;
        public float JumpRadius = 4.0f;
        public float Cooldown = 2.0f;
        public float CameraShakeScale = 0f;
        public bool EnableCameraShake { get => CameraShakeScale > 0.001f; set => CameraShakeScale = value ? 100f : 0f; }
    }

    [Serializable]
    public class OrbitalConfig
    {
        public float Damage = 25f;
        public float Radius = 2.0f;
        public float RotationSpeed = 4.18879f;
        public int BladeCount = 2;
        public float Cooldown = 0.20f;
        public float CameraShakeScale = 0f;
        public bool EnableCameraShake { get => CameraShakeScale > 0.001f; set => CameraShakeScale = value ? 100f : 0f; }
    }

    [Serializable]
    public class BloodEaterConfig
    {
        public float Damage = 85f;
        public float Radius = 4.8f;
        public float ArcAngle = 150f;
        public float HealAmount = 2.0f;
        public float Cooldown = 0.85f;
        public float CameraShakeScale = 0f;
        public bool EnableCameraShake { get => CameraShakeScale > 0.001f; set => CameraShakeScale = value ? 100f : 0f; }
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
        public float CameraShakeScale = 0f;
        public bool EnableCameraShake { get => CameraShakeScale > 0.001f; set => CameraShakeScale = value ? 100f : 0f; }
    }

    [Serializable]
    public class MeteorStrikeConfig
    {
        public float Damage = 85f;
        public float Radius = 2.8f;
        public float Speed = 15f;
        public int FireballCount = 3;
        public int PierceCount = 1;
        public float Cooldown = 1.4f;
        public float CameraShakeScale = 50f;
        public bool EnableCameraShake { get => CameraShakeScale > 0.001f; set => CameraShakeScale = value ? 100f : 0f; }
    }

    [Serializable]
    public class TempestWhirlwindConfig
    {
        public float Damage = 75f;
        public float Radius = 4.2f;
        public int SlashWaveCount = 4;
        public float Cooldown = 1.1f;
        public float CameraShakeScale = 0f;
        public bool EnableCameraShake { get => CameraShakeScale > 0.001f; set => CameraShakeScale = value ? 100f : 0f; }
    }

    [Serializable]
    public class EarthshakerConfig
    {
        public float Damage = 80f;
        public float Radius = 4.8f;
        public int FissureCount = 4;
        public float Cooldown = 1.6f;
        public float CameraShakeScale = 0f;
        public bool EnableCameraShake { get => CameraShakeScale > 0.001f; set => CameraShakeScale = value ? 100f : 0f; }
    }

    [Serializable]
    public class PhantomGlaiveConfig
    {
        public float Damage = 60f;
        public float Distance = 11.0f;
        public float Speed = 17.0f;
        public float BladeScale = 1.0f;
        public int PhantomCount = 2;
        public float Cooldown = 1.3f;
        public float CameraShakeScale = 0f;
        public bool EnableCameraShake { get => CameraShakeScale > 0.001f; set => CameraShakeScale = value ? 100f : 0f; }
    }

    [Serializable]
    public class StellarRainConfig
    {
        public float Damage = 75f;
        public float Radius = 5.0f;
        public float Duration = 2.0f;
        public int ArrowCount = 60;
        public float Cooldown = 2.2f;
        public float CameraShakeScale = 0f;
        public bool EnableCameraShake { get => CameraShakeScale > 0.001f; set => CameraShakeScale = value ? 100f : 0f; }
    }

    [Serializable]
    public class GigastormLightningConfig
    {
        public float Damage = 85f;
        public int ChainCount = 10;
        public float JumpRadius = 7.5f;
        public float SparkRadius = 2.2f;
        public float Cooldown = 1.2f;
        public float CameraShakeScale = 0f;
        public bool EnableCameraShake { get => CameraShakeScale > 0.001f; set => CameraShakeScale = value ? 100f : 0f; }
    }

    [Serializable]
    public class BlizzardNovaConfig
    {
        public float Damage = 70f;
        public float Radius = 5.2f;
        public int ShardCount = 8;
        public float Cooldown = 1.8f;
        public float CameraShakeScale = 0f;
        public bool EnableCameraShake { get => CameraShakeScale > 0.001f; set => CameraShakeScale = value ? 100f : 0f; }
    }

    [Serializable]
    public class ExpConfig
    {
        public float GemExpMultiplier = 1.0f; // 경험치 알 하나당 획득 경험치 배율
        public int BaseRequiredExp = 4;       // 1레벨업 기본 필요 경험치
        public float ExpGrowthFactor = 0.85f; // 레벨별 필요 경험치 증가율
        public bool EnableLevelExpScaling = true; // 레벨업 경험치 증가율 비례 최대 몹 수 증가 활성화
        public float MobScalingRatio = 0.30f;     // 경험치 증가량 중 몹 수 반영 비율 (30%)
        public float MobHpScalingRatio = 0.10f;    // 경험치 증가량 중 몹 체력 감쇠 반영 비율 (0%~50%, 기본 10%)
        public float MobHpMaxCapMultiplier = 5.0f; // 몹 최대 체력 배율 상한선 (1.0x ~ 10.0x, 기본 5.0x)
        public int MaxMonsterCapLimit = 1200; // 최대 몬스터 수 상한선 (기본 1200)

        // Juice & Game Feel Tuning
        public bool EnableHitStop = true;         // 타격 역경직(Hit-Stop) 활성화
        public float HitStopDuration = 0.035f;    // 역경직 지속 시간 (초, 0.00~0.15s)
        public float HitStopSlowScale = 0.20f;    // 역경직 슬로우 강도 (0.00~0.50, 부드러운 슬로우 모션)
        public float MasterCameraShakeScale = 100f; // 📳 마스터 카메라 셰이크 강도 배율 (0~100%)
    }

    [Serializable]
    public class PassiveConfigData
    {
        // 1. Vampire Fang (흡혈귀의 이빨)
        public float FangAttackPowerPercent = 15f; // Lv당 +15%

        // 2. Wind Feather (바람의 깃털)
        public float FeatherMoveSpeed = 0.6f; // Lv당 +0.6m/s
        public float FeatherProjSpeedPercent = 15f; // Lv당 +15%

        // 3. Mana Rune (마나 룬)
        public float RuneCooldownReductionPercent = 10f; // Lv당 +10%
        public float RuneAreaMultiplierPercent = 15f; // Lv당 +15%

        // 4. Iron Armor (강철 갑옷)
        public float ArmorAmount = 5f; // Lv당 +5

        // 5. Golden Ring (황금 반지)
        public float RingPickupRadius = 1.5f; // Lv당 +1.5m

        // 6. Heart Pendant (생명의 펜던트)
        public float HeartMaxHp = 30f; // Lv당 +30
        public float HeartHpRegen = 1.5f; // Lv당 +1.5 HP/s

        // 7. Ignition Flame (발화의 불꽃)
        public float IgnitionAttackPowerPercent = 10f; // Lv당 +10%

        // 8. Overcharge Core (과전류의 핵)
        public float OverchargeCooldownReductionPercent = 6f; // Lv당 +6%

        // 9. Hawk's Eye (치명타의 눈)
        public float CritEyeChancePercent = 8f; // Lv당 +8%
        public float CritEyeDamageMultiplierPercent = 5f; // Lv당 +5%
    }
}
