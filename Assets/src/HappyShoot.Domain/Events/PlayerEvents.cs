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

    public readonly struct WhirlwindExecutedEvent : IDomainEvent
    {
        public readonly Vector2D CenterPosition;
        public readonly float Radius;

        public WhirlwindExecutedEvent(Vector2D centerPosition, float radius)
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
        public readonly float DirectionAngleDegrees;
        public readonly float Radius;
        public readonly float ArcAngleDegrees;
        public readonly float Damage;
        public readonly float HealedAmount;
        public readonly System.Collections.Generic.IReadOnlyList<Vector2D> HitPositions;

        public BloodEaterExecutedEvent(
            int casterId, 
            Vector2D centerPosition, 
            float directionAngleDegrees, 
            float radius, 
            float arcAngleDegrees, 
            float damage, 
            float healedAmount,
            System.Collections.Generic.IReadOnlyList<Vector2D> hitPositions = null)
        {
            CasterId = casterId;
            CenterPosition = centerPosition;
            DirectionAngleDegrees = directionAngleDegrees;
            Radius = radius;
            ArcAngleDegrees = arcAngleDegrees;
            Damage = damage;
            HealedAmount = healedAmount;
            HitPositions = hitPositions ?? System.Array.Empty<Vector2D>();
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

    public readonly struct TempestWhirlwindExecutedEvent : IDomainEvent
    {
        public readonly Vector2D CenterPosition;
        public readonly float Radius;
        public readonly float Damage;
        public readonly int SlashWaveCount;

        public TempestWhirlwindExecutedEvent(Vector2D centerPosition, float radius, float damage, int slashWaveCount = 4)
        {
            CenterPosition = centerPosition;
            Radius = radius;
            Damage = damage;
            SlashWaveCount = slashWaveCount;
        }
    }

    public readonly struct EarthshakerExecutedEvent : IDomainEvent
    {
        public readonly Vector2D CenterPosition;
        public readonly float Radius;
        public readonly float Damage;
        public readonly int FissureCount;

        public EarthshakerExecutedEvent(Vector2D centerPosition, float radius, float damage, int fissureCount = 4)
        {
            CenterPosition = centerPosition;
            Radius = radius;
            Damage = damage;
            FissureCount = fissureCount;
        }
    }

    public readonly struct PhantomGlaiveExecutedEvent : IDomainEvent
    {
        public readonly Vector2D Origin;
        public readonly Vector2D TargetDirection;
        public readonly float Damage;
        public readonly float MaxDistance;
        public readonly float Speed;
        public readonly int PhantomCount;
        public readonly float BladeScale;

        public PhantomGlaiveExecutedEvent(Vector2D origin, Vector2D targetDirection, float damage, float maxDistance, float speed, int phantomCount = 2, float bladeScale = 1.0f)
        {
            Origin = origin;
            TargetDirection = targetDirection;
            Damage = damage;
            MaxDistance = maxDistance;
            Speed = speed;
            PhantomCount = phantomCount;
            BladeScale = bladeScale;
        }
    }

    public readonly struct StellarRainExecutedEvent : IDomainEvent
    {
        public readonly Vector2D TargetCenter;
        public readonly float Radius;
        public readonly float Damage;
        public readonly int ArrowCount;
        public readonly float Duration;

        public StellarRainExecutedEvent(Vector2D targetCenter, float radius, float damage, int arrowCount = 60, float duration = 2.0f)
        {
            TargetCenter = targetCenter;
            Radius = radius;
            Damage = damage;
            ArrowCount = arrowCount;
            Duration = duration;
        }
    }

    public readonly struct PiercingArrowExecutedEvent : IDomainEvent
    {
        public readonly Vector2D Origin;
        public readonly Vector2D TargetDirection;
        public readonly int ArrowCount;

        public PiercingArrowExecutedEvent(Vector2D origin, Vector2D targetDirection, int arrowCount)
        {
            Origin = origin;
            TargetDirection = targetDirection;
            ArrowCount = arrowCount;
        }
    }
}
