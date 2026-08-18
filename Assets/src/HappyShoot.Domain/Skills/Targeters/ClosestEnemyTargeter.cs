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
