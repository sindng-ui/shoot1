using System;

namespace HappyShoot.Domain.Entities
{
    [Serializable]
    public class MonsterTuningConfigData
    {
        public MonsterStatConfig Slime = new MonsterStatConfig(hp: 30f, speed: 2.5f, damage: 8f, exp: 1, gold: 1);
        public MonsterStatConfig Bat = new MonsterStatConfig(hp: 15f, speed: 4.05f, damage: 5f, exp: 1, gold: 1);
        public SkeletonStatConfig Skeleton = new SkeletonStatConfig(hp: 45f, speed: 1.8f, damage: 12f, projSpeed: 2.75f, projDamage: 10f, exp: 2, gold: 2);
        public MonsterStatConfig Golem = new MonsterStatConfig(hp: 160f, speed: 1.2f, damage: 22f, exp: 4, gold: 5);
        public MonsterStatConfig FireImp = new MonsterStatConfig(hp: 150f, speed: 3.8f, damage: 18f, exp: 5, gold: 6);
        public MonsterStatConfig ToxicSpider = new MonsterStatConfig(hp: 280f, speed: 2.2f, damage: 20f, exp: 8, gold: 9);
        public DarkKnightStatConfig DarkKnight = new DarkKnightStatConfig(hp: 600f, speed: 1.5f, damage: 35f, projSpeed: 3.5f, projDamage: 20f, exp: 14, gold: 18);
        public BossStatConfig Boss = new BossStatConfig(hp: 800f, speed: 2.2f, damage: 25f, laserInterval: 8.0f, laserDamage: 25f, hazardInterval: 6.5f, hazardDamage: 18f, hazardRadius: 2.8f, exp: 30, gold: 100);

        public float GetVisualScale(MonsterType type)
        {
            switch (type)
            {
                case MonsterType.Slime: return Slime != null ? Slime.VisualScale : 1.0f;
                case MonsterType.Bat: return Bat != null ? Bat.VisualScale : 1.0f;
                case MonsterType.Skeleton: return Skeleton != null ? Skeleton.VisualScale : 1.0f;
                case MonsterType.Golem: return Golem != null ? Golem.VisualScale : 1.0f;
                case MonsterType.FireImp: return FireImp != null ? FireImp.VisualScale : 1.0f;
                case MonsterType.ToxicSpider: return ToxicSpider != null ? ToxicSpider.VisualScale : 1.0f;
                case MonsterType.DarkKnight: return DarkKnight != null ? DarkKnight.VisualScale : 1.0f;
                case MonsterType.Boss:
                case MonsterType.Boss2:
                case MonsterType.Boss3: return Boss != null ? Boss.VisualScale : 1.0f;
                default: return 1.0f;
            }
        }
    }

    [Serializable]
    public class MonsterStatConfig
    {
        public float MaxHealth;
        public float MoveSpeed;
        public float ContactDamage;
        public int ExpValue;
        public int GoldValue;
        public float VisualScale = 1.0f;

        public MonsterStatConfig() { }

        public MonsterStatConfig(float hp, float speed, float damage, int exp, int gold)
        {
            MaxHealth = hp;
            MoveSpeed = speed;
            ContactDamage = damage;
            ExpValue = exp;
            GoldValue = gold;
        }
    }

    [Serializable]
    public class SkeletonStatConfig : MonsterStatConfig
    {
        public float ProjectileSpeed;
        public float ProjectileDamage;

        public SkeletonStatConfig() { }

        public SkeletonStatConfig(float hp, float speed, float damage, float projSpeed, float projDamage, int exp, int gold)
            : base(hp, speed, damage, exp, gold)
        {
            ProjectileSpeed = projSpeed;
            ProjectileDamage = projDamage;
        }
    }

    [Serializable]
    public class DarkKnightStatConfig : MonsterStatConfig
    {
        public float ProjectileSpeed;
        public float ProjectileDamage;

        public DarkKnightStatConfig() { }

        public DarkKnightStatConfig(float hp, float speed, float damage, float projSpeed, float projDamage, int exp, int gold)
            : base(hp, speed, damage, exp, gold)
        {
            ProjectileSpeed = projSpeed;
            ProjectileDamage = projDamage;
        }
    }

    [Serializable]
    public class BossStatConfig : MonsterStatConfig
    {
        public float LaserInterval;
        public float LaserDamage;
        public float HazardInterval;
        public float HazardDamage;
        public float HazardRadius;

        public BossStatConfig() { }

        public BossStatConfig(float hp, float speed, float damage, float laserInterval, float laserDamage, float hazardInterval, float hazardDamage, float hazardRadius, int exp, int gold)
            : base(hp, speed, damage, exp, gold)
        {
            LaserInterval = laserInterval;
            LaserDamage = laserDamage;
            HazardInterval = hazardInterval;
            HazardDamage = hazardDamage;
            HazardRadius = hazardRadius;
        }
    }
}
