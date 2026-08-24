using System.Collections.Generic;
using UnityEngine;
using HappyShoot.Domain.Events;
using HappyShoot.View.Cameras;
using HappyShoot.View.Utils;

namespace HappyShoot.View.Projectiles
{
    /// <summary>
    /// Presentation manager for Blood Eater (Evolved Greatsword) ultimate skill.
    /// Spawns crimson blood essence orbs from hit enemies traveling in curved paths towards the player for life-steal.
    /// Strictly modular and under 500 lines.
    /// </summary>
    public class BloodEaterManagerView : MonoBehaviour
    {
        private struct ActiveBloodParticle
        {
            public GameObject Root;
            public SpriteRenderer Renderer;
            public Vector2 Position;
            public Vector2 Velocity;
            public float Elapsed;
            public float Lifetime;
            public bool IsActive;
        }

        private struct ActiveLifeDrainOrb
        {
            public GameObject Root;
            public SpriteRenderer Renderer;
            public Vector2 StartPos;
            public Transform TargetTransform;
            public float Elapsed;
            public float Duration;
            public Vector2 CurveControl;
            public bool IsActive;
        }

        private readonly List<ActiveBloodParticle> _particlePool = new List<ActiveBloodParticle>(32);
        private readonly List<ActiveLifeDrainOrb> _orbPool = new List<ActiveLifeDrainOrb>(32);

        private EventBus _eventBus;
        private Transform _playerTransform;
        private Sprite _orbSprite;

        public void Initialize(EventBus eventBus, Transform playerTransform)
        {
            _eventBus = eventBus;
            _playerTransform = playerTransform;
            _orbSprite = SpriteHelper.GetOrCreateBloodOrbSprite();

            PrewarmPools();
            _eventBus?.Subscribe<BloodEaterExecutedEvent>(OnBloodEaterExecuted);
        }

        private void PrewarmPools()
        {
            // 1. Blood Particle & Absorption Flash Pool
            for (int i = 0; i < 32; i++)
            {
                var go = new GameObject($"BloodDebris_{i}");
                go.transform.SetParent(transform, false);
                var sr = go.AddComponent<SpriteRenderer>();
                sr.sprite = _orbSprite;
                sr.sortingOrder = 29;
                go.SetActive(false);

                _particlePool.Add(new ActiveBloodParticle { Root = go, Renderer = sr, IsActive = false });
            }

            // 2. Life Drain Blood Essence Orb Pool
            for (int i = 0; i < 32; i++)
            {
                var go = new GameObject($"BloodDrainOrb_{i}");
                go.transform.SetParent(transform, false);
                var sr = go.AddComponent<SpriteRenderer>();
                sr.sprite = _orbSprite;
                sr.sortingOrder = 30;
                go.SetActive(false);

                _orbPool.Add(new ActiveLifeDrainOrb { Root = go, Renderer = sr, IsActive = false });
            }
        }

        private void OnBloodEaterExecuted(BloodEaterExecutedEvent evt)
        {
            // 1. Camera Slash Shake
            CameraFollowView.Instance?.TriggerShake("blood_eater", duration: 0.18f, intensity: 0.24f);

            // 2. Spawn crimson blood essence orbs from each hit enemy position
            if (evt.HitPositions != null && evt.HitPositions.Count > 0 && _playerTransform != null)
            {
                int maxOrbsToSpawn = Mathf.Min(evt.HitPositions.Count * 2, 24);
                int spawnedOrbs = 0;

                for (int h = 0; h < evt.HitPositions.Count && spawnedOrbs < maxOrbsToSpawn; h++)
                {
                    Vector2 hitPos = new Vector2((float)evt.HitPositions[h].X, (float)evt.HitPositions[h].Y);

                    // Spawn 1~2 life drain orbs per hit enemy
                    int orbsFromThisEnemy = (h % 2 == 0) ? 2 : 1;
                    for (int o = 0; o < orbsFromThisEnemy && spawnedOrbs < maxOrbsToSpawn; o++)
                    {
                        for (int i = 0; i < _orbPool.Count; i++)
                        {
                            var orb = _orbPool[i];
                            if (!orb.IsActive)
                            {
                                orb.IsActive = true;
                                orb.StartPos = hitPos + Random.insideUnitCircle * 0.25f;
                                orb.TargetTransform = _playerTransform;
                                orb.Elapsed = 0f;
                                orb.Duration = Random.Range(0.28f, 0.42f);

                                // Random bezier control point for organic curved flight towards player
                                Vector2 toPlayer = (Vector2)_playerTransform.position - hitPos;
                                Vector2 perp = new Vector2(-toPlayer.y, toPlayer.x).normalized;
                                float curveOffset = Random.Range(-1.8f, 1.8f);
                                orb.CurveControl = hitPos + (toPlayer * 0.5f) + (perp * curveOffset);

                                orb.Root.transform.position = orb.StartPos;
                                orb.Root.transform.localScale = Vector3.one * Random.Range(0.9f, 1.35f);
                                orb.Renderer.color = new Color(1.0f, 0.15f, 0.25f, 1.0f);
                                orb.Root.SetActive(true);
                                _orbPool[i] = orb;

                                spawnedOrbs++;
                                break;
                            }
                        }
                    }

                    // Spawn a small blood splash spark at hit position
                    SpawnBloodSplatter(hitPos, count: 2);
                }
            }
        }

