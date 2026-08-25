using System.Collections.Generic;
using UnityEngine;
using HappyShoot.Domain.Entities;

namespace HappyShoot.View.Utils
{
    /// <summary>
    /// Generates high-polish, cute Chibi-style 9-directional sprites for Heroes and Weapons.
    /// Preserves original beloved silhouette & design while enhancing shading, highlights, and 9-way viewing angles.
    /// Strictly modular and under 500 lines.
    /// </summary>
    public static class HeroSpriteHelper
    {
        public enum ViewDirection
        {
            Front,          // S (South / Center)
            FrontDiagonal,  // SE / SW
            Side,           // E / W
            BackDiagonal,   // NE / NW
            Back            // N (North)
        }

        private static readonly Dictionary<string, Sprite> _spriteCache = new Dictionary<string, Sprite>(64);

        public static Sprite GetHeroSprite(CharacterClassType classType, ViewDirection dir, int size = 36)
        {
            string key = $"{classType}_{dir}_{size}";
            if (_spriteCache.TryGetValue(key, out var cached) && cached != null)
                return cached;

            var tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            tex.filterMode = FilterMode.Point;
            Color[] pixels = new Color[size * size];
            for (int i = 0; i < pixels.Length; i++) pixels[i] = Color.clear;

            int cx = size / 2;
            int cy = size / 2;

            switch (classType)
            {
                case CharacterClassType.Warrior:
                    DrawChibiWarrior(pixels, size, cx, cy, dir);
                    break;
                case CharacterClassType.Ranger:
                    DrawChibiRanger(pixels, size, cx, cy, dir);
                    break;
                case CharacterClassType.Wizard:
                    DrawChibiWizard(pixels, size, cx, cy, dir);
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
            string key = $"Weapon_{classType}_{size}";
            if (_spriteCache.TryGetValue(key, out var cached) && cached != null)
                return cached;

            var tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            tex.filterMode = FilterMode.Point;
            Color[] pixels = new Color[size * size];
            for (int i = 0; i < pixels.Length; i++) pixels[i] = Color.clear;

            int cx = size / 2;
            int cy = size / 2;

            switch (classType)
            {
                case CharacterClassType.Warrior:
                    DrawChibiSword(pixels, size, cx, cy);
                    break;
                case CharacterClassType.Ranger:
                    DrawChibiBow(pixels, size, cx, cy);
                    break;
                case CharacterClassType.Wizard:
                    DrawChibiStaff(pixels, size, cx, cy);
                    break;
            }

            tex.SetPixels(pixels);
            tex.Apply();

            var sprite = Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), size);
            _spriteCache[key] = sprite;
            return sprite;
        }

