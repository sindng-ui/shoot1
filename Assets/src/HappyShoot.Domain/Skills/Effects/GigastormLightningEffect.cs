using System;
using System.Collections.Generic;
using HappyShoot.Domain.Entities;
using HappyShoot.Domain.Events;
using HappyShoot.Domain.Spatial;

namespace HappyShoot.Domain.Skills.Effects
{
    /// <summary>
    /// Wizard Evolved Skill: Gigastorm Chain Lightning (Evolved from Chain Lightning + Overcharge Core).
    /// Chains across up to 8 enemies in rapid succession, triggering plasma spark bursts at each node and inflicting 100% Shock.
    /// Strictly modular and under 500 lines.
    /// </summary>
    public class GigastormLightningEffect : ISkillEffect
    {
        public float BaseDamage { get; set; }
        public int ChainCount { get; set; }
        public float ChainRange { get; set; }
        public float SparkRadius { get; set; }

        private readonly List<ISpatialEntity> _queryBuffer = new List<ISpatialEntity>(32);
        private readonly List<ISpatialEntity> _splashBuffer = new List<ISpatialEntity>(16);
        private readonly HashSet<int> _hitMonsterIds = new HashSet<int>();

        public GigastormLightningEffect(float baseDamage = 85f, int chainCount = 10, float chainRange = 7.5f, float sparkRadius = 2.2f)
        {
            BaseDamage = baseDamage;
            ChainCount = chainCount;
            ChainRange = chainRange;
            SparkRadius = sparkRadius;
        }

        public void ApplyEffect(SkillContext context, IList<Vector2D> targetPositions)
        {
            if (context?.TargetGrid == null || targetPositions == null || targetPositions.Count == 0)
                return;

            float effectiveDamage = BaseDamage * (context.BaseDamage / 10f);
            float effectiveChainRange = ChainRange * context.AreaMultiplier;
            float effectiveSparkRadius = SparkRadius * context.AreaMultiplier;

            Vector2D currentPos = context.CasterPosition;
            _hitMonsterIds.Clear();

            List<Vector2D> struckPositions = new List<Vector2D>(ChainCount);

            for (int chain = 0; chain < ChainCount; chain++)
            {
                int count = context.TargetGrid.QueryRadiusNonAlloc(currentPos, effectiveChainRange, _queryBuffer);
                MonsterEntity closestEnemy = null;
                float closestDistSq = float.MaxValue;

                for (int i = 0; i < count; i++)
                {
                    if (_queryBuffer[i] is MonsterEntity monster && monster.IsActive && !monster.IsDead)
                    {
                        if (_hitMonsterIds.Contains(monster.Id))
                            continue;

                        float distSq = (float)(monster.Position - currentPos).SqrMagnitude;
                        if (distSq < closestDistSq)
                        {
                            closestDistSq = distSq;
                            closestEnemy = monster;
                        }
                    }
                }

                if (closestEnemy == null)
                    break;

                _hitMonsterIds.Add(closestEnemy.Id);
                currentPos = closestEnemy.Position;
                struckPositions.Add(currentPos);

                // Inflict primary damage and guaranteed Shock DoT
                closestEnemy.ApplyShock(duration: 7.0f, damagePerTick: effectiveDamage * 0.18f);
                var (hitDmg, isCrit) = context.RollDamage(effectiveDamage);
                closestEnemy.TakeDamage(hitDmg, isCrit);

                // Overcharge Node Plasma Splash: Area discharge to adjacent enemies around each struck node
                if (effectiveSparkRadius > 0.5f)
                {
                    int splashCount = context.TargetGrid.QueryRadiusNonAlloc(currentPos, effectiveSparkRadius, _splashBuffer);
                    float splashDmg = effectiveDamage * 0.35f;
                    for (int s = 0; s < splashCount; s++)
                    {
                        if (_splashBuffer[s] is MonsterEntity splashMonster && splashMonster.IsActive && !splashMonster.IsDead && splashMonster.Id != closestEnemy.Id)
                        {
                            splashMonster.ApplyShock(duration: 4.0f, damagePerTick: splashDmg * 0.15f);
                            var (sDmg, sCrit) = context.RollDamage(splashDmg);
                            splashMonster.TakeDamage(sDmg, sCrit);
                        }
                    }
                }
            }

            if (struckPositions.Count > 0)
            {
                context.EventBus?.Publish(new GigastormLightningExecutedEvent(context.CasterPosition, struckPositions, effectiveDamage, effectiveSparkRadius));
                context.EventBus?.Publish(new PlaySoundEvent(SoundEffectType.MagicExplosion, volume: 0.95f));
            }
        }
    }
}
