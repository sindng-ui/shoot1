using UnityEngine;

namespace HappyShoot.View.Utils
{
    /// <summary>
    /// Procedural pixel-art sprite generator for Phase 3 monsters and Boss 3 (Arch-Lich King).
    /// Creates distinct, vibrant retro sprites with zero external asset dependencies.
    /// Strictly modular and under 500 lines.
    /// </summary>
    public static class Phase3MonsterSpriteHelper
    {
        private static Sprite _cachedWraithSprite;
        private static Sprite _cachedNecromancerSprite;
        private static Sprite _cachedAbominationSprite;
        private static Sprite _cachedReaperSprite;
        private static Sprite _cachedLichKingSprite;
        private static Sprite _cachedSoulOrbSprite;

        /// <summary>
        /// 1. Wraith (망령): Ethereal cyan ghost with ghostly glowing eyes and flowing wisps.
        /// </summary>
        public static Sprite GetOrCreateWraithSprite()
        {
            if (_cachedWraithSprite != null) return _cachedWraithSprite;

            const int w = 16, h = 16;
            var tex = new Texture2D(w, h, TextureFormat.RGBA32, false) { filterMode = FilterMode.Point };
            var cols = new Color[w * h];
            for (int i = 0; i < cols.Length; i++) cols[i] = Color.clear;

            Color ghostCyan = new Color(0.35f, 0.85f, 0.95f, 0.78f);
            Color ghostLight = new Color(0.70f, 0.95f, 1.0f, 0.90f);
            Color glowEye = new Color(1.0f, 1.0f, 0.40f, 1.0f);
            Color outline = new Color(0.10f, 0.35f, 0.55f, 0.85f);

            for (int y = 2; y <= 14; y++)
            {
                for (int x = 3; x <= 12; x++)
                {
                    float dx = (x - 7.5f) / 4.5f;
                    float dy = (y - 9.0f) / 5.0f;
                    if (dx * dx + dy * dy <= 1.0f || (y <= 6 && (x == 4 || x == 7 || x == 10 || x == 11)))
                    {
                        cols[y * w + x] = ghostCyan;
                        if (x == 3 || x == 12 || y == 14 || (y <= 3 && (x % 3 == 0)))
                            cols[y * w + x] = outline;
                        else if (y >= 10 && x >= 5 && x <= 10)
                            cols[y * w + x] = ghostLight;
                    }
                }
            }

            // Glowing yellow eyes
            cols[9 * w + 6] = glowEye;
            cols[9 * w + 9] = glowEye;

            tex.SetPixels(cols);
            tex.Apply();
            _cachedWraithSprite = Sprite.Create(tex, new Rect(0, 0, w, h), new Vector2(0.5f, 0.5f), 16);
            return _cachedWraithSprite;
        }

        /// <summary>
        /// 2. Necromancer (사령술사): Dark purple hooded robe with bone staff.
        /// </summary>
        public static Sprite GetOrCreateNecromancerSprite()
        {
            if (_cachedNecromancerSprite != null) return _cachedNecromancerSprite;

            const int w = 16, h = 16;
            var tex = new Texture2D(w, h, TextureFormat.RGBA32, false) { filterMode = FilterMode.Point };
            var cols = new Color[w * h];
            for (int i = 0; i < cols.Length; i++) cols[i] = Color.clear;

            Color robeDark = new Color(0.20f, 0.08f, 0.32f, 1.0f);
            Color robeTrim = new Color(0.55f, 0.20f, 0.75f, 1.0f);
            Color skullFace = new Color(0.90f, 0.88f, 0.82f, 1.0f);
            Color staffWood = new Color(0.40f, 0.25f, 0.15f, 1.0f);
            Color staffGem = new Color(0.10f, 0.95f, 0.50f, 1.0f);

            // Robe Body
            for (int y = 1; y <= 11; y++)
            {
                int spread = (12 - y) / 3;
                int minX = Mathf.Max(3, 5 - spread);
                int maxX = Mathf.Min(11, 9 + spread);
                for (int x = minX; x <= maxX; x++)
                {
                    cols[y * w + x] = (x == minX || x == maxX) ? robeTrim : robeDark;
                }
            }

            // Skull Hood & Face
            for (int y = 10; y <= 14; y++)
            {
                for (int x = 5; x <= 9; x++)
                {
                    cols[y * w + x] = robeDark;
                }
            }
            cols[11 * w + 6] = skullFace;
            cols[11 * w + 8] = skullFace;
            cols[12 * w + 7] = skullFace;

            // Bone Staff on the right hand
            for (int y = 2; y <= 15; y++) cols[y * w + 13] = staffWood;
            cols[15 * w + 12] = staffGem;
            cols[15 * w + 13] = staffGem;
            cols[15 * w + 14] = staffGem;
            cols[14 * w + 13] = staffGem;

            tex.SetPixels(cols);
            tex.Apply();
            _cachedNecromancerSprite = Sprite.Create(tex, new Rect(0, 0, w, h), new Vector2(0.5f, 0.5f), 16);
            return _cachedNecromancerSprite;
        }

