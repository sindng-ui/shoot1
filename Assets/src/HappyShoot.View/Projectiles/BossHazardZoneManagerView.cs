using System.Collections.Generic;
using UnityEngine;
using HappyShoot.Domain.Entities;
using HappyShoot.Domain.Events;
using HappyShoot.View.Cameras;
using HappyShoot.View.Player;
using HappyShoot.View.Utils;

namespace HappyShoot.View.Projectiles
{
    /// <summary>
    /// Ultra high-performance 0-allocation pooled manager for Boss Hazard AoE Zones.
    /// Manages large telegraph warning rings (1.2s) followed by fiery magma hazard detonation (2.0s).
    /// Strictly modular and under 240 lines (500-line architecture rule).
    /// </summary>
    public class BossHazardZoneManagerView : MonoBehaviour
    {
        private enum ZoneState
        {
            Inactive = 0,
            Telegraph = 1,
            ActiveHazard = 2,
            FadeOut = 3
        }

        private struct HazardZone
        {
            public Vector2 Position;
            public float Radius;
            public float Damage;
            public float Timer;
            public float MaxTimer;
            public float TickTimer;
            public ZoneState State;
        }

        private const int PoolCapacity = 16;
        private readonly HazardZone[] _zones = new HazardZone[PoolCapacity];
        private readonly GameObject[] _gameObjects = new GameObject[PoolCapacity];
        private readonly Transform[] _transforms = new Transform[PoolCapacity];
        private readonly SpriteRenderer[] _renderers = new SpriteRenderer[PoolCapacity];

        private const float TelegraphDuration = 1.2f;
        private const float ActiveDuration = 2.0f;
        private const float FadeDuration = 0.35f;
        private const float DamageTickInterval = 0.20f;

        private Sprite _warningSprite;
        private Sprite _magmaSprite;

        private EventBus _eventBus;
        private PlayerView _playerView;
        private MonsterEntity _boss;

        private float _fireInterval = 6.5f;
        private float _hazardDamage = 18.0f;
        private float _hazardRadius = 2.8f;
        private float _fireTimer;

        public void Initialize(EventBus eventBus, PlayerView playerView)
        {
            _eventBus = eventBus;
            _playerView = playerView;
            _warningSprite = SpriteHelper.GetOrCreateWarningCircleSprite(64);
            _magmaSprite = SpriteHelper.GetOrCreateHazardMagmaSprite(64);
            _fireTimer = 2.5f; // First hazard fires early for dynamic pacing

            for (int i = 0; i < PoolCapacity; i++)
            {
                var go = new GameObject($"BossHazard_{i + 1}");
                go.transform.SetParent(transform, false);
                var sr = go.AddComponent<SpriteRenderer>();
                sr.sortingOrder = 3; // Above floor / craters, below monsters
                go.SetActive(false);

                _gameObjects[i] = go;
                _transforms[i] = go.transform;
                _renderers[i] = sr;
                _zones[i] = default;
            }
        }

        public void SetActiveBoss(MonsterEntity boss)
        {
            _boss = boss;
            var cfg = Config.SkillConfigRepository.Instance.GetConfig()?.Monsters?.Boss;
            if (cfg != null)
            {
                _fireInterval = cfg.HazardInterval > 0.5f ? cfg.HazardInterval : 6.5f;
                _hazardDamage = cfg.HazardDamage > 0f ? cfg.HazardDamage : 18.0f;
                _hazardRadius = cfg.HazardRadius > 0.5f ? cfg.HazardRadius : 2.8f;
            }
            _fireTimer = 2.5f;
        }

        public void ClearBoss()
        {
            _boss = null;
            for (int i = 0; i < PoolCapacity; i++)
            {
                _zones[i].State = ZoneState.Inactive;
                if (_gameObjects[i] != null) _gameObjects[i].SetActive(false);
            }
        }

        private void Update()
        {
            if (_boss == null || _boss.IsDead || !_boss.IsActive) return;

            float dt = Time.deltaTime;

            // Hazard spawn timer
            _fireTimer -= dt;
            if (_fireTimer <= 0f)
            {
                var cfg = Config.SkillConfigRepository.Instance.GetConfig()?.Monsters?.Boss;
                if (cfg != null)
                {
                    _fireInterval = cfg.HazardInterval > 0.5f ? cfg.HazardInterval : 6.5f;
                    _hazardDamage = cfg.HazardDamage > 0f ? cfg.HazardDamage : 18.0f;
                    _hazardRadius = cfg.HazardRadius > 0.5f ? cfg.HazardRadius : 2.8f;
                }
                _fireTimer = _fireInterval;
                SpawnHazardAtPlayer();
            }

            UpdateActiveZones(dt);
        }

