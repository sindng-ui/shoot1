using System;
using HappyShoot.Domain.Events;

namespace HappyShoot.Domain.Leveling
{
    /// <summary>
    /// Manages player level progression, experience thresholds, and level-up event triggers.
    /// </summary>
    public class LevelSystem
    {
        public int Level { get; private set; } = 1;
        public int CurrentExp { get; private set; } = 0;
        public int RequiredExp => CalculateRequiredExp(Level);
        public float NormalizedProgress => (float)CurrentExp / RequiredExp;

        private readonly EventBus _eventBus;

        public event Action<int> OnLevelUp;

        public LevelSystem(EventBus eventBus = null, int startingLevel = 1)
        {
            _eventBus = eventBus;
            Level = Math.Max(1, startingLevel);
            CurrentExp = 0;
        }

        public static int CalculateRequiredExp(int level)
        {
            // Survivors curve: 5 + (level * 5) + (level^2 * 2)
            return 5 + (level * 5) + (level * level * 2);
        }

        /// <summary>
        /// Adds experience, handling multiple sequential level-ups and carrying over overflow.
        /// </summary>
        public void AddExp(int amount)
        {
            if (amount <= 0) return;

            CurrentExp += amount;
            _eventBus?.Publish(new ExpGainedEvent(amount, CurrentExp, RequiredExp));

            while (CurrentExp >= RequiredExp)
            {
                CurrentExp -= RequiredExp;
                Level++;

                OnLevelUp?.Invoke(Level);
                _eventBus?.Publish(new PlayerLevelUpEvent(Level));
            }
        }
    }
}
