using UnityEngine;

namespace HappyShoot.View.Utils
{
    /// <summary>
    /// Procedural pixel-art sprite generators for enemy projectiles and boss hazard zones.
    /// (Dark Knight Slash Wave, Boss Telegraph Warning Ring, Boss Magma Hazard Ground).
    /// Strictly modular and under 180 lines (500-line architecture rule).
    /// </summary>
    public static class EnemyAttackSpriteHelper
    {
        private static Sprite _darkSlashSprite;
        private static Sprite _warningCircleSprite;
        private static Sprite _hazardMagmaSprite;

        /// <summary>
        /// 28x14 Sharp Neon Purple & Void Violet Crescent Blade projectile for Dark Knight.
        /// </summary>
        public static Sprite GetOrCreateDarkSlashSprite(int width = 28, int height = 14)
        {
            if (_darkSlashSprite != null) return _darkSlashSprite;

            var tex = new Texture2D(width, height, TextureFormat.RGBA32, false) { filterMode = FilterMode.Point };
            Color[] pixels = new Color[width * height];
            for (int i = 0; i < pixels.Length; i++) pixels[i] = Color.clear;

            Color darkCore = new Color(0.95f, 0.70f, 1.00f, 0.98f); // Bright violet-white core
            Color purpleBody = new Color(0.68f, 0.20f, 0.95f, 0.92f); // Deep neon purple
            Color darkEdge = new Color(0.30f, 0.05f, 0.50f, 0.90f); // Dark void rim
            Color glowAura = new Color(0.85f, 0.35f, 1.00f, 0.45f); // Soft outer glow

            int cx = width / 2;
            int cy = height / 2;

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    float nx = (x - cx) / (float)(width * 0.5f); // -1 to 1
                    float ny = (y - cy) / (float)(height * 0.5f); // -1 to 1

                    // Crescent curve equation: x > ny^2
                    float curve = ny * ny * 0.85f;
                    float distFromCurve = (nx - curve);

                    if (distFromCurve >= -0.2f && distFromCurve <= 0.85f && Mathf.Abs(ny) <= 0.92f)
                    {
                        if (distFromCurve >= 0.15f && distFromCurve <= 0.45f && Mathf.Abs(ny) <= 0.6f)
                        {
                            pixels[y * width + x] = darkCore;
                        }
                        else if (distFromCurve >= 0.0f && distFromCurve <= 0.65f)
                        {
                            pixels[y * width + x] = purpleBody;
                        }
                        else
                        {
                            pixels[y * width + x] = darkEdge;
                        }
                    }
                    else if (distFromCurve >= -0.35f && distFromCurve <= 0.95f && Mathf.Abs(ny) <= 0.98f)
                    {
                        pixels[y * width + x] = glowAura;
                    }
                }
            }

            tex.SetPixels(pixels);
            tex.Apply();
            _darkSlashSprite = Sprite.Create(tex, new Rect(0, 0, width, height), new Vector2(0.5f, 0.5f), 16);
            return _darkSlashSprite;
        }

        /// <summary>
        /// 64x64 Glowing Red Circular Warning Ring for Boss AoE telegraph phase.
        /// </summary>
        public static Sprite GetOrCreateWarningCircleSprite(int size = 64)
        {
            if (_warningCircleSprite != null) return _warningCircleSprite;

            var tex = new Texture2D(size, size, TextureFormat.RGBA32, false) { filterMode = FilterMode.Bilinear };
            Color[] pixels = new Color[size * size];
            for (int i = 0; i < pixels.Length; i++) pixels[i] = Color.clear;

            Color ringBorder = new Color(1.0f, 0.20f, 0.15f, 0.95f);
            Color ringGlow = new Color(1.0f, 0.40f, 0.10f, 0.45f);
            Color fillCenter = new Color(0.95f, 0.15f, 0.10f, 0.18f);

            int cx = size / 2;
            int cy = size / 2;
            float maxR = (size * 0.5f) - 1.5f;

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float dist = Mathf.Sqrt((x - cx) * (x - cx) + (y - cy) * (y - cy));
                    float normDist = dist / maxR;

                    if (normDist <= 1.0f)
                    {
                        if (normDist >= 0.88f)
                        {
                            pixels[y * size + x] = ringBorder;
                        }
                        else if (normDist >= 0.76f)
                        {
                            pixels[y * size + x] = ringGlow;
                        }
                        else
                        {
                            pixels[y * size + x] = fillCenter;
                        }
                    }
                }
            }

            tex.SetPixels(pixels);
            tex.Apply();
            _warningCircleSprite = Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), size);
            return _warningCircleSprite;
        }

        /// <summary>
        /// 64x64 Scorched Magma / Hellfire Ground Sprite for Boss active hazard zone.
        /// </summary>
        public static Sprite GetOrCreateHazardMagmaSprite(int size = 64)
        {
            if (_hazardMagmaSprite != null) return _hazardMagmaSprite;

            var tex = new Texture2D(size, size, TextureFormat.RGBA32, false) { filterMode = FilterMode.Bilinear };
            Color[] pixels = new Color[size * size];
            for (int i = 0; i < pixels.Length; i++) pixels[i] = Color.clear;

            Color magmaCore = new Color(1.0f, 0.90f, 0.35f, 0.92f); // Hot yellow core
            Color magmaOrange = new Color(1.0f, 0.45f, 0.05f, 0.85f); // Vivid orange flame
            Color darkCrust = new Color(0.35f, 0.08f, 0.05f, 0.75f); // Charcoal rock crust
            Color outerEdge = new Color(0.85f, 0.15f, 0.05f, 0.40f); // Fiery edge falloff

            int cx = size / 2;
            int cy = size / 2;
            float maxR = (size * 0.5f) - 1.0f;

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float dist = Mathf.Sqrt((x - cx) * (x - cx) + (y - cy) * (y - cy));
                    float normDist = dist / maxR;

                    if (normDist <= 1.0f)
                    {
                        float angle = Mathf.Atan2(y - cy, x - cx);
                        float noise = Mathf.Sin(angle * 6f + dist * 0.5f) * 0.15f;
                        float val = normDist + noise;

                        if (val <= 0.35f)
                        {
                            pixels[y * size + x] = magmaCore;
                        }
                        else if (val <= 0.70f)
                        {
                            pixels[y * size + x] = (noise > 0.04f) ? darkCrust : magmaOrange;
                        }
                        else
                        {
                            pixels[y * size + x] = outerEdge;
                        }
                    }
                }
            }

            tex.SetPixels(pixels);
            tex.Apply();
            _hazardMagmaSprite = Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), size);
            return _hazardMagmaSprite;
        }
    }
}
