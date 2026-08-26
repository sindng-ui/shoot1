using System;
using System.Collections.Generic;
using HappyShoot.Domain.Entities;
using HappyShoot.Domain.Events;
using HappyShoot.Domain.Spatial;

namespace HappyShoot.Domain.Skills.Effects
{
    /// <summary>
    /// Warrior: Upheaval (Ground Stomp) - Rips the ground apart in the target direction,
    /// creating violent sequential fissure tremors that erupt forward with flying rock debris.
    /// Multi-line fissures scale with level (Lv1: 1 line, Lv3: 2 lines, Lv5: 3 lines).
    /// </summary>
    public class GroundStompEffect : ISkillEffect, ILevelableEffect
    {
        public float BaseDamage { get; set; }
        public float Length { get; set; }
        public float StepRadius { get; set; }
        public int LineCount { get; set; }

        // Backward compatibility for StompRadius
        public float StompRadius
        {
            get => StepRadius;
            set => StepRadius = value;
        }

        private readonly List<ISpatialEntity> _hitBuffer = new List<ISpatialEntity>(32);
        private readonly HashSet<int> _hitMonsterIds = new HashSet<int>();
        private readonly List<Vector2D> _stepPositionsBuffer = new List<Vector2D>(24);

        private readonly float _initialDamage;
        private readonly float _initialLength;
        private readonly float _initialRadius;

        public GroundStompEffect(float baseDamage, float stompRadius)
            : this(baseDamage, 5.5f, stompRadius, 1)
        {
        }

        public GroundStompEffect(float baseDamage = 35f, float length = 5.5f, float stepRadius = 0.70f, int lineCount = 1)
        {
            BaseDamage = baseDamage;
            Length = length;
            StepRadius = stepRadius;
            LineCount = Math.Max(1, lineCount);

            _initialDamage = baseDamage;
            _initialLength = length;
            _initialRadius = stepRadius;
        }

        public void OnLevelUp(int newLevel)
        {
            // Level scaling:
            // Lv 1: 1 line, 35 dmg, 5.5m (slim 0.70m radius)
            // Lv 2: 1 line, 45 dmg, 6.2m
            // Lv 3: 2 lines, 56 dmg, 7.0m (2-way spread)
            // Lv 4: 2 lines, 68 dmg, 7.8m
            // Lv 5: 3 lines, 82 dmg, 8.5m (3-way wide upheaval) - Width stays fixed at 0.70m!
            LineCount = newLevel >= 5 ? 3 : (newLevel >= 3 ? 2 : 1);
            BaseDamage = _initialDamage + 11.5f * (newLevel - 1);
            Length = _initialLength + 0.75f * (newLevel - 1);
            StepRadius = _initialRadius; // Fixed width across all levels
        }

        public void ApplyEffect(SkillContext context, IList<Vector2D> targetPositions)
        {
            if (context?.TargetGrid == null)
                return;

            float effectiveLength = Length * context.AreaMultiplier;
            float effectiveRadius = StepRadius * context.AreaMultiplier;
            float effectiveDamage = BaseDamage * (context.BaseDamage / 10f);

            Vector2D casterPos = context.CasterPosition;
            Vector2D mainDir = Vector2D.Right;

            if (targetPositions != null && targetPositions.Count > 0)
            {
                Vector2D diff = targetPositions[0] - casterPos;
                if (diff.SqrMagnitude > 0.0001f)
                {
                    mainDir = diff.Normalized;
                }
            }

            int lineCount = Math.Max(1, LineCount);
            float baseAngle = (float)Math.Atan2(mainDir.Y, mainDir.X);

            _stepPositionsBuffer.Clear();
            _hitMonsterIds.Clear();

            const int stepsPerLine = 5;

            // Calculate fissure lines spread angles
            float spreadAngleStep = 22f * (float)(Math.PI / 180.0); // 22 degrees spread
            float startAngle = lineCount > 1
                ? baseAngle - (spreadAngleStep * (lineCount - 1) * 0.5f)
                : baseAngle;

            for (int l = 0; l < lineCount; l++)
            {
                float lineAngle = startAngle + l * spreadAngleStep;
                Vector2D lineDir = new Vector2D((float)Math.Cos(lineAngle), (float)Math.Sin(lineAngle));
                float stepDist = effectiveLength / stepsPerLine;

                for (int s = 1; s <= stepsPerLine; s++)
                {
                    Vector2D stepPos = casterPos + lineDir * (stepDist * s);
                    _stepPositionsBuffer.Add(stepPos);

                    int hitCount = context.TargetGrid.QueryRadiusNonAlloc(stepPos, effectiveRadius, _hitBuffer);
                    for (int j = 0; j < hitCount; j++)
                    {
                        if (_hitBuffer[j] is MonsterEntity monster && monster.IsActive && !monster.IsDead)
                        {
                            if (_hitMonsterIds.Add(monster.Id))
                            {
                                var (hitDmg, isCrit) = context.RollDamage(effectiveDamage);
                                monster.TakeDamage(hitDmg, isCrit);
                            }
                        }
                    }
                }
            }

            // Publish visual event containing all fissure step positions
            var stepsArray = _stepPositionsBuffer.ToArray();
            context.EventBus?.Publish(new GroundStompExecutedEvent(casterPos, mainDir, effectiveLength, effectiveRadius, lineCount, stepsArray));
            context.EventBus?.Publish(new PlaySoundEvent(SoundEffectType.MonsterHit, volume: 0.95f));
        }
    }
}
