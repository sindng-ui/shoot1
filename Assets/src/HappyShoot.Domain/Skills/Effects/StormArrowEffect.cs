using System;
using System.Collections.Generic;
using HappyShoot.Domain.Entities;
using HappyShoot.Domain.Events;
using HappyShoot.Domain.Spatial;

namespace HappyShoot.Domain.Skills.Effects
{
    /// <summary>
    /// Evolved Piercing Bow (Storm Bow):
    /// Fires 5+ high-speed piercing gale arrows (exact same fan spread as Lv.5 Bow).
    /// On every piercing hit, triggers a satisfying mini storm blast explosion with AoE splash damage!
    /// Strictly modular and under 500 lines.
    /// </summary>
    public class StormArrowEffect : ISkillEffect
    {
        public float ArrowDamage { get; set; }
        public float ExplosionDamage { get; set; }
        public float ExplosionRadius { get; set; }
        public float Speed { get; set; }
        public int BaseArrowCount { get; set; }
        public float SpreadAngleDeg { get; set; }

        public StormArrowEffect(
            float arrowDamage = 65f,
            float explosionDamage = 45f,
            float explosionRadius = 1.6f,
            float speed = 20f,
            int arrowCount = 5,
            float spreadAngleDeg = 28f)
        {
            ArrowDamage = arrowDamage;
            ExplosionDamage = explosionDamage;
            ExplosionRadius = explosionRadius;
            Speed = speed;
            BaseArrowCount = arrowCount;
            SpreadAngleDeg = spreadAngleDeg;
        }

        public void ApplyEffect(SkillContext context, IList<Vector2D> targetPositions)
        {
            if (context?.ProjectileManager == null || targetPositions == null || targetPositions.Count == 0)
                return;

            float effectiveSpeed = Speed * context.SpeedMultiplier;
            float effectiveArrowDmg = ArrowDamage * (context.BaseDamage / 10f);
            float effectiveExplosionDmg = ExplosionDamage * (context.BaseDamage / 10f);
            float effectiveRadius = ExplosionRadius * context.AreaMultiplier;

            int extraProj = context.CasterEntity != null ? context.CasterEntity.Stats.ExtraProjectiles : 0;
            int totalArrows = Math.Max(1, BaseArrowCount + extraProj);

            for (int t = 0; t < targetPositions.Count; t++)
            {
                Vector2D baseDir = (targetPositions[t] - context.CasterPosition).Normalized;
                if (baseDir.SqrMagnitude < 1e-4f) baseDir = Vector2D.Right;

                float baseAngleRad = (float)Math.Atan2(baseDir.Y, baseDir.X);
                float spreadRad = (SpreadAngleDeg * (float)Math.PI / 180f) * (totalArrows > 3 ? 1.25f : 1.0f);
                float angleStep = totalArrows > 1 ? spreadRad / (totalArrows - 1) : 0f;
                float startAngle = baseAngleRad - spreadRad * 0.5f;

                for (int i = 0; i < totalArrows; i++)
                {
                    float angle = totalArrows > 1 ? startAngle + angleStep * i : baseAngleRad;
                    Vector2D dir = new Vector2D((float)Math.Cos(angle), (float)Math.Sin(angle));

                    // Launch via domain ProjectileManager (same as Piercing Bow, but with explosion parameters!)
                    context.ProjectileManager.LaunchProjectile(
                        origin: context.CasterPosition,
                        direction: dir,
                        speed: effectiveSpeed,
                        damage: effectiveArrowDmg,
                        pierceCount: 999,
                        lifetime: 2.5f,
                        explosionRadius: effectiveRadius,
                        explosionDamage: effectiveExplosionDmg
                    );
                }
            }

            context.EventBus?.Publish(new PlaySoundEvent(SoundEffectType.BowShoot, volume: 1.0f));
        }
    }
}
