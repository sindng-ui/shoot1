using UnityEngine;

namespace HappyShoot.View.Utils
{
    /// <summary>
    /// Procedural pixel-art sprite generators for Monster archetypes (Bat, Skeleton, Golem, Boss, FireImp, ToxicSpider, DarkKnight) and Treasure Chests.
    /// All sprites under 500-line rule - split into MonsterSpriteHelper2 if needed.
    /// </summary>
    public static class MonsterSpriteHelper
    {
        private static Sprite _batSprite;
        private static Sprite _skeletonSprite;
        private static Sprite _golemSprite;
        private static Sprite _bossSprite;
        private static Sprite _chestSprite;
        private static Sprite _fireImpSprite;
        private static Sprite _toxicSpiderSprite;
        private static Sprite _darkKnightSprite;

        /// <summary>
        /// 24x24 Purple Vampire Bat with flapping wings and glowing red eyes.
        /// </summary>
        public static Sprite GetOrCreateBatSprite(int size = 24)
        {
            if (_batSprite != null) return _batSprite;

            var tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            tex.filterMode = FilterMode.Point;
            Color[] pixels = new Color[size * size];
            for (int i = 0; i < pixels.Length; i++) pixels[i] = Color.clear;

            Color body = new Color(0.35f, 0.15f, 0.45f);
            Color wing = new Color(0.55f, 0.25f, 0.70f);
            Color redEye = new Color(1.0f, 0.15f, 0.15f);

            int cx = size / 2;
            int cy = size / 2;

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    int dx = Mathf.Abs(x - cx);
                    int dy = y - cy;

                    if (dx <= 3 && Mathf.Abs(dy) <= 4) pixels[y * size + x] = body;
                    if (dx >= 4 && dx <= 10 && dy >= -2 && dy <= 5) pixels[y * size + x] = wing;
                    if (dy == 1 && (x == cx - 2 || x == cx + 2)) pixels[y * size + x] = redEye;
                }
            }

