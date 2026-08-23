using System.Collections.Generic;
using UnityEngine;
using HappyShoot.Domain.Events;
using HappyShoot.View.Cameras;
using HappyShoot.View.Utils;

namespace HappyShoot.View.Projectiles
{
    /// <summary>
    /// Presentation manager for Meteor Strike (Evolved Fireball):
    /// Crisp circular AOE target indicator with countdown ring, high-speed falling flaming meteor,
    /// compact & punchy explosion matched 1:1 to damage radius, and comfortable camera shake.
    /// Strictly modular and under 500 lines.
    /// </summary>
    public class MeteorStrikeManagerView : MonoBehaviour
    {
        private struct ActiveIndicator
        {
            public GameObject Root;
            public SpriteRenderer DecalRenderer;
            public SpriteRenderer PulseRenderer;
            public Vector2 Position;
            public float Radius;
            public float Elapsed;
            public float Duration;
            public bool IsActive;
        }

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

        private readonly List<ActiveIndicator> _indicatorPool = new List<ActiveIndicator>(6);
        private readonly List<ActiveMeteor> _meteorPool = new List<ActiveMeteor>(6);
        private readonly List<ActiveBlast> _blastPool = new List<ActiveBlast>(6);
        private readonly List<MeteorDebris> _debrisPool = new List<MeteorDebris>(24);

        private EventBus _eventBus;
        private Sprite _indicatorSprite;
        private Sprite _meteorSprite;
        private Sprite _flameSprite;
        private Sprite _ringSprite;

        public void Initialize(EventBus eventBus)
        {
            _eventBus = eventBus;
            _indicatorSprite = WizardSpriteHelper.GetOrCreateTargetIndicatorSprite(128);
            _meteorSprite = WizardSpriteHelper.GetOrCreateMeteorSprite(48);
            _flameSprite = WizardSpriteHelper.GetOrCreateFireballSprite(32);
            _ringSprite = SkillSpriteHelper.GetOrCreateGroundStompSprite();

            PrewarmPools();
            _eventBus?.Subscribe<MeteorStrikeExecutedEvent>(OnMeteorStrikeExecuted);
        }

        private void PrewarmPools()
        {
            // 1. Target Indicator Decal Pool
            for (int i = 0; i < 6; i++)
            {
                var go = new GameObject($"MeteorIndicator_{i}");
                go.transform.SetParent(transform, false);

                var decal = new GameObject("Decal");
                decal.transform.SetParent(go.transform, false);
                var decalSr = decal.AddComponent<SpriteRenderer>();
                decalSr.sprite = _indicatorSprite;
                decalSr.sortingOrder = 5; // On ground

                var pulse = new GameObject("PulseRing");
                pulse.transform.SetParent(go.transform, false);
                var pulseSr = pulse.AddComponent<SpriteRenderer>();
                pulseSr.sprite = _indicatorSprite;
                pulseSr.sortingOrder = 6;

                go.SetActive(false);
                _indicatorPool.Add(new ActiveIndicator { Root = go, DecalRenderer = decalSr, PulseRenderer = pulseSr, IsActive = false });
            }

            // 2. Meteor Drop Pool
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

            // 3. Blast Pool
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

            // 4. Debris Pool
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
            Vector2 skyOrigin = target + new Vector2(-3.5f, 9.0f);
            float radius = e.Radius > 0f ? e.Radius : 3.0f;
            float dropDuration = 0.38f;

            // 1. Spawn Ground Target Indicator (Decal + Convergence Pulse)
            for (int i = 0; i < _indicatorPool.Count; i++)
            {
                var ind = _indicatorPool[i];
                if (!ind.IsActive)
                {
                    ind.IsActive = true;
                    ind.Position = target;
                    ind.Radius = radius;
                    ind.Elapsed = 0f;
                    ind.Duration = dropDuration;

                    ind.Root.transform.position = target;
                    // Decal fixed at exact diameter (radius * 2.0f)
                    ind.DecalRenderer.transform.localScale = Vector3.one * (radius * 2.0f);
                    ind.DecalRenderer.color = new Color(0.95f, 0.40f, 0.1f, 0.45f);

                    // Pulse ring starts at double size and converges into center
                    ind.PulseRenderer.transform.localScale = Vector3.one * (radius * 2.4f);
                    ind.PulseRenderer.color = new Color(1.0f, 0.65f, 0.15f, 0.35f);

                    ind.Root.SetActive(true);
                    _indicatorPool[i] = ind;
                    break;
                }
            }

            // 2. Spawn Dropping Meteor (Compact & Fast)
            for (int i = 0; i < _meteorPool.Count; i++)
            {
                var m = _meteorPool[i];
                if (!m.IsActive)
                {
                    m.IsActive = true;
                    m.StartPos = skyOrigin;
                    m.TargetPos = target;
                    m.Elapsed = 0f;
                    m.Duration = dropDuration;
                    m.Radius = radius;
                    m.Root.transform.position = skyOrigin;
                    m.Root.transform.localScale = Vector3.one * 1.15f; // Sleek and compact
                    m.Root.SetActive(true);
                    _meteorPool[i] = m;
                    break;
                }
            }
        }

