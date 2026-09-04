using System;
using System.Collections.Generic;
using UnityEngine;
using HappyShoot.Domain.Entities;
using HappyShoot.Domain.Spatial;
using HappyShoot.View.Monsters;
using HappyShoot.View.Player;

namespace HappyShoot.View.SideScroll
{
    /// <summary>
    /// Spawns massive dimension monster hordes, Unstable Void Crystals, Speed Boost Rings, and Gem Storms in Side-Scrolling mode.
    /// At 300m distance, spawns the massive Dimensional Void Core boss.
    /// Strictly modular and under 500 lines.
    /// </summary>
    public class SideScrollMonsterSpawner : MonoBehaviour
    {
        private PlayerView _playerView;
        private MonsterSpawnerView _mainSpawner;
        private Action _onVoidCoreDefeated;

        private float _spawnTimer;
        private float _crystalTimer;
        private float _ringTimer;
        private float _gemStormTimer;
        private bool _bossSpawned;
        private GameObject _activeVoidCore;

        private readonly List<GameObject> _activeSpawns = new List<GameObject>();

        public bool IsBossDefeated { get; private set; }

        public void Initialize(PlayerView playerView, MonsterSpawnerView mainSpawner, Action onVoidCoreDefeated)
        {
            _playerView = playerView;
            _mainSpawner = mainSpawner;
            _onVoidCoreDefeated = onVoidCoreDefeated;
            _spawnTimer = 0.2f; // Spawn initial horde right away!
            _crystalTimer = 4.0f;
            _ringTimer = 7.0f;
            _gemStormTimer = 9.0f;
        }

        private void Update()
        {
            if (_playerView == null) return;

            float dt = Time.deltaTime;
            float playerX = _playerView.transform.position.x;

            // Clean up spawns that went too far to the left
            for (int i = _activeSpawns.Count - 1; i >= 0; i--)
            {
                if (_activeSpawns[i] == null)
                {
                    _activeSpawns.RemoveAt(i);
                }
                else if (_activeSpawns[i].transform.position.x < playerX - 18f)
                {
                    Destroy(_activeSpawns[i]);
                    _activeSpawns.RemoveAt(i);
                }
            }

            // Normal wave loop before 300m boss
            if (!_bossSpawned && playerX < 300f)
            {
                // 1. Massive Monster Swarms (Every 0.45s ~ 0.75s)
                _spawnTimer -= dt;
                if (_spawnTimer <= 0f)
                {
                    _spawnTimer = UnityEngine.Random.Range(0.45f, 0.75f);
                    SpawnMassiveHorde(playerX);
                }

                // 2. Unstable Explosive Void Crystals (Every 6s ~ 9s)
                _crystalTimer -= dt;
                if (_crystalTimer <= 0f)
                {
                    _crystalTimer = UnityEngine.Random.Range(6.0f, 9.0f);
                    SpawnExplosiveCrystal(playerX);
                }

                // 3. Super Speed Boost Rings (Every 8s ~ 12s)
                _ringTimer -= dt;
                if (_ringTimer <= 0f)
                {
                    _ringTimer = UnityEngine.Random.Range(8.0f, 12.0f);
                    SpawnSpeedRing(playerX);
                }

                // 4. Gem Showers (Every 9s ~ 14s)
                _gemStormTimer -= dt;
                if (_gemStormTimer <= 0f)
                {
                    _gemStormTimer = UnityEngine.Random.Range(9.0f, 14.0f);
                    TriggerGemStorm(playerX);
                }
            }
            else if (!_bossSpawned && playerX >= 300f)
            {
                float halfWidth = GetScreenHalfWidth();
                SpawnVoidCore(playerX + halfWidth + 5.0f);
            }
        }

        private float GetScreenHalfWidth()
        {
            var cam = UnityEngine.Camera.main;
            if (cam != null)
            {
                return Mathf.Max(14.0f, cam.orthographicSize * cam.aspect);
            }
            return 16.0f;
        }

        private void SpawnMassiveHorde(float playerX)
        {
            if (_mainSpawner?.DomainSpawner == null) return;

            // Clean up monsters that fell far behind on the left
            var activeMonsters = _mainSpawner.DomainSpawner.ActiveMonsters;
            for (int i = activeMonsters.Count - 1; i >= 0; i--)
            {
                var m = activeMonsters[i];
                if (m.IsActive && m.Position.X < playerX - 18f)
                {
                    _mainSpawner.DomainSpawner.Despawn(m);
                }
            }

            // Spawn deep off-screen to the right (accounting for camera right-bias +4.5m)
            // Ensures player has ample 3s ~ 4s of visibility from the right edge before contact
            float halfWidth = GetScreenHalfWidth();
            float minOffset = Mathf.Max(halfWidth + 12.0f, 25.0f);
            float spawnX = playerX + minOffset + UnityEngine.Random.Range(0f, 3.5f);

            // Ground Rushers (5 ~ 8 Slimes + Golem Vanguard)
            int groundCount = UnityEngine.Random.Range(5, 8);
            bool spawnGolem = UnityEngine.Random.value < 0.40f;

            for (int i = 0; i < groundCount; i++)
            {
                float posX = spawnX + i * 1.0f;
                float surfaceY = SideScrollPlatformManager.Instance != null
                    ? SideScrollPlatformManager.Instance.GetHighestSurfaceYAt(posX)
                    : -1.8f;

                Vector2D pos = new Vector2D(posX, surfaceY);
                MonsterType mType = (i == 0 && spawnGolem) ? MonsterType.Golem : MonsterType.Slime;
                string name = mType == MonsterType.Golem ? "Golem" : "Slime";
                float hp = mType == MonsterType.Golem ? 140f : 45f;
                float speed = mType == MonsterType.Golem ? 2.2f : 3.4f;

                var runner = _mainSpawner.DomainSpawner.SpawnMonster(name, hp, speed, 10f, 2, 2, pos, mType);
                if (runner != null)
                {
                    runner.IsSideScrollMode = true;
                    runner.SideScrollBaseY = surfaceY;
                    runner.SideScrollWaveAmplitude = 0f;
                    _mainSpawner.EnsureViewForEntity(runner);
                }
            }

            // Aerial Wave (3 ~ 5 Bats flying in wave formation)
            int airCount = UnityEngine.Random.Range(3, 5);
            for (int i = 0; i < airCount; i++)
            {
                float posX = spawnX + i * 1.3f;
                Vector2D pos = new Vector2D(posX, 0.4f);
                var bat = _mainSpawner.DomainSpawner.SpawnMonster("Bat", 35f, 3.8f, 8f, 1, 1, pos, MonsterType.Bat);
                if (bat != null)
                {
                    bat.IsSideScrollMode = true;
                    bat.SideScrollBaseY = 0.4f;
                    bat.SideScrollWaveAmplitude = 0.7f;
                    bat.SideScrollWaveSpeed = 3.2f;
                    _mainSpawner.EnsureViewForEntity(bat);
                }
            }
        }

