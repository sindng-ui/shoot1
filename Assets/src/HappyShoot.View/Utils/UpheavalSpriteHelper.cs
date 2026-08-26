using UnityEngine;

namespace HappyShoot.View.Utils
{
    /// <summary>
    /// Dedicated procedural pixel-art sprite generator for Warrior Upheaval (Ground Stomp).
    /// Modularized to strictly respect the 500-line architecture rule.
    /// </summary>
    public static class UpheavalSpriteHelper
    {
        private static Sprite _upheavalWaveSprite;
        private static Sprite _upheavalChunkSprite;
        private static Sprite _upheavalSpikeSprite;

        /// <summary>
        /// 32x32 Forward-Facing Wedge / V-Shape Earth Shockwave Crest with molten magma fracture.
        /// When spawned in succession (30ms), creates an intense 'dudududu' advancing seismic shockwave.
        /// </summary>
        public static Sprite GetOrCreateUpheavalWaveSprite(int size = 32)
        {
            if (_upheavalWaveSprite != null) return _upheavalWaveSprite;

            var texture = new Texture2D(size, size, TextureFormat.RGBA32, false) { filterMode = FilterMode.Point };
            Color[] pixels = new Color[size * size];
            for (int i = 0; i < pixels.Length; i++) pixels[i] = Color.clear;

            Color crestGold = new Color(1.0f, 0.90f, 0.40f, 1.0f); // Blazing shockwave crest
            Color magmaOrange = new Color(1.0f, 0.50f, 0.08f, 0.95f); // Hot magma inner
            Color basaltRock = new Color(0.28f, 0.14f, 0.06f, 0.90f); // Shattered crust rim
            Color outerCrust = new Color(0.15f, 0.07f, 0.03f, 0.55f);

            Vector2 center = new Vector2(size * 0.25f, size * 0.5f); // Arc centered to project forward (+X)

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    Vector2 pos = new Vector2(x + 0.5f, y + 0.5f);
                    float dist = Vector2.Distance(pos, center);
                    float angle = Mathf.Atan2(pos.y - center.y, pos.x - center.x) * Mathf.Rad2Deg;

                    // 120-degree forward wedge arc (-60 to +60 deg)
                    if (Mathf.Abs(angle) <= 65f)
                    {
                        float targetR = size * 0.65f + Mathf.Sin(angle * 0.15f) * 1.5f;
                        float diff = Mathf.Abs(dist - targetR);

                        if (diff <= 6.0f)
                        {
                            if (diff <= 1.5f)
                            {
                                pixels[y * size + x] = crestGold;
                            }
                            else if (diff <= 3.2f)
                            {
                                pixels[y * size + x] = magmaOrange;
                            }
                            else if (diff <= 5.0f)
                            {
                                pixels[y * size + x] = basaltRock;
                            }
                            else
                            {
                                pixels[y * size + x] = outerCrust;
                            }
                        }
                    }
                }
            }

            texture.SetPixels(pixels);
            texture.Apply();
            // Pivot at center (0.35, 0.5) so it projects forward smoothly
            _upheavalWaveSprite = Sprite.Create(texture, new Rect(0, 0, size, size), new Vector2(0.35f, 0.5f), 24);
            return _upheavalWaveSprite;
        }

        /// <summary>
        /// 24x20 Heavy Fractured Earth Rock Slab / Chunk that violently lifts up on shockwave pass.
        /// </summary>
        public static Sprite GetOrCreateUpheavalChunkSprite(int w = 24, int h = 20)
        {
            if (_upheavalChunkSprite != null) return _upheavalChunkSprite;

            var texture = new Texture2D(w, h, TextureFormat.RGBA32, false) { filterMode = FilterMode.Point };
            Color[] pixels = new Color[w * h];
            for (int i = 0; i < pixels.Length; i++) pixels[i] = Color.clear;

            Color darkCrust = new Color(0.20f, 0.10f, 0.05f, 1f);
            Color rockFace = new Color(0.48f, 0.28f, 0.14f, 1f);
            Color rockEdge = new Color(0.72f, 0.48f, 0.24f, 1f);
            Color magmaCrack = new Color(1.0f, 0.70f, 0.20f, 1f);

            int cx = w / 2, cy = h / 2;

            for (int y = 0; y < h; y++)
            {
                for (int x = 0; x < w; x++)
                {
                    float nx = (x - cx) / (float)(w * 0.45f);
                    float ny = (y - cy) / (float)(h * 0.45f);
                    float jagged = Mathf.Sin(x * 1.2f + y * 0.8f) * 0.18f;

                    if ((nx * nx + ny * ny + jagged) <= 1.0f)
                    {
                        if (y >= h - 4) pixels[y * w + x] = rockEdge;
                        else if (Mathf.Abs(x - cx) <= 1.2f && y <= cy) pixels[y * w + x] = magmaCrack;
                        else if (x > cx + 2) pixels[y * w + x] = darkCrust;
                        else pixels[y * w + x] = rockFace;
                    }
                }
            }

            texture.SetPixels(pixels);
            texture.Apply();
            _upheavalChunkSprite = Sprite.Create(texture, new Rect(0, 0, w, h), new Vector2(0.5f, 0.2f), 24);
            return _upheavalChunkSprite;
        }

        /// <summary>
        /// 18x24 Compact Sharp Jagged Earth Rock Spike / Shard (High PPU = 48).
        /// </summary>
        public static Sprite GetOrCreateUpheavalSpikeSprite(int w = 18, int h = 24)
        {
            if (_upheavalSpikeSprite != null) return _upheavalSpikeSprite;

            var texture = new Texture2D(w, h, TextureFormat.RGBA32, false) { filterMode = FilterMode.Point };
            Color[] pixels = new Color[w * h];
            for (int i = 0; i < pixels.Length; i++) pixels[i] = Color.clear;

            Color darkRock = new Color(0.18f, 0.10f, 0.05f, 1f);
            Color rockFace = new Color(0.42f, 0.24f, 0.12f, 1f);
            Color rockHighlight = new Color(0.68f, 0.44f, 0.22f, 1f);
            Color magmaGlow = new Color(1.0f, 0.65f, 0.15f, 1f);
            Color sharpTip = new Color(1.0f, 0.90f, 0.60f, 1f);

            int cx = w / 2;

            for (int y = 0; y < h; y++)
            {
                float t = (float)y / h;
                float halfWidth = (1.0f - t * 0.88f) * (w * 0.45f) + (Mathf.Sin(y * 1.1f) * 0.8f);

                for (int x = 0; x < w; x++)
                {
                    float dx = x - cx;
                    if (Mathf.Abs(dx) <= halfWidth)
                    {
                        if (y >= h - 3)
                        {
                            pixels[y * w + x] = sharpTip;
                        }
                        else if (dx < -halfWidth * 0.25f)
                        {
                            pixels[y * w + x] = rockHighlight;
                        }
                        else if (Mathf.Abs(dx) <= 0.8f && y >= 3 && y <= h - 5)
                        {
                            pixels[y * w + x] = magmaGlow;
                        }
                        else if (dx > halfWidth * 0.30f)
                        {
                            pixels[y * w + x] = darkRock;
                        }
                        else
                        {
                            pixels[y * w + x] = rockFace;
                        }
                    }
                }
            }

            texture.SetPixels(pixels);
            texture.Apply();
            _upheavalSpikeSprite = Sprite.Create(texture, new Rect(0, 0, w, h), new Vector2(0.5f, 0.0f), 48);
            return _upheavalSpikeSprite;
        }
    }
}
