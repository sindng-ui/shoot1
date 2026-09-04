using System;
using UnityEngine;
using HappyShoot.Domain.Entities;
using HappyShoot.View.Cameras;
using HappyShoot.View.Player;

namespace HappyShoot.View.SideScroll
{
    /// <summary>
    /// Massive Dimensional Void Core Boss appearing at 300m mark in side-scrolling corridor.
    /// Strictly modular and under 500 lines.
    /// </summary>
    public class DimensionalVoidCoreView : MonoBehaviour
    {
        private PlayerView _playerView;
        private MonsterEntity _bossEntity;
        private Action _onDefeated;
        private float _maxHp = 3000f;
        private float _currentHp;
        private float _pulseTimer;
        private float _flashTimer;
        private bool _isDead;

        private SpriteRenderer _sr;
        private GameObject _hpBarBg;
        private GameObject _hpBarFill;
        private static Sprite _voidCoreSprite;

        public void Initialize(PlayerView playerView, MonsterEntity bossEntity, Action onDefeated)
        {
            _playerView = playerView;
            _bossEntity = bossEntity;
            _onDefeated = onDefeated;

            _maxHp = bossEntity != null ? bossEntity.MaxHealth : 3000f;
            _currentHp = _maxHp;
            _isDead = false;

            _sr = gameObject.GetComponent<SpriteRenderer>();
            if (_sr == null) _sr = gameObject.AddComponent<SpriteRenderer>();
            _sr.sprite = GetOrCreateVoidCoreSprite();
            _sr.sortingOrder = 15;
            transform.localScale = Vector3.one * 3.2f;

            BuildHealthBar();
        }

        private void BuildHealthBar()
        {
            _hpBarBg = new GameObject("BossHpBarBg");
            _hpBarBg.transform.SetParent(transform, false);
            _hpBarBg.transform.localPosition = new Vector3(0f, 1.8f, 0f);
            var bgSr = _hpBarBg.AddComponent<SpriteRenderer>();
            bgSr.sprite = Utils.SpriteHelper.GetOrCreateWhiteSprite();
            bgSr.color = new Color(0.1f, 0.1f, 0.15f, 0.85f);
            bgSr.sortingOrder = 25;
            _hpBarBg.transform.localScale = new Vector3(3.2f, 0.35f, 1f);

            _hpBarFill = new GameObject("BossHpBarFill");
            _hpBarFill.transform.SetParent(_hpBarBg.transform, false);
            _hpBarFill.transform.localPosition = new Vector3(-0.5f, 0f, 0f);
            var fillSr = _hpBarFill.AddComponent<SpriteRenderer>();
            fillSr.sprite = Utils.SpriteHelper.GetOrCreateWhiteSprite();
            fillSr.color = new Color(0.85f, 0.15f, 0.95f, 1.0f); // Radiant Void Purple
            fillSr.sortingOrder = 26;
            _hpBarFill.transform.localScale = new Vector3(1f, 0.8f, 1f);
        }

        private void Update()
        {
            if (_isDead) return;

            // 1. Check Domain MonsterEntity health & death
            if (_bossEntity != null)
            {
                _currentHp = _bossEntity.CurrentHealth;
                if (_bossEntity.IsDead || _currentHp <= 0f)
                {
                    _isDead = true;
                    Die();
                    return;
                }
            }

            // 2. Pulse cosmic dark light (Stationary boss arena, never runs away!)
            _pulseTimer += Time.deltaTime * 3.5f;
            float s = 3.2f + Mathf.Sin(_pulseTimer) * 0.25f;
            transform.localScale = new Vector3(s, s, 1f);

            // 3. Hit flash feedback
            if (_flashTimer > 0f)
            {
                _flashTimer -= Time.deltaTime;
                if (_flashTimer <= 0f && _sr != null)
                {
                    _sr.color = Color.white;
                }
            }

            // 4. Update Health Bar
            if (_hpBarFill != null && _maxHp > 0f)
            {
                float ratio = Mathf.Clamp01(_currentHp / _maxHp);
                _hpBarFill.transform.localScale = new Vector3(ratio, 0.8f, 1f);
                _hpBarFill.transform.localPosition = new Vector3(-0.5f + ratio * 0.5f, 0f, 0f);
            }

            // 5. Proximity damage from player close combat
            if (_playerView != null)
            {
                float dist = Vector2.Distance(transform.position, _playerView.transform.position);
                if (dist <= 3.8f)
                {
                    TakeDamage(350f * Time.deltaTime);
                }
            }
        }

