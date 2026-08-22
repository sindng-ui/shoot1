using System.Collections.Generic;
using UnityEngine;
using HappyShoot.Domain.Events;
using HappyShoot.View.Cameras;
using HappyShoot.View.Utils;

namespace HappyShoot.View.Projectiles
{
    /// <summary>
    /// Presentation manager for Blood Eater (Evolved Greatsword) ultimate skill.
    /// Manages 360-degree crimson vortex ring expansions, blood splatter particles,
    /// life-steal absorption orbs traveling towards the player, and camera shakes.
    /// Strictly modular and under 500 lines.
    /// </summary>
    public class BloodEaterManagerView : MonoBehaviour
    {
        private struct ActiveBloodRing
        {
            public GameObject Root;
            public SpriteRenderer Renderer;
            public Vector2 Center;
            public float TargetRadius;
            public float Elapsed;
            public float Duration;
            public float InitialRotation;
            public bool IsActive;
        }

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

        private readonly List<ActiveBloodRing> _ringPool = new List<ActiveBloodRing>(6);
        private readonly List<ActiveBloodParticle> _particlePool = new List<ActiveBloodParticle>(32);
        private readonly List<ActiveLifeDrainOrb> _orbPool = new List<ActiveLifeDrainOrb>(16);

        private EventBus _eventBus;
        private Transform _playerTransform;
        private Sprite _ringSprite;
        private Sprite _orbSprite;

        public void Initialize(EventBus eventBus, Transform playerTransform)
        {
            _eventBus = eventBus;
            _playerTransform = playerTransform;
            _ringSprite = SkillSpriteHelper.GetOrCreateBloodSpinSprite();
            _orbSprite = SkillSpriteHelper.GetOrCreateBloodOrbSprite();

            PrewarmPools();
            _eventBus?.Subscribe<BloodEaterExecutedEvent>(OnBloodEaterExecuted);
        }

        private void PrewarmPools()
        {
            // 1. Ring Pool
            for (int i = 0; i < 6; i++)
            {
                var go = new GameObject($"BloodRing_{i}");
                go.transform.SetParent(transform, false);
                var sr = go.AddComponent<SpriteRenderer>();
                sr.sprite = _ringSprite;
                sr.sortingOrder = 28;
                go.SetActive(false);

                _ringPool.Add(new ActiveBloodRing { Root = go, Renderer = sr, IsActive = false });
            }

            // 2. Particle Pool
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

            // 3. Life Drain Orb Pool
            for (int i = 0; i < 16; i++)
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
            Vector2 center = new Vector2((float)evt.CenterPosition.X, (float)evt.CenterPosition.Y);
            float radius = evt.Radius > 0f ? evt.Radius : 4.8f;

            // 1. Trigger Screen Shake (filtered by slash shake setting)
            CameraFollowView.Instance?.TriggerShake("slash", duration: 0.18f, intensity: 0.22f);

            // 2. Spawn Expanding Crimson Vortex Ring
            for (int i = 0; i < _ringPool.Count; i++)
            {
                var r = _ringPool[i];
                if (!r.IsActive)
                {
                    r.IsActive = true;
                    r.Center = center;
                    r.TargetRadius = radius;
                    r.Elapsed = 0f;
                    r.Duration = 0.38f;
                    r.InitialRotation = Random.Range(0f, 360f);
                    r.Root.transform.position = center;
                    r.Root.transform.localScale = Vector3.zero;
                    r.Root.SetActive(true);
                    _ringPool[i] = r;
                    break;
                }
            }

            // 3. Spawn 12 Blood Splatter Particles in 360 degrees
            int spawnedParticles = 0;
            for (int i = 0; i < _particlePool.Count && spawnedParticles < 12; i++)
            {
                var p = _particlePool[i];
                if (!p.IsActive)
                {
                    p.IsActive = true;
                    p.Position = center;
                    float angle = (spawnedParticles * 30f + Random.Range(-10f, 10f)) * Mathf.Deg2Rad;
                    float speed = Random.Range(5.0f, 11.0f);
                    p.Velocity = new Vector2(Mathf.Cos(angle) * speed, Mathf.Sin(angle) * speed);
                    p.Elapsed = 0f;
                    p.Lifetime = Random.Range(0.25f, 0.45f);
                    p.Root.transform.position = center;
                    p.Root.transform.localScale = Vector3.one * Random.Range(0.6f, 1.2f);
                    p.Renderer.color = new Color(0.9f, 0.1f, 0.15f, 1.0f);
                    p.Root.SetActive(true);
                    _particlePool[i] = p;
                    spawnedParticles++;
                }
            }

            // 4. If Healed > 0, spawn absorption orbs flying towards player
            if (evt.HealedAmount > 0f && _playerTransform != null)
            {
                int orbCount = Mathf.Clamp(Mathf.RoundToInt(evt.HealedAmount / 2.0f), 2, 6);
                int spawnedOrbs = 0;
                for (int i = 0; i < _orbPool.Count && spawnedOrbs < orbCount; i++)
                {
                    var o = _orbPool[i];
                    if (!o.IsActive)
                    {
                        o.IsActive = true;
                        float randAngle = Random.Range(0f, 360f) * Mathf.Deg2Rad;
                        float spawnDist = Random.Range(1.8f, radius * 0.85f);
                        o.StartPos = center + new Vector2(Mathf.Cos(randAngle) * spawnDist, Mathf.Sin(randAngle) * spawnDist);
                        o.TargetTransform = _playerTransform;
                        o.Elapsed = 0f;
                        o.Duration = Random.Range(0.35f, 0.55f);

                        // Curve offset for curved homing suction effect
                        Vector2 perp = new Vector2(-Mathf.Sin(randAngle), Mathf.Cos(randAngle)) * Random.Range(-1.5f, 1.5f);
                        o.CurveControl = (o.StartPos + (Vector2)_playerTransform.position) * 0.5f + perp;

                        o.Root.transform.position = o.StartPos;
                        o.Root.transform.localScale = Vector3.one * 1.1f;
                        o.Renderer.color = new Color(1.0f, 0.3f, 0.35f, 1.0f);
                        o.Root.SetActive(true);
                        _orbPool[i] = o;
                        spawnedOrbs++;
                    }
                }
            }
        }

