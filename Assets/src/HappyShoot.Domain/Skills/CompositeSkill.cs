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
        public Forge.RuneModifiers Rune { get; set; } = Forge.RuneModifiers.None;

        private int _castCount;
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
            if (context != null)
            {
                context.SkillId = Id;
                context.ActiveRune = Rune;
            }

            // 1. Update continuous / tick-based effects (e.g. Arrow Rain ongoing barrage)
            if (Effect is ITickableEffect tickable)
            {
                tickable.Update(deltaTime, context);
            }

            // 2. Trigger new skill activations when cooldown/trigger is ready
            float effectiveDelta = deltaTime;
            if (Rune.IsActive && Rune.CooldownMultiplier > 0f)
            {
                effectiveDelta = deltaTime / Rune.CooldownMultiplier;
            }

            if (Trigger.CanTrigger(effectiveDelta))
            {
                if (Targeter.TryFindTargets(context, Range, _cachedTargetPositions))
                {
                    Effect.ApplyEffect(context, _cachedTargetPositions);
                    Trigger.OnTriggered();

                    _castCount++;
                    // Tempo Rune: free cast every Nth use
                    if (Rune.IsActive && Rune.FreecastEveryN > 0 && (_castCount % Rune.FreecastEveryN == 0))
                    {
                        if (Targeter.TryFindTargets(context, Range, _cachedTargetPositions))
                        {
                            Effect.ApplyEffect(context, _cachedTargetPositions);
                        }
                    }
                }
            }
        }
    }
}
