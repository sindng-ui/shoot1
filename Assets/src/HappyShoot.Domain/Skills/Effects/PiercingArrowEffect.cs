using System.Collections.Generic;
using HappyShoot.Domain.Spatial;

namespace HappyShoot.Domain.Skills.Effects
{
    /// <summary>
    /// Ranger: Fires high-speed piercing arrows toward targets.
    /// </summary>
    public class PiercingArrowEffect : ISkillEffect
    {
        public float BaseDamage { get; set; }
        public float Speed { get; set; }
        public int PierceCount { get; set; }

        public PiercingArrowEffect(float baseDamage = 20f, float speed = 14f, int pierceCount = 3)
        {
            BaseDamage = baseDamage;
            Speed = speed;
            PierceCount = pierceCount;
        }

        public void ApplyEffect(SkillContext context, IList<Vector2D> targetPositions)
        {
            if (context?.ProjectileManager == null || targetPositions == null || targetPositions.Count == 0)
                return;

            float effectiveSpeed = Speed * context.SpeedMultiplier;
            float effectiveDamage = BaseDamage * (context.BaseDamage / 10f);

            for (int i = 0; i < targetPositions.Count; i++)
            {
                Vector2D direction = (targetPositions[i] - context.CasterPosition).Normalized;
                if (direction.SqrMagnitude < 1e-4f) direction = Vector2D.Right;

                context.ProjectileManager.LaunchProjectile(
                    origin: context.CasterPosition,
                    direction: direction,
                    speed: effectiveSpeed,
                    damage: effectiveDamage,
                    pierceCount: PierceCount,
                    lifetime: 2.5f
                );
            }
        }
    }
}
