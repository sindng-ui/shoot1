using System;
using HappyShoot.Domain.Events;
using HappyShoot.Domain.Skills;

namespace HappyShoot.Domain.Leveling
{
    /// <summary>
    /// Manages player level progression, dynamic experience thresholds, and level-up event triggers.
    /// Strictly modular and under 500 lines.
    /// </summary>
    public class LevelSystem
    {
        public int Level { get; private set; } = 1;
        public int CurrentExp { get; private set; } = 0;
        public int RequiredExp => CalculateRequiredExp(Level);
        public float NormalizedProgress => (float)CurrentExp / RequiredExp;

        public ExpConfig Config { get; set; }

        private readonly EventBus _eventBus;

        public event Action<int> OnLevelUp;

        public LevelSystem(EventBus eventBus = null, int startingLevel = 1, ExpConfig config = null)
        {
            _eventBus = eventBus;
            Level = Math.Max(1, startingLevel);
            CurrentExp = 0;
            Config = config;
        }

        public int CalculateRequiredExp(int level)
        {
            int baseExp = Config != null ? Config.BaseRequiredExp : 4;
            float growth = Config != null ? Config.ExpGrowthFactor : 0.85f;
            float calc = (baseExp + (level * 4.0f) + (level * level * 1.5f)) * growth;
            return Math.Max(1, (int)Math.Round(calc));
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
