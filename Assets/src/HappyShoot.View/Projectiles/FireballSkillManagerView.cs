using System.Collections.Generic;
using UnityEngine;
using HappyShoot.Domain.Events;
using HappyShoot.View.Cameras;
using HappyShoot.View.Monsters;
using HappyShoot.View.Utils;

namespace HappyShoot.View.Projectiles
{
    /// <summary>
    /// Presentation manager for Wizard Fireball primary spell.
    /// Handles flying comet projectiles with fiery tails, incandescent 128x128 explosion nebulae, and radial ember sparks.
    /// Strictly modular and under 500 lines.
    /// </summary>
    public class FireballSkillManagerView : MonoBehaviour
    {
        private struct ActiveFireballComet
        {
            public GameObject Root;
            public SpriteRenderer Renderer;
            public Vector2 StartPos;
            public Vector2 TargetPos;
            public float Speed;
            public float Elapsed;
            public float TotalDuration;
            public float Radius;
            public float Damage;
            public float SparkTimer;
            public bool IsActive;
        }

        private struct ActiveFireballExplosion
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

        private struct ActiveEmberSpark
        {
            public GameObject Root;
            public SpriteRenderer Renderer;
            public Vector2 Position;
            public Vector2 Velocity;
            public float Elapsed;
            public float Lifetime;
            public bool IsActive;
        }

        private const int CometPoolSize = 16;
        private const int ExplosionPoolSize = 16;
        private const int EmberPoolSize = 48;

        private readonly List<ActiveFireballComet> _cometPool = new List<ActiveFireballComet>(CometPoolSize);
        private readonly List<ActiveFireballExplosion> _explosionPool = new List<ActiveFireballExplosion>(ExplosionPoolSize);
        private readonly List<ActiveEmberSpark> _emberPool = new List<ActiveEmberSpark>(EmberPoolSize);
        private readonly List<HappyShoot.Domain.Entities.MonsterEntity> _damageHitBuffer = new List<HappyShoot.Domain.Entities.MonsterEntity>(32);

        private EventBus _eventBus;
        private MonsterSpawnerView _spawnerView;
        private Player.PlayerView _playerView;
        private Sprite _cometSprite;
        private Sprite _explosionSprite;
        private Sprite _emberSprite;
        private Sprite _flashSprite;

        public void Initialize(EventBus eventBus, MonsterSpawnerView spawnerView = null, Player.PlayerView playerView = null)
        {
            _eventBus = eventBus;
            _spawnerView = spawnerView;
            _playerView = playerView;
            _cometSprite = WizardSkillSpriteHelper.GetOrCreateFireballCometSprite();
            _explosionSprite = WizardSkillSpriteHelper.GetOrCreateFireballExplosionSprite();
            _emberSprite = WizardSkillSpriteHelper.GetOrCreateEmberSparkSprite();
            _flashSprite = WizardSkillSpriteHelper.GetOrCreateMuzzleFlashSprite();

            PrewarmPools();

            _eventBus?.Subscribe<FireballLaunchedEvent>(OnFireballLaunched);
            _eventBus?.Subscribe<FireballExplodedEvent>(OnFireballExploded);
        }

        private void PrewarmPools()
        {
            // 1. Comet Projectiles Pool
            for (int i = 0; i < CometPoolSize; i++)
            {
                var go = new GameObject($"FireballComet_{i}");
                go.transform.SetParent(transform, false);
                var sr = go.AddComponent<SpriteRenderer>();
                sr.sprite = _cometSprite;
                sr.sortingOrder = 25;
                go.SetActive(false);

                _cometPool.Add(new ActiveFireballComet { Root = go, Renderer = sr, IsActive = false });
            }

            // 2. Explosion Nebulae Pool
            for (int i = 0; i < ExplosionPoolSize; i++)
            {
                var go = new GameObject($"FireballExplosion_{i}");
                go.transform.SetParent(transform, false);
                var sr = go.AddComponent<SpriteRenderer>();
                sr.sprite = _explosionSprite;
                sr.sortingOrder = 26;
                go.SetActive(false);

                _explosionPool.Add(new ActiveFireballExplosion { Root = go, Renderer = sr, IsActive = false });
            }

            // 3. Ember Sparks Pool
            for (int i = 0; i < EmberPoolSize; i++)
            {
                var go = new GameObject($"FireballEmber_{i}");
                go.transform.SetParent(transform, false);
                var sr = go.AddComponent<SpriteRenderer>();
                sr.sprite = _emberSprite;
                sr.sortingOrder = 27;
                go.SetActive(false);

                _emberPool.Add(new ActiveEmberSpark { Root = go, Renderer = sr, IsActive = false });
            }
        }

