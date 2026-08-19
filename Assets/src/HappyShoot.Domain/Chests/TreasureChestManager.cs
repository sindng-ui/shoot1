using System.Collections.Generic;
using HappyShoot.Domain.Entities;
using HappyShoot.Domain.Events;
using HappyShoot.Domain.Leveling;
using HappyShoot.Domain.Pool;
using HappyShoot.Domain.Spatial;

namespace HappyShoot.Domain.Chests
{
    /// <summary>
    /// Manages spawning, collision detection, and opening of all active treasure chests.
    /// </summary>
    public class TreasureChestManager
    {
        private readonly ObjectPool<TreasureChestEntity> _pool;
        private readonly List<TreasureChestEntity> _activeChests = new List<TreasureChestEntity>(16);
        private readonly EventBus _eventBus;
        private int _idCounter = 3000;

        public IReadOnlyList<TreasureChestEntity> ActiveChests => _activeChests;
        public int ActiveCount => _activeChests.Count;

        public TreasureChestManager(EventBus eventBus = null, int initialCapacity = 8)
        {
            _eventBus = eventBus;
            _pool = new ObjectPool<TreasureChestEntity>(() => new TreasureChestEntity(), initialCapacity: initialCapacity);

            _eventBus?.Subscribe<BossDiedEvent>(OnBossDied);
        }

        public TreasureChestEntity SpawnChest(Vector2D position, int bonusGold = 100)
        {
            var chest = _pool.Spawn();
            chest.Initialize(++_idCounter, position, bonusGold, _eventBus);
            _activeChests.Add(chest);
            return chest;
        }

        public void Update(PlayerEntity player, SkillRewardManager rewardManager, float pickupRadius = 0.8f)
        {
            if (player == null || player.IsDead) return;

            float radiusSqr = pickupRadius * pickupRadius;
            Vector2D playerPos = player.Position;

            for (int i = _activeChests.Count - 1; i >= 0; i--)
            {
                var chest = _activeChests[i];
                if (!chest.IsActive)
                {
                    _activeChests.RemoveAt(i);
                    _pool.Despawn(chest);
                    continue;
                }

                if ((chest.Position - playerPos).SqrMagnitude <= radiusSqr)
                {
                    chest.Open(player, rewardManager, rewardCount: 3);
                    _activeChests.RemoveAt(i);
                    _pool.Despawn(chest);
                }
            }
        }

        private void OnBossDied(BossDiedEvent evt)
        {
            SpawnChest(evt.Position, evt.GoldReward);
        }

        public void Clear()
        {
            for (int i = _activeChests.Count - 1; i >= 0; i--)
            {
                _pool.Despawn(_activeChests[i]);
            }
            _activeChests.Clear();
        }
    }
}
