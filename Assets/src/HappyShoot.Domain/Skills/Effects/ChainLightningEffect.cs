using System.Collections.Generic;
using HappyShoot.Domain.Entities;
using HappyShoot.Domain.Events;
using HappyShoot.Domain.Spatial;

namespace HappyShoot.Domain.Skills.Effects
{
    /// <summary>
    /// Wizard exclusive skill: Strikes a primary enemy with lightning, then chains to up to 4 nearby enemies.
    /// Base: 30 damage, 4 chains, 4.0m jump distance.
    /// Leveling: +8 damage, +1 chain per 2 levels.
    /// </summary>
    public class ChainLightningEffect : ISkillEffect, ILevelableEffect
    {
        public float BaseDamage { get; set; }
        public int ChainCount { get; set; }
        public float JumpRadius { get; set; }

        private readonly float _initialDamage;
        private readonly int _initialChains;
        private readonly List<ISpatialEntity> _queryBuffer = new List<ISpatialEntity>(16);
        private readonly HashSet<int> _hitMonsterIds = new HashSet<int>();
        private readonly List<Vector2D> _hitPositions = new List<Vector2D>(8);

        public ChainLightningEffect(float baseDamage = 30f, int chainCount = 4, float jumpRadius = 4.0f)
        {
            BaseDamage = baseDamage;
            ChainCount = chainCount;
            JumpRadius = jumpRadius;
            _initialDamage = baseDamage;
            _initialChains = chainCount;
        }

        public void OnLevelUp(int newLevel)
        {
            // Lv.1: 4 chains, 4.0m -> Lv.5: 8 chains, 6.4m jump distance
            BaseDamage = _initialDamage + 8.5f * (newLevel - 1);
            ChainCount = _initialChains + 1 * (newLevel - 1); // +1 chain per level (4 -> 8)
            JumpRadius = 4.0f + 0.60f * (newLevel - 1); // 4.0m -> 6.4m
        }

        public void ApplyEffect(SkillContext context, IList<Vector2D> targetPositions)
        {
            if (context?.TargetGrid == null || targetPositions == null || targetPositions.Count == 0)
                return;

            float effectiveDamage = BaseDamage * (context.BaseDamage / 10f);
            float effectiveJumpRadius = JumpRadius * context.AreaMultiplier;

            if (context.ActiveRune.IsActive)
            {
                effectiveDamage = context.ActiveRune.ApplyDamage(effectiveDamage);
                effectiveJumpRadius = context.ActiveRune.ApplyArea(effectiveJumpRadius);
            }

            // Bonus chain jumps from AreaMultiplier & Rune Pierce/Split
            int bonusChains = (int)((context.AreaMultiplier - 1.0f) * 6f + 0.5f);
            int runeExtraChains = context.ActiveRune.IsActive ? (context.ActiveRune.ExtraPierceCount + context.ActiveRune.ExtraProjectiles) : 0;
            int totalChains = ChainCount + (bonusChains > 0 ? bonusChains : 0) + runeExtraChains;

            _hitMonsterIds.Clear();
            _hitPositions.Clear();

            Vector2D currentOrigin = targetPositions[0];

            // Find primary monster at initial target
            int initialHits = context.TargetGrid.QueryRadiusNonAlloc(currentOrigin, 1.5f, _queryBuffer);
            MonsterEntity currentMonster = null;
            for (int i = 0; i < initialHits; i++)
            {
                if (_queryBuffer[i] is MonsterEntity m && m.IsActive && !m.IsDead)
                {
                    currentMonster = m;
                    break;
                }
            }

            if (currentMonster != null)
            {
                currentMonster.ApplyShock(duration: 7.0f, damagePerTick: effectiveDamage * 0.10f);
                var (hitDmg, isCrit) = context.RollDamage(effectiveDamage);
                currentMonster.TakeDamage(hitDmg, isCrit, DamageType.Lightning);
                _hitMonsterIds.Add(currentMonster.Id);
                _hitPositions.Add(currentMonster.Position);
                currentOrigin = currentMonster.Position;
            }
            else
            {
                _hitPositions.Add(currentOrigin);
            }

            // Chain to subsequent nearby targets
            for (int chain = 1; chain < totalChains; chain++)
            {
                int nearbyCount = context.TargetGrid.QueryRadiusNonAlloc(currentOrigin, effectiveJumpRadius, _queryBuffer);
                MonsterEntity closestNext = null;
                float closestDistSq = float.MaxValue;

                for (int i = 0; i < nearbyCount; i++)
                {
                    if (_queryBuffer[i] is MonsterEntity nextMonster && nextMonster.IsActive && !nextMonster.IsDead)
                    {
                        if (!_hitMonsterIds.Contains(nextMonster.Id))
                        {
                            float distSq = (nextMonster.Position - currentOrigin).SqrMagnitude;
                            if (distSq < closestDistSq)
                            {
                                closestDistSq = distSq;
                                closestNext = nextMonster;
                            }
                        }
                    }
                }

                if (closestNext != null)
                {
                    closestNext.ApplyShock(duration: 7.0f, damagePerTick: effectiveDamage * 0.10f);
                    var (hitDmg, isCrit) = context.RollDamage(effectiveDamage);
                    closestNext.TakeDamage(hitDmg, isCrit, DamageType.Lightning);
                    _hitMonsterIds.Add(closestNext.Id);
                    _hitPositions.Add(closestNext.Position);
                    currentOrigin = closestNext.Position;
                }
                else
                {
                    break; // No more enemies within chain jump radius
                }
            }

            if (_hitPositions.Count > 0)
            {
                context.EventBus?.Publish(new ChainLightningExecutedEvent(context.CasterPosition, _hitPositions, effectiveDamage));
                context.EventBus?.Publish(new PlaySoundEvent(SoundEffectType.MagicExplosion, volume: 0.85f));
            }
        }
    }
}