        // =========================================================================
        // 1. WARRIOR (Chibi Knight - Cute Steel Helmet, Gold Crest, Red Cape)
        // =========================================================================
        private static void DrawChibiWarrior(Color[] pixels, int size, int cx, int cy, ViewDirection dir)
        {
            Color steelHi = new Color(0.92f, 0.94f, 0.98f);
            Color steelLight = new Color(0.80f, 0.84f, 0.90f);
            Color steelMid = new Color(0.52f, 0.58f, 0.68f);
            Color steelDark = new Color(0.24f, 0.27f, 0.35f);
            Color goldTrim = new Color(1.0f, 0.82f, 0.22f);
            Color goldDark = new Color(0.75f, 0.55f, 0.12f);
            Color capeRed = new Color(0.90f, 0.22f, 0.26f);
            Color capeDark = new Color(0.55f, 0.10f, 0.14f);
            Color eyeCyan = new Color(0.35f, 0.92f, 1.0f);
            Color leather = new Color(0.48f, 0.28f, 0.16f);

            bool isBack = dir == ViewDirection.Back || dir == ViewDirection.BackDiagonal;
            bool isSide = dir == ViewDirection.Side;
            bool isDiag = dir == ViewDirection.FrontDiagonal || dir == ViewDirection.BackDiagonal;

            // Red Cape (Background)
            for (int y = -14; y <= 2; y++)
            {
                int capeW = isSide ? (int)(7 - y * 0.25f) : (int)(9 - y * 0.3f);
                for (int x = -capeW; x <= capeW; x++)
                {
                    int ox = isSide ? x - 2 : x;
                    if (isBack || Mathf.Abs(x) >= 5)
                    {
                        SetPixel(pixels, size, cx + ox, cy + y, (x < 0 || isBack) ? capeRed : capeDark);
                    }
                }
            }

            // Body Armor (Torso & Pauldrons)
            for (int y = -10; y <= 0; y++)
            {
                for (int x = -7; x <= 7; x++)
                {
                    if (Mathf.Abs(x) <= 5)
                    {
                        if (y == -8) SetPixel(pixels, size, cx + x, cy + y, leather);
                        else if (y == -7) SetPixel(pixels, size, cx + x, cy + y, goldTrim);
                        else SetPixel(pixels, size, cx + x, cy + y, (Mathf.Abs(x) == 5 || y == -10) ? steelDark : (x < 0 ? steelLight : steelMid));
                    }
                    // Shoulder Pauldrons
                    if (y >= -3 && y <= 0 && Mathf.Abs(x) >= 5 && Mathf.Abs(x) <= 7)
                    {
                        SetPixel(pixels, size, cx + x, cy + y, (Mathf.Abs(x) == 7) ? goldDark : goldTrim);
                    }
                }
            }

            // Cute Round Steel Helmet (Head)
            int headCy = 6;
            for (int y = 0; y <= 14; y++)
            {
                for (int x = -7; x <= 7; x++)
                {
                    int dy = y - headCy;
                    if (x * x + dy * dy <= 49)
                    {
                        // Outer rim outline
                        if (x * x + dy * dy >= 36)
                        {
                            SetPixel(pixels, size, cx + x, cy + y, steelDark);
                        }
                        else
                        {
                            // Top Golden Crest
                            if (y >= 10 && Mathf.Abs(x) <= (isSide ? 3 : 2))
                            {
                                SetPixel(pixels, size, cx + x, cy + y, goldTrim);
                            }
                            // Visor Slit & Glowing Eyes
                            else if (!isBack && y >= 3 && y <= 6 && Mathf.Abs(x) <= (isSide ? 5 : 6))
                            {
                                SetPixel(pixels, size, cx + x, cy + y, steelDark);
                                if (y == 5)
                                {
                                    if (isSide)
                                    {
                                        if (x >= 0 && x <= 2) SetPixel(pixels, size, cx + x, cy + y, eyeCyan);
                                    }
                                    else if (isDiag)
                                    {
                                        if (x == -1 || x == 3) SetPixel(pixels, size, cx + x, cy + y, eyeCyan);
                                    }
                                    else // Front
                                    {
                                        if (x == -2 || x == 2) SetPixel(pixels, size, cx + x, cy + y, eyeCyan);
                                    }
                                }
                            }
                            else
                            {
                                SetPixel(pixels, size, cx + x, cy + y, (x < -1) ? steelHi : (x < 2 ? steelLight : steelMid));
                            }
                        }
                    }
                }
            }

            // Boots
            for (int y = -14; y <= -11; y++)
            {
                for (int b = -4; b <= 4; b += 8)
                {
                    int bx = isSide ? (b < 0 ? -2 : 2) : b;
                    for (int x = -2; x <= 2; x++)
                    {
                        SetPixel(pixels, size, cx + bx + x, cy + y, steelDark);
                    }
                }
            }
        }

