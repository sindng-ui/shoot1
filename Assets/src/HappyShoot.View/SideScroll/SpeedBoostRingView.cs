using UnityEngine;
using HappyShoot.Domain.Entities;
using HappyShoot.Domain.Events;
using HappyShoot.View.Cameras;
using HappyShoot.View.Monsters;
using HappyShoot.View.Player;

namespace HappyShoot.View.SideScroll
{
    /// <summary>
    /// Interactive speed boost ring in side-scrolling corridor.
    /// When passed through, grants 3.5s of Hyper-Speed Overdrive:
    /// - +70% Move Speed
    /// - Roadkill Shockwave: Stomps and blasts away any monsters in path with 100 damage!
    /// Strictly modular and under 500 lines.
    /// </summary>
    public class SpeedBoostRingView : MonoBehaviour
    {
        private PlayerView _playerView;
        private MonsterSpawnerView _mainSpawner;
        private bool _collected;
        private static Sprite _ringSprite;

        public void Initialize(PlayerView playerView, MonsterSpawnerView mainSpawner = null)
        {
            _playerView = playerView;
            _mainSpawner = mainSpawner;

            var sr = gameObject.GetComponent<SpriteRenderer>();
            if (sr == null) sr = gameObject.AddComponent<SpriteRenderer>();
            sr.sprite = GetOrCreateRingSprite();
            sr.sortingOrder = 14;
        }

        private void Update()
        {
            if (_collected || _playerView == null) return;

            // Rotate ring continuously
            transform.Rotate(0f, 0f, 110f * Time.deltaTime);

            if (Vector2.Distance(transform.position, _playerView.transform.position) <= 1.4f)
            {
                TriggerSpeedBoost();
            }
        }

        private void TriggerSpeedBoost()
        {
            _collected = true;

            // Audio & Camera Juice
            _playerView.EventBus?.Publish(new Domain.Events.PlaySoundEvent(Domain.Events.SoundEffectType.WeaponEvolve));
            CameraFollowView.Instance?.TriggerShake(null, 0.4f, 0.3f);

            // Grant 3.5s Hyper-Speed Overdrive & Roadkill buff to Player
            var playerGo = _playerView.gameObject;
            var existingBuff = playerGo.GetComponent<SideScrollHyperSpeedBuff>();
            if (existingBuff == null)
            {
                existingBuff = playerGo.AddComponent<SideScrollHyperSpeedBuff>();
            }
            existingBuff.Activate(3.5f, _playerView, _mainSpawner);

            Destroy(gameObject);
        }

