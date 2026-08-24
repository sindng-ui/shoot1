using System.Collections.Generic;
using UnityEngine;

namespace HappyShoot.View.Utils
{
    /// <summary>
    /// Generates high-quality 2D procedural pixel-art sprites for Wizard hero, Arcane Staff, Magic spells, and skill icons.
    /// Keeps file size strictly modular under 500 lines.
    /// </summary>
    public static class WizardSpriteHelper
    {
        private static Sprite _wizardSprite;
        private static Sprite _staffSprite;
        private static Sprite _fireballSprite;
        private static Sprite _frostRingSprite;
        private static Sprite _lightningSprite;
        private static readonly Dictionary<string, Sprite> _magicIconCache = new Dictionary<string, Sprite>(8);

        /// <summary>
        /// 32x32 Chibi Arcane Wizard with deep purple/violet robe, pointed wizard hat with golden buckle, and glowing mystical eyes.
        /// </summary>
        public static Sprite GetOrCreateWizardSprite(int size = 32)
        {
            if (_wizardSprite != null) return _wizardSprite;

            var tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            tex.filterMode = FilterMode.Point;
            Color[] pixels = new Color[size * size];
            for (int i = 0; i < pixels.Length; i++) pixels[i] = Color.clear;

            Color robeLight = new Color(0.62f, 0.35f, 0.88f);
            Color robeMid = new Color(0.42f, 0.18f, 0.65f);
            Color robeDark = new Color(0.24f, 0.08f, 0.40f);
            Color gold = new Color(1.0f, 0.84f, 0.25f);
            Color eyeCyan = new Color(0.30f, 0.95f, 1.0f);
            Color trimGold = new Color(0.95f, 0.75f, 0.20f);

            int cx = size / 2;
            int cy = size / 2;

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    int dx = x - cx;
                    int dy = y - cy;

                    // 1. Flowing Robe Bottom
                    if (Mathf.Abs(dx) <= 7 && dy >= -10 && dy <= -2)
                    {
                        if (dy == -10 || Mathf.Abs(dx) == 7)
                            pixels[y * size + x] = (dy == -10) ? trimGold : robeDark;
                        else
                            pixels[y * size + x] = (dx == 0) ? trimGold : ((Mathf.Abs(dx) <= 4) ? robeLight : robeMid);
                    }

                    // 2. Wizard Belt & Golden Buckle
                    if (Mathf.Abs(dx) <= 5 && dy == -2)
                    {
                        pixels[y * size + x] = (Mathf.Abs(dx) <= 1) ? gold : robeDark;
                    }

                    // 3. Shadowed Face / Head
                    if (Mathf.Abs(dx) <= 5 && dy >= 0 && dy <= 6)
                    {
                        pixels[y * size + x] = new Color(0.12f, 0.08f, 0.20f); // Dark mystical shadow under hat brim
                        // Glowing cyan eyes
                        if (dy == 3 && (dx == -2 || dx == 2))
                        {
                            pixels[y * size + x] = eyeCyan;
                        }
                    }

                    // 4. Pointed Wizard Hat Brim (Wide rim)
                    if (Mathf.Abs(dx) <= 8 && dy == 7)
                    {
                        pixels[y * size + x] = (Mathf.Abs(dx) <= 2) ? gold : robeDark;
                    }

                    // 5. Pointed Wizard Hat Cone
                    if (dy >= 8 && dy <= 14)
                    {
                        int coneWidth = 6 - (dy - 8);
                        if (Mathf.Abs(dx) <= coneWidth)
                        {
                            if (dy == 8) pixels[y * size + x] = gold; // Hat band
                            else if (Mathf.Abs(dx) == coneWidth) pixels[y * size + x] = robeDark;
                            else pixels[y * size + x] = (dx <= 0) ? robeLight : robeMid;
                        }
                    }

                    // 6. Hat Tip Curled Star/Gold
                    if (dx == 1 && dy == 15)
                    {
                        pixels[y * size + x] = gold;
                    }
                }
            }

