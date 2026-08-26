using System.Collections.Generic;
using UnityEngine;
using HappyShoot.Domain.Events;
using HappyShoot.View.Cameras;
using HappyShoot.View.Utils;

namespace HappyShoot.View.Projectiles
{
    /// <summary>
    /// Visual Manager for Warrior Upheaval (Ground Stomp) skill.
    /// Spawns forward-driving V-shape wedge earth shockwaves and fracturing rock slabs
    /// that ripple forward in rapid succession (30ms per step) for an intense 'dudududu' seismic impact.
    /// Zero-allocation pooling, under 350 lines.
    /// </summary>
    public class GroundStompManagerView : MonoBehaviour
    {
        private class RockDebris
        {
            public Transform Transform;
            public SpriteRenderer Renderer;
            public Vector3 Velocity;
            public float Gravity;
            public float Lifetime;
            public float MaxLifetime;
            public bool IsActive;
        }

        private class RockChunk
        {
            public GameObject GameObject;
            public Transform Transform;
            public SpriteRenderer Renderer;
            public Vector3 BaseLocalPos;
            public Vector3 TargetScale;
        }

        private class RuptureWaveInstance
        {
            public GameObject GameObject;
            public Transform Transform;
            public SpriteRenderer WaveRenderer;
            public Transform WaveTransform;
            public float Timer;
            public float Duration;
            public bool IsActive;
            public Vector3 WaveTargetScale;
            public List<RockChunk> Chunks = new List<RockChunk>(2);
            public List<RockDebris> DebrisList = new List<RockDebris>(8);
        }

        private struct PendingRupture
        {
            public Vector2 Position;
            public Vector2 ForwardDir;
            public float Radius;
            public float DelayTimer;
            public bool IsEarthshaker;
        }

        private const int PoolCapacity = 48;
        private readonly List<RuptureWaveInstance> _pool = new List<RuptureWaveInstance>(PoolCapacity);
        private readonly List<PendingRupture> _pendingQueue = new List<PendingRupture>(64);
        private EventBus _eventBus;

        public void Initialize(EventBus eventBus)
        {
            _eventBus = eventBus;
            _eventBus?.Subscribe<GroundStompExecutedEvent>(OnGroundStompExecuted);
            _eventBus?.Subscribe<EarthshakerExecutedEvent>(OnEarthshakerExecuted);

            PrewarmPool();
        }

        private void OnDestroy()
        {
            _eventBus?.Unsubscribe<GroundStompExecutedEvent>(OnGroundStompExecuted);
            _eventBus?.Unsubscribe<EarthshakerExecutedEvent>(OnEarthshakerExecuted);
        }

        private void PrewarmPool()
        {
            if (_pool.Count > 0) return;

            var waveSprite = SpriteHelper.GetOrCreateUpheavalWaveSprite();
            var chunkSprite = SpriteHelper.GetOrCreateUpheavalChunkSprite();
            var rockSprite = SpriteHelper.GetOrCreateWhiteSprite();

            for (int i = 0; i < PoolCapacity; i++)
            {
                var rootGo = new GameObject($"UpheavalWaveNode_{i + 1}");
                rootGo.transform.SetParent(transform, false);

                // 1. Forward-facing V-Shape Earth Shockwave Crest
                var waveGo = new GameObject("ShockwaveCrest");
                waveGo.transform.SetParent(rootGo.transform, false);
                var waveSr = waveGo.AddComponent<SpriteRenderer>();
                waveSr.sprite = waveSprite;
                waveSr.sortingOrder = 6; // Floor shockwave crest layer

                var instance = new RuptureWaveInstance
                {
                    GameObject = rootGo,
                    Transform = rootGo.transform,
                    WaveRenderer = waveSr,
                    WaveTransform = waveGo.transform,
                    IsActive = false
                };

                // 2. Heavy fractured rock slabs lifting on both sides
                for (int c = 0; c < 2; c++)
                {
                    var chunkGo = new GameObject($"EarthChunk_{c + 1}");
                    chunkGo.transform.SetParent(rootGo.transform, false);
                    var chunkSr = chunkGo.AddComponent<SpriteRenderer>();
                    chunkSr.sprite = chunkSprite;
                    chunkSr.sortingOrder = 5; // Under wave crest, above floor

                    instance.Chunks.Add(new RockChunk
                    {
                        GameObject = chunkGo,
                        Transform = chunkGo.transform,
                        Renderer = chunkSr,
                        BaseLocalPos = Vector3.zero,
                        TargetScale = Vector3.one
                    });
                }

                // 3. Fast flying rock fragments & magma embers
                for (int d = 0; d < 8; d++)
                {
                    var rockGo = new GameObject($"Debris_{d + 1}");
                    rockGo.transform.SetParent(rootGo.transform, false);
                    rockGo.transform.localScale = Vector3.one * Random.Range(0.09f, 0.16f);

                    var rockSr = rockGo.AddComponent<SpriteRenderer>();
                    rockSr.sprite = rockSprite;
                    rockSr.sortingOrder = 7;
                    rockGo.SetActive(false);

                    instance.DebrisList.Add(new RockDebris
                    {
                        Transform = rockGo.transform,
                        Renderer = rockSr,
                        IsActive = false
                    });
                }

                rootGo.SetActive(false);
                _pool.Add(instance);
            }
        }

