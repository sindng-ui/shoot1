using UnityEngine;

namespace HappyShoot.View.Utils
{
    /// <summary>
    /// Generates procedural pixel-art sprites for combat skills, earthquake shockwaves, bone projectiles, and slash arcs.
    /// </summary>
    public static class SkillSpriteHelper
    {
        private static Sprite _slashArcSprite;
        private static Sprite _boneSprite;
        private static Sprite _groundStompSprite;

        /// <summary>
        /// 64x64 Golden Melee Crescent Slash Arc for Warrior Greatsword attack.
        /// </summary>
        public static Sprite GetOrCreateSlashArcSprite(int size = 64)
        {
            if (_slashArcSprite != null) return _slashArcSprite;

            var texture = new Texture2D(size, size, TextureFormat.RGBA32, false);
            texture.filterMode = FilterMode.Bilinear;
            texture.wrapMode = TextureWrapMode.Clamp;

            Vector2 center = new Vector2(size * 0.15f, size * 0.5f);
            float radius = size * 0.75f;
            float innerRadius = size * 0.45f;

            Color[] pixels = new Color[size * size];
            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    Vector2 pos = new Vector2(x + 0.5f, y + 0.5f);
                    float dist = Vector2.Distance(pos, center);
                    float angle = Mathf.Atan2(pos.y - center.y, pos.x - center.x) * Mathf.Rad2Deg;

                    if (dist >= innerRadius && dist <= radius && angle >= -60f && angle <= 60f)
                    {
                        float edgeFade = Mathf.Sin((angle + 60f) / 120f * Mathf.PI);
                        pixels[y * size + x] = new Color(1f, 0.95f, 0.35f, edgeFade * 0.95f);
                    }
                    else
                    {
                        pixels[y * size + x] = Color.clear;
                    }
                }
            }