            tex.SetPixels(pixels);
            tex.Apply();
            _wizardSprite = Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), size);
            return _wizardSprite;
        }

        /// <summary>
        /// 32x32 Golden Arcane Staff with glowing blue-purple crystal orb.
        /// </summary>
        public static Sprite GetOrCreateStaffSprite(int size = 32)
        {
            if (_staffSprite != null) return _staffSprite;

            var tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            tex.filterMode = FilterMode.Point;
            Color[] pixels = new Color[size * size];
            for (int i = 0; i < pixels.Length; i++) pixels[i] = Color.clear;

            Color woodMid = new Color(0.45f, 0.28f, 0.15f);
            Color gold = new Color(1.0f, 0.84f, 0.25f);
            Color gemCore = new Color(0.40f, 0.90f, 1.0f);
            Color gemGlow = new Color(0.70f, 0.40f, 1.0f, 0.8f);

            int cx = size / 2;
            int cy = size / 2;

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    int dx = x - cx;
                    int dy = y - cy;

                    // Staff Shaft (vertical tilted slightly)
                    if (dx == 0 && dy >= -10 && dy <= 5)
                    {
                        pixels[y * size + x] = woodMid;
                    }
                    if (dx == 0 && (dy == -8 || dy == 3))
                    {
                        pixels[y * size + x] = gold; // Gold rings
                    }

                    // Staff Head Golden Claws
                    if ((Mathf.Abs(dx) == 2 && dy >= 6 && dy <= 9) || (dx == 0 && dy == 6))
                    {
                        pixels[y * size + x] = gold;
                    }

                    // Floating Arcane Gem Core
                    if (Mathf.Abs(dx) <= 1 && dy >= 8 && dy <= 10)
                    {
                        pixels[y * size + x] = (dx == 0 && dy == 9) ? Color.white : gemCore;
                    }
                    else if (Mathf.Abs(dx) <= 2 && dy >= 7 && dy <= 11)
                    {
                        if (pixels[y * size + x] == Color.clear)
                            pixels[y * size + x] = gemGlow;
                    }
                }
            }

            tex.SetPixels(pixels);
            tex.Apply();
            _staffSprite = Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), size);
            return _staffSprite;
        }

        /// <summary>
        /// 24x24 Fiery blazing Fireball projectile sprite.
        /// </summary>
        public static Sprite GetOrCreateFireballSprite(int size = 24)
        {
            if (_fireballSprite != null) return _fireballSprite;

            var tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            tex.filterMode = FilterMode.Point;
            Color[] pixels = new Color[size * size];
            for (int i = 0; i < pixels.Length; i++) pixels[i] = Color.clear;

            Color fireWhite = new Color(1.0f, 1.0f, 0.9f);
            Color fireYellow = new Color(1.0f, 0.85f, 0.2f);
            Color fireOrange = new Color(1.0f, 0.45f, 0.1f);
            Color fireRed = new Color(0.85f, 0.15f, 0.1f);

            int cx = size / 2;
            int cy = size / 2;

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float dist = Vector2.Distance(new Vector2(x, y), new Vector2(cx, cy));
                    if (dist <= 3f) pixels[y * size + x] = fireWhite;
                    else if (dist <= 6f) pixels[y * size + x] = fireYellow;
                    else if (dist <= 9f) pixels[y * size + x] = fireOrange;
                    else if (dist <= 11f) pixels[y * size + x] = fireRed;
                }
            }

            tex.SetPixels(pixels);
            tex.Apply();
            _fireballSprite = Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), size);
            return _fireballSprite;
        }

        /// <summary>
        /// 64x64 Frost Nova expanding ice ring sprite.
        /// </summary>
        public static Sprite GetOrCreateFrostNovaRingSprite(int size = 64)
        {
            if (_frostRingSprite != null) return _frostRingSprite;

            var tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            tex.filterMode = FilterMode.Bilinear;
            Color[] pixels = new Color[size * size];
            for (int i = 0; i < pixels.Length; i++) pixels[i] = Color.clear;

            Vector2 center = new Vector2(size * 0.5f, size * 0.5f);
            float outerR = size * 0.48f;
            float innerR = size * 0.36f;

            Color iceCore = new Color(0.9f, 0.98f, 1.0f, 0.95f);
            Color iceBlue = new Color(0.2f, 0.75f, 1.0f, 0.85f);
            Color iceGlow = new Color(0.1f, 0.45f, 0.9f, 0.4f);

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float dist = Vector2.Distance(new Vector2(x + 0.5f, y + 0.5f), center);
                    if (dist >= innerR && dist <= outerR)
                    {
                        float mid = (innerR + outerR) * 0.5f;
                        float t = 1.0f - Mathf.Abs(dist - mid) / ((outerR - innerR) * 0.5f);
                        pixels[y * size + x] = Color.Lerp(iceBlue, iceCore, t);
                    }
                    else if (dist >= innerR - 3f && dist <= outerR + 3f)
                    {
                        pixels[y * size + x] = iceGlow;
                    }
                }
            }

            tex.SetPixels(pixels);
            tex.Apply();
            _frostRingSprite = Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), size);
            return _frostRingSprite;
        }

        /// <summary>
        /// 80x80 High-Res Icons for Wizard Skills: Fireball, Frost Nova, Chain Lightning.
        /// </summary>
        public static Sprite GetOrCreateMagicIcon(string skillId, int size = 80)
        {
            if (_magicIconCache.TryGetValue(skillId, out var cached) && cached != null)
                return cached;

            var tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            tex.filterMode = FilterMode.Point;
            Color[] pixels = new Color[size * size];
            for (int i = 0; i < pixels.Length; i++) pixels[i] = Color.clear;

            int cx = size / 2;
            int cy = size / 2;

            if (skillId == "fireball")
            {
                // Fiery Flaming Meteor / Fireball icon
                for (int y = 0; y < size; y++)
                {
                    for (int x = 0; x < size; x++)
                    {
                        float dist = Vector2.Distance(new Vector2(x, y), new Vector2(cx, cy));
                        if (dist <= 12f) pixels[y * size + x] = new Color(1.0f, 0.95f, 0.6f);
                        else if (dist <= 22f) pixels[y * size + x] = new Color(1.0f, 0.55f, 0.1f);
                        else if (dist <= 30f) pixels[y * size + x] = new Color(0.9f, 0.15f, 0.1f);
                        else if (dist <= 35f && (x + y) % 3 == 0) pixels[y * size + x] = new Color(1.0f, 0.8f, 0.2f, 0.7f);
                    }
                }
            }
            else if (skillId == "frost_nova")
            {
                // Cold Blue Snowflake / Ice Ring icon
                Color iceCyan = new Color(0.35f, 0.90f, 1.0f);
                Color iceWhite = Color.white;
                for (int y = 0; y < size; y++)
                {
                    for (int x = 0; x < size; x++)
                    {
                        int dx = Mathf.Abs(x - cx);
                        int dy = Mathf.Abs(y - cy);
                        if (dx <= 4 || dy <= 4 || dx == dy)
                        {
                            if (dx + dy <= 32) pixels[y * size + x] = (dx <= 1 || dy <= 1) ? iceWhite : iceCyan;
                        }
                    }
                }
            }
            else if (skillId == "chain_lightning")
            {
                // Electric Yellow/Blue Lightning Bolt icon
                Color elecGold = new Color(1.0f, 0.95f, 0.3f);
                Color elecBlue = new Color(0.3f, 0.75f, 1.0f);
                for (int y = 0; y < size; y++)
                {
                    for (int x = 0; x < size; x++)
                    {
                        int progress = y; // 0 to 80
                        int targetX = cx + (int)(Mathf.Sin(progress * 0.15f) * 16f) + (progress > 40 ? 6 : -6);
                        if (Mathf.Abs(x - targetX) <= 3)
                        {
                            pixels[y * size + x] = (Mathf.Abs(x - targetX) <= 1) ? Color.white : ((y > 40) ? elecGold : elecBlue);
                        }
                    }
                }
            }

            else if (skillId == "passive_ignition")
            {
                // Blazing Fire Aura Icon
                Color fireGold = new Color(1.0f, 0.9f, 0.3f);
                Color fireRed = new Color(0.95f, 0.2f, 0.1f);
                for (int y = 0; y < size; y++)
                {
                    for (int x = 0; x < size; x++)
                    {
                        float dist = Vector2.Distance(new Vector2(x, y), new Vector2(cx, cy));
                        if (dist <= 24f && (y >= cy - 10))
                        {
                            pixels[y * size + x] = (dist <= 12f) ? fireGold : fireRed;
                        }
                    }
                }
            }
            else if (skillId == "passive_overcharge")
            {
                // Electric Overcharge Battery/Spark Icon
                Color elecCyan = new Color(0.3f, 0.95f, 1.0f);
                Color elecGold = new Color(1.0f, 0.95f, 0.4f);
                for (int y = 0; y < size; y++)
                {
                    for (int x = 0; x < size; x++)
                    {
                        if (Mathf.Abs(x - cx) <= 16 && Mathf.Abs(y - cy) <= 24)
                        {
                            pixels[y * size + x] = ((x + y) % 6 < 3) ? elecCyan : elecGold;
                        }
                    }
                }
            }

            tex.SetPixels(pixels);
            tex.Apply();
            var sprite = Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), size);
            _magicIconCache[skillId] = sprite;
            return sprite;
        }

        private static Sprite _meteorSprite;
        private static Sprite _iceShardSprite;
        private static Sprite _targetIndicatorSprite;
        private static Sprite _novaFlashSprite;
        private static Sprite _magmaCraterSprite;

        public static Sprite GetOrCreateMeteorSprite(int size = 48)
        {
            if (_meteorSprite != null) return _meteorSprite;
            var tex = new Texture2D(size, size, TextureFormat.RGBA32, false) { filterMode = FilterMode.Point };
            Color[] p = new Color[size * size];
            Color rDark = new Color(0.22f, 0.14f, 0.10f), rMid = new Color(0.55f, 0.28f, 0.14f), magma = new Color(1.0f, 0.85f, 0.25f), flame = new Color(1.0f, 0.40f, 0.1f);
            int cx = size / 2, cy = size / 2;
            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float d = Vector2.Distance(new Vector2(x, y), new Vector2(cx, cy));
                    if (d <= 18f) p[y * size + x] = ((x * 3 + y * 5) % 9 == 0) ? magma : (d <= 11f ? rMid : rDark);
                    else if (d <= 22f && (x + y) % 2 == 0) p[y * size + x] = flame;
                }
            }
            tex.SetPixels(p); tex.Apply();
            return _meteorSprite = Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), size);
        }

        public static Sprite GetOrCreateNovaFlashSprite(int size = 32)
        {
            if (_novaFlashSprite != null) return _novaFlashSprite;
            var tex = new Texture2D(size, size, TextureFormat.RGBA32, false) { filterMode = FilterMode.Bilinear };
            Color[] p = new Color[size * size];
            float c = size * 0.5f;
            Color core = new Color(1f, 1f, 1f, 1f), gold = new Color(1.0f, 0.85f, 0.35f, 0.85f);
            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float dx = Mathf.Abs(x + 0.5f - c), dy = Mathf.Abs(y + 0.5f - c), d = Mathf.Sqrt(dx * dx + dy * dy);
                    float cross = Mathf.Max(0f, 1f - (dx * 0.15f + dy * 1.2f)) + Mathf.Max(0f, 1f - (dx * 1.2f + dy * 0.15f));
                    float diag = Mathf.Max(0f, 1f - (Mathf.Abs(dx - dy) * 0.8f + (dx + dy) * 0.3f));
                    float intensity = Mathf.Clamp01(Mathf.Exp(-d * 0.45f) * 1.5f + cross * 0.8f + diag * 0.4f);
                    p[y * size + x] = (intensity > 0.01f) ? Color.Lerp(gold, core, Mathf.Clamp01(intensity * 1.4f)) * intensity : Color.clear;
                }
            }
            tex.SetPixels(p); tex.Apply();
            return _novaFlashSprite = Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), size);
        }

        public static Sprite GetOrCreateMagmaCraterSprite(int size = 64)
        {
            if (_magmaCraterSprite != null) return _magmaCraterSprite;
            var tex = new Texture2D(size, size, TextureFormat.RGBA32, false) { filterMode = FilterMode.Bilinear };
            Color[] p = new Color[size * size];
            float c = size * 0.5f;
            Color magma = new Color(1.0f, 0.55f, 0.12f, 0.75f), rim = new Color(0.85f, 0.25f, 0.05f, 0.50f);
            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float d = Vector2.Distance(new Vector2(x + 0.5f, y + 0.5f), new Vector2(c, c));
                    if (d <= 28f)
                    {
                        float t = 1.0f - (d / 28f);
                        float crack = (Mathf.Sin(x * 0.8f) * Mathf.Cos(y * 0.8f) > 0.1f) ? 0.3f : 0f;
                        p[y * size + x] = Color.Lerp(rim, magma, t + crack) * Mathf.Clamp01(t * 1.5f);
                    }
                }
            }
            tex.SetPixels(p); tex.Apply();
            return _magmaCraterSprite = Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), size);
        }

        public static Sprite GetOrCreateIceShardSprite(int size = 16)
        {
            if (_iceShardSprite != null) return _iceShardSprite;
            var tex = new Texture2D(size, size, TextureFormat.RGBA32, false) { filterMode = FilterMode.Point };
            Color[] p = new Color[size * size];
            Color iceW = new Color(1f, 1f, 1f, 0.95f), iceC = new Color(0.35f, 0.85f, 1.0f, 0.9f), iceB = new Color(0.12f, 0.55f, 0.95f, 0.8f);
            for (int y = 0; y < size; y++)
                for (int x = 0; x < size; x++)
                    if (Mathf.Abs(x - 8) <= (size - y) / 2) p[y * size + x] = (Mathf.Abs(x - 8) <= 1) ? iceW : ((Mathf.Abs(x - 8) <= 3) ? iceC : iceB);
            tex.SetPixels(p); tex.Apply();
            return _iceShardSprite = Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), size);
        }

        public static Sprite GetOrCreateTargetIndicatorSprite(int size = 128)
        {
            if (_targetIndicatorSprite != null) return _targetIndicatorSprite;
            var tex = new Texture2D(size, size, TextureFormat.RGBA32, false) { filterMode = FilterMode.Bilinear, wrapMode = TextureWrapMode.Clamp };
            float center = size * 0.5f, outerRadius = size * 0.48f, innerRadius = size * 0.44f;
            Color[] pixels = new Color[size * size];
            Color borderGlow = new Color(0.95f, 0.42f, 0.10f, 0.65f), crossCol = new Color(1.0f, 0.68f, 0.22f, 0.70f), innerFill = new Color(0.85f, 0.20f, 0.05f, 0.06f);
            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float dx = x - center, dy = y - center, dist = Mathf.Sqrt(dx * dx + dy * dy);
                    if (dist >= innerRadius && dist <= outerRadius)
                        pixels[y * size + x] = new Color(borderGlow.r, borderGlow.g, borderGlow.b, Mathf.Sin((dist - innerRadius) / (outerRadius - innerRadius) * Mathf.PI) * borderGlow.a);
                    else if (dist <= outerRadius && (Mathf.Abs(dx) < 1.5f || Mathf.Abs(dy) < 1.5f) && dist >= outerRadius * 0.7f)
                        pixels[y * size + x] = crossCol;
                    else if (dist < innerRadius) pixels[y * size + x] = innerFill;
                }
            }
            tex.SetPixels(pixels); tex.Apply();
            return _targetIndicatorSprite = Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), size);
        }
    }
}
