using System.Collections.Generic;
using HappyShoot.Domain.Entities;
using HappyShoot.Domain.Events;
using HappyShoot.Domain.Spatial;

namespace HappyShoot.Domain.Skills.Effects
{
    /// <summary>
    /// Warrior: Stomps the ground creating a violent earthquake tremor directly centered at the player's position.
    /// Balanced melee area physical damage (Radius: 2.2m, BaseDamage: 32f).
    /// </summary>
    public class GroundStompEffect : ISkillEffect, ILevelableEffect
    {
        public float BaseDamage { get; set; }
        public float StompRadius { get; set; }

        private readonly List<ISpatialEntity> _hitBuffer = new List<ISpatialEntity>(32);
        private readonly float _initialRadius;
        private readonly float _initialBaseDamage;

        public GroundStompEffect(float baseDamage = 32f, float stompRadius = 2.2f)
        {
            BaseDamage = baseDamage;
            StompRadius = stompRadius;
            _initialBaseDamage = baseDamage;
            _initialRadius = stompRadius;
        }

        public void OnLevelUp(int newLevel)
        {
            // Level 1: 2.2m, 32 dmg
            // Level 2: 2.5m (+0.3m), 40 dmg (+8 dmg)
            // Level 3: 2.8m, 48 dmg
            // Level 4: 3.1m, 56 dmg
            // Level 5: 3.4m, 64 dmg
            StompRadius = _initialRadius + 0.3f * (newLevel - 1);
            BaseDamage = _initialBaseDamage + 8f * (newLevel - 1);
        }

        public void ApplyEffect(SkillContext context, IList<Vector2D> targetPositions)
        {
            if (context?.TargetGrid == null)
                return;

            float effectiveRadius = StompRadius * context.AreaMultiplier;
            float effectiveDamage = BaseDamage * (context.BaseDamage / 10f);

            // Always centered directly on the player's position at the cast moment
            Vector2D center = context.CasterPosition;

            // Publish visual & audio events for warrior ground stomp earthquake
            context.EventBus?.Publish(new GroundStompExecutedEvent(center, effectiveRadius));
            context.EventBus?.Publish(new PlaySoundEvent(SoundEffectType.MonsterHit, volume: 0.95f));

            int hitCount = context.TargetGrid.QueryRadiusNonAlloc(center, effectiveRadius, _hitBuffer);

            for (int j = 0; j < hitCount; j++)
            {
                if (_hitBuffer[j] is MonsterEntity monster && monster.IsActive && !monster.IsDead)
                {
                    monster.TakeDamage(effectiveDamage);
                }
            }
        }
    }
}
