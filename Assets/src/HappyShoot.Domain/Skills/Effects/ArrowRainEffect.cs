using System.Collections.Generic;
using HappyShoot.Domain.Entities;
using HappyShoot.Domain.Events;
using HappyShoot.Domain.Spatial;

namespace HappyShoot.Domain.Skills.Effects
{
    /// <summary>
    /// Ranger signature skill: Concentrated continuous arrow barrage over an area.
    /// Fires dense arrows over dynamic duration (1.5s - 3.5s).
    /// Each falling arrow deals individual impact damage the exact moment it hits the ground.
    /// Strictly modular and under 500 lines.
    /// </summary>
    public class ArrowRainEffect : ISkillEffect, ILevelableEffect
    {
        public float BaseDamage { get; set; }
        public float Radius { get; set; }
        public float Duration { get; set; }
        public int ArrowCount { get; set; }

        private readonly float _initialDamage;
        private readonly float _initialRadius;
        private readonly float _initialDuration;
        private readonly int _initialArrowCount;

        public ArrowRainEffect(float baseDamage = 24f, float radius = 2.0f, float duration = 1.5f, int arrowCount = 20)
        {
            BaseDamage = baseDamage;
            Radius = radius;
            Duration = duration;
            ArrowCount = arrowCount;

            _initialDamage = baseDamage;
            _initialRadius = radius;
            _initialDuration = duration;
            _initialArrowCount = arrowCount;
        }

        public void OnLevelUp(int newLevel)
        {
            Duration = _initialDuration + 0.5f * (newLevel - 1); // 1.5s -> 3.5s
            Radius = _initialRadius + 0.45f * (newLevel - 1);    // 2.0m -> 3.8m
            ArrowCount = _initialArrowCount + 10 * (newLevel - 1); // 20 -> 60 arrows
            BaseDamage = _initialDamage + 6.5f * (newLevel - 1); // 24 -> 50 dmg
        }

        public void ApplyEffect(SkillContext context, IList<Vector2D> targetPositions)
        {
            if (context == null || targetPositions == null || targetPositions.Count == 0)
                return;

            float effectiveRadius = Radius * context.AreaMultiplier;
            float effectiveDamage = BaseDamage * (context.BaseDamage / 10f);

            for (int t = 0; t < targetPositions.Count; t++)
            {
                Vector2D center = targetPositions[t];

                // Play Audio & Publish event with center, radius, duration, count, and damage per arrow
                context.EventBus?.Publish(new PlaySoundEvent(SoundEffectType.BowShoot, volume: 0.85f));
                context.EventBus?.Publish(new ArrowRainExecutedEvent(center, effectiveRadius, Duration, ArrowCount, effectiveDamage));
            }
        }
    }
}
