using System.Collections.Generic;
using UnityEngine;

namespace HappyShoot.View.Utils
{
    /// <summary>
    /// Master procedural pixel-art icon cache and dispatcher for all skills, passives, and evolutions.
    /// Strictly modularized under 200 lines (500-line architecture rule).
    /// </summary>
    public static class RewardIconHelper
    {
        private static readonly Dictionary<string, Sprite> _iconCache = new Dictionary<string, Sprite>(32);

        public static void PreloadIcons()
        {
            string[] allIds = { "slash", "whirlwind", "ground_stomp", "blood_eater", "tempest_whirlwind", "earthshaker",
                                "bow", "arrow_rain", "glaive", "storm_bow", "phantom_glaive", "stellar_rain",
                                "fireball", "frost_nova", "chain_lightning", "meteor_strike", "blizzard_nova", "gigastorm_lightning",
                                "orbital", "passive_fang", "passive_feather", "passive_rune", "passive_armor", "passive_ring", "passive_heart", "passive_crit", "passive_ignition", "passive_overcharge" };
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

            // Wizard Magic Skills & Specific Passives
            if (rewardId == "fireball" || rewardId == "frost_nova" || rewardId == "chain_lightning" || rewardId == "blizzard_nova" || rewardId == "gigastorm_lightning" || rewardId == "passive_ignition" || rewardId == "passive_overcharge")
            {
                var magicIcon = WizardSpriteHelper.GetOrCreateMagicIcon(rewardId, size);
                _iconCache[rewardId] = magicIcon;
                return magicIcon;
            }

            var tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            tex.filterMode = FilterMode.Point;
            Color[] pixels = new Color[size * size];
            for (int i = 0; i < pixels.Length; i++) pixels[i] = Color.clear;

            int cx = size / 2;
            int cy = size / 2;

            switch (rewardId)
            {
                // Warrior Skills & Evolutions
                case "slash":
                    WarriorRewardIconHelper.DrawGreatswordIcon(pixels, size, cx, cy);
                    break;
                case "whirlwind":
                case "tempest_whirlwind":
                    WarriorRewardIconHelper.DrawWhirlwindIcon(pixels, size, cx, cy);
                    break;
                case "ground_stomp":
                case "earthshaker":
                    WarriorRewardIconHelper.DrawGroundStompIcon(pixels, size, cx, cy);
                    break;
                case "blood_eater":
                    WarriorRewardIconHelper.DrawBloodEaterIcon(pixels, size, cx, cy);
                    break;

                // Ranger Skills & Evolutions
                case "bow":
                case "multishot":
                case "piercing_arrow":
                    RangerRewardIconHelper.DrawPiercingArrowIcon(pixels, size, cx, cy);
                    break;
                case "arrow_rain":
                    RangerRewardIconHelper.DrawArrowRainIcon(pixels, size, cx, cy);
                    break;
                case "glaive":
                case "wind_glaive":
                case "phantom_glaive":
                    RangerRewardIconHelper.DrawWindGlaiveIcon(pixels, size, cx, cy);
                    break;
                case "stellar_rain":
                    RangerRewardIconHelper.DrawStellarRainIcon(pixels, size, cx, cy);
                    break;
                case "storm_bow":
                    DrawStormBowIcon(pixels, size, cx, cy);
                    break;

                // Evolved Wizard Inferno Fireball
                case "meteor_strike":
                    DrawInfernoFireballIcon(pixels, size, cx, cy);
                    break;

                // Common Orbital Blades
                case "orbital":
                case "evolved_orbital":
                    DrawOrbitalBladesIcon(pixels, size, cx, cy);
                    break;

                // Passives
                case "passive_fang":
                    PassiveRewardIconHelper.DrawVampireFangIcon(pixels, size, cx, cy);
                    break;
                case "passive_feather":
                    PassiveRewardIconHelper.DrawWindFeatherIcon(pixels, size, cx, cy);
                    break;
                case "passive_rune":
                    PassiveRewardIconHelper.DrawManaRuneIcon(pixels, size, cx, cy);
                    break;
                case "passive_armor":
                    PassiveRewardIconHelper.DrawIronArmorIcon(pixels, size, cx, cy);
                    break;
                case "passive_ring":
                    PassiveRewardIconHelper.DrawGoldenRingIcon(pixels, size, cx, cy);
                    break;
                case "passive_heart":
                    PassiveRewardIconHelper.DrawHeartPendantIcon(pixels, size, cx, cy);
                    break;
                case "passive_crit":
                    PassiveRewardIconHelper.DrawCritEyeIcon(pixels, size, cx, cy);
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

        private static void DrawStormBowIcon(Color[] pixels, int size, int cx, int cy)
        {
            RangerRewardIconHelper.DrawPiercingArrowIcon(pixels, size, cx, cy);
            Color lightning = new Color(0.40f, 0.95f, 1.0f);
            for (int x = -24; x <= 24; x += 4)
            {
                int y = (x % 8 == 0) ? 5 : -5;
                SetPixelSafe(pixels, size, cx + x, cy + y, lightning);
            }
        }

        private static void DrawInfernoFireballIcon(Color[] pixels, int size, int cx, int cy)
        {
            Color coreWhite = new Color(1.0f, 0.98f, 0.90f);
            Color magmaOrange = new Color(1.0f, 0.45f, 0.10f);
            Color flameGold = new Color(1.0f, 0.85f, 0.20f);
            Color darkCrimson = new Color(0.55f, 0.08f, 0.08f);

            int[] ox = { 8, -12, -14 };
            int[] oy = { 0, 14, -14 };
            float[] rad = { 13f, 8f, 8f };

            for (int b = 0; b < 3; b++)
            {
                int bx = cx + ox[b];
                int by = cy + oy[b];
                float r = rad[b];

                for (int y = -(int)r - 4; y <= (int)r + 4; y++)
                {
                    for (int x = -(int)r - 12; x <= (int)r + 4; x++)
                    {
                        float dist = Mathf.Sqrt(x * x + y * y);
                        if (dist <= r * 0.45f) SetPixelSafe(pixels, size, bx + x, by + y, coreWhite);
                        else if (dist <= r * 0.85f) SetPixelSafe(pixels, size, bx + x, by + y, flameGold);
                        else if (dist <= r) SetPixelSafe(pixels, size, bx + x, by + y, magmaOrange);
                        else if (x < 0 && Mathf.Abs(y) <= r * 0.7f && dist <= r + 10f)
                            SetPixelSafe(pixels, size, bx + x, by + y, (x < -6) ? darkCrimson : magmaOrange);
                    }
                }
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
