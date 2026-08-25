using System;
using System.Collections.Generic;
using HappyShoot.Domain.Entities;
using HappyShoot.Domain.Events;
using HappyShoot.Domain.Spatial;

namespace HappyShoot.Domain.Skills.Effects
{
    /// <summary>
    /// Wizard exclusive primary skill: Fires explosive fireballs that blast enemies and apply burning DoT.
    /// Hits exact targeted / mouse location within range.
    /// Leveling grants extra fireballs (Lv.1: 1 -> Lv.3: 2 -> Lv.5: 3) and dramatically expands blast radius (1.6m -> 3.2m).
    /// Strictly modular and under 500 lines.
    /// </summary>
    public class FireballEffect : ISkillEffect, ILevelableEffect
    {
        public float BaseDamage { get; set; }
        public float Radius { get; set; }
        public float Speed { get; set; }
        public int FireballCount { get; set; }

        private readonly float _initialDamage;
        private readonly float _initialRadius;
        private readonly float _initialSpeed;
        private readonly List<ISpatialEntity> _hitBuffer = new List<ISpatialEntity>(64);
        private readonly List<ISpatialEntity> _targetQueryBuffer = new List<ISpatialEntity>(16);

        public FireballEffect(float baseDamage = 35f, float radius = 1.6f, float speed = 14f, int fireballCount = 1)
        {
            BaseDamage = baseDamage;
            Radius = radius;
            Speed = speed;
            FireballCount = fireballCount;

            _initialDamage = baseDamage;
            _initialRadius = radius;
            _initialSpeed = speed;
        }

        public void OnLevelUp(int newLevel)
        {
            // Lv.1: 1 ball, 1.6m -> Lv.3: 2 balls, 2.4m -> Lv.5: 3 balls, 3.2m
            FireballCount = 1 + (newLevel >= 5 ? 2 : (newLevel >= 3 ? 1 : 0));
            Radius = _initialRadius + 0.40f * (newLevel - 1); // 1.6m -> 3.2m giant fireball
            BaseDamage = _initialDamage + 8f * (newLevel - 1); // 35 -> 67 dmg
            Speed = _initialSpeed + 1.2f * (newLevel - 1);
        }

        public void ApplyEffect(SkillContext context, IList<Vector2D> targetPositions)
        {
            if (context?.TargetGrid == null || targetPositions == null || targetPositions.Count == 0)
                return;

            float effectiveDamage = BaseDamage * (context.BaseDamage / 10f);
            float effectiveRadius = Radius * context.AreaMultiplier;

            int extraProj = context.CasterEntity != null ? context.CasterEntity.Stats.ExtraProjectiles : 0;
            int totalFireballs = Math.Max(1, FireballCount + extraProj);

            Vector2D primaryTarget = targetPositions[0];
            Vector2D primaryOffset = primaryTarget - context.CasterPosition;
            float primaryDist = (float)primaryOffset.Magnitude;
            Vector2D primaryDir = primaryDist > 1e-4f ? primaryOffset.Normalized : Vector2D.Right;

            // Find secondary targets for multi-fireball spread
            float searchRadius = Math.Max(primaryDist + 2.0f, 8.0f);
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
            float fanDist = Math.Max(1.2f, primaryDist);
            while (targetsToBlast.Count < totalFireballs)
            {
                int idx = targetsToBlast.Count;
                float angleOffset = (-18f + (36f / totalFireballs) * idx) * (float)Math.PI / 180f;
                float baseAngle = (float)Math.Atan2(primaryDir.Y, primaryDir.X) + angleOffset;
                Vector2D fanTarget = context.CasterPosition + new Vector2D((float)Math.Cos(baseAngle), (float)Math.Sin(baseAngle)) * fanDist;
                targetsToBlast.Add(fanTarget);
            }

            bool isFacingLeft = primaryDir.X < -0.05f;
            Vector2D staffTipPos = context.CasterPosition + new Vector2D(isFacingLeft ? -0.38f : 0.38f, 0.22f);

            // Launch flying fireball comets from staff tip
            for (int b = 0; b < targetsToBlast.Count; b++)
            {
                Vector2D targetPos = targetsToBlast[b];

                // Publish Domain Event for Wizard Fireball flight (explosion & damage trigger upon arrival)
                context.EventBus?.Publish(new FireballLaunchedEvent(staffTipPos, targetPos, effectiveRadius, effectiveDamage, Speed));
                context.EventBus?.Publish(new PlaySoundEvent(SoundEffectType.Fireball, volume: 0.85f));
            }
        }
    }
}
