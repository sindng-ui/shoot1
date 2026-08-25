using System;
using System.Collections.Generic;
using HappyShoot.Domain.Events;
using HappyShoot.Domain.Spatial;

namespace HappyShoot.Domain.Skills.Effects
{
    /// <summary>
    /// Ranger: Fires high-speed piercing arrows that pierce infinitely (999) through all enemies across the screen.
    /// Gains +1 additional arrow per level in a fan spread (Lv.1: 1 -> Lv.5: 5 arrows).
    /// Strictly modular and under 500 lines.
    /// </summary>
    public class PiercingArrowEffect : ISkillEffect, ILevelableEffect
    {
        public float BaseDamage { get; set; }
        public float Speed { get; set; }
        public int ArrowCount { get; set; }
        public int PierceCount { get; set; }
        public float SpreadAngleDeg { get; set; }

        private readonly float _initialBaseDamage;
        private readonly float _initialSpeed;

        public PiercingArrowEffect(float baseDamage = 25f, float speed = 16f, int arrowCount = 1, int pierceCount = 999, float spreadAngleDeg = 28f)
        {
            BaseDamage = baseDamage;
            Speed = speed;
            ArrowCount = arrowCount;
            PierceCount = pierceCount;
            SpreadAngleDeg = spreadAngleDeg;
            _initialBaseDamage = baseDamage;
            _initialSpeed = speed;
        }

        public void OnLevelUp(int newLevel)
        {
            ArrowCount = 1 + (newLevel - 1); // Lv.1: 1 -> Lv.2: 2 -> Lv.3: 3 -> Lv.4: 4 -> Lv.5: 5
            BaseDamage = _initialBaseDamage + 7f * (newLevel - 1);
            Speed = _initialSpeed + 1.0f * (newLevel - 1);
            PierceCount = 999;
        }

        public void ApplyEffect(SkillContext context, IList<Vector2D> targetPositions)
        {
            if (context?.ProjectileManager == null || targetPositions == null || targetPositions.Count == 0)
                return;

            float effectiveSpeed = Speed * context.SpeedMultiplier;
            float effectiveDamage = BaseDamage * (context.BaseDamage / 10f);

            int extraProj = context.CasterEntity != null ? context.CasterEntity.Stats.ExtraProjectiles : 0;
            int totalArrows = Math.Max(1, ArrowCount + extraProj);

            float critChance = context.CasterEntity != null ? context.CasterEntity.Stats.CritChance : 0f;
            float critDmgMult = context.CasterEntity != null ? context.CasterEntity.Stats.CritDamageMultiplier : 1.5f;

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

                    context.ProjectileManager.LaunchProjectile(
                        origin: context.CasterPosition,
                        direction: dir,
                        speed: effectiveSpeed,
                        damage: effectiveDamage,
                        pierceCount: PierceCount,
                        lifetime: 2.5f,
                        explosionRadius: 0f,
                        explosionDamage: 0f,
                        critChance: critChance,
                        critDamageMultiplier: critDmgMult
                    );
                }

                // Audio & visual feedback: Crisply audible bowstring release and bow recoil kickback
                context.EventBus?.Publish(new PlaySoundEvent(SoundEffectType.BowShoot, volume: 1.0f));
                context.EventBus?.Publish(new PiercingArrowExecutedEvent(context.CasterPosition, baseDir, totalArrows));
            }
        }
    }
}
