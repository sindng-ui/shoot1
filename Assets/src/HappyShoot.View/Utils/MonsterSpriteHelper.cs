using UnityEngine;

namespace HappyShoot.View.Utils
{
    /// <summary>
    /// Procedural pixel-art sprite generators for Monster archetypes (Bat, Skeleton, Golem, Boss) and Treasure Chests.
    /// </summary>
    public static class MonsterSpriteHelper
    {
        private static Sprite _batSprite;
        private static Sprite _skeletonSprite;
        private static Sprite _golemSprite;
        private static Sprite _bossSprite;
        private static Sprite _chestSprite;

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
        /// 48x48 Epic Boss Fiend with red horns, spiked crown, and fiery aura.
        /// </summary>
        public static Sprite GetOrCreateBossSprite(int size = 48)
        {
            if (_bossSprite != null) return _bossSprite;

            var tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            tex.filterMode = FilterMode.Point;
            Color[] pixels = new Color[size * size];
            for (int i = 0; i < pixels.Length; i++) pixels[i] = Color.clear;

            Color bodyDark = new Color(0.22f, 0.08f, 0.12f);
            Color bodyMid = new Color(0.65f, 0.15f, 0.22f);
            Color goldCrown = new Color(1.0f, 0.85f, 0.20f);
            Color eyeFire = new Color(1.0f, 0.95f, 0.35f);

            int cx = size / 2;
            int cy = size / 2;

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    int dx = Mathf.Abs(x - cx);
                    int dy = Mathf.Abs(y - cy);

                    if (dx <= 16 && dy <= 16)
                    {
                        pixels[y * size + x] = (dx >= 14 || dy >= 14) ? bodyDark : bodyMid;
                        if (y >= cy + 12 && y <= cy + 18 && dx <= 8) pixels[y * size + x] = goldCrown;
                        if (y == cy + 4 && (dx == 5 || dx == 6)) pixels[y * size + x] = eyeFire;
                    }
                }
            }

            tex.SetPixels(pixels);
            tex.Apply();
            _bossSprite = Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), size);
            return _bossSprite;
        }

        /// <summary>
        /// 24x24 Golden Shimmering Treasure Chest.
        /// </summary>
        public static Sprite GetOrCreateChestSprite(int size = 24)
        {
            if (_chestSprite != null) return _chestSprite;

            var tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            tex.filterMode = FilterMode.Point;
            Color[] pixels = new Color[size * size];
            for (int i = 0; i < pixels.Length; i++) pixels[i] = Color.clear;

            Color goldLight = new Color(1.0f, 0.90f, 0.35f);
            Color goldMid = new Color(0.85f, 0.65f, 0.15f);
            Color woodDark = new Color(0.40f, 0.22f, 0.10f);
            Color gem = new Color(0.20f, 0.85f, 1.0f);

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
