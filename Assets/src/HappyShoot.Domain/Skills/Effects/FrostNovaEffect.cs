using System.Collections.Generic;
using HappyShoot.Domain.Entities;
using HappyShoot.Domain.Events;
using HappyShoot.Domain.Spatial;

namespace HappyShoot.Domain.Skills.Effects
{
    /// <summary>
    /// Wizard exclusive skill: Releases an instant 360-degree freezing wave around the caster.
    /// Base: 28 damage, 2.8m full circle radius.
    /// Leveling: +8 damage, +0.3m radius per level.
    /// </summary>
    public class FrostNovaEffect : ISkillEffect, ILevelableEffect
    {
        public float BaseDamage { get; set; }
        public float Radius { get; set; }

        private readonly float _initialDamage;
        private readonly float _initialRadius;
        private readonly List<ISpatialEntity> _hitBuffer = new List<ISpatialEntity>(32);

        public FrostNovaEffect(float baseDamage = 28f, float radius = 2.8f)
        {
            BaseDamage = baseDamage;
            Radius = radius;
            _initialDamage = baseDamage;
            _initialRadius = radius;
        }

        public void OnLevelUp(int newLevel)
        {
            BaseDamage = _initialDamage + 8f * (newLevel - 1);
            Radius = _initialRadius + 0.3f * (newLevel - 1);
        }

        public void ApplyEffect(SkillContext context, IList<Vector2D> targetPositions)
        {
            if (context?.TargetGrid == null) return;

            float effectiveRadius = Radius * context.AreaMultiplier;
            float effectiveDamage = BaseDamage * (context.BaseDamage / 10f);
            Vector2D center = context.CasterPosition;

            // Audio & Events
            context.EventBus?.Publish(new FrostNovaExecutedEvent(center, effectiveRadius, effectiveDamage));
            context.EventBus?.Publish(new PlaySoundEvent(SoundEffectType.MagicExplosion, volume: 0.75f));

            int hitCount = context.TargetGrid.QueryRadiusNonAlloc(center, effectiveRadius, _hitBuffer);
            for (int i = 0; i < hitCount; i++)
            {
                if (_hitBuffer[i] is MonsterEntity monster && monster.IsActive && !monster.IsDead)
                {
                    monster.ApplyChill(duration: 3.5f, slowFactor: 0.40f);
                    monster.TakeDamage(effectiveDamage);
                }
            }
        }
    }
}
