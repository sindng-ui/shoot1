using HappyShoot.Domain.Spatial;

namespace HappyShoot.Domain.Events
{
    public readonly struct PlayerDamagedEvent : IDomainEvent
    {
        public readonly int PlayerId;
        public readonly float Damage;
        public readonly float RemainingHealth;
        public readonly float MaxHealth;

        public PlayerDamagedEvent(int playerId, float damage, float remainingHealth, float maxHealth)
        {
            PlayerId = playerId;
            Damage = damage;
            RemainingHealth = remainingHealth;
            MaxHealth = maxHealth;
        }
    }

    public readonly struct PlayerHealedEvent : IDomainEvent
    {
        public readonly int PlayerId;
        public readonly float Amount;
        public readonly float CurrentHealth;
        public readonly float MaxHealth;

        public PlayerHealedEvent(int playerId, float amount, float currentHealth, float maxHealth)
        {
            PlayerId = playerId;
            Amount = amount;
            CurrentHealth = currentHealth;
            MaxHealth = maxHealth;
        }
    }

    public readonly struct PlayerDiedEvent : IDomainEvent
    {
        public readonly int PlayerId;

        public PlayerDiedEvent(int playerId)
        {
            PlayerId = playerId;
        }
    }

    public readonly struct PlayerMovedEvent : IDomainEvent
    {
        public readonly int PlayerId;
        public readonly Vector2D Position;

        public PlayerMovedEvent(int playerId, Vector2D position)
        {
            PlayerId = playerId;
            Position = position;
        }
    }
}