            tex.SetPixels(pixels);
            tex.Apply();
            _batSprite = Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), size);
            return _batSprite;
        }

        /// <summary>
        /// 28x28 Ivory Skeleton Archer with skull and bone body.
        /// </summary>
        public static Sprite GetOrCreateSkeletonSprite(int size = 28)
        {
            if (_skeletonSprite != null) return _skeletonSprite;

            var tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            tex.filterMode = FilterMode.Point;
            Color[] pixels = new Color[size * size];
            for (int i = 0; i < pixels.Length; i++) pixels[i] = Color.clear;

            Color bone = new Color(0.92f, 0.90f, 0.82f);
            Color darkEye = new Color(0.12f, 0.12f, 0.14f);

            int cx = size / 2;
            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    int dx = Mathf.Abs(x - cx);
                    if (y >= 16 && y <= 26 && dx <= 5)
                    {
                        pixels[y * size + x] = bone;
                        if ((y == 21 || y == 22) && (dx == 2 || dx == 3)) pixels[y * size + x] = darkEye;
                    }
                    if (y >= 6 && y <= 15 && (dx == 0 || (y % 3 == 0 && dx <= 4))) pixels[y * size + x] = bone;
                    if (y >= 1 && y <= 5 && (dx == 2 || dx == 3)) pixels[y * size + x] = bone;
                }
            }

            tex.SetPixels(pixels);
            tex.Apply();
            _skeletonSprite = Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.4f), size);
            return _skeletonSprite;
        }

        /// <summary>
        /// 32x32 Heavy Stone Golem with rugged rocky texture and glowing core.
        /// </summary>
        public static Sprite GetOrCreateGolemSprite(int size = 32)
        {
            if (_golemSprite != null) return _golemSprite;

            var tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            tex.filterMode = FilterMode.Point;
            Color[] pixels = new Color[size * size];
            for (int i = 0; i < pixels.Length; i++) pixels[i] = Color.clear;

            Color stoneMid = new Color(0.48f, 0.44f, 0.40f);
            Color stoneDark = new Color(0.28f, 0.24f, 0.20f);
            Color coreGlow = new Color(1.0f, 0.65f, 0.15f);

            int cx = size / 2;
            int cy = size / 2;

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    int dx = Mathf.Abs(x - cx);
                    int dy = Mathf.Abs(y - cy);

                    if (dx <= 10 && dy <= 10)
                    {
                        pixels[y * size + x] = (dx == 10 || dy == 10) ? stoneDark : stoneMid;
                        if (dx <= 2 && dy <= 2) pixels[y * size + x] = coreGlow;
                        if (y == cy + 6 && dx <= 4) pixels[y * size + x] = coreGlow;
                    }
                }
            }

            tex.SetPixels(pixels);
            tex.Apply();
            _golemSprite = Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), size);
            return _golemSprite;
        }

        /// <summary>
        /// 56x56 Fearsome Demon Lord with massive curved horns, blazing red eyes,
        /// dark armored body, and a jagged maw - far more menacing than a simple box.
        /// </summary>
        public static Sprite GetOrCreateBossSprite(int size = 56)
        {
            if (_bossSprite != null) return _bossSprite;

            var tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            tex.filterMode = FilterMode.Point;
            Color[] pixels = new Color[size * size];
            for (int i = 0; i < pixels.Length; i++) pixels[i] = Color.clear;

            Color bodyOuter  = new Color(0.12f, 0.04f, 0.08f);
            Color bodyMid    = new Color(0.45f, 0.10f, 0.16f);
            Color bodyInner  = new Color(0.62f, 0.14f, 0.20f);
            Color hornBase   = new Color(0.55f, 0.08f, 0.08f);
            Color hornTip    = new Color(0.90f, 0.30f, 0.10f);
            Color eyeGlow    = new Color(1.0f, 0.90f, 0.10f);
            Color eyeCore    = new Color(1.0f, 0.50f, 0.05f);
            Color mawDark    = new Color(0.08f, 0.02f, 0.02f);
            Color shoulderArmor = new Color(0.25f, 0.08f, 0.12f);

            int cx = size / 2;
            int cy = size / 2;

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    int dx = x - cx;
                    int dy = y - cy;
                    int adx = Mathf.Abs(dx);
                    int ady = Mathf.Abs(dy);

                    // --- Body: rounded diamond shape ---
                    float bodyEllipse = (adx / 13f) * (adx / 13f) + (ady / 17f) * (ady / 17f);
                    if (bodyEllipse <= 1.0f)
                    {
                        pixels[y * size + x] = bodyEllipse > 0.72f ? bodyOuter : bodyEllipse > 0.44f ? bodyMid : bodyInner;
                    }

                    // --- Left horn: curves up-left from top ---
                    // Thick curved horn using Bresenham line approach
                    for (int ht = 0; ht < 15; ht++)
                    {
                        int hx = cx - 6 - ht;
                        int hy = cy + 18 - ht * 2 + (ht > 7 ? ht - 7 : 0);
                        int thickness = ht < 5 ? 3 : ht < 10 ? 2 : 1;
                        if (Mathf.Abs(x - hx) <= thickness && Mathf.Abs(y - hy) <= thickness)
                        {
                            pixels[y * size + x] = ht < 5 ? hornBase : hornTip;
                        }
                    }
                    // --- Right horn: mirror ---
                    for (int ht = 0; ht < 15; ht++)
                    {
                        int hx = cx + 6 + ht;
                        int hy = cy + 18 - ht * 2 + (ht > 7 ? ht - 7 : 0);
                        int thickness = ht < 5 ? 3 : ht < 10 ? 2 : 1;
                        if (Mathf.Abs(x - hx) <= thickness && Mathf.Abs(y - hy) <= thickness)
                        {
                            pixels[y * size + x] = ht < 5 ? hornBase : hornTip;
                        }
                    }

                    // --- Glowing Eyes ---
                    bool leftEye  = (Mathf.Abs(dx + 5) <= 3 && Mathf.Abs(dy - 6) <= 2);
                    bool rightEye = (Mathf.Abs(dx - 5) <= 3 && Mathf.Abs(dy - 6) <= 2);
                    bool leftCore  = (Mathf.Abs(dx + 5) <= 1 && Mathf.Abs(dy - 6) <= 1);
                    bool rightCore = (Mathf.Abs(dx - 5) <= 1 && Mathf.Abs(dy - 6) <= 1);
                    if (leftEye || rightEye)  pixels[y * size + x] = eyeGlow;
                    if (leftCore || rightCore) pixels[y * size + x] = eyeCore;

                    // --- Jagged Maw (teeth row) ---
                    if (dy >= -5 && dy <= -2 && adx <= 8)
                    {
                        if ((x % 3 == 0) || dy == -5) pixels[y * size + x] = mawDark;
                    }

                    // --- Shoulder plates (wider than body, dark armor) ---
                    if (ady <= 4 && adx >= 12 && adx <= 17)
                    {
                        pixels[y * size + x] = shoulderArmor;
                    }
                }
            }

            tex.SetPixels(pixels);
            tex.Apply();
            _bossSprite = Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), size);
            return _bossSprite;
        }

        /// <summary>28x28 Crimson Fire Imp - small agile fiend with pointed ears and flame aura.</summary>
        public static Sprite GetOrCreateFireImpSprite(int size = 28)
        {
            if (_fireImpSprite != null) return _fireImpSprite;

            var tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            tex.filterMode = FilterMode.Point;
            Color[] pixels = new Color[size * size];
            for (int i = 0; i < pixels.Length; i++) pixels[i] = Color.clear;

            Color bodyRed  = new Color(0.85f, 0.22f, 0.08f);
            Color bodyDark = new Color(0.50f, 0.10f, 0.04f);
            Color flame    = new Color(1.0f, 0.75f, 0.10f);
            Color eyeWhite = new Color(1.0f, 0.95f, 0.60f);

            int cx = size / 2;
            int cy = size / 2;

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    int adx = Mathf.Abs(x - cx);
                    int ady = Mathf.Abs(y - cy);
                    float ellipse = (adx / 8f) * (adx / 8f) + (ady / 10f) * (ady / 10f);
                    if (ellipse <= 1.0f)
                        pixels[y * size + x] = ellipse > 0.65f ? bodyDark : bodyRed;

                    // Pointed ears
                    if (y > cy + 9 && y <= cy + 14 && (Mathf.Abs(x - (cx - 7)) <= 1 || Mathf.Abs(x - (cx + 7)) <= 1))
                        pixels[y * size + x] = bodyDark;

                    // Flame crown on top
                    if (y >= cy + 7 && y <= cy + 13 && adx <= 6 && ((x - cx) % 2 == 0 || y == cy + 10))
                        pixels[y * size + x] = flame;

                    // Eyes
                    if (Mathf.Abs(y - (cy + 3)) <= 1 && (Mathf.Abs(x - (cx - 3)) <= 1 || Mathf.Abs(x - (cx + 3)) <= 1))
                        pixels[y * size + x] = eyeWhite;
                }
            }

            tex.SetPixels(pixels);
            tex.Apply();
            _fireImpSprite = Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), size);
            return _fireImpSprite;
        }

        /// <summary>32x32 Toxic Spider - fat body with 8 spindly legs and toxic green glow.</summary>
        public static Sprite GetOrCreateToxicSpiderSprite(int size = 32)
        {
            if (_toxicSpiderSprite != null) return _toxicSpiderSprite;

            var tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            tex.filterMode = FilterMode.Point;
            Color[] pixels = new Color[size * size];
            for (int i = 0; i < pixels.Length; i++) pixels[i] = Color.clear;

            Color bodyGreen  = new Color(0.20f, 0.60f, 0.15f);
            Color bodyDark   = new Color(0.08f, 0.28f, 0.05f);
            Color toxicGlow  = new Color(0.60f, 1.0f, 0.20f);
            Color legColor   = new Color(0.12f, 0.35f, 0.08f);
            Color eyeRed     = new Color(1.0f, 0.10f, 0.10f);

            int cx = size / 2;
            int cy = size / 2;

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    int adx = Mathf.Abs(x - cx);
                    int ady = Mathf.Abs(y - cy);
                    float ellipse = (adx / 9f) * (adx / 9f) + (ady / 7f) * (ady / 7f);
                    if (ellipse <= 1.0f)
                        pixels[y * size + x] = ellipse > 0.65f ? bodyDark : bodyGreen;

                    // Toxic core center
                    if (adx <= 2 && ady <= 2)
                        pixels[y * size + x] = toxicGlow;

                    // 4 pairs of legs (8 total)
                    int[] legDy = { -4, -2, 2, 4 };
                    foreach (int ldy in legDy)
                    {
                        // Left leg
                        if (y == cy + ldy && x >= cx - 14 && x <= cx - 9)
                            pixels[y * size + x] = legColor;
                        // Right leg
                        if (y == cy + ldy && x >= cx + 9 && x <= cx + 14)
                            pixels[y * size + x] = legColor;
                    }

                    // Multi eyes row
                    if (y == cy + 5 && (adx == 2 || adx == 4 || adx == 6))
                        pixels[y * size + x] = eyeRed;
                }
            }

            tex.SetPixels(pixels);
            tex.Apply();
            _toxicSpiderSprite = Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), size);
            return _toxicSpiderSprite;
        }

        /// <summary>38x38 Dark Knight - imposing armored warrior with visor, pauldrons and dark blade silhouette.</summary>
        public static Sprite GetOrCreateDarkKnightSprite(int size = 38)
        {
            if (_darkKnightSprite != null) return _darkKnightSprite;

            var tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            tex.filterMode = FilterMode.Point;
            Color[] pixels = new Color[size * size];
            for (int i = 0; i < pixels.Length; i++) pixels[i] = Color.clear;

            Color armorDark    = new Color(0.14f, 0.14f, 0.18f);
            Color armorMid     = new Color(0.30f, 0.30f, 0.38f);
            Color armorLight   = new Color(0.50f, 0.50f, 0.62f);
            Color visorGlow    = new Color(0.65f, 0.10f, 1.0f);  // purple ominous visor
            Color swordEdge    = new Color(0.75f, 0.82f, 1.0f);

            int cx = size / 2;
            int cy = size / 2;

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    int dx = x - cx;
                    int dy = y - cy;
                    int adx = Mathf.Abs(dx);
                    int ady = Mathf.Abs(dy);

                    // Helmet (rounded top)
                    if (ady <= 10 && adx <= 6 && (dy >= 0 || (adx / 5f) * (adx / 5f) + ((dy - 5f) / 8f) * ((dy - 5f) / 8f) <= 1.1f))
                    {
                        pixels[y * size + x] = dy >= 8 ? armorLight : adx >= 5 ? armorDark : armorMid;
                    }

                    // Visor slit
                    if (dy >= 2 && dy <= 4 && adx <= 5)
                        pixels[y * size + x] = visorGlow;

                    // Body torso
                    if (ady <= 7 && adx <= 7 && dy < 0)
                    {
                        pixels[y * size + x] = adx >= 6 || ady >= 6 ? armorDark : armorMid;
                    }

                    // Pauldrons (shoulder armor - wider)
                    if (dy >= -4 && dy <= 2 && adx >= 7 && adx <= 11)
                        pixels[y * size + x] = armorDark;

                    // Sword/blade silhouette on the right side
                    if (dx >= 8 && dx <= 10 && dy >= -14 && dy <= 6)
                        pixels[y * size + x] = swordEdge;
                    if (dx == 9 && dy >= -16 && dy <= -14)
                        pixels[y * size + x] = armorLight;

                    // Legs
                    if (dy <= -8 && ady >= 8 && adx <= 4)
                        pixels[y * size + x] = armorDark;
                }
            }

            tex.SetPixels(pixels);
            tex.Apply();
            _darkKnightSprite = Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.45f), size);
            return _darkKnightSprite;
        }

        /// <summary>24x24 Golden Shimmering Treasure Chest.</summary>
        public static Sprite GetOrCreateChestSprite(int size = 24)
        {
            if (_chestSprite != null) return _chestSprite;

            var tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            tex.filterMode = FilterMode.Point;
            Color[] pixels = new Color[size * size];
            for (int i = 0; i < pixels.Length; i++) pixels[i] = Color.clear;

            Color goldLight = new Color(1.0f, 0.90f, 0.35f);
            Color goldMid   = new Color(0.85f, 0.65f, 0.15f);
            Color woodDark  = new Color(0.40f, 0.22f, 0.10f);
            Color gem       = new Color(0.20f, 0.85f, 1.0f);

            int cx = size / 2;
            for (int y = 2; y < size - 2; y++)
            {
                for (int x = 2; x < size - 2; x++)
                {
                    int dx = Mathf.Abs(x - cx);
                    pixels[y * size + x] = woodDark;
                    if (x == 2 || x == size - 3 || y == 2 || y == size - 3 || y == 12) pixels[y * size + x] = goldMid;
                    if (y >= 10 && y <= 14 && dx <= 2) pixels[y * size + x] = (dx == 0 && y == 12) ? gem : goldLight;
                }
            }

            tex.SetPixels(pixels);
            tex.Apply();
            _chestSprite = Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.4f), size);
            return _chestSprite;
        }
    }
}
