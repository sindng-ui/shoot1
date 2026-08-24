using System.Collections.Generic;
using UnityEngine;
using HappyShoot.Domain.Events;
using HappyShoot.View.Cameras;
using HappyShoot.View.Utils;

namespace HappyShoot.View.Projectiles
{
    /// <summary>
    /// Presentation manager for Meteor Strike (Evolved Fireball):
    /// 1. Sleek, comfortable golden-amber rune target indicator.
    /// 2. Blazing high-gravity dropping meteor with fiery comet trail.
    /// 3. Compact golden shockwave ring (reduced to less than half screen spread).
    /// 4. Brilliant instant white-gold nova glint, magma crater decal, and ascending flame burst.
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
            public float TrailTimer;
            public bool IsActive;
        }

        private struct ActiveBlast
        {
            public GameObject Root;
            public SpriteRenderer RingRenderer;
            public SpriteRenderer CoreRenderer;
            public SpriteRenderer FlashRenderer;
            public SpriteRenderer CraterRenderer;
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
        private readonly List<MeteorDebris> _debrisPool = new List<MeteorDebris>(36);

        private EventBus _eventBus;
        private Sprite _indicatorSprite;
        private Sprite _meteorSprite;
        private Sprite _flameSprite;
        private Sprite _ringSprite;
        private Sprite _flashSprite;
        private Sprite _craterSprite;

        public void Initialize(EventBus eventBus)
        {
            _eventBus = eventBus;
            _indicatorSprite = WizardSpriteHelper.GetOrCreateTargetIndicatorSprite(128);
            _meteorSprite = WizardSpriteHelper.GetOrCreateMeteorSprite(48);
            _flameSprite = WizardSpriteHelper.GetOrCreateFireballSprite(32);
            _ringSprite = SkillSpriteHelper.GetOrCreateGroundStompSprite();
            _flashSprite = WizardSpriteHelper.GetOrCreateNovaFlashSprite(32);
            _craterSprite = WizardSpriteHelper.GetOrCreateMagmaCraterSprite(64);

            PrewarmPools();
            _eventBus?.Subscribe<MeteorStrikeExecutedEvent>(OnMeteorStrikeExecuted);
        }

        private void OnDestroy()
        {
            _eventBus?.Unsubscribe<MeteorStrikeExecutedEvent>(OnMeteorStrikeExecuted);
        }

        private void PrewarmPools()
        {
            // 1. Target Indicator Decal Pool (Soft, comfortable rune circles)
            for (int i = 0; i < 6; i++)
            {
                var go = new GameObject($"MeteorIndicator_{i}");
                go.transform.SetParent(transform, false);

                var decal = new GameObject("Decal");
                decal.transform.SetParent(go.transform, false);
                var decalSr = decal.AddComponent<SpriteRenderer>();
                decalSr.sprite = _indicatorSprite;
                decalSr.sortingOrder = 5;

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

            // 3. Blast Shockwave & Impact Glint Pool
            for (int i = 0; i < 6; i++)
            {
                var go = new GameObject($"MeteorBlast_{i}");
                go.transform.SetParent(transform, false);

                var crater = new GameObject("MagmaCrater");
                crater.transform.SetParent(go.transform, false);
                var craterSr = crater.AddComponent<SpriteRenderer>();
                craterSr.sprite = _craterSprite;
                craterSr.sortingOrder = 4;

                var ring = new GameObject("ShockRing");
                ring.transform.SetParent(go.transform, false);
                var ringSr = ring.AddComponent<SpriteRenderer>();
                ringSr.sprite = _ringSprite;
                ringSr.sortingOrder = 34;

                var core = new GameObject("FlameCore");
                core.transform.SetParent(go.transform, false);
                var coreSr = core.AddComponent<SpriteRenderer>();
                coreSr.sprite = _flameSprite;
                coreSr.sortingOrder = 36;

                var flash = new GameObject("NovaFlash");
                flash.transform.SetParent(go.transform, false);
                var flashSr = flash.AddComponent<SpriteRenderer>();
                flashSr.sprite = _flashSprite;
                flashSr.sortingOrder = 38;

                go.SetActive(false);
                _blastPool.Add(new ActiveBlast
                {
                    Root = go,
                    RingRenderer = ringSr,
                    CoreRenderer = coreSr,
                    FlashRenderer = flashSr,
                    CraterRenderer = craterSr,
                    IsActive = false
                });
            }

            // 4. Debris & Trail Particle Pool
            for (int i = 0; i < 36; i++)
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
            Vector2 skyOrigin = target + new Vector2(-3.0f, 8.5f);
            float radius = e.Radius > 0f ? e.Radius : 3.0f;
            float dropDuration = 0.32f;

            // 1. Spawn Ground Target Indicator
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
                    ind.DecalRenderer.transform.localScale = Vector3.one * (radius * 2.0f);
                    ind.DecalRenderer.color = new Color(1.0f, 0.65f, 0.15f, 0.30f);

                    ind.PulseRenderer.transform.localScale = Vector3.one * (radius * 2.2f);
                    ind.PulseRenderer.color = new Color(1.0f, 0.80f, 0.25f, 0.25f);

                    ind.Root.SetActive(true);
                    _indicatorPool[i] = ind;
                    break;
                }
            }

            // 2. Spawn Dropping Meteor (High velocity with flaming corona)
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
                    m.TrailTimer = 0f;
                    m.Root.transform.position = skyOrigin;
                    m.Root.transform.localScale = Vector3.one * 1.35f;
                    m.Root.SetActive(true);
                    _meteorPool[i] = m;
                    break;
                }
            }
        }

        private void TriggerGroundExplosion(Vector2 center, float radius)
        {
            // 1. Punchy Camera Shake
            CameraFollowView.Instance?.TriggerShake("meteor_strike", duration: 0.18f, intensity: 0.28f);

            // 2. Spawn Shockwave Blast, Glint Flash & Magma Crater (Compact spread: radius * 0.85f)
            for (int i = 0; i < _blastPool.Count; i++)
            {
                var b = _blastPool[i];
                if (!b.IsActive)
                {
                    b.IsActive = true;
                    b.Center = center;
                    b.Elapsed = 0f;
                    b.Duration = 0.35f;
                    b.TargetScale = radius * 0.85f; // Reduced to less than half of previous 2.0f scale
                    b.Root.transform.position = center;

                    b.CraterRenderer.transform.localScale = Vector3.one * (radius * 1.1f);
                    b.CraterRenderer.color = new Color(1.0f, 0.55f, 0.15f, 0.70f);

                    b.FlashRenderer.transform.localScale = Vector3.one * (radius * 1.5f);
                    b.FlashRenderer.color = new Color(1.0f, 0.95f, 0.70f, 1.0f);

                    b.Root.SetActive(true);
                    _blastPool[i] = b;
                    break;
                }
            }

            // 3. Spawn 8 Compact Magma Spark Debris Particles (Tightly contained within impact zone)
            int spawnedDebris = 0;
            for (int i = 0; i < _debrisPool.Count && spawnedDebris < 8; i++)
            {
                var d = _debrisPool[i];
                if (!d.IsActive)
                {
                    d.IsActive = true;
                    d.Position = center;
                    float angle = Random.Range(0f, 360f) * Mathf.Deg2Rad;
                    float speed = Random.Range(2.2f, 4.8f); // Soft, controlled splash
                    d.Velocity = new Vector2(Mathf.Cos(angle) * speed, Mathf.Sin(angle) * speed);
                    d.Elapsed = 0f;
                    d.Lifetime = Random.Range(0.22f, 0.38f);
                    d.Root.transform.position = center;
                    d.Root.transform.localScale = Vector3.one * Random.Range(0.35f, 0.60f);
                    d.Renderer.color = (spawnedDebris % 2 == 0) ? new Color(1.0f, 0.90f, 0.30f, 1.0f) : new Color(1.0f, 0.55f, 0.12f, 1.0f);
                    d.Root.SetActive(true);
                    _debrisPool[i] = d;
                    spawnedDebris++;
                }
            }
        }

        private void SpawnFallingTrailParticle(Vector2 pos)
        {
            for (int i = 0; i < _debrisPool.Count; i++)
            {
                var d = _debrisPool[i];
                if (!d.IsActive)
                {
                    d.IsActive = true;
                    d.Position = pos + new Vector2(Random.Range(-0.15f, 0.15f), Random.Range(-0.15f, 0.15f));
                    d.Velocity = new Vector2(Random.Range(-0.8f, 0.8f), Random.Range(0.3f, 1.5f));
                    d.Elapsed = 0f;
                    d.Lifetime = Random.Range(0.12f, 0.20f);
                    d.Root.transform.position = d.Position;
                    d.Root.transform.localScale = Vector3.one * Random.Range(0.25f, 0.45f);
                    d.Renderer.color = new Color(1.0f, 0.75f, 0.20f, 0.85f);
                    d.Root.SetActive(true);
                    _debrisPool[i] = d;
                    break;
                }
            }
        }

        private void Update()
        {
            float dt = Time.deltaTime;

            // 1. Update Target Indicators
            for (int i = 0; i < _indicatorPool.Count; i++)
            {
                var ind = _indicatorPool[i];
                if (ind.IsActive)
                {
                    ind.Elapsed += dt;
                    float progress = Mathf.Clamp01(ind.Elapsed / ind.Duration);

                    float currentPulseScale = Mathf.Lerp(ind.Radius * 2.2f, ind.Radius * 2.0f, progress);
                    ind.PulseRenderer.transform.localScale = Vector3.one * currentPulseScale;

                    float alpha = Mathf.Lerp(0.18f, 0.45f, progress);
                    ind.DecalRenderer.color = new Color(1.0f, 0.65f, 0.15f, alpha);
                    ind.PulseRenderer.color = new Color(1.0f, 0.80f, 0.25f, alpha * 0.65f);

                    if (progress >= 1.0f)
                    {
                        ind.IsActive = false;
                        ind.Root.SetActive(false);
                    }
                    _indicatorPool[i] = ind;
                }
            }

            // 2. Update Dropping Meteors (Cubic in acceleration + dense comet trail)
            for (int i = 0; i < _meteorPool.Count; i++)
            {
                var m = _meteorPool[i];
                if (m.IsActive)
                {
                    m.Elapsed += dt;
                    m.TrailTimer += dt;
                    float t = Mathf.Clamp01(m.Elapsed / m.Duration);
                    float easeT = t * t * t; // Fast gravity drop

                    Vector2 currentPos = Vector2.Lerp(m.StartPos, m.TargetPos, easeT);
                    m.Root.transform.position = currentPos;

                    if (m.TrailTimer >= 0.03f)
                    {
                        m.TrailTimer = 0f;
                        SpawnFallingTrailParticle(currentPos);
                    }

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

            // 3. Update Blasts (Clean shockwave ring, instant nova flash fade, and glowing crater)
            for (int i = 0; i < _blastPool.Count; i++)
            {
                var b = _blastPool[i];
                if (b.IsActive)
                {
                    b.Elapsed += dt;
                    float progress = Mathf.Clamp01(b.Elapsed / b.Duration);

                    // A. Instant Nova Glint (Fades out quickly in first 0.09s)
                    float flashAlpha = Mathf.Clamp01(1.0f - (b.Elapsed / 0.09f));
                    b.FlashRenderer.color = new Color(1.0f, 0.95f, 0.70f, flashAlpha);
                    b.FlashRenderer.transform.localScale = Vector3.one * (b.TargetScale * 1.5f * (1f + flashAlpha * 0.3f));

                    // B. Compact Golden Shockwave Ring (Clean, sharp boundary)
                    float ringScale = Mathf.Lerp(b.TargetScale * 0.3f, b.TargetScale * 1.2f, Mathf.Sqrt(progress));
                    b.RingRenderer.transform.localScale = Vector3.one * ringScale;
                    float ringAlpha = Mathf.Clamp01(1.0f - progress * 1.8f);
                    b.RingRenderer.color = new Color(1.0f, 0.80f, 0.25f, ringAlpha * 0.60f);

                    // C. Center Flame Core & Magma Crater (Smooth ember fade)
                    float coreScale = Mathf.Lerp(b.TargetScale * 0.2f, b.TargetScale * 0.7f, Mathf.Sin(progress * Mathf.PI * 0.5f));
                    b.CoreRenderer.transform.localScale = Vector3.one * coreScale;
                    b.CoreRenderer.color = new Color(1.0f, 0.50f, 0.10f, Mathf.Clamp01((1f - progress) * 0.65f));

                    float craterAlpha = Mathf.Clamp01(1.0f - progress);
                    b.CraterRenderer.color = new Color(1.0f, 0.55f, 0.15f, craterAlpha * 0.55f);

                    if (progress >= 1.0f)
                    {
                        b.IsActive = false;
                        b.Root.SetActive(false);
                    }
                    _blastPool[i] = b;
                }
            }

            // 4. Update Debris & Trail Particles
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
                    Color curCol = d.Renderer.color;
                    d.Renderer.color = new Color(curCol.r, curCol.g, curCol.b, alpha);

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
