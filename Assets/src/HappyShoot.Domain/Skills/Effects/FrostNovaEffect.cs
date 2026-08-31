using System.Collections.Generic;
using HappyShoot.Domain.Entities;
using HappyShoot.Domain.Events;
using HappyShoot.Domain.Spatial;

namespace HappyShoot.Domain.Skills.Effects
{
    /// <summary>
    /// Wizard exclusive skill: Releases an instant 360-degree freezing wave around the caster.
    /// Leveling dramatically expands freezing radius (2.8m -> 5.2m screen-spanning wave) and chill duration (3.5s -> 6.5s).
    /// Strictly modular and under 500 lines.
    /// </summary>
    public class FrostNovaEffect : ISkillEffect, ILevelableEffect
    {
        public float BaseDamage { get; set; }
        public float Radius { get; set; }
        public float ChillDuration { get; set; }

        private readonly float _initialDamage;
        private readonly float _initialRadius;
        private readonly float _initialChillDuration;
        private readonly List<ISpatialEntity> _hitBuffer = new List<ISpatialEntity>(64);

        public FrostNovaEffect(float baseDamage = 28f, float radius = 2.8f, float chillDuration = 3.5f)
        {
            BaseDamage = baseDamage;
            Radius = radius;
            ChillDuration = chillDuration;

            _initialDamage = baseDamage;
            _initialRadius = radius;
            _initialChillDuration = chillDuration;
        }

        public void OnLevelUp(int newLevel)
        {
            // Lv.1: 2.8m, 3.5s -> Lv.5: 5.2m, 6.5s chill wave
            BaseDamage = _initialDamage + 8.5f * (newLevel - 1); // 28 -> 62 dmg
            Radius = _initialRadius + 0.60f * (newLevel - 1);    // 2.8m -> 5.2m
            ChillDuration = _initialChillDuration + 0.75f * (newLevel - 1); // 3.5s -> 6.5s
        }

        public void ApplyEffect(SkillContext context, IList<Vector2D> targetPositions)
        {
            if (context?.TargetGrid == null) return;

            float effectiveRadius = Radius * context.AreaMultiplier;
            float effectiveDamage = BaseDamage * (context.BaseDamage / 10f);

            if (context.ActiveRune.IsActive)
            {
                effectiveDamage = context.ActiveRune.ApplyDamage(effectiveDamage);
                effectiveRadius = context.ActiveRune.ApplyArea(effectiveRadius);
            }

            Vector2D center = context.CasterPosition;

            // Audio & Events
            context.EventBus?.Publish(new FrostNovaExecutedEvent(center, effectiveRadius, effectiveDamage));
            context.EventBus?.Publish(new PlaySoundEvent(SoundEffectType.MagicExplosion, volume: 0.85f));

            float totalDamageDealt = 0f;
            int hitCount = context.TargetGrid.QueryRadiusNonAlloc(center, effectiveRadius, _hitBuffer);
            for (int i = 0; i < hitCount; i++)
            {
                if (_hitBuffer[i] is MonsterEntity monster && monster.IsActive && !monster.IsDead)
                {
                    monster.ApplyChill(duration: ChillDuration, slowFactor: 0.45f);
                    var (hitDmg, isCrit) = context.RollDamage(effectiveDamage);
                    monster.TakeDamage(hitDmg, isCrit);
                    totalDamageDealt += hitDmg;
                }
            }

            if (context.ActiveRune.IsActive && context.ActiveRune.LifeStealPercent > 0f && totalDamageDealt > 0f)
            {
                context.CasterEntity?.Heal(totalDamageDealt * context.ActiveRune.LifeStealPercent);
            }
        }
    }
}
