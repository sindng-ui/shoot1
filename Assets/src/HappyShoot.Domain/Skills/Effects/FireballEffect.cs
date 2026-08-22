using System.Collections.Generic;
using HappyShoot.Domain.Entities;
using HappyShoot.Domain.Events;
using HappyShoot.Domain.Spatial;

namespace HappyShoot.Domain.Skills.Effects
{
    /// <summary>
    /// Wizard exclusive primary skill: Fires an explosive fireball that blasts enemies in a circular area.
    /// Base: 35 damage, 1.6m blast radius, 14m/s flight speed.
    /// Leveling: +10 damage, +0.2m blast radius per level.
    /// </summary>
    public class FireballEffect : ISkillEffect, ILevelableEffect
    {
        public float BaseDamage { get; set; }
        public float Radius { get; set; }
        public float Speed { get; set; }

        private readonly float _initialDamage;
        private readonly float _initialRadius;
        private readonly float _initialSpeed;
        private readonly List<ISpatialEntity> _hitBuffer = new List<ISpatialEntity>(32);

        public FireballEffect(float baseDamage = 35f, float radius = 1.6f, float speed = 14f)
        {
            BaseDamage = baseDamage;
            Radius = radius;
            Speed = speed;
            _initialDamage = baseDamage;
            _initialRadius = radius;
            _initialSpeed = speed;
        }

        public void OnLevelUp(int newLevel)
        {
            BaseDamage = _initialDamage + 10f * (newLevel - 1);
            Radius = _initialRadius + 0.2f * (newLevel - 1);
            Speed = _initialSpeed + 1.0f * (newLevel - 1);
        }

        public void ApplyEffect(SkillContext context, IList<Vector2D> targetPositions)
        {
            if (context?.TargetGrid == null || targetPositions == null || targetPositions.Count == 0)
                return;

            float effectiveDamage = BaseDamage * (context.BaseDamage / 10f);
            float effectiveRadius = Radius * context.AreaMultiplier;

            for (int t = 0; t < targetPositions.Count; t++)
            {
                Vector2D targetPos = targetPositions[t];
                Vector2D direction = (targetPos - context.CasterPosition).Normalized;
                if (direction.SqrMagnitude < 1e-4f) direction = Vector2D.Right;

                // Launch visual fireball projectile if projectile manager is available
                context.ProjectileManager?.LaunchProjectile(
                    origin: context.CasterPosition,
                    direction: direction,
                    speed: Speed * context.SpeedMultiplier,
                    damage: effectiveDamage,
                    pierceCount: 1,
                    lifetime: 1.5f
                );

                // Area explosion at target position
                int hitCount = context.TargetGrid.QueryRadiusNonAlloc(targetPos, effectiveRadius, _hitBuffer);
                for (int i = 0; i < hitCount; i++)
                {
                    if (_hitBuffer[i] is MonsterEntity monster && monster.IsActive && !monster.IsDead)
                    {
                        monster.ApplyBurn(duration: 7.0f, damagePerTick: effectiveDamage * 0.12f);
                        monster.TakeDamage(effectiveDamage);
                    }
                }

                // Publish Domain Events
                context.EventBus?.Publish(new FireballExplodedEvent(targetPos, effectiveRadius, effectiveDamage));
                context.EventBus?.Publish(new PlaySoundEvent(SoundEffectType.MagicExplosion, volume: 0.9f));
            }
        }
    }
}
