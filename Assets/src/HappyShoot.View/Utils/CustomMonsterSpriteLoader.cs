using System.Collections.Generic;
using System.IO;
using UnityEngine;
using HappyShoot.Domain.Entities;

namespace HappyShoot.View.Utils
{
    /// <summary>
    /// Loads and manages high-resolution custom monster sprites with robust scaling & pivot control:
    /// Forces explicit archetype-tuned PPU and Pivot to align with 2.5D Blob Shadows and hitboxes.
    /// Strictly modular and under 250 lines (500-line architecture rule).
    /// </summary>
    public static class CustomMonsterSpriteLoader
    {
        private static readonly Dictionary<MonsterType, Sprite> _customCache = new Dictionary<MonsterType, Sprite>(16);

        // Archetype-tuned high-res PPU (1024x1024 canvas with ~600-850px character height)
        // Scaled to exactly match existing 1.0 unit base scale in MonsterView.cs
        private const float SlimePPU = 1400f;
        private const float BatPPU = 650f;
        private const float SkeletonPPU = 750f;
        private const float FireImpPPU = 650f;
        private const float ToxicSpiderPPU = 650f;
        private const float DarkKnightPPU = 600f;
        private const float GolemPPU = 650f;
        private const float LichKingPPU = 650f;
        private const float Boss1PPU = 550f;
        private const float Boss2PPU = 550f;

        public static Sprite TryGetCustomMonsterSprite(MonsterType type)
        {
            if (_customCache.TryGetValue(type, out var cached) && cached != null)
                return cached;

            string subFolder;
            string fileName;
            float ppu;
            Vector2 pivot;

            switch (type)
            {
                case MonsterType.Slime:
                    subFolder = "Monsters/Slime";
                    fileName = "slime";
                    ppu = SlimePPU;
                    pivot = new Vector2(0.5f, 0.20f);
                    break;
                case MonsterType.Bat:
                    subFolder = "Monsters/VampireBat";
                    fileName = "vampirebat";
                    ppu = BatPPU;
                    pivot = new Vector2(0.5f, 0.50f); // Centered for wing flutter
                    break;
                case MonsterType.Skeleton:
                    subFolder = "Monsters/Skeleton";
                    fileName = "skeleton";
                    ppu = SkeletonPPU;
                    pivot = new Vector2(0.5f, 0.15f);
                    break;
                case MonsterType.FireImp:
                    subFolder = "Monsters/FireImp";
                    fileName = "fireimp";
                    ppu = FireImpPPU;
                    pivot = new Vector2(0.5f, 0.20f);
                    break;
                case MonsterType.ToxicSpider:
                    subFolder = "Monsters/ToxicSpider";
                    fileName = "toxicspider";
                    ppu = ToxicSpiderPPU;
                    pivot = new Vector2(0.5f, 0.20f);
                    break;
                case MonsterType.DarkKnight:
                    subFolder = "Monsters/DarkNight";
                    fileName = "darknight";
                    ppu = DarkKnightPPU;
                    pivot = new Vector2(0.5f, 0.22f);
                    break;
                case MonsterType.Golem:
                    subFolder = "Monsters/AncientRockGolem";
                    fileName = "ancientrockgolem";
                    ppu = GolemPPU;
                    pivot = new Vector2(0.5f, 0.12f);
                    break;
                case MonsterType.Boss:
                    subFolder = "Monsters/Boss1";
                    fileName = "boss1";
                    ppu = Boss1PPU;
                    pivot = new Vector2(0.5f, 0.15f);
                    break;
                case MonsterType.Boss2:
                    subFolder = "Monsters/Boss2";
                    fileName = "boss2";
                    ppu = Boss2PPU;
                    pivot = new Vector2(0.5f, 0.15f);
                    break;
                case MonsterType.Boss3:
                    subFolder = "Monsters/LichKing";
                    fileName = "lichking";
                    ppu = LichKingPPU;
                    pivot = new Vector2(0.5f, 0.15f);
                    break;
                default:
                    return null;
            }

            Sprite result = LoadSpriteInternal(subFolder, fileName, pivot, ppu);
            if (result != null)
            {
                _customCache[type] = result;
            }
            return result;
        }

        public static void ClearCache()
        {
            _customCache.Clear();
        }

        private static Sprite LoadSpriteInternal(string subFolder, string fileName, Vector2 pivot, float ppu)
        {
            string resPath = $"{subFolder}/{fileName}";
            Texture2D tex = null;

            // 1. In Editor or standalone runtime, direct disk loading ensures 100% instant hot-reload
            string diskPath = Path.Combine(Application.dataPath, "Resources", subFolder, $"{fileName}.png");
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
                catch
                {
                    // Ignore and fallback to Resources.Load
                }
            }

            // 2. Fallback: Try loading Texture2D directly from Resources
            if (tex == null)
            {
                tex = Resources.Load<Texture2D>(resPath);
            }

            // 3. Fallback: Try loading Sprite and borrow its underlying Texture2D
            if (tex == null)
            {
                var sprite = Resources.Load<Sprite>(resPath);
                if (sprite != null)
                {
                    tex = sprite.texture;
                }
            }

            // 4. Force Create Sprite with explicit Pivot and PPU
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
    }
}
