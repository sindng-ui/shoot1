using System.Collections.Generic;
using HappyShoot.Domain.Entities;
using HappyShoot.Domain.Events;
using HappyShoot.Domain.Spatial;

namespace HappyShoot.Domain.Skills.Effects
{
    /// <summary>
    /// Warrior Evolved Skill: Tempest Whirlwind (Evolved from Whirlwind + Wind Feather).
    /// Executes dual hyper-speed hurricane spins (1440 deg/s) and projects 4 razor-sharp tempest slash waves outwards.
    /// Strictly modular and under 500 lines.
    /// </summary>
    public class TempestWhirlwindEffect : ISkillEffect
    {
        public float BaseDamage { get; set; }
        public float Radius { get; set; }
        public int SlashWaveCount { get; set; }

        private readonly List<ISpatialEntity> _hitBuffer = new List<ISpatialEntity>(64);

        public TempestWhirlwindEffect(float baseDamage = 75f, float radius = 4.2f, int slashWaveCount = 4)
        {
            BaseDamage = baseDamage;
            Radius = radius;
            SlashWaveCount = slashWaveCount;
        }

        public void ApplyEffect(SkillContext context, IList<Vector2D> targetPositions)
        {
            if (context?.TargetGrid == null)
                return;

            float effectiveDamage = BaseDamage * (context.BaseDamage / 10f);
            float effectiveRadius = Radius * context.AreaMultiplier;

            // Publish Domain Event for Dual Cyclone & 4-way Tempest Slashes
            context.EventBus?.Publish(new TempestWhirlwindExecutedEvent(context.CasterPosition, effectiveRadius, effectiveDamage, SlashWaveCount));
            context.EventBus?.Publish(new PlaySoundEvent(SoundEffectType.SlashAttack, volume: 0.95f));

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
