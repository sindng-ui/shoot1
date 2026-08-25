using System.Collections.Generic;
using UnityEngine;

namespace HappyShoot.View.Utils
{
    /// <summary>
    /// Procedural sprite and texture generator for high-quality Soulstone Survivors-style in-game HUD.
    /// Strictly modular and under 500 lines.
    /// </summary>
    public static class HudSpriteHelper
    {
        private static readonly Dictionary<string, Sprite> _cache = new Dictionary<string, Sprite>();

        /// <summary>
        /// 10-segmented EXP bar background with crisp golden/metallic dividers.
        /// </summary>
        public static Sprite GetOrCreateExpBar10SegmentSprite()
        {
            const string key = "hud_exp_bar_10_segment";
            if (_cache.TryGetValue(key, out var cached) && cached != null) return cached;

            int w = 960;
            int h = 24;
            var tex = new Texture2D(w, h, TextureFormat.RGBA32, false)
            {
                filterMode = FilterMode.Bilinear,
                wrapMode = TextureWrapMode.Clamp
            };

            var darkBg = new Color(0.06f, 0.08f, 0.12f, 0.95f);
            var borderGold = new Color(0.85f, 0.68f, 0.25f, 0.90f);
            var dividerGold = new Color(0.95f, 0.80f, 0.35f, 0.85f);
            var innerGlow = new Color(0.12f, 0.16f, 0.24f, 0.95f);

            var pixels = new Color[w * h];
            for (int y = 0; y < h; y++)
            {
                for (int x = 0; x < w; x++)
                {
                    bool isBorder = (y == 0 || y == h - 1 || x == 0 || x == w - 1);
                    bool isDivider = false;

                    // 10 Segments (9 dividers at 10%, 20%, ... 90%)
                    for (int s = 1; s <= 9; s++)
                    {
                        int divX = (w * s) / 10;
                        if (x == divX || x == divX + 1)
                        {
                            isDivider = true;
                            break;
                        }
                    }

                    if (isBorder)
                    {
                        pixels[y * w + x] = borderGold;
                    }
                    else if (isDivider)
                    {
                        pixels[y * w + x] = dividerGold;
                    }
                    else
                    {
                        float vGrad = Mathf.Abs((y - (h * 0.5f)) / (h * 0.5f));
                        pixels[y * w + x] = Color.Lerp(innerGlow, darkBg, vGrad);
                    }
                }
            }

            tex.SetPixels(pixels);
            tex.Apply();

            var sprite = Sprite.Create(tex, new Rect(0, 0, w, h), new Vector2(0.5f, 0.5f), 100f);
            _cache[key] = sprite;
            return sprite;
        }

        /// <summary>
        /// Diamond/Shield shaped golden Level badge sprite.
        /// </summary>
        public static Sprite GetOrCreateLevelBadgeSprite()
        {
            const string key = "hud_level_badge_diamond";
            if (_cache.TryGetValue(key, out var cached) && cached != null) return cached;

            int size = 48;
            var tex = new Texture2D(size, size, TextureFormat.RGBA32, false)
            {
                filterMode = FilterMode.Bilinear,
                wrapMode = TextureWrapMode.Clamp
            };

            var clear = Color.clear;
            var goldEdge = new Color(1.0f, 0.85f, 0.30f, 1f);
            var darkCore = new Color(0.10f, 0.14f, 0.22f, 0.95f);
            var rimHighlight = new Color(0.30f, 0.70f, 0.95f, 1f);

            var pixels = new Color[size * size];
            float c = size * 0.5f;

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float dx = Mathf.Abs(x - c);
                    float dy = Mathf.Abs(y - c);
                    float dist = dx + dy; // Diamond metric (Manhattan distance)

                    if (dist > c - 1f)
                    {
                        pixels[y * size + x] = clear;
                    }
                    else if (dist > c - 4f)
                    {
                        pixels[y * size + x] = goldEdge;
                    }
                    else if (dist > c - 6f)
                    {
                        pixels[y * size + x] = rimHighlight;
                    }
                    else
                    {
                        pixels[y * size + x] = darkCore;
                    }
                }
            }

            tex.SetPixels(pixels);
            tex.Apply();

