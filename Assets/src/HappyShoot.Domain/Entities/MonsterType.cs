namespace HappyShoot.Domain.Entities
{
    public enum MonsterType
    {
        Slime = 0,     // Standard melee (balanced HP, balanced speed)
        Bat = 1,       // Swarm flyer (low HP, very fast speed)
        Skeleton = 2,  // Ranged shooter (medium HP, keeps distance, shoots bones)
        Golem = 3,     // Heavy tank (very high HP, slow speed, high contact damage)
        Boss = 4       // Epic Boss (massive HP, rush patterns, barrage)
    }

    /// <summary>
    /// Pure C# configuration data describing a monster archetype.
    /// </summary>
    public class MonsterDefinition
    {
        public MonsterType Type { get; }
        public string Name { get; }
        public float BaseMaxHealth { get; }
        public float BaseMoveSpeed { get; }
        public float BaseDamage { get; }
        public float Radius { get; }
        public int ExpValue { get; }
        public int GoldValue { get; }
        public bool IsRanged { get; }
        public float PreferredDistance { get; }
        public float AttackInterval { get; }

        public MonsterDefinition(
            MonsterType type,
            string name,
            float baseMaxHealth,
            float baseMoveSpeed,
            float baseDamage,
            float radius,
            int expValue,
            int goldValue,
            bool isRanged = false,
            float preferredDistance = 0f,
            float attackInterval = 1.5f)
        {
            Type = type;
            Name = name;
            BaseMaxHealth = baseMaxHealth;
            BaseMoveSpeed = baseMoveSpeed;
            BaseDamage = baseDamage;
            Radius = radius;
            ExpValue = expValue;
            GoldValue = goldValue;
            IsRanged = isRanged;
            PreferredDistance = preferredDistance;
            AttackInterval = attackInterval;
        }

        public static MonsterDefinition Slime => new MonsterDefinition(
            MonsterType.Slime, "Slime", baseMaxHealth: 30f, baseMoveSpeed: 2.5f, baseDamage: 8f, radius: 0.4f, expValue: 1, goldValue: 1);

        public static MonsterDefinition Bat => new MonsterDefinition(
            MonsterType.Bat, "Bat", baseMaxHealth: 15f, baseMoveSpeed: 4.5f, baseDamage: 5f, radius: 0.3f, expValue: 1, goldValue: 1);

        public static MonsterDefinition Skeleton => new MonsterDefinition(
            MonsterType.Skeleton, "Skeleton", baseMaxHealth: 45f, baseMoveSpeed: 1.8f, baseDamage: 12f, radius: 0.45f, expValue: 2, goldValue: 2, isRanged: true, preferredDistance: 4.5f, attackInterval: 2.0f);

        public static MonsterDefinition Golem => new MonsterDefinition(
            MonsterType.Golem, "Golem", baseMaxHealth: 160f, baseMoveSpeed: 1.2f, baseDamage: 22f, radius: 0.7f, expValue: 4, goldValue: 5);

        public static MonsterDefinition CreateBoss(string name, float hp, float speed, float damage, int exp, int gold)
        {
            return new MonsterDefinition(MonsterType.Boss, name, hp, speed, damage, radius: 1.0f, exp, gold, isRanged: false, preferredDistance: 0f, attackInterval: 1.2f);
        }
    }
}
