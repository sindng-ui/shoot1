using System.Collections.Generic;
using UnityEngine;
using HappyShoot.Domain.Entities;
using HappyShoot.Domain.Events;
using HappyShoot.Domain.Spatial;
using HappyShoot.View.Cameras;
using HappyShoot.View.Monsters;
using HappyShoot.View.Player;
using HappyShoot.View.Utils;

namespace HappyShoot.View.Projectiles
{
    /// <summary>
    /// Presentation View Manager for Ranger signature Wind Glaive (Boomerang).
    /// Renders compact high-speed spinning aerodynamic cyan boomerangs.
    /// Deals precise double-hit damage: 1st hit on outward flight + 2nd hit on return flight.
    /// Strictly modular and under 500 lines.
    /// </summary>
    public class WindGlaiveManagerView : MonoBehaviour
    {
        private class ActiveGlaive
        {
            public GameObject GameObject;
            public SpriteRenderer Renderer;
            public Vector2 StartPos;
            public Vector2 TargetPeakPos;
            public float OutwardDuration;
            public float ReturnDuration;
            public float ElapsedTime;
            public float Damage;
            public float BladeScale = 1.0f;
            public bool IsReturning;
            public float RotationAngle;
            public readonly HashSet<int> HitMonstersOutward = new HashSet<int>();
            public readonly HashSet<int> HitMonstersReturn = new HashSet<int>();
        }

        private PlayerView _playerView;
        private MonsterSpawnerView _spawnerView;
        private EventBus _eventBus;
        private readonly List<ActiveGlaive> _activeGlaives = new List<ActiveGlaive>(32);
        private readonly Queue<ActiveGlaive> _pool = new Queue<ActiveGlaive>(32);
        private readonly List<MonsterEntity> _hitBuffer = new List<MonsterEntity>(16);

        public void Initialize(PlayerView playerView, EventBus eventBus, MonsterSpawnerView spawnerView = null)
        {
            _playerView = playerView;
            _eventBus = eventBus;
            _spawnerView = spawnerView;

            _eventBus?.Subscribe<WindGlaiveExecutedEvent>(OnWindGlaiveExecuted);
            _eventBus?.Subscribe<PhantomGlaiveExecutedEvent>(OnPhantomGlaiveExecuted);
        }

        private void OnDestroy()
        {
            _eventBus?.Unsubscribe<WindGlaiveExecutedEvent>(OnWindGlaiveExecuted);
            _eventBus?.Unsubscribe<PhantomGlaiveExecutedEvent>(OnPhantomGlaiveExecuted);
        }

        private void Update()
        {
            float dt = Time.deltaTime;
            if (dt <= 0f) return;

            Vector2 playerPos = _playerView != null ? (Vector2)_playerView.transform.position : Vector2.zero;

            for (int i = _activeGlaives.Count - 1; i >= 0; i--)
            {
                var g = _activeGlaives[i];
                g.ElapsedTime += dt;
                g.RotationAngle += 1440f * dt; // Rapid 4 rev/s spin

                if (!g.IsReturning)
                {
                    // Outward Flight Phase
                    float t = Mathf.Clamp01(g.ElapsedTime / g.OutwardDuration);
                    float smoothT = Mathf.Sin(t * Mathf.PI * 0.5f);
                    Vector2 currentPos = Vector2.Lerp(g.StartPos, g.TargetPeakPos, smoothT);
                    g.GameObject.transform.position = new Vector3(currentPos.x, currentPos.y, -0.2f);
                    g.GameObject.transform.rotation = Quaternion.Euler(0f, 0f, g.RotationAngle);

                    // 1st Hit Check (Outward)
                    CheckHitMonsters(g.Damage, currentPos, g.HitMonstersOutward, g.BladeScale);

                    if (t >= 1.0f)
                    {
                        g.IsReturning = true;
                        g.ElapsedTime = 0f;
                        g.StartPos = currentPos;
                    }
                }
                else
                {
                    // Return Flight Phase (Double-hit guaranteed!)
                    float t = Mathf.Clamp01(g.ElapsedTime / g.ReturnDuration);
                    float smoothT = t * t; // Accelerating return curve
                    Vector2 currentPos = Vector2.Lerp(g.StartPos, playerPos, smoothT);
                    g.GameObject.transform.position = new Vector3(currentPos.x, currentPos.y, -0.2f);
                    g.GameObject.transform.rotation = Quaternion.Euler(0f, 0f, g.RotationAngle);

                    // 2nd Hit Check (Return path)
                    CheckHitMonsters(g.Damage, currentPos, g.HitMonstersReturn, g.BladeScale);

                    if (t >= 1.0f || Vector2.Distance(currentPos, playerPos) < 0.5f)
                    {
                        RecycleGlaive(g);
                        _activeGlaives.RemoveAt(i);
                    }
                }
            }
        }

