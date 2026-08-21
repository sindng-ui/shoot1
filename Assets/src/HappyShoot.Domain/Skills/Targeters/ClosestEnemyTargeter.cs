using System;
using System.Collections.Generic;
using HappyShoot.Domain.Spatial;

namespace HappyShoot.Domain.Skills.Targeters
{
    /// <summary>
    /// Targets the single closest enemy within the given range using the spatial grid.
    /// </summary>
    public class ClosestEnemyTargeter : ISkillTargeter
    {
        public bool TryFindTargets(SkillContext context, float range, IList<Vector2D> targetPositionsBuffer)
        {
            if (context == null) throw new ArgumentNullException(nameof(context));
            if (targetPositionsBuffer == null) throw new ArgumentNullException(nameof(targetPositionsBuffer));

            targetPositionsBuffer.Clear();

            // If manual aiming is enabled in GameSettings, target in the AimDirection
            if (!Settings.GameSettings.AutoTargeting)
            {
                Vector2D aimDir = context.AimDirection.SqrMagnitude > 1e-4f ? context.AimDirection.Normalized : Vector2D.Right;
                targetPositionsBuffer.Add(context.CasterPosition + aimDir * Math.Max(1.0f, range));
                return true;
            }

            if (context.TargetGrid == null)
            {
                return false;
            }

            if (context.TargetGrid.TryGetClosest(context.CasterPosition, range, out var closest))
            {
                targetPositionsBuffer.Add(closest.Position);
                return true;
            }

            return false;
        }
    }
}
