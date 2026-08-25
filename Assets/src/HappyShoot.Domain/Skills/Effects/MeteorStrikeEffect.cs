using System;
using System.Collections.Generic;
using HappyShoot.Domain.Entities;
using HappyShoot.Domain.Events;
using HappyShoot.Domain.Spatial;

namespace HappyShoot.Domain.Skills.Effects
{
    /// <summary>
    /// Evolved Wizard Ultimate Skill: Inferno Fireball (evolved from Fireball Lv.5 + Mana Rune).
    /// Fires 3 massive penetrating hellfire comets that each pierce once (exploding on 1st hit AND on 2nd hit/arrival).
    /// Strictly modular and under 500 lines.
    /// </summary>
    public class MeteorStrikeEffect : ISkillEffect
    {
        public float BaseDamage { get; set; }
        public float ExplosionRadius { get; set; }
        public float Speed { get; set; }
        public int FireballCount { get; set; }
        public int PierceCount { get; set; }

        private readonly List<ISpatialEntity> _targetQueryBuffer = new List<ISpatialEntity>(16);

        public MeteorStrikeEffect(float baseDamage = 85f, float explosionRadius = 2.8f, float speed = 15f, int fireballCount = 3, int pierceCount = 1)
        {
            BaseDamage = baseDamage;
            ExplosionRadius = explosionRadius;
            Speed = speed;
            FireballCount = fireballCount;
            PierceCount = pierceCount;
        }

        public void ApplyEffect(SkillContext context, IList<Vector2D> targetPositions)
        {
            if (context?.TargetGrid == null || targetPositions == null || targetPositions.Count == 0)
                return;

            float effectiveRadius = ExplosionRadius * context.AreaMultiplier;
            float effectiveDamage = BaseDamage * (context.BaseDamage / 10f);

            int extraProj = context.CasterEntity != null ? context.CasterEntity.Stats.ExtraProjectiles : 0;
            int totalFireballs = Math.Max(1, FireballCount + extraProj);

            Vector2D primaryTarget = targetPositions[0];
            Vector2D primaryOffset = primaryTarget - context.CasterPosition;
            float primaryDist = (float)primaryOffset.Magnitude;
            Vector2D primaryDir = primaryDist > 1e-4f ? primaryOffset.Normalized : Vector2D.Right;

            // Multi-target search for hellfire spread
            float searchRadius = Math.Max(primaryDist + 3.0f, 9.0f);
            int foundEnemies = context.TargetGrid.QueryRadiusNonAlloc(context.CasterPosition, searchRadius, _targetQueryBuffer);
            List<Vector2D> targetsToBlast = new List<Vector2D>(totalFireballs);
            targetsToBlast.Add(primaryTarget);

            for (int i = 0; i < foundEnemies && targetsToBlast.Count < totalFireballs; i++)
            {
                if (_targetQueryBuffer[i] is MonsterEntity m && m.IsActive && !m.IsDead)
                {
                    if ((m.Position - primaryTarget).SqrMagnitude > 1.0f)
                    {
                        targetsToBlast.Add(m.Position);
                    }
                }
            }

            // Fallback fan spread targets relative to primary distance
            float fanDist = Math.Max(2.0f, primaryDist);
            while (targetsToBlast.Count < totalFireballs)
            {
                int idx = targetsToBlast.Count;
                float angleOffset = (-20f + (40f / totalFireballs) * idx) * (float)Math.PI / 180f;
                float baseAngle = (float)Math.Atan2(primaryDir.Y, primaryDir.X) + angleOffset;
                Vector2D fanTarget = context.CasterPosition + new Vector2D((float)Math.Cos(baseAngle), (float)Math.Sin(baseAngle)) * fanDist;
                targetsToBlast.Add(fanTarget);
            }

            bool isFacingLeft = primaryDir.X < -0.05f;
            Vector2D staffTipPos = context.CasterPosition + new Vector2D(isFacingLeft ? -0.38f : 0.38f, 0.22f);

            // Launch penetrating inferno fireball comets from staff tip
            for (int b = 0; b < targetsToBlast.Count; b++)
            {
                Vector2D targetPos = targetsToBlast[b];
                context.EventBus?.Publish(new MeteorStrikeLaunchedEvent(staffTipPos, targetPos, effectiveRadius, effectiveDamage, Speed, PierceCount));
                context.EventBus?.Publish(new MeteorStrikeExecutedEvent(targetPos, effectiveRadius, effectiveDamage));
                context.EventBus?.Publish(new PlaySoundEvent(SoundEffectType.Fireball, volume: 1.0f));
            }
        }
    }
}
