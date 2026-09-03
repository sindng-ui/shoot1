namespace HappyShoot.Domain.Entities
{
    public enum MonsterType
    {
        Slime = 0,       // Standard melee (balanced HP, balanced speed)
        Bat = 1,         // Swarm flyer (low HP, very fast speed)
        Skeleton = 2,    // Ranged shooter (medium HP, keeps distance, shoots bones)
        Golem = 3,       // Heavy tank (very high HP, slow speed, high contact damage)
        Boss = 4,        // Epic Boss (massive HP, rush patterns, barrage)
        FireImp = 5,     // Phase 2 wave 1: fast flame imp (medium HP, fast, aggressive)
        ToxicSpider = 6, // Phase 2 wave 2: toxic spider (high HP, pack tactics)
        DarkKnight = 7,  // Phase 2 wave 3: armored dark knight (very high HP, slow, lethal)
        Wraith = 8,      // Phase 3 wave 1: ethereal ghost (fast speed, stealth zigzag)
        Necromancer = 9, // Phase 3 wave 2: death mage (shoots cursed soul orbs)
        Abomination = 10,// Phase 3 wave 3: flesh colossus (colossal HP & high damage)
        Reaper = 11,     // Phase 3 wave 4: scythe reaper (high speed dash & lethal damage)
        Boss3 = 12,      // Final Boss: Arch-Lich King
        Boss2 = 13       // Phase 2 Boss: Venom Queen Arachne
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
            MonsterType.Bat, "Bat", baseMaxHealth: 15f, baseMoveSpeed: 4.05f, baseDamage: 5f, radius: 0.3f, expValue: 1, goldValue: 1);

        public static MonsterDefinition Skeleton => new MonsterDefinition(
            MonsterType.Skeleton, "Skeleton", baseMaxHealth: 45f, baseMoveSpeed: 1.8f, baseDamage: 12f, radius: 0.45f, expValue: 2, goldValue: 2, isRanged: true, preferredDistance: 4.5f, attackInterval: 2.0f);

        public static MonsterDefinition Golem => new MonsterDefinition(
            MonsterType.Golem, "Golem", baseMaxHealth: 160f, baseMoveSpeed: 1.2f, baseDamage: 22f, radius: 0.7f, expValue: 4, goldValue: 5);

        // Phase 2 Wave 1: Fast flame imp - medium HP, very fast, aggressive
        public static MonsterDefinition FireImp => new MonsterDefinition(
            MonsterType.FireImp, "Fire Imp", baseMaxHealth: 150f, baseMoveSpeed: 3.8f, baseDamage: 18f, radius: 0.38f, expValue: 5, goldValue: 6);

        // Phase 2 Wave 2: Toxic spider - high HP, slower, comes in packs
        public static MonsterDefinition ToxicSpider => new MonsterDefinition(
            MonsterType.ToxicSpider, "Toxic Spider", baseMaxHealth: 280f, baseMoveSpeed: 2.2f, baseDamage: 20f, radius: 0.52f, expValue: 8, goldValue: 9);

        // Phase 2 Wave 3: Dark Knight - very high HP, armored, shoots dark slashes
        public static MonsterDefinition DarkKnight => new MonsterDefinition(
            MonsterType.DarkKnight, "Dark Knight", baseMaxHealth: 600f, baseMoveSpeed: 1.5f, baseDamage: 35f, radius: 0.75f, expValue: 14, goldValue: 18, isRanged: true, preferredDistance: 4.8f, attackInterval: 2.5f);

        // Phase 3 Wave 1: Wraith - ethereal ghost, fast speed, stealth zigzag
        public static MonsterDefinition Wraith => new MonsterDefinition(
            MonsterType.Wraith, "Wraith", baseMaxHealth: 450f, baseMoveSpeed: 3.6f, baseDamage: 25f, radius: 0.42f, expValue: 18, goldValue: 20);

        // Phase 3 Wave 2: Necromancer - death mage, shoots cursed soul orbs
        public static MonsterDefinition Necromancer => new MonsterDefinition(
            MonsterType.Necromancer, "Necromancer", baseMaxHealth: 700f, baseMoveSpeed: 1.8f, baseDamage: 30f, radius: 0.50f, expValue: 25, goldValue: 30, isRanged: true, preferredDistance: 5.0f, attackInterval: 2.2f);

        // Phase 3 Wave 3: Abomination - flesh colossus, colossal HP & high damage
        public static MonsterDefinition Abomination => new MonsterDefinition(
            MonsterType.Abomination, "Abomination", baseMaxHealth: 1500f, baseMoveSpeed: 1.3f, baseDamage: 55f, radius: 0.85f, expValue: 40, goldValue: 50);

        // Phase 3 Wave 4: Reaper - scythe reaper, high speed dash & lethal damage
        public static MonsterDefinition Reaper => new MonsterDefinition(
            MonsterType.Reaper, "Reaper", baseMaxHealth: 950f, baseMoveSpeed: 3.8f, baseDamage: 60f, radius: 0.55f, expValue: 45, goldValue: 55);

        public static MonsterDefinition CreateBoss(string name, float hp, float speed, float damage, int exp, int gold)
        {
            return new MonsterDefinition(MonsterType.Boss, name, hp, speed, damage, radius: 1.0f, exp, gold, isRanged: false, preferredDistance: 0f, attackInterval: 1.2f);
        }

        public static MonsterDefinition CreateBoss3(string name, float hp, float speed, float damage, int exp, int gold)
        {
            return new MonsterDefinition(MonsterType.Boss3, name, hp, speed, damage, radius: 1.2f, exp, gold, isRanged: true, preferredDistance: 4.5f, attackInterval: 1.0f);
        }

        public static MonsterDefinition CreateBoss2(string name, float hp, float speed, float damage, int exp, int gold)
        {
            return new MonsterDefinition(MonsterType.Boss2, name, hp, speed, damage, radius: 1.1f, exp, gold, isRanged: true, preferredDistance: 3.5f, attackInterval: 1.0f);
        }

        public static MonsterDefinition FromConfig(MonsterType type, MonsterTuningConfigData cfg)
        {
            if (cfg == null)
            {
                switch (type)
                {
                    case MonsterType.Bat: return Bat;
                    case MonsterType.Skeleton: return Skeleton;
                    case MonsterType.Golem: return Golem;
                    case MonsterType.FireImp: return FireImp;
                    case MonsterType.ToxicSpider: return ToxicSpider;
                    case MonsterType.DarkKnight: return DarkKnight;
                    case MonsterType.Wraith: return Wraith;
                    case MonsterType.Necromancer: return Necromancer;
                    case MonsterType.Abomination: return Abomination;
                    case MonsterType.Reaper: return Reaper;
                    default: return Slime;
                }
            }

            switch (type)
            {
                case MonsterType.Bat:
                    return new MonsterDefinition(MonsterType.Bat, "Bat", cfg.Bat.MaxHealth, cfg.Bat.MoveSpeed, cfg.Bat.ContactDamage, 0.3f, cfg.Bat.ExpValue, cfg.Bat.GoldValue);
                case MonsterType.Skeleton:
                    return new MonsterDefinition(MonsterType.Skeleton, "Skeleton", cfg.Skeleton.MaxHealth, cfg.Skeleton.MoveSpeed, cfg.Skeleton.ContactDamage, 0.45f, cfg.Skeleton.ExpValue, cfg.Skeleton.GoldValue, isRanged: true, preferredDistance: 4.5f, attackInterval: 2.0f);
                case MonsterType.Golem:
                    return new MonsterDefinition(MonsterType.Golem, "Golem", cfg.Golem.MaxHealth, cfg.Golem.MoveSpeed, cfg.Golem.ContactDamage, 0.7f, cfg.Golem.ExpValue, cfg.Golem.GoldValue);
                case MonsterType.FireImp:
                    return new MonsterDefinition(MonsterType.FireImp, "Fire Imp", cfg.FireImp.MaxHealth, cfg.FireImp.MoveSpeed, cfg.FireImp.ContactDamage, 0.38f, cfg.FireImp.ExpValue, cfg.FireImp.GoldValue);
                case MonsterType.ToxicSpider:
                    return new MonsterDefinition(MonsterType.ToxicSpider, "Toxic Spider", cfg.ToxicSpider.MaxHealth, cfg.ToxicSpider.MoveSpeed, cfg.ToxicSpider.ContactDamage, 0.52f, cfg.ToxicSpider.ExpValue, cfg.ToxicSpider.GoldValue);
                case MonsterType.DarkKnight:
                    return new MonsterDefinition(MonsterType.DarkKnight, "Dark Knight", cfg.DarkKnight.MaxHealth, cfg.DarkKnight.MoveSpeed, cfg.DarkKnight.ContactDamage, 0.75f, cfg.DarkKnight.ExpValue, cfg.DarkKnight.GoldValue, isRanged: true, preferredDistance: 4.8f, attackInterval: 2.5f);
                case MonsterType.Boss:
                    return CreateBoss("Goblin King", cfg.Boss.MaxHealth, cfg.Boss.MoveSpeed, cfg.Boss.ContactDamage, cfg.Boss.ExpValue, cfg.Boss.GoldValue);
                case MonsterType.Slime:
                default:
                    return new MonsterDefinition(MonsterType.Slime, "Slime", cfg.Slime.MaxHealth, cfg.Slime.MoveSpeed, cfg.Slime.ContactDamage, 0.4f, cfg.Slime.ExpValue, cfg.Slime.GoldValue);
            }
        }
    }
}
