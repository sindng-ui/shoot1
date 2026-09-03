using UnityEngine;
using HappyShoot.Domain.Entities;

namespace HappyShoot.View.Utils
{
    /// <summary>
    /// Helper for Boss sprites across all 3 phases:
    /// Boss 1: Magma Lord
    /// Boss 2: Venom Queen Arachne
    /// Boss 3: Arch-Lich King
    /// Strictly modular and under 500 lines.
    /// </summary>
    public static class BossSpriteHelper
    {
        private static Sprite _boss1Fallback;

        public static Sprite GetOrCreateBoss1Sprite()
        {
            var custom = CustomMonsterSpriteLoader.TryGetCustomMonsterSprite(MonsterType.Boss);
            if (custom != null) return custom;
            return GetOrCreateBoss1Fallback();
        }

        public static Sprite GetOrCreateBoss2Sprite()
        {
            var custom = CustomMonsterSpriteLoader.TryGetCustomMonsterSprite(MonsterType.Boss2);
            if (custom != null) return custom;
            return GetOrCreateBoss1Fallback();
        }

        public static Sprite GetOrCreateBoss3Sprite()
        {
            return Phase3MonsterSpriteHelper.GetOrCreateLichKingSprite();
        }

        public static Sprite GetOrCreateBoss1Fallback(int size = 56)
        {
            if (_boss1Fallback != null) return _boss1Fallback;

            var tex = new Texture2D(size, size, TextureFormat.RGBA32, false) { filterMode = FilterMode.Point };
            Color[] pixels = new Color[size * size];
            for (int i = 0; i < pixels.Length; i++) pixels[i] = Color.clear;

            Color bodyOuter = new Color(0.12f, 0.04f, 0.08f);
            Color bodyMid = new Color(0.50f, 0.12f, 0.18f);
            Color bodyInner = new Color(0.72f, 0.16f, 0.24f);
            Color hornBase = new Color(0.55f, 0.08f, 0.08f);
            Color hornTip = new Color(0.95f, 0.40f, 0.15f);
            Color eyeGlow = new Color(1.0f, 0.92f, 0.15f);
            Color eyeCore = new Color(1.0f, 0.45f, 0.05f);

            int cx = size / 2;
            int cy = size / 2 - 2;

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float dx = (x - cx) / 18.0f;
                    float dy = (y - cy) / 20.0f;
                    float dist = dx * dx + dy * dy;

                    if (y >= 4 && dist <= 1.0f)
                    {
                        if (dist >= 0.78f) pixels[y * size + x] = bodyOuter;
                        else if (dist >= 0.45f) pixels[y * size + x] = bodyMid;
                        else pixels[y * size + x] = bodyInner;
                    }

                    int hdx = Mathf.Abs(x - cx);
                    if (y >= 34 && y <= 52 && hdx >= 8 && hdx <= 22)
                    {
                        int hornCurve = cx + (x > cx ? 1 : -1) * (10 + (y - 34) / 2);
                        if (Mathf.Abs(x - hornCurve) <= 2)
                        {
                            pixels[y * size + x] = (y >= 46) ? hornTip : hornBase;
                        }
                    }

                    if (y >= 26 && y <= 30 && (hdx == 5 || hdx == 9))
                    {
                        pixels[y * size + x] = eyeGlow;
                        if (hdx == 5 && y == 28) pixels[y * size + x] = eyeCore;
                    }
                }
            }

            tex.SetPixels(pixels);
            tex.Apply();
            _boss1Fallback = Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.35f), size);
            return _boss1Fallback;
        }
    }
}
