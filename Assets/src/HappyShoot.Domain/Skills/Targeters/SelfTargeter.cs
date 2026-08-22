using System.Collections.Generic;
using HappyShoot.Domain.Spatial;

namespace HappyShoot.Domain.Skills.Targeters
{
    /// <summary>
    /// Returns the caster's own position as the target.
    /// Used for self-centered auras, stomps, and orbiting projectiles.
    /// Strictly modular and under 500 lines.
    /// </summary>
    public class SelfTargeter : ISkillTargeter
    {
        public bool TryFindTargets(SkillContext context, float range, IList<Vector2D> outTargets)
        {
            if (context == null || outTargets == null) return false;

            outTargets.Clear();
            outTargets.Add(context.CasterPosition);
            return true;
        }
    }
}
