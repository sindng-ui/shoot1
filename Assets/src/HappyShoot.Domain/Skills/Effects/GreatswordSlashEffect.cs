using System.Collections.Generic;
using HappyShoot.Domain.Entities;
using HappyShoot.Domain.Spatial;

namespace HappyShoot.Domain.Skills.Effects
{
    /// <summary>
    /// Warrior: Slashes in front/around the player in a melee arc, hitting all enemies in range.
    /// </summary>
    public class GreatswordSlashEffect : ISkillEffect
    {
        public float BaseDamage { get; set; }
        public float Radius { get; set; }

        private readonly List<ISpatialEntity> _hitBuffer = new List<ISpatialEntity>(16);

        public GreatswordSlashEffect(float baseDamage = 35f, float radius = 2.5f)
        {
            BaseDamage = baseDamage;
            Radius = radius;
        }

        public void ApplyEffect(SkillContext context, IList<Vector2D> targetPositions)
        {
            if (context?.TargetGrid == null) return;

            float effectiveRadius = Radius * context.AreaMultiplier;
            float effectiveDamage = BaseDamage * (context.BaseDamage / 10f);

            int hitCount = context.TargetGrid.QueryRadiusNonAlloc(context.CasterPosition, effectiveRadius, _hitBuffer);
            for (int i = 0; i < hitCount; i++)
            {
                if (_hitBuffer[i] is MonsterEntity monster && monster.IsActive && !monster.IsDead)
                {
                    monster.TakeDamage(effectiveDamage);
                }
            }
        }
    }
}