            var sprite = Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 100f);
            _cache[key] = sprite;
            return sprite;
        }

        /// <summary>
        /// Golden framed square slot for Active Skills and Dash.
        /// </summary>
        public static Sprite GetOrCreateSkillSlotFrameSprite()
        {
            const string key = "hud_skill_slot_frame";
            if (_cache.TryGetValue(key, out var cached) && cached != null) return cached;

            int size = 56;
            var tex = new Texture2D(size, size, TextureFormat.RGBA32, false)
            {
                filterMode = FilterMode.Bilinear,
                wrapMode = TextureWrapMode.Clamp
            };

            var borderGold = new Color(0.88f, 0.72f, 0.30f, 1f);
            var innerBevel = new Color(0.25f, 0.35f, 0.50f, 0.9f);
            var slotDark = new Color(0.08f, 0.10f, 0.15f, 0.95f);

            var pixels = new Color[size * size];
            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    bool isBorder = (x < 2 || x >= size - 2 || y < 2 || y >= size - 2);
                    bool isBevel = (x < 4 || x >= size - 4 || y < 4 || y >= size - 4);

                    if (isBorder)
                    {
                        pixels[y * size + x] = borderGold;
                    }
                    else if (isBevel)
                    {
                        pixels[y * size + x] = innerBevel;
                    }
                    else
                    {
                        pixels[y * size + x] = slotDark;
                    }
                }
            }

            tex.SetPixels(pixels);
            tex.Apply();

            var sprite = Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 100f);
            _cache[key] = sprite;
            return sprite;
        }

        /// <summary>
        /// Warrior / Helmet Horned Crest Emblem for Health Bar center.
        /// </summary>
        public static Sprite GetOrCreateHelmetEmblemSprite()
        {
            const string key = "hud_helmet_emblem";
            if (_cache.TryGetValue(key, out var cached) && cached != null) return cached;

            int w = 64;
            int h = 48;
            var tex = new Texture2D(w, h, TextureFormat.RGBA32, false)
            {
                filterMode = FilterMode.Bilinear,
                wrapMode = TextureWrapMode.Clamp
            };

            var clear = Color.clear;
            var steel = new Color(0.70f, 0.78f, 0.88f, 1f);
            var darkSteel = new Color(0.18f, 0.22f, 0.30f, 1f);
            var gold = new Color(0.95f, 0.80f, 0.25f, 1f);
            var eyeGlow = new Color(0.20f, 0.85f, 1.0f, 1f);

            var pixels = new Color[w * h];
            float cx = w * 0.5f;

            for (int y = 0; y < h; y++)
            {
                for (int x = 0; x < w; x++)
                {
                    float dx = Mathf.Abs(x - cx);
                    // Helmet dome & horned wing silhouette
                    bool isHelmet = (dx < 16f && y >= 8 && y <= 38);
                    bool isHorn = (dx >= 12f && dx <= 26f && y >= 18 && y <= 44 && (y - 18) > (dx - 12f) * 1.5f);
                    bool isBase = (dx < 22f && y >= 4 && y <= 12);

                    if (isHelmet || isHorn || isBase)
                    {
                        bool isBorder = (dx >= 15f && isHelmet) || (dx >= 24f && isHorn) || (y <= 5);
                        bool isSlit = (y >= 20 && y <= 24 && dx >= 3f && dx <= 11f);

                        if (isSlit)
                        {
                            pixels[y * w + x] = eyeGlow;
                        }
                        else if (isBorder)
                        {
                            pixels[y * w + x] = gold;
                        }
                        else if (dx <= 2f)
                        {
                            pixels[y * w + x] = steel;
                        }
                        else
                        {
                            pixels[y * w + x] = darkSteel;
                        }
                    }
                    else
                    {
                        pixels[y * w + x] = clear;
                    }
                }
            }

            tex.SetPixels(pixels);
            tex.Apply();

            var sprite = Sprite.Create(tex, new Rect(0, 0, w, h), new Vector2(0.5f, 0.5f), 100f);
            _cache[key] = sprite;
            return sprite;
        }

        /// <summary>
        /// Wide Health Bar Frame background.
        /// </summary>
        public static Sprite GetOrCreateHpBarFrameSprite()
        {
            const string key = "hud_hp_bar_frame";
            if (_cache.TryGetValue(key, out var cached) && cached != null) return cached;

            int w = 512;
            int h = 32;
            var tex = new Texture2D(w, h, TextureFormat.RGBA32, false)
            {
                filterMode = FilterMode.Bilinear,
                wrapMode = TextureWrapMode.Clamp
            };

            var borderGold = new Color(0.75f, 0.62f, 0.28f, 0.95f);
            var darkSlot = new Color(0.12f, 0.04f, 0.06f, 0.95f);

            var pixels = new Color[w * h];
            for (int y = 0; y < h; y++)
            {
                for (int x = 0; x < w; x++)
                {
                    bool isBorder = (y < 2 || y >= h - 2 || x < 2 || x >= w - 2);
                    pixels[y * w + x] = isBorder ? borderGold : darkSlot;
                }
            }

            tex.SetPixels(pixels);
            tex.Apply();

            var sprite = Sprite.Create(tex, new Rect(0, 0, w, h), new Vector2(0.5f, 0.5f), 100f);
            _cache[key] = sprite;
            return sprite;
        }
    }
}
