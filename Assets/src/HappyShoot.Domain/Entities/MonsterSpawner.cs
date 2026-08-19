using System;
using System.Collections.Generic;
using HappyShoot.Domain.Events;
using HappyShoot.Domain.Pool;
using HappyShoot.Domain.Spatial;

namespace HappyShoot.Domain.Entities
{
    /// <summary>
    /// Manages zero-allocation monster spawning, pooling, archetypes, and spatial grid updates.
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
        /// Spawns a monster at the specified position using raw stats (legacy support).
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
            return SpawnMonster(typeName, maxHealth, moveSpeed, contactDamage, expValue, goldValue, position, MonsterType.Slime);
        }

        public MonsterEntity SpawnMonster(
            string typeName,
            float maxHealth,
            float moveSpeed,
            float contactDamage,
            int expValue,
            int goldValue,
            Vector2D position,
            MonsterType type)
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
                eventBus: _eventBus,
                type: type
            );

            _activeMonsters.Add(monster);
            _monsterGrid.Register(monster);
            return monster;
        }

        /// <summary>
        /// Spawns a monster defined by MonsterDefinition.
        /// </summary>
        public MonsterEntity SpawnByDefinition(MonsterDefinition def, Vector2D position, float hpMultiplier = 1.0f, float damageMultiplier = 1.0f)
        {
            MonsterEntity monster = _monsterPool.Spawn();
            monster.InitializeFromDefinition(++_idCounter, def, position, _eventBus, hpMultiplier, damageMultiplier);

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
        /// Spawns a specific archetype around the player.
        /// </summary>
        public MonsterEntity SpawnDefinitionAroundPlayer(
            Vector2D playerPosition,
            float spawnRadius,
            float angleRadians,
            MonsterDefinition def,
            float hpMultiplier = 1.0f)
        {
            float posX = playerPosition.X + (float)Math.Cos(angleRadians) * spawnRadius;
            float posY = playerPosition.Y + (float)Math.Sin(angleRadians) * spawnRadius;
            return SpawnByDefinition(def, new Vector2D(posX, posY), hpMultiplier);
        }

        /// <summary>
        /// Spawns a Boss monster at a specific offset from the player.
        /// </summary>
        public MonsterEntity SpawnBoss(Vector2D playerPosition, string bossName, float hp, float speed, float damage, int exp = 50, int gold = 100)
        {
            var def = MonsterDefinition.CreateBoss(bossName, hp, speed, damage, exp, gold);
            float posX = playerPosition.X + 8.0f;
            float posY = playerPosition.Y + 8.0f;
            return SpawnByDefinition(def, new Vector2D(posX, posY));
        }

        /// <summary>
        /// Updates all active monsters AI, attacks the player if in contact, and updates spatial grid.
        /// </summary>
        public void Update(PlayerEntity player, float deltaTime)
        {
            if (player == null) return;

            for (int i = _activeMonsters.Count - 1; i >= 0; i--)
            {
                var monster = _activeMonsters[i];
                if (!monster.IsActive || monster.IsDead)
                {
                    DespawnMonster(monster, i);
                    continue;
                }

                monster.UpdateAI(player, deltaTime);

                // Handle ranged skeleton attacks
                if (monster.HasPendingRangedAttack)
                {
                    monster.ConsumePendingAttack();
                    if (!player.IsDead)
                    {
                        player.TakeDamage(monster.ContactDamage * 0.8f);
                    }
                }

                _monsterGrid.UpdatePosition(monster);
            }
        }

        /// <summary>
        /// Updates all active monsters AI and spatial grid coordinates (Vector2D target overload).
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
            // Cleaned up on next update loop or explicit call
        }

        private void DespawnMonster(MonsterEntity monster, int index)
        {
            _monsterGrid.Unregister(monster);
            _activeMonsters.RemoveAt(index);
            _monsterPool.Despawn(monster);
        }

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
