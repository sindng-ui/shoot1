using System.Collections.Generic;
using UnityEngine;
using HappyShoot.Domain.Entities;
using HappyShoot.Domain.Spatial;
using HappyShoot.View.Player;

namespace HappyShoot.View.Monsters
{
    /// <summary>
    /// Spawns and synchronizes monster views in the Unity scene.
    /// </summary>
    public class MonsterSpawnerView : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private PlayerView _playerView;
        [SerializeField] private GameObject _monsterPrefab;

        [Header("Spawn Settings")]
        [SerializeField] private float _spawnInterval = 1.0f;
        [SerializeField] private float _spawnRadius = 12.0f;
        [SerializeField] private int _maxActiveMonsters = 300;

        private MonsterSpawner _domainSpawner;
        private SpatialGrid2D<MonsterEntity> _monsterGrid;
        private readonly List<MonsterView> _viewPool = new List<MonsterView>(256);
        private float _timer;

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

            // Spawn cycle
            _timer += Time.deltaTime;
            if (_timer >= _spawnInterval && _domainSpawner.ActiveCount < _maxActiveMonsters)
            {
                _timer = 0f;
                float randomAngle = Random.Range(0f, Mathf.PI * 2f);
                var monster = _domainSpawner.SpawnAroundPlayer(playerPos, _spawnRadius, randomAngle);

                GetOrCreateView(monster);
            }

            // Update Domain AI & Spatial Grid
            _domainSpawner.Update(playerPos, Time.deltaTime);

            // Update Views
            for (int i = 0; i < _viewPool.Count; i++)
            {
                if (_viewPool[i].gameObject.activeSelf)
                {
                    _viewPool[i].UpdateView();
                }
            }
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

            GameObject go;
            if (_monsterPrefab != null)
            {
                go = Instantiate(_monsterPrefab, transform);
            }
            else
            {
                go = new GameObject($"MonsterView_{_viewPool.Count + 1}");
                go.transform.SetParent(transform);
                go.transform.localScale = Vector3.one * 0.7f;
                var sr = go.AddComponent<SpriteRenderer>();
                sr.sprite = Utils.SpriteHelper.GetOrCreateCircleSprite();
                sr.color = Color.green; // Default green slime
            }

            var view = go.GetComponent<MonsterView>() ?? go.AddComponent<MonsterView>();
            view.Bind(entity);
            _viewPool.Add(view);
            return view;
        }
    }
}
