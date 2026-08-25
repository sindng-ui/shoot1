using System.Collections.Generic;
using UnityEngine;
using HappyShoot.Domain.Events;
using HappyShoot.View.Cameras;
using HappyShoot.View.Utils;

namespace HappyShoot.View.Projectiles
{
    /// <summary>
    /// Visual Manager for Warrior Whirlwind skill.
    /// Spawns a high-speed 1080 deg/s spinning 360-degree steel cyclone blade storm with razor wind sparks.
    /// Uses prewarmed zero-allocation pooling.
    /// </summary>
    public class WhirlwindManagerView : MonoBehaviour
    {
        private class WindSpark
        {
            public Transform Transform;
            public SpriteRenderer Renderer;
            public Vector3 Velocity;
            public float Lifetime;
            public float MaxLifetime;
            public bool IsActive;
        }

        private class WhirlwindInstance
        {
            public GameObject GameObject;
            public Transform Transform;
            public SpriteRenderer Renderer;
            public float TargetScale;
            public float Timer;
            public float Duration;
            public float RotationSpeed;
            public bool IsActive;
            public List<WindSpark> SparkList = new List<WindSpark>(8);
        }

        private const int PoolCapacity = 16;
        private readonly List<WhirlwindInstance> _pool = new List<WhirlwindInstance>(PoolCapacity);
        private EventBus _eventBus;

        public void Initialize(EventBus eventBus)
        {
            _eventBus = eventBus;
            _eventBus?.Subscribe<WhirlwindExecutedEvent>(OnWhirlwindExecuted);
            _eventBus?.Subscribe<TempestWhirlwindExecutedEvent>(OnTempestWhirlwindExecuted);

            PrewarmPool();
        }

        private void OnDestroy()
        {
            _eventBus?.Unsubscribe<WhirlwindExecutedEvent>(OnWhirlwindExecuted);
            _eventBus?.Unsubscribe<TempestWhirlwindExecutedEvent>(OnTempestWhirlwindExecuted);
        }

        private void PrewarmPool()
        {
            if (_pool.Count > 0) return;

            var bladeSprite = SpriteHelper.GetOrCreateWhirlwindBladeSprite();
            var sparkSprite = SpriteHelper.GetOrCreateWhiteSprite();

            for (int i = 0; i < PoolCapacity; i++)
            {
                var rootGo = new GameObject($"Whirlwind_{i + 1}");
                rootGo.transform.SetParent(transform, false);

                var sr = rootGo.AddComponent<SpriteRenderer>();
                sr.sprite = bladeSprite;
                sr.sortingOrder = 28; // On top of monsters (10)
                rootGo.SetActive(false);

                var instance = new WhirlwindInstance
                {
                    GameObject = rootGo,
                    Transform = rootGo.transform,
                    Renderer = sr,
                    IsActive = false,
                    RotationSpeed = 1080f // 3 full revolutions per second
                };

                // Create 8 razor wind slash spark particles
                for (int s = 0; s < 8; s++)
                {
                    var sparkGo = new GameObject($"WindSpark_{s + 1}");
                    sparkGo.transform.SetParent(rootGo.transform, false);
                    sparkGo.transform.localScale = new Vector3(0.35f, 0.08f, 1f);

                    var sparkSr = sparkGo.AddComponent<SpriteRenderer>();
                    sparkSr.sprite = sparkSprite;
                    sparkSr.color = new Color(0.70f, 0.95f, 1.0f, 1f);
                    sparkSr.sortingOrder = 29; // In front of whirlwind body
                    sparkGo.SetActive(false);

                    instance.SparkList.Add(new WindSpark
                    {
                        Transform = sparkGo.transform,
                        Renderer = sparkSr,
                        IsActive = false
                    });
                }

                _pool.Add(instance);
            }
        }

        private void OnWhirlwindExecuted(WhirlwindExecutedEvent evt)
        {
            SpawnWhirlwindVisual(new Vector2(evt.CenterPosition.X, evt.CenterPosition.Y), evt.Radius * 2.0f);
        }

        private void OnTempestWhirlwindExecuted(TempestWhirlwindExecutedEvent evt)
        {
            SpawnWhirlwindVisual(new Vector2(evt.CenterPosition.X, evt.CenterPosition.Y), evt.Radius * 2.0f, isTempest: true);
        }

        public void SpawnWhirlwindVisual(Vector2 position, float diameter, bool isTempest = false)
        {
            // Subtle camera slash vibration
            CameraFollowView.Instance?.TriggerShake(isTempest ? "tempest_whirlwind" : "whirlwind", duration: isTempest ? 0.16f : 0.12f, intensity: isTempest ? 0.22f : 0.15f);

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
                    fx.Duration = 0.28f; // 280ms high-speed cyclone spin duration
                    fx.Transform.localScale = Vector3.zero;
                    fx.Transform.localRotation = Quaternion.Euler(0f, 0f, Random.Range(0f, 360f));

                    Color c = Color.white;
                    c.a = 1.0f;
                    fx.Renderer.color = c;

                    // Launch razor wind sparks outward in cyclone trajectory
                    for (int s = 0; s < fx.SparkList.Count; s++)
                    {
                        var spark = fx.SparkList[s];
                        spark.IsActive = true;
                        spark.Transform.localPosition = Vector3.zero;
                        spark.MaxLifetime = Random.Range(0.18f, 0.26f);
                        spark.Lifetime = spark.MaxLifetime;

                        float angle = (s * (360f / fx.SparkList.Count) + Random.Range(-10f, 10f)) * Mathf.Deg2Rad;
                        float speed = Random.Range(5.0f, 8.5f);
                        spark.Velocity = new Vector3(Mathf.Cos(angle) * speed, Mathf.Sin(angle) * speed, 0f);
                        spark.Transform.localRotation = Quaternion.Euler(0f, 0f, angle * Mathf.Rad2Deg);

                        spark.Renderer.color = (s % 2 == 0) 
                            ? new Color(0.85f, 0.98f, 1.0f, 1f) 
                            : new Color(0.30f, 0.80f, 1.0f, 1f);
                        spark.Transform.gameObject.SetActive(true);
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

                // Continuous high-speed 360-degree rotation
                fx.Transform.Rotate(0f, 0f, -fx.RotationSpeed * dt);

                // Pop-expansion in first 40ms, then stable blade spin, then fast alpha fade
                float scaleT = Mathf.Clamp01(fx.Timer / 0.04f);
                float currentScale = Mathf.Lerp(fx.TargetScale * 0.4f, fx.TargetScale, scaleT);
                fx.Transform.localScale = Vector3.one * currentScale;

                Color c = fx.Renderer.color;
                if (progress > 0.50f)
                {
                    c.a = Mathf.Clamp01((1.0f - progress) / 0.50f);
                }
                else
                {
                    c.a = 1.0f;
                }
                fx.Renderer.color = c;

                // Update wind spark particles
                for (int s = 0; s < fx.SparkList.Count; s++)
                {
                    var spark = fx.SparkList[s];
                    if (!spark.IsActive) continue;

                    spark.Lifetime -= dt;
                    if (spark.Lifetime <= 0f)
                    {
                        spark.IsActive = false;
                        spark.Transform.gameObject.SetActive(false);
                        continue;
                    }

                    spark.Transform.localPosition += spark.Velocity * dt;
                    spark.Velocity *= Mathf.Pow(0.1f, dt); // Wind resistance drag

                    float sparkAlpha = Mathf.Clamp01(spark.Lifetime / spark.MaxLifetime);
                    Color sc = spark.Renderer.color;
                    sc.a = sparkAlpha;
                    spark.Renderer.color = sc;
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
