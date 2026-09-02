using UnityEngine;
using HappyShoot.Domain.Events;
using HappyShoot.View.Cameras;

namespace HappyShoot.View.Player
{
    /// <summary>
    /// Dedicated Juice component for Player hit feedback.
    /// Handles two-stage hit flash (White -> Vivid Crimson), punchy micro cam shake,
    /// responsive flinch squash, and 2.5D pixel impact sparks.
    /// Strictly modular and under 500 lines.
    /// </summary>
    public class PlayerHitFeedbackView : MonoBehaviour
    {
        private PlayerView _playerView;
        private EventBus _eventBus;
        private SpriteRenderer _bodySr;
        private SpriteRenderer _weaponSr;
        private Transform _bodyVisualTransform;

        private Color _bodyOrigColor = Color.white;
        private Color _weaponOrigColor = Color.white;

        // Flash timer states
        private float _flashTimer;
        private const float WhitePhaseDuration = 0.04f;
        private const float TotalFlashDuration = 0.14f;

        private static readonly Color WhiteFlashColor = Color.white;
        private static readonly Color CrimsonFlashColor = new Color(1.0f, 0.20f, 0.25f, 1.0f);

        // Flinch squash states
        private float _flinchTimer;
        private const float FlinchDuration = 0.10f;
        private Vector3 _originalBodyScale = Vector3.one * 1.5f;

        // Spark particles pooling
        private const int SparkPoolSize = 8;
        private readonly SparkParticle[] _sparks = new SparkParticle[SparkPoolSize];
        private static Sprite _cachedSparkSprite;

        private struct SparkParticle
        {
            public GameObject Go;
            public Transform Tf;
            public SpriteRenderer Sr;
            public Vector2 Velocity;
            public float Lifetime;
            public float MaxLifetime;
            public bool IsActive;
        }

        public void Initialize(PlayerView playerView, EventBus eventBus, SpriteRenderer bodySr, SpriteRenderer weaponSr, Transform bodyVisualTf)
        {
            _playerView = playerView;
            _eventBus = eventBus;
            _bodySr = bodySr;
            _weaponSr = weaponSr;
            _bodyVisualTransform = bodyVisualTf;

            if (_bodySr != null) _bodyOrigColor = _bodySr.color;
            if (_weaponSr != null) _weaponOrigColor = _weaponSr.color;
            if (_bodyVisualTransform != null) _originalBodyScale = _bodyVisualTransform.localScale;

            InitializeSparkPool();

            _eventBus?.Subscribe<PlayerDamagedEvent>(OnPlayerDamaged);
            _eventBus?.Subscribe<PlayerDiedEvent>(OnPlayerDied);
        }

        private void InitializeSparkPool()
        {
            var sparkContainer = new GameObject("HitSparkPool");
            sparkContainer.transform.SetParent(transform, false);

            var sparkSprite = GetOrCreateSparkSprite();

            for (int i = 0; i < SparkPoolSize; i++)
            {
                var go = new GameObject($"Spark_{i}");
                go.transform.SetParent(sparkContainer.transform, false);
                var sr = go.AddComponent<SpriteRenderer>();
                sr.sprite = sparkSprite;
                sr.sortingOrder = 35;
                go.SetActive(false);

                _sparks[i] = new SparkParticle
                {
                    Go = go,
                    Tf = go.transform,
                    Sr = sr,
                    IsActive = false
                };
            }
        }

        private void Update()
        {
            float dt = Time.deltaTime;

            // 1. Two-Stage Hit Flash
            if (_flashTimer > 0f)
            {
                _flashTimer -= dt;
                float elapsed = TotalFlashDuration - _flashTimer;

                Color activeColor;
                if (elapsed <= WhitePhaseDuration)
                {
                    activeColor = WhiteFlashColor;
                }
                else
                {
                    activeColor = CrimsonFlashColor;
                }

                if (_bodySr != null) _bodySr.color = activeColor;
                if (_weaponSr != null) _weaponSr.color = activeColor;

                if (_flashTimer <= 0f)
                {
                    if (_bodySr != null) _bodySr.color = _bodyOrigColor;
                    if (_weaponSr != null) _weaponSr.color = _weaponOrigColor;
                }
            }

            // 2. Flinch Squash Animation
            if (_flinchTimer > 0f && _bodyVisualTransform != null)
            {
                _flinchTimer -= dt;
                float progress = Mathf.Clamp01(1.0f - (_flinchTimer / FlinchDuration));
                // Momentary squash (X wider, Y shorter) then bounce back
                float squashCurve = Mathf.Sin(progress * Mathf.PI);
                float scaleX = _originalBodyScale.x * (1.0f + squashCurve * 0.15f);
                float scaleY = _originalBodyScale.y * (1.0f - squashCurve * 0.18f);
                _bodyVisualTransform.localScale = new Vector3(scaleX, scaleY, _originalBodyScale.z);

                if (_flinchTimer <= 0f)
                {
                    _bodyVisualTransform.localScale = _originalBodyScale;
                }
            }

            // 3. Update Spark Particles
            UpdateSparks(dt);
        }

