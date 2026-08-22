using System.Collections.Generic;
using HappyShoot.Domain.Entities;
using HappyShoot.Domain.Events;
using HappyShoot.Domain.Spatial;

namespace HappyShoot.Domain.Skills.Effects
{
    /// <summary>
    /// Evolved Greatsword (Blood Eater): 360-degree full circle crimson spin slash with life-steal on hit.
    /// </summary>
    public class BloodEaterEffect : ISkillEffect
    {
        public float BaseDamage { get; set; }
        public float SpinRadius { get; set; }
        public float LifeStealPerHit { get; set; }

        public float Radius
        {
            get => SpinRadius;
            set => SpinRadius = value;
        }

        public float HealAmount
        {
            get => LifeStealPerHit;
            set => LifeStealPerHit = value;
        }

        private readonly List<ISpatialEntity> _hitBuffer = new List<ISpatialEntity>(64);

        public BloodEaterEffect(float baseDamage = 85f, float spinRadius = 4.8f, float lifeStealPerHit = 2.0f)
        {
            BaseDamage = baseDamage;
            SpinRadius = spinRadius;
            LifeStealPerHit = lifeStealPerHit;
        }

        public void ApplyEffect(SkillContext context, IList<Vector2D> targetPositions)
        {
            if (context?.TargetGrid == null) return;

            float effectiveRadius = SpinRadius * context.AreaMultiplier;
            float effectiveDamage = BaseDamage * (context.BaseDamage / 10f);

            int hitCount = context.TargetGrid.QueryRadiusNonAlloc(context.CasterPosition, effectiveRadius, _hitBuffer);
            int actualHits = 0;

            for (int i = 0; i < hitCount; i++)
            {
                if (_hitBuffer[i] is MonsterEntity monster && monster.IsActive && !monster.IsDead)
                {
                    monster.TakeDamage(effectiveDamage);
                    actualHits++;
                }
            }

            float totalHeal = actualHits * LifeStealPerHit;

            // 1. Publish visual and sound events
            context.EventBus?.Publish(new BloodEaterExecutedEvent(
                context.CasterId,
                context.CasterPosition,
                effectiveRadius,
                effectiveDamage,
                totalHeal
            ));

            context.EventBus?.Publish(new PlaySoundEvent(SoundEffectType.SlashAttack, volume: 1.0f));

            // 2. Apply Life-Steal Heal
            if (totalHeal > 0f)
            {
                context.CasterEntity?.Heal(totalHeal);
            }
        }
    }
}

