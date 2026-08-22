using System.Collections.Generic;
using HappyShoot.Domain.Events;
using HappyShoot.Domain.Spatial;

namespace HappyShoot.Domain.Skills
{
    /// <summary>
    /// Execution context passed to skill triggers, targeters, and effects.
    /// </summary>
    public class SkillContext
    {
        public int CasterId { get; set; }
        public string SkillId { get; set; }
        public Entities.PlayerEntity CasterEntity { get; set; }
        public Vector2D CasterPosition { get; set; }
        public float BaseDamage { get; set; }
        public float AreaMultiplier { get; set; } = 1.0f;
        public float SpeedMultiplier { get; set; } = 1.0f;
        public float DeltaTime { get; set; } = 0.016f;
        public Vector2D AimDirection { get; set; } = Vector2D.Right;
        public Vector2D AimTargetPosition { get; set; } = Vector2D.Zero;
        public ISpatialGrid2D TargetGrid { get; set; }
        public Projectiles.ProjectileManager ProjectileManager { get; set; }
        public EventBus EventBus { get; set; }
    }

    /// <summary>
    /// Component responsible for deciding WHEN a skill should fire.
    /// </summary>
    public interface ISkillTrigger
    {
        bool CanTrigger(float deltaTime);
        void OnTriggered();
        void Reset();
    }

    /// <summary>
    /// Component responsible for selecting target position(s) or entity(ies).
    /// </summary>
    public interface ISkillTargeter
    {
        bool TryFindTargets(SkillContext context, float range, IList<Vector2D> targetPositionsBuffer);
    }

    /// <summary>
    /// Component responsible for executing the payload/behavior (damage, projectile spawn, buff, etc).
    /// </summary>
    public interface ISkillEffect
    {
        void ApplyEffect(SkillContext context, IList<Vector2D> targetPositions);
    }

    /// <summary>
    /// Optional interface for skill effects that scale custom stats (radius, arc, count) on level up.
    /// </summary>
    public interface ILevelableEffect
    {
        void OnLevelUp(int newLevel);
    }

    /// <summary>
    /// Optional interface for continuous AoE / over-time skill effects updated per frame.
    /// </summary>
    public interface ITickableEffect
    {
        void Update(float deltaTime, SkillContext context);
    }

    /// <summary>
    /// Unified contract for all active/passive skills.
    /// </summary>
    public interface ISkill
    {
        string Id { get; }
        string Name { get; }
        int Level { get; }
        int MaxLevel { get; }
        bool IsMaxLevel { get; }

        void LevelUp();
        void Update(float deltaTime, SkillContext context);
    }
}