        private void UpdateSparks(float dt)
        {
            for (int i = 0; i < SparkPoolSize; i++)
            {
                if (!_sparks[i].IsActive) continue;

                _sparks[i].Lifetime -= dt;
                if (_sparks[i].Lifetime <= 0f)
                {
                    _sparks[i].IsActive = false;
                    _sparks[i].Go.SetActive(false);
                    continue;
                }

                float progress = 1.0f - (_sparks[i].Lifetime / _sparks[i].MaxLifetime);
                _sparks[i].Tf.position += (Vector3)_sparks[i].Velocity * dt;
                _sparks[i].Velocity *= 0.88f; // Air friction deceleration

                // Scale down and fade out
                float scale = (1.0f - progress) * 0.45f;
                _sparks[i].Tf.localScale = new Vector3(scale, scale, 1.0f);
                if (_sparks[i].Sr != null)
                {
                    _sparks[i].Sr.color = Color.Lerp(Color.yellow, Color.red, progress);
                }
            }
        }

        private void OnPlayerDamaged(PlayerDamagedEvent evt)
        {
            // Trigger 2-Stage Flash
            _flashTimer = TotalFlashDuration;

            // Trigger Flinch Squash
            _flinchTimer = FlinchDuration;

            // Trigger Micro Camera Shake
            CameraFollowView.Instance?.TriggerShake("player_hit", duration: 0.12f, intensity: 0.22f);

            // Spawn 4-5 Impact Sparks
            SpawnHitSparks(Random.Range(4, 6));
        }

        private void SpawnHitSparks(int count)
        {
            Vector3 center = transform.position;
            int spawned = 0;

            for (int i = 0; i < SparkPoolSize && spawned < count; i++)
            {
                if (_sparks[i].IsActive) continue;

                float angle = Random.Range(0f, 360f) * Mathf.Deg2Rad;
                float speed = Random.Range(2.5f, 4.5f);
                Vector2 vel = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * speed;

                _sparks[i].IsActive = true;
                _sparks[i].Lifetime = 0.15f;
                _sparks[i].MaxLifetime = 0.15f;
                _sparks[i].Velocity = vel;
                _sparks[i].Tf.position = center + new Vector3(Mathf.Cos(angle) * 0.1f, Mathf.Sin(angle) * 0.1f, 0f);
                _sparks[i].Tf.localScale = Vector3.one * 0.45f;

                if (_sparks[i].Sr != null)
                {
                    _sparks[i].Sr.color = Color.white;
                }

                _sparks[i].Go.SetActive(true);
                spawned++;
            }
        }

        private void OnPlayerDied(PlayerDiedEvent evt)
        {
            _flashTimer = 0f;
            _flinchTimer = 0f;
            if (_bodySr != null) _bodySr.color = Color.gray;
            if (_weaponSr != null) _weaponSr.color = Color.gray;
        }

        private void OnDestroy()
        {
            _eventBus?.Unsubscribe<PlayerDamagedEvent>(OnPlayerDamaged);
            _eventBus?.Unsubscribe<PlayerDiedEvent>(OnPlayerDied);
        }

        private static Sprite GetOrCreateSparkSprite()
        {
            if (_cachedSparkSprite != null) return _cachedSparkSprite;

            const int size = 8;
            var tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            tex.filterMode = FilterMode.Point;

            Color[] pixels = new Color[size * size];
            for (int i = 0; i < pixels.Length; i++) pixels[i] = Color.clear;

            // Diamond/Star shaped 8x8 bright pixel spark
            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    int dist = Mathf.Abs(x - 3) + Mathf.Abs(y - 3);
                    if (dist <= 2)
                    {
                        pixels[y * size + x] = Color.white;
                    }
                }
            }

            tex.SetPixels(pixels);
            tex.Apply();

            _cachedSparkSprite = Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 16);
            return _cachedSparkSprite;
        }
    }
}