        private void SpawnHazardAtPlayer()
        {
            if (_playerView == null || _playerView.Entity == null || _playerView.Entity.IsDead) return;

            Vector2 targetPos = _playerView.transform.position;
            // Add slight predictive offset based on player movement
            if (_playerView.CurrentMoveDirection.sqrMagnitude > 0.01f)
            {
                targetPos += _playerView.CurrentMoveDirection * 0.8f;
            }

            for (int i = 0; i < PoolCapacity; i++)
            {
                if (_zones[i].State == ZoneState.Inactive)
                {
                    _zones[i].State = ZoneState.Telegraph;
                    _zones[i].Position = targetPos;
                    _zones[i].Radius = _hazardRadius;
                    _zones[i].Damage = _hazardDamage;
                    _zones[i].Timer = TelegraphDuration;
                    _zones[i].MaxTimer = TelegraphDuration;
                    _zones[i].TickTimer = 0f;

                    var tf = _transforms[i];
                    tf.position = new Vector3(targetPos.x, targetPos.y, 0f);
                    float diameter = _hazardRadius * 2.0f;
                    tf.localScale = new Vector3(diameter, diameter, 1f);

                    var sr = _renderers[i];
                    sr.sprite = _warningSprite;
                    sr.color = new Color(1f, 0.2f, 0.1f, 0.7f);
                    sr.sortingOrder = 3;

                    _gameObjects[i].SetActive(true);
                    return;
                }
            }
        }

        private void UpdateActiveZones(float dt)
        {
            if (_playerView == null || _playerView.Entity == null) return;
            var playerPos = (Vector2)_playerView.transform.position;
            var entity = _playerView.Entity;
            const float playerRadius = 0.45f;

            for (int i = 0; i < PoolCapacity; i++)
            {
                if (_zones[i].State == ZoneState.Inactive) continue;

                _zones[i].Timer -= dt;

                if (_zones[i].State == ZoneState.Telegraph)
                {
                    // Telegraph Phase: Warning pulse
                    float progress = 1f - Mathf.Clamp01(_zones[i].Timer / _zones[i].MaxTimer);
                    float pulse = 0.5f + 0.45f * Mathf.PingPong(progress * 7f, 1f);
                    _renderers[i].color = new Color(1.0f, 0.15f, 0.10f, pulse);

                    if (_zones[i].Timer <= 0f)
                    {
                        // Transition to Active Hazard (Magma blast)
                        _zones[i].State = ZoneState.ActiveHazard;
                        _zones[i].Timer = ActiveDuration;
                        _zones[i].MaxTimer = ActiveDuration;
                        _zones[i].TickTimer = 0f;

                        _renderers[i].sprite = _magmaSprite;
                        _renderers[i].color = Color.white;
                        _renderers[i].sortingOrder = 4;
                        CameraFollowView.Instance?.TriggerShake("hazard", 0.08f, 0.15f);
                    }
                }
                else if (_zones[i].State == ZoneState.ActiveHazard)
                {
                    // Active Hazard Phase: Magma burn & damage tick
                    float progress = 1f - Mathf.Clamp01(_zones[i].Timer / _zones[i].MaxTimer);
                    float flamePulse = 0.85f + 0.15f * Mathf.Sin(progress * 24f);
                    _renderers[i].color = new Color(1.0f, 1.0f, 1.0f, flamePulse);

                    _zones[i].TickTimer -= dt;
                    if (_zones[i].TickTimer <= 0f && !entity.IsDead)
                    {
                        _zones[i].TickTimer = DamageTickInterval;
                        float dist = (playerPos - _zones[i].Position).magnitude;
                        if (dist <= _zones[i].Radius + playerRadius)
                        {
                            float tickDmg = _zones[i].Damage * DamageTickInterval;
                            entity.TakeDamage(tickDmg);
                        }
                    }

                    if (_zones[i].Timer <= 0f)
                    {
                        // Transition to FadeOut
                        _zones[i].State = ZoneState.FadeOut;
                        _zones[i].Timer = FadeDuration;
                        _zones[i].MaxTimer = FadeDuration;
                    }
                }
                else if (_zones[i].State == ZoneState.FadeOut)
                {
                    // FadeOut Phase
                    float alpha = Mathf.Clamp01(_zones[i].Timer / FadeDuration);
                    _renderers[i].color = new Color(1f, 1f, 1f, alpha);

                    if (_zones[i].Timer <= 0f)
                    {
                        _zones[i].State = ZoneState.Inactive;
                        _gameObjects[i].SetActive(false);
                    }
                }
            }
        }
    }
}
