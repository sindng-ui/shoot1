using System.Collections.Generic;
using UnityEngine;
using HappyShoot.Domain.Events;
using HappyShoot.View.Utils;

namespace HappyShoot.View.Projectiles
{
    /// <summary>
    /// Visual Manager for Wizard magic spells:
    /// - Frost Nova expanding crystalline frost wave
    /// - Ice Shatter crystalline shard explosion on chilled monster death
    /// - Chain Lightning dynamic zigzag electric bolts
    /// - Fireball splash blast explosions
    /// Uses 100% prewarmed zero-allocation pooling and stays under 500 lines.
    /// </summary>
    public class MagicSkillManagerView : MonoBehaviour
    {
        // 1. Frost Nova Ring Visual
        private class FrostNovaInstance
        {
            public GameObject GameObject;
            public Transform Transform;
            public SpriteRenderer Renderer;
            public float TargetScale;
            public float Timer;
            public float Duration;
            public bool IsActive;
        }

        // 2. Chain Lightning Zigzag Segment Visual
        private class LightningSegment
        {
            public GameObject GameObject;
            public Transform Transform;
            public SpriteRenderer Renderer;
            public float Timer;
            public float Duration;
            public bool IsActive;
        }

        // 3. Fireball Blast Visual
        private class FireballBlastInstance
        {
            public GameObject GameObject;
            public Transform Transform;
            public SpriteRenderer Renderer;
            public float TargetScale;
            public float Timer;
            public float Duration;
            public bool IsActive;
        }

        // 4. Ice Shard Shatter Visual
        private class IceShardInstance
        {
            public GameObject GameObject;
            public Transform Transform;
            public SpriteRenderer Renderer;
            public Vector2 Velocity;
            public float RotSpeed;
            public float Timer;
            public float Lifetime;
            public bool IsActive;
        }

        private const int NovaPoolSize = 12;
        private const int LightningPoolSize = 64;
        private const int BlastPoolSize = 16;
        private const int ShardPoolSize = 36;

        private readonly List<FrostNovaInstance> _novaPool = new List<FrostNovaInstance>(NovaPoolSize);
        private readonly List<LightningSegment> _lightningPool = new List<LightningSegment>(LightningPoolSize);
        private readonly List<FireballBlastInstance> _blastPool = new List<FireballBlastInstance>(BlastPoolSize);
        private readonly List<IceShardInstance> _shardPool = new List<IceShardInstance>(ShardPoolSize);

        private EventBus _eventBus;
        private Sprite _iceShardSprite;

        public void Initialize(EventBus eventBus)
        {
            _eventBus = eventBus;
            _eventBus?.Subscribe<FrostNovaExecutedEvent>(OnFrostNovaExecuted);
            _eventBus?.Subscribe<ChainLightningExecutedEvent>(OnChainLightningExecuted);
            _eventBus?.Subscribe<FireballExplodedEvent>(OnFireballExploded);
            _eventBus?.Subscribe<MonsterShatteredEvent>(OnMonsterShattered);

            PrewarmPools();
        }

        private void PrewarmPools()
        {
            var frostSprite = WizardSpriteHelper.GetOrCreateFrostNovaRingSprite();
            var whiteSprite = SpriteHelper.GetOrCreateWhiteSprite();
            var fireballSprite = WizardSpriteHelper.GetOrCreateFireballSprite();
            _iceShardSprite = WizardSpriteHelper.GetOrCreateIceShardSprite();

            // 1. Frost Nova Pool
            for (int i = 0; i < NovaPoolSize; i++)
            {
                var go = new GameObject($"FrostNova_{i + 1}");
                go.transform.SetParent(transform, false);
                var sr = go.AddComponent<SpriteRenderer>();
                sr.sprite = frostSprite;
                sr.sortingOrder = 4;
                go.SetActive(false);

                _novaPool.Add(new FrostNovaInstance
                {
                    GameObject = go,
                    Transform = go.transform,
                    Renderer = sr,
                    IsActive = false
                });
            }

            // 2. Lightning Pool
            for (int i = 0; i < LightningPoolSize; i++)
            {
                var go = new GameObject($"Lightning_{i + 1}");
                go.transform.SetParent(transform, false);
                var sr = go.AddComponent<SpriteRenderer>();
                sr.sprite = whiteSprite;
                sr.color = new Color(0.9f, 0.98f, 1.0f, 0.95f);
                sr.sortingOrder = 7;
                go.SetActive(false);

                _lightningPool.Add(new LightningSegment
                {
                    GameObject = go,
                    Transform = go.transform,
                    Renderer = sr,
                    IsActive = false
                });
            }

            // 3. Blast Pool
            for (int i = 0; i < BlastPoolSize; i++)
            {
                var go = new GameObject($"FireballBlast_{i + 1}");
                go.transform.SetParent(transform, false);
                var sr = go.AddComponent<SpriteRenderer>();
                sr.sprite = fireballSprite;
                sr.sortingOrder = 5;
                go.SetActive(false);

                _blastPool.Add(new FireballBlastInstance
                {
                    GameObject = go,
                    Transform = go.transform,
                    Renderer = sr,
                    IsActive = false
                });
            }

            // 4. Ice Shard Pool
            for (int i = 0; i < ShardPoolSize; i++)
            {
                var go = new GameObject($"IceShard_{i + 1}");
                go.transform.SetParent(transform, false);
                var sr = go.AddComponent<SpriteRenderer>();
                sr.sprite = _iceShardSprite;
                sr.sortingOrder = 6;
                go.SetActive(false);

                _shardPool.Add(new IceShardInstance
                {
                    GameObject = go,
                    Transform = go.transform,
                    Renderer = sr,
                    IsActive = false
                });
            }
        }

