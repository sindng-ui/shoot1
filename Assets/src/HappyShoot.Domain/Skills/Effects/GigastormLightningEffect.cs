using System;
using System.Collections.Generic;
using HappyShoot.Domain.Entities;
using HappyShoot.Domain.Events;
using HappyShoot.Domain.Spatial;

namespace HappyShoot.Domain.Skills.Effects
{
    /// <summary>
    /// Wizard Evolved Skill: Gigastorm Chain Lightning (Evolved from Chain Lightning + Overcharge Core).
    /// Supports 1 to 3 concurrent lightning streams (forks) that independently chain across up to ChainCount enemies,
    /// triggering plasma spark bursts at each node and inflicting guaranteed Shock.
    /// Strictly modular and under 500 lines.
    /// </summary>
    public class GigastormLightningEffect : ISkillEffect
    {
        public float BaseDamage { get; set; }
        public int ChainCount { get; set; }
        public float ChainRange { get; set; }
        public float SparkRadius { get; set; }
        public int StreamCount { get; set; }

        private readonly List<ISpatialEntity> _queryBuffer = new List<ISpatialEntity>(32);
        private readonly List<ISpatialEntity> _splashBuffer = new List<ISpatialEntity>(16);
        private readonly HashSet<int> _hitMonsterIds = new HashSet<int>();
        private readonly List<MonsterEntity> _startTargets = new List<MonsterEntity>(4);

        public GigastormLightningEffect(
            float baseDamage = 85f,
            int chainCount = 10,
            float chainRange = 7.5f,
            float sparkRadius = 2.2f,
            int streamCount = 1)
        {
            BaseDamage = baseDamage;
            ChainCount = chainCount;
            ChainRange = chainRange;
            SparkRadius = sparkRadius;
            StreamCount = streamCount;
        }

        public void ApplyEffect(SkillContext context, IList<Vector2D> targetPositions)
        {
            if (context?.TargetGrid == null || targetPositions == null || targetPositions.Count == 0)
                return;

            float effectiveDamage = BaseDamage * (context.BaseDamage / 10f);
            float effectiveChainRange = ChainRange * context.AreaMultiplier;
            float effectiveSparkRadius = SparkRadius * context.AreaMultiplier;
            int forks = Math.Clamp(StreamCount, 1, 3);

            _hitMonsterIds.Clear();
            _startTargets.Clear();

            // 1. Query initial potential targets around the caster to initiate up to 'forks' distinct streams
            _queryBuffer.Clear();
            int count = context.TargetGrid.QueryRadiusNonAlloc(context.CasterPosition, effectiveChainRange, _queryBuffer);

            // Sort enemies by distance to caster to pick the closest N distinct enemies
            var candidates = new List<(MonsterEntity monster, float distSq)>(count);
            for (int i = 0; i < count; i++)
            {
                if (_queryBuffer[i] is MonsterEntity m && m.IsActive && !m.IsDead)
                {
                    float distSq = (float)(m.Position - context.CasterPosition).SqrMagnitude;
                    candidates.Add((m, distSq));
                }
            }

            candidates.Sort((a, b) => a.distSq.CompareTo(b.distSq));
            int pickCount = Math.Min(forks, candidates.Count);
            for (int i = 0; i < pickCount; i++)
            {
                _startTargets.Add(candidates[i].monster);
            }

            // Fallback: If no enemies within range, nothing happens
            if (_startTargets.Count == 0)
                return;

            bool playedSound = false;

            // 2. Execute each lightning stream independently
            for (int f = 0; f < _startTargets.Count; f++)
            {
                var currentTarget = _startTargets[f];
                _hitMonsterIds.Add(currentTarget.Id);

                var struckPositions = new List<Vector2D>(ChainCount);
                Vector2D currentPos = currentTarget.Position;
                struckPositions.Add(currentPos);

                ApplyNodeDamageAndShock(context, currentTarget, effectiveDamage, effectiveSparkRadius);

                // Chain to subsequent enemies
                for (int chain = 1; chain < ChainCount; chain++)
                {
                    _queryBuffer.Clear();
                    int nearCount = context.TargetGrid.QueryRadiusNonAlloc(currentPos, effectiveChainRange, _queryBuffer);
                    MonsterEntity closestEnemy = null;
                    float closestDistSq = float.MaxValue;

                    for (int i = 0; i < nearCount; i++)
                    {
                        if (_queryBuffer[i] is MonsterEntity m && m.IsActive && !m.IsDead)
                        {
                            if (_hitMonsterIds.Contains(m.Id))
                                continue;

                            float distSq = (float)(m.Position - currentPos).SqrMagnitude;
                            if (distSq < closestDistSq)
                            {
                                closestDistSq = distSq;
                                closestEnemy = m;
                            }
                        }
                    }

                    if (closestEnemy == null)
                        break;

                    _hitMonsterIds.Add(closestEnemy.Id);
                    currentPos = closestEnemy.Position;
                    struckPositions.Add(currentPos);

                    ApplyNodeDamageAndShock(context, closestEnemy, effectiveDamage, effectiveSparkRadius);
                }

                if (struckPositions.Count > 0)
                {
                    context.EventBus?.Publish(new GigastormLightningExecutedEvent(context.CasterPosition, struckPositions, effectiveDamage, effectiveSparkRadius));

                    if (!playedSound)
                    {
                        context.EventBus?.Publish(new PlaySoundEvent(SoundEffectType.MagicExplosion, volume: 0.95f));
                        playedSound = true;
                    }
                }
            }
        }

        private void ApplyNodeDamageAndShock(SkillContext context, MonsterEntity target, float damage, float sparkRadius)
        {
            // Inflict primary damage with Shock DoT
            target.ApplyShock(duration: 7.0f, damagePerTick: damage * 0.18f);
            var (hitDmg, isCrit) = context.RollDamage(damage);
            target.TakeDamage(hitDmg, isCrit, DamageType.ShockDot);

            // Plasma splash discharge to adjacent enemies around the node
            if (sparkRadius > 0.5f)
            {
                _splashBuffer.Clear();
                int splashCount = context.TargetGrid.QueryRadiusNonAlloc(target.Position, sparkRadius, _splashBuffer);
                float splashDmg = damage * 0.35f;

                for (int s = 0; s < splashCount; s++)
                {
                    if (_splashBuffer[s] is MonsterEntity splashMonster && splashMonster.IsActive && !splashMonster.IsDead && splashMonster.Id != target.Id)
                    {
                        splashMonster.ApplyShock(duration: 4.0f, damagePerTick: splashDmg * 0.15f);
                        var (sDmg, sCrit) = context.RollDamage(splashDmg);
                        splashMonster.TakeDamage(sDmg, sCrit, DamageType.ShockDot);
                    }
                }
            }
        }
    }
}
