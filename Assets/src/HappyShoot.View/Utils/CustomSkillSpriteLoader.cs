using System.IO;
using UnityEngine;

namespace HappyShoot.View.Utils
{
    /// <summary>
    /// Loads and caches custom high-resolution skill effect sprites (e.g. Chain Lightning).
    /// Provides transparent fallback to procedural sprites if custom assets are missing.
    /// </summary>
    public static class CustomSkillSpriteLoader
    {
        private static Sprite _cachedChainLightningSprite;
        private static bool _attemptedLoad;

        /// <summary>
        /// Attempts to get the custom chain lightning sprite from Resources/skills/ChainLightning.
        /// Falls back to procedural plasma beam sprite if not found.
        /// </summary>
        public static Sprite GetOrCreateChainLightningSprite()
        {
            if (_cachedChainLightningSprite != null)
                return _cachedChainLightningSprite;

            if (_attemptedLoad)
                return WizardSkillSpriteHelper.GetOrCreateLightningBeamSprite();

            _attemptedLoad = true;
            _cachedChainLightningSprite = LoadChainLightningSpriteInternal();

            return _cachedChainLightningSprite ?? WizardSkillSpriteHelper.GetOrCreateLightningBeamSprite();
        }

        private static Sprite LoadChainLightningSpriteInternal()
        {
            // Try different Resource paths (case tolerance & filename variants)
            string[] resourcePaths = new[]
            {
                "skills/ChainLightning/chainlightning_ready",
                "Skills/ChainLightning/chainlightning_ready",
                "skills/ChainLightning/chainlightning",
                "Skills/ChainLightning/chainlightning"
            };

            foreach (var path in resourcePaths)
            {
                var tex = Resources.Load<Texture2D>(path);
                if (tex == null)
                {
                    var sp = Resources.Load<Sprite>(path);
                    if (sp != null) tex = sp.texture;
                }

                if (tex != null)
                {
                    return CreateSpriteFromTexture(tex);
                }
            }

            // Direct file I/O fallback for immediate loading without waiting for Unity DB reindex
            string[] diskPaths = new[]
            {
                Path.Combine(Application.dataPath, "Resources", "skills", "ChainLightning", "chainlightning_ready.png"),
                Path.Combine(Application.dataPath, "Resources", "Skills", "ChainLightning", "chainlightning_ready.png"),
                Path.Combine(Application.dataPath, "Resources", "skills", "ChainLightning", "chainlightning.png"),
                Path.Combine(Application.dataPath, "Resources", "Skills", "ChainLightning", "chainlightning.png")
            };

            foreach (var diskPath in diskPaths)
            {
                if (File.Exists(diskPath))
                {
                    try
                    {
                        byte[] fileData = File.ReadAllBytes(diskPath);
                        var diskTex = new Texture2D(2, 2, TextureFormat.RGBA32, mipChain: false);
                        if (diskTex.LoadImage(fileData))
                        {
                            return CreateSpriteFromTexture(diskTex);
                        }
                    }
                    catch
                    {
                        // Ignore and try next path
                    }
                }
            }

            return null;
        }

        private static Sprite CreateSpriteFromTexture(Texture2D tex)
        {
            tex.filterMode = FilterMode.Bilinear;
            tex.wrapMode = TextureWrapMode.Clamp;
            // PPU 100: Pivot exactly in center (0.5f, 0.5f)
            return Sprite.Create(
                tex,
                new Rect(0, 0, tex.width, tex.height),
                new Vector2(0.5f, 0.5f),
                100f
            );
        }

        /// <summary>
        /// Clears the cached sprite (useful for live reload during testing).
        /// </summary>
        public static void ClearCache()
        {
            _cachedChainLightningSprite = null;
            _attemptedLoad = false;
        }
    }
}
