using System.Collections.Generic;
using UnityEngine;
using HappyShoot.Domain.Entities;

namespace HappyShoot.View.Utils
{
    /// <summary>
    /// Procedural pixel-art sprite generator for 3 Heroes (Warrior, Ranger, Wizard) and Weapons.
    /// Exactly preserves the beloved original 32x32 pixel-art formulas with clean 9-directional support.
    /// Strictly modular and under 500 lines.
    /// </summary>
    public static class HeroSpriteHelper
    {
        public enum ViewDirection
        {
            Front,          // South (Facing forward)
            FrontDiagonal,  // SE / SW
            Side,           // East / West
            BackDiagonal,   // NE / NW
            Back            // North (Back of character)
        }

        private static readonly Dictionary<string, Sprite> _spriteCache = new Dictionary<string, Sprite>(64);

        public static Sprite GetHeroSprite(CharacterClassType classType, ViewDirection dir, int size = 32)
        {
            string key = $"{classType}_{dir}_{size}";
            if (_spriteCache.TryGetValue(key, out var cached) && cached != null)
                return cached;

            // Check if high-resolution custom sprite is available
            var customSprite = CustomHeroSpriteLoader.TryGetCustomHeroSprite(classType, dir);
            if (customSprite != null)
            {
                _spriteCache[key] = customSprite;
                return customSprite;
            }

            var tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            tex.filterMode = FilterMode.Point;
            Color[] pixels = new Color[size * size];
            for (int i = 0; i < pixels.Length; i++) pixels[i] = Color.clear;

            int cx = size / 2;
            int cy = size / 2;

            switch (classType)
            {
                case CharacterClassType.Warrior:
                    DrawWarrior(pixels, size, cx, cy, dir);
                    break;
                case CharacterClassType.Ranger:
                    DrawRanger(pixels, size, cx, cy, dir);
                    break;
                case CharacterClassType.Wizard:
                    DrawWizard(pixels, size, cx, cy, dir);
                    break;
            }

            tex.SetPixels(pixels);
            tex.Apply();

            var sprite = Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.4f), size);
            _spriteCache[key] = sprite;
            return sprite;
        }

        public static Sprite GetWeaponSprite(CharacterClassType classType, int size = 32)
        {
            switch (classType)
            {
                case CharacterClassType.Warrior:
                    return SpriteHelper.GetOrCreateSwordSprite(size);
                case CharacterClassType.Ranger:
                    return SpriteHelper.GetOrCreateBowSprite(size);
                case CharacterClassType.Wizard:
                    return WizardSpriteHelper.GetOrCreateStaffSprite(size);
                default:
                    return SpriteHelper.GetOrCreateSwordSprite(size);
            }
        }

        // =========================================================================
        // 1. ORIGINAL WARRIOR (32x32 Chibi Knight)
        // =========================================================================
        private static void DrawWarrior(Color[] pixels, int size, int cx, int cy, ViewDirection dir)
        {
            Color steelLight = new Color(0.85f, 0.88f, 0.92f);
            Color steelMid = new Color(0.55f, 0.60f, 0.68f);
            Color steelDark = new Color(0.25f, 0.28f, 0.35f);
            Color goldTrim = new Color(0.95f, 0.78f, 0.25f);
            Color capeRed = new Color(0.85f, 0.22f, 0.25f);
            Color eyeGlow = new Color(0.30f, 0.85f, 1.0f);
            Color leather = new Color(0.45f, 0.28f, 0.18f);

            bool isBack = dir == ViewDirection.Back || dir == ViewDirection.BackDiagonal;
            bool isSide = dir == ViewDirection.Side;
            bool isDiag = dir == ViewDirection.FrontDiagonal || dir == ViewDirection.BackDiagonal;

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    int dx = x - cx;
                    int dy = y - cy;

                    // 1. Red Cape on back
                    if (y >= 4 && y <= 16)
                    {
                        if (isBack)
                        {
                            if (Mathf.Abs(dx) <= 7) pixels[y * size + x] = (x < 0) ? capeRed : new Color(0.65f, 0.15f, 0.18f);
                        }
                        else
                        {
                            if (Mathf.Abs(dx) >= 6 && Mathf.Abs(dx) <= 10) pixels[y * size + x] = capeRed;
                        }
                    }

                    // 2. Torso Armor
                    if (y >= 6 && y <= 16 && Mathf.Abs(dx) <= 6)
                    {
                        pixels[y * size + x] = (Mathf.Abs(dx) == 6 || y == 6) ? steelDark : steelMid;
                        if (!isBack)
                        {
                            if (y == 8 && Mathf.Abs(dx) <= 5) pixels[y * size + x] = goldTrim;
                            if (y == 7 && Mathf.Abs(dx) <= 5) pixels[y * size + x] = leather;
                        }
                    }

                    // 3. Golden Shoulder Pads
                    if (y >= 13 && y <= 17 && (Mathf.Abs(dx) >= 6 && Mathf.Abs(dx) <= 9))
                    {
                        pixels[y * size + x] = goldTrim;
                    }

                    // 4. Round Helmet Head
                    int headDy = y - 22;
                    if (dx * dx + headDy * headDy <= 36)
                    {
                        pixels[y * size + x] = (dx * dx + headDy * headDy >= 25 || y == 28) ? steelDark : steelLight;

                        // Golden Crest
                        if (y >= 26 && Mathf.Abs(dx) <= (isSide ? 3 : 2))
                        {
                            pixels[y * size + x] = goldTrim;
                        }

                        // Visor Slit and Glowing Eyes
                        if (!isBack && y >= 20 && y <= 22 && Mathf.Abs(dx) <= (isSide ? 4 : 4))
                        {
                            pixels[y * size + x] = steelDark;
                            if (y == 21)
                            {
                                if (isSide)
                                {
                                    if (dx >= 0 && dx <= 2) pixels[y * size + x] = eyeGlow;
                                }
                                else if (isDiag)
                                {
                                    if (dx == -1 || dx == 3) pixels[y * size + x] = eyeGlow;
                                }
                                else // Front
                                {
                                    if (dx == -2 || dx == 2) pixels[y * size + x] = eyeGlow;
                                }
                            }
                        }
                    }

                    // 5. Boots
                    if (y >= 1 && y <= 5 && (Mathf.Abs(dx) >= 2 && Mathf.Abs(dx) <= 5))
                    {
                        pixels[y * size + x] = steelDark;
                    }
                }
            }
        }

        // =========================================================================
        // 2. ORIGINAL RANGER (32x32 Chibi Swift Hunter)
        // =========================================================================
        private static void DrawRanger(Color[] pixels, int size, int cx, int cy, ViewDirection dir)
        {
            Color hoodLight = new Color(0.25f, 0.80f, 0.40f);
            Color hoodMid = new Color(0.12f, 0.55f, 0.25f);
            Color hoodDark = new Color(0.06f, 0.32f, 0.14f);
            Color leatherLight = new Color(0.65f, 0.42f, 0.22f);
            Color leatherDark = new Color(0.38f, 0.22f, 0.10f);
            Color eyeGlow = new Color(1.0f, 0.90f, 0.30f);
            Color featherRed = new Color(0.95f, 0.25f, 0.25f);
            Color quiverGold = new Color(0.95f, 0.75f, 0.20f);

            bool isBack = dir == ViewDirection.Back || dir == ViewDirection.BackDiagonal;
            bool isSide = dir == ViewDirection.Side;
            bool isDiag = dir == ViewDirection.FrontDiagonal || dir == ViewDirection.BackDiagonal;

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    int dx = x - cx;
                    int dy = y - cy;

                    // 1. Quiver with arrows on back
                    if (dx >= 4 && dx <= 8 && dy >= -4 && dy <= 7)
                    {
                        pixels[y * size + x] = (dy >= 4) ? featherRed : ((dx == 4 || dx == 8) ? quiverGold : leatherDark);
                    }

                    // 2. Forest Cloak / Cape
                    if (Mathf.Abs(dx) <= 7 && dy >= -10 && dy <= -2)
                    {
                        pixels[y * size + x] = (Mathf.Abs(dx) == 7 || dy == -10) ? hoodDark : (isBack ? hoodLight : hoodMid);
                    }

                    // 3. Leather Tunic Body & Belt
                    if (Mathf.Abs(dx) <= 5 && dy >= -8 && dy <= 0)
                    {
                        if (dy == -4 && !isBack) pixels[y * size + x] = quiverGold; // Belt
                        else if (Mathf.Abs(dx) <= 4) pixels[y * size + x] = (dy > -4) ? leatherLight : leatherDark;
                        else pixels[y * size + x] = leatherDark;
                    }

                    // 4. Ranger Green Hood (Head)
                    if (Mathf.Abs(dx) <= 6 && dy >= 0 && dy <= 10)
                    {
                        if (dy >= 8 && Mathf.Abs(dx) > (10 - dy) + 1) continue;

                        if (Mathf.Abs(dx) == 6 || dy == 10 || (dy >= 8 && Mathf.Abs(dx) == (10 - dy) + 1))
                        {
                            pixels[y * size + x] = hoodDark;
                        }
                        else if (!isBack && dx >= -4 && dx <= 4 && dy >= 2 && dy <= 6)
                        {
                            // Shadowed face area inside hood
                            pixels[y * size + x] = new Color(0.10f, 0.12f, 0.14f);
                            // Keen golden eyes
                            if (dy == 4)
                            {
                                if (isSide)
                                {
                                    if (dx >= 0 && dx <= 2) pixels[y * size + x] = eyeGlow;
                                }
                                else if (isDiag)
                                {
                                    if (dx == -1 || dx == 3) pixels[y * size + x] = eyeGlow;
                                }
                                else // Front
                                {
                                    if (dx == -2 || dx == 2) pixels[y * size + x] = eyeGlow;
                                }
                            }
                        }
                        else
                        {
                            pixels[y * size + x] = (dy >= 7 || dx < 0) ? hoodLight : hoodMid;
                        }
                    }

                    // 5. Feather on hood side
                    if (dx >= -8 && dx <= -5 && dy >= 8 && dy <= 12)
                    {
                        if (dy - 8 == -(dx + 5)) pixels[y * size + x] = featherRed;
                    }

                    // 6. Leather Boots
                    if (Mathf.Abs(dx) >= 2 && Mathf.Abs(dx) <= 4 && dy >= -12 && dy <= -9)
                    {
                        pixels[y * size + x] = leatherDark;
                    }
                }
            }
        }

        // =========================================================================
        // 3. ORIGINAL WIZARD (32x32 Chibi Arcane Mage)
        // =========================================================================
        private static void DrawWizard(Color[] pixels, int size, int cx, int cy, ViewDirection dir)
        {
            Color robeLight = new Color(0.62f, 0.35f, 0.88f);
            Color robeMid = new Color(0.42f, 0.18f, 0.65f);
            Color robeDark = new Color(0.24f, 0.08f, 0.40f);
            Color gold = new Color(1.0f, 0.84f, 0.25f);
            Color eyeCyan = new Color(0.30f, 0.95f, 1.0f);
            Color trimGold = new Color(0.95f, 0.75f, 0.20f);

            bool isBack = dir == ViewDirection.Back || dir == ViewDirection.BackDiagonal;
            bool isSide = dir == ViewDirection.Side;
            bool isDiag = dir == ViewDirection.FrontDiagonal || dir == ViewDirection.BackDiagonal;

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    int dx = x - cx;
                    int dy = y - cy;

                    // 1. Flowing Robe Bottom
                    if (Mathf.Abs(dx) <= 7 && dy >= -10 && dy <= -2)
                    {
                        if (dy == -10 || Mathf.Abs(dx) == 7)
                            pixels[y * size + x] = (dy == -10) ? trimGold : robeDark;
                        else
                            pixels[y * size + x] = (dx == 0 && !isBack) ? trimGold : ((Mathf.Abs(dx) <= 4) ? robeLight : robeMid);
                    }

                    // 2. Wizard Belt & Golden Buckle
                    if (Mathf.Abs(dx) <= 5 && dy == -2)
                    {
                        pixels[y * size + x] = (Mathf.Abs(dx) <= 1 && !isBack) ? gold : robeDark;
                    }

                    // 3. Shadowed Face / Head
                    if (Mathf.Abs(dx) <= 5 && dy >= 0 && dy <= 6)
                    {
                        if (isBack)
                        {
                            pixels[y * size + x] = (dx < 0) ? robeLight : robeMid;
                        }
                        else
                        {
                            pixels[y * size + x] = new Color(0.12f, 0.08f, 0.20f);
                            // Glowing cyan eyes
                            if (dy == 3)
                            {
                                if (isSide)
                                {
                                    if (dx >= 0 && dx <= 2) pixels[y * size + x] = eyeCyan;
                                }
                                else if (isDiag)
                                {
                                    if (dx == -1 || dx == 3) pixels[y * size + x] = eyeCyan;
                                }
                                else // Front
                                {
                                    if (dx == -2 || dx == 2) pixels[y * size + x] = eyeCyan;
                                }
                            }
                        }
                    }

                    // 4. Pointed Wizard Hat Brim
                    if (Mathf.Abs(dx) <= 8 && dy == 7)
                    {
                        pixels[y * size + x] = (Mathf.Abs(dx) <= 2 && !isBack) ? gold : robeDark;
                    }

                    // 5. Pointed Wizard Hat Cone
                    if (dy >= 8 && dy <= 14)
                    {
                        int coneWidth = 6 - (dy - 8);
                        if (Mathf.Abs(dx) <= coneWidth)
                        {
                            if (dy == 8 && !isBack) pixels[y * size + x] = gold; // Hat band
                            else if (Mathf.Abs(dx) == coneWidth) pixels[y * size + x] = robeDark;
                            else pixels[y * size + x] = (dx <= 0) ? robeLight : robeMid;
                        }
                    }

                    // 6. Hat Tip Curled Star/Gold
                    if (dx == 1 && dy == 15)
                    {
                        pixels[y * size + x] = gold;
                    }
                }
            }
        }
    }
}
