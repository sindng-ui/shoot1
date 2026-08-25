using System;
using System.Collections.Generic;
using HappyShoot.Domain.Entities;
using HappyShoot.Domain.Events;
using HappyShoot.Domain.Spatial;

namespace HappyShoot.Domain.Skills.Effects
{
    /// <summary>
    /// Evolved Greatsword (Blood Eater): Massive forward crimson arc slash with life-steal on hit enemies.
    /// Hits all enemies within the 150-degree forward swing arc and spawns blood essence orbs.
    /// </summary>
    public class BloodEaterEffect : ISkillEffect
    {
        public float BaseDamage { get; set; }
        public float Radius { get; set; }
        public float ArcAngleDegrees { get; set; } = 150f;
        public float LifeStealPerHit { get; set; }

        public float HealAmount
        {
            get => LifeStealPerHit;
            set => LifeStealPerHit = value;
        }

        private readonly List<ISpatialEntity> _hitBuffer = new List<ISpatialEntity>(64);
        private readonly List<Vector2D> _hitPositions = new List<Vector2D>(32);

        public BloodEaterEffect(float baseDamage = 85f, float radius = 4.8f, float lifeStealPerHit = 2.0f, float arcAngleDegrees = 150f)
        {
            BaseDamage = baseDamage;
            Radius = radius;
            LifeStealPerHit = lifeStealPerHit;
            ArcAngleDegrees = arcAngleDegrees;
        }

        public void ApplyEffect(SkillContext context, IList<Vector2D> targetPositions)
        {
            if (context?.TargetGrid == null) return;

            // 1. Calculate attack forward direction and angle towards target
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

            _hitPositions.Clear();
            int actualHits = 0;

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
                    if (dist <= 0.35f)
                    {
                        var (hitDmg, isCrit) = context.RollDamage(effectiveDamage);
                        monster.TakeDamage(hitDmg, isCrit);
                        _hitPositions.Add(monster.Position);
                        actualHits++;
                        continue;
                    }

                    // Sector / Arc angle validation
                    Vector2D dirToMonster = toMonster / dist;
                    float dot = Vector2D.Dot(forward, dirToMonster);

                    // Hit if within arc angle
                    if (dot >= minDot)
                    {
                        var (hitDmg, isCrit) = context.RollDamage(effectiveDamage);
                        monster.TakeDamage(hitDmg, isCrit);
                        _hitPositions.Add(monster.Position);
                        actualHits++;
                    }
                }
            }

            float totalHeal = actualHits * LifeStealPerHit;

            // 3. Publish visual and sound events
            context.EventBus?.Publish(new BloodEaterExecutedEvent(
                context.CasterId,
                context.CasterPosition,
                slashAngleDegrees,
                effectiveRadius,
                ArcAngleDegrees,
                effectiveDamage,
                totalHeal,
                _hitPositions.ToArray()
            ));

            context.EventBus?.Publish(new PlaySoundEvent(SoundEffectType.SlashAttack, volume: 1.0f));

            // 4. Apply Life-Steal Heal to Caster
            if (totalHeal > 0f)
            {
                context.CasterEntity?.Heal(totalHeal);
            }
        }
    }
}
