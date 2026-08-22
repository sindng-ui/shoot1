using System;
using System.Collections.Generic;
using HappyShoot.Domain.Entities;
using HappyShoot.Domain.Events;
using HappyShoot.Domain.Spatial;

namespace HappyShoot.Domain.Skills.Effects
{
    /// <summary>
    /// Ranger signature skill (Wind Glaive): Throws spinning wind boomerangs that pierce enemies forward,
    /// and return back to the player dealing double-hit pierce damage.
    /// Balanced compact size and double-hit mechanics.
    /// Strictly modular and under 500 lines.
    /// </summary>
    public class WindGlaiveEffect : ISkillEffect, ILevelableEffect
    {
        public float BaseDamage { get; set; }
        public float MaxDistance { get; set; }
        public float Speed { get; set; }
        public int GlaiveCount { get; set; }

        private readonly float _initialDamage;

        public WindGlaiveEffect(float baseDamage = 28f, float maxDistance = 8.5f, float speed = 15f, int glaiveCount = 1)
        {
            BaseDamage = baseDamage;
            MaxDistance = maxDistance;
            Speed = speed;
            GlaiveCount = glaiveCount;
            _initialDamage = baseDamage;
        }

        public void OnLevelUp(int newLevel)
        {
            // Lv.1: 1 -> Lv.2: 2 -> Lv.3: 2 -> Lv.4: 3 -> Lv.5: 4 glaives
            GlaiveCount = 1 + (newLevel == 2 ? 1 : (newLevel == 3 ? 1 : (newLevel == 4 ? 2 : (newLevel >= 5 ? 3 : 0))));
            BaseDamage = _initialDamage + 7.0f * (newLevel - 1); // 28 -> 56 dmg per hit
            MaxDistance = 8.5f + 0.8f * (newLevel - 1); // 8.5m -> 11.7m
            Speed = 15f + 1.0f * (newLevel - 1);
        }

        public void ApplyEffect(SkillContext context, IList<Vector2D> targetPositions)
        {
            if (context?.TargetGrid == null || targetPositions == null || targetPositions.Count == 0)
                return;

            float effectiveDamage = BaseDamage * (context.BaseDamage / 10f);
            float effectiveDistance = MaxDistance * context.AreaMultiplier;
            float effectiveSpeed = Speed * context.SpeedMultiplier;

            int extraProj = context.CasterEntity != null ? context.CasterEntity.Stats.ExtraProjectiles : 0;
            int totalGlaives = Math.Max(1, GlaiveCount + extraProj);

            for (int t = 0; t < targetPositions.Count; t++)
            {
                Vector2D baseDir = (targetPositions[t] - context.CasterPosition).Normalized;
                if (baseDir.SqrMagnitude < 1e-4f) baseDir = Vector2D.Right;

                // Publish Domain Event for presentation layer visualization & real-time double hit
                context.EventBus?.Publish(new WindGlaiveExecutedEvent(
                    context.CasterPosition,
                    baseDir,
                    effectiveDamage,
                    effectiveDistance,
                    effectiveSpeed,
                    totalGlaives
                ));

                context.EventBus?.Publish(new PlaySoundEvent(SoundEffectType.SlashAttack, volume: 0.75f));
            }
        }
    }
}
