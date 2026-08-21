using System.Collections.Generic;
using UnityEngine;
using HappyShoot.Domain.Events;
using HappyShoot.Domain.Spatial;
using HappyShoot.View.Utils;

namespace HappyShoot.View.Projectiles
{
    /// <summary>
    /// Presentation View managing the visual animation of the Ranger's Arrow Rain skill.
    /// Spawns a volley of falling arrows hitting a concentrated circular zone over 1.0 second.
    /// Uses prewarmed zero-allocation pooling.
    /// </summary>
    public class ArrowRainManagerView : MonoBehaviour
    {
        private class FallingArrow
        {
            public Transform Transform;
            public SpriteRenderer Renderer;
            public Vector3 StartPos;
            public Vector3 TargetPos;
            public float FallProgress; // 0 to 1
            public float FallSpeed;
            public float LandTimer;
            public bool IsActive;
        }

        private class RainZone
        {
            public Vector3 Center;
            public float Radius;
            public float Elapsed;
            public float Duration;
            public bool IsActive;
            public List<FallingArrow> Arrows = new List<FallingArrow>(20);
            public Transform ZoneIndicator;
            public SpriteRenderer IndicatorRenderer;
        }

        private const int MaxZones = 6;
        private const int ArrowsPerZone = 32;
        private readonly List<RainZone> _zones = new List<RainZone>(MaxZones);

        public void Initialize(EventBus eventBus)
        {
            if (eventBus == null) return;
            eventBus.Subscribe<ArrowRainExecutedEvent>(OnArrowRainExecuted);

            PrewarmPool();
        }

        private void PrewarmPool()
        {
            if (_zones.Count > 0) return;

            var arrowSprite = SpriteHelper.GetOrCreateBoneSprite();
            var circleSprite = SpriteHelper.GetOrCreateCircleSprite();

            for (int z = 0; z < MaxZones; z++)
            {
                var zoneGo = new GameObject($"ArrowRainZone_{z + 1}");
                zoneGo.transform.SetParent(transform, false);

                var indicatorGo = new GameObject("ZoneIndicator");
                indicatorGo.transform.SetParent(zoneGo.transform, false);
                var indSr = indicatorGo.AddComponent<SpriteRenderer>();
                indSr.sprite = circleSprite;
                indSr.color = new Color(0.2f, 0.8f, 0.4f, 0.25f);
                indSr.sortingOrder = 1;

                var zone = new RainZone
                {
                    ZoneIndicator = indicatorGo.transform,
                    IndicatorRenderer = indSr,
                    IsActive = false
                };

                for (int a = 0; a < ArrowsPerZone; a++)
                {
                    var arrowGo = new GameObject($"RainArrow_{a + 1}");
                    arrowGo.transform.SetParent(zoneGo.transform, false);
                    arrowGo.transform.localScale = new Vector3(0.8f, 0.8f, 1f);
                    var sr = arrowGo.AddComponent<SpriteRenderer>();
                    sr.sprite = arrowSprite;
                    sr.color = new Color(0.3f, 0.9f, 0.5f, 1f);
                    sr.sortingOrder = 5;
                    arrowGo.SetActive(false);

                    zone.Arrows.Add(new FallingArrow
                    {
                        Transform = arrowGo.transform,
                        Renderer = sr,
                        IsActive = false
                    });
                }

                zoneGo.SetActive(false);
                _zones.Add(zone);
            }
        }

        private void OnArrowRainExecuted(ArrowRainExecutedEvent evt)
        {
            SpawnRainZone(evt.CenterPosition, evt.Radius, evt.Duration);
        }

        private void SpawnRainZone(Vector2D center, float radius, float duration)
        {
            for (int i = 0; i < _zones.Count; i++)
            {
                var zone = _zones[i];
                if (!zone.IsActive)
                {
                    zone.Center = new Vector3(center.X, center.Y, 0f);
                    zone.Radius = radius;
                    zone.Duration = duration > 0f ? duration : 1.0f;
                    zone.Elapsed = 0f;
                    zone.IsActive = true;

                    zone.ZoneIndicator.position = zone.Center;
                    zone.ZoneIndicator.localScale = Vector3.one * (radius * 2f);
                    zone.ZoneIndicator.gameObject.SetActive(true);
                    zone.ZoneIndicator.parent.gameObject.SetActive(true);

                    // Setup staggered falling arrows
                    for (int a = 0; a < zone.Arrows.Count; a++)
                    {
                        var arrow = zone.Arrows[a];
                        float r = Random.Range(0f, radius * 0.85f);
                        float angle = Random.Range(0f, Mathf.PI * 2f);
                        Vector3 landPos = zone.Center + new Vector3(Mathf.Cos(angle) * r, Mathf.Sin(angle) * r, 0f);
                        Vector3 startPos = landPos + new Vector3(Random.Range(-0.6f, -0.3f), Random.Range(4.5f, 6.0f), 0f);

                        arrow.StartPos = startPos;
                        arrow.TargetPos = landPos;
                        arrow.FallProgress = -((float)a / zone.Arrows.Count * 1.5f); // Stagger spawn over 1.5s
                        arrow.FallSpeed = Random.Range(3.5f, 4.8f);
                        arrow.LandTimer = 0.35f;
                        arrow.Transform.position = startPos;

                        // Point arrow downwards towards target
                        Vector3 dir = (landPos - startPos).normalized;
                        float rotZ = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
                        arrow.Transform.rotation = Quaternion.Euler(0f, 0f, rotZ);

                        arrow.Renderer.color = new Color(0.3f, 0.9f, 0.5f, 1f);
                        arrow.Transform.gameObject.SetActive(false);
                        arrow.IsActive = true;
                    }

                    return;
                }
            }
        }

        private void Update()
        {
            float dt = Time.deltaTime;

            for (int z = 0; z < _zones.Count; z++)
            {
                var zone = _zones[z];
                if (!zone.IsActive) continue;

                zone.Elapsed += dt;
                float zoneAlpha = Mathf.Clamp01(1f - (zone.Elapsed / zone.Duration));
                if (zone.IndicatorRenderer != null)
                {
                    zone.IndicatorRenderer.color = new Color(0.2f, 0.8f, 0.4f, zoneAlpha * 0.35f);
                }

                // Update falling arrows
                for (int a = 0; a < zone.Arrows.Count; a++)
                {
                    var arrow = zone.Arrows[a];
                    if (!arrow.IsActive) continue;

                    arrow.FallProgress += dt * arrow.FallSpeed;

                    if (arrow.FallProgress < 0f)
                    {
                        // Waiting for staggered launch
                        arrow.Transform.gameObject.SetActive(false);
                    }
                    else if (arrow.FallProgress < 1.0f)
                    {
                        // In flight towards ground
                        arrow.Transform.gameObject.SetActive(true);
                        arrow.Transform.position = Vector3.Lerp(arrow.StartPos, arrow.TargetPos, arrow.FallProgress);
                    }
                    else
                    {
                        // Landed in ground, stick & fade out
                        arrow.Transform.position = arrow.TargetPos;
                        arrow.LandTimer -= dt;
                        float fade = Mathf.Clamp01(arrow.LandTimer / 0.25f);
                        arrow.Renderer.color = new Color(0.3f, 0.9f, 0.5f, fade);

                        if (arrow.LandTimer <= 0f)
                        {
                            arrow.Transform.gameObject.SetActive(false);
                            arrow.IsActive = false;
                        }
                    }
                }

                if (zone.Elapsed >= zone.Duration)
                {
                    zone.IsActive = false;
                    zone.ZoneIndicator.parent.gameObject.SetActive(false);
                }
            }
        }
    }
}