        private void Update()
        {
            float dt = Time.deltaTime;

            // 1. Update Expanding Vortex Rings
            for (int i = 0; i < _ringPool.Count; i++)
            {
                var r = _ringPool[i];
                if (r.IsActive)
                {
                    r.Elapsed += dt;
                    float progress = Mathf.Clamp01(r.Elapsed / r.Duration);

                    // Rapid burst expansion
                    float easeScale = Mathf.Sin(progress * Mathf.PI * 0.5f);
                    float diameter = r.TargetRadius * 2.0f * easeScale;
                    r.Root.transform.localScale = new Vector3(diameter, diameter, 1f);

                    // Fast spin rotation
                    float currentRot = r.InitialRotation + progress * 360f;
                    r.Root.transform.rotation = Quaternion.Euler(0, 0, currentRot);

                    // Alpha fade out near end
                    float alpha = 1.0f - (progress * progress);
                    r.Renderer.color = new Color(1.0f, 0.15f, 0.2f, alpha * 0.95f);

                    if (progress >= 1.0f)
                    {
                        r.IsActive = false;
                        r.Root.SetActive(false);
                    }
                    _ringPool[i] = r;
                }
            }

            // 2. Update Blood Splatter Particles
            for (int i = 0; i < _particlePool.Count; i++)
            {
                var p = _particlePool[i];
                if (p.IsActive)
                {
                    p.Elapsed += dt;
                    float progress = Mathf.Clamp01(p.Elapsed / p.Lifetime);

                    p.Position += p.Velocity * dt;
                    p.Velocity = Vector2.Lerp(p.Velocity, Vector2.zero, dt * 6.0f); // Air resistance
                    p.Root.transform.position = p.Position;

                    float alpha = 1.0f - progress;
                    p.Renderer.color = new Color(0.9f, 0.1f, 0.15f, alpha);

                    if (progress >= 1.0f)
                    {
                        p.IsActive = false;
                        p.Root.SetActive(false);
                    }
                    _particlePool[i] = p;
                }
            }

            // 3. Update Life Drain Absorption Orbs
            for (int i = 0; i < _orbPool.Count; i++)
            {
                var o = _orbPool[i];
                if (o.IsActive)
                {
                    o.Elapsed += dt;
                    float t = Mathf.Clamp01(o.Elapsed / o.Duration);

                    Vector2 target = o.TargetTransform != null ? (Vector2)o.TargetTransform.position : o.StartPos;

                    // Quadratic Bezier curve suction
                    float u = 1f - t;
                    Vector2 currentPos = (u * u * o.StartPos) + (2f * u * t * o.CurveControl) + (t * t * target);
                    o.Root.transform.position = currentPos;

                    // Pulsing scale shrink into player
                    o.Root.transform.localScale = Vector3.one * Mathf.Lerp(1.2f, 0.4f, t);

                    if (t >= 1.0f)
                    {
                        o.IsActive = false;
                        o.Root.SetActive(false);
                    }
                    _orbPool[i] = o;
                }
            }
        }
    }
}
