using UnityEngine;

namespace HappyShoot.View.UI
{
    /// <summary>
    /// Procedural pixel-art sprite generator for mobile virtual touch joystick.
    /// Provides cached sprites for joystick outer base ring and inner knob/stick.
    /// </summary>
    public static class TouchJoystickSpriteHelper
    {
        private static Sprite _baseSprite;
        private static Sprite _knobSprite;

        public static Sprite GetOrCreateBaseSprite()
        {
            if (_baseSprite != null) return _baseSprite;

            int size = 160;
            var tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            tex.filterMode = FilterMode.Point;
            Color[] pixels = new Color[size * size];

            float center = (size - 1) * 0.5f;
            float outerRadius = center - 4f;
            float ringThickness = 6f;
            float innerRadius = outerRadius - ringThickness;

            Color transparent = new Color(0f, 0f, 0f, 0f);
            Color ringColor = new Color(0.2f, 0.75f, 1.0f, 0.65f); // Neon cyan rim
            Color ringGlowColor = new Color(0.1f, 0.4f, 0.8f, 0.35f); // Subtle outer glow
            Color innerBgColor = new Color(0.04f, 0.08f, 0.16f, 0.40f); // Dark translucent fill

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float dx = x - center;
                    float dy = y - center;
                    float dist = Mathf.Sqrt(dx * dx + dy * dy);

                    if (dist > outerRadius + 2f)
                    {
                        pixels[y * size + x] = transparent;
                    }
                    else if (dist > outerRadius)
                    {
                        pixels[y * size + x] = ringGlowColor;
                    }
                    else if (dist >= innerRadius)
                    {
                        // Accent tick marks at 0, 90, 180, 270 degrees
                        float angle = Mathf.Atan2(dy, dx) * Mathf.Rad2Deg;
                        if (angle < 0) angle += 360f;
                        bool isCardinal = (angle >= 355 || angle <= 5) ||
                                          (angle >= 85 && angle <= 95) ||
                                          (angle >= 175 && angle <= 185) ||
                                          (angle >= 265 && angle <= 275);

                        pixels[y * size + x] = isCardinal
                            ? new Color(0.8f, 0.95f, 1.0f, 0.85f)
                            : ringColor;
                    }
                    else
                    {
                        // Radial gradient inside base
                        float t = dist / innerRadius;
                        Color fill = Color.Lerp(new Color(0.04f, 0.08f, 0.16f, 0.25f), innerBgColor, t);
                        pixels[y * size + x] = fill;
                    }
                }
            }

            tex.SetPixels(pixels);
            tex.Apply();

            _baseSprite = Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 100f);
            return _baseSprite;
        }

        public static Sprite GetOrCreateKnobSprite()
        {
            if (_knobSprite != null) return _knobSprite;

            int size = 72;
            var tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            tex.filterMode = FilterMode.Point;
            Color[] pixels = new Color[size * size];

            float center = (size - 1) * 0.5f;
            float radius = center - 2f;
            Color transparent = new Color(0f, 0f, 0f, 0f);
            Color rimColor = new Color(0.3f, 0.85f, 1.0f, 0.9f); // Bright cyan rim
            Color centerColor = new Color(0.9f, 0.98f, 1.0f, 0.95f); // Glowing white-cyan center
            Color bodyColor = new Color(0.1f, 0.35f, 0.65f, 0.85f); // Deep energetic blue

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float dx = x - center;
                    float dy = y - center;
                    float dist = Mathf.Sqrt(dx * dx + dy * dy);

                    if (dist > radius)
                    {
                        pixels[y * size + x] = transparent;
                    }
                    else if (dist > radius - 3f)
                    {
                        pixels[y * size + x] = rimColor;
                    }
                    else
                    {
                        float t = dist / (radius - 3f);
                        Color fill = Color.Lerp(centerColor, bodyColor, t);
                        pixels[y * size + x] = fill;
                    }
                }
            }

            tex.SetPixels(pixels);
            tex.Apply();

            _knobSprite = Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 100f);
            return _knobSprite;
        }
    }
}
