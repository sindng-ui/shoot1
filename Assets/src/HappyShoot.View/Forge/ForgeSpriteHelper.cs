using System.Collections.Generic;
using UnityEngine;
using HappyShoot.Domain.Forge;

namespace HappyShoot.View.Forge
{
    /// <summary>
    /// Procedural pixel-art icon and frame generator for Runes and Magic Crystals.
    /// Pure code-based zero asset dependency, crisp filtering, zero GC during gameplay.
    /// Under 500 lines.
    /// </summary>
    public static class ForgeSpriteHelper
    {
        private static readonly Dictionary<string, Sprite> _spriteCache = new Dictionary<string, Sprite>(32);

        public static Sprite GetRuneSprite(RuneDefinition def, int size = 48)
        {
            if (def == null) return null;
            string key = $"rune_{def.Id}_{size}";
            if (_spriteCache.TryGetValue(key, out var cached)) return cached;

            var tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            tex.filterMode = FilterMode.Point;
            Color[] pixels = new Color[size * size];

            Color frameCol = def.Grade switch
            {
                RuneGrade.Legendary => new Color(0.85f, 0.40f, 1.0f, 1f), // Purple
                RuneGrade.Rare => new Color(0.30f, 0.70f, 1.0f, 1f),      // Blue
                _ => new Color(0.35f, 0.90f, 0.45f, 1f)                   // Green
            };

            Color bgCol = new Color(0.12f, 0.12f, 0.18f, 0.95f);
            Color gemCol = def.PrimaryGem switch
            {
                HappyShoot.Domain.Progression.GemType.Ruby => new Color(1.0f, 0.25f, 0.35f, 1f),
                HappyShoot.Domain.Progression.GemType.Emerald => new Color(0.25f, 0.95f, 0.40f, 1f),
                _ => new Color(0.75f, 0.35f, 1.0f, 1f)
            };

            int cx = size / 2;
            int cy = size / 2;
            float maxR = size * 0.44f;

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    int idx = y * size + x;
                    float dx = x - cx;
                    float dy = y - cy;
                    float dist = Mathf.Sqrt(dx * dx + dy * dy);

                    // Outer circular border
                    if (dist <= maxR)
                    {
                        if (dist >= maxR - 2.5f)
                        {
                            pixels[idx] = frameCol;
                        }
                        else
                        {
                            pixels[idx] = bgCol;

                            // Rune glyph core shape (diamond or cross)
                            float innerD = Mathf.Abs(dx) + Mathf.Abs(dy);
                            if (innerD <= maxR * 0.55f)
                            {
                                pixels[idx] = Color.Lerp(gemCol, Color.white, (maxR * 0.55f - innerD) / (maxR * 0.55f) * 0.7f);
                            }
                        }
                    }
                    else
                    {
                        pixels[idx] = Color.clear;
                    }
                }
            }

            tex.SetPixels(pixels);
            tex.Apply();

            var sprite = Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f));
            _spriteCache[key] = sprite;
            return sprite;
        }

        public static Sprite GetSlotEmptySprite(int size = 48)
        {
            string key = $"slot_empty_{size}";
            if (_spriteCache.TryGetValue(key, out var cached)) return cached;

            var tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            tex.filterMode = FilterMode.Point;
            Color[] pixels = new Color[size * size];

            Color borderCol = new Color(0.35f, 0.35f, 0.45f, 0.8f);
            Color bgCol = new Color(0.10f, 0.10f, 0.14f, 0.6f);

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    int idx = y * size + x;
                    if (x == 0 || x == size - 1 || y == 0 || y == size - 1)
                        pixels[idx] = borderCol;
                    else
                        pixels[idx] = bgCol;
                }
            }

            tex.SetPixels(pixels);
            tex.Apply();

            var sprite = Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f));
            _spriteCache[key] = sprite;
            return sprite;
        }
    }
}
