using UnityEngine;

namespace HappyShoot.View.Utils
{
    /// <summary>
    /// Procedural pixel-art icon drawer for Ranger skills (Bow, Arrow Rain, Wind Glaive, Storm Bow, Phantom Glaive, Stellar Rain).
    /// Modularized to strictly respect the 500-line architecture rule.
    /// </summary>
    public static class RangerRewardIconHelper
    {
        public static void DrawPiercingArrowIcon(Color[] pixels, int size, int cx, int cy)
        {
            Color wood = new Color(0.72f, 0.45f, 0.20f);
            Color woodDark = new Color(0.45f, 0.25f, 0.10f);
            Color stringCol = new Color(0.9f, 0.9f, 0.95f);
            Color arrowGold = new Color(1.0f, 0.85f, 0.25f);
            Color arrowCore = new Color(1.0f, 1.0f, 0.90f);

            // Bow curve
            for (int y = -28; y <= 28; y++)
            {
                int xOffset = (int)(22f - (y * y) / 36f);
                SetPixel(pixels, size, cx - 10 + xOffset, cy + y, wood);
                SetPixel(pixels, size, cx - 11 + xOffset, cy + y, woodDark);
            }

            // String
            for (int y = -28; y <= 28; y++)
            {
                SetPixel(pixels, size, cx - 10 + 22, cy + y, stringCol);
            }

            // Golden Piercing Arrow Shaft & Head
            for (int x = -28; x <= 26; x++)
            {
                SetPixel(pixels, size, cx + x, cy, arrowGold);
                SetPixel(pixels, size, cx + x, cy + 1, arrowCore);
            }

            // Arrowhead Barbs
            for (int b = 1; b <= 6; b++)
            {
                SetPixel(pixels, size, cx + 26 - b, cy + b, arrowGold);
                SetPixel(pixels, size, cx + 26 - b, cy - b, arrowGold);
            }
        }

        public static void DrawArrowRainIcon(Color[] pixels, int size, int cx, int cy)
        {
            Color magicGold = new Color(1.0f, 0.85f, 0.25f);
            Color magicGlow = new Color(0.30f, 0.95f, 0.70f); // Vibrant Emerald-Cyan
            Color arrowCore = new Color(1.0f, 1.0f, 0.95f);
            Color arrowShaft = new Color(0.20f, 0.65f, 0.45f);
            Color fletching = new Color(0.85f, 0.35f, 0.15f); // Fiery Orange Fletching
            Color targetRing = new Color(1.0f, 0.40f, 0.20f, 0.85f);
            Color spark = new Color(1.0f, 0.95f, 0.50f);

            // 1. Target Magic Impact Circle on Ground (Lower Half)
            int targetCy = cy - 14;
            for (int y = -10; y <= 10; y++)
            {
                for (int x = -26; x <= 26; x++)
                {
                    // Elliptical ground reticle (2.5D perspective)
                    float dist = (x * x) / (24f * 24f) + (y * y) / (8f * 8f);
                    if (dist >= 0.75f && dist <= 1.05f)
                    {
                        SetPixel(pixels, size, cx + x, targetCy + y, targetRing);
                    }
                    else if (dist >= 0.20f && dist <= 0.40f)
                    {
                        SetPixel(pixels, size, cx + x, targetCy + y, magicGold);
                    }
                }
            }

            // 2. 5 Powerful Downward Raining Volley Arrows (Converging towards target center)
            // Staggered layout: Left-Outer, Left-Inner, Center, Right-Inner, Right-Outer
            int[] arrowStartX = { -20, -10, 0, 10, 20 };
            int[] arrowStartY = { 26, 30, 32, 29, 25 };
            int[] arrowLen = { 28, 32, 34, 31, 27 };
            float[] angleOffsets = { 0.22f, 0.10f, 0.0f, -0.10f, -0.22f }; // Slight inward convergence

            for (int a = 0; a < 5; a++)
            {
                int sx = arrowStartX[a];
                int sy = arrowStartY[a];
                int len = arrowLen[a];
                float conv = angleOffsets[a];

                for (int i = 0; i < len; i++)
                {
                    int ax = cx + sx + (int)(i * conv);
                    int ay = cy + sy - i;

                    if (i >= len - 5)
                    {
                        // Razor-sharp Broadhead Arrow Tip (Downwards)
                        int tipProgress = i - (len - 5);
                        SetPixel(pixels, size, ax, ay, arrowCore);
                        SetPixel(pixels, size, ax - 1, ay + 1, magicGold);
                        SetPixel(pixels, size, ax + 1, ay + 1, magicGold);
                        if (tipProgress >= 3)
                        {
                            SetPixel(pixels, size, ax - 2, ay + 2, magicGold);
                            SetPixel(pixels, size, ax + 2, ay + 2, magicGold);
                        }
                    }
                    else if (i <= 4)
                    {
                        // V-Shaped Feather Fletching at the top
                        SetPixel(pixels, size, ax, ay, arrowShaft);
                        SetPixel(pixels, size, ax - (i + 1), ay + 1, fletching);
                        SetPixel(pixels, size, ax + (i + 1), ay + 1, fletching);
                    }
                    else
                    {
                        // Glowing Magic Shaft & Speedline Trail
                        SetPixel(pixels, size, ax, ay, arrowShaft);
                        SetPixel(pixels, size, ax - 1, ay, magicGlow);
                        SetPixel(pixels, size, ax + 1, ay, magicGlow);
                    }
                }

                // Impact sparks at arrowhead landing
                int tipX = cx + sx + (int)(len * conv);
                int tipY = cy + sy - len;
                SetPixel(pixels, size, tipX - 2, tipY - 1, spark);
                SetPixel(pixels, size, tipX + 2, tipY - 1, spark);
                SetPixel(pixels, size, tipX, tipY - 2, magicGold);
            }
        }

