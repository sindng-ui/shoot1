using System.Collections.Generic;
using HappyShoot.Domain.Spatial;

namespace HappyShoot.Domain.Events
{
    /// <summary>
    /// Event triggered when a Wizard casts Frost Nova around the player.
    /// </summary>
    public readonly struct FrostNovaExecutedEvent : IDomainEvent
    {
        public readonly Vector2D CenterPosition;
        public readonly float Radius;
        public readonly float Damage;

        public FrostNovaExecutedEvent(Vector2D centerPosition, float radius, float damage)
        {
            CenterPosition = centerPosition;
            Radius = radius;
            Damage = damage;
        }
    }

    /// <summary>
    /// Event triggered when Chain Lightning strikes a sequence of enemies.
    /// </summary>
    public readonly struct ChainLightningExecutedEvent : IDomainEvent
    {
        public readonly Vector2D StartPosition;
        public readonly IReadOnlyList<Vector2D> TargetPositions;
        public readonly float Damage;

        public ChainLightningExecutedEvent(Vector2D startPosition, IReadOnlyList<Vector2D> targetPositions, float damage)
        {
            StartPosition = startPosition;
            TargetPositions = targetPositions;
            Damage = damage;
        }
    }

    /// <summary>
    /// Event triggered when a Wizard launches a flying fiery comet fireball towards a target location.
    /// </summary>
    public readonly struct FireballLaunchedEvent : IDomainEvent
    {
        public readonly Vector2D StartPosition;
        public readonly Vector2D TargetPosition;
        public readonly float Radius;
        public readonly float Damage;
        public readonly float Speed;

        public FireballLaunchedEvent(Vector2D startPosition, Vector2D targetPosition, float radius, float damage, float speed = 18f)
        {
            StartPosition = startPosition;
            TargetPosition = targetPosition;
            Radius = radius;
            Damage = damage;
            Speed = speed;
        }
    }

    /// <summary>
    /// Event triggered when a Fireball explodes on impact.
    /// </summary>
    public readonly struct FireballExplodedEvent : IDomainEvent
    {
        public readonly Vector2D CenterPosition;
        public readonly float Radius;
        public readonly float Damage;

        public FireballExplodedEvent(Vector2D centerPosition, float radius, float damage)
        {
            CenterPosition = centerPosition;
            Radius = radius;
            Damage = damage;
        }
    }

    /// <summary>
    /// Event triggered when Meteor Strike drops a massive flaming meteor onto the ground.
    /// </summary>
    public readonly struct MeteorStrikeExecutedEvent : IDomainEvent
    {
        public readonly Vector2D TargetPosition;
        public readonly float Radius;
        public readonly float Damage;

        public MeteorStrikeExecutedEvent(Vector2D targetPosition, float radius, float damage)
        {
            TargetPosition = targetPosition;
            Radius = radius;
            Damage = damage;
        }
    }

    /// <summary>
    /// Event triggered when a chilled monster dies and shatters into crystalline ice shards.
    /// </summary>
    public readonly struct MonsterShatteredEvent : IDomainEvent
    {
        public readonly int MonsterId;
        public readonly Vector2D Position;
        public readonly float Size;

        public MonsterShatteredEvent(int monsterId, Vector2D position, float size = 1.0f)
        {
            MonsterId = monsterId;
            Position = position;
            Size = size;
        }
    }

    /// <summary>
    /// Event triggered when Wizard casts Evolved Gigastorm Chain Lightning.
    /// </summary>
    public readonly struct GigastormLightningExecutedEvent : IDomainEvent
    {
        public readonly Vector2D StartPosition;
        public readonly IReadOnlyList<Vector2D> TargetPositions;
        public readonly float Damage;
        public readonly float SparkRadius;

        public GigastormLightningExecutedEvent(Vector2D startPosition, IReadOnlyList<Vector2D> targetPositions, float damage, float sparkRadius = 1.5f)
        {
            StartPosition = startPosition;
            TargetPositions = targetPositions;
            Damage = damage;
            SparkRadius = sparkRadius;
        }
    }

    /// <summary>
    /// Event triggered when Wizard casts Evolved Blizzard Nova.
    /// </summary>
    public readonly struct BlizzardNovaExecutedEvent : IDomainEvent
    {
        public readonly Vector2D CenterPosition;
        public readonly float Radius;
        public readonly float Damage;
        public readonly int ShardCount;

        public BlizzardNovaExecutedEvent(Vector2D centerPosition, float radius, float damage, int shardCount = 8)
        {
            CenterPosition = centerPosition;
            Radius = radius;
            Damage = damage;
            ShardCount = shardCount;
        }
    }

    /// <summary>
    /// Event triggered when Wizard casts Evolved Meteor Strike (Inferno Fireball comets that pierce and double explode).
    /// </summary>
    public readonly struct MeteorStrikeLaunchedEvent : IDomainEvent
    {
        public readonly Vector2D StartPosition;
        public readonly Vector2D TargetPosition;
        public readonly float Radius;
        public readonly float Damage;
        public readonly float Speed;
        public readonly int MaxPierces;

        public MeteorStrikeLaunchedEvent(Vector2D startPosition, Vector2D targetPosition, float radius, float damage, float speed = 15f, int maxPierces = 1)
        {
            StartPosition = startPosition;
            TargetPosition = targetPosition;
            Radius = radius;
            Damage = damage;
            Speed = speed;
            MaxPierces = maxPierces;
        }
    }
}
