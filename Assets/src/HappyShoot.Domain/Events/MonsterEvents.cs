using HappyShoot.Domain.Spatial;

namespace HappyShoot.Domain.Events
{
    public readonly struct MonsterDamagedEvent : IDomainEvent
    {
        public readonly int MonsterId;
        public readonly float Damage;
        public readonly float RemainingHealth;
        public readonly float MaxHealth;
        public readonly Vector2D Position;
        public readonly bool IsCritical;

        public MonsterDamagedEvent(int monsterId, float damage, float remainingHealth, float maxHealth, Vector2D position, bool isCritical = false)
        {
            MonsterId = monsterId;
            Damage = damage;
            RemainingHealth = remainingHealth;
            MaxHealth = maxHealth;
            Position = position;
            IsCritical = isCritical;
        }
    }

    public readonly struct MonsterDiedEvent : IDomainEvent
    {
        public readonly int MonsterId;
        public readonly Vector2D Position;
        public readonly int ExpValue;
        public readonly int GoldValue;
        public readonly bool IsBoss;

        public MonsterDiedEvent(int monsterId, Vector2D position, int expValue, int goldValue, bool isBoss = false)
        {
            MonsterId = monsterId;
            Position = position;
            ExpValue = expValue;
            GoldValue = goldValue;
            IsBoss = isBoss;
        }
    }
}

