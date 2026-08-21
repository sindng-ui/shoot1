using System;
using System.Collections.Generic;
using HappyShoot.Domain.Spatial;

namespace HappyShoot.Domain.Skills
{
    /// <summary>
    /// Composable skill that combines a Trigger, a Targeter, and an Effect.
    /// Allows infinite skill variations without modifying class hierarchies (Open-Closed Principle).
    /// </summary>
    public class CompositeSkill : ISkill
    {
        public string Id { get; }
        public string Name { get; }
        public int Level { get; private set; } = 1;
        public int MaxLevel { get; }
        public bool IsMaxLevel => Level >= MaxLevel;

        public ISkillTrigger Trigger { get; set; }
        public ISkillTargeter Targeter { get; set; }
        public ISkillEffect Effect { get; set; }
        public float Range { get; set; }

        private readonly List<Vector2D> _cachedTargetPositions = new List<Vector2D>(8);

        public CompositeSkill(
            string id,
            string name,
            ISkillTrigger trigger,
            ISkillTargeter targeter,
            ISkillEffect effect,
            float range = 10f,
            int maxLevel = 5)
        {
            Id = id ?? throw new ArgumentNullException(nameof(id));
            Name = name ?? throw new ArgumentNullException(nameof(name));
            Trigger = trigger ?? throw new ArgumentNullException(nameof(trigger));
            Targeter = targeter ?? throw new ArgumentNullException(nameof(targeter));
            Effect = effect ?? throw new ArgumentNullException(nameof(effect));
            Range = range;
            MaxLevel = maxLevel;
            Level = 1;
        }

        public void LevelUp()
        {
            if (Level < MaxLevel)
            {
                Level++;
                if (Effect is ILevelableEffect levelable)
                {
                    levelable.OnLevelUp(Level);
                }
                Range += 0.85f;
            }
        }

        public void Update(float deltaTime, SkillContext context)
        {
            if (Trigger.CanTrigger(deltaTime))
            {
                if (Targeter.TryFindTargets(context, Range, _cachedTargetPositions))
                {
                    Effect.ApplyEffect(context, _cachedTargetPositions);
                    Trigger.OnTriggered();
                }
            }
        }
    }
}