        // =========================================================================
        // 2. RANGER (Chibi Hunter - Green Hood, Keen Gold Eyes, Feather, Quiver)
        // =========================================================================
        private static void DrawChibiRanger(Color[] pixels, int size, int cx, int cy, ViewDirection dir)
        {
            Color hoodHi = new Color(0.35f, 0.88f, 0.45f);
            Color hoodLight = new Color(0.22f, 0.72f, 0.32f);
            Color hoodMid = new Color(0.12f, 0.52f, 0.22f);
            Color hoodDark = new Color(0.06f, 0.30f, 0.12f);
            Color leatherLight = new Color(0.70f, 0.45f, 0.22f);
            Color leatherDark = new Color(0.40f, 0.22f, 0.10f);
            Color goldTrim = new Color(1.0f, 0.85f, 0.25f);
            Color eyeGold = new Color(1.0f, 0.92f, 0.35f);
            Color featherRed = new Color(0.95f, 0.25f, 0.25f);

            bool isBack = dir == ViewDirection.Back || dir == ViewDirection.BackDiagonal;
            bool isSide = dir == ViewDirection.Side;
            bool isDiag = dir == ViewDirection.FrontDiagonal || dir == ViewDirection.BackDiagonal;

            // Quiver on Back
            if (isBack || isSide || isDiag)
            {
                int qx = isSide ? -4 : 4;
                for (int y = -6; y <= 8; y++)
                {
                    for (int x = -2; x <= 2; x++)
                    {
                        if (y >= 5) SetPixel(pixels, size, cx + qx + x, cy + y, featherRed);
                        else SetPixel(pixels, size, cx + qx + x, cy + y, (Mathf.Abs(x) == 2) ? goldTrim : leatherDark);
                    }
                }
            }

            // Forest Cloak / Cape
            for (int y = -14; y <= 0; y++)
            {
                int cloakW = isSide ? (int)(6 - y * 0.22f) : (int)(8 - y * 0.28f);
                for (int x = -cloakW; x <= cloakW; x++)
                {
                    int ox = isSide ? x - 2 : x;
                    if (isBack || Mathf.Abs(x) >= 4)
                    {
                        SetPixel(pixels, size, cx + ox, cy + y, (x < 0 || isBack) ? hoodLight : hoodMid);
                    }
                }
            }

            // Leather Tunic & Belt
            for (int y = -10; y <= 0; y++)
            {
                for (int x = -5; x <= 5; x++)
                {
                    if (y == -6) SetPixel(pixels, size, cx + x, cy + y, goldTrim);
                    else if (y == -7) SetPixel(pixels, size, cx + x, cy + y, leatherDark);
                    else SetPixel(pixels, size, cx + x, cy + y, (Mathf.Abs(x) == 5 || y == -10) ? leatherDark : (x < 0 ? leatherLight : leatherDark));
                }
            }

            // Cute Green Hood (Head)
            int headCy = 6;
            for (int y = 0; y <= 15; y++)
            {
                for (int x = -7; x <= 7; x++)
                {
                    int dy = y - headCy;
                    if (y >= 12 && Mathf.Abs(x) > (15 - y)) continue;

                    if (x * x + dy * dy <= 49 || y >= 12)
                    {
                        if (x * x + dy * dy >= 38 || y == 15)
                        {
                            SetPixel(pixels, size, cx + x, cy + y, hoodDark);
                        }
                        else if (!isBack && y >= 3 && y <= 7 && Mathf.Abs(x) <= (isSide ? 5 : 5))
                        {
                            // Shadowed face with cute glowing eyes
                            SetPixel(pixels, size, cx + x, cy + y, new Color(0.10f, 0.12f, 0.14f));
                            if (y == 5)
                            {
                                if (isSide)
                                {
                                    if (x >= 0 && x <= 2) SetPixel(pixels, size, cx + x, cy + y, eyeGold);
                                }
                                else if (isDiag)
                                {
                                    if (x == -1 || x == 3) SetPixel(pixels, size, cx + x, cy + y, eyeGold);
                                }
                                else // Front
                                {
                                    if (x == -2 || x == 2) SetPixel(pixels, size, cx + x, cy + y, eyeGold);
                                }
                            }
                        }
                        else
                        {
                            SetPixel(pixels, size, cx + x, cy + y, (x < -1 || y >= 11) ? hoodHi : (x < 2 ? hoodLight : hoodMid));
                        }
                    }
                }
            }

            // Feather on side of hood
            for (int f = 0; f < 5; f++)
            {
                SetPixel(pixels, size, cx - 6 - f, cy + 9 + f, featherRed);
            }
        }

