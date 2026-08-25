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
    /// Presentation manager for Evolved Meteor Strike (Inferno Fireball):
    /// 1. Fires 3 massive blazing inferno comets.
    /// 2. Pierces 1 time (explodes on 1st monster collision AND on 2nd collision/destination).
    /// 3. Creates high-res incandescent magma explosions with 7-second burn DoT and camera shake.
    /// Strictly modular and under 500 lines.
    /// </summary>
    public class MeteorStrikeManagerView : MonoBehaviour
    {
        private struct ActiveInfernoComet
        {
            public GameObject Root;
            public SpriteRenderer Renderer;
            public Vector2 StartPos;
            public Vector2 Direction;
            public float Speed;
            public float DistanceTravelled;
            public float MaxDistance;
            public float Radius;
            public float Damage;
            public int PiercesRemaining;
            public float SparkTimer;
            public bool IsActive;
            public HashSet<int> HitMonsterIds;
        }

        private struct ActiveInfernoExplosion
        {
            public GameObject Root;
            public SpriteRenderer Renderer;
            public Vector2 Position;
            public float TargetScale;
            public float RotSpeed;
            public float Elapsed;
            public float Duration;
            public bool IsActive;
        }

        private struct ActiveInfernoEmber
        {
            public GameObject Root;
            public SpriteRenderer Renderer;
            public Vector2 Position;
            public Vector2 Velocity;
            public float Elapsed;
            public float Lifetime;
            public bool IsActive;
        }

        private const int CometPoolSize = 18;
        private const int ExplosionPoolSize = 24;
        private const int EmberPoolSize = 64;

        private readonly List<ActiveInfernoComet> _cometPool = new List<ActiveInfernoComet>(CometPoolSize);
        private readonly List<ActiveInfernoExplosion> _explosionPool = new List<ActiveInfernoExplosion>(ExplosionPoolSize);
        private readonly List<ActiveInfernoEmber> _emberPool = new List<ActiveInfernoEmber>(EmberPoolSize);
        private readonly List<ISpatialEntity> _collisionQueryBuffer = new List<ISpatialEntity>(32);
        private readonly List<ISpatialEntity> _damageHitBuffer = new List<ISpatialEntity>(64);

        private EventBus _eventBus;
        private MonsterSpawnerView _spawnerView;
        private Player.PlayerView _playerView;
        private Sprite _cometSprite;
        private Sprite _explosionSprite;
        private Sprite _emberSprite;

        public void Initialize(EventBus eventBus, MonsterSpawnerView spawnerView = null, Player.PlayerView playerView = null)
        {
            _eventBus = eventBus;
            _spawnerView = spawnerView;
            _playerView = playerView;

            _cometSprite = WizardSkillSpriteHelper.GetOrCreateFireballCometSprite();
            _explosionSprite = WizardSkillSpriteHelper.GetOrCreateFireballExplosionSprite();
            _emberSprite = WizardSkillSpriteHelper.GetOrCreateEmberSparkSprite();

            PrewarmPools();
            _eventBus?.Subscribe<MeteorStrikeLaunchedEvent>(OnMeteorStrikeLaunched);
        }

        private void OnDestroy()
        {
            _eventBus?.Unsubscribe<MeteorStrikeLaunchedEvent>(OnMeteorStrikeLaunched);
        }

        private void PrewarmPools()
        {
            // 1. Comet Projectiles Pool
            for (int i = 0; i < CometPoolSize; i++)
            {
                var go = new GameObject($"InfernoComet_{i}");
                go.transform.SetParent(transform, false);
                var sr = go.AddComponent<SpriteRenderer>();
                sr.sprite = _cometSprite;
                sr.sortingOrder = 28;
                go.SetActive(false);

                _cometPool.Add(new ActiveInfernoComet
                {
                    Root = go,
                    Renderer = sr,
                    IsActive = false,
                    HitMonsterIds = new HashSet<int>()
                });
            }

            // 2. Explosion Pool
            for (int i = 0; i < ExplosionPoolSize; i++)
            {
                var go = new GameObject($"InfernoExplosion_{i}");
                go.transform.SetParent(transform, false);
                var sr = go.AddComponent<SpriteRenderer>();
                sr.sprite = _explosionSprite;
                sr.sortingOrder = 29;
                go.SetActive(false);

                _explosionPool.Add(new ActiveInfernoExplosion { Root = go, Renderer = sr, IsActive = false });
            }

            // 3. Ember Sparks Pool
            for (int i = 0; i < EmberPoolSize; i++)
            {
                var go = new GameObject($"InfernoEmber_{i}");
                go.transform.SetParent(transform, false);
                var sr = go.AddComponent<SpriteRenderer>();
                sr.sprite = _emberSprite;
                sr.sortingOrder = 30;
                go.SetActive(false);

                _emberPool.Add(new ActiveInfernoEmber { Root = go, Renderer = sr, IsActive = false });
            }
        }

        private void OnMeteorStrikeLaunched(MeteorStrikeLaunchedEvent evt)
        {
            Vector2 start = new Vector2((float)evt.StartPosition.X, (float)evt.StartPosition.Y);
            Vector2 target = new Vector2((float)evt.TargetPosition.X, (float)evt.TargetPosition.Y);
            Vector2 dir = (target - start).normalized;
            if (dir.sqrMagnitude < 0.001f) dir = Vector2.right;

            float maxDist = Mathf.Max(Vector2.Distance(start, target) + 4.0f, 10.0f);
            float speed = evt.Speed > 0f ? evt.Speed : 15f;

            for (int i = 0; i < _cometPool.Count; i++)
            {
                var comet = _cometPool[i];
                if (!comet.IsActive)
                {
                    comet.IsActive = true;
                    comet.StartPos = start;
                    comet.Direction = dir;
                    comet.Speed = speed;
                    comet.DistanceTravelled = 0f;
                    comet.MaxDistance = maxDist;
                    comet.Radius = evt.Radius;
                    comet.Damage = evt.Damage;
                    comet.PiercesRemaining = evt.MaxPierces;
                    comet.SparkTimer = 0f;
                    comet.HitMonsterIds.Clear();

                    float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
                    comet.Root.transform.position = start;
                    comet.Root.transform.rotation = Quaternion.Euler(0f, 0f, angle);
                    comet.Root.transform.localScale = Vector3.one * 1.6f; // Giant 1.6x size for Evolved spell
                    comet.Renderer.color = new Color(1.0f, 0.45f, 0.15f, 1.0f); // Fiery orange-red tint
                    comet.Root.SetActive(true);

                    _cometPool[i] = comet;
                    break;
                }
            }
        }

        private void Update()
        {
            float dt = Time.deltaTime;
            ISpatialGrid2D grid = _spawnerView != null ? _spawnerView.MonsterGrid : null;

            // 1. Update Flying Inferno Comets
            for (int i = 0; i < _cometPool.Count; i++)
            {
                var c = _cometPool[i];
                if (!c.IsActive) continue;

                float step = c.Speed * dt;
                c.DistanceTravelled += step;
                Vector2 curPos = c.StartPos + c.Direction * c.DistanceTravelled;
                c.Root.transform.position = curPos;

                // Flight ember trail
                c.SparkTimer += dt;
                if (c.SparkTimer >= 0.025f)
                {
                    c.SparkTimer = 0f;
                    SpawnFlightEmber(curPos);
                }

                // Check collision against monsters
                bool triggerExplosion = false;
                if (grid != null)
                {
                    int found = grid.QueryRadiusNonAlloc(new Vector2D(curPos.x, curPos.y), 0.7f, _collisionQueryBuffer);
                    for (int m = 0; m < found; m++)
                    {
                        if (_collisionQueryBuffer[m] is MonsterEntity monster && monster.IsActive && !monster.IsDead)
                        {
                            if (!c.HitMonsterIds.Contains(monster.Id))
                            {
                                c.HitMonsterIds.Add(monster.Id);
                                triggerExplosion = true;
                                break;
                            }
                        }
                    }
                }

                if (triggerExplosion)
                {
                    // Explode on hit!
                    SpawnExplosion(curPos, c.Radius);
                    ApplyExplosionDamage(curPos, c.Radius, c.Damage);

                    if (c.PiercesRemaining > 0)
                    {
                        // 1st explosion done, continue piercing!
                        c.PiercesRemaining--;
                    }
                    else
                    {
                        // 2nd explosion done, destroy comet!
                        c.IsActive = false;
                        c.Root.SetActive(false);
                    }
                }
                else if (c.DistanceTravelled >= c.MaxDistance)
                {
                    // Reached max range -> Final explosion
                    c.IsActive = false;
                    c.Root.SetActive(false);
                    SpawnExplosion(curPos, c.Radius);
                    ApplyExplosionDamage(curPos, c.Radius, c.Damage);
                }

                _cometPool[i] = c;
            }

            // 2. Update Explosion Nebulae
            for (int i = 0; i < _explosionPool.Count; i++)
            {
                var exp = _explosionPool[i];
                if (!exp.IsActive) continue;

                exp.Elapsed += dt;
                float progress = Mathf.Clamp01(exp.Elapsed / exp.Duration);

                float scale = Mathf.Lerp(exp.TargetScale * 0.2f, exp.TargetScale, Mathf.Sin(progress * Mathf.PI * 0.5f));
                exp.Root.transform.localScale = Vector3.one * scale;
                exp.Root.transform.Rotate(0f, 0f, exp.RotSpeed * dt);

                float alpha = Mathf.Clamp01(1.0f - progress * progress);
                Color col = Color.Lerp(Color.white, new Color(1f, 0.25f, 0.05f, 1f), progress);
                col.a = alpha * 0.95f;
                exp.Renderer.color = col;

                if (exp.Elapsed >= exp.Duration)
                {
                    exp.IsActive = false;
                    exp.Root.SetActive(false);
                }

                _explosionPool[i] = exp;
            }

            // 3. Update Ember Sparks
            for (int i = 0; i < _emberPool.Count; i++)
            {
                var ember = _emberPool[i];
                if (!ember.IsActive) continue;

                ember.Elapsed += dt;
                if (ember.Elapsed >= ember.Lifetime)
                {
                    ember.IsActive = false;
                    ember.Root.SetActive(false);
                    _emberPool[i] = ember;
                    continue;
                }

                ember.Position += ember.Velocity * dt;
                ember.Velocity *= Mathf.Pow(0.20f, dt);
                ember.Root.transform.position = ember.Position;

                float t = ember.Elapsed / ember.Lifetime;
                Color col = ember.Renderer.color;
                col.a = 1.0f - t;
                ember.Renderer.color = col;
                _emberPool[i] = ember;
            }
        }

        private void SpawnExplosion(Vector2 pos, float radius)
        {
            CameraFollowView.Instance?.TriggerShake("meteor_strike", duration: 0.16f, intensity: 0.26f);

            for (int i = 0; i < _explosionPool.Count; i++)
            {
                var exp = _explosionPool[i];
                if (!exp.IsActive)
                {
                    exp.IsActive = true;
                    exp.Position = pos;
                    exp.TargetScale = Mathf.Max(1.0f, radius * 1.15f);
                    exp.RotSpeed = Random.Range(-200f, 200f);
                    exp.Elapsed = 0f;
                    exp.Duration = 0.28f;

                    exp.Root.transform.position = pos;
                    exp.Root.transform.rotation = Quaternion.Euler(0f, 0f, Random.Range(0f, 360f));
                    exp.Root.transform.localScale = Vector3.one * (exp.TargetScale * 0.2f);
                    exp.Renderer.color = Color.white;
                    exp.Root.SetActive(true);
                    _explosionPool[i] = exp;
                    break;
                }
            }

            // 10 radial embers per explosion
            int spawned = 0;
            for (int i = 0; i < _emberPool.Count && spawned < 10; i++)
            {
                var ember = _emberPool[i];
                if (!ember.IsActive)
                {
                    ember.IsActive = true;
                    ember.Position = pos;
                    float angle = (spawned * 36f + Random.Range(-10f, 10f)) * Mathf.Deg2Rad;
                    float speed = Random.Range(5.0f, 10.0f);
                    ember.Velocity = new Vector2(Mathf.Cos(angle) * speed, Mathf.Sin(angle) * speed);
                    ember.Elapsed = 0f;
                    ember.Lifetime = Random.Range(0.28f, 0.48f);

                    ember.Root.transform.position = pos;
                    ember.Root.transform.localScale = Vector3.one * Random.Range(0.9f, 1.4f);
                    ember.Renderer.color = new Color(1.0f, Random.Range(0.4f, 0.85f), 0.1f, 1f);
                    ember.Root.SetActive(true);
                    _emberPool[i] = ember;
                    spawned++;
                }
            }
        }

        private void SpawnFlightEmber(Vector2 pos)
        {
            for (int i = 0; i < _emberPool.Count; i++)
            {
                var ember = _emberPool[i];
                if (!ember.IsActive)
                {
                    ember.IsActive = true;
                    ember.Position = pos + Random.insideUnitCircle * 0.2f;
                    float angle = Random.Range(0f, 360f) * Mathf.Deg2Rad;
                    float speed = Random.Range(1.2f, 3.0f);
                    ember.Velocity = new Vector2(Mathf.Cos(angle) * speed, Mathf.Sin(angle) * speed);
                    ember.Elapsed = 0f;
                    ember.Lifetime = Random.Range(0.14f, 0.24f);

                    ember.Root.transform.position = ember.Position;
                    ember.Root.transform.localScale = Vector3.one * 0.85f;
                    ember.Renderer.color = new Color(1.0f, 0.40f, 0.05f, 0.9f);
                    ember.Root.SetActive(true);
                    _emberPool[i] = ember;
                    break;
                }
            }
        }

        private void ApplyExplosionDamage(Vector2 pos, float radius, float damage)
        {
            ISpatialGrid2D grid = _spawnerView != null ? _spawnerView.MonsterGrid : null;
            if (grid == null) return;

            int hitCount = grid.QueryRadiusNonAlloc(new Vector2D(pos.x, pos.y), radius, _damageHitBuffer);
            for (int i = 0; i < hitCount; i++)
            {
                if (_damageHitBuffer[i] is MonsterEntity monster && monster.IsActive && !monster.IsDead)
                {
                    monster.ApplyBurn(duration: 7.0f, damagePerTick: damage * 0.15f);
                    var (hitDmg, isCrit) = _playerView != null ? _playerView.Entity.RollDamage(damage) : (damage, false);
                    monster.TakeDamage(hitDmg, isCrit);
                }
            }
        }
    }
}
