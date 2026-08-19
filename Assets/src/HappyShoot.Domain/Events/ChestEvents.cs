using System.Collections.Generic;
using HappyShoot.Domain.Leveling;
using HappyShoot.Domain.Spatial;

namespace HappyShoot.Domain.Events
{
    public readonly struct TreasureChestSpawnedEvent : IDomainEvent
    {
        public readonly int ChestId;
        public readonly Vector2D Position;

        public TreasureChestSpawnedEvent(int chestId, Vector2D position)
        {
            ChestId = chestId;
            Position = position;
        }
    }

    public readonly struct TreasureChestOpenedEvent : IDomainEvent
    {
        public readonly int ChestId;
        public readonly IReadOnlyList<SkillRewardOption> Rewards;
        public readonly int BonusGold;

        public TreasureChestOpenedEvent(int chestId, IReadOnlyList<SkillRewardOption> rewards, int bonusGold)
        {
            ChestId = chestId;
            Rewards = rewards;
            BonusGold = bonusGold;
        }
    }
}
