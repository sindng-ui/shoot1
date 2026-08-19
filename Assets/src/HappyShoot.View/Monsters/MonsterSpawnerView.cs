using System.Collections.Generic;
using UnityEngine;
using HappyShoot.Domain.Entities;
using HappyShoot.Domain.Spatial;
using HappyShoot.View.Player;

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
        [SerializeField] private float _spawnInterval = 0.8f;
        [SerializeField] private float _spawnRadius = 12.0f;
        [SerializeField] private int _maxActiveMonsters = 300;

        private MonsterSpawner _domainSpawner;
        private SpatialGrid2D<MonsterEntity> _monsterGrid;
        private readonly List<MonsterView> _viewPool = new List<MonsterView>(256);
        private float _timer;
        private float _elapsedTime;

        private bool _spawnedBoss1;
        private bool _spawnedBoss2;
        private bool _spawnedBoss3;

        public MonsterSpawner DomainSpawner => _domainSpawner;
        public SpatialGrid2D<MonsterEntity> MonsterGrid => _monsterGrid;

        private void Awake()
        {
            _monsterGrid = new SpatialGrid2D<MonsterEntity>(cellSize: 2.0f);
            _domainSpawner = new MonsterSpawner(_monsterGrid, initialPoolSize: 64);
        }

        private void Update()
        {
            if (_playerView == null || _playerView.Entity == null || _playerView.Entity.IsDead)
                return;

            Vector2D playerPos = _playerView.Entity.Position;
            _elapsedTime += Time.deltaTime;

            // Check Boss Timeline Spawns
            CheckBossSpawns(playerPos);

            // Regular Spawn cycle with archetype weights
            _timer += Time.deltaTime;
            if (_timer >= _spawnInterval && _domainSpawner.ActiveCount < _maxActiveMonsters)
            {
                _timer = 0f;
                float randomAngle = Random.Range(0f, Mathf.PI * 2f);
                var archetype = RollArchetype(_elapsedTime);
                var monster = _domainSpawner.SpawnDefinitionAroundPlayer(playerPos, _spawnRadius, randomAngle, archetype);

                GetOrCreateView(monster);
            }

            // Update Domain AI, Player Collisions & Spatial Grid
            _domainSpawner.Update(_playerView.Entity, Time.deltaTime);

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
                _domainSpawner = new MonsterSpawner(_monsterGrid, playerView.EventBus, initialPoolSize: 64);
            }
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
