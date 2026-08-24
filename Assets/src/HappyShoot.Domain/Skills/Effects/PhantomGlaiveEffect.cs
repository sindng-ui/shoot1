using System;
using System.Collections.Generic;
using HappyShoot.Domain.Entities;
using HappyShoot.Domain.Events;
using HappyShoot.Domain.Spatial;

namespace HappyShoot.Domain.Skills.Effects
{
    /// <summary>
    /// Ranger Evolved Skill: Phantom Glaive (Evolved from Wind Glaive + Eye of the Hawk).
    /// Hurls twin phantom spiral boomerangs orbiting a prime piercing glaive, shredding through enemy lines with dual-pass hitboxes.
    /// Strictly modular and under 500 lines.
    /// </summary>
    public class PhantomGlaiveEffect : ISkillEffect
    {
        public float BaseDamage { get; set; }
        public float MaxDistance { get; set; }
        public float Speed { get; set; }
        public int PhantomCount { get; set; }
        public float BladeScale { get; set; }

        private readonly List<ISpatialEntity> _hitBuffer = new List<ISpatialEntity>(64);

        public PhantomGlaiveEffect(float baseDamage = 60f, float maxDistance = 11.0f, float speed = 17.0f, int phantomCount = 2, float bladeScale = 1.0f)
        {
            BaseDamage = baseDamage;
            MaxDistance = maxDistance;
            Speed = speed;
            PhantomCount = phantomCount;
            BladeScale = bladeScale;
        }

        public void ApplyEffect(SkillContext context, IList<Vector2D> targetPositions)
        {
            if (context?.TargetGrid == null || targetPositions == null || targetPositions.Count == 0)
                return;

            float effectiveDamage = BaseDamage * (context.BaseDamage / 10f);
            float effectiveDistance = MaxDistance * context.AreaMultiplier;
            float effectiveSpeed = Speed * context.SpeedMultiplier;

            Vector2D primaryTarget = targetPositions[0];
            Vector2D offset = primaryTarget - context.CasterPosition;
            Vector2D dir = offset.SqrMagnitude > 1e-4f ? offset.Normalized : Vector2D.Right;

            // Publish Domain Event for Presentation Manager
            context.EventBus?.Publish(new PhantomGlaiveExecutedEvent(
                context.CasterPosition,
                dir,
                effectiveDamage,
                effectiveDistance,
                effectiveSpeed,
                PhantomCount,
                BladeScale
            ));

            context.EventBus?.Publish(new PlaySoundEvent(SoundEffectType.BowShoot, volume: 0.95f));

            // Immediate domain damage over line trajectory
            int count = context.TargetGrid.QueryRadiusNonAlloc(context.CasterPosition, effectiveDistance, _hitBuffer);
            float baseWidth = 1.5f * BladeScale;
            float maxPerpDistSq = baseWidth * baseWidth;

            for (int i = 0; i < count; i++)
            {
                if (_hitBuffer[i] is MonsterEntity monster && monster.IsActive && !monster.IsDead)
                {
                    Vector2D toMonster = monster.Position - context.CasterPosition;
                    float projAlongDir = (float)(toMonster.X * dir.X + toMonster.Y * dir.Y);

                    if (projAlongDir >= 0f && projAlongDir <= effectiveDistance)
                    {
                        float perpDistSq = (float)(toMonster.SqrMagnitude - projAlongDir * projAlongDir);
                        if (perpDistSq <= maxPerpDistSq)
                        {
                            var (hitDmg, isCrit) = context.RollDamage(effectiveDamage);
                            monster.TakeDamage(hitDmg, isCrit);
                        }
                    }
                }
            }
        }
    }
}
