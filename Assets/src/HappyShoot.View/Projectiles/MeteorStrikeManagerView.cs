using System.Collections.Generic;
using UnityEngine;
using HappyShoot.Domain.Events;
using HappyShoot.Domain.Spatial;
using HappyShoot.View.Cameras;
using HappyShoot.View.Utils;

namespace HappyShoot.View.Projectiles
{
    /// <summary>
    /// Manages the massive visual presentation for Meteor Strike (Evolved Fireball):
    /// High-speed sky drop, colossal fiery ground explosion, shockwaves, debris, scorch marks, and camera shake.
    /// Strictly modular and under 500 lines.
    /// </summary>
    public class MeteorStrikeManagerView : MonoBehaviour
    {
        private struct ActiveMeteor
        {
            public GameObject Root;
            public SpriteRenderer Renderer;
            public Vector2 StartPos;
            public Vector2 TargetPos;
            public float Elapsed;
            public float Duration;
            public float Radius;
            public bool IsActive;
        }

        private struct ActiveBlast
        {
            public GameObject Root;
            public SpriteRenderer CoreRenderer;
            public SpriteRenderer RingRenderer;
            public Vector2 Center;
            public float Elapsed;
            public float Duration;
            public float TargetScale;
            public bool IsActive;
        }

        private struct MeteorDebris
        {
            public GameObject Root;
            public SpriteRenderer Renderer;
            public Vector2 Position;
            public Vector2 Velocity;
            public float Elapsed;
            public float Lifetime;
            public bool IsActive;
        }

        private readonly List<ActiveMeteor> _meteorPool = new List<ActiveMeteor>(8);
        private readonly List<ActiveBlast> _blastPool = new List<ActiveBlast>(8);
        private readonly List<MeteorDebris> _debrisPool = new List<MeteorDebris>(32);

        private EventBus _eventBus;
        private Sprite _meteorSprite;
        private Sprite _flameSprite;
        private Sprite _ringSprite;

        public void Initialize(EventBus eventBus)
        {
            _eventBus = eventBus;
            _meteorSprite = WizardSpriteHelper.GetOrCreateMeteorSprite(48);
            _flameSprite = WizardSpriteHelper.GetOrCreateFireballSprite(32);
            _ringSprite = SkillSpriteHelper.GetOrCreateGroundStompSprite();

            PrewarmPools();
            _eventBus?.Subscribe<MeteorStrikeExecutedEvent>(OnMeteorStrikeExecuted);
        }

        private void PrewarmPools()
        {
            // 1. Meteor Drop Pool
            for (int i = 0; i < 6; i++)
            {
                var go = new GameObject($"MeteorDrop_{i}");
                go.transform.SetParent(transform, false);
                var sr = go.AddComponent<SpriteRenderer>();
                sr.sprite = _meteorSprite;
                sr.sortingOrder = 35;
                go.SetActive(false);

                _meteorPool.Add(new ActiveMeteor { Root = go, Renderer = sr, IsActive = false });
            }

            // 2. Blast Pool
            for (int i = 0; i < 6; i++)
            {
                var go = new GameObject($"MeteorBlast_{i}");
                go.transform.SetParent(transform, false);

                var core = new GameObject("Core");
                core.transform.SetParent(go.transform, false);
                var coreSr = core.AddComponent<SpriteRenderer>();
                coreSr.sprite = _flameSprite;
                coreSr.sortingOrder = 36;

                var ring = new GameObject("Ring");
                ring.transform.SetParent(go.transform, false);
                var ringSr = ring.AddComponent<SpriteRenderer>();
                ringSr.sprite = _ringSprite;
                ringSr.sortingOrder = 34;

                go.SetActive(false);
                _blastPool.Add(new ActiveBlast { Root = go, CoreRenderer = coreSr, RingRenderer = ringSr, IsActive = false });
            }

            // 3. Debris Pool
            for (int i = 0; i < 24; i++)
            {
                var go = new GameObject($"MeteorDebris_{i}");
                go.transform.SetParent(transform, false);
                var sr = go.AddComponent<SpriteRenderer>();
                sr.sprite = _flameSprite;
                sr.sortingOrder = 37;
                go.SetActive(false);

                _debrisPool.Add(new MeteorDebris { Root = go, Renderer = sr, IsActive = false });
            }
        }

        private void OnMeteorStrikeExecuted(MeteorStrikeExecutedEvent e)
        {
            Vector2 target = new Vector2((float)e.TargetPosition.X, (float)e.TargetPosition.Y);
            Vector2 skyOrigin = target + new Vector2(-4.0f, 10.0f);

            // Spawn dropping meteor
            for (int i = 0; i < _meteorPool.Count; i++)
            {
                var m = _meteorPool[i];
                if (!m.IsActive)
                {
                    m.IsActive = true;
                    m.StartPos = skyOrigin;
                    m.TargetPos = target;
                    m.Elapsed = 0f;
                    m.Duration = 0.38f;
                    m.Radius = Mathf.Max(3.5f, e.Radius);
                    m.Root.transform.position = skyOrigin;
                    m.Root.transform.localScale = Vector3.one * 1.8f;
                    m.Root.SetActive(true);
                    _meteorPool[i] = m;
                    break;
                }
            }
        }

