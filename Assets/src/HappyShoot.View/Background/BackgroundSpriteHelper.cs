using UnityEngine;

namespace HappyShoot.View.Background
{
    /// <summary>
    /// Generates high-quality procedural 2.5D isometric diamond slate floor tiles and ambient dust particles.
    /// Features seamless diamond tiling (45-degree isometric projection) with 3D depth bevels,
    /// surface variations (Classic, Cracked, Runic, Moss), and zero runtime GC allocation.
    /// Strictly modular and under 500 lines.
    /// </summary>
    public static class BackgroundSpriteHelper
    {
        private const int TileTextureSize = 240; // 240x240 pixels
        private const float PixelsPerUnit = 10f; // 240 px / 24 units = 10 PPU (Matches 24x24m world tile)

        private static Sprite _classicStoneSprite;
        private static Sprite _crackedStoneSprite;
        private static Sprite _runicStoneSprite;
        private static Sprite _mossStoneSprite;
        private static Sprite _dustMoteSprite;

        public static Sprite GetTileSprite(int variationIndex)
        {
            switch (variationIndex % 4)
            {
                case 0: return GetOrCreateClassicStoneSprite();
                case 1: return GetOrCreateCrackedStoneSprite();
                case 2: return GetOrCreateRunicStoneSprite();
                case 3: return GetOrCreateMossStoneSprite();
                default: return GetOrCreateClassicStoneSprite();
            }
        }

        public static Sprite GetOrCreateClassicStoneSprite()
        {
            if (_classicStoneSprite != null) return _classicStoneSprite;
            var tex = GenerateBaseTileTexture(false, false, false);
            _classicStoneSprite = CreateTileSprite(tex);
            return _classicStoneSprite;
        }

        public static Sprite GetOrCreateCrackedStoneSprite()
        {
            if (_crackedStoneSprite != null) return _crackedStoneSprite;
            var tex = GenerateBaseTileTexture(hasCracks: true, false, false);
            _crackedStoneSprite = CreateTileSprite(tex);
            return _crackedStoneSprite;
        }

        public static Sprite GetOrCreateRunicStoneSprite()
        {
            if (_runicStoneSprite != null) return _runicStoneSprite;
            var tex = GenerateBaseTileTexture(false, hasRunes: true, false);
            _runicStoneSprite = CreateTileSprite(tex);
            return _runicStoneSprite;
        }

        public static Sprite GetOrCreateMossStoneSprite()
        {
            if (_mossStoneSprite != null) return _mossStoneSprite;
            var tex = GenerateBaseTileTexture(false, false, hasMoss: true);
            _mossStoneSprite = CreateTileSprite(tex);
            return _mossStoneSprite;
        }

        public static Sprite GetOrCreateDustMoteSprite()
        {
            if (_dustMoteSprite != null) return _dustMoteSprite;

            const int size = 16;
            var tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            tex.filterMode = FilterMode.Bilinear;
            tex.wrapMode = TextureWrapMode.Clamp;

            Color[] pixels = new Color[size * size];
            float center = (size - 1) * 0.5f;

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float dist = Vector2.Distance(new Vector2(x, y), new Vector2(center, center));
                    float t = Mathf.Clamp01(1f - (dist / center));
                    float alpha = t * t * 0.45f;
                    pixels[y * size + x] = new Color(0.85f, 0.92f, 1.0f, alpha);
                }
            }

            tex.SetPixels(pixels);
            tex.Apply();

