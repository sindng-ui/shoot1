using System.Collections.Generic;
using HappyShoot.Domain.Entities;
using HappyShoot.Domain.Events;
using HappyShoot.Domain.Spatial;

namespace HappyShoot.Domain.Skills.Effects
{
    /// <summary>
    /// Ranger exclusive skill: Calls down a concentrated barrage of arrows on a target area over 2.0 seconds.
    /// Base: 24 damage, 2.2m area radius (+20% expanded).
    /// Leveling: +5 damage per level, +0.25m radius per level.
    /// </summary>
    public class ArrowRainEffect : ISkillEffect, ILevelableEffect
    {
        public float BaseDamage { get; set; }
        public float Radius { get; set; }

        private readonly float _initialDamage;
        private readonly float _initialRadius;
        private readonly List<ISpatialEntity> _hitBuffer = new List<ISpatialEntity>(32);

        public ArrowRainEffect(float baseDamage = 24f, float radius = 2.2f)
        {
            BaseDamage = baseDamage;
            Radius = radius;
            _initialDamage = baseDamage;
            _initialRadius = radius;
        }

        public void OnLevelUp(int newLevel)
        {
            // Lv.1: 2.2m, 24 dmg
            // Lv.2: 2.45m, 29 dmg
            // Lv.3: 2.7m, 34 dmg
            // Lv.4: 2.95m, 39 dmg
            // Lv.5: 3.2m, 44 dmg
            Radius = _initialRadius + 0.25f * (newLevel - 1);
            BaseDamage = _initialDamage + 5f * (newLevel - 1);
        }

        public void ApplyEffect(SkillContext context, IList<Vector2D> targetPositions)
        {
            if (context?.TargetGrid == null || targetPositions == null || targetPositions.Count == 0)
                return;

            float effectiveRadius = Radius * context.AreaMultiplier;
            float effectiveDamage = BaseDamage * (context.BaseDamage / 10f);

            for (int t = 0; t < targetPositions.Count; t++)
            {
                Vector2D center = targetPositions[t];

                // Play Audio & Dedicated Arrow Rain Visual Event (2.0s continuous rain)
                context.EventBus?.Publish(new PlaySoundEvent(SoundEffectType.BowShoot, volume: 0.85f));
                context.EventBus?.Publish(new ArrowRainExecutedEvent(center, effectiveRadius, 2.0f));

                int hitCount = context.TargetGrid.QueryRadiusNonAlloc(center, effectiveRadius, _hitBuffer);

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
}
