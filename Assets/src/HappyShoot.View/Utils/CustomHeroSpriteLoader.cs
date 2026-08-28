using System.Collections.Generic;
using System.IO;
using UnityEngine;
using HappyShoot.Domain.Entities;

namespace HappyShoot.View.Utils
{
    /// <summary>
    /// Loads and manages high-resolution custom hero sprites (Warrior & Ranger) with robust scaling & pivot control:
    /// Forces explicit class-tuned PPU and Pivot to avoid Unity's default 100 PPU / Center pivot overrides.
    /// </summary>
    public static class CustomHeroSpriteLoader
    {
        private static readonly Dictionary<string, Sprite> _customCache = new Dictionary<string, Sprite>(32);

        // Class-specific high-res PPU tuning:
        // Warrior: canvas 350x450, char height ~405px -> PPU 520f (scaled height ~1.29m)
        private const float WarriorPPU = 520f;
        // Ranger: canvas 350x450, char height ~325px -> PPU 400f (scaled height ~1.22m, nimble hunter ratio)
        private const float RangerPPU = 400f;
        // Wizard: canvas 350x450, char height ~365px -> PPU 450f (scaled height ~1.22m, cute pointed hat mage)
        private const float WizardPPU = 450f;

        // Shared bottom-aligned pivot: places feet precisely at Y = -0.33m, directly on top of BlobShadow (Y = -0.36m)
        private static readonly Vector2 DefaultPivot = new Vector2(0.5f, 0.30f);

        public static Sprite TryGetCustomHeroSprite(CharacterClassType classType, HeroSpriteHelper.ViewDirection dir)
        {
            string subFolder;
            string prefix;
            float ppu;
            Vector2 pivot = DefaultPivot;

            switch (classType)
            {
                case CharacterClassType.Warrior:
                    subFolder = "Characters/Warrior";
                    prefix = "warrior";
                    ppu = WarriorPPU;
                    break;
                case CharacterClassType.Ranger:
                    subFolder = "Characters/Ranger";
                    prefix = "ranger";
                    ppu = RangerPPU;
                    break;
                case CharacterClassType.Wizard:
                    subFolder = "Characters/Wizard";
                    prefix = "wizard";
                    ppu = WizardPPU;
                    break;
                default:
                    return null;
            }

            string dirSuffix = dir switch
            {
                HeroSpriteHelper.ViewDirection.Front => "front",
                HeroSpriteHelper.ViewDirection.FrontDiagonal => "front_diagonal",
                HeroSpriteHelper.ViewDirection.Side => "side",
                HeroSpriteHelper.ViewDirection.BackDiagonal => "back_diagonal",
                HeroSpriteHelper.ViewDirection.Back => "back",
                _ => "front"
            };

            string fileName = $"{prefix}_{dirSuffix}";
            string cacheKey = $"Custom_{classType}_{fileName}";
            if (_customCache.TryGetValue(cacheKey, out var cached) && cached != null)
                return cached;

            Sprite result = LoadSpriteInternal(subFolder, fileName, pivot, ppu);
            if (result != null)
            {
                _customCache[cacheKey] = result;
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

            // 1. In Editor or standalone runtime, direct disk loading ensures 100% instant hot-reload without stale Resources cache
            string diskPath = Path.Combine(Application.dataPath, "Resources", subFolder, $"{fileName}.png");
            if (File.Exists(diskPath))
            {
                try
                {
                    byte[] fileData = File.ReadAllBytes(diskPath);
                    var diskTex = new Texture2D(2, 2, TextureFormat.RGBA32, mipChain: false);
                    if (diskTex.LoadImage(fileData))
                    {
                        diskTex.filterMode = FilterMode.Point;
                        diskTex.anisoLevel = 0;
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
                tex.filterMode = FilterMode.Point;
                tex.anisoLevel = 0;
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
