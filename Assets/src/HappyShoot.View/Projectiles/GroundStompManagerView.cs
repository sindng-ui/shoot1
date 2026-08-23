using System.Collections.Generic;
using UnityEngine;
using HappyShoot.Domain.Events;
using HappyShoot.View.Cameras;
using HappyShoot.View.Utils;

namespace HappyShoot.View.Projectiles
{
    /// <summary>
    /// Visual Manager for Warrior Ground Stomp skill.
    /// Spawns violent expanding earthquake tremors, flying rock debris particles, and camera shake.
    /// Uses prewarmed zero-allocation pooling.
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

        private class GroundStompInstance
        {
            public GameObject GameObject;
            public Transform Transform;
            public SpriteRenderer Renderer;
            public float TargetScale;
            public float Timer;
            public float Duration;
            public bool IsActive;
            public List<RockDebris> DebrisList = new List<RockDebris>(8);
        }

        private const int PoolCapacity = 16;
        private readonly List<GroundStompInstance> _pool = new List<GroundStompInstance>(PoolCapacity);
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

            var stompSprite = SpriteHelper.GetOrCreateGroundStompSprite();
            var rockSprite = SpriteHelper.GetOrCreateWhiteSprite();

            for (int i = 0; i < PoolCapacity; i++)
            {
                var rootGo = new GameObject($"Earthquake_{i + 1}");
                rootGo.transform.SetParent(transform, false);

                var sr = rootGo.AddComponent<SpriteRenderer>();
                sr.sprite = stompSprite;
                sr.sortingOrder = 2; // Floor ground layer
                rootGo.SetActive(false);

                var instance = new GroundStompInstance
                {
                    GameObject = rootGo,
                    Transform = rootGo.transform,
                    Renderer = sr,
                    IsActive = false
                };

                // Create 12 flying rock debris and glowing magma ember particles
                for (int d = 0; d < 12; d++)
                {
                    var rockGo = new GameObject($"RockDebris_{d + 1}");
                    rockGo.transform.SetParent(rootGo.transform, false);
                    rockGo.transform.localScale = Vector3.one * Random.Range(0.12f, 0.22f);

                    var rockSr = rockGo.AddComponent<SpriteRenderer>();
                    rockSr.sprite = rockSprite;
                    rockSr.color = new Color(0.42f, 0.25f, 0.12f, 1f);
                    rockSr.sortingOrder = 5;
                    rockGo.SetActive(false);

                    instance.DebrisList.Add(new RockDebris
                    {
                        Transform = rockGo.transform,
                        Renderer = rockSr,
                        IsActive = false
                    });
                }

                _pool.Add(instance);
            }
        }

        private void OnGroundStompExecuted(GroundStompExecutedEvent evt)
        {
            SpawnStompVisual(new Vector2(evt.CenterPosition.X, evt.CenterPosition.Y), evt.Radius * 2.0f);
        }

        private void OnEarthshakerExecuted(EarthshakerExecutedEvent evt)
        {
            Vector2 center = new Vector2(evt.CenterPosition.X, evt.CenterPosition.Y);
            SpawnStompVisual(center, evt.Radius * 2.0f, isEarthshaker: true);

            // 4-cardinal direction fissure tremors
            Vector2[] dirs = { Vector2.up, Vector2.down, Vector2.left, Vector2.right };
            for (int d = 0; d < dirs.Length; d++)
            {
                Vector2 fissurePos = center + dirs[d] * (evt.Radius * 0.55f);
                SpawnStompVisual(fissurePos, evt.Radius * 1.1f, isEarthshaker: false);
            }
        }

        public void SpawnStompVisual(Vector2 position, float diameter, bool isEarthshaker = false)
        {
            // Camera impact shake (filtered by ground_stomp shake setting)
            CameraFollowView.Instance?.TriggerShake("ground_stomp", duration: isEarthshaker ? 0.24f : 0.18f, intensity: isEarthshaker ? 0.32f : 0.26f);

            for (int i = 0; i < _pool.Count; i++)
            {
                var fx = _pool[i];
                if (!fx.IsActive)
                {
                    fx.IsActive = true;
                    fx.Transform.position = position;
                    // 128px sprite at 16 PPU = 8.0 units base width. Scale to exactly match world diameter.
                    fx.TargetScale = Mathf.Max(0.5f, diameter / 8.0f);
                    fx.Timer = 0f;
                    fx.Duration = 0.45f; // Heavy 450ms earthquake ground crater duration
                    fx.Transform.localScale = Vector3.zero;

                    Color c = Color.white;
                    c.a = 1.0f;
                    fx.Renderer.color = c;

                    // Launch flying rock debris particles in all directions
                    for (int d = 0; d < fx.DebrisList.Count; d++)
                    {
                        var debris = fx.DebrisList[d];
                        debris.IsActive = true;
                        debris.Transform.localPosition = Vector3.zero;
                        debris.MaxLifetime = Random.Range(0.32f, 0.45f);
                        debris.Lifetime = debris.MaxLifetime;
                        debris.Gravity = Random.Range(20.0f, 30.0f);

                        float angle = (d * (360f / fx.DebrisList.Count) + Random.Range(-12f, 12f)) * Mathf.Deg2Rad;
                        float speed = Random.Range(4.0f, 7.5f);
                        debris.Velocity = new Vector3(Mathf.Cos(angle) * speed, Mathf.Sin(angle) * speed + Random.Range(3.0f, 5.5f), 0f);

                        // Alternate between heavy rock brown and glowing ember orange/gold
                        debris.Renderer.color = (d % 3 == 0) 
                            ? new Color(1.0f, 0.75f, 0.20f, 1f) 
                            : (d % 3 == 1 ? new Color(0.95f, 0.40f, 0.10f, 1f) : new Color(0.35f, 0.18f, 0.08f, 1f));
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

            for (int i = 0; i < _pool.Count; i++)
            {
                var fx = _pool[i];
                if (!fx.IsActive) continue;

                fx.Timer += dt;
                float progress = Mathf.Clamp01(fx.Timer / fx.Duration);

                // Fast slam expansion in first 80ms, then stable crater, then smooth fade
                float scaleT = Mathf.Clamp01(fx.Timer / 0.08f);
                float currentScale = Mathf.Lerp(fx.TargetScale * 0.3f, fx.TargetScale, Mathf.Sin(scaleT * Mathf.PI * 0.5f));
                fx.Transform.localScale = Vector3.one * currentScale;

                Color c = fx.Renderer.color;
                if (progress > 0.65f)
                {
                    c.a = Mathf.Clamp01((1.0f - progress) / 0.35f);
                }
                else
                {
                    c.a = 1.0f;
                }
                fx.Renderer.color = c;

                // Update flying rock debris physics
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
