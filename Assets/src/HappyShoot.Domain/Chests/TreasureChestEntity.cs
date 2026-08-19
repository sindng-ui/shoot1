using System;
using System.Collections.Generic;
using HappyShoot.Domain.Entities;
using HappyShoot.Domain.Events;
using HappyShoot.Domain.Leveling;
using HappyShoot.Domain.Pool;
using HappyShoot.Domain.Spatial;

namespace HappyShoot.Domain.Chests
{
    /// <summary>
    /// Pure C# Treasure Chest entity dropped by bosses or elites.
    /// </summary>
    public class TreasureChestEntity : ISpatialEntity, IPoolable
    {
        public int Id { get; private set; }
        public Vector2D Position { get; private set; }
        public float Radius { get; set; } = 0.6f;
        public bool IsActive { get; private set; }
        public bool IsOpened { get; private set; }
        public int BonusGold { get; private set; } = 50;

        private EventBus _eventBus;

        public TreasureChestEntity()
        {
            IsActive = false;
        }

        public void Initialize(int id, Vector2D position, int bonusGold = 50, EventBus eventBus = null)
        {
            Id = id;
            Position = position;
            BonusGold = bonusGold;
            _eventBus = eventBus;
            IsOpened = false;
            IsActive = true;

            _eventBus?.Publish(new TreasureChestSpawnedEvent(Id, Position));
        }

        public void OnSpawn()
        {
            IsActive = true;
        }

        public void OnDespawn()
        {
            IsActive = false;
            _eventBus = null;
        }

        /// <summary>
        /// Opens the chest, rolling 1-3 instant skill rewards and granting bonus gold.
        /// </summary>
        public IReadOnlyList<SkillRewardOption> Open(PlayerEntity player, SkillRewardManager rewardManager, int rewardCount = 3)
        {
            if (!IsActive || IsOpened) return Array.Empty<SkillRewardOption>();

            IsOpened = true;
            IsActive = false;

            var rewards = rewardManager != null && player != null
                ? rewardManager.RollRewards(player, count: Math.Min(3, rewardCount))
                : new List<SkillRewardOption>();

            // Automatically apply rewards to player
            if (rewardManager != null && player != null)
            {
                for (int i = 0; i < rewards.Count; i++)
                {
                    rewardManager.ApplyReward(player, rewards[i]);
                }
            }

            _eventBus?.Publish(new TreasureChestOpenedEvent(Id, rewards, BonusGold));
            return rewards;
        }
    }
}
