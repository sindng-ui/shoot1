using System.Collections.Generic;
using UnityEngine;
using HappyShoot.Domain.Events;
using HappyShoot.Domain.Spatial;

namespace HappyShoot.View.Effects
{
    /// <summary>
    /// Lightweight, zero-allocation visual effects manager for crisp, juicy Slash Hit Cut Sparks.
    /// Spawns high-intensity diagonal cutting blade flares (sortingOrder = 32) whenever monsters take damage.
    /// Strictly modular and under 500 lines.
    /// </summary>
    public class SlashHitVfxManagerView : MonoBehaviour
    {
        private class SlashVfxItem
        {
            public GameObject GameObject;
            public Transform Transform;
            public SpriteRenderer Renderer;
            public float RemainingTime;
            public float Duration;
            public float BaseScale;
            public bool IsActive;
        }

        private const int InitialPoolCapacity = 32;
        private const int MaxPoolCapacity = 64;
        private readonly List<SlashVfxItem> _pool = new List<SlashVfxItem>(InitialPoolCapacity);
        private EventBus _eventBus;
        private static Sprite _slashCutSprite;

        public void Initialize(EventBus eventBus)
        {
            _eventBus = eventBus;
            _eventBus?.Subscribe<MonsterDamagedEvent>(OnMonsterDamaged);

            if (_slashCutSprite == null)
            {
                _slashCutSprite = CreateSlashCutSprite();
            }

            // Prewarm 32 items
            for (int i = 0; i < InitialPoolCapacity; i++)
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
            // Spawn crisp diagonal cutting blade spark right on the monster
            SpawnSlashHitVfx(evt.Position, evt.IsCritical);
        }

        public void SpawnSlashHitVfx(Vector2D position, bool isCritical = false)
        {
            SlashVfxItem item = null;
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
                if (_pool.Count < MaxPoolCapacity)
                {
                    item = CreateNewItem(_pool.Count);
                    _pool.Add(item);
                }
                else
                {
                    // Fallback to oldest item in pool
                    item = _pool[0];
                }
            }

            item.Transform.position = new Vector3((float)position.X, (float)position.Y, -0.4f);
            // Random diagonal cut angle (-45 deg +- 35 deg) or reverse slash (+45 deg +- 35 deg)
            float baseAngle = Random.value > 0.5f ? -45f : 45f;
            item.Transform.rotation = Quaternion.Euler(0f, 0f, baseAngle + Random.Range(-25f, 25f));
            
            item.Duration = isCritical ? 0.14f : 0.10f;
            item.RemainingTime = item.Duration;
            item.BaseScale = isCritical ? Random.Range(1.8f, 2.3f) : Random.Range(1.2f, 1.6f);
            item.Transform.localScale = Vector3.one * (item.BaseScale * 0.4f);

            // Radiant gold/amber for critical, sharp energetic steel white/cyan for regular hits
            item.Renderer.color = isCritical 
                ? new Color(1.0f, 0.95f, 0.40f, 1f) 
                : new Color(1.0f, 1.0f, 0.95f, 1f);

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
                // Snappy stretch along X (blade cut direction) and quick fade
                float scaleX = Mathf.Lerp(item.BaseScale * 0.3f, item.BaseScale * 1.4f, progress);
                float scaleY = Mathf.Lerp(item.BaseScale * 1.2f, item.BaseScale * 0.2f, progress);

                float alpha = progress < 0.25f ? 1f : Mathf.Lerp(1f, 0f, (progress - 0.25f) / 0.75f);

                item.Transform.localScale = new Vector3(scaleX, scaleY, 1f);
                Color c = item.Renderer.color;
                c.a = alpha;
                item.Renderer.color = c;
            }
        }

        private SlashVfxItem CreateNewItem(int index)
        {
            var go = new GameObject($"SlashHitSpark_{index + 1}");
            go.transform.SetParent(transform, false);
            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = _slashCutSprite;
            sr.sortingOrder = 32; // On top of monsters (10), player (15), and skills (30)
            go.SetActive(false);

            return new SlashVfxItem
            {
                GameObject = go,
                Transform = go.transform,
                Renderer = sr,
                IsActive = false
            };
        }

        private static Sprite CreateSlashCutSprite()
        {
            int width = 64;
            int height = 64;
            var tex = new Texture2D(width, height, TextureFormat.RGBA32, false);
            tex.filterMode = FilterMode.Bilinear;
            tex.wrapMode = TextureWrapMode.Clamp;

            Color[] pixels = new Color[width * height];
            for (int i = 0; i < pixels.Length; i++) pixels[i] = Color.clear;

            int cx = width / 2;
            int cy = height / 2;

            Color coreWhite = Color.white;
            Color bladeGlowCyan = new Color(0.70f, 0.95f, 1.0f, 0.95f);
            Color outerAura = new Color(0.20f, 0.75f, 1.0f, 0.70f);

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    int dx = Mathf.Abs(x - cx);
                    int dy = Mathf.Abs(y - cy);

                    // 1. Sharp Horizontal Razor Cut Flare along X-axis
                    float cutLen = 28f;
                    float cutThick = 3.5f;
                    float cutVal = 0f;

                    if (dx <= cutLen && dy <= cutThick)
                    {
                        float lenNorm = 1f - (dx / cutLen);
                        float thickNorm = 1f - (dy / cutThick);
                        cutVal = lenNorm * thickNorm;
                    }

                    // 2. Perpendicular Cross Glint along Y-axis (Shorter)
                    float crossLen = 12f;
                    float crossThick = 2.0f;
                    float crossVal = 0f;

                    if (dy <= crossLen && dx <= crossThick)
                    {
                        float lenNorm = 1f - (dy / crossLen);
                        float thickNorm = 1f - (dx / crossThick);
                        crossVal = lenNorm * thickNorm;
                    }

                    // 3. Center Radial Core
                    float dist = Mathf.Sqrt(dx * dx + dy * dy);
                    float radialVal = dist <= 4.5f ? (1f - dist / 4.5f) : 0f;

                    float combined = Mathf.Clamp01(cutVal * 1.4f + crossVal * 0.7f + radialVal * 1.5f);

                    if (combined > 0.05f)
                    {
                        if (dist <= 2.0f || (dx <= 14 && dy <= 1))
                        {
                            pixels[y * width + x] = Color.Lerp(bladeGlowCyan, coreWhite, combined);
                        }
                        else if (combined > 0.5f)
                        {
                            pixels[y * width + x] = bladeGlowCyan;
                        }
                        else
                        {
                            Color c = outerAura;
                            c.a = combined;
                            pixels[y * width + x] = c;
                        }
                    }
                }
            }

            tex.SetPixels(pixels);
            tex.Apply();
            return Sprite.Create(tex, new Rect(0, 0, width, height), new Vector2(0.5f, 0.5f), 32);
        }
    }
}
