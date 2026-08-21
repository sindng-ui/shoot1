using System.Collections.Generic;
using UnityEngine;
using HappyShoot.Domain.Entities;
using HappyShoot.Domain.Spatial;
using HappyShoot.View.Player;
using HappyShoot.View.Projectiles;

namespace HappyShoot.View.Monsters
{
    /// <summary>
    /// Spawns and synchronizes monster views in the Unity scene with variety and boss timeline triggers.
    /// </summary>
    public class MonsterSpawnerView : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private PlayerView _playerView;
        [SerializeField] private GameObject _monsterPrefab;

        [Header("Spawn Settings")]
        [SerializeField] private float _spawnRadius = 21.0f;

        private const int MaxPoolCapacity = 512;
        private MonsterSpawner _domainSpawner;
        private SpatialGrid2D<MonsterEntity> _monsterGrid;
        private readonly List<MonsterView> _viewPool = new List<MonsterView>(MaxPoolCapacity);
        private float _timer;
        private float _elapsedTime;

        private bool _spawnedBoss1;
        private bool _spawnedBoss2;
        private bool _spawnedBoss3;

        private EnemyProjectileManagerView _enemyProjManager;

        public MonsterSpawner DomainSpawner => _domainSpawner;
        public SpatialGrid2D<MonsterEntity> MonsterGrid => _monsterGrid;

        public void SetEnemyProjectileManager(EnemyProjectileManagerView enemyProjManager)
        {
            _enemyProjManager = enemyProjManager;
        }

        private void Awake()
        {
            _monsterGrid = new SpatialGrid2D<MonsterEntity>(cellSize: 2.0f);
            _domainSpawner = new MonsterSpawner(_monsterGrid, initialPoolSize: MaxPoolCapacity);
            PrewarmViewPool(MaxPoolCapacity);
        }

        private void PrewarmViewPool(int count)
        {
            if (_viewPool.Count > 0) return;

            for (int i = 0; i < count; i++)
            {
                var go = new GameObject($"MonsterView_{i + 1}");
                go.transform.SetParent(transform, false);
                var view = go.AddComponent<MonsterView>();
                go.SetActive(false);
                _viewPool.Add(view);
            }
        }

        private void Update()
        {
            if (_playerView == null || _playerView.Entity == null || _playerView.Entity.IsDead)
                return;

            Vector2D playerPos = _playerView.Entity.Position;
            _elapsedTime += Time.deltaTime;

            // Check Boss Timeline Spawns
            CheckBossSpawns(playerPos);

            // Dynamic Exponential Spawn Interval & Max Capacity
            float currentSpawnInterval = GetSpawnInterval(_elapsedTime);
            int currentMaxMonsters = GetMaxMonsters(_elapsedTime);

            // Regular Spawn cycle with exponential curve & archetype weights
            _timer += Time.deltaTime;
            if (_timer >= currentSpawnInterval && _domainSpawner.ActiveCount < currentMaxMonsters)
            {
                _timer = 0f;
                float randomAngle = Random.Range(0f, Mathf.PI * 2f);
                var archetype = RollArchetype(_elapsedTime);
                var monster = _domainSpawner.SpawnDefinitionAroundPlayer(playerPos, _spawnRadius, randomAngle, archetype);

                GetOrCreateView(monster);
            }

            // Update Domain AI, Player Collisions & Spatial Grid
            _domainSpawner.Update(_playerView.Entity, Time.deltaTime);

            // Check for ranged skeleton attacks and spawn visible bone projectiles
            if (_enemyProjManager != null)
            {
                var activeMonsters = _domainSpawner.ActiveMonsters;
                int count = activeMonsters.Count;
                for (int i = 0; i < count; i++)
                {
                    var m = activeMonsters[i];
                    if (m.IsRanged && m.IsActive && !m.IsDead && m.HasPendingRangedAttack)
                    {
                        m.ConsumePendingAttack();
                        float dx = playerPos.X - m.Position.X;
                        float dy = playerPos.Y - m.Position.Y;
                        float dist = Mathf.Sqrt(dx * dx + dy * dy);
                        Vector2 dir = dist > 0.001f ? new Vector2(dx / dist, dy / dist) : Vector2.left;
                        
                        _enemyProjManager.SpawnBoneProjectile(new Vector2(m.Position.X, m.Position.Y), dir, speed: 2.75f, damage: m.ContactDamage * 0.8f);
                    }
                }
            }

            // Update Views
            for (int i = 0; i < _viewPool.Count; i++)
            {
                if (_viewPool[i].gameObject.activeSelf)
                {
                    _viewPool[i].UpdateView();
                }
            }
        }