        /// <summary>
        /// 3. Abomination (어보미네이션): Massive stitched flesh colossus (sickly olive & crimson stitches).
        /// </summary>
        public static Sprite GetOrCreateAbominationSprite()
        {
            if (_cachedAbominationSprite != null) return _cachedAbominationSprite;

            const int w = 18, h = 18;
            var tex = new Texture2D(w, h, TextureFormat.RGBA32, false) { filterMode = FilterMode.Point };
            var cols = new Color[w * h];
            for (int i = 0; i < cols.Length; i++) cols[i] = Color.clear;

            Color skinMain = new Color(0.38f, 0.50f, 0.28f, 1.0f);
            Color skinShadow = new Color(0.24f, 0.32f, 0.18f, 1.0f);
            Color stitchRed = new Color(0.85f, 0.15f, 0.20f, 1.0f);
            Color boneSpike = new Color(0.92f, 0.90f, 0.80f, 1.0f);

            // Heavy torso & limbs
            for (int y = 2; y <= 15; y++)
            {
                for (int x = 2; x <= 15; x++)
                {
                    float dx = (x - 8.5f) / 6.0f;
                    float dy = (y - 8.5f) / 6.5f;
                    if (dx * dx + dy * dy <= 1.0f)
                    {
                        cols[y * w + x] = (x < 5 || y < 5) ? skinShadow : skinMain;
                    }
                }
            }

            // Diagonal Stitches across chest
            for (int i = 0; i <= 6; i++)
            {
                int sx = 5 + i;
                int sy = 6 + i;
                if (sx < w && sy < h)
                {
                    cols[sy * w + sx] = stitchRed;
                    if (i % 2 == 0 && sy + 1 < h) cols[(sy + 1) * w + sx] = boneSpike;
                }
            }

            // Angry glowing red cyclops eye
            cols[13 * w + 8] = Color.yellow;
            cols[13 * w + 9] = Color.red;

            tex.SetPixels(cols);
            tex.Apply();
            _cachedAbominationSprite = Sprite.Create(tex, new Rect(0, 0, w, h), new Vector2(0.5f, 0.5f), 16);
            return _cachedAbominationSprite;
        }

        /// <summary>
        /// 4. Reaper (사신): Pitch-black hooded executioner holding a gleaming curved silver scythe.
        /// </summary>
        public static Sprite GetOrCreateReaperSprite()
        {
            if (_cachedReaperSprite != null) return _cachedReaperSprite;

            const int w = 18, h = 18;
            var tex = new Texture2D(w, h, TextureFormat.RGBA32, false) { filterMode = FilterMode.Point };
            var cols = new Color[w * h];
            for (int i = 0; i < cols.Length; i++) cols[i] = Color.clear;

            Color cloak = new Color(0.08f, 0.08f, 0.12f, 1.0f);
            Color cloakRim = new Color(0.25f, 0.25f, 0.38f, 1.0f);
            Color scytheBlade = new Color(0.85f, 0.92f, 0.98f, 1.0f);
            Color scytheGlint = Color.white;
            Color scytheHandle = new Color(0.35f, 0.22f, 0.12f, 1.0f);

            // Cloak body
            for (int y = 1; y <= 14; y++)
            {
                int minX = Mathf.Max(2, 6 - (14 - y) / 3);
                int maxX = Mathf.Min(11, 9 + (14 - y) / 3);
                for (int x = minX; x <= maxX; x++)
                {
                    cols[y * w + x] = (x == minX || x == maxX) ? cloakRim : cloak;
                }
            }

            // Glowing red piercing gaze
            cols[11 * w + 6] = Color.red;
            cols[11 * w + 8] = Color.red;

            // Long Scythe Handle
            for (int y = 2; y <= 16; y++) cols[y * w + 13] = scytheHandle;

            // Curved Giant Scythe Blade (Crescent shape at top)
            for (int i = 0; i <= 6; i++)
            {
                int bx = 13 - i;
                int by = 16 - (i * i) / 6;
                if (bx >= 0 && by < h)
                {
                    cols[by * w + bx] = scytheBlade;
                    if (by - 1 >= 0) cols[(by - 1) * w + bx] = scytheGlint;
                }
            }

            tex.SetPixels(cols);
            tex.Apply();
            _cachedReaperSprite = Sprite.Create(tex, new Rect(0, 0, w, h), new Vector2(0.5f, 0.5f), 16);
            return _cachedReaperSprite;
        }

