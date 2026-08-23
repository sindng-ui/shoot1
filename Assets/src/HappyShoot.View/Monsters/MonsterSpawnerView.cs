using System.Collections.Generic;
using UnityEngine;
using HappyShoot.Domain.Entities;
using HappyShoot.Domain.Events;
using HappyShoot.Domain.Spatial;
using HappyShoot.View.Player;
using HappyShoot.View.Projectiles;

namespace HappyShoot.View.Monsters
{
    /// <summary>
    /// Spawns and synchronizes monster views with phase-based wave evolution.
    /// Phase 1 -> Boss 1 -> Phase 2 (3 new monster types) -> Boss 2.
    /// Also manages BossLaserBeamManagerView lifecycle.
    /// </summary>
    public class MonsterSpawnerView : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private PlayerView _playerView;

        [Header("Spawn Settings")]
        [SerializeField] private float _spawnRadius = 21.0f;

        private const int MaxPoolCapacity = 1200;
        private MonsterSpawner _domainSpawner;
        private SpatialGrid2D<MonsterEntity> _monsterGrid;
        private readonly List<MonsterView> _viewPool = new List<MonsterView>(MaxPoolCapacity);
        private readonly Dictionary<int, MonsterView> _activeViewMap = new Dictionary<int, MonsterView>(MaxPoolCapacity);
        private HappyShoot.Domain.Leveling.LevelSystem _levelSystem;

        private float _timer;
        private float _elapsedTime;

        // Phase 1 bosses
        private bool _spawnedBoss1;
        // Phase 2 boss
        private bool _spawnedBoss2;
        // Boss tracking for laser system
        private MonsterEntity _activeBoss;

        private EnemyProjectileManagerView _enemyProjManager;
        private BossLaserBeamManagerView _laserManager;
        private readonly WavePhaseController _phaseCtrl = new WavePhaseController();

        public MonsterSpawner DomainSpawner => _domainSpawner;
        public SpatialGrid2D<MonsterEntity> MonsterGrid => _monsterGrid;
        public bool IsSpawningSuppressed { get; set; } = false;

        public void SetEnemyProjectileManager(EnemyProjectileManagerView mgr) => _enemyProjManager = mgr;
        public void SetLevelSystem(HappyShoot.Domain.Leveling.LevelSystem levelSystem) => _levelSystem = levelSystem;