        public void TakeDamage(float amount)
        {
            if (_isDead) return;

            if (_bossEntity != null)
            {
                _bossEntity.TakeDamage(amount);
                _currentHp = _bossEntity.CurrentHealth;
            }
            else
            {
                _currentHp -= amount;
            }

            if (_sr != null)
            {
                _sr.color = new Color(1f, 0.5f, 0.7f, 1f);
                _flashTimer = 0.08f;
            }

            if (_currentHp <= 0f)
            {
                _isDead = true;
                Die();
            }
        }

        private void Die()
        {
            _playerView?.EventBus?.Publish(new Domain.Events.PlaySoundEvent(Domain.Events.SoundEffectType.BossSpawn, 1.3f));
            CameraFollowView.Instance?.TriggerShake(null, 0.8f, 0.6f);

            // Massive gold & gem explosion
            for (int i = 0; i < 15; i++)
            {
                var gemDropGo = new GameObject("BossGemReward");
                gemDropGo.transform.position = transform.position + new Vector3(UnityEngine.Random.Range(-2f, 2f), UnityEngine.Random.Range(-1f, 2f), 0f);
                var comp = gemDropGo.AddComponent<FallingGemShowerView>();
                comp.Initialize(_playerView);
            }

            _onDefeated?.Invoke();
            Destroy(gameObject, 0.35f);
        }

        public static Sprite GetOrCreateVoidCoreSprite(int size = 64)
        {
            if (_voidCoreSprite != null) return _voidCoreSprite;
            var tex = new Texture2D(size, size, TextureFormat.RGBA32, false) { filterMode = FilterMode.Point };
            var pixels = new Color[size * size];
            float c = size * 0.5f;

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float dist = Vector2.Distance(new Vector2(x, y), new Vector2(c, c));
                    if (dist <= 24f)
                    {
                        if (dist <= 12f)
                            pixels[y * size + x] = new Color(0.9f, 0.2f, 1.0f, 0.98f);
                        else
                            pixels[y * size + x] = new Color(0.2f, 0.05f, 0.45f, 0.90f);
                    }
                    else
                    {
                        pixels[y * size + x] = Color.clear;
                    }
                }
            }
            tex.SetPixels(pixels);
            tex.Apply();
            _voidCoreSprite = Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), size);
            return _voidCoreSprite;
        }
    }

    /// <summary>
    /// Falling gold coin shower in side-scrolling corridor.
    /// Drops radiant gold coins that award actual gold without any level-up interruption!
    /// </summary>
    public class FallingGemShowerView : MonoBehaviour
    {
        private PlayerView _playerView;
        private float _fallSpeed = 4.5f;

        public void Initialize(PlayerView playerView)
        {
            _playerView = playerView;
            var sr = gameObject.AddComponent<SpriteRenderer>();
            var coinSprite = Utils.CustomResourceSpriteLoader.TryGetGoldCoinSprite();
            if (coinSprite != null)
            {
                sr.sprite = coinSprite;
                transform.localScale = Vector3.one * 0.22f;
            }
            else
            {
                sr.sprite = Utils.SpriteHelper.GetOrCreateWhiteSprite();
                sr.color = new Color(1f, 0.85f, 0.2f, 0.95f);
                transform.localScale = Vector3.one * 0.35f;
            }
            sr.sortingOrder = 14;
        }

        private void Update()
        {
            transform.position += Vector3.down * (_fallSpeed * Time.deltaTime);

            if (_playerView != null && Vector2.Distance(transform.position, _playerView.transform.position) <= 1.2f)
            {
                SideScrollModeController.Instance?.GameSession?.AddGold(15);
                _playerView.EventBus?.Publish(new Domain.Events.PlaySoundEvent(Domain.Events.SoundEffectType.GemCollect, 0.9f));
                Destroy(gameObject);
            }
            else if (transform.position.y < -3.5f)
            {
                Destroy(gameObject);
            }
        }
    }
}