        private void CheckHitMonsters(float damage, Vector2 currentPos, HashSet<int> hitSet, float bladeScale = 1.0f)
        {
            if (_spawnerView == null || _spawnerView.MonsterGrid == null) return;

            Vector2D pos2D = new Vector2D(currentPos.x, currentPos.y);
            float hitRadius = 0.65f * Mathf.Max(0.5f, bladeScale);

            int count = _spawnerView.MonsterGrid.QueryRadiusNonAlloc(pos2D, hitRadius, _hitBuffer);
            for (int i = 0; i < count; i++)
            {
                var monster = _hitBuffer[i];
                if (monster != null && monster.IsActive && !monster.IsDead)
                {
                    if (!hitSet.Contains(monster.Id))
                    {
                        hitSet.Add(monster.Id);
                        if (_playerView != null && _playerView.Entity != null)
                        {
                            var (hitDmg, isCrit) = _playerView.Entity.RollDamage(damage);
                            monster.TakeDamage(hitDmg, isCrit);
                        }
                        else
                        {
                            monster.TakeDamage(damage);
                        }
                    }
                }
            }
        }

        private void OnWindGlaiveExecuted(WindGlaiveExecutedEvent evt)
        {
            Vector2 origin = new Vector2((float)evt.Origin.X, (float)evt.Origin.Y);
            Vector2 baseDir = new Vector2((float)evt.TargetDirection.X, (float)evt.TargetDirection.Y).normalized;
            if (baseDir.sqrMagnitude < 0.01f) baseDir = Vector2.right;

            float baseAngle = Mathf.Atan2(baseDir.y, baseDir.x) * Mathf.Rad2Deg;
            int count = Mathf.Max(1, evt.GlaiveCount);

            float outwardTime = Mathf.Max(0.25f, evt.MaxDistance / Mathf.Max(1f, evt.Speed));
            float returnTime = outwardTime * 0.85f;

            for (int i = 0; i < count; i++)
            {
                float offsetAngle = count > 1 ? (-15f + (30f / (count - 1)) * i) : 0f;
                float finalAngle = (baseAngle + offsetAngle) * Mathf.Deg2Rad;
                Vector2 dir = new Vector2(Mathf.Cos(finalAngle), Mathf.Sin(finalAngle));
                Vector2 peakPos = origin + dir * evt.MaxDistance;

                var glaive = GetOrCreateGlaive();
                glaive.StartPos = origin;
                glaive.TargetPeakPos = peakPos;
                glaive.OutwardDuration = outwardTime;
                glaive.ReturnDuration = returnTime;
                glaive.ElapsedTime = 0f;
                glaive.Damage = evt.Damage;
                glaive.BladeScale = 1.0f;
                glaive.IsReturning = false;
                glaive.RotationAngle = Random.Range(0f, 360f);
                glaive.HitMonstersOutward.Clear();
                glaive.HitMonstersReturn.Clear();

                glaive.GameObject.transform.position = new Vector3(origin.x, origin.y, -0.2f);
                glaive.GameObject.transform.localScale = Vector3.one * 0.70f;
                glaive.Renderer.color = Color.white;
                glaive.GameObject.SetActive(true);

                _activeGlaives.Add(glaive);
            }

            CameraFollowView.Instance?.TriggerShake("glaive", duration: 0.10f, intensity: 0.14f);
        }

