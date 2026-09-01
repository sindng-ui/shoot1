using System;
using UnityEngine;
using HappyShoot.Domain.Progression;

namespace HappyShoot.View.SkillTree
{
    /// <summary>
    /// Generates high-contrast brilliant-cut procedural pixel-art sprites for gems (Ruby, Emerald, Amethyst).
    /// Strictly modular and under 500 lines (500-line architecture rule).
    /// </summary>
    public static class GemSpriteHelper
    {
        private static Sprite _rubySprite;
        private static Sprite _emeraldSprite;
        private static Sprite _amethystSprite;

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

        public static Sprite GetOrCreateRubySprite(int size = 32)
        {
            if (_rubySprite != null) return _rubySprite;
            var tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            tex.filterMode = FilterMode.Point;
            Color[] p = new Color[size * size];
            for (int i = 0; i < p.Length; i++) p[i] = Color.clear;

            Color glint = new Color(1.0f, 1.0f, 1.0f, 1.0f);
            Color highlight = new Color(1.0f, 0.65f, 0.75f, 1.0f);
            Color mainBright = new Color(1.0f, 0.20f, 0.35f, 1.0f);
            Color mainDark = new Color(0.85f, 0.10f, 0.22f, 1.0f);
            Color shadow = new Color(0.50f, 0.04f, 0.12f, 1.0f);
            Color deepShadow = new Color(0.28f, 0.02f, 0.08f, 1.0f);
            Color outline = new Color(0.12f, 0.02f, 0.05f, 0.95f);

            int cx = size / 2;

            for (int y = 4; y <= 27; y++)
            {
                int halfW = (y <= 20) ? (int)((y - 4) * (11f / 16f)) + 1 : (int)(12 - (y - 20) * (3f / 7f));
                for (int x = cx - halfW; x <= cx + halfW; x++)
                {
                    int dx = x - cx;
                    bool isBorder = (x == cx - halfW || x == cx + halfW || y == 4 || y == 27);
                    if (isBorder)
                    {
                        p[y * size + x] = outline;
                    }
                    else if (y >= 21)
                    {
                        if (y >= 24 && dx >= -5 && dx <= -1) p[y * size + x] = glint;
                        else if (dx < 0) p[y * size + x] = highlight;
                        else if (dx <= 4) p[y * size + x] = mainBright;
                        else p[y * size + x] = mainDark;
                    }
                    else
                    {
                        if (dx < -halfW / 2) p[y * size + x] = (y > 12) ? highlight : mainBright;
                        else if (dx < 0) p[y * size + x] = mainBright;
                        else if (dx <= halfW / 2) p[y * size + x] = mainDark;
                        else p[y * size + x] = (y < 12) ? deepShadow : shadow;
                    }
                }
            }

            DrawFacetLine(p, size, cx - 8, 21, cx + 8, 21, outline * 0.7f + mainBright * 0.3f);
            DrawFacetLine(p, size, cx - 6, 21, cx, 5, outline * 0.8f + mainDark * 0.2f);
            DrawFacetLine(p, size, cx + 6, 21, cx, 5, outline * 0.8f + shadow * 0.2f);
            DrawFacetLine(p, size, cx, 21, cx, 5, mainBright * 0.5f + mainDark * 0.5f);

            p[25 * size + (cx - 3)] = glint;
            p[26 * size + (cx - 3)] = glint;
            p[25 * size + (cx - 4)] = glint;
            p[24 * size + (cx - 3)] = highlight;
            p[25 * size + (cx - 2)] = highlight;

            tex.SetPixels(p);
            tex.Apply();
            _rubySprite = Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 16);
            return _rubySprite;
        }

