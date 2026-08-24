using System.Collections.Generic;
using UnityEngine;
using HappyShoot.Domain.Events;
using HappyShoot.Domain.Spatial;
using HappyShoot.View.Cameras;
using HappyShoot.View.Utils;

namespace HappyShoot.View.Projectiles
{
    /// <summary>
    /// Presentation View managing the visuals and juice for Wizard's magic skills:
    /// - Chain Lightning & Gigastorm: Fractal jagged multi-segment bolts with tapering non-uniform width, electric dual-layer glow, and forked sparks.
    /// - Frost Nova & Blizzard Nova: Double expanding frosty shockwaves and 8-way glacial ice shatters.
    /// Strictly modular and under 500 lines.
    /// </summary>
    public class MagicSkillManagerView : MonoBehaviour
    {
        private const int NovaPoolSize = 12;
        private const int LightningPoolSize = 180;
        private const int ShardPoolSize = 32;
        private const int SparkPoolSize = 24;

        private readonly List<FrostNovaInstance> _novaPool = new List<FrostNovaInstance>(NovaPoolSize);
        private readonly List<LightningSegment> _lightningPool = new List<LightningSegment>(LightningPoolSize);
        private readonly List<IceShardInstance> _shardPool = new List<IceShardInstance>(ShardPoolSize);
        private readonly List<ElectricSparkInstance> _sparkPool = new List<ElectricSparkInstance>(SparkPoolSize);

        private EventBus _eventBus;
        private Sprite _iceShardSprite;
        private Sprite _electricSparkSprite;

        public void Initialize(EventBus eventBus)
        {
            _eventBus = eventBus;
            _eventBus?.Subscribe<FrostNovaExecutedEvent>(OnFrostNovaExecuted);
            _eventBus?.Subscribe<BlizzardNovaExecutedEvent>(OnBlizzardNovaExecuted);
            _eventBus?.Subscribe<ChainLightningExecutedEvent>(OnChainLightningExecuted);
            _eventBus?.Subscribe<GigastormLightningExecutedEvent>(OnGigastormLightningExecuted);
            _eventBus?.Subscribe<MonsterShatteredEvent>(OnMonsterShattered);

            PrewarmPools();
        }

