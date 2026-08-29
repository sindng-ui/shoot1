using UnityEngine;
using HappyShoot.Domain.Entities;
using HappyShoot.View.Player;
using HappyShoot.View.Projectiles;

namespace HappyShoot.View.Monsters
{
    /// <summary>
    /// AI and menacing combat pattern controller for the Final Boss (Arch-Lich Malakar).
    /// Orchestrates 8-way soul spirals, triple death slashes, and undead escort summonings.
    /// Strictly modular and under 500 lines.
    /// </summary>
    public class ArchLichPatternController : MonoBehaviour
    {
        private MonsterEntity _lichEntity;
        private PlayerView _playerView;
        private EnemyProjectileManagerView _projManager;
        private MonsterSpawner _domainSpawner;
        private System.Action<MonsterEntity> _onMinionSpawned;

        private float _soulSpiralTimer;
        private const float SoulSpiralInterval = 2.2f;
        private float _spiralAngleOffset;

        private float _deathSlashTimer;
        private const float DeathSlashInterval = 4.2f;
        private int _slashesLeft;
        private float _slashBurstSubTimer;

        private float _summonTimer;
        private const float SummonInterval = 9.0f;

        public void Initialize(
            MonsterEntity lichEntity,
            PlayerView playerView,
            EnemyProjectileManagerView projManager,
            MonsterSpawner domainSpawner,
            System.Action<MonsterEntity> onMinionSpawned)
        {
            _lichEntity = lichEntity;
            _playerView = playerView;
            _projManager = projManager;
            _domainSpawner = domainSpawner;
            _onMinionSpawned = onMinionSpawned;

            _soulSpiralTimer = 1.0f;
            _deathSlashTimer = 2.5f;
            _summonTimer = 4.0f;
        }

        public void Clear()
        {
            _lichEntity = null;
        }

        private void Update()
        {
            if (_lichEntity == null || !_lichEntity.IsActive || _lichEntity.IsDead) return;
            if (_playerView == null || _playerView.Entity == null || _playerView.Entity.IsDead) return;

            float dt = Time.deltaTime;
            Vector2 bossPos = new Vector2(_lichEntity.Position.X, _lichEntity.Position.Y);
            Vector2 playerPos = new Vector2(_playerView.Entity.Position.X, _playerView.Entity.Position.Y);

            // 1. 8-Way Soul Barrage Spiral (360도 나선 영혼 마탄 난사)
            _soulSpiralTimer -= dt;
            if (_soulSpiralTimer <= 0f)
            {
                _soulSpiralTimer = SoulSpiralInterval;
                _spiralAngleOffset += 22.5f; // Rotates slightly each burst

                if (_projManager != null)
                {
                    const int projCount = 8;
                    for (int i = 0; i < projCount; i++)
                    {
                        float angleDeg = (360f / projCount) * i + _spiralAngleOffset;
                        float rad = angleDeg * Mathf.Deg2Rad;
                        Vector2 dir = new Vector2(Mathf.Cos(rad), Mathf.Sin(rad));
                        _projManager.SpawnSoulOrbProjectile(bossPos, dir, speed: 3.4f, damage: 32f);
                    }
                }
            }

            // 2. Triple Death Slash (플레이어 조준 3연발 암흑 쾌속 참격파)
            _deathSlashTimer -= dt;
            if (_deathSlashTimer <= 0f && _slashesLeft <= 0)
            {
                _deathSlashTimer = DeathSlashInterval;
                _slashesLeft = 3;
                _slashBurstSubTimer = 0f;
            }

            if (_slashesLeft > 0)
            {
                _slashBurstSubTimer -= dt;
                if (_slashBurstSubTimer <= 0f)
                {
                    _slashBurstSubTimer = 0.22f; // Fast 0.22s burst
                    _slashesLeft--;

                    Vector2 toPlayer = (playerPos - bossPos).normalized;
                    if (toPlayer.sqrMagnitude < 0.01f) toPlayer = Vector2.left;

                    // Slight angular fan spread (-10 deg, 0 deg, +10 deg)
                    float spreadAngle = (_slashesLeft - 1) * 12f;
                    Vector2 slashDir = Quaternion.Euler(0f, 0f, spreadAngle) * toPlayer;

                    _projManager?.SpawnDarkSlashProjectile(bossPos, slashDir, speed: 4.5f, damage: 38f);
                }
            }

            // 3. Summon Undead Escort (망령 2마리 + 사령술사 1마리 즉시 소환)
            _summonTimer -= dt;
            if (_summonTimer <= 0f)
            {
                _summonTimer = SummonInterval;
                if (_domainSpawner != null)
                {
                    // Spawn 2 Wraiths
                    SpawnMinion(bossPos + new Vector2(-2.0f, 1.5f), MonsterType.Wraith);
                    SpawnMinion(bossPos + new Vector2(2.0f, -1.5f), MonsterType.Wraith);
                    // Spawn 1 Necromancer
                    SpawnMinion(bossPos + new Vector2(0f, 2.5f), MonsterType.Necromancer);
                }
            }
        }

        private void SpawnMinion(Vector2 pos, MonsterType type)
        {
            var def = MonsterDefinition.FromConfig(type, null);
            var minion = _domainSpawner.SpawnByDefinition(def, new Domain.Spatial.Vector2D(pos.x, pos.y), hpMultiplier: 1.2f);
            _onMinionSpawned?.Invoke(minion);
        }
    }
}
