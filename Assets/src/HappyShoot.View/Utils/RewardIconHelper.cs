using System.Collections.Generic;
using UnityEngine;

namespace HappyShoot.View.Utils
{
    /// <summary>
    /// Generates high-resolution 80x80 crisp procedural pixel-art icons for all 13 active weapons, passives, and evolutions.
    /// </summary>
    public static class RewardIconHelper
    {
        private static readonly Dictionary<string, Sprite> _iconCache = new Dictionary<string, Sprite>(16);

        public static void PreloadIcons()
        {
            string[] allIds = { "slash", "bow", "ground_stomp", "orbital", "multishot", "whirlwind", "arrow_rain", "passive_fang", "passive_feather", "passive_rune", "passive_armor", "passive_ring", "passive_heart", "evolved_greatsword", "evolved_windbow", "evolved_orbital" };
            for (int i = 0; i < allIds.Length; i++)
            {
                GetOrCreateRewardIcon(allIds[i]);
            }
        }

        public static Sprite GetOrCreateRewardIcon(string rewardId, int size = 80)
        {
            if (string.IsNullOrEmpty(rewardId)) rewardId = "default";

            if (_iconCache.TryGetValue(rewardId, out var cached) && cached != null)
            {
                return cached;
            }

            var tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            tex.filterMode = FilterMode.Point;
            Color[] pixels = new Color[size * size];
            for (int i = 0; i < pixels.Length; i++) pixels[i] = Color.clear;

            int cx = size / 2;
            int cy = size / 2;

            if (rewardId == "fireball" || rewardId == "frost_nova" || rewardId == "chain_lightning" || rewardId == "passive_ignition" || rewardId == "passive_overcharge")
            {
                var magicIcon = WizardSpriteHelper.GetOrCreateMagicIcon(rewardId, size);
                _iconCache[rewardId] = magicIcon;
                return magicIcon;
            }

            switch (rewardId)
            {
                case "slash":
                case "whirlwind":
                    DrawGreatswordIcon(pixels, size, cx, cy);
                    break;
                case "bow":
                case "multishot":
                case "arrow_rain":
                    DrawBowIcon(pixels, size, cx, cy);
                    break;
                case "explosion":
                case "ground_stomp":
                    DrawExplosionIcon(pixels, size, cx, cy);
                    break;
                case "orbital":
                    DrawOrbitalBladesIcon(pixels, size, cx, cy);
                    break;
                case "passive_fang":
                    DrawVampireFangIcon(pixels, size, cx, cy);
                    break;
                case "passive_feather":
                    DrawWindFeatherIcon(pixels, size, cx, cy);
                    break;
                case "passive_rune":
                    DrawManaRuneIcon(pixels, size, cx, cy);
                    break;
                case "passive_armor":
                    DrawIronArmorIcon(pixels, size, cx, cy);
                    break;
                case "passive_ring":
                    DrawGoldenRingIcon(pixels, size, cx, cy);
                    break;
                case "passive_heart":
                    DrawHeartPendantIcon(pixels, size, cx, cy);
                    break;
                case "passive_crit":
                    DrawCritEyeIcon(pixels, size, cx, cy);
                    break;
                case "blood_eater":
                    DrawBloodEaterIcon(pixels, size, cx, cy);
                    break;
                case "storm_bow":
                    DrawStormBowIcon(pixels, size, cx, cy);
                    break;
                case "meteor_strike":
                    DrawMeteorStrikeIcon(pixels, size, cx, cy);
                    break;
                default:
                    DrawDefaultIcon(pixels, size, cx, cy);
                    break;
            }

            tex.SetPixels(pixels);
            tex.Apply();

            var sprite = Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), size);
            _iconCache[rewardId] = sprite;
            return sprite;
        }

        private static void DrawGreatswordIcon(Color[] pixels, int size, int cx, int cy)
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
                    SetPixelSafe(pixels, size, x, y, bladeLight);
                    SetPixelSafe(pixels, size, x - 1, y + 1, bladeLight);
                    SetPixelSafe(pixels, size, x + 1, y - 1, bladeMid);
                    SetPixelSafe(pixels, size, x - 2, y + 2, bladeMid);
                    SetPixelSafe(pixels, size, x + 2, y - 2, bladeMid);
                }
                else if (d >= -16)
                {
                    for (int w = -8; w <= 8; w++)
                        SetPixelSafe(pixels, size, x + w, y - w, gold);
                }
                else
                {
                    SetPixelSafe(pixels, size, x, y, grip);
                    SetPixelSafe(pixels, size, x - 1, y + 1, grip);
                }
            }
        }

        private static void DrawBowIcon(Color[] pixels, int size, int cx, int cy)
        {
            Color wood = new Color(0.72f, 0.45f, 0.20f);
            Color woodDark = new Color(0.45f, 0.25f, 0.10f);
            Color stringCol = new Color(0.9f, 0.9f, 0.95f);
            Color arrowCol = new Color(0.3f, 0.85f, 1.0f);

            // Bow curve
            for (int y = -28; y <= 28; y++)
            {
                int xOffset = (int)(22f - (y * y) / 36f);
                SetPixelSafe(pixels, size, cx - 10 + xOffset, cy + y, wood);
                SetPixelSafe(pixels, size, cx - 11 + xOffset, cy + y, woodDark);
            }

            // String
            for (int y = -28; y <= 28; y++)
                SetPixelSafe(pixels, size, cx - 10 + 22, cy + y, stringCol);

            // Arrow
            for (int x = -28; x <= 26; x++)
            {
                SetPixelSafe(pixels, size, cx + x, cy, arrowCol);
                SetPixelSafe(pixels, size, cx + x, cy + 1, arrowCol);
            }
        }

        private static void DrawExplosionIcon(Color[] pixels, int size, int cx, int cy)
        {
            Color orbCenter = new Color(1.0f, 0.9f, 1.0f);
            Color orbMid = new Color(0.85f, 0.35f, 0.95f);
            Color orbEdge = new Color(0.45f, 0.15f, 0.75f);
            Color spark = new Color(0.35f, 0.85f, 1.0f);

            for (int y = -26; y <= 26; y++)
            {
                for (int x = -26; x <= 26; x++)
                {
                    float dist = Mathf.Sqrt(x * x + y * y);
                    if (dist <= 8f) SetPixelSafe(pixels, size, cx + x, cy + y, orbCenter);
                    else if (dist <= 18f) SetPixelSafe(pixels, size, cx + x, cy + y, orbMid);
                    else if (dist <= 25f) SetPixelSafe(pixels, size, cx + x, cy + y, orbEdge);
                }
            }

            // Magic Sparks
            for (int i = 0; i < 8; i++)
            {
                float ang = i * Mathf.PI * 0.25f;
                int sx = cx + (int)(Mathf.Cos(ang) * 28f);
                int sy = cy + (int)(Mathf.Sin(ang) * 28f);
                SetPixelSafe(pixels, size, sx, sy, spark);
                SetPixelSafe(pixels, size, sx + 1, sy, spark);
                SetPixelSafe(pixels, size, sx, sy + 1, spark);
            }
        }

        private static void DrawOrbitalBladesIcon(Color[] pixels, int size, int cx, int cy)
        {
            Color steel = new Color(0.85f, 0.9f, 0.95f);
            Color edge = new Color(0.3f, 0.75f, 1.0f);

            for (int i = 0; i < 4; i++)
            {
                float ang = i * Mathf.PI * 0.5f;
                int bx = cx + (int)(Mathf.Cos(ang) * 18f);
                int by = cy + (int)(Mathf.Sin(ang) * 18f);

                for (int d = -8; d <= 8; d++)
                {
                    int dx = (int)(-Mathf.Sin(ang) * d);
                    int dy = (int)(Mathf.Cos(ang) * d);
                    SetPixelSafe(pixels, size, bx + dx, by + dy, steel);
                    SetPixelSafe(pixels, size, bx + dx + 1, by + dy, edge);
                }
            }
        }

        private static void DrawVampireFangIcon(Color[] pixels, int size, int cx, int cy)
        {
            Color tooth = new Color(0.95f, 0.95f, 0.98f);
            Color blood = new Color(0.95f, 0.15f, 0.20f);

            // Left fang
            DrawFang(pixels, size, cx - 12, cy, tooth, blood);
            // Right fang
            DrawFang(pixels, size, cx + 12, cy, tooth, blood);
        }

        private static void DrawFang(Color[] pixels, int size, int ox, int oy, Color tooth, Color blood)
        {
            for (int y = 14; y >= -16; y--)
            {
                int width = (y + 16) / 4;
                for (int x = -width; x <= width; x++)
                {
                    Color col = (y <= -8) ? blood : tooth;
                    SetPixelSafe(pixels, size, ox + x, oy + y, col);
                }
            }
        }

        private static void DrawWindFeatherIcon(Color[] pixels, int size, int cx, int cy)
        {
            Color feather = new Color(0.35f, 0.90f, 0.75f);
            Color stem = new Color(0.95f, 0.98f, 1.0f);

            for (int d = -22; d <= 22; d++)
            {
                int x = cx + d;
                int y = cy + d;
                SetPixelSafe(pixels, size, x, y, stem);

                int width = (24 - Mathf.Abs(d)) / 2;
                for (int w = 1; w <= width; w++)
                {
                    SetPixelSafe(pixels, size, x - w, y + w, feather);
                    SetPixelSafe(pixels, size, x + w, y - w, feather);
                }
            }
        }

        private static void DrawManaRuneIcon(Color[] pixels, int size, int cx, int cy)
        {
            Color stone = new Color(0.20f, 0.22f, 0.30f);
            Color runeGlow = new Color(0.30f, 0.85f, 1.0f);

            for (int y = -22; y <= 22; y++)
            {
                for (int x = -20; x <= 20; x++)
                {
                    if (Mathf.Abs(x) + Mathf.Abs(y) <= 26)
                        SetPixelSafe(pixels, size, cx + x, cy + y, stone);
                }
            }

            // Glowing Rune symbol (Runic 'X' and vertical bar)
            for (int d = -12; d <= 12; d++)
            {
                SetPixelSafe(pixels, size, cx + d, cy + d, runeGlow);
                SetPixelSafe(pixels, size, cx + d, cy - d, runeGlow);
                SetPixelSafe(pixels, size, cx, cy + d, runeGlow);
            }
        }

        private static void DrawIronArmorIcon(Color[] pixels, int size, int cx, int cy)
        {
            Color steel = new Color(0.65f, 0.70f, 0.78f);
            Color steelDark = new Color(0.35f, 0.40f, 0.48f);
            Color goldTrim = new Color(0.95f, 0.80f, 0.25f);

            for (int y = -20; y <= 20; y++)
            {
                int width = 20 - Mathf.Abs(y) / 3;
                for (int x = -width; x <= width; x++)
                {
                    Color col = (Mathf.Abs(x) >= width - 2 || y == 20 || y == -20) ? goldTrim : (x < 0 ? steel : steelDark);
                    SetPixelSafe(pixels, size, cx + x, cy + y, col);
                }
            }
        }

        private static void DrawGoldenRingIcon(Color[] pixels, int size, int cx, int cy)
        {
            Color gold = new Color(1.0f, 0.82f, 0.20f);
            Color goldDark = new Color(0.75f, 0.55f, 0.10f);
            Color ruby = new Color(0.95f, 0.15f, 0.25f);

            for (int y = -22; y <= 16; y++)
            {
                for (int x = -20; x <= 20; x++)
                {
                    float dist = Mathf.Sqrt(x * x + y * y);
                    if (dist >= 14f && dist <= 20f)
                        SetPixelSafe(pixels, size, cx + x, cy + y, (y > 0 ? gold : goldDark));
                }
            }

            // Big Gem Top
            for (int y = 14; y <= 24; y++)
            {
                int w = 5 - Mathf.Abs(y - 19);
                for (int x = -w; x <= w; x++)
                    SetPixelSafe(pixels, size, cx + x, cy + y, ruby);
            }
        }

        private static void DrawHeartPendantIcon(Color[] pixels, int size, int cx, int cy)
        {
            Color heart = new Color(0.95f, 0.20f, 0.35f);
            Color heartGlow = new Color(1.0f, 0.65f, 0.75f);
            Color goldChain = new Color(1.0f, 0.85f, 0.30f);

            // Chain
            for (int d = -16; d <= 16; d++)
                SetPixelSafe(pixels, size, cx + d, cy + 24 - Mathf.Abs(d) / 2, goldChain);

            // Heart shape
            for (int y = -16; y <= 16; y++)
            {
                for (int x = -18; x <= 18; x++)
                {
                    float fx = x / 16f;
                    float fy = y / 16f;
                    float formula = (fx * fx + fy * fy - 1f);
                    if (formula * formula * formula - fx * fx * fy * fy * fy <= 0f)
                    {
                        Color c = (x < -2 && y > 0) ? heartGlow : heart;
                        SetPixelSafe(pixels, size, cx + x, cy + y + 2, c);
                    }
                }
            }
        }

        private static void DrawBloodEaterIcon(Color[] pixels, int size, int cx, int cy)
        {
            DrawGreatswordIcon(pixels, size, cx, cy);
            Color bloodAura = new Color(1.0f, 0.15f, 0.25f, 0.95f);

            for (int d = -26; d <= 26; d++)
            {
                int x = cx + d;
                int y = cy + d;
                if (d > -12)
                {
                    SetPixelSafe(pixels, size, x - 3, y + 3, bloodAura);
                    SetPixelSafe(pixels, size, x + 3, y - 3, bloodAura);
                }
            }
        }

        private static void DrawStormBowIcon(Color[] pixels, int size, int cx, int cy)
        {
            DrawBowIcon(pixels, size, cx, cy);
            Color lightning = new Color(0.40f, 0.95f, 1.0f);

            for (int x = -24; x <= 24; x += 4)
            {
                int y = (x % 8 == 0) ? 5 : -5;
                SetPixelSafe(pixels, size, cx + x, cy + y, lightning);
            }
        }

        private static void DrawMeteorStrikeIcon(Color[] pixels, int size, int cx, int cy)
        {
            Color magma = new Color(1.0f, 0.35f, 0.10f);
            Color flame = new Color(1.0f, 0.85f, 0.20f);
            Color rock = new Color(0.25f, 0.15f, 0.10f);

            for (int y = -22; y <= 22; y++)
            {
                for (int x = -22; x <= 22; x++)
                {
                    float dist = Mathf.Sqrt(x * x + y * y);
                    if (dist <= 14f) SetPixelSafe(pixels, size, cx + x, cy + y, rock);
                    else if (dist <= 20f) SetPixelSafe(pixels, size, cx + x, cy + y, magma);
                    else if (dist <= 26f && (x + y) > 0) SetPixelSafe(pixels, size, cx + x, cy + y, flame);
                }
            }
        }

        private static void DrawCritEyeIcon(Color[] pixels, int size, int cx, int cy)
        {
            Color gold = new Color(1.0f, 0.85f, 0.15f);
            Color neonRed = new Color(1.0f, 0.20f, 0.25f);
            Color darkRing = new Color(0.20f, 0.10f, 0.30f);
            Color shine = new Color(1.0f, 1.0f, 0.80f);

            // 1. Outer Eye / Reticle Shape
            for (int y = -22; y <= 22; y++)
            {
                for (int x = -22; x <= 22; x++)
                {
                    float dist = Mathf.Sqrt(x * x + y * y);
                    // Reticle ring
                    if (dist >= 17f && dist <= 21f)
                    {
                        SetPixelSafe(pixels, size, cx + x, cy + y, gold);
                    }
                    // Inner glowing iris
                    else if (dist <= 12f)
                    {
                        SetPixelSafe(pixels, size, cx + x, cy + y, neonRed);
                    }
                }
            }

            // 2. Crosshair Ticks (4 directions)
            for (int i = 12; i <= 26; i++)
            {
                SetPixelSafe(pixels, size, cx + i, cy, gold);
                SetPixelSafe(pixels, size, cx - i, cy, gold);
                SetPixelSafe(pixels, size, cx, cy + i, gold);
                SetPixelSafe(pixels, size, cx, cy - i, gold);
            }

            // 3. Central Critical Glint / Core Pupil
            for (int y = -3; y <= 3; y++)
            {
                for (int x = -3; x <= 3; x++)
                {
                    if (Mathf.Abs(x) + Mathf.Abs(y) <= 4)
                        SetPixelSafe(pixels, size, cx + x, cy + y, shine);
                }
            }
        }

        private static void DrawDefaultIcon(Color[] pixels, int size, int cx, int cy)
        {
            Color gold = new Color(1.0f, 0.85f, 0.25f);
            for (int y = -16; y <= 16; y++)
            {
                for (int x = -16; x <= 16; x++)
                {
                    if (Mathf.Abs(x) + Mathf.Abs(y) <= 16)
                        SetPixelSafe(pixels, size, cx + x, cy + y, gold);
                }
            }
        }

        private static void SetPixelSafe(Color[] pixels, int size, int x, int y, Color color)
        {
            if (x >= 0 && x < size && y >= 0 && y < size)
            {
                pixels[y * size + x] = color;
            }
        }
    }
}
