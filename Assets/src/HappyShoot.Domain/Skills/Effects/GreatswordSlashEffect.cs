using System;
using System.Collections.Generic;
using HappyShoot.Domain.Entities;
using HappyShoot.Domain.Events;
using HappyShoot.Domain.Spatial;

namespace HappyShoot.Domain.Skills.Effects
{
    /// <summary>
    /// Warrior: Slashes in a sweeping melee arc in front of the player, hitting enemies within the swing trajectory.
    /// Trajectory Arc: ~150 degrees (slightly wider than visual 120-degree swing for responsive hit registration).
    /// </summary>
    public class GreatswordSlashEffect : ISkillEffect, ILevelableEffect
    {
        public float BaseDamage { get; set; }
        public float Radius { get; set; }
        public float ArcAngleDegrees { get; set; }

        private readonly List<ISpatialEntity> _hitBuffer = new List<ISpatialEntity>(32);
        private const float DefaultArcAngleDegrees = 150f; // +-75 degrees from forward
        private readonly float _initialRadius;
        private readonly float _initialArcAngle;
        private readonly float _initialBaseDamage;

        public GreatswordSlashEffect(float baseDamage = 35f, float radius = 2.5f, float arcAngleDegrees = DefaultArcAngleDegrees)
        {
            BaseDamage = baseDamage;
            Radius = radius;
            ArcAngleDegrees = arcAngleDegrees;

            _initialBaseDamage = baseDamage;
            _initialRadius = radius;
            _initialArcAngle = arcAngleDegrees;
        }

        public void OnLevelUp(int newLevel)
        {
            // Massive upgrade scaling:
            // Level 1: 2.50m, 150 deg, 35 dmg
            // Level 2: 3.35m (+0.85m), 175 deg (+25 deg), 50 dmg
            // Level 3: 4.20m, 200 deg, 65 dmg
            // Level 4: 5.05m, 225 deg, 80 dmg
            // Level 5: 5.90m, 250 deg, 100 dmg
            Radius = _initialRadius + 0.85f * (newLevel - 1);
            ArcAngleDegrees = _initialArcAngle + 25f * (newLevel - 1);
            BaseDamage = _initialBaseDamage + 15f * (newLevel - 1);
        }

        public void ApplyEffect(SkillContext context, IList<Vector2D> targetPositions)
        {
            if (context?.TargetGrid == null) return;

            // 1. Calculate attack forward direction and angle
            Vector2D forward = Vector2D.Right;
            if (targetPositions != null && targetPositions.Count > 0)
            {
                Vector2D diff = targetPositions[0] - context.CasterPosition;
                if (diff.SqrMagnitude > 1e-4f)
                {
                    forward = diff.Normalized;
                }
            }

            float slashAngleDegrees = (float)(Math.Atan2(forward.Y, forward.X) * (180.0 / Math.PI));
            float effectiveRadius = Radius * context.AreaMultiplier;
            float effectiveDamage = BaseDamage * (context.BaseDamage / 10f);

            // Minimum dot product for the arc sector (cos(halfAngle))
            float halfAngleRad = (ArcAngleDegrees * 0.5f) * ((float)Math.PI / 180f);
            float minDot = (float)Math.Cos(halfAngleRad);

            // 2. Query spatial grid around player with margin for enemy hitbox radius
            float queryRadius = effectiveRadius + 0.6f;
            int hitCount = context.TargetGrid.QueryRadiusNonAlloc(context.CasterPosition, queryRadius, _hitBuffer);

            for (int i = 0; i < hitCount; i++)
            {
                if (_hitBuffer[i] is MonsterEntity monster && monster.IsActive && !monster.IsDead)
                {
                    Vector2D toMonster = monster.Position - context.CasterPosition;
                    float dist = toMonster.Magnitude;
                    float monsterRadius = monster.Radius > 0f ? monster.Radius : 0.4f;

                    // Max distance check
                    if (dist > effectiveRadius + monsterRadius)
                    {
                        continue;
                    }

                    // Point-blank enemies in center overlap are always hit
                    if (dist <= 0.25f)
                    {
                        monster.TakeDamage(effectiveDamage);
                        continue;
                    }

                    // Sector / Arc angle validation
                    Vector2D dirToMonster = toMonster / dist;
                    float dot = Vector2D.Dot(forward, dirToMonster);

                    // Strictly ignore opposite side (behind the back)
                    if (dot <= 0f)
                    {
                        continue;
                    }

                    // Hit if within arc angle or monster hitbox touches the swing sector
                    if (dot >= minDot)
                    {
                        monster.TakeDamage(effectiveDamage);
                    }
                }
            }

            // 3. Publish domain events for visual & audio synchronization
            context.EventBus?.Publish(new PlayerSlashExecutedEvent(
                context.CasterId,
                context.CasterPosition,
                slashAngleDegrees,
                effectiveRadius,
                ArcAngleDegrees
            ));

            context.EventBus?.Publish(new PlaySoundEvent(SoundEffectType.SlashAttack));
        }
    }
}
