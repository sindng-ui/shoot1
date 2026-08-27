using System;
using UnityEngine;

namespace HappyShoot.View.SkillTree
{
    /// <summary>
    /// Generates high-res ancient cosmic stone dial and constellation rune circle backgrounds.
    /// Strictly modular and under 500 lines.
    /// </summary>
    public static class SkillTreeBackgroundHelper
    {
        private static Sprite _dialSprite;

        /// <summary>
        /// Creates a gorgeous 512x512 ancient astronomical stone dial with concentric rune rings
        /// and 120-degree elemental division fissures.
        /// </summary>
        public static Sprite GetOrCreateDialSprite(int size = 512)
        {
            if (_dialSprite != null) return _dialSprite;

            var tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            tex.filterMode = FilterMode.Bilinear;
            Color[] pixels = new Color[size * size];

            int cx = size / 2;
            int cy = size / 2;
            float maxR = (size / 2f) - 4f;

            // Fissure angles (30°, 150°, 270°)
            float rad30 = 30f * Mathf.Deg2Rad;
            float rad150 = 150f * Mathf.Deg2Rad;
            float rad270 = 270f * Mathf.Deg2Rad;

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float dx = x - cx;
                    float dy = y - cy;
                    float dist = Mathf.Sqrt(dx * dx + dy * dy);

                    if (dist > maxR)
                    {
                        pixels[y * size + x] = Color.clear;
                        continue;
                    }

                    // Base slate disc gradient
                    float normR = dist / maxR;
                    float slateVal = Mathf.Lerp(0.09f, 0.05f, normR);
                    Color col = new Color(slateVal, slateVal + 0.01f, slateVal + 0.025f, 0.98f);

                    // Outer stone border rim
                    if (dist >= maxR - 8f)
                    {
                        col = new Color(0.18f, 0.22f, 0.30f, 1.0f);
                    }
                    else if (dist >= maxR - 12f)
                    {
                        col = new Color(0.25f, 0.30f, 0.40f, 0.8f);
                    }

                    // 4 Concentric Orbit Groove Rings
                    float[] orbitRadii = new float[] { maxR * 0.22f, maxR * 0.42f, maxR * 0.62f, maxR * 0.82f };
                    for (int i = 0; i < orbitRadii.Length; i++)
                    {
                        float dRing = Mathf.Abs(dist - orbitRadii[i]);
                        if (dRing <= 1.8f)
                        {
                            // Glowing or engraved rune groove
                            float grooveAlpha = 1f - (dRing / 1.8f);
                            Color ringCol = (i == 0)
                                ? new Color(0.85f, 0.70f, 0.25f, 0.55f * grooveAlpha) // Gold inner ring
                                : new Color(0.25f, 0.35f, 0.50f, 0.40f * grooveAlpha); // Cosmic blue
                            col = Color.Lerp(col, ringCol, ringCol.a);
                        }
                    }

                    // 3 Fissure dividing rays (30°, 150°, 270°)
                    float angle = Mathf.Atan2(dy, dx);
                    if (angle < 0) angle += Mathf.PI * 2;

                    float dAng1 = Mathf.Abs(angle - rad30);
                    float dAng2 = Mathf.Abs(angle - rad150);
                    float dAng3 = Mathf.Abs(angle - rad270);
                    float minDAng = Mathf.Min(dAng1, Mathf.Min(dAng2, dAng3));

                    // Fissure width in radians at this distance
                    if (dist > 30f && minDAng < (1.5f / dist))
                    {
                        col = Color.Lerp(col, new Color(0.03f, 0.04f, 0.06f, 1.0f), 0.75f);
                    }

                    pixels[y * size + x] = col;
                }
            }

            tex.SetPixels(pixels);
            tex.Apply();
            _dialSprite = Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 100);
            return _dialSprite;
        }
    }
}