        private void CheckBossSpawns(Vector2D playerPos)
        {
            // 1 Min: Goblin King
            if (_elapsedTime >= 60f && !_spawnedBoss1)
            {
                _spawnedBoss1 = true;
                var boss = _domainSpawner.SpawnBoss(playerPos, "Goblin King", hp: 800f, speed: 2.2f, damage: 25f, exp: 30, gold: 100);
                GetOrCreateView(boss);
            }
            // 3 Min: Necromancer
            else if (_elapsedTime >= 180f && !_spawnedBoss2)
            {
                _spawnedBoss2 = true;
                var boss = _domainSpawner.SpawnBoss(playerPos, "Necromancer", hp: 2500f, speed: 1.8f, damage: 35f, exp: 80, gold: 300);
                GetOrCreateView(boss);
            }
            // 5 Min: Dragon Fiend
            else if (_elapsedTime >= 300f && !_spawnedBoss3)
            {
                _spawnedBoss3 = true;
                var boss = _domainSpawner.SpawnBoss(playerPos, "Dragon Fiend", hp: 7500f, speed: 2.6f, damage: 50f, exp: 200, gold: 800);
                GetOrCreateView(boss);
            }
        }

        private MonsterDefinition RollArchetype(float time)
        {
            // Early game: Slimes & Bats
            if (time < 45f)
            {
                return Random.value < 0.7f ? MonsterDefinition.Slime : MonsterDefinition.Bat;
            }
            // Mid game: Bats, Skeletons & Slimes
            if (time < 150f)
            {
                float r = Random.value;
                if (r < 0.4f) return MonsterDefinition.Slime;
                if (r < 0.75f) return MonsterDefinition.Bat;
                return MonsterDefinition.Skeleton;
            }
            // Late game: All archetypes including heavy Golems
            float roll = Random.value;
            if (roll < 0.3f) return MonsterDefinition.Slime;
            if (roll < 0.55f) return MonsterDefinition.Bat;
            if (roll < 0.8f) return MonsterDefinition.Skeleton;
            return MonsterDefinition.Golem;
        }

        public void Initialize(PlayerView playerView)
        {
            _playerView = playerView;
            if (playerView != null && playerView.EventBus != null)
            {
                _domainSpawner = new MonsterSpawner(_monsterGrid, playerView.EventBus, initialPoolSize: MaxPoolCapacity);
            }
        }

        private float GetSpawnInterval(float time)
        {
            // Phase 1: 0~55s Exponential acceleration (0.8s -> 0.12s)
            if (time < 55f)
            {
                return Mathf.Max(0.12f, 0.8f * Mathf.Pow(0.965f, time));
            }
            // Phase 1 Boss Arrival: 55s~70s Breathing Pause Buffer (0.55s slow spawn for boss 1:1 engagement)
            if (time < 70f)
            {
                return 0.55f;
            }
            // Phase 2: 70s~175s 2nd Exponential acceleration (0.55s -> 0.08s)
            if (time < 175f)
            {
                float p2Time = time - 70f;
                return Mathf.Max(0.08f, 0.55f * Mathf.Pow(0.975f, p2Time));
            }
            // Phase 2 Boss Arrival: 175s~190s Breathing Pause Buffer (0.45s)
            if (time < 190f)
            {
                return 0.45f;
            }
            // Phase 3: 190s+ Extreme Peak Exponential Pulse (0.05s - 20 monsters/sec swarm!)
            return 0.05f;
        }

        private int GetMaxMonsters(float time)
        {
            if (time < 55f) return 250;
            if (time < 70f) return 180; // Boss 1 breathing capacity reduction
            if (time < 175f) return 400;
            if (time < 190f) return 300; // Boss 2 breathing capacity
            return 500; // Final peak horde
        }

        private MonsterView GetOrCreateView(MonsterEntity entity)
        {
            for (int i = 0; i < _viewPool.Count; i++)
            {
                if (!_viewPool[i].gameObject.activeSelf)
                {
                    _viewPool[i].Bind(entity);
                    return _viewPool[i];
                }
            }

            var go = new GameObject($"MonsterView_{_viewPool.Count + 1}");
            go.transform.SetParent(transform);
            var view = go.AddComponent<MonsterView>();
            view.Bind(entity);
            _viewPool.Add(view);
            return view;
        }
    }
}