        private void TriggerGroundExplosion(Vector2 center, float radius)
        {
            // Camera Shake
            CameraFollowView.Instance?.TriggerShake(0.35f, 0.45f);

            // Spawn Blast
            for (int i = 0; i < _blastPool.Count; i++)
            {
                var b = _blastPool[i];
                if (!b.IsActive)
                {
                    b.IsActive = true;
                    b.Center = center;
                    b.Elapsed = 0f;
                    b.Duration = 0.65f;
                    b.TargetScale = radius * 2.2f;
                    b.Root.transform.position = center;
                    b.Root.SetActive(true);
                    _blastPool[i] = b;
                    break;
                }
            }

            // Spawn 10 fiery debris particles
            int spawnedDebris = 0;
            for (int i = 0; i < _debrisPool.Count && spawnedDebris < 10; i++)
            {
                var d = _debrisPool[i];
                if (!d.IsActive)
                {
                    d.IsActive = true;
                    d.Position = center;
                    float angle = Random.Range(0f, 360f) * Mathf.Deg2Rad;
                    float speed = Random.Range(6.0f, 14.0f);
                    d.Velocity = new Vector2(Mathf.Cos(angle) * speed, Mathf.Sin(angle) * speed);
                    d.Elapsed = 0f;
                    d.Lifetime = Random.Range(0.4f, 0.7f);
                    d.Root.transform.position = center;
                    d.Root.transform.localScale = Vector3.one * Random.Range(0.4f, 0.8f);
                    d.Renderer.color = new Color(1.0f, 0.7f, 0.2f, 1.0f);
                    d.Root.SetActive(true);
                    _debrisPool[i] = d;
                    spawnedDebris++;
                }
            }
        }

        private void Update()
        {
            float dt = Time.deltaTime;

            // 1. Update Dropping Meteors
            for (int i = 0; i < _meteorPool.Count; i++)
            {
                var m = _meteorPool[i];
                if (m.IsActive)
                {
                    m.Elapsed += dt;
                    float t = Mathf.Clamp01(m.Elapsed / m.Duration);
                    float easeT = t * t; // Acceleration downwards

                    Vector2 currentPos = Vector2.Lerp(m.StartPos, m.TargetPos, easeT);
                    m.Root.transform.position = currentPos;

                    // Rotate towards trajectory
                    Vector2 dir = m.TargetPos - m.StartPos;
                    float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
                    m.Root.transform.rotation = Quaternion.Euler(0, 0, angle - 90f);

                    if (t >= 1.0f)
                    {
                        m.IsActive = false;
                        m.Root.SetActive(false);
                        TriggerGroundExplosion(m.TargetPos, m.Radius);
                    }
                    _meteorPool[i] = m;
                }
            }

            // 2. Update Blasts
            for (int i = 0; i < _blastPool.Count; i++)
            {
                var b = _blastPool[i];
                if (b.IsActive)
                {
                    b.Elapsed += dt;
                    float progress = Mathf.Clamp01(b.Elapsed / b.Duration);

                    // Core expand & fade
                    float scale = Mathf.Lerp(0.5f, b.TargetScale, Mathf.Sqrt(progress));
                    b.Root.transform.localScale = Vector3.one * scale;

                    float alpha = 1.0f - progress;
                    b.CoreRenderer.color = new Color(1.0f, 0.85f * (1f - progress), 0.2f, alpha * 0.9f);
                    b.RingRenderer.color = new Color(1.0f, 0.4f, 0.1f, alpha * 0.8f);

                    if (progress >= 1.0f)
                    {
                        b.IsActive = false;
                        b.Root.SetActive(false);
                    }
                    _blastPool[i] = b;
                }
            }

            // 3. Update Debris
            for (int i = 0; i < _debrisPool.Count; i++)
            {
                var d = _debrisPool[i];
                if (d.IsActive)
                {
                    d.Elapsed += dt;
                    d.Position += d.Velocity * dt;
                    d.Velocity *= Mathf.Pow(0.1f, dt); // Air drag deceleration
                    d.Root.transform.position = d.Position;

                    float t = d.Elapsed / d.Lifetime;
                    float alpha = 1.0f - t;
                    d.Renderer.color = new Color(1.0f, 0.6f * (1f - t), 0.1f, alpha);

                    if (t >= 1.0f)
                    {
                        d.IsActive = false;
                        d.Root.SetActive(false);
                    }
                    _debrisPool[i] = d;
                }
            }
        }

        private void OnDestroy()
        {
            _eventBus?.Unsubscribe<MeteorStrikeExecutedEvent>(OnMeteorStrikeExecuted);
        }
    }
}