        private void OnFireballLaunched(FireballLaunchedEvent evt)
        {
            Vector2 start = new Vector2((float)evt.StartPosition.X, (float)evt.StartPosition.Y);
            Vector2 target = new Vector2((float)evt.TargetPosition.X, (float)evt.TargetPosition.Y);
            float dist = Vector2.Distance(start, target);
            float speed = evt.Speed > 0f ? evt.Speed : 18f;
            float duration = Mathf.Clamp(dist / speed, 0.08f, 0.60f);

            for (int i = 0; i < _cometPool.Count; i++)
            {
                var comet = _cometPool[i];
                if (!comet.IsActive)
                {
                    comet.IsActive = true;
                    comet.StartPos = start;
                    comet.TargetPos = target;
                    comet.Speed = speed;
                    comet.Elapsed = 0f;
                    comet.TotalDuration = duration;
                    comet.Radius = evt.Radius;
                    comet.Damage = evt.Damage;
                    comet.SparkTimer = 0f;

                    Vector2 dir = (target - start).normalized;
                    float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;

                    comet.Root.transform.position = start;
                    comet.Root.transform.rotation = Quaternion.Euler(0f, 0f, angle);
                    comet.Root.transform.localScale = Vector3.one * 1.25f;
                    comet.Renderer.color = Color.white;
                    comet.Root.SetActive(true);
                    _cometPool[i] = comet;
                    break;
                }
            }
        }

        private void OnFireballExploded(FireballExplodedEvent evt)
        {
            // Ignored to avoid duplicate explosion before projectile arrives
        }

        private void SpawnExplosion(Vector2 pos, float radius)
        {
            // 1. Camera Shake
            CameraFollowView.Instance?.TriggerShake("magic", duration: 0.14f, intensity: 0.18f);

            // 2. High-Res Fiery Burst Nebula
            for (int i = 0; i < _explosionPool.Count; i++)
            {
                var exp = _explosionPool[i];
                if (!exp.IsActive)
                {
                    exp.IsActive = true;
                    exp.Position = pos;
                    exp.TargetScale = Mathf.Max(0.8f, radius * 1.05f);
                    exp.RotSpeed = Random.Range(-180f, 180f);
                    exp.Elapsed = 0f;
                    exp.Duration = 0.26f;

                    exp.Root.transform.position = pos;
                    exp.Root.transform.rotation = Quaternion.Euler(0f, 0f, Random.Range(0f, 360f));
                    exp.Root.transform.localScale = Vector3.one * (exp.TargetScale * 0.2f);
                    exp.Renderer.color = Color.white;
                    exp.Root.SetActive(true);
                    _explosionPool[i] = exp;
                    break;
                }
            }

            // 3. Radial Fiery Ember Sparks (8 fragments)
            int spawnedEmbers = 0;
            for (int i = 0; i < _emberPool.Count && spawnedEmbers < 8; i++)
            {
                var ember = _emberPool[i];
                if (!ember.IsActive)
                {
                    ember.IsActive = true;
                    ember.Position = pos;
                    float angle = (spawnedEmbers * 45f + Random.Range(-12f, 12f)) * Mathf.Deg2Rad;
                    float speed = Random.Range(4.5f, 9.0f);
                    ember.Velocity = new Vector2(Mathf.Cos(angle) * speed, Mathf.Sin(angle) * speed);
                    ember.Elapsed = 0f;
                    ember.Lifetime = Random.Range(0.25f, 0.45f);

                    ember.Root.transform.position = pos;
                    ember.Root.transform.localScale = Vector3.one * Random.Range(0.8f, 1.3f);
                    ember.Renderer.color = new Color(1.0f, Random.Range(0.6f, 0.95f), 0.15f, 1f);
                    ember.Root.SetActive(true);
                    _emberPool[i] = ember;
                    spawnedEmbers++;
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
                    ember.Position = pos + Random.insideUnitCircle * 0.15f;
                    float angle = Random.Range(0f, 360f) * Mathf.Deg2Rad;
                    float speed = Random.Range(1.0f, 2.5f);
                    ember.Velocity = new Vector2(Mathf.Cos(angle) * speed, Mathf.Sin(angle) * speed);
                    ember.Elapsed = 0f;
                    ember.Lifetime = Random.Range(0.12f, 0.22f);

                    ember.Root.transform.position = ember.Position;
                    ember.Root.transform.localScale = Vector3.one * 0.75f;
                    ember.Renderer.color = new Color(1.0f, 0.55f, 0.1f, 0.85f);
                    ember.Root.SetActive(true);
                    _emberPool[i] = ember;
                    break;
                }
            }
        }