        // =========================================================================
        // 3. WIZARD (Chibi Mage - Violet Robe, Pointy Hat with Gold Buckle, Cyan Eyes)
        // =========================================================================
        private static void DrawChibiWizard(Color[] pixels, int size, int cx, int cy, ViewDirection dir)
        {
            Color robeHi = new Color(0.75f, 0.48f, 0.98f);
            Color robeLight = new Color(0.58f, 0.32f, 0.85f);
            Color robeMid = new Color(0.38f, 0.16f, 0.62f);
            Color robeDark = new Color(0.20f, 0.08f, 0.38f);
            Color goldTrim = new Color(1.0f, 0.85f, 0.25f);
            Color eyeCyan = new Color(0.35f, 0.95f, 1.0f);

            bool isBack = dir == ViewDirection.Back || dir == ViewDirection.BackDiagonal;
            bool isSide = dir == ViewDirection.Side;
            bool isDiag = dir == ViewDirection.FrontDiagonal || dir == ViewDirection.BackDiagonal;

            // Flowing Robe
            for (int y = -14; y <= 0; y++)
            {
                int robeW = (int)(7 - y * 0.3f);
                for (int x = -robeW; x <= robeW; x++)
                {
                    if (y == -14 || Mathf.Abs(x) == robeW)
                    {
                        SetPixel(pixels, size, cx + x, cy + y, (y == -14) ? goldTrim : robeDark);
                    }
                    else if (y == -4)
                    {
                        SetPixel(pixels, size, cx + x, cy + y, (Mathf.Abs(x) <= 1) ? goldTrim : robeDark); // Belt
                    }
                    else
                    {
                        SetPixel(pixels, size, cx + x, cy + y, (x == 0 && !isBack) ? goldTrim : (x < 0 ? robeLight : robeMid));
                    }
                }
            }

            // Shadowed Face Area
            if (!isBack)
            {
                for (int y = 1; y <= 6; y++)
                {
                    for (int x = -5; x <= 5; x++)
                    {
                        SetPixel(pixels, size, cx + x, cy + y, new Color(0.12f, 0.08f, 0.20f));
                        if (y == 4)
                        {
                            if (isSide)
                            {
                                if (x >= 0 && x <= 2) SetPixel(pixels, size, cx + x, cy + y, eyeCyan);
                            }
                            else if (isDiag)
                            {
                                if (x == -1 || x == 3) SetPixel(pixels, size, cx + x, cy + y, eyeCyan);
                            }
                            else
                            {
                                if (x == -2 || x == 2) SetPixel(pixels, size, cx + x, cy + y, eyeCyan);
                            }
                        }
                    }
                }
            }

            // Pointed Wizard Hat (Brim)
            for (int x = -8; x <= 8; x++)
            {
                SetPixel(pixels, size, cx + x, cy + 7, (Mathf.Abs(x) <= 2) ? goldTrim : robeDark);
                SetPixel(pixels, size, cx + x, cy + 6, robeDark);
            }

            // Pointed Wizard Hat (Cone)
            for (int y = 8; y <= 16; y++)
            {
                int coneW = (int)((16 - y) * 0.75f);
                for (int x = -coneW; x <= coneW; x++)
                {
                    if (y == 8)
                    {
                        SetPixel(pixels, size, cx + x, cy + y, goldTrim); // Gold Band
                    }
                    else if (Mathf.Abs(x) == coneW || y == 16)
                    {
                        SetPixel(pixels, size, cx + x, cy + y, robeDark);
                    }
                    else
                    {
                        SetPixel(pixels, size, cx + x, cy + y, (x <= 0) ? robeHi : robeMid);
                    }
                }
            }

            // Gold star on hat tip
            SetPixel(pixels, size, cx + 1, cy + 17, goldTrim);
        }

