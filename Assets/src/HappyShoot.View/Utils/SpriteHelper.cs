using UnityEngine;

namespace HappyShoot.View.Utils
{
    /// <summary>
    /// Generates beautiful 2D retro procedural pixel-art sprites for player characters, weapons, and gems.
    /// </summary>
    public static class SpriteHelper
    {
        private static Sprite _warriorSprite;
        private static Sprite _swordSprite;
        private static Sprite _slimeSprite;
        private static Sprite _gemSprite;
        private static Sprite _slashArcSprite;

        /// <summary>
        /// 32x32 Chibi Armored Warrior Knight with steel helmet, visor slit, shoulder pads, and red cape.
        /// </summary>
        public static Sprite GetOrCreateWarriorSprite(int size = 32)
        {
            if (_warriorSprite != null) return _warriorSprite;

            var tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            tex.filterMode = FilterMode.Point;
            Color[] pixels = new Color[size * size];
            for (int i = 0; i < pixels.Length; i++) pixels[i] = Color.clear;

            Color steelLight = new Color(0.85f, 0.88f, 0.92f);
            Color steelMid = new Color(0.55f, 0.60f, 0.68f);
            Color steelDark = new Color(0.25f, 0.28f, 0.35f);
            Color goldTrim = new Color(0.95f, 0.78f, 0.25f);
            Color capeRed = new Color(0.85f, 0.22f, 0.25f);
            Color eyeGlow = new Color(0.30f, 0.85f, 1.0f);
            Color leather = new Color(0.45f, 0.28f, 0.18f);

            int cx = size / 2;
            int cy = size / 2;

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    int dx = x - cx;
                    int dy = y - cy;

                    if (y >= 4 && y <= 16 && Mathf.Abs(dx) >= 6 && Mathf.Abs(dx) <= 10)
                        pixels[y * size + x] = capeRed;

                    if (y >= 6 && y <= 16 && Mathf.Abs(dx) <= 6)
                    {
                        pixels[y * size + x] = (Mathf.Abs(dx) == 6 || y == 6) ? steelDark : steelMid;
                        if (y == 8 && Mathf.Abs(dx) <= 5) pixels[y * size + x] = goldTrim;
                        if (y == 7 && Mathf.Abs(dx) <= 5) pixels[y * size + x] = leather;
                    }

                    if (y >= 13 && y <= 17 && (Mathf.Abs(dx) >= 6 && Mathf.Abs(dx) <= 9))
                        pixels[y * size + x] = goldTrim;

                    int headDy = y - 22;
                    if (dx * dx + headDy * headDy <= 36)
                    {
                        pixels[y * size + x] = (dx * dx + headDy * headDy >= 25 || y == 28) ? steelDark : steelLight;
                        if (y >= 26 && Mathf.Abs(dx) <= 2) pixels[y * size + x] = goldTrim;
                        if (y >= 20 && y <= 22 && Mathf.Abs(dx) <= 4)
                        {
                            pixels[y * size + x] = steelDark;
                            if (y == 21 && (dx == -2 || dx == 2)) pixels[y * size + x] = eyeGlow;
                        }
                    }

                    if (y >= 1 && y <= 5 && (Mathf.Abs(dx) >= 2 && Mathf.Abs(dx) <= 5))
                        pixels[y * size + x] = steelDark;
                }
            }