        private void OnFrostNovaExecuted(FrostNovaExecutedEvent e)
        {
            for (int i = 0; i < _novaPool.Count; i++)
            {
                var item = _novaPool[i];
                if (!item.IsActive)
                {
                    item.IsActive = true;
                    item.Timer = 0.40f;
                    item.Duration = 0.40f;
                    item.TargetScale = e.Radius * 2.4f;
                    item.Transform.position = new Vector3((float)e.CenterPosition.X, (float)e.CenterPosition.Y, 0f);
                    item.Transform.localScale = Vector3.zero;
                    item.Renderer.color = new Color(0.4f, 0.9f, 1.0f, 0.95f);
                    item.GameObject.SetActive(true);
                    break;
                }
            }
        }

        private void OnMonsterShattered(MonsterShatteredEvent e)
        {
            Vector2 origin = new Vector2((float)e.Position.X, (float)e.Position.Y);
            int shardsSpawned = 0;

            for (int i = 0; i < _shardPool.Count && shardsSpawned < 8; i++)
            {
                var item = _shardPool[i];
                if (!item.IsActive)
                {
                    item.IsActive = true;
                    item.Transform.position = origin;
                    item.Transform.localScale = Vector3.one * Random.Range(0.6f, 1.1f);
                    item.Timer = 0f;
                    item.Lifetime = Random.Range(0.35f, 0.55f);

                    float angle = Random.Range(0f, 360f) * Mathf.Deg2Rad;
                    float speed = Random.Range(4.5f, 9.0f);
                    item.Velocity = new Vector2(Mathf.Cos(angle) * speed, Mathf.Sin(angle) * speed);
                    item.RotSpeed = Random.Range(-500f, 500f);

                    item.Renderer.color = new Color(0.7f, 0.95f, 1.0f, 1.0f);
                    item.GameObject.SetActive(true);
                    shardsSpawned++;
                }
            }
        }

        private void OnChainLightningExecuted(ChainLightningExecutedEvent e)
        {
            if (e.TargetPositions == null || e.TargetPositions.Count == 0) return;

            Vector3 prevPos = new Vector3((float)e.StartPosition.X, (float)e.StartPosition.Y, 0f);

            for (int t = 0; t < e.TargetPositions.Count; t++)
            {
                Vector3 currentPos = new Vector3((float)e.TargetPositions[t].X, (float)e.TargetPositions[t].Y, 0f);
                SpawnZigzagLightning(prevPos, currentPos);
                prevPos = currentPos;
            }
        }

        private void SpawnZigzagLightning(Vector3 from, Vector3 to)
        {
            Vector3 diff = to - from;
            float totalDist = diff.magnitude;
            if (totalDist < 0.05f) return;

            Vector3 dir = diff.normalized;
            Vector3 perp = new Vector3(-dir.y, dir.x, 0f);

            // Create 3-segment zigzag bolt
            int segments = (totalDist > 3.0f) ? 3 : 2;
            Vector3 cur = from;

            for (int s = 1; s <= segments; s++)
            {
                float t = (float)s / segments;
                Vector3 next = (s == segments) ? to : (Vector3.Lerp(from, to, t) + perp * Random.Range(-0.35f, 0.35f));

                SpawnSingleSegment(cur, next);
                cur = next;
            }
        }

