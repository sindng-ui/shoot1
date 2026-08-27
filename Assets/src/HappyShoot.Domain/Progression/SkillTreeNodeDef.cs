using HappyShoot.Domain.Entities;

namespace HappyShoot.Domain.Progression
{
    /// <summary>
    /// Identifies the category of stat or effect a node applies.
    /// </summary>
    public enum NodeEffectType
    {
        // ── Core stat nodes ──
        MaxHealth,
        Armor,
        AttackSpeed,
        MoveSpeed,
        CritChance,
        CritDamage,
        ProjectileSpeed,
        CooldownReduction,
        AreaMultiplier,
        AttackPower,
        DodgeChance,
        ComboStatCdrArea,

        // ── Branch elemental nodes ──
        FireBurnOnHit,
        FireDeathExplosion,
        FireWhirlwindAura,
        FireGroundLava,
        IceChillOnHit,
        IceShatterExecute,
        IceStompFreeze,
        IceFrostCounter,
        LightningShockOnHit,
        LightningStormOverload,
        LightningWhirlwindDischarge,
        LightningThunderStrike,

        FireArrowBurn,
        FireCritExplosion,
        FireMeteorRain,
        FirePhoenixSummon,
        IceArrowChill,
        IceShardBurst,
        IceGlaiveFrost,
        IceAutoTurret,
        LightningArrowShock,
        LightningChainJump,
        LightningCritThunder,
        LightningFullPierce,

        FireballDotBoost,
        FireballAreaBoost,
        FireAutoMeteor,
        FireChainExplosion,
        IceNovaSlowBoost,
        IceShardOnThaw,
        IceChanceFreeze,
        IceFrostAura,
        LightningChainCountBoost,
        LightningChainOnKill,
        LightningChainOnHit,
        LightningShockShield
    }

    /// <summary>
    /// Immutable data definition for a single skill tree node.
    /// Kept lightweight: no delegates, no Unity references.
    /// </summary>
    public sealed class SkillTreeNodeDef
    {
        public readonly string Id;
        public readonly string Title;
        public readonly string Description;
        public readonly GemType GemType;
        public readonly int GemCost;
        public readonly int MaxLevel;
        public readonly BranchType Branch;
        public readonly NodeEffectType EffectType;
        public readonly float EffectValue;
        public readonly string[] PrerequisiteIds;
        public readonly CharacterClassType ClassType;

        public SkillTreeNodeDef(
            string id,
            string title,
            string description,
            GemType gemType,
            int gemCost,
            int maxLevel,
            BranchType branch,
            NodeEffectType effectType,
            float effectValue,
            string[] prerequisiteIds = null)
        {
            Id = id;
            Title = title;
            Description = description;
            GemType = gemType;
            GemCost = gemCost;
            MaxLevel = maxLevel;
            Branch = branch;
            EffectType = effectType;
            EffectValue = effectValue;
            PrerequisiteIds = prerequisiteIds ?? System.Array.Empty<string>();
            ClassType = gemType.ToClassType();
        }
    }
}
