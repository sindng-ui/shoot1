using System;
using System.Collections.Generic;
using HappyShoot.Domain.Spatial;

namespace HappyShoot.Domain.Skills.Effects
{
    /// <summary>
    /// Evolved Piercing Bow (Storm Arrow): Rapid 8-directional high-speed splitting arrows.
    /// </summary>
    public class StormArrowEffect : ISkillEffect
    {
        public float BaseDamage { get; set; }
        public float Speed { get; set; }
        public int ProjectileCount { get; set; }

        public StormArrowEffect(float baseDamage = 45f, float speed = 18f, int projectileCount = 8)
        {
            BaseDamage = baseDamage;
            Speed = speed;
            ProjectileCount = projectileCount;
        }

        public void ApplyEffect(SkillContext context, IList<Vector2D> targetPositions)
        {
            if (context?.ProjectileManager == null) return;

            float effectiveSpeed = Speed * context.SpeedMultiplier;
            float effectiveDamage = BaseDamage * (context.BaseDamage / 10f);

            // Fire in a radial storm pattern (8 directions)
            float angleStep = (float)(Math.PI * 2.0 / ProjectileCount);
            for (int i = 0; i < ProjectileCount; i++)
            {
                float angle = i * angleStep;
                Vector2D dir = new Vector2D((float)Math.Cos(angle), (float)Math.Sin(angle));

                context.ProjectileManager.LaunchProjectile(
                    origin: context.CasterPosition,
                    direction: dir,
                    speed: effectiveSpeed,
                    damage: effectiveDamage,
                    pierceCount: 5,
                    lifetime: 3.0f
                );
            }
        }
    }
}
