using System;
using System.Collections.Generic;
using HappyShoot.Domain.Events;
using HappyShoot.Domain.Pool;
using HappyShoot.Domain.Spatial;

namespace HappyShoot.Domain.UI
{
    /// <summary>
    /// Manages zero-allocation pooling, spawning, and lifecycle of floating damage numbers.
    /// </summary>
    public class DamageTextManager
    {
        private readonly ObjectPool<DamageTextEntity> _pool;
        private readonly List<DamageTextEntity> _activeTexts = new List<DamageTextEntity>(128);
        private readonly EventBus _eventBus;
        private int _idCounter = 20000;

        public IReadOnlyList<DamageTextEntity> ActiveTexts => _activeTexts;
        public int ActiveCount => _activeTexts.Count;
        public event Action<DamageTextEntity> OnTextSpawned;

        public DamageTextManager(EventBus eventBus = null, int initialCapacity = 32)
        {
            _eventBus = eventBus;
            _pool = new ObjectPool<DamageTextEntity>(() => new DamageTextEntity(), initialCapacity: initialCapacity);

            _eventBus?.Subscribe<MonsterDamagedEvent>(OnMonsterDamaged);
        }

        private void OnMonsterDamaged(MonsterDamagedEvent evt)
        {
            // Slight random offset to prevent texts stacking exactly on top of each other
            float offsetX = ((evt.MonsterId % 5) - 2) * 0.15f;
            float offsetY = ((evt.MonsterId % 3)) * 0.15f;
            Vector2D spawnPos = evt.Position + new Vector2D(offsetX, offsetY);

            SpawnText(spawnPos, evt.Damage, isCritical: evt.IsCritical);
        }

        public DamageTextEntity SpawnText(Vector2D position, float damage, bool isCritical = false)
        {
            var textEntity = _pool.Spawn();
            textEntity.Initialize(++_idCounter, position, damage, isCritical);
            _activeTexts.Add(textEntity);
            OnTextSpawned?.Invoke(textEntity);
            return textEntity;
        }

        public void Update(float deltaTime)
        {
            for (int i = _activeTexts.Count - 1; i >= 0; i--)
            {
                var text = _activeTexts[i];
                text.Update(deltaTime);

                if (!text.IsActive)
                {
                    _activeTexts.RemoveAt(i);
                    _pool.Despawn(text);
                }
            }
        }

        public void Clear()
        {
            for (int i = _activeTexts.Count - 1; i >= 0; i--)
            {
                _pool.Despawn(_activeTexts[i]);
            }
            _activeTexts.Clear();
        }
    }
}