        private void SpawnExplosiveCrystal(float playerX)
        {
            float halfWidth = GetScreenHalfWidth();
            float crystalX = playerX + halfWidth + 8.0f;
            float crystalY = SideScrollPlatformManager.Instance != null
                ? SideScrollPlatformManager.Instance.GetHighestSurfaceYAt(crystalX)
                : -1.8f;

            var crystalGo = new GameObject("UnstableVoidCrystal");
            crystalGo.transform.position = new Vector3(crystalX, crystalY, 0f);

            var comp = crystalGo.AddComponent<UnstableVoidCrystalView>();
            comp.Initialize(_playerView, _mainSpawner);

            _activeSpawns.Add(crystalGo);
        }

        private void SpawnSpeedRing(float playerX)
        {
            float halfWidth = GetScreenHalfWidth();
            float ringX = playerX + halfWidth + 9.5f;
            float ringY = SideScrollPlatformManager.Instance != null
                ? SideScrollPlatformManager.Instance.GetHighestSurfaceYAt(ringX)
                : -1.8f;

            var ringGo = new GameObject("SpeedBoostRing");
            ringGo.transform.position = new Vector3(ringX, ringY, 0f);
            ringGo.transform.localScale = Vector3.one * 1.5f;

            var comp = ringGo.AddComponent<SpeedBoostRingView>();
            comp.Initialize(_playerView, _mainSpawner);

            _activeSpawns.Add(ringGo);
        }

        private void TriggerGemStorm(float playerX)
        {
            for (int i = 0; i < 10; i++)
            {
                float dropX = playerX + UnityEngine.Random.Range(-3f, 12f);
                float dropY = UnityEngine.Random.Range(3.5f, 6.5f);
                var gemDropGo = new GameObject("GemStormDrop");
                gemDropGo.transform.position = new Vector3(dropX, dropY, 0f);

                var comp = gemDropGo.AddComponent<FallingGemShowerView>();
                comp.Initialize(_playerView);
                _activeSpawns.Add(gemDropGo);
            }
        }

        private MonsterEntity _activeBossEntity;

        private void SpawnVoidCore(float bossX)
        {
            _bossSpawned = true;

            float surfaceY = SideScrollPlatformManager.Instance != null
                ? SideScrollPlatformManager.Instance.GetHighestSurfaceYAt(bossX)
                : -1.0f;

            float coreY = surfaceY + 0.9f;

            // Spawn stationary Boss MonsterEntity into domain spatial grid
            // Enables full auto-targeting for player/companion projectiles, orbit blades, lightning, and melee skills!
            if (_mainSpawner?.DomainSpawner != null)
            {
                _activeBossEntity = _mainSpawner.DomainSpawner.SpawnMonster(
                    "차원의 핵", 3000f, 0f, 15f, 50, 200,
                    new Vector2D(bossX, coreY),
                    MonsterType.Boss);

                if (_activeBossEntity != null)
                {
                    _activeBossEntity.IsSideScrollMode = true;
                    _activeBossEntity.SideScrollBaseY = coreY;
                    _activeBossEntity.SideScrollWaveAmplitude = 0f;
                    _activeBossEntity.Radius = 1.8f;
                }
            }

            _activeVoidCore = new GameObject("DimensionalVoidCore");
            _activeVoidCore.transform.position = new Vector3(bossX, coreY, 0f);

            var coreView = _activeVoidCore.AddComponent<DimensionalVoidCoreView>();
            coreView.Initialize(_playerView, _activeBossEntity, OnBossKilled);
        }

        private void OnBossKilled()
        {
            IsBossDefeated = true;
            _onVoidCoreDefeated?.Invoke();
        }

        public void Cleanup()
        {
            for (int i = 0; i < _activeSpawns.Count; i++)
            {
                if (_activeSpawns[i] != null) Destroy(_activeSpawns[i]);
            }
            _activeSpawns.Clear();

            if (_activeBossEntity != null && _activeBossEntity.IsActive && _mainSpawner?.DomainSpawner != null)
            {
                _mainSpawner.DomainSpawner.Despawn(_activeBossEntity);
                _activeBossEntity = null;
            }

            if (_activeVoidCore != null) Destroy(_activeVoidCore);
        }
    }
}

