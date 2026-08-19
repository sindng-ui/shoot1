using System;
using System.Collections.Generic;
using HappyShoot.Domain.Entities;
using HappyShoot.Domain.Spatial;

namespace HappyShoot.Domain.Skills.Effects
{
    /// <summary>
    /// Orbiting Blades / Holy Water effect that revolves around the player and damages all collided enemies.
    /// </summary>
    public class OrbitingBladesEffect : ISkillEffect
    {
        public float BaseDamage { get; set; }
        public float OrbitRadius { get; set; }
        public float RotationSpeed { get; set; }
        public int BladeCount { get; set; }

        private float _currentAngle;
        private readonly List<ISpatialEntity> _hitBuffer = new List<ISpatialEntity>(16);

        public OrbitingBladesEffect(float baseDamage = 25f, float orbitRadius = 2.0f, float rotationSpeed = 3.5f, int bladeCount = 2)
        {
            BaseDamage = baseDamage;
            OrbitRadius = orbitRadius;
            RotationSpeed = rotationSpeed;
            BladeCount = bladeCount;
            _currentAngle = 0f;
        }

        public void ApplyEffect(SkillContext context, IList<Vector2D> targetPositions)
        {
            if (context?.TargetGrid == null) return;

            // Advance rotation angle
            _currentAngle += RotationSpeed * 0.1f;
            if (_currentAngle > (float)Math.PI * 2f) _currentAngle -= (float)Math.PI * 2f;

            float step = (float)(Math.PI * 2.0 / BladeCount);
            float damage = BaseDamage * (context.BaseDamage / 10f);
            float hitRadius = 0.6f * context.AreaMultiplier;

            for (int i = 0; i < BladeCount; i++)
            {
                float angle = _currentAngle + i * step;
                float bx = context.CasterPosition.X + (float)Math.Cos(angle) * (OrbitRadius * context.AreaMultiplier);
                float by = context.CasterPosition.Y + (float)Math.Sin(angle) * (OrbitRadius * context.AreaMultiplier);
                Vector2D bladePos = new Vector2D(bx, by);

                int hitCount = context.TargetGrid.QueryRadiusNonAlloc(bladePos, hitRadius, _hitBuffer);
                for (int m = 0; m < hitCount; m++)
                {
                    if (_hitBuffer[m] is MonsterEntity monster && monster.IsActive && !monster.IsDead)
                    {
                        monster.TakeDamage(damage);
                    }
                }
            }
        }

        public Vector2D GetBladePosition(Vector2D playerPos, int bladeIndex, float areaMultiplier = 1.0f)
        {
            float step = (float)(Math.PI * 2.0 / BladeCount);
            float angle = _currentAngle + bladeIndex * step;
            float bx = playerPos.X + (float)Math.Cos(angle) * (OrbitRadius * areaMultiplier);
            float by = playerPos.Y + (float)Math.Sin(angle) * (OrbitRadius * areaMultiplier);
            return new Vector2D(bx, by);
        }
    }
}
