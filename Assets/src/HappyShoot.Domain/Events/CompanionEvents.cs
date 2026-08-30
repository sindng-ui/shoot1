using HappyShoot.Domain.Leveling;

namespace HappyShoot.Domain.Events
{
    /// <summary>
    /// Event fired when the Wizard acquires a new skill, levels up an active skill,
    /// or upgrades a passive skill, prompting companions to synchronize their growth.
    /// Strictly modular and under 500 lines.
    /// </summary>
    public readonly struct CompanionRewardSyncEvent : IDomainEvent
    {
        public readonly RewardCategory Category;
        public readonly string RewardId;

        public CompanionRewardSyncEvent(RewardCategory category, string rewardId)
        {
            Category = category;
            RewardId = rewardId;
        }
    }
}
