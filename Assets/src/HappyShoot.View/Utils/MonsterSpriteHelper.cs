using UnityEngine;

namespace HappyShoot.View.Utils
{
    /// <summary>
    /// Procedural high-detail pixel-art sprite generators for Monster archetypes
    /// (Slime, Bat, Skeleton, Golem, Boss, FireImp, ToxicSpider, DarkKnight) and Treasure Chests.
    /// Enhanced with rich cel-shading, highlights, glowing cores, and sharp outlines.
    /// Strictly modular and under 460 lines (500-line architecture rule).
    /// </summary>
    public static class MonsterSpriteHelper
    {
        private static Sprite _slimeSprite;
        private static Sprite _batSprite;
        private static Sprite _skeletonSprite;
        private static Sprite _golemSprite;
        private static Sprite _bossSprite;
        private static Sprite _chestSprite;
        private static Sprite _fireImpSprite;
        private static Sprite _toxicSpiderSprite;
        private static Sprite _darkKnightSprite;

        /// <summary>
        /// 28x28 Bouncy Translucent Emerald Jelly Slime with rich highlights and cute pupils.
        /// </summary>
        public static Sprite GetOrCreateSlimeSprite(int size = 28)
        {
            if (_slimeSprite != null) return _slimeSprite;

            var tex = new Texture2D(size, size, TextureFormat.RGBA32, false) { filterMode = FilterMode.Point };
            Color[] pixels = new Color[size * size];
            for (int i = 0; i < pixels.Length; i++) pixels[i] = Color.clear;

            Color slimeBody = new Color(0.22f, 0.88f, 0.38f, 0.95f);
            Color slimeShine = new Color(0.75f, 1.0f, 0.78f, 0.95f);
            Color slimeDark = new Color(0.08f, 0.48f, 0.18f, 0.95f);
            Color eyeWhite = Color.white;
            Color eyePupil = new Color(0.04f, 0.14f, 0.06f);

            int cx = size / 2, cy = size / 2 - 2;

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float dx = (x - cx) / 10.0f;
                    float dy = (y - cy) / 8.5f;

                    if (y >= 1 && (dx * dx + dy * dy <= 1.0f))
                    {
                        pixels[y * size + x] = slimeBody;
                        if (dx * dx + dy * dy >= 0.72f || y <= 3) pixels[y * size + x] = slimeDark;
                        if (y >= 16 && y <= 20 && x >= cx - 6 && x <= cx - 2) pixels[y * size + x] = slimeShine;
                        if ((y >= 9 && y <= 13) && (x == cx - 4 || x == cx + 4)) pixels[y * size + x] = eyeWhite;
                        if ((y >= 10 && y <= 12) && (x == cx - 4 || x == cx + 4)) pixels[y * size + x] = eyePupil;
                        if (y == 11 && (x == cx - 4 || x == cx + 4)) pixels[y * size + x] = Color.white; // pupil sparkle
                    }
                }
            }