        private void Update()
        {
            float dt = Time.deltaTime;

            // 1. Update Flying Comets
            for (int i = 0; i < _cometPool.Count; i++)
            {
                var c = _cometPool[i];
                if (!c.IsActive) continue;

                c.Elapsed += dt;
                float progress = Mathf.Clamp01(c.Elapsed / c.TotalDuration);
                Vector2 curPos = Vector2.Lerp(c.StartPos, c.TargetPos, progress);
                c.Root.transform.position = curPos;

                // Spawn trailing ember sparkles
                c.SparkTimer += dt;
                if (c.SparkTimer >= 0.035f)
                {
                    c.SparkTimer = 0f;
                    SpawnFlightEmber(curPos);
                }

                if (progress >= 1.0f)
                {
                    c.IsActive = false;
                    c.Root.SetActive(false);
                    SpawnExplosion(c.TargetPos, c.Radius);
                    ApplyExplosionDamage(c.TargetPos, c.Radius, c.Damage);
                    _eventBus?.Publish(new PlaySoundEvent(SoundEffectType.MagicExplosion, volume: 0.85f));
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

                // Quick expansion followed by smooth fade
                float scale = Mathf.Lerp(exp.TargetScale * 0.2f, exp.TargetScale, Mathf.Sin(progress * Mathf.PI * 0.5f));
                exp.Root.transform.localScale = Vector3.one * scale;
                exp.Root.transform.Rotate(0f, 0f, exp.RotSpeed * dt);

                float alpha = Mathf.Clamp01(1.0f - progress * progress);
                Color col = Color.Lerp(Color.white, new Color(1f, 0.45f, 0.1f, 1f), progress);
                col.a = alpha * 0.9f;
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
                ember.Velocity *= Mathf.Pow(0.18f, dt); // Drag
                ember.Root.transform.position = ember.Position;

                float t = ember.Elapsed / ember.Lifetime;
                Color col = ember.Renderer.color;
                col.a = 1.0f - t;
                ember.Renderer.color = col;
                _emberPool[i] = ember;
            }
        }

        private void ApplyExplosionDamage(Vector2 pos, float radius, float damage)
        {
            if (_spawnerView == null || _spawnerView.MonsterGrid == null) return;

            int hitCount = _spawnerView.MonsterGrid.QueryRadiusNonAlloc(new HappyShoot.Domain.Spatial.Vector2D(pos.x, pos.y), radius, _damageHitBuffer);
            for (int i = 0; i < hitCount; i++)
            {
                if (_damageHitBuffer[i] is HappyShoot.Domain.Entities.MonsterEntity monster && monster.IsActive && !monster.IsDead)
                {
                    monster.ApplyBurn(duration: 7.0f, damagePerTick: damage * 0.12f);
                    var (hitDmg, isCrit) = _playerView != null ? _playerView.Entity.RollDamage(damage) : (damage, false);
                    monster.TakeDamage(hitDmg, isCrit);
                }
            }
        }

        private void OnDestroy()
        {
            _eventBus?.Unsubscribe<FireballLaunchedEvent>(OnFireballLaunched);
            _eventBus?.Unsubscribe<FireballExplodedEvent>(OnFireballExploded);
        }
    }
}
