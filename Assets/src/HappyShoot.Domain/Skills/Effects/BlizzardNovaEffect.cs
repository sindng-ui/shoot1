using System.Collections.Generic;
using HappyShoot.Domain.Entities;
using HappyShoot.Domain.Events;
using HappyShoot.Domain.Spatial;

namespace HappyShoot.Domain.Skills.Effects
{
    /// <summary>
    /// Wizard Evolved Skill: Blizzard Nova (Evolved from Frost Nova + Heart Pendant).
    /// Releases double-expanding glacial shockwaves with 8 flying piercing ice shards, freezing and chilling all surrounding foes.
    /// Strictly modular and under 500 lines.
    /// </summary>
    public class BlizzardNovaEffect : ISkillEffect
    {
        public float BaseDamage { get; set; }
        public float Radius { get; set; }
        public int ShardCount { get; set; }

        private readonly List<ISpatialEntity> _hitBuffer = new List<ISpatialEntity>(64);

        public BlizzardNovaEffect(float baseDamage = 70f, float radius = 5.2f, int shardCount = 8)
        {
            BaseDamage = baseDamage;
            Radius = radius;
            ShardCount = shardCount;
        }

        public void ApplyEffect(SkillContext context, IList<Vector2D> targetPositions)
        {
            if (context?.TargetGrid == null)
                return;

            float effectiveDamage = BaseDamage * (context.BaseDamage / 10f);
            float effectiveRadius = Radius * context.AreaMultiplier;

            // Publish Domain Event for Double-Expanding Blizzard Shockwave & Glacial Shards
            context.EventBus?.Publish(new BlizzardNovaExecutedEvent(context.CasterPosition, effectiveRadius, effectiveDamage, ShardCount));
            context.EventBus?.Publish(new PlaySoundEvent(SoundEffectType.MagicExplosion, volume: 0.95f));

            int hitCount = context.TargetGrid.QueryRadiusNonAlloc(context.CasterPosition, effectiveRadius, _hitBuffer);
            for (int i = 0; i < hitCount; i++)
            {
                if (_hitBuffer[i] is MonsterEntity monster && monster.IsActive && !monster.IsDead)
                {
                    // Apply 60% chill slowdown and damage
                    monster.ApplyChill(duration: 5.0f, slowFactor: 0.40f);
                    var (hitDmg, isCrit) = context.RollDamage(effectiveDamage);
                    monster.TakeDamage(hitDmg, isCrit);
                }
            }
        }
    }
}
