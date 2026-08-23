using UnityEngine;

namespace HappyShoot.View.Utils
{
    /// <summary>
    /// Generates procedural pixel-art sprites for combat skills, bone projectiles, and redirects warrior sprites to WarriorSkillSpriteHelper.
    /// Strictly under 300 lines for modularity.
    /// </summary>
    public static class SkillSpriteHelper
    {
        private static Sprite _boneSprite;
        private static Sprite _windGlaiveSprite;
        private static Sprite _stormArrowSprite;
        private static Sprite _stormBlastSprite;

        #region Forwarding Wrappers for Warrior Skills
        public static Sprite GetOrCreateSlashArcSprite(int size = 128) => WarriorSkillSpriteHelper.GetOrCreateSlashArcSprite(size);
        public static Sprite GetOrCreateBloodSlashArcSprite(int size = 128) => WarriorSkillSpriteHelper.GetOrCreateBloodSlashArcSprite(size);
        public static Sprite GetOrCreateGroundStompSprite() => WarriorSkillSpriteHelper.GetOrCreateGroundStompSprite();
        public static Sprite GetOrCreateWhirlwindBladeSprite() => WarriorSkillSpriteHelper.GetOrCreateWhirlwindBladeSprite();
        public static Sprite GetOrCreateBloodOrbSprite(int size = 32) => WarriorSkillSpriteHelper.GetOrCreateBloodOrbSprite(size);
        public static Sprite GetOrCreateBloodSpinSprite(int size = 128) => WarriorSkillSpriteHelper.GetOrCreateBloodSlashArcSprite(size);
        #endregion

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

                    float bladeMod = (Mathf.Cos(3f * angle) + 1f) * 0.5f;
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

                    float spark = Mathf.Abs(Mathf.Cos(angle * 4f));
                    float ringR = Mathf.Lerp(innerR, maxR, spark * 0.6f + 0.4f);

                    if (dist <= innerR)
                    {
                        float t = dist / innerR;
                        pixels[y * size + x] = Color.Lerp(coreWhite, brightCyan, t);
                    }
                    else if (dist <= ringR)
                    {
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