            texture.SetPixels(pixels);
            texture.Apply();
            _slashArcSprite = Sprite.Create(texture, new Rect(0, 0, size, size), new Vector2(0.15f, 0.5f), size);
            return _slashArcSprite;
        }

        /// <summary>
        /// 24x10 Ivory Flying Bone Arrow projectile sprite for Skeleton Archer.
        /// </summary>
        public static Sprite GetOrCreateBoneSprite()
        {
            if (_boneSprite != null) return _boneSprite;

            int w = 24;
            int h = 10;
            var tex = new Texture2D(w, h, TextureFormat.RGBA32, false);
            tex.filterMode = FilterMode.Point;
            Color[] pixels = new Color[w * h];
            for (int i = 0; i < pixels.Length; i++) pixels[i] = Color.clear;

            Color boneLight = new Color(0.96f, 0.96f, 0.90f, 1f);
            Color boneDark = new Color(0.72f, 0.70f, 0.62f, 1f);

            for (int y = 0; y < h; y++)
            {
                for (int x = 0; x < w; x++)
                {
                    if (x >= 18)
                    {
                        int tipDist = x - 18;
                        int maxDy = 4 - tipDist;
                        if (Mathf.Abs(y - 5) <= maxDy)
                            pixels[y * w + x] = boneLight;
                    }
                    else if (x >= 4 && x <= 17)
                    {
                        if (y >= 3 && y <= 6)
                            pixels[y * w + x] = (y == 3 || y == 6) ? boneDark : boneLight;
                    }
                    else
                    {
                        if ((y >= 1 && y <= 3) || (y >= 6 && y <= 8))
                            pixels[y * w + x] = boneLight;
                    }
                }
            }

            tex.SetPixels(pixels);
            tex.Apply();
            _boneSprite = Sprite.Create(tex, new Rect(0, 0, w, h), new Vector2(0.5f, 0.5f), 16);
            return _boneSprite;
        }

        /// <summary>
        /// 64x64 Earthquake Ground Fracture Shockwave for Warrior Ground Stomp.
        /// </summary>
        public static Sprite GetOrCreateGroundStompSprite()
        {
            if (_groundStompSprite != null) return _groundStompSprite;

            int size = 64;
            var tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            tex.filterMode = FilterMode.Bilinear;
            tex.wrapMode = TextureWrapMode.Clamp;
            Color[] pixels = new Color[size * size];

            float center = size * 0.5f;
            float outerRadius = size * 0.48f;
            float innerRadius = size * 0.20f;
            Color magmaCore = new Color(1.0f, 0.85f, 0.25f, 1f);
            Color magmaOrange = new Color(1.0f, 0.45f, 0.05f, 0.95f);
            Color earthDark = new Color(0.28f, 0.15f, 0.08f, 0.95f);
            Color crackColor = new Color(0.12f, 0.06f, 0.02f, 1f);

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float dx = x - center;
                    float dy = y - center;
                    float dist = Mathf.Sqrt(dx * dx + dy * dy);
                    float angle = Mathf.Atan2(dy, dx);

                    // 8-directional jagged fracture lines
                    float crackNoise = Mathf.Sin(angle * 8f) * 4f + Mathf.Cos(angle * 4f) * 2f;
                    bool isCrack = Mathf.Abs(dist - (size * 0.35f + crackNoise)) < 2.0f && dist <= outerRadius;

                    if (isCrack)
                    {
                        pixels[y * size + x] = Color.Lerp(magmaOrange, crackColor, dist / outerRadius);
                    }
                    else if (dist >= innerRadius && dist <= outerRadius)
                    {
                        float ringT = (dist - innerRadius) / (outerRadius - innerRadius);
                        float alpha = Mathf.Sin(ringT * Mathf.PI);
                        Color ringCol = Color.Lerp(magmaOrange, earthDark, ringT);
                        pixels[y * size + x] = new Color(ringCol.r, ringCol.g, ringCol.b, alpha * 0.95f);
                    }
                    else if (dist < innerRadius)
                    {
                        float coreT = dist / innerRadius;
                        pixels[y * size + x] = Color.Lerp(magmaCore, magmaOrange, coreT);
                    }
                    else
                    {
                        pixels[y * size + x] = Color.clear;
                    }
                }
            }

            tex.SetPixels(pixels);
            tex.Apply();
            _groundStompSprite = Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 16);
            return _groundStompSprite;
        }

        private static Sprite _bloodSpinSprite;
        private static Sprite _bloodOrbSprite;

        /// <summary>
        /// 128x128 360-degree crimson vortex blood slash ring for Blood Eater ultimate skill.
        /// </summary>
        public static Sprite GetOrCreateBloodSpinSprite(int size = 128)
        {
            if (_bloodSpinSprite != null) return _bloodSpinSprite;

            var tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            tex.filterMode = FilterMode.Bilinear;
            tex.wrapMode = TextureWrapMode.Clamp;

            float center = size * 0.5f;
            float outerRadius = size * 0.48f;
            float innerRadius = size * 0.22f;
            Color[] pixels = new Color[size * size];

            Color crimson = new Color(0.95f, 0.1f, 0.15f, 0.95f);
            Color darkBlood = new Color(0.45f, 0.02f, 0.05f, 0.85f);
            Color brightGlow = new Color(1.0f, 0.4f, 0.45f, 1.0f);

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float dx = x - center;
                    float dy = y - center;
                    float dist = Mathf.Sqrt(dx * dx + dy * dy);
                    float angle = Mathf.Atan2(dy, dx);

                    // 4-spiral curved blade streak
                    float swirl = angle * 2.0f + (dist / outerRadius) * Mathf.PI;
                    float swirlVal = Mathf.Sin(swirl) * 0.5f + 0.5f;

                    if (dist >= innerRadius && dist <= outerRadius)
                    {
                        float ringT = (dist - innerRadius) / (outerRadius - innerRadius);
                        float edgeFade = Mathf.Sin(ringT * Mathf.PI);
                        Color col = Color.Lerp(darkBlood, crimson, swirlVal);
                        if (swirlVal > 0.8f) col = Color.Lerp(col, brightGlow, (swirlVal - 0.8f) * 5f);

                        pixels[y * size + x] = new Color(col.r, col.g, col.b, edgeFade * 0.95f);
                    }
                    else
                    {
                        pixels[y * size + x] = Color.clear;
                    }
                }
            }

            tex.SetPixels(pixels);
            tex.Apply();
            _bloodSpinSprite = Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 16);
            return _bloodSpinSprite;
        }

        /// <summary>
        /// 24x24 Glowing red blood droplet/orb for life-steal visual effect.
        /// </summary>
        public static Sprite GetOrCreateBloodOrbSprite(int size = 24)
        {
            if (_bloodOrbSprite != null) return _bloodOrbSprite;

            var tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            tex.filterMode = FilterMode.Bilinear;
            tex.wrapMode = TextureWrapMode.Clamp;

            float center = size * 0.5f;
            float radius = size * 0.45f;
            Color[] pixels = new Color[size * size];

            Color core = new Color(1.0f, 0.7f, 0.7f, 1.0f);
            Color rim = new Color(0.9f, 0.05f, 0.1f, 0.9f);

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float dx = x - center;
                    float dy = y - center;
                    float dist = Mathf.Sqrt(dx * dx + dy * dy);

                    if (dist <= radius)
                    {
                        float t = dist / radius;
                        Color c = Color.Lerp(core, rim, t);
                        float alpha = 1.0f - (t * t);
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
            _bloodOrbSprite = Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 16);
            return _bloodOrbSprite;
        }

        private static Sprite _windGlaiveSprite;

        /// <summary>
        /// 32x32 Cyan-Emerald 3-bladed aerodynamic spinning wind glaive boomerang.
        /// </summary>
        public static Sprite GetOrCreateWindGlaiveSprite(int size = 32)
        {
            if (_windGlaiveSprite != null) return _windGlaiveSprite;

            var tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            tex.filterMode = FilterMode.Bilinear;
            tex.wrapMode = TextureWrapMode.Clamp;

            float center = size * 0.5f;
            float maxR = size * 0.46f;
            Color[] pixels = new Color[size * size];

            Color coreColor = new Color(0.9f, 1.0f, 0.95f, 1.0f);
            Color bladeColor = new Color(0.2f, 0.9f, 0.7f, 0.95f);
            Color edgeColor = new Color(0.05f, 0.5f, 0.4f, 0.8f);

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float dx = x - center;
                    float dy = y - center;
                    float dist = Mathf.Sqrt(dx * dx + dy * dy);
                    float angle = Mathf.Atan2(dy, dx);

                    // 3-bladed aerodynamic shape: cos(3 * theta)
                    float bladeMod = (Mathf.Cos(3f * angle) + 1f) * 0.5f; // 0 to 1
                    float bladeR = maxR * (0.35f + 0.65f * bladeMod);

                    if (dist <= bladeR)
                    {
                        float t = dist / bladeR;
                        Color c = t < 0.3f ? Color.Lerp(coreColor, bladeColor, t / 0.3f) : Color.Lerp(bladeColor, edgeColor, (t - 0.3f) / 0.7f);
                        pixels[y * size + x] = c;
                    }
                    else
                    {
                        pixels[y * size + x] = Color.clear;
                    }
                }
            }

            tex.SetPixels(pixels);
            tex.Apply();
            _windGlaiveSprite = Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 16);
            return _windGlaiveSprite;
        }

        private static Sprite _stormArrowSprite;

        /// <summary>
        /// 28x12 Glowing Cyan Storm Arrow sprite for Blessed Hammer style typhoon spiral.
        /// </summary>
        public static Sprite GetOrCreateStormArrowSprite(int width = 28, int height = 12)
        {
            if (_stormArrowSprite != null) return _stormArrowSprite;

            var tex = new Texture2D(width, height, TextureFormat.RGBA32, false);
            tex.filterMode = FilterMode.Bilinear;
            tex.wrapMode = TextureWrapMode.Clamp;

            Color[] pixels = new Color[width * height];
            Color coreCyan = new Color(0.9f, 1.0f, 1.0f, 1.0f);
            Color bodyCyan = new Color(0.2f, 0.95f, 0.85f, 0.95f);
            Color tipGold = new Color(1.0f, 0.9f, 0.4f, 1.0f);

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    float u = (float)x / width;
                    float v = Mathf.Abs((y + 0.5f) - height * 0.5f) / (height * 0.5f);

                    // Arrowhead shape at front (u > 0.7) and shaft (u <= 0.7)
                    bool isHead = u > 0.65f && v <= (1.0f - (u - 0.65f) / 0.35f);
                    bool isShaft = u <= 0.65f && v <= 0.35f;
                    bool isFletching = u < 0.25f && v <= (0.25f - u) / 0.25f * 0.9f;

                    if (isHead)
                    {
                        pixels[y * width + x] = Color.Lerp(bodyCyan, tipGold, (u - 0.65f) / 0.35f);
                    }
                    else if (isShaft || isFletching)
                    {
                        pixels[y * width + x] = v < 0.15f ? coreCyan : bodyCyan;
                    }
                    else
                    {
                        pixels[y * width + x] = Color.clear;
                    }
                }
            }

            tex.SetPixels(pixels);
            tex.Apply();
            _stormArrowSprite = Sprite.Create(tex, new Rect(0, 0, width, height), new Vector2(0.5f, 0.5f), 16);
            return _stormArrowSprite;
        }

        private static Sprite _stormBlastSprite;

        /// <summary>
        /// 32x32 Glowing Cyan & Electric Blue Impact Shockwave Burst with 8-way sparks for Storm Bow hits.
        /// </summary>
        public static Sprite GetOrCreateStormBlastSprite(int size = 32)
        {
            if (_stormBlastSprite != null) return _stormBlastSprite;

            var tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            tex.filterMode = FilterMode.Bilinear;
            tex.wrapMode = TextureWrapMode.Clamp;

            Vector2 center = new Vector2(size * 0.5f, size * 0.5f);
            float maxR = size * 0.46f;
            float innerR = size * 0.22f;

            Color[] pixels = new Color[size * size];
            Color coreWhite = new Color(1.0f, 1.0f, 1.0f, 1.0f);
            Color brightCyan = new Color(0.2f, 1.0f, 0.95f, 1.0f);
            Color outerElectric = new Color(0.1f, 0.6f, 1.0f, 0.8f);

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    Vector2 pos = new Vector2(x + 0.5f, y + 0.5f);
                    float dist = Vector2.Distance(pos, center);
                    float angle = Mathf.Atan2(pos.y - center.y, pos.x - center.x);

                    // 8-way star spark modulation
                    float spark = Mathf.Abs(Mathf.Cos(angle * 4f));
                    float ringR = Mathf.Lerp(innerR, maxR, spark * 0.6f + 0.4f);

                    if (dist <= innerR)
                    {
                        // White-cyan bright core
                        float t = dist / innerR;
                        pixels[y * size + x] = Color.Lerp(coreWhite, brightCyan, t);
                    }
                    else if (dist <= ringR)
                    {
                        // Expanding shockwave glow
                        float t = (dist - innerR) / (ringR - innerR);
                        pixels[y * size + x] = Color.Lerp(brightCyan, outerElectric, t) * (1.0f - t * 0.5f);
                    }
                    else
                    {
                        pixels[y * size + x] = Color.clear;
                    }
                }
            }

            tex.SetPixels(pixels);
            tex.Apply();
            _stormBlastSprite = Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), size);
            return _stormBlastSprite;
        }
    }
}