            tex.SetPixels(pixels);
            tex.Apply();
            _warriorSprite = Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.4f), size);
            return _warriorSprite;
        }

        /// <summary>
        /// 32x32 Broad Steel Greatsword with golden hilt, crossguard, and blood groove.
        /// </summary>
        public static Sprite GetOrCreateSwordSprite(int size = 32)
        {
            if (_swordSprite != null) return _swordSprite;

            var tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            tex.filterMode = FilterMode.Point;
            Color[] pixels = new Color[size * size];
            for (int i = 0; i < pixels.Length; i++) pixels[i] = Color.clear;

            Color bladeLight = new Color(0.92f, 0.95f, 0.98f);
            Color bladeMid = new Color(0.70f, 0.75f, 0.82f);
            Color bladeDark = new Color(0.35f, 0.40f, 0.48f);
            Color gold = new Color(0.95f, 0.80f, 0.20f);
            Color gemRed = new Color(0.95f, 0.20f, 0.25f);
            Color handle = new Color(0.40f, 0.25f, 0.15f);

            int cy = size / 2;

            for (int x = 2; x < size - 2; x++)
            {
                for (int y = cy - 4; y <= cy + 4; y++)
                {
                    int dy = Mathf.Abs(y - cy);
                    if (x >= 2 && x <= 7)
                    {
                        if (dy == 0) pixels[y * size + x] = handle;
                        if (x == 2) pixels[y * size + x] = gold;
                    }
                    else if (x >= 8 && x <= 9)
                    {
                        if (dy <= 4) pixels[y * size + x] = gold;
                        if (dy == 0 && x == 8) pixels[y * size + x] = gemRed;
                    }
                    else if (x >= 10 && x <= 28)
                    {
                        int maxBladeWidth = x >= 26 ? (28 - x) : 2;
                        if (dy <= maxBladeWidth)
                        {
                            if (dy == 0) pixels[y * size + x] = bladeLight;
                            else if (dy == maxBladeWidth) pixels[y * size + x] = bladeDark;
                            else pixels[y * size + x] = bladeMid;
                        }
                    }
                }
            }

            tex.SetPixels(pixels);
            tex.Apply();
            _swordSprite = Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(5f / size, 16f / size), size);
            return _swordSprite;
        }

        /// <summary>
        /// 24x24 Bouncy Jelly Slime.
        /// </summary>
        public static Sprite GetOrCreateSlimeSprite(int size = 24)
        {
            if (_slimeSprite != null) return _slimeSprite;

            var tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            tex.filterMode = FilterMode.Point;
            Color[] pixels = new Color[size * size];
            for (int i = 0; i < pixels.Length; i++) pixels[i] = Color.clear;

            Color slimeBody = new Color(0.20f, 0.85f, 0.35f, 0.95f);
            Color slimeShine = new Color(0.65f, 1.0f, 0.70f, 0.95f);
            Color slimeDark = new Color(0.10f, 0.55f, 0.20f, 0.95f);
            Color eyeWhite = Color.white;
            Color eyePupil = new Color(0.05f, 0.15f, 0.08f);

            int cx = size / 2;
            int cy = size / 2 - 2;

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float dx = (x - cx) / 8.5f;
                    float dy = (y - cy) / 7.0f;

                    if (y >= 1 && (dx * dx + dy * dy <= 1.0f))
                    {
                        pixels[y * size + x] = slimeBody;
                        if (dx * dx + dy * dy >= 0.75f || y <= 3) pixels[y * size + x] = slimeDark;
                        if (y >= 13 && y <= 16 && x >= cx - 5 && x <= cx - 2) pixels[y * size + x] = slimeShine;
                        if ((y >= 7 && y <= 10) && (x == cx - 3 || x == cx + 3)) pixels[y * size + x] = eyeWhite;
                        if ((y >= 8 && y <= 9) && (x == cx - 3 || x == cx + 3)) pixels[y * size + x] = eyePupil;
                    }
                }
            }

            tex.SetPixels(pixels);
            tex.Apply();
            _slimeSprite = Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.3f), size);
            return _slimeSprite;
        }

        /// <summary>
        /// 16x16 Faceted Shimmering Crystal Experience Gem.
        /// </summary>
        public static Sprite GetOrCreateGemSprite(int size = 16)
        {
            if (_gemSprite != null) return _gemSprite;

            var tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            tex.filterMode = FilterMode.Point;
            Color[] pixels = new Color[size * size];
            for (int i = 0; i < pixels.Length; i++) pixels[i] = Color.clear;

            Color gemBright = new Color(0.60f, 1.0f, 0.70f);
            Color gemMid = new Color(0.20f, 0.85f, 0.35f);
            Color gemDark = new Color(0.08f, 0.50f, 0.18f);

            int center = size / 2;
            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    int dx = Mathf.Abs(x - center);
                    int dy = Mathf.Abs(y - center);
                    if (dx + dy <= 6)
                    {
                        if (x <= center && y >= center) pixels[y * size + x] = gemBright;
                        else if (dx + dy == 6) pixels[y * size + x] = gemDark;
                        else pixels[y * size + x] = gemMid;
                    }
                }
            }

            tex.SetPixels(pixels);
            tex.Apply();
            _gemSprite = Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), size);
            return _gemSprite;
        }

        /// <summary>
        /// 48x48 Golden Sweeping Sword Slash Wave Arc.
        /// </summary>
        public static Sprite GetOrCreateSlashArcSprite(int size = 48)
        {
            if (_slashArcSprite != null) return _slashArcSprite;

            var texture = new Texture2D(size, size, TextureFormat.RGBA32, false);
            texture.filterMode = FilterMode.Bilinear;
            float radius = size * 0.48f;
            float innerRadius = size * 0.28f;
            Vector2 center = new Vector2(size * 0.15f, size * 0.5f);

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

        // Forwarding helpers for monsters and chests to MonsterSpriteHelper
        public static Sprite GetOrCreateBatSprite(int size = 24) => MonsterSpriteHelper.GetOrCreateBatSprite(size);
        public static Sprite GetOrCreateSkeletonSprite(int size = 28) => MonsterSpriteHelper.GetOrCreateSkeletonSprite(size);
        public static Sprite GetOrCreateGolemSprite(int size = 32) => MonsterSpriteHelper.GetOrCreateGolemSprite(size);
        public static Sprite GetOrCreateBossSprite(int size = 48) => MonsterSpriteHelper.GetOrCreateBossSprite(size);
        public static Sprite GetOrCreateChestSprite(int size = 24) => MonsterSpriteHelper.GetOrCreateChestSprite(size);

        public static Sprite GetOrCreateCircleSprite(int size = 32) => GetOrCreateWarriorSprite(size);
        public static Sprite GetOrCreateSquareSprite(int size = 16) => GetOrCreateGemSprite(size);
        public static Sprite GetOrCreateDiamondSprite(int size = 24) => GetOrCreateGemSprite(size);
    }
}