        private void PrewarmPools()
        {
            var frostSprite = WizardSpriteHelper.GetOrCreateFrostNovaRingSprite();
            var lightningBeamSprite = WizardSkillSpriteHelper.GetOrCreateLightningBeamSprite();
            _iceShardSprite = WizardSpriteHelper.GetOrCreateIceShardSprite();
            _electricSparkSprite = WizardSkillSpriteHelper.GetOrCreateElectricSparkSprite();

            // 1. Frost Nova Pool
            for (int i = 0; i < NovaPoolSize; i++)
            {
                var go = new GameObject($"FrostNova_{i + 1}");
                go.transform.SetParent(transform, false);
                var sr = go.AddComponent<SpriteRenderer>();
                sr.sprite = frostSprite;
                sr.sortingOrder = 4;
                go.SetActive(false);
                _novaPool.Add(new FrostNovaInstance { GameObject = go, Transform = go.transform, Renderer = sr, IsActive = false });
            }

            // 2. Lightning Beam Pool (High-voltage plasma beam texture)
            for (int i = 0; i < LightningPoolSize; i++)
            {
                var go = new GameObject($"Lightning_{i + 1}");
                go.transform.SetParent(transform, false);
                var sr = go.AddComponent<SpriteRenderer>();
                sr.sprite = lightningBeamSprite;
                sr.sortingOrder = 7;
                go.SetActive(false);
                _lightningPool.Add(new LightningSegment { GameObject = go, Transform = go.transform, Renderer = sr, IsActive = false });
            }

            // 3. Ice Shard Pool (Frost only)
            for (int i = 0; i < ShardPoolSize; i++)
            {
                var go = new GameObject($"IceShard_{i + 1}");
                go.transform.SetParent(transform, false);
                var sr = go.AddComponent<SpriteRenderer>();
                sr.sprite = _iceShardSprite;
                sr.sortingOrder = 6;
                go.SetActive(false);
                _shardPool.Add(new IceShardInstance { GameObject = go, Transform = go.transform, Renderer = sr, IsActive = false });
            }

            // 4. Electric Spark Pool (Lightning only)
            for (int i = 0; i < SparkPoolSize; i++)
            {
                var go = new GameObject($"ElecSpark_{i + 1}");
                go.transform.SetParent(transform, false);
                var sr = go.AddComponent<SpriteRenderer>();
                sr.sprite = _electricSparkSprite;
                sr.sortingOrder = 9;
                go.SetActive(false);
                _sparkPool.Add(new ElectricSparkInstance { GameObject = go, Transform = go.transform, Renderer = sr, IsActive = false });
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

            CameraFollowView.Instance?.TriggerShake("frost_nova", duration: 0.14f, intensity: 0.18f);
        }

        private void OnBlizzardNovaExecuted(BlizzardNovaExecutedEvent e)
        {
            // 1. Double Expanding Glacial Waves
            int spawnedWaves = 0;
            for (int i = 0; i < _novaPool.Count && spawnedWaves < 2; i++)
            {
                var item = _novaPool[i];
                if (!item.IsActive)
                {
                    item.IsActive = true;
                    item.Timer = 0.45f + spawnedWaves * 0.10f;
                    item.Duration = 0.45f + spawnedWaves * 0.10f;
                    item.TargetScale = e.Radius * (2.2f + spawnedWaves * 0.4f);
                    item.Transform.position = new Vector3((float)e.CenterPosition.X, (float)e.CenterPosition.Y, 0f);
                    item.Transform.localScale = Vector3.zero;
                    item.Renderer.color = (spawnedWaves == 0) ? new Color(0.3f, 0.9f, 1.0f, 1.0f) : new Color(0.7f, 1.0f, 1.0f, 0.85f);
                    item.GameObject.SetActive(true);
                    _novaPool[i] = item;
                    spawnedWaves++;
                }
            }

            // 2. Radial Glacial Ice Shards (8 shards)
            Vector2 origin = new Vector2((float)e.CenterPosition.X, (float)e.CenterPosition.Y);
            int shardsSpawned = 0;
            for (int i = 0; i < _shardPool.Count && shardsSpawned < e.ShardCount; i++)
            {
                var item = _shardPool[i];
                if (!item.IsActive)
                {
                    item.IsActive = true;
                    item.Transform.position = origin;
                    item.Transform.localScale = Vector3.one * 1.3f;
                    item.Timer = 0f;
                    item.Lifetime = 0.48f;

                    float angle = (shardsSpawned * (360f / e.ShardCount) + Random.Range(-5f, 5f)) * Mathf.Deg2Rad;
                    float speed = Random.Range(7.0f, 10.5f);
                    item.Velocity = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * speed;
                    item.RotSpeed = Random.Range(-500f, 500f);

                    item.Renderer.color = new Color(0.85f, 1.0f, 1.0f, 1.0f);
                    item.GameObject.SetActive(true);
                    _shardPool[i] = item;
                    shardsSpawned++;
                }
            }

            CameraFollowView.Instance?.TriggerShake("blizzard_nova", duration: 0.18f, intensity: 0.25f);
        }

        private void OnGigastormLightningExecuted(GigastormLightningExecutedEvent e)
        {
            if (e.TargetPositions == null || e.TargetPositions.Count == 0) return;

            Vector3 prevPos = new Vector3((float)e.StartPosition.X, (float)e.StartPosition.Y, 0f);

            for (int t = 0; t < e.TargetPositions.Count; t++)
            {
                Vector3 currentPos = new Vector3((float)e.TargetPositions[t].X, (float)e.TargetPositions[t].Y, 0f);
                // 1. Crisp, razor-sharp high-voltage thunderbolt
                SpawnGigastormLightning(prevPos, currentPos);
                // 2. Subtle electric spark glint at struck node (No messy ice shards!)
                SpawnGigastormNodeSpark(currentPos);
                prevPos = currentPos;
            }

            CameraFollowView.Instance?.TriggerShake("gigastorm_lightning", duration: 0.15f, intensity: 0.22f);
        }

        private void SpawnGigastormLightning(Vector3 from, Vector3 to)
        {
            Vector3 diff = to - from;
            float totalDist = diff.magnitude;
            if (totalDist < 0.05f) return;

            Vector3 dir = diff.normalized;
            Vector3 perp = new Vector3(-dir.y, dir.x, 0f);

            int segments = Mathf.Clamp(Mathf.RoundToInt(totalDist * 2.6f), 4, 7);
            Vector3 cur = from;

            for (int s = 1; s <= segments; s++)
            {
                float t = (float)s / segments;
                float jitter = (s == segments) ? 0f : Random.Range(-0.35f, 0.35f) * Mathf.Sin(t * Mathf.PI);
                Vector3 next = (s == segments) ? to : (Vector3.Lerp(from, to, t) + perp * jitter);

                // Heavy, thick high-voltage lightning pillar (0.75m -> 0.45m)
                float baseWidth = Mathf.Lerp(0.72f, 0.42f, t);

                // Layer 1: Outer Heavy Plasma Aura (Order = 7)
                SpawnSingleSegment(cur, next, baseWidth * 1.5f, new Color(0.12f, 0.80f, 1.0f, 0.95f), 7, duration: 0.13f);

                // Layer 2: Inner Blinding High-Voltage Core (Order = 8)
                SpawnSingleSegment(cur, next, baseWidth * 0.70f, new Color(1.0f, 1.0f, 0.95f, 1.0f), 8, duration: 0.13f);

                cur = next;
            }
        }

        private void SpawnGigastormNodeSpark(Vector3 pos)
        {
            int spawned = 0;
            for (int i = 0; i < _sparkPool.Count && spawned < 3; i++)
            {
                var item = _sparkPool[i];
                if (!item.IsActive)
                {
                    item.IsActive = true;
                    item.Transform.position = pos;
                    item.Transform.localScale = Vector3.one * Random.Range(0.9f, 1.4f);
                    item.Timer = 0f;
                    item.Lifetime = 0.12f;

                    float angle = (spawned * 120f + Random.Range(-20f, 20f)) * Mathf.Deg2Rad;
                    float speed = Random.Range(2.5f, 5.0f);
                    item.Velocity = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * speed;

                    item.Renderer.color = (spawned % 2 == 0) ? new Color(1.0f, 0.95f, 0.35f, 1f) : new Color(0.30f, 0.90f, 1.0f, 1f);
                    item.GameObject.SetActive(true);
                    _sparkPool[i] = item;
                    spawned++;
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
                    float speed = Random.Range(3.5f, 8.0f);
                    item.Velocity = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * speed;
                    item.RotSpeed = Random.Range(-400f, 400f);

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
                SpawnFractalLightning(prevPos, currentPos);
                prevPos = currentPos;
            }

            CameraFollowView.Instance?.TriggerShake("chain_lightning", duration: 0.10f, intensity: 0.14f);
        }

        private void SpawnFractalLightning(Vector3 from, Vector3 to)
        {
            Vector3 diff = to - from;
            float totalDist = diff.magnitude;
            if (totalDist < 0.05f) return;

            Vector3 dir = diff.normalized;
            Vector3 perp = new Vector3(-dir.y, dir.x, 0f);

            int segments = Mathf.Clamp(Mathf.RoundToInt(totalDist * 2.8f), 4, 7);
            Vector3 cur = from;

            for (int s = 1; s <= segments; s++)
            {
                float t = (float)s / segments;
                float jitter = (s == segments) ? 0f : Random.Range(-0.28f, 0.28f) * Mathf.Sin(t * Mathf.PI);
                Vector3 next = (s == segments) ? to : (Vector3.Lerp(from, to, t) + perp * jitter);

                // Standard chain lightning width (0.34m -> 0.16m)
                float baseWidth = Mathf.Lerp(0.34f, 0.16f, t);

                SpawnSingleSegment(cur, next, baseWidth * 1.4f, new Color(0.15f, 0.85f, 1.0f, 0.85f), 7, duration: 0.12f);
                SpawnSingleSegment(cur, next, baseWidth * 0.65f, Color.white, 8, duration: 0.12f);

                cur = next;
            }
        }

        private void SpawnSingleSegment(Vector3 from, Vector3 to, float width, Color color, int sortOrder, float duration = 0.14f)
        {
            for (int i = 0; i < _lightningPool.Count; i++)
            {
                var item = _lightningPool[i];
                if (!item.IsActive)
                {
                    item.IsActive = true;
                    item.Timer = duration;
                    item.Duration = duration;
                    item.BaseWidth = width;
                    item.BaseColor = color;

                    Vector3 mid = (from + to) * 0.5f;
                    Vector3 diff = to - from;
                    float dist = diff.magnitude;
                    float angle = Mathf.Atan2(diff.y, diff.x) * Mathf.Rad2Deg;

                    item.Transform.position = mid;
                    item.Transform.rotation = Quaternion.Euler(0f, 0f, angle);
                    item.Transform.localScale = new Vector3(dist, width, 1f);
                    item.Renderer.color = color;
                    item.Renderer.sortingOrder = sortOrder;
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

            // 2. Update Lightning Segments with Electric Flicker & Fast Fade
            for (int i = 0; i < _lightningPool.Count; i++)
            {
                var item = _lightningPool[i];
                if (!item.IsActive) continue;

                item.Timer -= dt;
                float progress = 1.0f - Mathf.Clamp01(item.Timer / item.Duration);
                float alpha = (1.0f - progress);

                // Electric flicker jitter
                float flicker = Random.Range(0.85f, 1.15f);
                item.Renderer.color = new Color(item.BaseColor.r, item.BaseColor.g, item.BaseColor.b, alpha * item.BaseColor.a * flicker);

                if (item.Timer <= 0f)
                {
                    item.IsActive = false;
                    item.GameObject.SetActive(false);
                }
            }

            // 3. Update Ice Shards (Glacial)
            for (int i = 0; i < _shardPool.Count; i++)
            {
                var item = _shardPool[i];
                if (!item.IsActive) continue;

                item.Timer += dt;
                item.Transform.position += (Vector3)(item.Velocity * dt);
                item.Velocity *= Mathf.Pow(0.15f, dt);
                item.Transform.Rotate(0f, 0f, item.RotSpeed * dt);

                float t = item.Timer / item.Lifetime;
                item.Renderer.color = new Color(0.7f, 0.95f, 1.0f, 1.0f - t);
                if (t >= 1.0f) { item.IsActive = false; item.GameObject.SetActive(false); }
            }

            // 4. Update Electric Sparks (Lightning)
            for (int i = 0; i < _sparkPool.Count; i++)
            {
                var item = _sparkPool[i];
                if (!item.IsActive) continue;

                item.Timer += dt;
                item.Transform.position += (Vector3)(item.Velocity * dt);
                item.Velocity *= Mathf.Pow(0.10f, dt);

                float t = item.Timer / item.Lifetime;
                Color col = item.Renderer.color;
                col.a = 1.0f - t;
                item.Renderer.color = col;

                if (t >= 1.0f) { item.IsActive = false; item.GameObject.SetActive(false); }
            }
        }

        private void OnDestroy()
        {
            _eventBus?.Unsubscribe<FrostNovaExecutedEvent>(OnFrostNovaExecuted);
            _eventBus?.Unsubscribe<BlizzardNovaExecutedEvent>(OnBlizzardNovaExecuted);
            _eventBus?.Unsubscribe<ChainLightningExecutedEvent>(OnChainLightningExecuted);
            _eventBus?.Unsubscribe<GigastormLightningExecutedEvent>(OnGigastormLightningExecuted);
            _eventBus?.Unsubscribe<MonsterShatteredEvent>(OnMonsterShattered);
        }
    }
}