        public static Sprite GetOrCreateRingSprite(int size = 32)
        {
            if (_ringSprite != null) return _ringSprite;
            var tex = new Texture2D(size, size, TextureFormat.RGBA32, false) { filterMode = FilterMode.Point };
            var pixels = new Color[size * size];
            float c = size * 0.5f;

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float dist = Vector2.Distance(new Vector2(x, y), new Vector2(c, c));
                    if (dist >= 10f && dist <= 14f)
                        pixels[y * size + x] = new Color(0.25f, 0.95f, 1.0f, 0.95f);
                    else
                        pixels[y * size + x] = Color.clear;
                }
            }
            tex.SetPixels(pixels);
            tex.Apply();
            _ringSprite = Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), size);
            return _ringSprite;
        }
    }

    /// <summary>
    /// Temporary Hyper-Speed Roadkill Overdrive buff component on Player.
    /// </summary>
    public class SideScrollHyperSpeedBuff : MonoBehaviour
    {
        private float _remainingTimer;
        private PlayerView _playerView;
        private MonsterSpawnerView _mainSpawner;
        private float _originalSpeed;

        // Visual juice: Afterimage ghost trail during Hyper-Speed rush!
        private readonly System.Collections.Generic.List<PlayerDashGhostTrail> _ghostPool = new System.Collections.Generic.List<PlayerDashGhostTrail>(16);
        private GameObject _ghostPoolRoot;
        private float _trailTimer;
        private const float TrailInterval = 0.05f;

        public void Activate(float duration, PlayerView playerView, MonsterSpawnerView mainSpawner)
        {
            _remainingTimer = duration;
            _playerView = playerView;
            _mainSpawner = mainSpawner;
            _trailTimer = 0f;

            if (_ghostPoolRoot == null)
            {
                _ghostPoolRoot = new GameObject("HyperSpeedGhostPool");
            }

            if (_playerView?.Entity != null)
            {
                _originalSpeed = _playerView.Entity.Stats.MoveSpeed;
                var s = _playerView.Entity.Stats;
                _playerView.Entity.Stats = new CharacterStats(
                    s.MaxHealth, s.HealthRegen, _originalSpeed * 1.7f, s.AttackPowerMultiplier,
                    s.Armor, s.CritChance, s.CritDamageMultiplier, s.CooldownReduction,
                    s.AreaMultiplier, s.ProjectileSpeedMultiplier, s.ExtraProjectiles, s.PickupRadius);
            }
        }

        private void Update()
        {
            if (_remainingTimer <= 0f) return;

            float dt = Time.deltaTime;
            _remainingTimer -= dt;

            // 1. Emit luminous afterimage ghost trails
            _trailTimer -= dt;
            if (_trailTimer <= 0f)
            {
                _trailTimer = TrailInterval;
                SpawnAfterimageTrail();
            }

            // 2. Roadkill shockwave: knock back and damage any monsters colliding with player
            if (_mainSpawner?.DomainSpawner != null && _playerView != null)
            {
                Vector2 playerPos = _playerView.transform.position;
                var activeMonsters = _mainSpawner.DomainSpawner.ActiveMonsters;

                for (int i = activeMonsters.Count - 1; i >= 0; i--)
                {
                    var m = activeMonsters[i];
                    if (m.IsActive && !m.IsDead)
                    {
                        float dist = Vector2.Distance(playerPos, new Vector2(m.Position.X, m.Position.Y));
                        if (dist <= 1.8f)
                        {
                            m.TakeDamage(100f, isCritical: false, DamageType.Default);
                        }
                    }
                }
            }

            if (_remainingTimer <= 0f)
            {
                // Revert speed
                if (_playerView?.Entity != null)
                {
                    var s = _playerView.Entity.Stats;
                    _playerView.Entity.Stats = new CharacterStats(
                        s.MaxHealth, s.HealthRegen, _originalSpeed, s.AttackPowerMultiplier,
                        s.Armor, s.CritChance, s.CritDamageMultiplier, s.CooldownReduction,
                        s.AreaMultiplier, s.ProjectileSpeedMultiplier, s.ExtraProjectiles, s.PickupRadius);
                }
                Cleanup();
                Destroy(this);
            }
        }

        private void SpawnAfterimageTrail()
        {
            if (_playerView == null || _playerView.BodyRenderer == null) return;
            var sr = _playerView.BodyRenderer;
            if (sr.sprite == null) return;

            PlayerDashGhostTrail trail = null;
            for (int i = 0; i < _ghostPool.Count; i++)
            {
                if (!_ghostPool[i].gameObject.activeSelf)
                {
                    trail = _ghostPool[i];
                    break;
                }
            }

            if (trail == null)
            {
                var go = new GameObject($"HyperGhostTrail_{_ghostPool.Count}");
                go.transform.SetParent(_ghostPoolRoot != null ? _ghostPoolRoot.transform : null, false);
                trail = go.AddComponent<PlayerDashGhostTrail>();
                _ghostPool.Add(trail);
            }

            trail.Spawn(_playerView.transform.position, sr.sprite, sr.flipX, sr.transform.lossyScale, sr.sortingOrder, 0.35f);
        }

        private void Cleanup()
        {
            if (_ghostPoolRoot != null)
            {
                Destroy(_ghostPoolRoot);
            }
            _ghostPool.Clear();
        }

        private void OnDestroy()
        {
            Cleanup();
        }
    }
}
