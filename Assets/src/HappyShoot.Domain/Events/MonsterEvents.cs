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

        public MonsterDamagedEvent(int monsterId, float damage, float remainingHealth, float maxHealth, Vector2D position)
        {
            MonsterId = monsterId;
            Damage = damage;
            RemainingHealth = remainingHealth;
            MaxHealth = maxHealth;
            Position = position;
        }
    }

    public readonly struct MonsterDiedEvent : IDomainEvent
    {
        public readonly int MonsterId;
        public readonly Vector2D Position;
        public readonly int ExpValue;
        public readonly int GoldValue;

        public MonsterDiedEvent(int monsterId, Vector2D position, int expValue, int goldValue)
        {
            MonsterId = monsterId;
            Position = position;
            ExpValue = expValue;
            GoldValue = goldValue;
        }
    }
}
