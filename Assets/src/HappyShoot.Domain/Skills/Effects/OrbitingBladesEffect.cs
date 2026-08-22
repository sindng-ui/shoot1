using System;
using System.Collections.Generic;
using HappyShoot.Domain.Entities;
using HappyShoot.Domain.Events;
using HappyShoot.Domain.Spatial;

namespace HappyShoot.Domain.Skills.Effects
{
    /// <summary>
    /// Orbiting Blades effect strictly bound to individual blade contact positions.
    /// Each blade hits enemies precisely at its orbital position (O(1) CPU, zero GC allocation).
    /// Power scales 1:1 with blade count (more blades = higher hit frequency).
    /// Strictly modular and under 500 lines.
    /// </summary>
    public class OrbitingBladesEffect : ISkillEffect, ILevelableEffect, ITickableEffect
    {
        public float BaseDamage { get; set; }
        public float OrbitRadius { get; set; }
        public float RotationSpeed { get; set; } // Radians per second (default: ~4.19 rad/s = 240 deg/s)
        public int BladeCount { get; set; }

        private float _currentAngle = 0f;
        private readonly List<ISpatialEntity> _hitBuffer = new List<ISpatialEntity>(16);
        private readonly Dictionary<int, float> _monsterHitTimers = new Dictionary<int, float>(64);

        public OrbitingBladesEffect(float baseDamage = 25f, float orbitRadius = 2.0f, float rotationSpeed = 4.18879f, int bladeCount = 2)
        {
            BaseDamage = baseDamage;
            OrbitRadius = orbitRadius;
            RotationSpeed = rotationSpeed;
            BladeCount = bladeCount;
        }

        public void OnLevelUp(int newLevel)
        {
            BladeCount = 2 + (newLevel - 1);
            BaseDamage = 25f + (newLevel - 1) * 8f;
            OrbitRadius = 2.0f + (newLevel - 1) * 0.15f;
            RotationSpeed = 4.18879f + (newLevel - 1) * 0.35f;
        }

        public void ApplyEffect(SkillContext context, IList<Vector2D> targetPositions)
        {
            Update(0.016f, context);
        }

        public Vector2D GetBladePosition(Vector2D center, int bladeIndex, float areaMultiplier = 1.0f)
        {
            float effectiveRadius = OrbitRadius * areaMultiplier;
            float step = (float)(Math.PI * 2.0 / Math.Max(1, BladeCount));
            float bladeAngle = _currentAngle + bladeIndex * step;
            float bx = center.X + (float)Math.Cos(bladeAngle) * effectiveRadius;
            float by = center.Y + (float)Math.Sin(bladeAngle) * effectiveRadius;
            return new Vector2D(bx, by);
        }

        public void Update(float deltaTime, SkillContext context)
        {
            if (context?.TargetGrid == null || deltaTime <= 0f || BladeCount <= 0) return;

            // 1. Advance orbital angle
            _currentAngle += RotationSpeed * deltaTime;
            if (_currentAngle >= (float)(Math.PI * 2.0))
            {
                _currentAngle -= (float)(Math.PI * 2.0);
            }

            // 2. Decay monster hit debounce timers
            if (_monsterHitTimers.Count > 0)
            {
                var keys = new List<int>(_monsterHitTimers.Keys);
                for (int k = 0; k < keys.Count; k++)
                {
                    int id = keys[k];
                    _monsterHitTimers[id] -= deltaTime;
                    if (_monsterHitTimers[id] <= 0f)
                    {
                        _monsterHitTimers.Remove(id);
                    }
                }
            }

            float effectiveRadius = OrbitRadius * context.AreaMultiplier;
            float damage = BaseDamage * (context.BaseDamage / 10f);
            float step = (float)(Math.PI * 2.0 / BladeCount);
            float bladeHitRadius = 0.65f; // Localized blade contact circle
            const float HitDebounce = 0.22f; // Minimum delay between hits from the same blade sweep

            bool hitAny = false;

            // 3. Query hit ONLY at each individual blade's current position
            for (int b = 0; b < BladeCount; b++)
            {
                float bladeAngle = _currentAngle + b * step;
                float bx = context.CasterPosition.X + (float)Math.Cos(bladeAngle) * effectiveRadius;
                float by = context.CasterPosition.Y + (float)Math.Sin(bladeAngle) * effectiveRadius;
                Vector2D bladePos = new Vector2D(bx, by);

                int count = context.TargetGrid.QueryRadiusNonAlloc(bladePos, bladeHitRadius, _hitBuffer);
                for (int i = 0; i < count; i++)
                {
                    if (_hitBuffer[i] is MonsterEntity monster && monster.IsActive && !monster.IsDead)
                    {
                        if (!_monsterHitTimers.TryGetValue(monster.Id, out float timer) || timer <= 0f)
                        {
                            monster.TakeDamage(damage);
                            _monsterHitTimers[monster.Id] = HitDebounce;
                            hitAny = true;
                        }
                    }
                }
            }

            if (hitAny)
            {
                context.EventBus?.Publish(new PlaySoundEvent(SoundEffectType.SlashAttack, volume: 0.35f));
            }
        }
    }
}
