using UnityEngine;

namespace HappyShoot.View.Utils
{
    /// <summary>
    /// Procedural pixel-art sprite generator dedicated to Wizard combat spells (Fireball Comet, Fiery Explosion Nebula, Ember Sparks).
    /// Modular and strictly under 500 lines.
    /// </summary>
    public static class WizardSkillSpriteHelper
    {
        private static Sprite _fireballCometSprite;
        private static Sprite _fireballExplosionSprite;
        private static Sprite _emberSparkSprite;
        private static Sprite _muzzleFlashSprite;

        /// <summary>
        /// 36x16 Aerodynamic Flying Fireball Comet Projectile.
        /// Features a blinding incandescent white-yellow plasma head and 3 trailing fiery flame tails.
        /// </summary>
        public static Sprite GetOrCreateFireballCometSprite(int width = 36, int height = 16)
        {
            if (_fireballCometSprite != null) return _fireballCometSprite;

            var tex = new Texture2D(width, height, TextureFormat.RGBA32, false);
            tex.filterMode = FilterMode.Bilinear;
            tex.wrapMode = TextureWrapMode.Clamp;

            Color[] pixels = new Color[width * height];
            Color coreWhite = new Color(1.0f, 1.0f, 0.95f, 1f);
            Color plasmaYellow = new Color(1.0f, 0.90f, 0.25f, 0.98f);
            Color flameOrange = new Color(1.0f, 0.50f, 0.08f, 0.90f);
            Color darkCrimson = new Color(0.85f, 0.12f, 0.05f, 0.75f);

            float headCenterX = width * 0.70f;
            float centerY = height * 0.5f;
            float headRadius = height * 0.44f;

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    float dx = x - headCenterX;
                    float dy = y - centerY;
                    float distToHead = Mathf.Sqrt(dx * dx + dy * dy);

                    if (x >= headCenterX)
                    {
                        // Rounded front nose of the fireball comet
                        if (distToHead <= headRadius)
                        {
                            float norm = distToHead / headRadius;
                            Color c = (norm < 0.35f) 
                                ? Color.Lerp(coreWhite, plasmaYellow, norm / 0.35f) 
                                : Color.Lerp(plasmaYellow, flameOrange, (norm - 0.35f) / 0.65f);
                            float alpha = 1.0f - (norm * norm * 0.4f);
                            pixels[y * width + x] = new Color(c.r, c.g, c.b, alpha);
                        }
                        else
                        {
                            pixels[y * width + x] = Color.clear;
                        }
                    }
                    else
                    {
                        // Trailing flame body and 3 wispy tails
                        float u = (float)x / headCenterX; // 0.0 (tail end) -> 1.0 (head)
                        float maxHalfH = headRadius * Mathf.Sqrt(Mathf.Clamp01(u));

                        // 3 wavy flame tails
                        float wave1 = Mathf.Sin(x * 0.45f + y * 0.3f) * 1.5f;
                        float wave2 = Mathf.Cos(x * 0.35f) * 1.2f;
                        float effectiveDy = Mathf.Abs(dy + wave1 * (1f - u));

                        if (effectiveDy <= maxHalfH + wave2 * (1f - u))
                        {
                            float normV = effectiveDy / (maxHalfH + 1e-3f);
                            Color c;
                            if (u > 0.6f && normV < 0.4f)
                            {
                                c = Color.Lerp(plasmaYellow, coreWhite, (u - 0.6f) / 0.4f);
                            }
                            else if (u > 0.3f)
                            {
                                c = Color.Lerp(flameOrange, plasmaYellow, (u - 0.3f) / 0.7f);
                            }
                            else
                            {
                                c = Color.Lerp(darkCrimson, flameOrange, u / 0.3f);
                            }

                            float tailFade = Mathf.Pow(u, 0.7f) * (1.0f - normV * 0.6f);
                            pixels[y * width + x] = new Color(c.r, c.g, c.b, tailFade);
                        }
                        else
                        {
                            pixels[y * width + x] = Color.clear;
                        }
                    }
                }
            }

