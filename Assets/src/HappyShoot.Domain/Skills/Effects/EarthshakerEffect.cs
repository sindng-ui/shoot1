using System.Collections.Generic;
using HappyShoot.Domain.Entities;
using HappyShoot.Domain.Events;
using HappyShoot.Domain.Spatial;

namespace HappyShoot.Domain.Skills.Effects
{
    /// <summary>
    /// Warrior Evolved Skill: Earthshaker (Evolved from Ground Stomp + Iron Armor).
    /// Shatters the earth with seismic fissure rupture shockwaves projecting in 4 cardinal directions and launches 16 magma boulders.
    /// Strictly modular and under 500 lines.
    /// </summary>
    public class EarthshakerEffect : ISkillEffect
    {
        public float BaseDamage { get; set; }
        public float Radius { get; set; }
        public int FissureCount { get; set; }

        private readonly List<ISpatialEntity> _hitBuffer = new List<ISpatialEntity>(64);

        public EarthshakerEffect(float baseDamage = 80f, float radius = 4.8f, int fissureCount = 4)
        {
            BaseDamage = baseDamage;
            Radius = radius;
            FissureCount = fissureCount;
        }

        public void ApplyEffect(SkillContext context, IList<Vector2D> targetPositions)
        {
            if (context?.TargetGrid == null)
                return;

            float effectiveDamage = BaseDamage * (context.BaseDamage / 10f);
            float effectiveRadius = Radius * context.AreaMultiplier;

            // Publish Domain Event for Earthshaker Seismic Rupture & Radial Fissures
            context.EventBus?.Publish(new EarthshakerExecutedEvent(context.CasterPosition, effectiveRadius, effectiveDamage, FissureCount));
            context.EventBus?.Publish(new PlaySoundEvent(SoundEffectType.SlashAttack, volume: 1.0f));

            int hitCount = context.TargetGrid.QueryRadiusNonAlloc(context.CasterPosition, effectiveRadius, _hitBuffer);
            for (int i = 0; i < hitCount; i++)
            {
                if (_hitBuffer[i] is MonsterEntity monster && monster.IsActive && !monster.IsDead)
                {
                    var (hitDmg, isCrit) = context.RollDamage(effectiveDamage);
                    monster.TakeDamage(hitDmg, isCrit);
                }
            }
        }
    }
}