        private void SpawnSingleSegment(Vector3 from, Vector3 to)
        {
            for (int i = 0; i < _lightningPool.Count; i++)
            {
                var item = _lightningPool[i];
                if (!item.IsActive)
                {
                    item.IsActive = true;
                    item.Timer = 0.15f;
                    item.Duration = 0.15f;

                    Vector3 mid = (from + to) * 0.5f;
                    Vector3 diff = to - from;
                    float dist = diff.magnitude;
                    float angle = Mathf.Atan2(diff.y, diff.x) * Mathf.Rad2Deg;

                    item.Transform.position = mid;
                    item.Transform.rotation = Quaternion.Euler(0f, 0f, angle);
                    item.Transform.localScale = new Vector3(dist, 0.12f, 1f);
                    item.Renderer.color = new Color(0.95f, 0.98f, 1.0f, 1.0f);
                    item.GameObject.SetActive(true);
                    break;
                }
            }
        }

        private void OnFireballExploded(FireballExplodedEvent e)
        {
            for (int i = 0; i < _blastPool.Count; i++)
            {
                var item = _blastPool[i];
                if (!item.IsActive)
                {
                    item.IsActive = true;
                    item.Timer = 0.25f;
                    item.Duration = 0.25f;
                    item.TargetScale = e.Radius * 2.0f;
                    item.Transform.position = new Vector3((float)e.CenterPosition.X, (float)e.CenterPosition.Y, 0f);
                    item.Transform.localScale = Vector3.one * 0.3f;
                    item.Renderer.color = new Color(1.0f, 0.8f, 0.2f, 1.0f);
                    item.GameObject.SetActive(true);
                    break;
                }
            }
        }

        private void Update()
        {
            float dt = Time.deltaTime;

            // 1. Update Frost Nova Rings
            for (int i = 0; i < _novaPool.Count; i++)
            {
                var item = _novaPool[i];
                if (!item.IsActive) continue;

                item.Timer -= dt;
                float progress = 1.0f - Mathf.Clamp01(item.Timer / item.Duration);

                item.Transform.localScale = Vector3.one * Mathf.Lerp(0.2f, item.TargetScale, progress);
                item.Transform.Rotate(0f, 0f, dt * 160f);
                item.Renderer.color = new Color(0.4f, 0.9f, 1.0f, (1.0f - progress) * 0.95f);

                if (item.Timer <= 0f)
                {
                    item.IsActive = false;
                    item.GameObject.SetActive(false);
                }
            }

            // 2. Update Lightning Segments
            for (int i = 0; i < _lightningPool.Count; i++)
            {
                var item = _lightningPool[i];
                if (!item.IsActive) continue;

                item.Timer -= dt;
                float progress = 1.0f - Mathf.Clamp01(item.Timer / item.Duration);
                item.Renderer.color = new Color(0.6f, 0.92f, 1.0f, (1.0f - progress));

                if (item.Timer <= 0f)
                {
                    item.IsActive = false;
                    item.GameObject.SetActive(false);
                }
            }

            // 3. Update Fireball Blasts
            for (int i = 0; i < _blastPool.Count; i++)
            {
                var item = _blastPool[i];
                if (!item.IsActive) continue;

                item.Timer -= dt;
                float progress = 1.0f - Mathf.Clamp01(item.Timer / item.Duration);
                item.Transform.localScale = Vector3.one * Mathf.Lerp(0.3f, item.TargetScale, progress);
                item.Renderer.color = new Color(1.0f, Mathf.Lerp(0.8f, 0.2f, progress), 0.1f, (1.0f - progress));

                if (item.Timer <= 0f)
                {
                    item.IsActive = false;
                    item.GameObject.SetActive(false);
                }
            }

            // 4. Update Ice Shards
            for (int i = 0; i < _shardPool.Count; i++)
            {
                var item = _shardPool[i];
                if (!item.IsActive) continue;

                item.Timer += dt;
                item.Transform.position += (Vector3)(item.Velocity * dt);
                item.Velocity *= Mathf.Pow(0.15f, dt);
                item.Transform.Rotate(0f, 0f, item.RotSpeed * dt);

                float t = item.Timer / item.Lifetime;
                float alpha = 1.0f - t;
                item.Renderer.color = new Color(0.7f, 0.95f, 1.0f, alpha);

                if (t >= 1.0f)
                {
                    item.IsActive = false;
                    item.GameObject.SetActive(false);
                }
            }
        }

        private void OnDestroy()
        {
            _eventBus?.Unsubscribe<FrostNovaExecutedEvent>(OnFrostNovaExecuted);
            _eventBus?.Unsubscribe<ChainLightningExecutedEvent>(OnChainLightningExecuted);
            _eventBus?.Unsubscribe<FireballExplodedEvent>(OnFireballExploded);
            _eventBus?.Unsubscribe<MonsterShatteredEvent>(OnMonsterShattered);
        }
    }
}
