using UnityEngine;

namespace HappyShoot.View.Utils
{
    /// <summary>
    /// Procedural pixel-art icon drawer for all 9 Passive skill items.
    /// Modularized to strictly respect the 500-line architecture rule.
    /// </summary>
    public static class PassiveRewardIconHelper
    {
        public static void DrawVampireFangIcon(Color[] pixels, int size, int cx, int cy)
        {
            Color tooth = new Color(0.95f, 0.95f, 0.98f);
            Color blood = new Color(0.95f, 0.15f, 0.20f);

            // Left Fang
            for (int y = -18; y <= 18; y++)
            {
                int w = (int)((18 - y) * 0.45f);
                for (int x = -w; x <= w; x++)
                    SetPixel(pixels, size, cx - 12 + x, cy + y, y < -6 ? blood : tooth);
            }

            // Right Fang
            for (int y = -18; y <= 18; y++)
            {
                int w = (int)((18 - y) * 0.45f);
                for (int x = -w; x <= w; x++)
                    SetPixel(pixels, size, cx + 12 + x, cy + y, y < -6 ? blood : tooth);
            }
        }

        public static void DrawWindFeatherIcon(Color[] pixels, int size, int cx, int cy)
        {
            Color featherLight = new Color(0.85f, 0.95f, 1.0f);
            Color featherCyan = new Color(0.35f, 0.85f, 0.95f);
            Color quill = new Color(0.95f, 0.95f, 0.95f);

            for (int d = -24; d <= 24; d++)
            {
                int x = cx + (int)(d * 0.7f);
                int y = cy + d;
                SetPixel(pixels, size, x, y, quill);

                int vaneWidth = (int)((24 - Mathf.Abs(d)) * 0.55f);
                for (int v = 1; v <= vaneWidth; v++)
                {
                    SetPixel(pixels, size, x - v, y, featherLight);
                    SetPixel(pixels, size, x + v, y, featherCyan);
                }
            }
        }

        public static void DrawManaRuneIcon(Color[] pixels, int size, int cx, int cy)
        {
            Color runeGlow = new Color(0.40f, 0.75f, 1.0f);
            Color runeCore = new Color(0.85f, 0.95f, 1.0f);

            for (int y = -22; y <= 22; y++)
            {
                for (int x = -22; x <= 22; x++)
                {
                    int d = Mathf.Abs(x) + Mathf.Abs(y);
                    if (d >= 18 && d <= 22)
                        SetPixel(pixels, size, cx + x, cy + y, runeGlow);
                    else if (d <= 14 && (x == 0 || y == 0 || Mathf.Abs(x) == Mathf.Abs(y)))
                        SetPixel(pixels, size, cx + x, cy + y, runeCore);
                }
            }
        }

        public static void DrawIronArmorIcon(Color[] pixels, int size, int cx, int cy)
        {
            Color steel = new Color(0.70f, 0.75f, 0.85f);
            Color steelDark = new Color(0.35f, 0.40f, 0.50f);
            Color trim = new Color(0.95f, 0.80f, 0.30f);

            for (int y = -22; y <= 22; y++)
            {
                int w = y > 6 ? (int)((22 - y) * 1.2f) : (int)((y + 22) * 0.8f);
                for (int x = -w; x <= w; x++)
                {
                    if (Mathf.Abs(x) == w || y == -22)
                        SetPixel(pixels, size, cx + x, cy + y, trim);
                    else
                        SetPixel(pixels, size, cx + x, cy + y, x < 0 ? steel : steelDark);
                }
            }
        }

        public static void DrawGoldenRingIcon(Color[] pixels, int size, int cx, int cy)
        {
            Color gold = new Color(1.0f, 0.85f, 0.25f);
            Color goldDark = new Color(0.75f, 0.55f, 0.15f);
            Color gemRuby = new Color(0.95f, 0.20f, 0.25f);

            for (int y = -20; y <= 16; y++)
            {
                for (int x = -20; x <= 20; x++)
                {
                    float dist = Mathf.Sqrt(x * x + y * y);
                    if (dist >= 14f && dist <= 20f)
                        SetPixel(pixels, size, cx + x, cy + y, y > 0 ? gold : goldDark);
                }
            }

            // Top Gem
            for (int y = 14; y <= 24; y++)
            {
                for (int x = -5; x <= 5; x++)
                {
                    if (Mathf.Abs(x) + Mathf.Abs(y - 19) <= 5)
                        SetPixel(pixels, size, cx + x, cy + y, gemRuby);
                }
            }
        }

        public static void DrawHeartPendantIcon(Color[] pixels, int size, int cx, int cy)
        {
            Color ruby = new Color(0.95f, 0.15f, 0.25f);
            Color rubyLight = new Color(1.0f, 0.45f, 0.55f);
            Color chainGold = new Color(1.0f, 0.85f, 0.30f);

            for (int y = -18; y <= 14; y++)
            {
                for (int x = -18; x <= 18; x++)
                {
                    float nx = x / 14f;
                    float ny = y / 14f;
                    float heart = (nx * nx + ny * ny - 1f);
                    if (heart * heart * heart - nx * nx * ny * ny * ny <= 0.0f)
                    {
                        SetPixel(pixels, size, cx + x, cy + y, x < 0 ? rubyLight : ruby);
                    }
                }
            }

            // Chain loop
            for (int d = -6; d <= 6; d++)
            {
                SetPixel(pixels, size, cx + d, cy + 18, chainGold);
            }
        }

        public static void DrawCritEyeIcon(Color[] pixels, int size, int cx, int cy)
        {
            Color gold = new Color(1.0f, 0.85f, 0.15f);
            Color neonRed = new Color(1.0f, 0.20f, 0.25f);
            Color shine = new Color(1.0f, 1.0f, 0.80f);

            for (int y = -22; y <= 22; y++)
            {
                for (int x = -22; x <= 22; x++)
                {
                    float dist = Mathf.Sqrt(x * x + y * y);
                    if (dist >= 17f && dist <= 21f)
                    {
                        SetPixel(pixels, size, cx + x, cy + y, gold);
                    }
                    else if (dist <= 12f)
                    {
                        SetPixel(pixels, size, cx + x, cy + y, neonRed);
                    }
                }
            }

            for (int i = 12; i <= 26; i++)
            {
                SetPixel(pixels, size, cx + i, cy, gold);
                SetPixel(pixels, size, cx - i, cy, gold);
                SetPixel(pixels, size, cx, cy + i, gold);
                SetPixel(pixels, size, cx, cy - i, gold);
            }

            for (int y = -3; y <= 3; y++)
            {
                for (int x = -3; x <= 3; x++)
                {
                    if (Mathf.Abs(x) + Mathf.Abs(y) <= 4)
                        SetPixel(pixels, size, cx + x, cy + y, shine);
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