        // =========================================================================
        // 4. WEAPONS (Chibi Broadsword, Curved Recurve Bow, Crystal Arcane Staff)
        // =========================================================================
        private static void DrawChibiSword(Color[] pixels, int size, int cx, int cy)
        {
            Color steelLight = new Color(0.92f, 0.95f, 0.98f);
            Color steelMid = new Color(0.65f, 0.72f, 0.82f);
            Color steelDark = new Color(0.28f, 0.32f, 0.40f);
            Color gold = new Color(1.0f, 0.82f, 0.22f);
            Color groove = new Color(0.40f, 0.48f, 0.60f);

            for (int y = 6; y < size - 4; y++)
            {
                for (int x = cx - 2; x <= cx + 2; x++)
                {
                    if (y >= 12)
                    {
                        // Blade
                        if (x == cx) SetPixel(pixels, size, x, y, groove);
                        else if (x < cx) SetPixel(pixels, size, x, y, steelLight);
                        else SetPixel(pixels, size, x, y, steelMid);
                    }
                    else if (y >= 10)
                    {
                        // Crossguard
                        for (int gx = cx - 4; gx <= cx + 4; gx++)
                            SetPixel(pixels, size, gx, y, gold);
                    }
                    else
                    {
                        // Grip & Pommel
                        if (x == cx) SetPixel(pixels, size, x, y, (y <= 7) ? gold : steelDark);
                    }
                }
            }
        }

        private static void DrawChibiBow(Color[] pixels, int size, int cx, int cy)
        {
            Color wood = new Color(0.72f, 0.48f, 0.25f);
            Color gold = new Color(1.0f, 0.82f, 0.22f);
            Color stringCol = new Color(0.92f, 0.95f, 1.0f, 0.9f);

            for (int y = 4; y < size - 4; y++)
            {
                int dy = y - cy;
                int curveX = cx + (int)(Mathf.Sqrt(Mathf.Max(0, 100 - dy * dy * 0.7f)) * 0.55f);
                SetPixel(pixels, size, curveX, y, (Mathf.Abs(dy) >= 8) ? gold : wood);
                SetPixel(pixels, size, cx - 2, y, stringCol);
            }
        }

        private static void DrawChibiStaff(Color[] pixels, int size, int cx, int cy)
        {
            Color wood = new Color(0.45f, 0.28f, 0.16f);
            Color gold = new Color(1.0f, 0.82f, 0.22f);
            Color crystalCyan = new Color(0.35f, 0.95f, 1.0f);
            Color crystalCore = new Color(0.92f, 0.98f, 1.0f);

            // Shaft
            for (int y = 4; y <= 20; y++)
            {
                SetPixel(pixels, size, cx, y, wood);
                SetPixel(pixels, size, cx + 1, y, gold);
            }

            // Head Crescent & Orb
            for (int y = 20; y <= 28; y++)
            {
                for (int x = cx - 4; x <= cx + 4; x++)
                {
                    float dist = Mathf.Sqrt((x - cx) * (x - cx) + (y - 24) * (y - 24));
                    if (dist <= 2.8f) SetPixel(pixels, size, x, y, dist <= 1.2f ? crystalCore : crystalCyan);
                    else if (dist <= 4.2f && (y <= 23 || Mathf.Abs(x - cx) >= 2)) SetPixel(pixels, size, x, y, gold);
                }
            }
        }

        private static void SetPixel(Color[] pixels, int size, int x, int y, Color color)
        {
            if (x >= 0 && x < size && y >= 0 && y < size)
            {
                pixels[y * size + x] = color;
            }
        }
    }
}
