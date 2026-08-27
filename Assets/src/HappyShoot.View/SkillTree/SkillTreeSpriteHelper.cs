using System;
using UnityEngine;
using HappyShoot.Domain.Progression;

namespace HappyShoot.View.SkillTree
{
    /// <summary>
    /// Generates high-contrast procedural pixel-art sprites for gems, elemental branch icons,
    /// and node frames for the Skill Tree progression UI.
    /// Strictly modular and under 500 lines.
    /// </summary>
    public static class SkillTreeSpriteHelper
    {
        private static Sprite _rubySprite;
        private static Sprite _emeraldSprite;
        private static Sprite _amethystSprite;

        private static Sprite _fireIconSprite;
        private static Sprite _iceIconSprite;
        private static Sprite _lightningIconSprite;
        private static Sprite _coreStatIconSprite;

        private static Sprite _nodeFrameAvailable;
        private static Sprite _nodeFrameUnlocked;
        private static Sprite _nodeFrameLocked;
        private static Sprite _nodeFrameBlocked;

        public static Sprite GetGemSprite(GemType type)
        {
            switch (type)
            {
                case GemType.Ruby: return GetOrCreateRubySprite();
                case GemType.Emerald: return GetOrCreateEmeraldSprite();
                case GemType.Amethyst: return GetOrCreateAmethystSprite();
                default: return GetOrCreateRubySprite();
            }
        }

        public static Sprite GetBranchIcon(BranchType branch)
        {
            switch (branch)
            {
                case BranchType.Fire: return GetOrCreateFireIcon();
                case BranchType.Ice: return GetOrCreateIceIcon();
                case BranchType.Lightning: return GetOrCreateLightningIcon();
                default: return GetOrCreateCoreStatIcon();
            }
        }

        // ── Gem Procedural Sprites (32x32) ──

        public static Sprite GetOrCreateRubySprite(int size = 32)
        {
            if (_rubySprite != null) return _rubySprite;
            _rubySprite = CreateGemSprite(size,
                highlight: new Color(1.0f, 0.65f, 0.70f),
                main: new Color(0.95f, 0.15f, 0.25f),
                shadow: new Color(0.50f, 0.05f, 0.12f));
            return _rubySprite;
        }

        public static Sprite GetOrCreateEmeraldSprite(int size = 32)
        {
            if (_emeraldSprite != null) return _emeraldSprite;
            _emeraldSprite = CreateGemSprite(size,
                highlight: new Color(0.65f, 1.0f, 0.70f),
                main: new Color(0.12f, 0.88f, 0.35f),
                shadow: new Color(0.04f, 0.45f, 0.15f));
            return _emeraldSprite;
        }

        public static Sprite GetOrCreateAmethystSprite(int size = 32)
        {
            if (_amethystSprite != null) return _amethystSprite;
            _amethystSprite = CreateGemSprite(size,
                highlight: new Color(0.90f, 0.70f, 1.0f),
                main: new Color(0.70f, 0.25f, 0.95f),
                shadow: new Color(0.35f, 0.08f, 0.55f));
            return _amethystSprite;
        }

        private static Sprite CreateGemSprite(int size, Color highlight, Color main, Color shadow)
        {
            var tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            tex.filterMode = FilterMode.Point;
            Color[] p = new Color[size * size];
            for (int i = 0; i < p.Length; i++) p[i] = Color.clear;

            int cx = size / 2;
            int cy = size / 2;
            int r = (size / 2) - 3;

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    int dx = Math.Abs(x - cx);
                    int dy = Math.Abs(y - cy);
                    // Octagonal gem shape
                    if (dx + dy <= r + 4 && dx <= r && dy <= r)
                    {
                        if (dx + dy == r + 4 || dx == r || dy == r)
                        {
                            p[y * size + x] = new Color(0.08f, 0.08f, 0.12f, 0.95f); // Outline
                        }
                        else if (y > cy && x <= cx)
                        {
                            p[y * size + x] = highlight; // Top-left facet
                        }
                        else if (y < cy)
                        {
                            p[y * size + x] = shadow; // Bottom facet
                        }
                        else
                        {
                            p[y * size + x] = main; // Center & right
                        }
                    }
                }
            }