            tex.SetPixels(pixels);
            tex.Apply();
            _fireballCometSprite = Sprite.Create(tex, new Rect(0, 0, width, height), new Vector2(0.70f, 0.5f), 32);
            return _fireballCometSprite;
        }

        /// <summary>
        /// 128x128 High-Resolution Fiery Plasma Explosion Nebula with 8-way flame spikes and expanding shockwave rim.
        /// </summary>
        public static Sprite GetOrCreateFireballExplosionSprite(int size = 128)
        {
            if (_fireballExplosionSprite != null) return _fireballExplosionSprite;

            var tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            tex.filterMode = FilterMode.Bilinear;
            tex.wrapMode = TextureWrapMode.Clamp;

            Color[] pixels = new Color[size * size];
            Color coreWhite = new Color(1.0f, 1.0f, 0.95f, 1.0f);
            Color plasmaYellow = new Color(1.0f, 0.88f, 0.20f, 0.98f);
            Color blastOrange = new Color(1.0f, 0.48f, 0.08f, 0.88f);
            Color smokeRed = new Color(0.65f, 0.08f, 0.04f, 0.65f);

            float center = size * 0.5f;
            float maxR = size * 0.48f;
            float innerR = size * 0.15f;

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float dx = x - center;
                    float dy = y - center;
                    float dist = Mathf.Sqrt(dx * dx + dy * dy);

                    if (dist > maxR)
                    {
                        pixels[y * size + x] = Color.clear;
                        continue;
                    }

                    float angle = Mathf.Atan2(dy, dx);
                    // 8-directional jagged flame burst modulation
                    float spike = Mathf.Abs(Mathf.Cos(angle * 4f)) * 0.25f + Mathf.Sin(angle * 8f + dist * 0.2f) * 0.12f;
                    float effectiveMaxR = maxR * (0.65f + spike);

                    if (dist <= effectiveMaxR)
                    {
                        float norm = dist / effectiveMaxR;
                        Color col;
                        if (norm <= 0.25f)
                        {
                            // Incandescent plasma core
                            col = Color.Lerp(coreWhite, plasmaYellow, norm / 0.25f);
                        }
                        else if (norm <= 0.65f)
                        {
                            // Fiery churning blast body
                            col = Color.Lerp(plasmaYellow, blastOrange, (norm - 0.25f) / 0.40f);
                        }
                        else
                        {
                            // Outer smoke and embers
                            col = Color.Lerp(blastOrange, smokeRed, (norm - 0.65f) / 0.35f);
                        }

                        float alpha = Mathf.Clamp01((1.0f - norm * norm) * 1.1f);
                        pixels[y * size + x] = new Color(col.r, col.g, col.b, alpha);
                    }
                    else
                    {
                        pixels[y * size + x] = Color.clear;
                    }
                }
            }

            tex.SetPixels(pixels);
            tex.Apply();
            _fireballExplosionSprite = Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 64);
            return _fireballExplosionSprite;
        }

        /// <summary>
        /// 16x16 Glowing 4-way diamond Ember Sparkle sprite.
        /// </summary>
        public static Sprite GetOrCreateEmberSparkSprite(int size = 16)
        {
            if (_emberSparkSprite != null) return _emberSparkSprite;

            var tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            tex.filterMode = FilterMode.Bilinear;
            tex.wrapMode = TextureWrapMode.Clamp;

            Color[] pixels = new Color[size * size];
            Color core = new Color(1.0f, 1.0f, 0.8f, 1.0f);
            Color glow = new Color(1.0f, 0.5f, 0.1f, 0.9f);

            float center = size * 0.5f;
            float maxR = size * 0.45f;

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float dx = Mathf.Abs(x + 0.5f - center);
                    float dy = Mathf.Abs(y + 0.5f - center);
                    // Diamond cross shape: |dx| + |dy| <= maxR
                    float diamondDist = dx + dy;

                    if (diamondDist <= maxR)
                    {
                        float norm = diamondDist / maxR;
                        Color c = Color.Lerp(core, glow, norm);
                        float alpha = 1.0f - norm * norm;
                        pixels[y * size + x] = new Color(c.r, c.g, c.b, alpha);
                    }
                    else
                    {
                        pixels[y * size + x] = Color.clear;
                    }
                }
            }

            tex.SetPixels(pixels);
            tex.Apply();
            _emberSparkSprite = Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 32);
            return _emberSparkSprite;
        }

        /// <summary>
        /// 24x24 Incandescent casting muzzle flash star burst sprite.
        /// </summary>
        public static Sprite GetOrCreateMuzzleFlashSprite(int size = 24)
        {
            if (_muzzleFlashSprite != null) return _muzzleFlashSprite;

            var tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            tex.filterMode = FilterMode.Bilinear;
            tex.wrapMode = TextureWrapMode.Clamp;

            Color[] pixels = new Color[size * size];
            Color coreWhite = new Color(1.0f, 1.0f, 1.0f, 1.0f);
            Color flashGold = new Color(1.0f, 0.75f, 0.2f, 0.9f);

            float center = size * 0.5f;
            float maxR = size * 0.46f;

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float dx = x + 0.5f - center;
                    float dy = y + 0.5f - center;
                    float dist = Mathf.Sqrt(dx * dx + dy * dy);
                    float angle = Mathf.Atan2(dy, dx);

                    // 4 major rays + 4 minor rays
                    float ray = Mathf.Pow(Mathf.Abs(Mathf.Cos(angle * 2f)), 4f) * 0.6f + Mathf.Pow(Mathf.Abs(Mathf.Cos(angle * 4f)), 6f) * 0.4f;
                    float starR = maxR * (0.3f + 0.7f * ray);

                    if (dist <= starR)
                    {
                        float norm = dist / starR;
                        Color c = Color.Lerp(coreWhite, flashGold, norm);
                        float alpha = 1.0f - norm;
                        pixels[y * size + x] = new Color(c.r, c.g, c.b, alpha);
                    }
                    else
                    {
                        pixels[y * size + x] = Color.clear;
                    }
                }
            }

            tex.SetPixels(pixels);
            tex.Apply();
            _muzzleFlashSprite = Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 32);
            return _muzzleFlashSprite;
        }

        private static Sprite _electricSparkSprite;

        /// <summary>
        /// 16x16 Sharp 4-way electric spark glint sprite for lightning hit nodes.
        /// </summary>
        public static Sprite GetOrCreateElectricSparkSprite(int size = 16)
        {
            if (_electricSparkSprite != null) return _electricSparkSprite;

            var tex = new Texture2D(size, size, TextureFormat.RGBA32, false) { filterMode = FilterMode.Point, wrapMode = TextureWrapMode.Clamp };
            Color[] pixels = new Color[size * size];
            Color core = Color.white;
            Color elecYellow = new Color(1.0f, 0.95f, 0.35f, 1.0f);
            Color elecCyan = new Color(0.30f, 0.90f, 1.0f, 0.9f);

            int cx = size / 2, cy = size / 2;
            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    int dx = Mathf.Abs(x - cx);
                    int dy = Mathf.Abs(y - cy);
                    if (dx == 0 && dy <= 6)
                        pixels[y * size + x] = (dy <= 2) ? core : elecYellow;
                    else if (dy == 0 && dx <= 6)
                        pixels[y * size + x] = (dx <= 2) ? core : elecYellow;
                    else if (dx == 1 && dy == 1)
                        pixels[y * size + x] = elecCyan;
                    else
                        pixels[y * size + x] = Color.clear;
                }
            }

            tex.SetPixels(pixels);
            tex.Apply();
            _electricSparkSprite = Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 32);
            return _electricSparkSprite;
        }

        private static Sprite _lightningBeamSprite;

        /// <summary>
        /// 32x16 High-voltage plasma lightning beam sprite with glowing electric falloff.
        /// </summary>
        public static Sprite GetOrCreateLightningBeamSprite(int width = 32, int height = 16)
        {
            if (_lightningBeamSprite != null) return _lightningBeamSprite;

            var tex = new Texture2D(width, height, TextureFormat.RGBA32, false)
            {
                filterMode = FilterMode.Bilinear,
                wrapMode = TextureWrapMode.Clamp
            };

            Color[] pixels = new Color[width * height];
            float centerY = (height - 1) * 0.5f;
            float maxDistY = centerY;

            for (int y = 0; y < height; y++)
            {
                float dy = Mathf.Abs(y - centerY) / maxDistY; // 0.0 at center, 1.0 at edge
                // Solid, dense incandescent core with electric glow falloff
                float coreWeight = Mathf.Clamp01(1.0f - dy * 1.5f);
                float glowWeight = Mathf.Clamp01(1.0f - Mathf.Pow(dy, 1.6f));

                Color col = Color.Lerp(new Color(0.20f, 0.85f, 1.0f, 0.95f), Color.white, coreWeight);
                col.a = glowWeight;

                for (int x = 0; x < width; x++)
                {
                    pixels[y * width + x] = col;
                }
            }

            tex.SetPixels(pixels);
            tex.Apply();
            _lightningBeamSprite = Sprite.Create(tex, new Rect(0, 0, width, height), new Vector2(0.5f, 0.5f), 16);
            return _lightningBeamSprite;
        }
    }
}
