using UnityEngine;

namespace HappyShoot.View.Utils
{
    /// <summary>
    /// Procedural pixel-art icon drawer for Warrior skills (Slash, Whirlwind, Ground Stomp, Blood Eater, Tempest Whirlwind, Earthshaker).
    /// Modularized to strictly respect the 500-line architecture rule.
    /// </summary>
    public static class WarriorRewardIconHelper
    {
        public static void DrawGreatswordIcon(Color[] pixels, int size, int cx, int cy)
        {
            Color bladeLight = new Color(0.95f, 0.96f, 1.0f);
            Color bladeMid = new Color(0.70f, 0.78f, 0.88f);
            Color gold = new Color(1.0f, 0.85f, 0.25f);
            Color grip = new Color(0.65f, 0.15f, 0.15f);

            for (int d = -26; d <= 26; d++)
            {
                int x = cx + d;
                int y = cy + d;
                if (d > -12)
                {
                    SetPixel(pixels, size, x, y, bladeLight);
                    SetPixel(pixels, size, x - 1, y + 1, bladeLight);
                    SetPixel(pixels, size, x + 1, y - 1, bladeMid);
                    SetPixel(pixels, size, x - 2, y + 2, bladeMid);
                    SetPixel(pixels, size, x + 2, y - 2, bladeMid);
                }
                else if (d >= -16)
                {
                    for (int w = -8; w <= 8; w++)
                        SetPixel(pixels, size, x + w, y - w, gold);
                }
                else
                {
                    SetPixel(pixels, size, x, y, grip);
                    SetPixel(pixels, size, x - 1, y + 1, grip);
                }
            }
        }

        public static void DrawWhirlwindIcon(Color[] pixels, int size, int cx, int cy)
        {
            Color steel = new Color(0.85f, 0.90f, 1.0f);
            Color windCyan = new Color(0.30f, 0.85f, 1.0f);
            Color trail = new Color(0.20f, 0.50f, 0.80f, 0.60f);

            // 3 Spinning Curved Cyclone Blades
            for (int b = 0; b < 3; b++)
            {
                float baseAng = (b * 120f) * Mathf.Deg2Rad;
                for (int r = 6; r <= 26; r++)
                {
                    float curvedAng = baseAng + (r * 0.08f);
                    int x = cx + (int)(Mathf.Cos(curvedAng) * r);
                    int y = cy + (int)(Mathf.Sin(curvedAng) * r);

                    SetPixel(pixels, size, x, y, steel);
                    SetPixel(pixels, size, x + 1, y, windCyan);
                    SetPixel(pixels, size, x, y + 1, trail);
                }
            }
        }

        public static void DrawGroundStompIcon(Color[] pixels, int size, int cx, int cy)
        {
            Color rock = new Color(0.35f, 0.28f, 0.22f);
            Color crackGold = new Color(1.0f, 0.80f, 0.20f);
            Color shockwave = new Color(0.95f, 0.50f, 0.15f);

            // Crater Ring & Cracks
            for (int y = -24; y <= 24; y++)
            {
                for (int x = -24; x <= 24; x++)
                {
                    float dist = Mathf.Sqrt(x * x + y * y);
                    if (dist >= 18f && dist <= 23f)
                    {
                        SetPixel(pixels, size, cx + x, cy + y, shockwave);
                    }
                    else if (dist <= 14f && (Mathf.Abs(x) == Mathf.Abs(y) || x == 0 || y == 0))
                    {
                        SetPixel(pixels, size, cx + x, cy + y, crackGold);
                    }
                    else if (dist <= 12f)
                    {
                        SetPixel(pixels, size, cx + x, cy + y, rock);
                    }
                }
            }
        }

        public static void DrawBloodEaterIcon(Color[] pixels, int size, int cx, int cy)
        {
            Color bloodRed = new Color(0.95f, 0.10f, 0.15f);
            Color darkCrimson = new Color(0.45f, 0.05f, 0.08f);
            Color gold = new Color(1.0f, 0.85f, 0.25f);

            DrawGreatswordIcon(pixels, size, cx, cy);

            // Blood Glow Overlay
            for (int y = -14; y <= 22; y++)
            {
                for (int x = -14; x <= 22; x++)
                {
                    if (Mathf.Abs(x - y) <= 3 && (x + y) > 0)
                    {
                        SetPixel(pixels, size, cx + x + 2, cy + y - 2, bloodRed);
                        SetPixel(pixels, size, cx + x - 2, cy + y + 2, darkCrimson);
                    }
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