        /// <summary>
        /// Boss 3: Arch-Lich King (사령왕 리치): Regal gold crown, obsidian skull, glowing azure soul aura.
        /// </summary>
        public static Sprite GetOrCreateLichKingSprite()
        {
            if (_cachedLichKingSprite != null) return _cachedLichKingSprite;
            if (CustomMonsterSpriteLoader.TryGetCustomMonsterSprite(HappyShoot.Domain.Entities.MonsterType.Boss3) is { } cl) return _cachedLichKingSprite = cl;

            const int w = 24, h = 24;
            var tex = new Texture2D(w, h, TextureFormat.RGBA32, false) { filterMode = FilterMode.Point };
            var cols = new Color[w * h];
            for (int i = 0; i < cols.Length; i++) cols[i] = Color.clear;

            Color goldCrown = new Color(1.0f, 0.85f, 0.20f, 1.0f);
            Color crownRuby = new Color(0.95f, 0.10f, 0.25f, 1.0f);
            Color regalPurple = new Color(0.28f, 0.08f, 0.45f, 1.0f);
            Color boneWhite = new Color(0.92f, 0.90f, 0.85f, 1.0f);
            Color soulBlue = new Color(0.20f, 0.85f, 1.0f, 1.0f);

            // Regal Royal Robe Body
            for (int y = 2; y <= 16; y++)
            {
                int minX = Mathf.Max(3, 8 - (16 - y) / 2);
                int maxX = Mathf.Min(20, 15 + (16 - y) / 2);
                for (int x = minX; x <= maxX; x++)
                {
                    cols[y * w + x] = regalPurple;
                    if (x == minX || x == maxX) cols[y * w + x] = goldCrown;
                }
            }

            // Skull Head
            for (int y = 14; y <= 19; y++)
            {
                for (int x = 8; x <= 15; x++)
                {
                    cols[y * w + x] = boneWhite;
                }
            }
            // Soul flame blue eyes
            cols[16 * w + 10] = soulBlue;
            cols[16 * w + 13] = soulBlue;

            // Crown with 3 peaks
            for (int x = 7; x <= 16; x++) cols[20 * w + x] = goldCrown;
            cols[21 * w + 8] = goldCrown;
            cols[22 * w + 8] = goldCrown;
            cols[21 * w + 12] = goldCrown;
            cols[22 * w + 12] = crownRuby;
            cols[23 * w + 12] = goldCrown; // Center peak
            cols[21 * w + 15] = goldCrown;
            cols[22 * w + 15] = goldCrown;

            tex.SetPixels(cols);
            tex.Apply();
            _cachedLichKingSprite = Sprite.Create(tex, new Rect(0, 0, w, h), new Vector2(0.5f, 0.5f), 16);
            return _cachedLichKingSprite;
        }

        /// <summary>
        /// Projectile: Cursed Soul Orb (저주받은 사령탄) fired by Necromancer.
        /// </summary>
        public static Sprite GetOrCreateSoulOrbSprite()
        {
            if (_cachedSoulOrbSprite != null) return _cachedSoulOrbSprite;

            const int size = 8;
            var tex = new Texture2D(size, size, TextureFormat.RGBA32, false) { filterMode = FilterMode.Point };
            var cols = new Color[size * size];
            for (int i = 0; i < cols.Length; i++) cols[i] = Color.clear;

            Color core = Color.white;
            Color halo = new Color(0.75f, 0.15f, 0.95f, 0.85f);

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    int dist = Mathf.Abs(x - 3) + Mathf.Abs(y - 3);
                    if (dist <= 1) cols[y * size + x] = core;
                    else if (dist <= 3) cols[y * size + x] = halo;
                }
            }

            tex.SetPixels(cols);
            tex.Apply();
            _cachedSoulOrbSprite = Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 16);
            return _cachedSoulOrbSprite;
        }
    }
}
