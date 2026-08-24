using System.Collections.Generic;
using UnityEngine;
using HappyShoot.Domain.Events;
using HappyShoot.Domain.Spatial;

namespace HappyShoot.View.Effects
{
    /// <summary>
    /// Lightweight, zero-allocation visual effects manager for impactful Critical Strike Starburst VFX.
    /// Strictly modular and under 500 lines.
    /// </summary>
    public class CriticalHitVfxManagerView : MonoBehaviour
    {
        private class CritVfxItem
        {
            public GameObject GameObject;
            public Transform Transform;
            public SpriteRenderer Renderer;
            public float RemainingTime;
            public float Duration;
            public float BaseScale;
            public bool IsActive;
        }

        private readonly List<CritVfxItem> _pool = new List<CritVfxItem>(32);
        private EventBus _eventBus;
        private static Sprite _sparkSprite;

        public void Initialize(EventBus eventBus)
        {
            _eventBus = eventBus;
            _eventBus?.Subscribe<MonsterDamagedEvent>(OnMonsterDamaged);

            if (_sparkSprite == null)
            {
                _sparkSprite = CreateCritSparkSprite();
            }

            // Prewarm 32 items
            for (int i = 0; i < 32; i++)
            {
                var item = CreateNewItem(i);
                _pool.Add(item);
            }
        }

        private void OnDestroy()
        {
            _eventBus?.Unsubscribe<MonsterDamagedEvent>(OnMonsterDamaged);
        }

        private void OnMonsterDamaged(MonsterDamagedEvent evt)
        {
            if (evt.IsCritical)
            {
                SpawnCritVfx(evt.Position);

                var expCfg = Config.SkillConfigRepository.Instance.GetConfig()?.Exp;
                if (expCfg == null || expCfg.EnableHitStop)
                {
                    float duration = expCfg != null ? expCfg.HitStopDuration : 0.04f;
                    float slowScale = expCfg != null ? expCfg.HitStopSlowScale : 0.05f;
                    Utils.HitStopManager.Instance?.TriggerHitStop(duration, slowScale);
                }
            }
        }

        public void SpawnCritVfx(Vector2D position)
        {
            CritVfxItem item = null;
            for (int i = 0; i < _pool.Count; i++)
            {
                if (!_pool[i].IsActive)
                {
                    item = _pool[i];
                    break;
                }
            }

            if (item == null)
            {
                if (_pool.Count < 64)
                {
                    item = CreateNewItem(_pool.Count);
                    _pool.Add(item);
                }
                else
                {
                    // Fallback to oldest
                    item = _pool[0];
                }
            }

            item.Transform.position = new Vector3(position.X, position.Y, -0.5f);
            item.Transform.rotation = Quaternion.Euler(0f, 0f, Random.Range(0f, 360f));
            item.Duration = 0.18f;
            item.RemainingTime = item.Duration;
            item.BaseScale = Random.Range(1.8f, 2.3f);
            item.Transform.localScale = Vector3.one * (item.BaseScale * 0.4f);
            item.Renderer.color = new Color(1f, 0.95f, 0.35f, 1f);
            item.GameObject.SetActive(true);
            item.IsActive = true;
        }

        private void Update()
        {
            float dt = Time.deltaTime;
            if (dt <= 0f) return;

            for (int i = 0; i < _pool.Count; i++)
            {
                var item = _pool[i];
                if (!item.IsActive) continue;

                item.RemainingTime -= dt;
                if (item.RemainingTime <= 0f)
                {
                    item.IsActive = false;
                    item.GameObject.SetActive(false);
                    continue;
                }

                float progress = 1f - (item.RemainingTime / item.Duration); // 0 -> 1
                // Fast pop in (0 -> 0.3) then fade out (0.3 -> 1.0)
                float scale = progress < 0.3f
                    ? Mathf.Lerp(item.BaseScale * 0.4f, item.BaseScale * 1.35f, progress / 0.3f)
                    : Mathf.Lerp(item.BaseScale * 1.35f, item.BaseScale * 0.9f, (progress - 0.3f) / 0.7f);

                float alpha = progress < 0.2f ? 1f : Mathf.Lerp(1f, 0f, (progress - 0.2f) / 0.8f);

                item.Transform.localScale = Vector3.one * scale;
                item.Renderer.color = new Color(1f, 0.90f, 0.25f, alpha);
            }
        }

        private CritVfxItem CreateNewItem(int index)
        {
            var go = new GameObject($"CritSpark_{index + 1}");
            go.transform.SetParent(transform, false);
            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = _sparkSprite;
            sr.sortingOrder = 30; // Above monsters and player
            go.SetActive(false);

            return new CritVfxItem
            {
                GameObject = go,
                Transform = go.transform,
                Renderer = sr,
                IsActive = false
            };
        }

        private static Sprite CreateCritSparkSprite()
        {
            int size = 64;
            var tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            tex.filterMode = FilterMode.Bilinear;
            Color[] pixels = new Color[size * size];
            for (int i = 0; i < pixels.Length; i++) pixels[i] = Color.clear;

            int cx = size / 2;
            int cy = size / 2;

            Color coreWhite = Color.white;
            Color neonGold = new Color(1.0f, 0.85f, 0.15f, 1f);
            Color outerOrange = new Color(1.0f, 0.50f, 0.05f, 0.8f);

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    int dx = Mathf.Abs(x - cx);
                    int dy = Mathf.Abs(y - cy);

                    // 1. Primary Cross Beam (Horizontal & Vertical)
                    float crossVal = 0f;
                    if (dx < 3 && dy < 28) crossVal = Mathf.Max(crossVal, 1f - (float)dy / 28f);
                    if (dy < 3 && dx < 28) crossVal = Mathf.Max(crossVal, 1f - (float)dx / 28f);

                    // 2. Secondary Diagonal Spark (X-shape)
                    int diagDist = Mathf.Abs(dx - dy);
                    int maxDist = Mathf.Max(dx, dy);
                    float diagVal = 0f;
                    if (diagDist < 3 && maxDist < 18) diagVal = 1f - (float)maxDist / 18f;

                    // 3. Central Radial Glow
                    float radialDist = Mathf.Sqrt(dx * dx + dy * dy);
                    float radialVal = radialDist <= 7f ? 1f - radialDist / 7f : 0f;

                    float combined = Mathf.Clamp01(crossVal * 1.2f + diagVal * 0.8f + radialVal * 1.5f);
                    if (combined > 0.05f)
                    {
                        if (radialDist <= 2.5f)
                        {
                            pixels[y * size + x] = Color.Lerp(neonGold, coreWhite, 1f - radialDist / 2.5f);
                        }
                        else if (combined > 0.6f)
                        {
                            pixels[y * size + x] = neonGold;
                        }
                        else
                        {
                            Color c = outerOrange;
                            c.a = combined;
                            pixels[y * size + x] = c;
                        }
                    }
                }
            }

            tex.SetPixels(pixels);
            tex.Apply();
            return Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 32);
        }
    }
}
