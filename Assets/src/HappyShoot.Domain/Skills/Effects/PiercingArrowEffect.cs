using System.Collections.Generic;
using HappyShoot.Domain.Spatial;

namespace HappyShoot.Domain.Skills.Effects
{
    /// <summary>
    /// Ranger: Fires high-speed piercing arrows that pierce infinitely through all enemies across the entire screen.
    /// Base: Infinite Pierce (999), 22 base damage, 16m/s speed.
    /// Leveling: +7 damage and +1m/s speed per level.
    /// </summary>
    public class PiercingArrowEffect : ISkillEffect, ILevelableEffect
    {
        public float BaseDamage { get; set; }
        public float Speed { get; set; }
        public int PierceCount { get; set; }

        private readonly float _initialBaseDamage;
        private readonly float _initialSpeed;

        public PiercingArrowEffect(float baseDamage = 22f, float speed = 16f, int pierceCount = 999)
        {
            BaseDamage = baseDamage;
            Speed = speed;
            PierceCount = pierceCount;
            _initialBaseDamage = baseDamage;
            _initialSpeed = speed;
        }

        public void OnLevelUp(int newLevel)
        {
            PierceCount = 999;
            BaseDamage = _initialBaseDamage + 7f * (newLevel - 1);
            Speed = _initialSpeed + 1.0f * (newLevel - 1);
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
