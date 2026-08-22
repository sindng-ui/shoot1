using System;
using System.Collections.Generic;
using HappyShoot.Domain.Entities;
using HappyShoot.Domain.Spatial;

namespace HappyShoot.Domain.Skills.Targeters
{
    /// <summary>
    /// Smart hybrid targeter:
    /// - When Manual Aiming (Mouse ON) is active, ONLY Primary Starting Skills track the mouse cursor.
    /// - Non-primary secondary skills (Glaive, Arrow Rain, Frost Nova, Lightning, etc.) ALWAYS auto-target nearby enemies.
    /// - When Auto-Aiming is active, all skills auto-target closest enemies.
    /// Strictly modular and under 500 lines.
    /// </summary>
    public class ClosestEnemyTargeter : ISkillTargeter
    {
        public bool TryFindTargets(SkillContext context, float range, IList<Vector2D> targetPositionsBuffer)
        {
            if (context == null) throw new ArgumentNullException(nameof(context));
            if (targetPositionsBuffer == null) throw new ArgumentNullException(nameof(targetPositionsBuffer));

            targetPositionsBuffer.Clear();

            bool isPrimary = IsPrimaryStartingSkill(context);

            // 1. Manual Mouse Aiming: ONLY Primary Starting Skill follows the mouse cursor
            if (!Settings.GameSettings.AutoTargeting && isPrimary)
            {
                targetPositionsBuffer.Add(GetClampedAimPosition(context, range));
                return true;
            }

            // 2. Secondary Skills OR Auto-Aiming: Always auto-target closest enemy in range
            if (context.TargetGrid != null && context.TargetGrid.TryGetClosest(context.CasterPosition, range, out var closest))
            {
                targetPositionsBuffer.Add(closest.Position);
                return true;
            }

            // 3. Fallback when no enemy is within range
            if (isPrimary)
            {
                targetPositionsBuffer.Add(GetClampedAimPosition(context, range));
            }
            else
            {
                // Secondary skills fire in the forward movement/facing direction without mouse distraction
                Vector2D forwardDir = context.AimDirection.SqrMagnitude > 1e-4f ? context.AimDirection.Normalized : Vector2D.Right;
                targetPositionsBuffer.Add(context.CasterPosition + forwardDir * Math.Max(0.5f, range));
            }

            return true;
        }

        private bool IsPrimaryStartingSkill(SkillContext context)
        {
            if (context?.CasterEntity == null || string.IsNullOrEmpty(context.SkillId)) return false;
            var heroClass = context.CasterEntity.ClassType;

            switch (heroClass)
            {
                case CharacterClassType.Warrior:
                    return context.SkillId == "slash" || context.SkillId == "blood_eater";
                case CharacterClassType.Ranger:
                    return context.SkillId == "bow" || context.SkillId == "storm_bow";
                case CharacterClassType.Wizard:
                    return context.SkillId == "fireball" || context.SkillId == "meteor_strike";
                default:
                    return false;
            }
        }

        private Vector2D GetClampedAimPosition(SkillContext context, float range)
        {
            Vector2D toAim = context.AimTargetPosition - context.CasterPosition;
            float dist = (float)toAim.Magnitude;

            if (dist > 0.1f)
            {
                float clampedDist = Math.Min(dist, Math.Max(0.5f, range));
                return context.CasterPosition + toAim.Normalized * clampedDist;
            }

            Vector2D aimDir = context.AimDirection.SqrMagnitude > 1e-4f ? context.AimDirection.Normalized : Vector2D.Right;
            return context.CasterPosition + aimDir * Math.Max(0.5f, range);
        }
    }
}