        private void OnGroundStompExecuted(GroundStompExecutedEvent evt)
        {
            if (evt.StepPositions == null || evt.StepPositions.Length == 0)
            {
                SpawnRuptureVisual(new Vector2((float)evt.Origin.X, (float)evt.Origin.Y), Vector2.right, evt.StepRadius);
                return;
            }

            Vector2 mainDir = new Vector2((float)evt.MainDirection.X, (float)evt.MainDirection.Y);
            if (mainDir.sqrMagnitude < 0.001f) mainDir = Vector2.right;

            int lineCount = Mathf.Max(1, evt.LineCount);
            int stepsPerLine = evt.StepPositions.Length / lineCount;

            // Enqueue rapid forward seismic ripples (30ms interval for intense dudududu drive)
            for (int i = 0; i < evt.StepPositions.Length; i++)
            {
                int stepIndex = (stepsPerLine > 0) ? (i % stepsPerLine) : i;
                float delay = stepIndex * 0.030f;
                Vector2 pos = new Vector2((float)evt.StepPositions[i].X, (float)evt.StepPositions[i].Y);

                _pendingQueue.Add(new PendingRupture
                {
                    Position = pos,
                    ForwardDir = mainDir,
                    Radius = evt.StepRadius,
                    DelayTimer = delay,
                    IsEarthshaker = false
                });
            }
        }

        private void OnEarthshakerExecuted(EarthshakerExecutedEvent evt)
        {
            Vector2 center = new Vector2((float)evt.CenterPosition.X, (float)evt.CenterPosition.Y);
            SpawnRuptureVisual(center, Vector2.up, evt.Radius, isEarthshaker: true);

            Vector2[] dirs = { Vector2.up, Vector2.down, Vector2.left, Vector2.right };
            for (int d = 0; d < dirs.Length; d++)
            {
                for (int s = 1; s <= 4; s++)
                {
                    Vector2 fissurePos = center + dirs[d] * (s * 1.1f);
                    _pendingQueue.Add(new PendingRupture
                    {
                        Position = fissurePos,
                        ForwardDir = dirs[d],
                        Radius = evt.Radius * 0.7f,
                        DelayTimer = s * 0.038f,
                        IsEarthshaker = true
                    });
                }
            }
        }

        public void SpawnRuptureVisual(Vector2 position, Vector2 forwardDir, float radius, bool isEarthshaker = false)
        {
            CameraFollowView.Instance?.TriggerShake(isEarthshaker ? "earthshaker" : "ground_stomp", duration: 0.12f, intensity: isEarthshaker ? 0.26f : 0.18f);

            float forwardAngle = Mathf.Atan2(forwardDir.y, forwardDir.x) * Mathf.Rad2Deg;

            for (int i = 0; i < _pool.Count; i++)
            {
                var fx = _pool[i];
                if (!fx.IsActive)
                {
                    fx.IsActive = true;
                    fx.Transform.position = position;
                    fx.Timer = 0f;
                    fx.Duration = 0.32f; // Punchy crisp shockwave duration

                    // 1. Forward-facing V-Shape Earth Shockwave Crest (scales 1:1 with radius)
                    float waveScale = (radius / 0.70f) * 1.15f;
                    fx.WaveTargetScale = new Vector3(waveScale * 1.15f, waveScale * 1.05f, 1f);
                    fx.WaveTransform.localScale = fx.WaveTargetScale * 0.5f; // Starts fast-expanding
                    fx.WaveTransform.rotation = Quaternion.Euler(0f, 0f, forwardAngle);
                    fx.WaveRenderer.color = Color.white;

                    // 2. Heavy fractured rock slabs lifting on left and right flanks
                    Vector2 perp = new Vector2(-forwardDir.y, forwardDir.x);
                    for (int c = 0; c < fx.Chunks.Count; c++)
                    {
                        var chunk = fx.Chunks[c];
                        chunk.GameObject.SetActive(true);

                        float flankOffset = (c == 0 ? -0.38f : 0.38f) * (radius / 0.70f);
                        chunk.BaseLocalPos = (Vector3)(perp * flankOffset);
                        chunk.Transform.localPosition = chunk.BaseLocalPos;

                        float chunkRot = forwardAngle + (c == 0 ? -25f : 25f);
                        chunk.Transform.rotation = Quaternion.Euler(0f, 0f, chunkRot);

                        float cScale = Random.Range(0.60f, 0.85f) * (radius / 0.70f);
                        chunk.TargetScale = new Vector3(cScale, cScale * 0.9f, 1f);
                        chunk.Transform.localScale = Vector3.zero;
                        chunk.Renderer.color = Color.white;
                    }

                    // 3. Fast flying rock fragments
                    for (int d = 0; d < fx.DebrisList.Count; d++)
                    {
                        var debris = fx.DebrisList[d];
                        debris.IsActive = true;
                        debris.Transform.localPosition = Vector3.zero;
                        debris.MaxLifetime = Random.Range(0.18f, 0.28f);
                        debris.Lifetime = debris.MaxLifetime;
                        debris.Gravity = Random.Range(22.0f, 32.0f);

                        float sprayAngle = (Random.Range(-35f, 35f) + forwardAngle) * Mathf.Deg2Rad;
                        float speed = Random.Range(3.5f, 6.0f);
                        debris.Velocity = new Vector3(Mathf.Cos(sprayAngle) * speed, Mathf.Sin(sprayAngle) * speed + Random.Range(1.5f, 3.5f), 0f);

                        debris.Renderer.color = (d % 2 == 0)
                            ? new Color(1.0f, 0.85f, 0.30f, 1f) // Blazing amber gold
                            : new Color(0.36f, 0.18f, 0.08f, 1f); // Dark basalt rock
                        debris.Transform.gameObject.SetActive(true);
                    }

                    fx.GameObject.SetActive(true);
                    return;
                }
            }
        }

