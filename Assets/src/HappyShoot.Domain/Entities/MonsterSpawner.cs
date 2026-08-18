using System;
using System.Collections.Generic;
using HappyShoot.Domain.Events;
using HappyShoot.Domain.Pool;
using HappyShoot.Domain.Spatial;

namespace HappyShoot.Domain.Entities
{
    /// <summary>
    /// Manages zero-allocation monster spawning, pooling, and spatial grid updates.
    /// </summary>
    public class MonsterSpawner
    {
        private readonly ObjectPool<MonsterEntity> _monsterPool;
        private readonly SpatialGrid2D<MonsterEntity> _monsterGrid;
        private readonly List<MonsterEntity> _activeMonsters = new List<MonsterEntity>(512);
        private readonly EventBus _eventBus;
        private int _idCounter = 1000;

        public SpatialGrid2D<MonsterEntity> MonsterGrid => _monsterGrid;
        public IReadOnlyList<MonsterEntity> ActiveMonsters => _activeMonsters;
        public int ActiveCount => _activeMonsters.Count;

        public MonsterSpawner(SpatialGrid2D<MonsterEntity> grid, EventBus eventBus = null, int initialPoolSize = 64)
        {
            _monsterGrid = grid ?? throw new ArgumentNullException(nameof(grid));
            _eventBus = eventBus;
            _monsterPool = new ObjectPool<MonsterEntity>(() => new MonsterEntity(), initialCapacity: initialPoolSize);

            _eventBus?.Subscribe<MonsterDiedEvent>(OnMonsterDied);
        }

        /// <summary>
        /// Spawns a monster at the specified position.
        /// </summary>
        public MonsterEntity SpawnMonster(
            string typeName,
            float maxHealth,
            float moveSpeed,
            float contactDamage,
            int expValue,
            int goldValue,
            Vector2D position)
        {
            MonsterEntity monster = _monsterPool.Spawn();
            monster.Initialize(
                id: ++_idCounter,
                typeName: typeName,
                maxHealth: maxHealth,
                moveSpeed: moveSpeed,
                contactDamage: contactDamage,
                expValue: expValue,
                goldValue: goldValue,
                startPosition: position,
                eventBus: _eventBus
            );

            _activeMonsters.Add(monster);
            _monsterGrid.Register(monster);
            return monster;
        }

        /// <summary>
        /// Spawns a monster along the circumference of a circle around the player.
        /// </summary>
        public MonsterEntity SpawnAroundPlayer(
            Vector2D playerPosition,
            float spawnRadius,
            float angleRadians,
            string typeName = "Slime",
            float maxHealth = 20f,
            float moveSpeed = 2.5f,
            float contactDamage = 5f,
            int expValue = 1,
            int goldValue = 1)
        {
            float posX = playerPosition.X + (float)Math.Cos(angleRadians) * spawnRadius;
            float posY = playerPosition.Y + (float)Math.Sin(angleRadians) * spawnRadius;
            return SpawnMonster(typeName, maxHealth, moveSpeed, contactDamage, expValue, goldValue, new Vector2D(posX, posY));
        }

        /// <summary>
        /// Updates all active monsters AI and spatial grid coordinates.
        /// </summary>
        public void Update(Vector2D playerPosition, float deltaTime)
        {
            for (int i = _activeMonsters.Count - 1; i >= 0; i--)
            {
                var monster = _activeMonsters[i];
                if (!monster.IsActive || monster.IsDead)
                {
                    DespawnMonster(monster, i);
                    continue;
                }

                monster.UpdateAI(playerPosition, deltaTime);
                _monsterGrid.UpdatePosition(monster);
            }
        }

        private void OnMonsterDied(MonsterDiedEvent evt)
        {
            // Handled during update or explicit check
        }

        private void DespawnMonster(MonsterEntity monster, int index)
        {
            _monsterGrid.Unregister(monster);
            _activeMonsters.RemoveAt(index);
            _monsterPool.Despawn(monster);
        }

        /// <summary>
        /// Despawns all active monsters and returns them to the pool.
        /// </summary>
        public void DespawnAll()
        {
            for (int i = _activeMonsters.Count - 1; i >= 0; i--)
            {
                var monster = _activeMonsters[i];
                _monsterGrid.Unregister(monster);
                _monsterPool.Despawn(monster);
            }
            _activeMonsters.Clear();
        }
    }
}
