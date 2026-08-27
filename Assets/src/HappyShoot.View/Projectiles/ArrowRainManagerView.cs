using System.Collections.Generic;
using UnityEngine;
using HappyShoot.Domain.Entities;
using HappyShoot.Domain.Events;
using HappyShoot.Domain.Spatial;
using HappyShoot.View.Cameras;
using HappyShoot.View.Monsters;
using HappyShoot.View.Utils;

namespace HappyShoot.View.Projectiles
{
    /// <summary>
    /// Presentation View managing the visual animation and instant landing impact damage of Arrow Rain.
    /// Each falling arrow hits the ground and instantly deals damage to nearby monsters with zero lag.
    /// Strictly modular and under 500 lines.
    /// </summary>
    public class ArrowRainManagerView : MonoBehaviour
    {
        private class FallingArrow
        {
            public Transform Transform;
            public SpriteRenderer Renderer;
            public Vector3 StartPos;
            public Vector3 TargetPos;
            public float FallProgress;
            public float FallSpeed;
            public float LandTimer;
            public bool IsActive;
            public bool HasLanded;
        }

        private class RainZone
        {
            public Vector3 Center;
            public float Radius;
            public float DamagePerArrow;
            public float Elapsed;
            public float Duration;
            public bool IsActive;
            public List<FallingArrow> Arrows = new List<FallingArrow>(64);
            public Transform ZoneIndicator;
            public SpriteRenderer IndicatorRenderer;
        }

        private const int MaxZones = 6;
        private const int ArrowsPerZone = 64;
        private const float IndicatorDuration = 0.60f; // 바닥 마법진/인디케이터 무늬 0.6초 후 페이드아웃 및 자동 제거
        private readonly List<RainZone> _zones = new List<RainZone>(MaxZones);
        private readonly List<MonsterEntity> _hitBuffer = new List<MonsterEntity>(32);
        private MonsterSpawnerView _spawnerView;
        private Player.PlayerView _playerView;

        private EventBus _eventBus;

        public void Initialize(EventBus eventBus, MonsterSpawnerView spawnerView = null, Player.PlayerView playerView = null)
        {
            _eventBus = eventBus;
            _spawnerView = spawnerView;
            _playerView = playerView;
            if (_eventBus != null)
            {
                _eventBus.Subscribe<ArrowRainExecutedEvent>(OnArrowRainExecuted);
                _eventBus.Subscribe<StellarRainExecutedEvent>(OnStellarRainExecuted);
            }

            PrewarmPool();
        }

        private void OnDestroy()
        {
            if (_eventBus != null)
            {
                _eventBus.Unsubscribe<ArrowRainExecutedEvent>(OnArrowRainExecuted);
                _eventBus.Unsubscribe<StellarRainExecutedEvent>(OnStellarRainExecuted);
            }
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
                    arrowGo.transform.localScale = new Vector3(0.85f, 0.85f, 1f);
                    var sr = arrowGo.AddComponent<SpriteRenderer>();
                    sr.sprite = arrowSprite;
                    sr.color = new Color(0.3f, 0.95f, 0.5f, 1f);
                    sr.sortingOrder = 5;
                    arrowGo.SetActive(false);

                    zone.Arrows.Add(new FallingArrow
                    {
                        Transform = arrowGo.transform,
                        Renderer = sr,
                        IsActive = false,
                        HasLanded = false
                    });
                }

