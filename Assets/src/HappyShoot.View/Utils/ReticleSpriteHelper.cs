using UnityEngine;

namespace HappyShoot.View.Utils
{
    /// <summary>
    /// Procedural pixel-art sprite generator for the Soulstone-style Neon Lime-Green Aim Reticle.
    /// Strictly modular and under 150 lines (500-line architecture rule).
    /// </summary>
    public static class ReticleSpriteHelper
    {
        private static Sprite _cachedReticleSprite;

        public static Sprite GetOrCreateAimReticleSprite(int size = 48)
        {
            if (_cachedReticleSprite != null)
                return _cachedReticleSprite;

            var tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            tex.filterMode = FilterMode.Point;
            Color[] pixels = new Color[size * size];
            for (int i = 0; i < pixels.Length; i++) pixels[i] = Color.clear;

            int cx = size / 2;
            int cy = size / 2;

            Color neonLime = new Color(0.45f, 1.0f, 0.12f, 0.95f);
            Color limeGlow = new Color(0.75f, 1.0f, 0.35f, 1.0f);
            Color darkOutline = new Color(0.05f, 0.15f, 0.02f, 0.95f);
            Color centerDot = new Color(1.0f, 1.0f, 0.9f, 0.95f);

            // 1. Draw Circular Ring (Radius 10 to 14)
            for (int y = -22; y <= 22; y++)
            {
                for (int x = -22; x <= 22; x++)
                {
                    float dist = Mathf.Sqrt(x * x + y * y);

                    // Outer Ring Outline & Core
                    if (dist >= 8.5f && dist <= 14.5f)
                    {
                        if (dist >= 9.8f && dist <= 13.2f)
                        {
                            SetPixelSafe(pixels, size, cx + x, cy + y, (dist >= 11f && dist <= 12f) ? limeGlow : neonLime);
                        }
                        else
                        {
                            SetPixelSafe(pixels, size, cx + x, cy + y, darkOutline);
                        }
                    }
                }
            }

            // 2. Draw 4 Directional Pointer Arrow Tips (Top, Bottom, Left, Right)
            int[][] directions = new int[][]
            {
                new int[] { 0, 1 },   // Up
                new int[] { 0, -1 },  // Down
                new int[] { -1, 0 },  // Left
                new int[] { 1, 0 }    // Right
            };

            for (int d = 0; d < 4; d++)
            {
                int dx = directions[d][0];
                int dy = directions[d][1];

                for (int step = 11; step <= 21; step++)
                {
                    int px = cx + dx * step;
                    int py = cy + dy * step;
                    int halfWidth = (21 - step) / 2;

                    for (int w = -halfWidth - 1; w <= halfWidth + 1; w++)
                    {
                        int ox = dx == 0 ? px + w : px;
                        int oy = dy == 0 ? py + w : py;

                        if (Mathf.Abs(w) <= halfWidth && step < 21)
                        {
                            SetPixelSafe(pixels, size, ox, oy, step >= 19 ? limeGlow : neonLime);
                        }
                        else
                        {
                            SetPixelSafe(pixels, size, ox, oy, darkOutline);
                        }
                    }
                }
            }

            // 3. Tiny Center Cross Dot
            for (int y = -1; y <= 1; y++)
            {
                for (int x = -1; x <= 1; x++)
                {
                    if (Mathf.Abs(x) + Mathf.Abs(y) <= 1)
                        SetPixelSafe(pixels, size, cx + x, cy + y, centerDot);
                }
            }

            tex.SetPixels(pixels);
            tex.Apply();

            _cachedReticleSprite = Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 32);
            return _cachedReticleSprite;
        }

        private static void SetPixelSafe(Color[] pixels, int size, int x, int y, Color color)
        {
            if (x >= 0 && x < size && y >= 0 && y < size)
            {
                pixels[y * size + x] = color;
            }
        }
    }
}
