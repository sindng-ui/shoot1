using System;
using System.Collections.Generic;
using HappyShoot.Domain.Spatial;

namespace HappyShoot.Domain.Skills.Effects
{
    /// <summary>
    /// Ranger exclusive skill: Fires a fan-shaped spread of piercing arrows with limited range.
    /// Base: 3 arrows in a 30-degree arc, 2 Pierce (hits 3 enemies each), 6.5m range.
    /// Leveling: +1 arrow at Lv.3 and Lv.5 (up to 5 arrows), +5 damage per level.
    /// </summary>
    public class MultiShotEffect : ISkillEffect, ILevelableEffect
    {
        public float BaseDamage { get; set; }
        public float Speed { get; set; }
        public int ArrowCount { get; set; }
        public int PierceCount { get; set; }
        public float SpreadAngleDeg { get; set; }
        public float Range { get; set; }

        private readonly float _initialDamage;
        private readonly int _initialArrowCount;

        public MultiShotEffect(
            float baseDamage = 18f,
            float speed = 16f,
            int arrowCount = 3,
            int pierceCount = 2,
            float spreadAngleDeg = 35f,
            float range = 6.5f)
        {
            BaseDamage = baseDamage;
            Speed = speed;
            ArrowCount = arrowCount;
            PierceCount = pierceCount;
            SpreadAngleDeg = spreadAngleDeg;
            Range = range;

            _initialDamage = baseDamage;
            _initialArrowCount = arrowCount;
        }

        public void OnLevelUp(int newLevel)
        {
            // Lv.1: 3 arrows, 18 dmg
            // Lv.2: 3 arrows, 23 dmg
            // Lv.3: 4 arrows, 28 dmg
            // Lv.4: 4 arrows, 33 dmg
            // Lv.5: 5 arrows, 38 dmg
            ArrowCount = _initialArrowCount + (newLevel - 1) / 2;
            BaseDamage = _initialDamage + 5f * (newLevel - 1);
        }

        public void ApplyEffect(SkillContext context, IList<Vector2D> targetPositions)
        {
            if (context?.ProjectileManager == null || targetPositions == null || targetPositions.Count == 0)
                return;

            float effectiveSpeed = Speed * context.SpeedMultiplier;
            float effectiveDamage = BaseDamage * (context.BaseDamage / 10f);
            float lifetime = Range / effectiveSpeed;

            for (int t = 0; t < targetPositions.Count; t++)
            {
                Vector2D baseDir = (targetPositions[t] - context.CasterPosition).Normalized;
                if (baseDir.SqrMagnitude < 1e-4f) baseDir = Vector2D.Right;

                float baseAngleRad = (float)Math.Atan2(baseDir.Y, baseDir.X);
                float spreadRad = SpreadAngleDeg * (float)Math.PI / 180f;
                float angleStep = ArrowCount > 1 ? spreadRad / (ArrowCount - 1) : 0f;
                float startAngle = baseAngleRad - spreadRad * 0.5f;

                for (int i = 0; i < ArrowCount; i++)
                {
                    float angle = ArrowCount > 1 ? startAngle + angleStep * i : baseAngleRad;
                    Vector2D dir = new Vector2D((float)Math.Cos(angle), (float)Math.Sin(angle));

                    context.ProjectileManager.LaunchProjectile(
                        origin: context.CasterPosition,
                        direction: dir,
                        speed: effectiveSpeed,
                        damage: effectiveDamage,
                        pierceCount: PierceCount,
                        lifetime: lifetime
                    );
                }
            }
        }
    }
}