        private void OnPhantomGlaiveExecuted(PhantomGlaiveExecutedEvent evt)
        {
            Vector2 origin = new Vector2((float)evt.Origin.X, (float)evt.Origin.Y);
            Vector2 baseDir = new Vector2((float)evt.TargetDirection.X, (float)evt.TargetDirection.Y).normalized;
            if (baseDir.sqrMagnitude < 0.01f) baseDir = Vector2.right;

            float baseAngle = Mathf.Atan2(baseDir.y, baseDir.x) * Mathf.Rad2Deg;
            float outwardTime = Mathf.Max(0.25f, evt.MaxDistance / Mathf.Max(1f, evt.Speed));
            float returnTime = outwardTime * 0.85f;
            float bladeScale = Mathf.Max(0.1f, evt.BladeScale);

            // Spawn Prime Glaive + up to 7 Phantom Glaives
            int phantomCount = Mathf.Clamp(evt.PhantomCount, 1, 7);
            int totalGlaives = 1 + phantomCount;
            float fanSpread = Mathf.Min(64f, 16f + phantomCount * 7f);

            for (int i = 0; i < totalGlaives; i++)
            {
                float offsetAngle = 0f;
                if (i > 0)
                {
                    if (phantomCount == 1)
                    {
                        offsetAngle = 14f;
                    }
                    else
                    {
                        float step = fanSpread / (phantomCount - 1);
                        offsetAngle = -fanSpread * 0.5f + step * (i - 1);
                    }
                }

                float finalAngle = (baseAngle + offsetAngle) * Mathf.Deg2Rad;
                Vector2 dir = new Vector2(Mathf.Cos(finalAngle), Mathf.Sin(finalAngle));
                Vector2 peakPos = origin + dir * (evt.MaxDistance * (i == 0 ? 1.0f : 0.90f));

                var glaive = GetOrCreateGlaive();
                glaive.StartPos = origin;
                glaive.TargetPeakPos = peakPos;
                glaive.OutwardDuration = outwardTime;
                glaive.ReturnDuration = returnTime;
                glaive.ElapsedTime = 0f;
                glaive.Damage = evt.Damage * (i == 0 ? 1.0f : 0.65f);
                glaive.BladeScale = bladeScale;
                glaive.IsReturning = false;
                glaive.RotationAngle = Random.Range(0f, 360f);
                glaive.HitMonstersOutward.Clear();
                glaive.HitMonstersReturn.Clear();

                glaive.GameObject.transform.position = new Vector3(origin.x, origin.y, -0.2f);
                glaive.GameObject.transform.localScale = Vector3.one * (0.70f * bladeScale);
                glaive.Renderer.color = (i == 0) ? Color.white : new Color(0.35f, 1.0f, 0.85f, 0.85f);
                glaive.GameObject.SetActive(true);

                _activeGlaives.Add(glaive);
            }

            CameraFollowView.Instance?.TriggerShake("phantom_glaive", duration: 0.12f, intensity: 0.18f);
        }

        private ActiveGlaive GetOrCreateGlaive()
        {
            if (_pool.Count > 0)
            {
                return _pool.Dequeue();
            }

            var go = new GameObject("WindGlaive_Instance");
            go.transform.SetParent(transform, false);
            go.transform.localScale = Vector3.one * 0.70f; // Compact, balanced scale
            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = SkillSpriteHelper.GetOrCreateWindGlaiveSprite();
            sr.sortingOrder = 18;

            return new ActiveGlaive
            {
                GameObject = go,
                Renderer = sr
            };
        }

        private void RecycleGlaive(ActiveGlaive g)
        {
            if (g?.GameObject != null)
            {
                g.GameObject.SetActive(false);
                _pool.Enqueue(g);
            }
        }
    }
}