        private void Awake()
        {
            _monsterGrid = new SpatialGrid2D<MonsterEntity>(cellSize: 2.0f);
            _domainSpawner = new MonsterSpawner(_monsterGrid, initialPoolSize: MaxPoolCapacity);
            PrewarmViewPool(MaxPoolCapacity);

            // Laser beam system
            var laserGo = new GameObject("BossLaserManager");
            laserGo.transform.SetParent(transform, false);
            _laserManager = laserGo.AddComponent<BossLaserBeamManagerView>();
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

        public void SpawnTrainingDummies(Vector2D center, int count = 5)
        {
            _domainSpawner.DespawnAll();
            _activeViewMap.Clear();
            for (int v = 0; v < _viewPool.Count; v++) _viewPool[v].gameObject.SetActive(false);

            float radius = 2.2f;
            float step = (Mathf.PI * 2f) / count;
            for (int i = 0; i < count; i++)
            {
                float angle = i * step;
                Vector2D pos = center + new Vector2D(
                    (float)System.Math.Cos(angle) * radius,
                    (float)System.Math.Sin(angle) * radius);
                var monster = _domainSpawner.SpawnMonster("훈련용 허수아비", 999999f, 0f, 0f, 1, 0, pos, MonsterType.Golem);
                GetOrCreateView(monster);
            }
            SpawnBatDummies(center, 12);
        }

        public void SpawnBatDummies(Vector2D center, int batCount = 10)
        {
            for (int i = 0; i < batCount; i++)
            {
                float angle = Random.Range(0f, Mathf.PI * 2f);
                float dist  = Random.Range(2.5f, 6.0f);
                Vector2D pos = center + new Vector2D(Mathf.Cos(angle) * dist, Mathf.Sin(angle) * dist);
                float speed  = Random.Range(3.8f, 5.2f);
                var bat = _domainSpawner.SpawnMonster("박쥐 허수아비", 999999f, speed, 0f, 1, 0, pos, MonsterType.Bat);
                GetOrCreateView(bat);
            }
        }

        public void Initialize(PlayerView playerView, HappyShoot.Domain.Leveling.LevelSystem levelSystem = null)
        {
            _playerView = playerView;
            _levelSystem = levelSystem;
            if (playerView != null && playerView.EventBus != null)
            {
                _domainSpawner = new MonsterSpawner(_monsterGrid, playerView.EventBus, initialPoolSize: MaxPoolCapacity);
                playerView.EventBus.Subscribe<MonsterDamagedEvent>(OnMonsterDamaged);
                playerView.EventBus.Subscribe<BossDiedEvent>(OnBossDied);

                // Give laser manager access to event bus and player
                _laserManager.Initialize(playerView.EventBus, playerView);
            }
        }

        private void OnMonsterDamaged(MonsterDamagedEvent evt)
        {
            if (_activeViewMap.TryGetValue(evt.MonsterId, out var view) && view != null && view.gameObject.activeSelf)
                view.OnHitFeedback(evt.IsCritical);
        }

        private void OnBossDied(BossDiedEvent evt)
        {
            bool isSecondBoss = _spawnedBoss1 && evt.BossName != "Goblin King";
            _phaseCtrl.OnBossDefeated(isSecondBoss);
            _laserManager.ClearBoss();
            _activeBoss = null;
        }

        private void Update()
        {
            if (_playerView == null || _playerView.Entity == null || _playerView.Entity.IsDead)
                return;

            Vector2D playerPos = _playerView.Entity.Position;

            if (IsSpawningSuppressed)
            {
                _domainSpawner.Update(_playerView.Entity, Time.deltaTime);
                for (int i = 0; i < _viewPool.Count; i++)
                    if (_viewPool[i].gameObject.activeSelf)
                        _viewPool[i].UpdateView();
                return;
            }

            float dt = Time.deltaTime;
            _elapsedTime += dt;
            _phaseCtrl.Update(dt);

            CheckBossSpawns(playerPos);

            float currentSpawnInterval = GetSpawnInterval(_elapsedTime);
            int currentMaxMonsters = GetMaxMonsters(_elapsedTime);

            var monsterCfg = Config.SkillConfigRepository.Instance.GetConfig()?.Monsters;

            _timer += dt;
            if (_timer >= currentSpawnInterval && _domainSpawner.ActiveCount < currentMaxMonsters)
            {
                _timer = 0f;
                Vector2 moveDir = _playerView != null ? _playerView.CurrentMoveDirection : Vector2.zero;
                float spawnAngle = GetBiasedSpawnAngle(moveDir);

                MonsterDefinition archetype = _phaseCtrl.Boss1Defeated
                    ? _phaseCtrl.RollPhase2Archetype(monsterCfg)
                    : _phaseCtrl.RollPhase1Archetype(_elapsedTime, monsterCfg);

                var monster = _domainSpawner.SpawnDefinitionAroundPlayer(playerPos, _spawnRadius, spawnAngle, archetype);
                GetOrCreateView(monster);
            }

            _domainSpawner.Update(_playerView.Entity, dt);

            // Skeleton ranged attacks
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
                        float skelProjSpeed = monsterCfg != null ? monsterCfg.Skeleton.ProjectileSpeed : 2.75f;
                        float skelProjDmg = monsterCfg != null ? monsterCfg.Skeleton.ProjectileDamage : m.ContactDamage * 0.8f;
                        _enemyProjManager.SpawnBoneProjectile(
                            new Vector2(m.Position.X, m.Position.Y), dir,
                            speed: skelProjSpeed, damage: skelProjDmg);
                    }
                }
            }

            for (int i = 0; i < _viewPool.Count; i++)
                if (_viewPool[i].gameObject.activeSelf)
                    _viewPool[i].UpdateView();
        }

        private void CheckBossSpawns(Vector2D playerPos)
        {
            var monsterCfg = Config.SkillConfigRepository.Instance.GetConfig()?.Monsters;

            // Phase 1: 60s Boss 1
            if (_elapsedTime >= 60f && !_spawnedBoss1)
            {
                _spawnedBoss1 = true;
                float bossHp = monsterCfg != null ? monsterCfg.Boss.MaxHealth : 800f;
                float bossSpd = monsterCfg != null ? monsterCfg.Boss.MoveSpeed : 2.2f;
                float bossDmg = monsterCfg != null ? monsterCfg.Boss.ContactDamage : 25f;
                int bossExp = monsterCfg != null ? monsterCfg.Boss.ExpValue : 30;
                int bossGold = monsterCfg != null ? monsterCfg.Boss.GoldValue : 100;

                var boss = _domainSpawner.SpawnBoss(playerPos, "Goblin King",
                    hp: bossHp, speed: bossSpd, damage: bossDmg, exp: bossExp, gold: bossGold);
                GetOrCreateView(boss);
                _activeBoss = boss;
                _laserManager.SetActiveBoss(boss);
            }
            // Phase 2: after wave 3 fully deployed (110s post boss1 defeat)
            else if (_phaseCtrl.CurrentPhase == WavePhaseController.Phase.Boss2Spawned && !_spawnedBoss2)
            {
                _spawnedBoss2 = true;
                var boss = _domainSpawner.SpawnBoss(playerPos, "Dragon Fiend",
                    hp: 7500f, speed: 2.6f, damage: 50f, exp: 200, gold: 800);
                GetOrCreateView(boss);
                _activeBoss = boss;
                _laserManager.SetActiveBoss(boss);
            }
        }

        private float GetBiasedSpawnAngle(Vector2 moveDir)
        {
            // If player is stationary, spawn uniformly across full 360 degrees
            if (moveDir.sqrMagnitude < 0.01f)
            {
                return Random.Range(0f, Mathf.PI * 2f);
            }

            float moveAngle = Mathf.Atan2(moveDir.y, moveDir.x);

            // 90% chance: Rear & flank 240-degree arc (moveAngle + 60 deg to moveAngle + 300 deg)
            // 10% chance: Forward 120-degree escape cone (moveAngle - 60 deg to moveAngle + 60 deg)
            bool spawnInEscapeCone = Random.value < 0.10f;
            if (spawnInEscapeCone)
            {
                float offset = Random.Range(-60f, 60f) * Mathf.Deg2Rad;
                return moveAngle + offset;
            }
            else
            {
                float offset = Random.Range(60f, 300f) * Mathf.Deg2Rad;
                return moveAngle + offset;
            }
        }

        private float GetExpGrowthScale()
        {
            var expCfg = Config.SkillConfigRepository.Instance.GetConfig()?.Exp;
            if (expCfg == null || !expCfg.EnableLevelExpScaling || _levelSystem == null)
                return 1.0f;

            int currentLevel = _levelSystem.Level;
            if (currentLevel <= 1) return 1.0f;

            int baseExp = _levelSystem.CalculateRequiredExp(1);
            int currentReqExp = _levelSystem.CalculateRequiredExp(currentLevel);
            if (baseExp <= 0) return 1.0f;

            float rawExpScale = (float)currentReqExp / baseExp;
            float expIncrease = Mathf.Max(0f, rawExpScale - 1.0f);
            float ratio = expCfg.MobScalingRatio; // 기본 30% (0.30f)

            float effectiveScale = 1.0f + (expIncrease * ratio);
            return Mathf.Max(1.0f, effectiveScale);
        }

        private float GetSpawnInterval(float time)
        {
            float baseInterval;
            if (_phaseCtrl.Boss1Defeated)
            {
                baseInterval = _phaseCtrl.CurrentPhase == WavePhaseController.Phase.Phase2Wave1
                    ? 0.35f
                    : _phaseCtrl.CurrentPhase == WavePhaseController.Phase.Phase2Wave2
                        ? 0.22f
                        : 0.12f;
            }
            else
            {
                if (time < 55f)  baseInterval = Mathf.Max(0.12f, 0.8f * Mathf.Pow(0.965f, time));
                else if (time < 70f)  baseInterval = 0.55f;  // Boss 1 breathing room
                else if (time < 175f) baseInterval = Mathf.Max(0.08f, 0.55f * Mathf.Pow(0.975f, time - 70f));
                else baseInterval = 0.05f;
            }

            float expScale = GetExpGrowthScale();
            float speedMult = Mathf.Clamp(Mathf.Sqrt(expScale), 1.0f, 3.5f);
            return Mathf.Max(0.03f, baseInterval / speedMult);
        }

        private int GetMaxMonsters(float time)
        {
            int baseMax;
            if (_phaseCtrl.Boss1Defeated) baseMax = 380;
            else if (time < 55f)  baseMax = 250;
            else if (time < 70f)  baseMax = 180;
            else if (time < 175f) baseMax = 400;
            else baseMax = 500;

            var expCfg = Config.SkillConfigRepository.Instance.GetConfig()?.Exp;
            int capLimit = expCfg != null ? expCfg.MaxMonsterCapLimit : MaxPoolCapacity;
            capLimit = Mathf.Clamp(capLimit, 100, MaxPoolCapacity);

            float expScale = GetExpGrowthScale();
            int scaledMax = Mathf.RoundToInt(baseMax * expScale);
            return Mathf.Clamp(scaledMax, baseMax, capLimit);
        }

        private MonsterView GetOrCreateView(MonsterEntity entity)
        {
            if (entity == null) return null;

            for (int i = 0; i < _viewPool.Count; i++)
            {
                if (!_viewPool[i].gameObject.activeSelf)
                {
                    _viewPool[i].Bind(entity);
                    _activeViewMap[entity.Id] = _viewPool[i];
                    return _viewPool[i];
                }
            }

            var go = new GameObject($"MonsterView_{_viewPool.Count + 1}");
            go.transform.SetParent(transform);
            var view = go.AddComponent<MonsterView>();
            view.Bind(entity);
            _viewPool.Add(view);
            _activeViewMap[entity.Id] = view;
            return view;
        }
    }
}