            _dustMoteSprite = Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 16f);
            return _dustMoteSprite;
        }

        /// <summary>
        /// Generates 2.5D isometric diamond slate tiles with top-down depth shading and 3D bevels.
        /// Seamless repetition is mathematically guaranteed by using period = 40 (divider of 240).
        /// </summary>
        private static Texture2D GenerateBaseTileTexture(bool hasCracks, bool hasRunes, bool hasMoss)
        {
            int size = TileTextureSize;
            var tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            tex.filterMode = FilterMode.Point;
            tex.wrapMode = TextureWrapMode.Repeat;

            Color[] pixels = new Color[size * size];

            // 2.5D Isometric Color Palette (Balanced for shadow contrast)
            Color mortarColor = new Color(0.07f, 0.08f, 0.11f, 1f);      // Deep chiseled groove
            Color stoneBaseA = new Color(0.22f, 0.26f, 0.33f, 1f);       // Primary slate stone
            Color stoneBaseB = new Color(0.25f, 0.29f, 0.37f, 1f);       // Alternating slate stone
            Color topHighlight = new Color(0.36f, 0.42f, 0.52f, 1f);     // 2.5D Top-light bevel rim
            Color bottomShadow = new Color(0.12f, 0.14f, 0.19f, 1f);     // 2.5D Bottom drop-depth shadow

            const int period = 40; // 240 / 40 = 6 diamond waves (Perfect Seamless Repeat)

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    // Diagonal 2.5D isometric diamond coordinates
                    int u = x + y;
                    int v = x - y + 24000;

                    int du = u % period;
                    int dv = v % period;

                    int slabU = u / period;
                    int slabV = v / period;

                    // 1. Mortar seams (Deep grooves between diamond slabs)
                    if (du == 0 || dv == 0)
                    {
                        pixels[y * size + x] = mortarColor;
                        continue;
                    }

                    // 2. 2.5D Depth Bevel: Top Light vs Bottom Shadow
                    // In isometric view, top faces catch light while bottom faces have depth shadow
                    if (du == 1 || dv == 1)
                    {
                        pixels[y * size + x] = topHighlight;
                        continue;
                    }
                    if (du >= period - 2 || dv >= period - 2)
                    {
                        pixels[y * size + x] = bottomShadow;
                        continue;
                    }

                    // 3. Base diamond slab body with subtle checkerboard alternating tone
                    bool altSlab = (slabU + slabV) % 2 == 0;
                    Color slabColor = altSlab ? stoneBaseA : stoneBaseB;

                    // Subtle micro-surface texture noise
                    int hash = (x * 73856093) ^ (y * 19349663) ^ (slabU * 83492791);
                    float noise = ((hash & 0x7FFF) / 32767f - 0.5f) * 0.04f;
                    slabColor.r += noise;
                    slabColor.g += noise;
                    slabColor.b += noise;

                    pixels[y * size + x] = slabColor;
                }
            }

            // 4. Feature details: 2.5D Cracks
            if (hasCracks)
            {
                ApplyCracks(pixels, size);
            }

            // 5. Feature details: Ancient 2.5D Runes
            if (hasRunes)
            {
                ApplyRunes(pixels, size);
            }

            // 6. Feature details: Moss Patches
            if (hasMoss)
            {
                ApplyMoss(pixels, size);
            }

            tex.SetPixels(pixels);
            tex.Apply();
            return tex;
        }

        private static void ApplyCracks(Color[] pixels, int size)
        {
            Color crackColor = new Color(0.04f, 0.05f, 0.07f, 1f);
            Color crackRim = new Color(0.28f, 0.32f, 0.40f, 1f);

            // Diagonal cracks aligned with 2.5D isometric geometry
            DrawJaggedLine(pixels, size, 40, 180, 100, 120, crackColor, crackRim);
            DrawJaggedLine(pixels, size, 100, 120, 160, 140, crackColor, crackRim);
            DrawJaggedLine(pixels, size, 140, 40, 200, 100, crackColor, crackRim);
        }

        private static void DrawJaggedLine(Color[] pixels, int size, int x0, int y0, int x1, int y1, Color mainCol, Color rimCol)
        {
            int steps = Mathf.Max(Mathf.Abs(x1 - x0), Mathf.Abs(y1 - y0));
            if (steps == 0) return;

            for (int i = 0; i <= steps; i++)
            {
                float t = (float)i / steps;
                int x = Mathf.RoundToInt(Mathf.Lerp(x0, x1, t));
                int y = Mathf.RoundToInt(Mathf.Lerp(y0, y1, t));

                int jitterX = ((i * 37) % 3) - 1;
                int jitterY = ((i * 59) % 3) - 1;
                x = Mathf.Clamp(x + jitterX, 0, size - 1);
                y = Mathf.Clamp(y + jitterY, 0, size - 1);

                pixels[y * size + x] = mainCol;
                if (y + 1 < size) pixels[(y + 1) * size + x] = rimCol;
            }
        }

        private static void ApplyRunes(Color[] pixels, int size)
        {
            Color runeGlow = new Color(0.26f, 0.38f, 0.48f, 1f);
            Color runeCore = new Color(0.38f, 0.52f, 0.65f, 1f);

            int cx = size / 2;
            int cy = size / 2;

            // Draw diamond-aligned 2.5D magic circle
            DrawDiamondRuneCircle(pixels, size, cx, cy, 32, runeGlow, runeCore);
        }

        private static void DrawDiamondRuneCircle(Color[] pixels, int size, int cx, int cy, int radius, Color glowCol, Color coreCol)
        {
            for (int angle = 0; angle < 360; angle += 6)
            {
                float rad = angle * Mathf.Deg2Rad;
                int px = Mathf.RoundToInt(cx + Mathf.Cos(rad) * radius);
                // 2.5D Y compression (0.75 ratio for oblique perspective)
                int py = Mathf.RoundToInt(cy + Mathf.Sin(rad) * (radius * 0.75f));

                if (px >= 0 && px < size && py >= 0 && py < size)
                {
                    pixels[py * size + px] = coreCol;
                    if (py + 1 < size) pixels[(py + 1) * size + px] = glowCol;
                }
            }
        }

        private static void ApplyMoss(Color[] pixels, int size)
        {
            Color mossA = new Color(0.16f, 0.26f, 0.18f, 1f);
            Color mossB = new Color(0.22f, 0.34f, 0.24f, 1f);

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    int u = x + y;
                    int v = x - y + 24000;
                    int du = u % 40;
                    int dv = v % 40;

                    // Moss grows along diagonal 2.5D seams
                    if (du <= 2 || dv <= 2)
                    {
                        int hash = (x * 492876847) ^ (y * 265443576);
                        if ((hash & 0xFF) < 55)
                        {
                            pixels[y * size + x] = ((hash & 1) == 0) ? mossA : mossB;
                        }
                    }
                }
            }
        }

        private static Sprite CreateTileSprite(Texture2D tex)
        {
            return Sprite.Create(
                tex,
                new Rect(0, 0, tex.width, tex.height),
                new Vector2(0.5f, 0.5f),
                PixelsPerUnit
            );
        }
    }
}
