using System.Collections.Generic;
using HappyShoot.Domain.Entities;
using HappyShoot.Domain.Spatial;

namespace HappyShoot.Domain.Skills.Effects
{
    /// <summary>
    /// Evolved Arcane Explosion (Meteor Strike): Massive screen-wide persistent meteor bombardment.
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
                int hitCount = context.TargetGrid.QueryRadiusNonAlloc(targetPositions[i], effectiveRadius, _hitBuffer);
                for (int j = 0; j < hitCount; j++)
                {
                    if (_hitBuffer[j] is MonsterEntity monster && monster.IsActive && !monster.IsDead)
                    {
                        monster.TakeDamage(effectiveDamage);
                    }
                }
            }
        }
    }
}
