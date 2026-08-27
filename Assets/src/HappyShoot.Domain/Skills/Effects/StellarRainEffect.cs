using System.Collections.Generic;
using HappyShoot.Domain.Entities;
using HappyShoot.Domain.Events;
using HappyShoot.Domain.Spatial;

namespace HappyShoot.Domain.Skills.Effects
{
    /// <summary>
    /// Ranger Evolved Skill: Stellar Rain (Evolved from Arrow Rain + Golden Ring).
    /// Showers down 60 high-density stardust meteor arrows onto the targeted area, triggering celestial starbursts on impact.
    /// Strictly modular and under 500 lines.
    /// </summary>
    public class StellarRainEffect : ISkillEffect
    {
        public float BaseDamage { get; set; }
        public float Radius { get; set; }
        public int ArrowCount { get; set; }
        public float Duration { get; set; }

        private readonly List<ISpatialEntity> _hitBuffer = new List<ISpatialEntity>(64);

        public StellarRainEffect(float baseDamage = 75f, float radius = 5.0f, int arrowCount = 60, float duration = 2.0f)
        {
            BaseDamage = baseDamage;
            Radius = radius;
            ArrowCount = arrowCount;
            Duration = duration;
        }

        public void ApplyEffect(SkillContext context, IList<Vector2D> targetPositions)
        {
            if (context?.TargetGrid == null || targetPositions == null || targetPositions.Count == 0)
                return;

            float effectiveDamage = BaseDamage * (context.BaseDamage / 10f);
            float effectiveRadius = Radius * context.AreaMultiplier;
            Vector2D targetCenter = targetPositions[0];

            // Publish Domain Event for Presentation Manager
            context.EventBus?.Publish(new StellarRainExecutedEvent(targetCenter, effectiveRadius, effectiveDamage, ArrowCount, Duration));
            context.EventBus?.Publish(new PlaySoundEvent(SoundEffectType.BowShoot, volume: 0.95f));

            // Domain impact damage
            int count = context.TargetGrid.QueryRadiusNonAlloc(targetCenter, effectiveRadius, _hitBuffer);
            for (int i = 0; i < count; i++)
            {
                if (_hitBuffer[i] is MonsterEntity monster && monster.IsActive && !monster.IsDead)
                {
                    var (hitDmg, isCrit) = context.RollDamage(effectiveDamage);
                    monster.TakeDamage(hitDmg, isCrit, DamageType.StellarRain);
                }
            }
        }
    }
}
