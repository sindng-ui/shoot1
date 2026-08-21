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
    }
}
