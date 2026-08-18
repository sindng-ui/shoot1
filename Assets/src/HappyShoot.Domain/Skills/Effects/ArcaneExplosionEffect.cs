using System.Collections.Generic;
using HappyShoot.Domain.Entities;
using HappyShoot.Domain.Spatial;

namespace HappyShoot.Domain.Skills.Effects
{
    /// <summary>
    /// Wizard: Creates arcane area explosions at target positions.
    /// </summary>
    public class ArcaneExplosionEffect : ISkillEffect
    {
        public float BaseDamage { get; set; }
        public float ExplosionRadius { get; set; }

        private readonly List<ISpatialEntity> _hitBuffer = new List<ISpatialEntity>(16);

        public ArcaneExplosionEffect(float baseDamage = 40f, float explosionRadius = 2.0f)
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
                Vector2D center = targetPositions[i];
                int hitCount = context.TargetGrid.QueryRadiusNonAlloc(center, effectiveRadius, _hitBuffer);

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