        private void SpawnBloodSplatter(Vector2 position, int count)
        {
            int spawned = 0;
            for (int i = 0; i < _particlePool.Count && spawned < count; i++)
            {
                var p = _particlePool[i];
                if (!p.IsActive)
                {
                    p.IsActive = true;
                    p.Position = position;
                    float angle = Random.Range(0f, 360f) * Mathf.Deg2Rad;
                    float speed = Random.Range(3.5f, 7.0f);
                    p.Velocity = new Vector2(Mathf.Cos(angle) * speed, Mathf.Sin(angle) * speed);
                    p.Elapsed = 0f;
                    p.Lifetime = Random.Range(0.18f, 0.30f);
                    p.Root.transform.position = position;
                    p.Root.transform.localScale = Vector3.one * Random.Range(0.5f, 0.9f);
                    p.Renderer.color = new Color(0.95f, 0.10f, 0.20f, 1.0f);
                    p.Root.SetActive(true);
                    _particlePool[i] = p;
                    spawned++;
                }
            }
        }

        private void SpawnAbsorptionBurst(Vector2 position)
        {
            int spawned = 0;
            for (int i = 0; i < _particlePool.Count && spawned < 2; i++)
            {
                var p = _particlePool[i];
                if (!p.IsActive)
                {
                    p.IsActive = true;
                    p.Position = position;
                    float angle = Random.Range(0f, 360f) * Mathf.Deg2Rad;
                    float speed = Random.Range(1.5f, 3.5f);
                    p.Velocity = new Vector2(Mathf.Cos(angle) * speed, Mathf.Sin(angle) * speed);
                    p.Elapsed = 0f;
                    p.Lifetime = Random.Range(0.15f, 0.25f);
                    p.Root.transform.position = position;
                    p.Root.transform.localScale = Vector3.one * 0.7f;
                    p.Renderer.color = new Color(1.0f, 0.45f, 0.60f, 1.0f); // Bright pink-ruby absorption sparkle
                    p.Root.SetActive(true);
                    _particlePool[i] = p;
                    spawned++;
                }
            }
        }

        private void Update()
        {
            float dt = Time.deltaTime;

            // 1. Update Blood Particles
            for (int i = 0; i < _particlePool.Count; i++)
            {
                var p = _particlePool[i];
                if (!p.IsActive) continue;

                p.Elapsed += dt;
                if (p.Elapsed >= p.Lifetime)
                {
                    p.IsActive = false;
                    p.Root.SetActive(false);
                    _particlePool[i] = p;
                    continue;
                }

                p.Position += p.Velocity * dt;
                p.Velocity *= Mathf.Pow(0.12f, dt); // Strong friction drag
                p.Root.transform.position = p.Position;

                float alpha = Mathf.Clamp01(1.0f - (p.Elapsed / p.Lifetime));
                Color c = p.Renderer.color;
                c.a = alpha;
                p.Renderer.color = c;
                _particlePool[i] = p;
            }

            // 2. Update Life Drain Absorption Orbs
            for (int i = 0; i < _orbPool.Count; i++)
            {
                var o = _orbPool[i];
                if (!o.IsActive) continue;

                o.Elapsed += dt;
                float progress = Mathf.Clamp01(o.Elapsed / o.Duration);

                Vector2 targetPos = (o.TargetTransform != null) 
                    ? (Vector2)o.TargetTransform.position 
                    : o.CurveControl;

                // Quadratic Bezier Curve: B(t) = (1-t)^2 * P0 + 2(1-t)t * P1 + t^2 * P2
                float u = 1f - progress;
                Vector2 currentPos = (u * u * o.StartPos) + (2f * u * progress * o.CurveControl) + (progress * progress * targetPos);

                o.Root.transform.position = currentPos;

                // Scale pulse and energetic glow
                float scaleMod = Mathf.Sin(progress * Mathf.PI) * 0.4f + 0.9f;
                o.Root.transform.localScale = Vector3.one * scaleMod;

                // Siphon into player upon arrival
                if (progress >= 1.0f || (o.TargetTransform != null && Vector2.Distance(currentPos, targetPos) < 0.35f))
                {
                    o.IsActive = false;
                    o.Root.SetActive(false);
                    SpawnAbsorptionBurst(targetPos);
                    _orbPool[i] = o;
                    continue;
                }

                _orbPool[i] = o;
            }
        }
    }
}
