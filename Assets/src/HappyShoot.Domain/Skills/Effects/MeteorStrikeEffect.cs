using System.Collections.Generic;
using HappyShoot.Domain.Entities;
using HappyShoot.Domain.Events;
using HappyShoot.Domain.Spatial;

namespace HappyShoot.Domain.Skills.Effects
{
    /// <summary>
    /// Evolved Skill (Meteor Strike): Massive screen-wide fiery meteor crash with shockwaves and burn DoT.
    /// </summary>
    public class MeteorStrikeEffect : ISkillEffect
    {
        public float BaseDamage { get; set; }
        public float ExplosionRadius { get; set; }

        private readonly List<ISpatialEntity> _hitBuffer = new List<ISpatialEntity>(64);

        public MeteorStrikeEffect(float baseDamage = 120f, float explosionRadius = 6.0f)
        {
            BaseDamage = baseDamage;
            ExplosionRadius = explosionRadius;
        }

        public void ApplyEffect(SkillContext context, IList<Vector2D> targetPositions)
        {
            if (context?.TargetGrid == null || targetPositions == null || targetPositions.Count == 0)
                return;

            float effectiveRadius = ExplosionRadius * context.AreaMultiplier;
            float effectiveDamage = BaseDamage * (context.BaseDamage / 10f);

            for (int i = 0; i < targetPositions.Count; i++)
            {
                Vector2D targetPos = targetPositions[i];

                // Publish Domain Events for Sky Meteor Drop visual and massive impact
                context.EventBus?.Publish(new MeteorStrikeExecutedEvent(targetPos, effectiveRadius, effectiveDamage));
                context.EventBus?.Publish(new PlaySoundEvent(SoundEffectType.MagicExplosion, volume: 1.0f));

                int hitCount = context.TargetGrid.QueryRadiusNonAlloc(targetPos, effectiveRadius, _hitBuffer);
                for (int j = 0; j < hitCount; j++)
                {
                    if (_hitBuffer[j] is MonsterEntity monster && monster.IsActive && !monster.IsDead)
                    {
                        // Apply 7-second Burn DoT (ticks every 0.5s)
                        monster.ApplyBurn(duration: 7.0f, damagePerTick: effectiveDamage * 0.10f);
                        monster.TakeDamage(effectiveDamage);
                    }
                }
            }
        }
    }
}