        public static Sprite GetOrCreateEmeraldSprite(int size = 32)
        {
            if (_emeraldSprite != null) return _emeraldSprite;
            var tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            tex.filterMode = FilterMode.Point;
            Color[] p = new Color[size * size];
            for (int i = 0; i < p.Length; i++) p[i] = Color.clear;

            Color glint = new Color(1.0f, 1.0f, 1.0f, 1.0f);
            Color highlight = new Color(0.70f, 1.0f, 0.82f, 1.0f);
            Color mintGlow = new Color(0.30f, 0.95f, 0.55f, 1.0f);
            Color mainGreen = new Color(0.08f, 0.80f, 0.35f, 1.0f);
            Color darkForest = new Color(0.04f, 0.45f, 0.18f, 1.0f);
            Color deepShadow = new Color(0.02f, 0.25f, 0.10f, 1.0f);
            Color outline = new Color(0.02f, 0.12f, 0.05f, 0.95f);

            int cx = size / 2;
            int cy = size / 2;
            int minX = cx - 10, maxX = cx + 10;
            int minY = cy - 11, maxY = cy + 11;
            int cornerCut = 5;

            for (int y = minY; y <= maxY; y++)
            {
                for (int x = minX; x <= maxX; x++)
                {
                    int dTopLeft = (minX + cornerCut - x) + (y - (maxY - cornerCut));
                    int dTopRight = (x - (maxX - cornerCut)) + (y - (maxY - cornerCut));
                    int dBotLeft = (minX + cornerCut - x) + ((minY + cornerCut) - y);
                    int dBotRight = (x - (maxX - cornerCut)) + ((minY + cornerCut) - y);

                    if ((x < minX + cornerCut && y > maxY - cornerCut && dTopLeft > cornerCut) ||
                        (x > maxX - cornerCut && y > maxY - cornerCut && dTopRight > cornerCut) ||
                        (x < minX + cornerCut && y < minY + cornerCut && dBotLeft > cornerCut) ||
                        (x > maxX - cornerCut && y < minY + cornerCut && dBotRight > cornerCut))
                        continue;

                    bool isBorder = (x == minX || x == maxX || y == minY || y == maxY ||
                        dTopLeft == cornerCut || dTopRight == cornerCut || dBotLeft == cornerCut || dBotRight == cornerCut);

                    if (isBorder)
                    {
                        p[y * size + x] = outline;
                    }
                    else
                    {
                        bool isTable = (x >= cx - 5 && x <= cx + 5 && y >= cy - 6 && y <= cy + 6);
                        if (isTable)
                        {
                            if (x < cx - 2 && y > cy + 2) p[y * size + x] = highlight;
                            else if (x < cx && y > cy - 2) p[y * size + x] = mintGlow;
                            else if (x <= cx + 2) p[y * size + x] = mainGreen;
                            else p[y * size + x] = darkForest;
                        }
                        else
                        {
                            if (y > maxY - 4 && x < cx) p[y * size + x] = highlight;
                            else if (x < minX + 4) p[y * size + x] = mintGlow;
                            else if (y > cy) p[y * size + x] = mainGreen;
                            else if (x > cx + 4 || y < minY + 4) p[y * size + x] = deepShadow;
                            else p[y * size + x] = darkForest;
                        }
                    }
                }
            }

            DrawFacetLine(p, size, cx - 5, cy + 6, cx + 5, cy + 6, outline * 0.6f + mintGlow * 0.4f);
            DrawFacetLine(p, size, cx - 5, cy - 6, cx + 5, cy - 6, outline * 0.7f + darkForest * 0.3f);
            DrawFacetLine(p, size, cx - 5, cy - 6, cx - 5, cy + 6, outline * 0.6f + mintGlow * 0.4f);
            DrawFacetLine(p, size, cx + 5, cy - 6, cx + 5, cy + 6, outline * 0.8f + deepShadow * 0.2f);

            DrawFacetLine(p, size, cx - 5, cy + 6, minX + cornerCut, maxY, outline * 0.7f + highlight * 0.3f);
            DrawFacetLine(p, size, cx + 5, cy + 6, maxX - cornerCut, maxY, outline * 0.7f + mainGreen * 0.3f);
            DrawFacetLine(p, size, cx - 5, cy - 6, minX + cornerCut, minY, outline * 0.7f + darkForest * 0.3f);
            DrawFacetLine(p, size, cx + 5, cy - 6, maxX - cornerCut, minY, outline * 0.8f + deepShadow * 0.2f);

            p[(maxY - 3) * size + (minX + 4)] = glint;
            p[(maxY - 2) * size + (minX + 4)] = glint;
            p[(maxY - 3) * size + (minX + 3)] = glint;
            p[(maxY - 4) * size + (minX + 4)] = highlight;
            p[(maxY - 3) * size + (minX + 5)] = highlight;

            tex.SetPixels(p);
            tex.Apply();
            _emeraldSprite = Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 16);
            return _emeraldSprite;
        }

        public static Sprite GetOrCreateAmethystSprite(int size = 32)
        {
            if (_amethystSprite != null) return _amethystSprite;
            var tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            tex.filterMode = FilterMode.Point;
            Color[] p = new Color[size * size];
            for (int i = 0; i < p.Length; i++) p[i] = Color.clear;

            Color glint = new Color(1.0f, 1.0f, 1.0f, 1.0f);
            Color highlight = new Color(0.92f, 0.75f, 1.0f, 1.0f);
            Color radiantPurple = new Color(0.78f, 0.35f, 1.0f, 1.0f);
            Color royalViolet = new Color(0.60f, 0.15f, 0.90f, 1.0f);
            Color deepPlum = new Color(0.38f, 0.05f, 0.60f, 1.0f);
            Color darkAbyss = new Color(0.18f, 0.02f, 0.32f, 1.0f);
            Color outline = new Color(0.08f, 0.01f, 0.15f, 0.95f);

            int cx = size / 2, cy = size / 2;
            int rx = 11, ry = 12;

            for (int y = cy - ry; y <= cy + ry; y++)
            {
                for (int x = cx - rx; x <= cx + rx; x++)
                {
                    float dx = (float)(x - cx) / rx;
                    float dy = (float)(y - cy) / ry;
                    float dist = Mathf.Abs(dx) + Mathf.Abs(dy);

                    if (dist <= 1.05f)
                    {
                        bool isBorder = (dist >= 0.88f);
                        if (isBorder)
                        {
                            p[y * size + x] = outline;
                        }
                        else
                        {
                            float innerDist = Mathf.Abs(dx * 1.8f) + Mathf.Abs(dy * 1.8f);
                            if (innerDist <= 0.65f)
                            {
                                if (dx < 0 && dy > 0) p[y * size + x] = highlight;
                                else if (dx < 0 || dy > 0) p[y * size + x] = radiantPurple;
                                else p[y * size + x] = royalViolet;
                            }
                            else if (dx <= 0 && dy >= 0) p[y * size + x] = (dx < -0.3f && dy > 0.3f) ? highlight : radiantPurple;
                            else if (dx > 0 && dy >= 0) p[y * size + x] = royalViolet;
                            else if (dx <= 0 && dy < 0) p[y * size + x] = royalViolet;
                            else p[y * size + x] = (dx > 0.4f || dy < -0.4f) ? darkAbyss : deepPlum;
                        }
                    }
                }
            }

            DrawFacetLine(p, size, cx - rx + 2, cy, cx + rx - 2, cy, outline * 0.7f + radiantPurple * 0.3f);
            DrawFacetLine(p, size, cx, cy - ry + 2, cx, cy + ry - 2, outline * 0.7f + radiantPurple * 0.3f);

            p[(cy + ry - 4) * size + (cx - 2)] = glint;
            p[(cy + ry - 3) * size + (cx - 2)] = glint;
            p[(cy + ry - 4) * size + (cx - 3)] = glint;
            p[(cy + ry - 5) * size + (cx - 2)] = highlight;
            p[(cy + ry - 4) * size + (cx - 1)] = highlight;

            tex.SetPixels(p);
            tex.Apply();
            _amethystSprite = Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 16);
            return _amethystSprite;
        }

        private static void DrawFacetLine(Color[] p, int size, int x0, int y0, int x1, int y1, Color col)
        {
            int dx = Math.Abs(x1 - x0);
            int dy = Math.Abs(y1 - y0);
            int sx = x0 < x1 ? 1 : -1;
            int sy = y0 < y1 ? 1 : -1;
            int err = dx - dy;

            while (true)
            {
                if (x0 >= 0 && x0 < size && y0 >= 0 && y0 < size)
                {
                    int idx = y0 * size + x0;
                    if (p[idx].a > 0.1f)
                    {
                        p[idx] = Color.Lerp(p[idx], col, 0.65f);
                    }
                }
                if (x0 == x1 && y0 == y1) break;
                int e2 = 2 * err;
                if (e2 > -dy) { err -= dy; x0 += sx; }
                if (e2 < dx) { err += dx; y0 += sy; }
            }
        }
    }
}
