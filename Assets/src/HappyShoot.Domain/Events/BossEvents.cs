using HappyShoot.Domain.Spatial;

namespace HappyShoot.Domain.Events
{
    public readonly struct BossSpawnedEvent : IDomainEvent
    {
        public readonly int BossId;
        public readonly string BossName;
        public readonly float MaxHealth;
        public readonly Vector2D Position;

        public BossSpawnedEvent(int bossId, string bossName, float maxHealth, Vector2D position)
        {
            BossId = bossId;
            BossName = bossName;
            MaxHealth = maxHealth;
            Position = position;
        }
    }

    public readonly struct BossHealthUpdatedEvent : IDomainEvent
    {
        public readonly int BossId;
        public readonly float CurrentHealth;
        public readonly float MaxHealth;

        public BossHealthUpdatedEvent(int bossId, float currentHealth, float maxHealth)
        {
            BossId = bossId;
            CurrentHealth = currentHealth;
            MaxHealth = maxHealth;
        }
    }

    public readonly struct BossDiedEvent : IDomainEvent
    {
        public readonly int BossId;
        public readonly string BossName;
        public readonly Vector2D Position;
        public readonly int GoldReward;

        public BossDiedEvent(int bossId, string bossName, Vector2D position, int goldReward)
        {
            BossId = bossId;
            BossName = bossName;
            Position = position;
            GoldReward = goldReward;
        }
    }
}
