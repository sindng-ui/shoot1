using System.Collections.Generic;
using HappyShoot.Domain.Entities;
using HappyShoot.Domain.Spatial;

namespace HappyShoot.Domain.Skills.Effects
{
    /// <summary>
    /// Evolved Greatsword (Blood Eater): 360-degree full circle spin slash with life-steal on hit.
    /// </summary>
    public class BloodEaterEffect : ISkillEffect
    {
        public float BaseDamage { get; set; }
        public float SpinRadius { get; set; }
        public float LifeStealPerHit { get; set; }

        private readonly List<ISpatialEntity> _hitBuffer = new List<ISpatialEntity>(32);

        public BloodEaterEffect(float baseDamage = 75f, float spinRadius = 4.5f, float lifeStealPerHit = 1.5f)
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

            // Life-steal healing
            if (actualHits > 0 && context.CasterId > 0)
            {
                // In full loop, player heal event can be published or triggered via context
            }
        }
    }
}