        private void TriggerGroundExplosion(Vector2 center, float radius)
        {
            // 1. Micro Camera Shake (Comfortable & Crisp, filtered by fireball shake setting)
            CameraFollowView.Instance?.TriggerShake("fireball", duration: 0.14f, intensity: 0.16f);

            // 2. Spawn Blast matching exact damage radius
            for (int i = 0; i < _blastPool.Count; i++)
            {
                var b = _blastPool[i];
                if (!b.IsActive)
                {
                    b.IsActive = true;
                    b.Center = center;
                    b.Elapsed = 0f;
                    b.Duration = 0.40f;
                    b.TargetScale = radius * 2.0f; // Exactly 1:1 match with damage hitbox
                    b.Root.transform.position = center;
                    b.Root.SetActive(true);
                    _blastPool[i] = b;
                    break;
                }
            }

            // 3. Spawn 8 fiery debris particles
            int spawnedDebris = 0;
            for (int i = 0; i < _debrisPool.Count && spawnedDebris < 8; i++)
            {
                var d = _debrisPool[i];
                if (!d.IsActive)
                {
                    d.IsActive = true;
                    d.Position = center;
                    float angle = Random.Range(0f, 360f) * Mathf.Deg2Rad;
                    float speed = Random.Range(5.0f, 10.0f);
                    d.Velocity = new Vector2(Mathf.Cos(angle) * speed, Mathf.Sin(angle) * speed);
                    d.Elapsed = 0f;
                    d.Lifetime = Random.Range(0.25f, 0.45f);
                    d.Root.transform.position = center;
                    d.Root.transform.localScale = Vector3.one * Random.Range(0.35f, 0.65f);
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

            // 1. Update Target Indicators (Countdown convergence)
            for (int i = 0; i < _indicatorPool.Count; i++)
            {
                var ind = _indicatorPool[i];
                if (ind.IsActive)
                {
                    ind.Elapsed += dt;
                    float progress = Mathf.Clamp01(ind.Elapsed / ind.Duration);

                    // Pulse ring shrinks from outer to exact boundary
                    float currentPulseScale = Mathf.Lerp(ind.Radius * 2.4f, ind.Radius * 2.0f, progress);
                    ind.PulseRenderer.transform.localScale = Vector3.one * currentPulseScale;

                    // Soft fade in / pulse alpha (comfortable for eyes)
                    float alpha = Mathf.Lerp(0.3f, 0.65f, progress);
                    ind.DecalRenderer.color = new Color(0.95f, 0.40f, 0.1f, alpha * 0.50f);
                    ind.PulseRenderer.color = new Color(1.0f, 0.65f, 0.15f, alpha * 0.35f);

                    if (progress >= 1.0f)
                    {
                        ind.IsActive = false;
                        ind.Root.SetActive(false);
                    }
                    _indicatorPool[i] = ind;
                }
            }

            // 2. Update Dropping Meteors
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

            // 3. Update Blasts
            for (int i = 0; i < _blastPool.Count; i++)
            {
                var b = _blastPool[i];
                if (b.IsActive)
                {
                    b.Elapsed += dt;
                    float progress = Mathf.Clamp01(b.Elapsed / b.Duration);

                    // Core expand & smooth fade
                    float scale = Mathf.Lerp(b.TargetScale * 0.30f, b.TargetScale, Mathf.Sqrt(progress));
                    b.Root.transform.localScale = Vector3.one * scale;

                    float alpha = Mathf.Clamp01(1.0f - progress * progress);
                    b.CoreRenderer.color = new Color(1.0f, 0.48f * (1f - progress), 0.08f, alpha * 0.70f);
                    b.RingRenderer.color = new Color(0.95f, 0.35f, 0.05f, alpha * 0.55f);

                    if (progress >= 1.0f)
                    {
                        b.IsActive = false;
                        b.Root.SetActive(false);
                    }
                    _blastPool[i] = b;
                }
            }

            // 4. Update Debris Particles
            for (int i = 0; i < _debrisPool.Count; i++)
            {
                var d = _debrisPool[i];
                if (d.IsActive)
                {
                    d.Elapsed += dt;
                    float progress = Mathf.Clamp01(d.Elapsed / d.Lifetime);

                    d.Position += d.Velocity * dt;
                    d.Velocity = Vector2.Lerp(d.Velocity, Vector2.zero, dt * 5.0f);
                    d.Root.transform.position = d.Position;

                    float alpha = 1.0f - progress;
                    d.Renderer.color = new Color(1.0f, 0.65f, 0.15f, alpha);

                    if (progress >= 1.0f)
                    {
                        d.IsActive = false;
                        d.Root.SetActive(false);
                    }
                    _debrisPool[i] = d;
                }
            }
        }
    }
}