            tex.SetPixels(pixels);
            tex.Apply();
            _slimeSprite = Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.35f), size);
            return _slimeSprite;
        }

        /// <summary>
        /// 28x28 Velvet Purple Vampire Bat with sharp wing ribs and piercing ruby eyes.
        /// </summary>
        public static Sprite GetOrCreateBatSprite(int size = 28)
        {
            if (_batSprite != null) return _batSprite;

            var tex = new Texture2D(size, size, TextureFormat.RGBA32, false) { filterMode = FilterMode.Point };
            Color[] pixels = new Color[size * size];
            for (int i = 0; i < pixels.Length; i++) pixels[i] = Color.clear;

            Color bodyDark = new Color(0.22f, 0.10f, 0.32f);
            Color bodyMid = new Color(0.42f, 0.20f, 0.58f);
            Color wingMembrane = new Color(0.62f, 0.30f, 0.80f);
            Color wingBone = new Color(0.25f, 0.12f, 0.36f);
            Color redEye = new Color(1.0f, 0.20f, 0.25f);
            Color fang = Color.white;

            int cx = size / 2, cy = size / 2;

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    int dx = Mathf.Abs(x - cx);
                    int dy = y - cy;

                    // Body and head
                    if (dx <= 4 && Mathf.Abs(dy) <= 5)
                    {
                        pixels[y * size + x] = dx <= 2 ? bodyMid : bodyDark;
                        // Glowing red eyes
                        if (dy == 1 && (dx == 2 || dx == 3)) pixels[y * size + x] = redEye;
                        // Tiny fangs
                        if (dy == -2 && (dx == 1 || dx == 2)) pixels[y * size + x] = fang;
                    }

                    // Ears
                    if (dy >= 6 && dy <= 9 && (dx == 3 || dx == 4)) pixels[y * size + x] = bodyMid;

                    // Wings with distinct bones & webbed shape
                    if (dx >= 5 && dx <= 12 && dy >= -4 && dy <= 6)
                    {
                        if (dx == 12 || dy == 6 || (dy == 0 && dx >= 8)) pixels[y * size + x] = wingBone;
                        else pixels[y * size + x] = wingMembrane;
                    }
                }
            }

            tex.SetPixels(pixels);
            tex.Apply();
            _batSprite = Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), size);
            return _batSprite;
        }

        /// <summary>
        /// 32x32 Ivory Skeleton Archer with shaded skull, ribbed chest, and bone bow.
        /// </summary>
        public static Sprite GetOrCreateSkeletonSprite(int size = 32)
        {
            if (_skeletonSprite != null) return _skeletonSprite;

            var tex = new Texture2D(size, size, TextureFormat.RGBA32, false) { filterMode = FilterMode.Point };
            Color[] pixels = new Color[size * size];
            for (int i = 0; i < pixels.Length; i++) pixels[i] = Color.clear;

            Color boneLight = new Color(0.96f, 0.94f, 0.88f);
            Color boneMid = new Color(0.80f, 0.78f, 0.70f);
            Color boneDark = new Color(0.35f, 0.32f, 0.28f);
            Color redEye = new Color(1.0f, 0.15f, 0.20f);
            Color leather = new Color(0.45f, 0.28f, 0.15f);

            int cx = size / 2;
            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    int dx = Mathf.Abs(x - cx);

                    // Skull Head (y: 18..29)
                    if (y >= 18 && y <= 29 && dx <= 6)
                    {
                        pixels[y * size + x] = (dx == 6 || y == 29) ? boneDark : (x < cx ? boneLight : boneMid);
                        // Deep eye sockets with blood red glowing core
                        if ((y == 22 || y == 23) && (dx == 2 || dx == 3))
                        {
                            pixels[y * size + x] = boneDark;
                            if (y == 23 && dx == 2) pixels[y * size + x] = redEye;
                        }
                        // Nasal cavity
                        if (y == 20 && dx == 0) pixels[y * size + x] = boneDark;
                        // Teeth slit
                        if (y == 18 && dx <= 3) pixels[y * size + x] = (dx % 2 == 0) ? boneLight : boneDark;
                    }

                    // Spine and Ribs (y: 8..17)
                    if (y >= 8 && y <= 17)
                    {
                        if (dx == 0 || dx == 1) pixels[y * size + x] = boneLight; // Spine
                        if ((y == 11 || y == 13 || y == 15) && dx <= 5) pixels[y * size + x] = boneMid; // Ribs
                        if (y == 8 && dx <= 4) pixels[y * size + x] = leather; // Belt
                    }

                    // Bone legs
                    if (y >= 1 && y <= 7 && (dx == 2 || dx == 3)) pixels[y * size + x] = boneLight;

                    // Bone bow on right side
                    if (x >= cx + 7 && x <= cx + 9 && y >= 6 && y <= 24)
                    {
                        if (x == cx + 8 || y == 6 || y == 24) pixels[y * size + x] = boneLight;
                    }
                }
            }

            tex.SetPixels(pixels);
            tex.Apply();
            _skeletonSprite = Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.4f), size);
            return _skeletonSprite;
        }

        /// <summary>
        /// 36x36 Heavy Granite Rock Golem with shoulder rock pauldrons and pulsing amber rune core.
        /// </summary>
        public static Sprite GetOrCreateGolemSprite(int size = 36)
        {
            if (_golemSprite != null) return _golemSprite;

            var tex = new Texture2D(size, size, TextureFormat.RGBA32, false) { filterMode = FilterMode.Point };
            Color[] pixels = new Color[size * size];
            for (int i = 0; i < pixels.Length; i++) pixels[i] = Color.clear;

            Color stoneLight = new Color(0.68f, 0.65f, 0.60f);
            Color stoneMid = new Color(0.48f, 0.44f, 0.40f);
            Color stoneDark = new Color(0.24f, 0.22f, 0.18f);
            Color runeCore = new Color(1.0f, 0.70f, 0.15f);
            Color runeGlow = new Color(1.0f, 0.92f, 0.45f);

            int cx = size / 2, cy = size / 2;

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    int dx = Mathf.Abs(x - cx);
                    int dy = Mathf.Abs(y - cy);

                    // Massive rocky torso
                    if (dx <= 12 && dy <= 12)
                    {
                        pixels[y * size + x] = (dx == 12 || dy == 12 || (x % 4 == 0 && y % 4 == 0)) ? stoneDark : ((x < cx) ? stoneLight : stoneMid);

                        // Pulsing Rune Core in chest
                        if (dx <= 3 && dy <= 3)
                        {
                            pixels[y * size + x] = (dx <= 1 && dy <= 1) ? runeGlow : runeCore;
                        }

                        // Eye slit visor with rune glow
                        if (y == cy + 7 && dx <= 5) pixels[y * size + x] = runeCore;
                    }

                    // Rocky shoulder boulders
                    if (dy >= 6 && dy <= 14 && dx >= 11 && dx <= 16)
                    {
                        pixels[y * size + x] = (dx >= 15 || dy == 14) ? stoneDark : stoneLight;
                    }
                }
            }

            tex.SetPixels(pixels);
            tex.Apply();
            _golemSprite = Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), size);
            return _golemSprite;
        }

        /// <summary>
        /// 56x56 Epic Demon Lord with massive wings, obsidian horns, and 3-eye laser cannon core.
        /// </summary>
        public static Sprite GetOrCreateBossSprite(int size = 56)
        {
            if (_bossSprite != null) return _bossSprite;

            var tex = new Texture2D(size, size, TextureFormat.RGBA32, false) { filterMode = FilterMode.Point };
            Color[] pixels = new Color[size * size];
            for (int i = 0; i < pixels.Length; i++) pixels[i] = Color.clear;

            Color bodyOuter = new Color(0.12f, 0.04f, 0.08f);
            Color bodyMid = new Color(0.50f, 0.12f, 0.18f);
            Color bodyInner = new Color(0.72f, 0.16f, 0.24f);
            Color hornBase = new Color(0.55f, 0.08f, 0.08f);
            Color hornTip = new Color(0.95f, 0.40f, 0.15f);
            Color eyeGlow = new Color(1.0f, 0.92f, 0.15f);
            Color eyeCore = new Color(1.0f, 0.45f, 0.05f);
            Color laserEye = new Color(0.20f, 0.95f, 1.0f); // Cyan 3rd Eye for laser cannon

            int cx = size / 2, cy = size / 2;

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    int dx = x - cx;
                    int dy = y - cy;
                    int adx = Mathf.Abs(dx);
                    int ady = Mathf.Abs(dy);

                    // Body: rounded diamond shape
                    float bodyEllipse = (adx / 14f) * (adx / 14f) + (ady / 18f) * (ady / 18f);
                    if (bodyEllipse <= 1.0f)
                    {
                        pixels[y * size + x] = bodyEllipse > 0.72f ? bodyOuter : (bodyEllipse > 0.42f ? bodyMid : bodyInner);
                    }

                    // Left & Right Curved Horns
                    for (int ht = 0; ht < 16; ht++)
                    {
                        int hxL = cx - 6 - ht, hxR = cx + 6 + ht;
                        int hy = cy + 18 - ht * 2 + (ht > 7 ? ht - 7 : 0);
                        int thickness = ht < 5 ? 3 : (ht < 10 ? 2 : 1);
                        if (Mathf.Abs(x - hxL) <= thickness && Mathf.Abs(y - hy) <= thickness) pixels[y * size + x] = ht < 5 ? hornBase : hornTip;
                        if (Mathf.Abs(x - hxR) <= thickness && Mathf.Abs(y - hy) <= thickness) pixels[y * size + x] = ht < 5 ? hornBase : hornTip;
                    }

                    // Main Glowing Eyes
                    bool leftEye = (Mathf.Abs(dx + 5) <= 3 && Mathf.Abs(dy - 5) <= 2);
                    bool rightEye = (Mathf.Abs(dx - 5) <= 3 && Mathf.Abs(dy - 5) <= 2);
                    if (leftEye || rightEye) pixels[y * size + x] = eyeGlow;

                    // 3rd Eye Laser Cannon in forehead
                    if (Mathf.Abs(dx) <= 2 && Mathf.Abs(dy - 11) <= 2)
                    {
                        pixels[y * size + x] = (dx == 0 && dy == 11) ? Color.white : laserEye;
                    }
                }
            }

            tex.SetPixels(pixels);
            tex.Apply();
            _bossSprite = Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), size);
            return _bossSprite;
        }

        /// <summary>
        /// 30x30 Crimson Fire Imp with sharp horns, bat-like wings, and fiery tail tip.
        /// </summary>
        public static Sprite GetOrCreateFireImpSprite(int size = 30)
        {
            if (_fireImpSprite != null) return _fireImpSprite;

            var tex = new Texture2D(size, size, TextureFormat.RGBA32, false) { filterMode = FilterMode.Point };
            Color[] pixels = new Color[size * size];
            for (int i = 0; i < pixels.Length; i++) pixels[i] = Color.clear;

            Color bodyRed = new Color(0.92f, 0.25f, 0.10f);
            Color bodyDark = new Color(0.52f, 0.10f, 0.04f);
            Color flame = new Color(1.0f, 0.85f, 0.15f);
            Color eyeWhite = new Color(1.0f, 0.98f, 0.70f);

            int cx = size / 2, cy = size / 2;

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    int adx = Mathf.Abs(x - cx);
                    int ady = Mathf.Abs(y - cy);
                    float ellipse = (adx / 8.5f) * (adx / 8.5f) + (ady / 10.5f) * (ady / 10.5f);
                    if (ellipse <= 1.0f) pixels[y * size + x] = ellipse > 0.65f ? bodyDark : bodyRed;

                    // Pointed demon ears / horns
                    if (y > cy + 9 && y <= cy + 14 && (Mathf.Abs(x - (cx - 7)) <= 1 || Mathf.Abs(x - (cx + 7)) <= 1))
                        pixels[y * size + x] = bodyDark;

                    // Flame crown on top
                    if (y >= cy + 7 && y <= cy + 14 && adx <= 6 && ((x - cx) % 2 == 0 || y >= cy + 11))
                        pixels[y * size + x] = flame;

                    // Keen glowing eyes
                    if (Mathf.Abs(y - (cy + 3)) <= 1 && (Mathf.Abs(x - (cx - 3)) <= 1 || Mathf.Abs(x - (cx + 3)) <= 1))
                        pixels[y * size + x] = eyeWhite;
                }
            }

            tex.SetPixels(pixels);
            tex.Apply();
            _fireImpSprite = Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), size);
            return _fireImpSprite;
        }

        /// <summary>
        /// 34x34 Toxic Spider with fluorescent poison sack, 8 jointed legs, and multiple red eyes.
        /// </summary>
        public static Sprite GetOrCreateToxicSpiderSprite(int size = 34)
        {
            if (_toxicSpiderSprite != null) return _toxicSpiderSprite;

            var tex = new Texture2D(size, size, TextureFormat.RGBA32, false) { filterMode = FilterMode.Point };
            Color[] pixels = new Color[size * size];
            for (int i = 0; i < pixels.Length; i++) pixels[i] = Color.clear;

            Color bodyGreen = new Color(0.24f, 0.68f, 0.18f);
            Color bodyDark = new Color(0.08f, 0.28f, 0.05f);
            Color toxicGlow = new Color(0.70f, 1.0f, 0.25f);
            Color legColor = new Color(0.14f, 0.40f, 0.10f);
            Color eyeRed = new Color(1.0f, 0.15f, 0.15f);

            int cx = size / 2, cy = size / 2;

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    int adx = Mathf.Abs(x - cx);
                    int ady = Mathf.Abs(y - cy);
                    float ellipse = (adx / 10f) * (adx / 10f) + (ady / 8f) * (ady / 8f);
                    if (ellipse <= 1.0f) pixels[y * size + x] = ellipse > 0.65f ? bodyDark : bodyGreen;

                    // Glowing toxic venom core
                    if (adx <= 3 && ady <= 3) pixels[y * size + x] = toxicGlow;

                    // 8 Jointed Legs
                    int[] legDy = { -5, -2, 2, 5 };
                    foreach (int ldy in legDy)
                    {
                        if (y == cy + ldy && ((x >= cx - 15 && x <= cx - 10) || (x >= cx + 10 && x <= cx + 15)))
                            pixels[y * size + x] = legColor;
                    }

                    // 4 cluster eyes
                    if (y == cy + 5 && (adx == 2 || adx == 5)) pixels[y * size + x] = eyeRed;
                }
            }

            tex.SetPixels(pixels);
            tex.Apply();
            _toxicSpiderSprite = Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), size);
            return _toxicSpiderSprite;
        }

        /// <summary>
        /// 40x40 Obsidian Dark Knight with sharp horned helmet, glowing purple visor, and dark claymore.
        /// </summary>
        public static Sprite GetOrCreateDarkKnightSprite(int size = 40)
        {
            if (_darkKnightSprite != null) return _darkKnightSprite;

            var tex = new Texture2D(size, size, TextureFormat.RGBA32, false) { filterMode = FilterMode.Point };
            Color[] pixels = new Color[size * size];
            for (int i = 0; i < pixels.Length; i++) pixels[i] = Color.clear;

            Color armorDark = new Color(0.12f, 0.12f, 0.16f);
            Color armorMid = new Color(0.32f, 0.32f, 0.42f);
            Color armorLight = new Color(0.55f, 0.55f, 0.68f);
            Color visorGlow = new Color(0.75f, 0.15f, 1.0f); // Ominous glowing purple visor
            Color swordEdge = new Color(0.80f, 0.88f, 1.0f);

            int cx = size / 2, cy = size / 2;

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    int dx = x - cx, dy = y - cy;
                    int adx = Mathf.Abs(dx), ady = Mathf.Abs(dy);

                    // Helmet
                    if (ady <= 11 && adx <= 7 && dy >= 0)
                    {
                        pixels[y * size + x] = dy >= 9 ? armorLight : (adx >= 6 ? armorDark : armorMid);
                    }

                    // Visor Slit
                    if (dy >= 3 && dy <= 5 && adx <= 5) pixels[y * size + x] = visorGlow;

                    // Body & Shoulder Plates
                    if (ady <= 8 && adx <= 8 && dy < 0) pixels[y * size + x] = adx >= 7 ? armorDark : armorMid;
                    if (dy >= -4 && dy <= 3 && adx >= 8 && adx <= 12) pixels[y * size + x] = armorDark;

                    // Claymore blade on right side
                    if (dx >= 9 && dx <= 11 && dy >= -15 && dy <= 7) pixels[y * size + x] = swordEdge;
                }
            }

            tex.SetPixels(pixels);
            tex.Apply();
            _darkKnightSprite = Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.45f), size);
            return _darkKnightSprite;
        }

        /// <summary>
        /// 28x28 Antique Golden Treasure Chest with cyan jewel lock.
        /// </summary>
        public static Sprite GetOrCreateChestSprite(int size = 28)
        {
            if (_chestSprite != null) return _chestSprite;

            var tex = new Texture2D(size, size, TextureFormat.RGBA32, false) { filterMode = FilterMode.Point };
            Color[] pixels = new Color[size * size];
            for (int i = 0; i < pixels.Length; i++) pixels[i] = Color.clear;

            Color goldLight = new Color(1.0f, 0.92f, 0.40f);
            Color goldMid = new Color(0.88f, 0.70f, 0.18f);
            Color woodDark = new Color(0.42f, 0.24f, 0.12f);
            Color gem = new Color(0.25f, 0.90f, 1.0f);

            int cx = size / 2;
            for (int y = 2; y < size - 2; y++)
            {
                for (int x = 2; x < size - 2; x++)
                {
                    int dx = Mathf.Abs(x - cx);
                    pixels[y * size + x] = woodDark;
                    if (x == 2 || x == size - 3 || y == 2 || y == size - 3 || y == 14) pixels[y * size + x] = goldMid;
                    if (y >= 12 && y <= 16 && dx <= 2) pixels[y * size + x] = (dx == 0 && y == 14) ? gem : goldLight;
                }
            }

            tex.SetPixels(pixels);
            tex.Apply();
            _chestSprite = Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.4f), size);
            return _chestSprite;
        }
    }
}