        private void Update()
        {
            float dt = Time.deltaTime;

            // 1. Process pending sequential shockwaves (30ms per step)
            for (int q = _pendingQueue.Count - 1; q >= 0; q--)
            {
                var item = _pendingQueue[q];
                item.DelayTimer -= dt;
                if (item.DelayTimer <= 0f)
                {
                    SpawnRuptureVisual(item.Position, item.ForwardDir, item.Radius, item.IsEarthshaker);
                    _pendingQueue.RemoveAt(q);
                }
                else
                {
                    _pendingQueue[q] = item;
                }
            }

            // 2. Animate active shockwave crests, rock chunks & debris
            for (int i = 0; i < _pool.Count; i++)
            {
                var fx = _pool[i];
                if (!fx.IsActive) continue;

                fx.Timer += dt;
                float progress = Mathf.Clamp01(fx.Timer / fx.Duration);

                // Wave crest fast forward slam surge (0 -> 1 in 45ms)
                float waveT = Mathf.Clamp01(fx.Timer / 0.045f);
                float surge = Mathf.Sin(waveT * Mathf.PI * 0.5f);
                fx.WaveTransform.localScale = Vector3.Lerp(fx.WaveTargetScale * 0.5f, fx.WaveTargetScale, surge);

                // Fade out wave crest
                Color wc = fx.WaveRenderer.color;
                wc.a = progress > 0.45f ? Mathf.Clamp01((1.0f - progress) / 0.55f) : 1.0f;
                fx.WaveRenderer.color = wc;

                // Animate fracturing rock chunks lifting and settling
                float chunkT = Mathf.Clamp01(fx.Timer / 0.06f);
                float chunkRise = Mathf.Sin(chunkT * Mathf.PI * 0.5f);
                for (int c = 0; c < fx.Chunks.Count; c++)
                {
                    var chunk = fx.Chunks[c];
                    float alpha = 1.0f;
                    if (progress > 0.45f)
                    {
                        alpha = Mathf.Clamp01((1.0f - progress) / 0.55f);
                    }

                    chunk.Transform.localScale = Vector3.Lerp(Vector3.zero, chunk.TargetScale, chunkRise);
                    Color cc = chunk.Renderer.color;
                    cc.a = alpha;
                    chunk.Renderer.color = cc;
                }

                // Update flying debris physics
                for (int d = 0; d < fx.DebrisList.Count; d++)
                {
                    var debris = fx.DebrisList[d];
                    if (!debris.IsActive) continue;

                    debris.Lifetime -= dt;
                    if (debris.Lifetime <= 0f)
                    {
                        debris.IsActive = false;
                        debris.Transform.gameObject.SetActive(false);
                        continue;
                    }

                    debris.Velocity.y -= debris.Gravity * dt;
                    debris.Transform.localPosition += debris.Velocity * dt;

                    float debrisAlpha = Mathf.Clamp01(debris.Lifetime / (debris.MaxLifetime * 0.4f));
                    Color dc = debris.Renderer.color;
                    dc.a = debrisAlpha;
                    debris.Renderer.color = dc;
                }

                if (fx.Timer >= fx.Duration)
                {
                    fx.IsActive = false;
                    fx.GameObject.SetActive(false);
                }
            }
        }
    }
}