        public static void DrawWindGlaiveIcon(Color[] pixels, int size, int cx, int cy)
        {
            float center = size * 0.5f;
            float maxR = size * 0.44f;

            Color coreColor = new Color(0.95f, 1.0f, 0.98f, 1.0f);
            Color bladeColor = new Color(0.20f, 0.90f, 0.70f, 0.95f);
            Color edgeColor = new Color(0.05f, 0.50f, 0.40f, 0.90f);

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
                        Color c = t < 0.3f
                            ? Color.Lerp(coreColor, bladeColor, t / 0.3f)
                            : Color.Lerp(bladeColor, edgeColor, (t - 0.3f) / 0.7f);
                        pixels[y * size + x] = c;
                    }
                }
            }
        }

        public static void DrawStellarRainIcon(Color[] pixels, int size, int cx, int cy)
        {
            Color starGold = new Color(1.0f, 0.95f, 0.40f);
            Color cosmosPurple = new Color(0.55f, 0.20f, 0.90f);
            Color meteorCyan = new Color(0.30f, 0.90f, 1.0f);

            // 1. Cosmic Night Starry Background
            for (int y = 14; y <= 28; y++)
            {
                for (int x = -28; x <= 28; x++)
                {
                    if (Mathf.Abs(x) < 26)
                    {
                        SetPixel(pixels, size, cx + x, cy + y, cosmosPurple);
                    }
                }
            }

            // 2. Golden Meteor Shower Streams
            int[] sx = { -18, -4, 10, 22 };
            for (int a = 0; a < sx.Length; a++)
            {
                for (int i = 0; i < 26; i++)
                {
                    int ax = cx + sx[a] + (int)(i * 0.6f);
                    int ay = cy + 18 - i;
                    SetPixel(pixels, size, ax, ay, i > 20 ? starGold : meteorCyan);
                    SetPixel(pixels, size, ax + 1, ay, starGold);
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
