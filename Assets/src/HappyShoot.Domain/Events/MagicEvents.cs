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
}
