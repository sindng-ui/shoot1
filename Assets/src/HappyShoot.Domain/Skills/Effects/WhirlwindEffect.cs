using System.Collections.Generic;
using HappyShoot.Domain.Entities;
using HappyShoot.Domain.Events;
using HappyShoot.Domain.Spatial;

namespace HappyShoot.Domain.Skills.Effects
{
    /// <summary>
    /// Warrior exclusive skill: Sweeps a 360-degree whirlwind blade attack around the player.
    /// Base: 30 damage, 2.2m full-circle radius.
    /// Leveling: +8 damage per level, +0.3m radius per level.
    /// </summary>
    public class WhirlwindEffect : ISkillEffect, ILevelableEffect
    {
        public float BaseDamage { get; set; }
        public float Radius { get; set; }

        private readonly float _initialDamage;
        private readonly float _initialRadius;
        private readonly List<ISpatialEntity> _hitBuffer = new List<ISpatialEntity>(32);

        public WhirlwindEffect(float baseDamage = 30f, float radius = 2.2f)
        {
            BaseDamage = baseDamage;
            Radius = radius;
            _initialDamage = baseDamage;
            _initialRadius = radius;
        }

        public void OnLevelUp(int newLevel)
        {
            // Lv.1: 2.2m, 30 dmg
            // Lv.2: 2.5m, 38 dmg
            // Lv.3: 2.8m, 46 dmg
            // Lv.4: 3.1m, 54 dmg
            // Lv.5: 3.4m, 62 dmg
            Radius = _initialRadius + 0.3f * (newLevel - 1);
            BaseDamage = _initialDamage + 8f * (newLevel - 1);
        }

        public void ApplyEffect(SkillContext context, IList<Vector2D> targetPositions)
        {
            if (context?.TargetGrid == null) return;

            float effectiveRadius = Radius * context.AreaMultiplier;
            float effectiveDamage = BaseDamage * (context.BaseDamage / 10f);
            Vector2D center = context.CasterPosition;

            // Audio & Event
            context.EventBus?.Publish(new PlaySoundEvent(SoundEffectType.SlashAttack, volume: 0.85f));
            context.EventBus?.Publish(new GroundStompExecutedEvent(center, effectiveRadius));

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
