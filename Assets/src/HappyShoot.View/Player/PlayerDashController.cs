using System.Collections.Generic;
using UnityEngine;
using HappyShoot.Domain.Entities;
using HappyShoot.Domain.Spatial;

namespace HappyShoot.View.Player
{
    /// <summary>
    /// Handles player Dash mechanics with smooth ease-out deceleration, world-space ghost trails, stock charges, and cooldown.
    /// Strictly modular, zero-allocation pooled, and under 500 lines.
    /// </summary>
    public class PlayerDashController : MonoBehaviour
    {
        public DashConfigData Config { get; set; } = new DashConfigData();

        private PlayerView _playerView;
        private PlayerDashChargeIndicatorView _indicatorView;

        private float _cooldownTimer;
        private bool _isDashing;
        private float _dashTimer;
        private Vector2 _dashStartPos;
        private Vector2 _dashDirection;
        private float _ghostSpawnTimer;

        private int _currentCharges = 1;
        private GameObject _ghostPoolRoot;
        private readonly List<PlayerDashGhostTrail> _ghostPool = new List<PlayerDashGhostTrail>(16);

        public bool IsDashing => _isDashing;
        public int CurrentCharges => _currentCharges;
        public int MaxCharges => Config != null ? Mathf.Max(1, Config.MaxCharges) : 1;
        public float CooldownRemaining => Mathf.Max(0f, _cooldownTimer);
        public float CooldownRatio => Config.Cooldown > 0f ? Mathf.Clamp01(_cooldownTimer / Config.Cooldown) : 0f;
        public bool CanDash => !_isDashing && _currentCharges > 0;

        public void Initialize(PlayerView playerView, DashConfigData config = null)
        {
            _playerView = playerView;
            if (config != null) Config = config;

            _currentCharges = MaxCharges;
            _cooldownTimer = 0f;

            PrewarmGhostPool(12);
        }

        public void SetIndicatorView(PlayerDashChargeIndicatorView indicatorView)
        {
            _indicatorView = indicatorView;
            NotifyChargesChanged();
        }

        private void NotifyChargesChanged()
        {
            if (_indicatorView != null)
            {
                _indicatorView.UpdateCharges(_currentCharges, MaxCharges);
            }
        }

        private void PrewarmGhostPool(int count)
        {
            if (_ghostPoolRoot == null)
            {
                _ghostPoolRoot = new GameObject("DashGhostPool_WorldRoot");
            }

            for (int i = 0; i < count; i++)
            {
                var go = new GameObject($"GhostTrail_{i}");
                go.transform.SetParent(_ghostPoolRoot.transform, false);
                var trail = go.AddComponent<PlayerDashGhostTrail>();
                go.SetActive(false);
                _ghostPool.Add(trail);
            }
        }

        public bool TryDash(Vector2 inputDirection)
        {
            if (!CanDash || _playerView == null || _playerView.Entity == null || _playerView.Entity.IsDead)
                return false;

            // Determine dash direction
            if (inputDirection.sqrMagnitude > 0.01f)
            {
                _dashDirection = inputDirection.normalized;
            }
            else
            {
                bool facingLeft = _playerView.BodyRenderer != null && _playerView.BodyRenderer.flipX;
                _dashDirection = facingLeft ? Vector2.left : Vector2.right;
            }

            _isDashing = true;
            _dashTimer = 0f;
            _ghostSpawnTimer = 0f;
            _dashStartPos = transform.position;

            // Consume one charge
            _currentCharges = Mathf.Max(0, _currentCharges - 1);
            NotifyChargesChanged();

            // If cooldown timer was idle, start it now
            if (_cooldownTimer <= 0f)
            {
                _cooldownTimer = Config.Cooldown;
            }

            SpawnGhostTrail();
            return true;
        }

        private void Update()
        {
            float dt = Time.deltaTime;

            // Recharge stock charges over time
            if (_currentCharges < MaxCharges)
            {
                _cooldownTimer -= dt;
                if (_cooldownTimer <= 0f)
                {
                    _currentCharges = Mathf.Min(MaxCharges, _currentCharges + 1);
                    NotifyChargesChanged();

                    // If still missing charges, chain next cooldown cycle
                    if (_currentCharges < MaxCharges)
                    {
                        _cooldownTimer = Config.Cooldown;
                    }
                    else
                    {
                        _cooldownTimer = 0f;
                    }
                }
            }

            if (!_isDashing) return;

            _dashTimer += dt;
            float duration = Mathf.Max(0.05f, Config.Duration);
            float u = Mathf.Clamp01(_dashTimer / duration);

            // Ease-out deceleration curve
            float p = 1.0f - Mathf.Pow(1.0f - u, Config.DecelExponent);
            Vector2 currentTargetPos = _dashStartPos + _dashDirection * (Config.Distance * p);

            if (_playerView != null && _playerView.Entity != null)
            {
                _playerView.Entity.SetPosition(new Vector2D(currentTargetPos.x, currentTargetPos.y));
                transform.position = new Vector3(currentTargetPos.x, currentTargetPos.y, 0f);
            }

            _ghostSpawnTimer += dt;
            float interval = Mathf.Clamp(Config.GhostInterval, 0.020f, 0.05f);
            if (_ghostSpawnTimer >= interval)
            {
                _ghostSpawnTimer = 0f;
                SpawnGhostTrail();
            }

            if (u >= 1.0f)
            {
                _isDashing = false;
                SpawnGhostTrail();
            }
        }

        private void SpawnGhostTrail()
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
                var go = new GameObject($"GhostTrail_{_ghostPool.Count}");
                go.transform.SetParent(_ghostPoolRoot != null ? _ghostPoolRoot.transform : null, false);
                trail = go.AddComponent<PlayerDashGhostTrail>();
                _ghostPool.Add(trail);
            }

            trail.Spawn(transform.position, sr.sprite, sr.flipX, sr.transform.lossyScale, sr.sortingOrder, 0.38f);
        }

        private void OnDestroy()
        {
            if (_ghostPoolRoot != null)
            {
                Destroy(_ghostPoolRoot);
            }
        }
    }
}
