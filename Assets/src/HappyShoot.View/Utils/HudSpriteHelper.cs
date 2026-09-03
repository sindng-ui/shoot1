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

        /// <summary>
        /// 24x24 Crisp Pixel-Art Skull Icon for Kill Count.
        /// </summary>
        public static Sprite GetOrCreateSkullIcon(int size = 24)
        {
            const string key = "hud_skull_icon_24";
            if (_cache.TryGetValue(key, out var cached) && cached != null) return cached;

            var tex = new Texture2D(size, size, TextureFormat.RGBA32, false) { filterMode = FilterMode.Point, wrapMode = TextureWrapMode.Clamp };
            var p = new Color[size * size];
            for (int i = 0; i < p.Length; i++) p[i] = Color.clear;

            Color boneWhite = new Color(0.95f, 0.95f, 0.98f, 1f);
            Color boneShade = new Color(0.68f, 0.72f, 0.82f, 1f);
            Color socketDark = new Color(0.12f, 0.08f, 0.14f, 1f);
            Color outline = new Color(0.10f, 0.04f, 0.08f, 0.95f);
            Color eyeGlow = new Color(1.0f, 0.35f, 0.35f, 1f);

            int cx = size / 2; // 12

            for (int y = 3; y <= 21; y++)
            {
                // Cranium (y: 10..21) vs Jaw (y: 3..9)
                int halfW = (y >= 10) ? (int)Mathf.Sqrt(36f - Mathf.Pow((y - 15) * 1.1f, 2)) + 2 : 4;
                if (halfW < 1) halfW = 1;
                if (halfW > 8) halfW = 8;

                for (int x = cx - halfW; x <= cx + halfW; x++)
                {
                    int dx = x - cx;
                    bool isBorder = (x == cx - halfW || x == cx + halfW || y == 3 || y == 21);

                    // Eye sockets at y: 12..14, dx: -4..-2 and 2..4
                    bool isLeftEye = (y >= 12 && y <= 14 && dx >= -4 && dx <= -2);
                    bool isRightEye = (y >= 12 && y <= 14 && dx >= 2 && dx <= 4);
                    // Nose at y: 10, dx: -1..0
                    bool isNose = (y == 10 && dx >= -1 && dx <= 0);
                    // Teeth slits at y: 4..6, x % 2 == 0
                    bool isTeethGap = (y >= 4 && y <= 6 && (dx == -2 || dx == 0 || dx == 2));

                    if (isBorder)
                    {
                        p[y * size + x] = outline;
                    }
                    else if (isLeftEye || isRightEye)
                    {
                        p[y * size + x] = (y == 13 && (dx == -3 || dx == 3)) ? eyeGlow : socketDark;
                    }
                    else if (isNose || isTeethGap)
                    {
                        p[y * size + x] = socketDark;
                    }
                    else
                    {
                        p[y * size + x] = (y >= 16 && dx <= 0) ? boneWhite : boneShade;
                    }
                }
            }

            tex.SetPixels(p);
            tex.Apply();
            var sprite = Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 16);
            _cache[key] = sprite;
            return sprite;
        }

        /// <summary>
        /// 24x24 Radiant Golden Coin Icon for Gold Earned.
        /// </summary>
        public static Sprite GetOrCreateCoinIcon(int size = 24)
        {
            var custom = CustomResourceSpriteLoader.TryGetGoldCoinSprite();
            if (custom != null) return custom;
            const string key = "hud_coin_icon_24";
            if (_cache.TryGetValue(key, out var cached) && cached != null) return cached;

            var tex = new Texture2D(size, size, TextureFormat.RGBA32, false) { filterMode = FilterMode.Point, wrapMode = TextureWrapMode.Clamp };
            var p = new Color[size * size];
            for (int i = 0; i < p.Length; i++) p[i] = Color.clear;

            Color glint = new Color(1.0f, 1.0f, 0.90f, 1f);
            Color rimBright = new Color(1.0f, 0.90f, 0.35f, 1f);
            Color goldMain = new Color(0.95f, 0.72f, 0.12f, 1f);
            Color goldDark = new Color(0.70f, 0.45f, 0.05f, 1f);
            Color outline = new Color(0.28f, 0.15f, 0.02f, 0.95f);

            int cx = size / 2;
            int cy = size / 2;
            float r = (size / 2f) - 2.5f;

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float dx = x - cx;
                    float dy = y - cy;
                    float dist = Mathf.Sqrt(dx * dx + dy * dy);

                    if (dist > r + 0.5f)
                    {
                        continue;
                    }
                    else if (dist >= r - 1.2f)
                    {
                        p[y * size + x] = outline;
                    }
                    else if (dist >= r - 2.8f)
                    {
                        p[y * size + x] = (dx <= 0 && dy >= 0) ? rimBright : goldDark;
                    }
                    else
                    {
                        // Inner stamp / core
                        if (dx >= -2 && dx <= 2 && dy >= -4 && dy <= 4)
                        {
                            p[y * size + x] = rimBright; // Inner G stamp
                        }
                        else
                        {
                            p[y * size + x] = (dx < 0) ? goldMain : goldDark;
                        }
                    }
                }
            }

            // Specular Glint Dot
            p[(cy + 4) * size + (cx - 4)] = glint;
            p[(cy + 5) * size + (cx - 4)] = glint;
            p[(cy + 4) * size + (cx - 5)] = glint;

            tex.SetPixels(p);
            tex.Apply();
            var sprite = Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 16);
            _cache[key] = sprite;
            return sprite;
        }
 
        /// <summary>
        /// 32x32 Luminous Wind Wing Dash Skill Icon with cyan breeze trails.
        /// </summary>
        public static Sprite GetOrCreateDashSkillIcon(int size = 32)
        {
            const string key = "hud_dash_skill_icon_32";
            if (_cache.TryGetValue(key, out var cached) && cached != null) return cached;

            var tex = new Texture2D(size, size, TextureFormat.RGBA32, false) { filterMode = FilterMode.Point, wrapMode = TextureWrapMode.Clamp };
            var p = new Color[size * size];
            for (int i = 0; i < p.Length; i++) p[i] = Color.clear;

            Color wingCore = new Color(0.95f, 0.98f, 1.0f, 1f);
            Color wingGlow = new Color(0.25f, 0.85f, 1.0f, 1f);
            Color windTrail = new Color(0.10f, 0.55f, 0.95f, 0.75f);
            Color rim = new Color(0.05f, 0.15f, 0.35f, 0.95f);

            int cx = size / 2;
            int cy = size / 2;

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    // Swift wing / boot shape pointing right
                    int dx = x - cx;
                    int dy = y - cy;

                    // Forward wing blade
                    if (dx >= -6 && dx <= 11 && dy >= -4 && dy <= 6)
                    {
                        int slant = dx - (dy / 2);
                        if (slant >= -3 && slant <= 8)
                        {
                            if (slant == 8 || dy == -4 || dy == 6 || slant == -3)
                                p[y * size + x] = rim;
                            else if (slant >= 4)
                                p[y * size + x] = wingCore;
                            else
                                p[y * size + x] = wingGlow;
                        }
                    }

                    // Dynamic wind trails behind
                    if (dx <= -4 && dx >= -12)
                    {
                        if ((dy == -2 || dy == 1 || dy == 4) && (x % 2 == 0))
                        {
                            p[y * size + x] = windTrail;
                        }
                    }
                }
            }

            tex.SetPixels(p);
            tex.Apply();
            var sprite = Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 16);
            _cache[key] = sprite;
            return sprite;
        }
    }
}