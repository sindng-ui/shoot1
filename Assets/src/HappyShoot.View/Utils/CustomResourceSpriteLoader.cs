using System.Collections.Generic;
using System.IO;
using UnityEngine;
using HappyShoot.Domain.Progression;

namespace HappyShoot.View.Utils
{
    /// <summary>
    /// High-resolution custom sprite loader for gems, experience orbs, and gold coins.
    /// Loaded from Assets/Resources/Resources/{SubFolder}/{FileName}.png.
    /// Strictly modular, zero-allocation cache, and under 500 lines.
    /// </summary>
    public static class CustomResourceSpriteLoader
    {
        private static readonly Dictionary<string, Sprite> _cache = new Dictionary<string, Sprite>(16);

        private const float GemPPU = 500f;
        private const float ExpPPU = 650f;
        private const float CoinPPU = 500f;

        private static readonly Vector2 CenterPivot = new Vector2(0.5f, 0.5f);

        public static Sprite TryGetRubySprite() => GetOrLoad("Ruby", "ruby", CenterPivot, GemPPU);
        public static Sprite TryGetEmeraldSprite() => GetOrLoad("Emerald", "emerald", CenterPivot, GemPPU);
        public static Sprite TryGetAmethystSprite() => GetOrLoad("Amethyst", "amethyst", CenterPivot, GemPPU);
        public static Sprite TryGetExp1Sprite() => GetOrLoad("Exp1", "exp1", CenterPivot, ExpPPU);
        public static Sprite TryGetExp2Sprite() => GetOrLoad("Exp2", "exp2", CenterPivot, ExpPPU * 0.95f);
        public static Sprite TryGetGoldCoinSprite() => GetOrLoad("goldcoin", "goldcoin", CenterPivot, CoinPPU);

        public static Sprite TryGetGemSprite(GemType type)
        {
            switch (type)
            {
                case GemType.Ruby: return TryGetRubySprite();
                case GemType.Emerald: return TryGetEmeraldSprite();
                case GemType.Amethyst: return TryGetAmethystSprite();
                default: return TryGetRubySprite();
            }
        }

        private static Sprite GetOrLoad(string subFolder, string fileName, Vector2 pivot, float ppu)
        {
            string key = $"{subFolder}/{fileName}";
            if (_cache.TryGetValue(key, out var cached) && cached != null)
                return cached;

            Sprite sprite = LoadSpriteInternal(subFolder, fileName, pivot, ppu);
            if (sprite != null)
            {
                _cache[key] = sprite;
            }
            return sprite;
        }

        private static Sprite LoadSpriteInternal(string subFolder, string fileName, Vector2 pivot, float ppu)
        {
            Texture2D tex = null;

            // 1. Direct disk loading for instant Editor/Runtime hot-reload
            string diskPath = Path.Combine(Application.dataPath, "Resources", "Resources", subFolder, $"{fileName}.png");
            if (File.Exists(diskPath))
            {
                try
                {
                    byte[] fileData = File.ReadAllBytes(diskPath);
                    var diskTex = new Texture2D(2, 2, TextureFormat.RGBA32, mipChain: false);
                    if (diskTex.LoadImage(fileData))
                    {
                        diskTex.filterMode = FilterMode.Bilinear;
                        diskTex.anisoLevel = 2;
                        diskTex.Apply(updateMipmaps: false, makeNoLongerReadable: false);
                        tex = diskTex;
                    }
                }
                catch (System.Exception ex)
                {
                    Debug.LogWarning($"[CustomResourceSpriteLoader] Failed loading {diskPath}: {ex.Message}");
                }
            }

            // 2. Fallback: Resources.Load
            if (tex == null)
            {
                string resPath = $"Resources/{subFolder}/{fileName}";
                tex = Resources.Load<Texture2D>(resPath);
                if (tex == null)
                {
                    var resSprite = Resources.Load<Sprite>(resPath);
                    if (resSprite != null) tex = resSprite.texture;
                }
            }

            // 3. Create Sprite
            if (tex != null)
            {
                tex.filterMode = FilterMode.Bilinear;
                tex.anisoLevel = 2;
                return Sprite.Create(
                    tex,
                    new Rect(0, 0, tex.width, tex.height),
                    pivot,
                    ppu
                );
            }

            return null;
        }

        public static void ClearCache()
        {
            _cache.Clear();
        }
    }
}
