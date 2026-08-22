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

    public readonly struct PlayerSlashExecutedEvent : IDomainEvent
    {
        public readonly int PlayerId;
        public readonly Vector2D CenterPosition;
        public readonly float DirectionAngleDegrees;
        public readonly float Radius;
        public readonly float ArcAngleDegrees;

        public PlayerSlashExecutedEvent(int playerId, Vector2D centerPosition, float directionAngleDegrees, float radius, float arcAngleDegrees = 150f)
        {
            PlayerId = playerId;
            CenterPosition = centerPosition;
            DirectionAngleDegrees = directionAngleDegrees;
            Radius = radius;
            ArcAngleDegrees = arcAngleDegrees;
        }
    }

    public readonly struct GroundStompExecutedEvent : IDomainEvent
    {
        public readonly Vector2D CenterPosition;
        public readonly float Radius;

        public GroundStompExecutedEvent(Vector2D centerPosition, float radius)
        {
            CenterPosition = centerPosition;
            Radius = radius;
        }
    }

    public readonly struct ArrowRainExecutedEvent : IDomainEvent
    {
        public readonly Vector2D CenterPosition;
        public readonly float Radius;
        public readonly float Duration;
        public readonly int ArrowCount;
        public readonly float DamagePerArrow;

        public ArrowRainExecutedEvent(Vector2D centerPosition, float radius, float duration = 1.5f, int arrowCount = 32, float damagePerArrow = 25f)
        {
            CenterPosition = centerPosition;
            Radius = radius;
            Duration = duration;
            ArrowCount = arrowCount;
            DamagePerArrow = damagePerArrow;
        }
    }

    public readonly struct BloodEaterExecutedEvent : IDomainEvent
    {
        public readonly int CasterId;
        public readonly Vector2D CenterPosition;
        public readonly float Radius;
        public readonly float Damage;
        public readonly float HealedAmount;

        public BloodEaterExecutedEvent(int casterId, Vector2D centerPosition, float radius, float damage, float healedAmount)
        {
            CasterId = casterId;
            CenterPosition = centerPosition;
            Radius = radius;
            Damage = damage;
            HealedAmount = healedAmount;
        }
    }

    public readonly struct StormArrowHitExplosionEvent : IDomainEvent
    {
        public readonly Vector2D Position;
        public readonly float Radius;

        public StormArrowHitExplosionEvent(Vector2D position, float radius)
        {
            Position = position;
            Radius = radius;
        }
    }

    public readonly struct StormBowExecutedEvent : IDomainEvent
    {
        public readonly Vector2D Origin;
        public readonly Vector2D TargetDirection;
        public readonly float ArrowDamage;
        public readonly float Speed;
        public readonly int ArrowCount;
        public readonly float SpreadAngleDeg;
        public readonly float ExplosionRadius;
        public readonly float ExplosionDamage;

        public StormBowExecutedEvent(
            Vector2D origin,
            Vector2D targetDirection,
            float arrowDamage,
            float speed = 20f,
            int arrowCount = 5,
            float spreadAngleDeg = 36f,
            float explosionRadius = 1.6f,
            float explosionDamage = 45f)
        {
            Origin = origin;
            TargetDirection = targetDirection;
            ArrowDamage = arrowDamage;
            Speed = speed;
            ArrowCount = arrowCount;
            SpreadAngleDeg = spreadAngleDeg;
            ExplosionRadius = explosionRadius;
            ExplosionDamage = explosionDamage;
        }
    }

    public readonly struct WindGlaiveExecutedEvent : IDomainEvent
    {
        public readonly Vector2D Origin;
        public readonly Vector2D TargetDirection;
        public readonly float Damage;
        public readonly float MaxDistance;
        public readonly float Speed;
        public readonly int GlaiveCount;

        public WindGlaiveExecutedEvent(Vector2D origin, Vector2D targetDirection, float damage, float maxDistance, float speed, int glaiveCount)
        {
            Origin = origin;
            TargetDirection = targetDirection;
            Damage = damage;
            MaxDistance = maxDistance;
            Speed = speed;
            GlaiveCount = glaiveCount;
        }
    }
}