                zoneGo.SetActive(false);
                _zones.Add(zone);
            }
        }

        private void OnArrowRainExecuted(ArrowRainExecutedEvent evt)
        {
            SpawnRainZone(evt.CenterPosition, evt.Radius, evt.Duration, evt.ArrowCount, evt.DamagePerArrow, isStellar: false);
        }

        private void OnStellarRainExecuted(StellarRainExecutedEvent evt)
        {
            SpawnRainZone(evt.TargetCenter, evt.Radius, evt.Duration, evt.ArrowCount, evt.Damage, isStellar: true);
        }

        private void SpawnRainZone(Vector2D center, float radius, float duration, int arrowCount, float damagePerArrow, bool isStellar = false)
        {
            for (int i = 0; i < _zones.Count; i++)
            {
                var zone = _zones[i];
                if (!zone.IsActive)
                {
                    zone.Center = new Vector3((float)center.X, (float)center.Y, 0f);
                    zone.Radius = radius;
                    zone.DamagePerArrow = damagePerArrow;
                    zone.Duration = duration > 0f ? duration : 1.5f;
                    zone.Elapsed = 0f;
                    zone.IsActive = true;

                    zone.ZoneIndicator.position = zone.Center;
                    zone.ZoneIndicator.localScale = Vector3.one * (radius * 2f);
                    zone.IndicatorRenderer.color = new Color(0.2f, 0.8f, 0.4f, 0.25f);
                    zone.ZoneIndicator.gameObject.SetActive(true);
                    zone.ZoneIndicator.parent.gameObject.SetActive(true);

                    CameraFollowView.Instance?.TriggerShake(isStellar ? "stellar_rain" : "arrow_rain", duration: isStellar ? 0.16f : 0.12f, intensity: isStellar ? 0.20f : 0.14f);

                    int activeArrowCount = Mathf.Clamp(arrowCount > 0 ? arrowCount : 32, 16, ArrowsPerZone);
                    float staggerDuration = zone.Duration * 0.90f;

                    for (int a = 0; a < zone.Arrows.Count; a++)
                    {
                        var arrow = zone.Arrows[a];
                        if (a < activeArrowCount)
                        {
                            float r = Random.Range(0f, radius * 0.90f);
                            float angle = Random.Range(0f, Mathf.PI * 2f);
                            Vector3 landPos = zone.Center + new Vector3(Mathf.Cos(angle) * r, Mathf.Sin(angle) * r, 0f);
                            Vector3 startPos = landPos + new Vector3(Random.Range(-0.4f, -0.2f), Random.Range(4.5f, 6.0f), 0f);

                            arrow.StartPos = startPos;
                            arrow.TargetPos = landPos;
                            arrow.FallProgress = -((float)a / activeArrowCount * staggerDuration);
                            arrow.FallSpeed = Random.Range(8.0f, 10.5f); // Fast razor-sharp landing in ~0.12s
                            arrow.LandTimer = 0.35f;
                            arrow.Transform.position = startPos;
                            arrow.HasLanded = false;

                            Vector3 dir = (landPos - startPos).normalized;
                            float rotZ = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
                            arrow.Transform.rotation = Quaternion.Euler(0f, 0f, rotZ);

                            arrow.Renderer.color = new Color(0.3f, 0.95f, 0.5f, 1f);
                            arrow.Transform.gameObject.SetActive(false);
                            arrow.IsActive = true;
                        }
                        else
                        {
                            arrow.IsActive = false;
                            arrow.Transform.gameObject.SetActive(false);
                        }
                    }

                    return;
                }
            }
        }

        private void Update()
        {
            float dt = Time.deltaTime;
            if (dt <= 0f) return;

            for (int z = 0; z < _zones.Count; z++)
            {
                var zone = _zones[z];
                if (zone.IsActive)
                {
                    zone.Elapsed += dt;
                    bool anyArrowAlive = false;

                    // 1. Zone Indicator Fadeout & Disappear in 0.6 seconds
                    if (zone.ZoneIndicator != null && zone.ZoneIndicator.gameObject.activeSelf)
                    {
                        if (zone.Elapsed < IndicatorDuration)
                        {
                            float t = zone.Elapsed / IndicatorDuration;
                            float alpha = Mathf.Lerp(0.25f, 0.0f, t);
                            zone.IndicatorRenderer.color = new Color(0.2f, 0.8f, 0.4f, alpha);
                        }
                        else
                        {
                            zone.ZoneIndicator.gameObject.SetActive(false);
                        }
                    }

                    for (int a = 0; a < zone.Arrows.Count; a++)
                    {
                        var arrow = zone.Arrows[a];
                        if (arrow.IsActive)
                        {
                            arrow.FallProgress += dt * arrow.FallSpeed;

                            if (arrow.FallProgress < 0f)
                            {
                                anyArrowAlive = true;
                                continue;
                            }

                            if (!arrow.Transform.gameObject.activeSelf)
                            {
                                arrow.Transform.gameObject.SetActive(true);
                            }

                            if (arrow.FallProgress < 1f)
                            {
                                arrow.Transform.position = Vector3.Lerp(arrow.StartPos, arrow.TargetPos, arrow.FallProgress);
                                anyArrowAlive = true;
                            }
                            else
                            {
                                arrow.Transform.position = arrow.TargetPos;

                                // Impact Trigger: EXACT moment arrow lands on ground!
                                if (!arrow.HasLanded)
                                {
                                    arrow.HasLanded = true;
                                    ApplyArrowLandDamage(arrow.TargetPos, zone.DamagePerArrow);
                                }

                                arrow.LandTimer -= dt;
                                float alpha = Mathf.Clamp01(arrow.LandTimer / 0.35f);
                                arrow.Renderer.color = new Color(0.3f, 0.95f, 0.5f, alpha);

                                if (arrow.LandTimer > 0f)
                                {
                                    anyArrowAlive = true;
                                }
                                else
                                {
                                    arrow.IsActive = false;
                                    arrow.Transform.gameObject.SetActive(false);
                                }
                            }
                        }
                    }

                    if (!anyArrowAlive && zone.Elapsed >= zone.Duration)
                    {
                        zone.IsActive = false;
                        zone.ZoneIndicator.parent.gameObject.SetActive(false);
                    }
                }
            }
        }

        private void ApplyArrowLandDamage(Vector3 landPos, float damage)
        {
            if (_spawnerView == null || _spawnerView.MonsterGrid == null) return;

            Vector2D center = new Vector2D(landPos.x, landPos.y);
            float impactRadius = 0.85f; // Small localized splash per arrow impact

            int hitCount = _spawnerView.MonsterGrid.QueryRadiusNonAlloc(center, impactRadius, _hitBuffer);
            for (int i = 0; i < hitCount; i++)
            {
                var monster = _hitBuffer[i];
                if (monster != null && monster.IsActive && !monster.IsDead)
                {
                    if (_playerView != null && _playerView.Entity != null)
                    {
                        var (hitDmg, isCrit) = _playerView.Entity.RollDamage(damage);
                        monster.TakeDamage(hitDmg, isCrit, DamageType.StellarRain);
                    }
                    else
                    {
                        monster.TakeDamage(damage, false, DamageType.StellarRain);
                    }
                }
            }
        }
    }
}