            tex.SetPixels(p);
            tex.Apply();
            return Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 16);
        }

        // ── Elemental & Core Icons (24x24) ──

        public static Sprite GetOrCreateFireIcon(int size = 24)
        {
            if (_fireIconSprite != null) return _fireIconSprite;
            _fireIconSprite = CreateProceduralIcon(size, (x, y, cx, cy) =>
            {
                // Flame teardrop shape
                int dx = Math.Abs(x - cx);
                int dy = y - cy;
                bool isFlame = (y >= cy - 6 && y <= cy + 8) && (dx <= (cy + 8 - y) / 2 + 1) && (dx <= 6);
                return isFlame ? new Color(1.0f, 0.45f + (y * 0.02f), 0.10f) : Color.clear;
            });
            return _fireIconSprite;
        }

        public static Sprite GetOrCreateIceIcon(int size = 24)
        {
            if (_iceIconSprite != null) return _iceIconSprite;
            _iceIconSprite = CreateProceduralIcon(size, (x, y, cx, cy) =>
            {
                int dx = Math.Abs(x - cx);
                int dy = Math.Abs(y - cy);
                bool isCross = (dx <= 1 && dy <= 7) || (dy <= 1 && dx <= 7) || (dx == dy && dx <= 5);
                return isCross ? new Color(0.40f, 0.85f, 1.0f) : Color.clear;
            });
            return _iceIconSprite;
        }

        public static Sprite GetOrCreateLightningIcon(int size = 24)
        {
            if (_lightningIconSprite != null) return _lightningIconSprite;
            _lightningIconSprite = CreateProceduralIcon(size, (x, y, cx, cy) =>
            {
                // Zig-zag bolt
                bool isBolt = (y > cy && Math.Abs(x - (cx + (y - cy) / 2)) <= 1) ||
                              (y <= cy && Math.Abs(x - (cx - (cy - y) / 2)) <= 1) ||
                              (y == cy && Math.Abs(x - cx) <= 3);
                return isBolt ? new Color(1.0f, 0.95f, 0.25f) : Color.clear;
            });
            return _lightningIconSprite;
        }

        public static Sprite GetOrCreateCoreStatIcon(int size = 24)
        {
            if (_coreStatIconSprite != null) return _coreStatIconSprite;
            _coreStatIconSprite = CreateProceduralIcon(size, (x, y, cx, cy) =>
            {
                // Diamond star
                int dx = Math.Abs(x - cx);
                int dy = Math.Abs(y - cy);
                return (dx + dy <= 6) ? new Color(0.95f, 0.85f, 0.50f) : Color.clear;
            });
            return _coreStatIconSprite;
        }

        private static Sprite CreateProceduralIcon(int size, Func<int, int, int, int, Color> colorFunc)
        {
            var tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            tex.filterMode = FilterMode.Point;
            Color[] p = new Color[size * size];
            int cx = size / 2;
            int cy = size / 2;

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    p[y * size + x] = colorFunc(x, y, cx, cy);
                }
            }

            tex.SetPixels(p);
            tex.Apply();
            return Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 16);
        }


        private static Sprite _centralHubSprite;

        public static Sprite GetOrCreateCentralHubSprite(int size = 64)
        {
            if (_centralHubSprite != null) return _centralHubSprite;
            var tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            tex.filterMode = FilterMode.Point;
            Color[] p = new Color[size * size];
            int cx = size / 2;
            int cy = size / 2;
            float r = (size / 2f) - 2f;

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float dx = x - cx;
                    float dy = y - cy;
                    float dist = Mathf.Sqrt(dx * dx + dy * dy);

                    if (dist > r)
                    {
                        p[y * size + x] = Color.clear;
                    }
                    else if (dist >= r - 3f)
                    {
                        p[y * size + x] = new Color(0.35f, 0.25f, 0.05f, 1.0f); // Dark gold edge
                    }
                    else if (dist >= r - 6f)
                    {
                        p[y * size + x] = new Color(1.0f, 0.90f, 0.40f, 1.0f); // Brilliant gold rim
                    }
                    else
                    {
                        // Radiant golden sun core
                        float glow = 1f - (dist / r);
                        p[y * size + x] = new Color(0.85f + (glow * 0.15f), 0.65f + (glow * 0.25f), 0.15f + (glow * 0.15f), 1.0f);
                    }
                }
            }
            tex.SetPixels(p);
            tex.Apply();
            _centralHubSprite = Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 16);
            return _centralHubSprite;
        }

        // ── Node Frame Sprites (Round Rune Gem Badges) ──

        public static Sprite GetNodeFrame(bool isUnlocked, bool canUnlock, bool isBlocked)
        {
            if (isBlocked) return GetOrCreateNodeFrameBlocked();
            if (isUnlocked) return GetOrCreateNodeFrameUnlocked();
            if (canUnlock) return GetOrCreateNodeFrameAvailable();
            return GetOrCreateNodeFrameLocked();
        }

        private static Sprite GetOrCreateNodeFrameUnlocked()
        {
            if (_nodeFrameUnlocked != null) return _nodeFrameUnlocked;
            // Radiant Emerald/Gold active rune badge
            _nodeFrameUnlocked = CreateRoundBadgeSprite(48,
                borderRim: new Color(0.95f, 0.85f, 0.35f, 1.0f),
                innerRim: new Color(0.20f, 0.80f, 0.45f, 1.0f),
                bgCol: new Color(0.08f, 0.20f, 0.14f, 1.0f));
            return _nodeFrameUnlocked;
        }

        private static Sprite GetOrCreateNodeFrameAvailable()
        {
            if (_nodeFrameAvailable != null) return _nodeFrameAvailable;
            // Glowing pulsing gold border
            _nodeFrameAvailable = CreateRoundBadgeSprite(48,
                borderRim: new Color(1.0f, 0.90f, 0.40f, 1.0f),
                innerRim: new Color(0.65f, 0.55f, 0.20f, 1.0f),
                bgCol: new Color(0.18f, 0.16f, 0.10f, 1.0f));
            return _nodeFrameAvailable;
        }

        private static Sprite GetOrCreateNodeFrameLocked()
        {
            if (_nodeFrameLocked != null) return _nodeFrameLocked;
            // Elegant dark slate metal rune
            _nodeFrameLocked = CreateRoundBadgeSprite(48,
                borderRim: new Color(0.35f, 0.40f, 0.50f, 0.9f),
                innerRim: new Color(0.20f, 0.24f, 0.32f, 0.9f),
                bgCol: new Color(0.09f, 0.11f, 0.15f, 1.0f));
            return _nodeFrameLocked;
        }

        private static Sprite GetOrCreateNodeFrameBlocked()
        {
            if (_nodeFrameBlocked != null) return _nodeFrameBlocked;
            // Crimson locked seal
            _nodeFrameBlocked = CreateRoundBadgeSprite(48,
                borderRim: new Color(0.65f, 0.20f, 0.20f, 0.9f),
                innerRim: new Color(0.35f, 0.10f, 0.10f, 0.9f),
                bgCol: new Color(0.15f, 0.06f, 0.06f, 1.0f));
            return _nodeFrameBlocked;
        }

        private static Sprite CreateRoundBadgeSprite(int size, Color borderRim, Color innerRim, Color bgCol)
        {
            var tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            tex.filterMode = FilterMode.Point;
            Color[] p = new Color[size * size];
            float r = (size / 2f) - 1.5f;
            float cx = size / 2f;
            float cy = size / 2f;

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float dx = x - cx;
                    float dy = y - cy;
                    float dist = Mathf.Sqrt(dx * dx + dy * dy);

                    if (dist > r)
                    {
                        p[y * size + x] = Color.clear;
                    }
                    else if (dist >= r - 2.5f)
                    {
                        p[y * size + x] = borderRim;
                    }
                    else if (dist >= r - 4.5f)
                    {
                        p[y * size + x] = innerRim;
                    }
                    else
                    {
                        // Soft radial shading inside badge
                        float shadow = dist / (r - 4.5f);
                        p[y * size + x] = Color.Lerp(bgCol, new Color(bgCol.r * 1.3f, bgCol.g * 1.3f, bgCol.b * 1.3f, 1f), 1f - shadow);
                    }
                }
            }

            tex.SetPixels(p);
            tex.Apply();
            return Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 16);
        }
    }
}
