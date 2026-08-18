using System;
using System.Collections.Generic;
using HappyShoot.Domain.Events;
using HappyShoot.Domain.Pool;
using HappyShoot.Domain.Spatial;

namespace HappyShoot.Domain.Gems
{
    /// <summary>
    /// Manages spawning, magnet collection, and pooling for all experience gems.
    /// </summary>
    public class GemManager
    {
        private readonly ObjectPool<ExpGemEntity> _pool;
        private readonly List<ExpGemEntity> _activeGems = new List<ExpGemEntity>(512);
        private readonly EventBus _eventBus;
        private int _idCounter = 10000;

        public IReadOnlyList<ExpGemEntity> ActiveGems => _activeGems;
        public int ActiveCount => _activeGems.Count;

        public event Action<int> OnExpCollected;

        public GemManager(EventBus eventBus = null, int initialCapacity = 64)
        {
            _eventBus = eventBus;
            _pool = new ObjectPool<ExpGemEntity>(() => new ExpGemEntity(), initialCapacity: initialCapacity);

            _eventBus?.Subscribe<MonsterDiedEvent>(OnMonsterDied);
        }

        private void OnMonsterDied(MonsterDiedEvent evt)
        {
            if (evt.ExpValue > 0)
            {
                SpawnGem(evt.Position, evt.ExpValue);
            }
        }

        /// <summary>
        /// Spawns an experience gem at the given position.
        /// </summary>
        public ExpGemEntity SpawnGem(Vector2D position, int expValue = 1)
        {
            var gem = _pool.Spawn();
            gem.Initialize(++_idCounter, position, expValue);
            _activeGems.Add(gem);
            return gem;
        }

        /// <summary>
        /// Updates magnet attraction for all active gems and collects completed ones.
        /// </summary>
        public void Update(Vector2D playerPosition, float pickupRadius, float deltaTime)
        {
            for (int i = _activeGems.Count - 1; i >= 0; i--)
            {
                var gem = _activeGems[i];
                bool collected = gem.Update(playerPosition, pickupRadius, deltaTime);

                if (collected)
                {
                    OnExpCollected?.Invoke(gem.ExpValue);
                    _activeGems.RemoveAt(i);
                    _pool.Despawn(gem);
                }
                else if (!gem.IsActive)
                {
                    _activeGems.RemoveAt(i);
                    _pool.Despawn(gem);
                }
            }
        }

        /// <summary>
        /// Despawns all active gems.
        /// </summary>
        public void Clear()
        {
            for (int i = _activeGems.Count - 1; i >= 0; i--)
            {
                _pool.Despawn(_activeGems[i]);
            }
            _activeGems.Clear();
        }
    }
}
