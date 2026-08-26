using UnityEngine;

namespace HappyShoot.View.Utils
{
    /// <summary>
    /// Procedural pixel-art sprite generator dedicated to Warrior skills
    /// (Greatsword Slash, Blood Eater, Ground Stomp, Whirlwind).
    /// Modularized to strictly respect the 500-line architecture rule.
    /// </summary>
    public static class WarriorSkillSpriteHelper
    {
        private static Sprite _slashArcSprite;
        private static Sprite _bloodSlashArcSprite;
        private static Sprite _groundStompSprite;
        private static Sprite _whirlwindBladeSprite;
        private static Sprite _bloodOrbSprite;

        /// <summary>
        /// 128x128 Solid Golden Greatsword Slash Fan Arc (150-degree sweep).
        /// Completely filled from character center (0m) to maximum reach with razor golden blade edge.
        /// </summary>
        public static Sprite GetOrCreateSlashArcSprite(int size = 128)
        {
            if (_slashArcSprite != null) return _slashArcSprite;

            var texture = new Texture2D(size, size, TextureFormat.RGBA32, false);
            texture.filterMode = FilterMode.Bilinear;
            texture.wrapMode = TextureWrapMode.Clamp;

            Vector2 center = new Vector2(size * 0.08f, size * 0.5f);
            float maxRadius = size * 0.88f;
            float innerRadius = size * 0.04f;

            Color bladeTipWhite = new Color(1.0f, 1.0f, 1.0f, 1f);
            Color bladeEdgeGold = new Color(1.0f, 0.90f, 0.30f, 1.0f);
            Color swordBodyOrange = new Color(1.0f, 0.60f, 0.08f, 0.92f);
            Color centerCoreGlow = new Color(1.0f, 0.98f, 0.70f, 0.98f);

            Color[] pixels = new Color[size * size];
            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    Vector2 pos = new Vector2(x + 0.5f, y + 0.5f);
                    float dist = Vector2.Distance(pos, center);
                    float angle = Mathf.Atan2(pos.y - center.y, pos.x - center.x) * Mathf.Rad2Deg;

                    if (dist >= innerRadius && dist <= maxRadius && angle >= -75f && angle <= 75f)
                    {
                        float normDist = Mathf.Clamp01((dist - innerRadius) / (maxRadius - innerRadius));
                        float angleNorm = (angle + 75f) / 150f;
                        float sweepAlpha = Mathf.Sin(angleNorm * Mathf.PI * 0.90f + 0.10f);

                        Color col;
                        if (normDist >= 0.78f)
                        {
                            float edgeT = (normDist - 0.78f) / 0.22f;
                            col = Color.Lerp(bladeEdgeGold, bladeTipWhite, edgeT);
                        }
                        else if (normDist <= 0.18f)
                        {
                            float centerT = normDist / 0.18f;
                            col = Color.Lerp(centerCoreGlow, swordBodyOrange, centerT);
                        }
                        else
                        {
                            float bodyT = (normDist - 0.18f) / 0.60f;
                            col = Color.Lerp(swordBodyOrange, bladeEdgeGold, bodyT);
                            float streamNoise = Mathf.Sin(dist * 0.45f + angle * 0.15f);
                            if (streamNoise > 0.5f) col = Color.Lerp(col, bladeTipWhite, 0.45f);
                        }

                        float edgeFade = normDist > 0.96f ? (1.0f - normDist) / 0.04f : 1.0f;
                        pixels[y * size + x] = new Color(col.r, col.g, col.b, col.a * sweepAlpha * edgeFade);
                    }
                    else
                    {
                        pixels[y * size + x] = Color.clear;
                    }
                }
            }

            texture.SetPixels(pixels);
            texture.Apply();
            _slashArcSprite = Sprite.Create(texture, new Rect(0, 0, size, size), new Vector2(0.08f, 0.5f), 32);
            return _slashArcSprite;
        }

        /// <summary>
        /// 128x128 Solid Crimson Blood Eater Greatsword Slash Fan Arc (150-degree sweep).
        /// Deep ruby red blade body with intense fiery crimson/white glowing edge.
        /// </summary>
        public static Sprite GetOrCreateBloodSlashArcSprite(int size = 128)
        {
            if (_bloodSlashArcSprite != null) return _bloodSlashArcSprite;

            var texture = new Texture2D(size, size, TextureFormat.RGBA32, false);
            texture.filterMode = FilterMode.Bilinear;
            texture.wrapMode = TextureWrapMode.Clamp;

            Vector2 center = new Vector2(size * 0.08f, size * 0.5f);
            float maxRadius = size * 0.88f;
            float innerRadius = size * 0.04f;

            Color bladeTipWhite = new Color(1.0f, 0.95f, 0.96f, 1f);
            Color bladeEdgeCrimson = new Color(1.0f, 0.18f, 0.30f, 1.0f);
            Color swordBodyRuby = new Color(0.80f, 0.04f, 0.15f, 0.95f);
            Color centerCorePink = new Color(1.0f, 0.55f, 0.65f, 0.98f);

            Color[] pixels = new Color[size * size];
            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    Vector2 pos = new Vector2(x + 0.5f, y + 0.5f);
                    float dist = Vector2.Distance(pos, center);
                    float angle = Mathf.Atan2(pos.y - center.y, pos.x - center.x) * Mathf.Rad2Deg;

                    if (dist >= innerRadius && dist <= maxRadius && angle >= -75f && angle <= 75f)
                    {
                        float normDist = Mathf.Clamp01((dist - innerRadius) / (maxRadius - innerRadius));
                        float angleNorm = (angle + 75f) / 150f;
                        float sweepAlpha = Mathf.Sin(angleNorm * Mathf.PI * 0.90f + 0.10f);

                        Color col;
                        if (normDist >= 0.78f)
                        {
                            float edgeT = (normDist - 0.78f) / 0.22f;
                            col = Color.Lerp(bladeEdgeCrimson, bladeTipWhite, edgeT);
                        }
                        else if (normDist <= 0.18f)
                        {
                            float centerT = normDist / 0.18f;
                            col = Color.Lerp(centerCorePink, swordBodyRuby, centerT);
                        }
                        else
                        {
                            float bodyT = (normDist - 0.18f) / 0.60f;
                            col = Color.Lerp(swordBodyRuby, bladeEdgeCrimson, bodyT);
                            float streamNoise = Mathf.Sin(dist * 0.5f + angle * 0.2f);
                            if (streamNoise > 0.5f) col = Color.Lerp(col, bladeTipWhite, 0.5f);
                        }

                        float edgeFade = normDist > 0.96f ? (1.0f - normDist) / 0.04f : 1.0f;
                        pixels[y * size + x] = new Color(col.r, col.g, col.b, col.a * sweepAlpha * edgeFade);
                    }
                    else
                    {
                        pixels[y * size + x] = Color.clear;
                    }
                }
            }

            texture.SetPixels(pixels);
            texture.Apply();
            _bloodSlashArcSprite = Sprite.Create(texture, new Rect(0, 0, size, size), new Vector2(0.08f, 0.5f), 32);
            return _bloodSlashArcSprite;
        }

        /// <summary>
        /// 128x128 High-Resolution Ground Stomp Crater & Impact Rim Sprite.
        /// </summary>
        public static Sprite GetOrCreateGroundStompSprite(int size = 128)
        {
            if (_groundStompSprite != null) return _groundStompSprite;

            var texture = new Texture2D(size, size, TextureFormat.RGBA32, false);
            texture.filterMode = FilterMode.Bilinear;
            texture.wrapMode = TextureWrapMode.Clamp;

            Vector2 center = new Vector2(size * 0.5f, size * 0.5f);
            float outerRadius = size * 0.48f;
            float innerPitRadius = size * 0.18f;

            Color craterPitDark = new Color(0.12f, 0.06f, 0.02f, 0.95f);
            Color fissureBrown = new Color(0.38f, 0.20f, 0.08f, 0.90f);
            Color magmaGlow = new Color(0.95f, 0.45f, 0.08f, 0.95f);
            Color rimGlowGold = new Color(1.0f, 0.75f, 0.20f, 0.95f);

            Color[] pixels = new Color[size * size];
            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    Vector2 pos = new Vector2(x + 0.5f, y + 0.5f);
                    float dist = Vector2.Distance(pos, center);
                    float angle = Mathf.Atan2(pos.y - center.y, pos.x - center.x);

                    if (dist <= outerRadius)
                    {
                        float normDist = dist / outerRadius;
                        float jaggedFissure = Mathf.Sin(angle * 12f) * 0.06f + Mathf.Cos(angle * 6f) * 0.04f;
                        float effectiveNorm = Mathf.Clamp01(normDist + jaggedFissure);

                        Color col;
                        if (dist <= innerPitRadius)
                        {
                            float pitT = dist / innerPitRadius;
                            col = Color.Lerp(craterPitDark, magmaGlow, pitT * 0.4f);
                        }
                        else if (effectiveNorm >= 0.82f)
                        {
                            float rimT = (effectiveNorm - 0.82f) / 0.18f;
                            col = Color.Lerp(magmaGlow, rimGlowGold, rimT);
                        }
                        else
                        {
                            float midT = (effectiveNorm - 0.25f) / 0.57f;
                            col = Color.Lerp(fissureBrown, magmaGlow, midT * 0.6f);
                            float fissureLine = Mathf.Abs(Mathf.Sin(angle * 8f));
                            if (fissureLine < 0.12f) col = Color.Lerp(col, magmaGlow, 0.75f);
                        }

                        float alpha = effectiveNorm > 0.90f ? Mathf.Clamp01((1.0f - effectiveNorm) / 0.10f) : 1.0f;
                        pixels[y * size + x] = new Color(col.r, col.g, col.b, col.a * alpha);
                    }
                    else
                    {
                        pixels[y * size + x] = Color.clear;
                    }
                }
            }

            texture.SetPixels(pixels);
            texture.Apply();
            _groundStompSprite = Sprite.Create(texture, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 16);
            return _groundStompSprite;
        }

        // Upheaval sprites are modularized in UpheavalSpriteHelper.cs
        public static Sprite GetOrCreateUpheavalWaveSprite(int size = 32) => UpheavalSpriteHelper.GetOrCreateUpheavalWaveSprite(size);
        public static Sprite GetOrCreateUpheavalChunkSprite(int w = 24, int h = 20) => UpheavalSpriteHelper.GetOrCreateUpheavalChunkSprite(w, h);
        public static Sprite GetOrCreateUpheavalSpikeSprite(int w = 18, int h = 24) => UpheavalSpriteHelper.GetOrCreateUpheavalSpikeSprite(w, h);

        /// <summary>
        /// 128x128 360-Degree Steel Cyclone 3-Blade Whirlwind Sprite.
        /// </summary>
        public static Sprite GetOrCreateWhirlwindBladeSprite(int size = 128)
        {
            if (_whirlwindBladeSprite != null) return _whirlwindBladeSprite;

            var texture = new Texture2D(size, size, TextureFormat.RGBA32, false);
            texture.filterMode = FilterMode.Bilinear;
            texture.wrapMode = TextureWrapMode.Clamp;

            Vector2 center = new Vector2(size * 0.5f, size * 0.5f);
            float outerRadius = size * 0.48f;
            float innerEyeRadius = size * 0.12f;

            Color bladeTipCyan = new Color(0.85f, 0.98f, 1.0f, 1.0f);
            Color bladeSteelBlue = new Color(0.40f, 0.75f, 0.95f, 0.95f);
            Color windTrailTeal = new Color(0.15f, 0.55f, 0.75f, 0.70f);

            Color[] pixels = new Color[size * size];
            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    Vector2 pos = new Vector2(x + 0.5f, y + 0.5f);
                    float dist = Vector2.Distance(pos, center);
                    float angle = Mathf.Atan2(pos.y - center.y, pos.x - center.x);

                    if (dist >= innerEyeRadius && dist <= outerRadius)
                    {
                        float normDist = (dist - innerEyeRadius) / (outerRadius - innerEyeRadius);
                        float spiral = angle + normDist * Mathf.PI * 1.5f;
                        float bladeIntensity = Mathf.Pow(Mathf.Sin(spiral * 1.5f) * 0.5f + 0.5f, 4f);

                        Color col = Color.Lerp(windTrailTeal, bladeSteelBlue, normDist);
                        if (bladeIntensity > 0.45f)
                        {
                            float bladeT = (bladeIntensity - 0.45f) / 0.55f;
                            col = Color.Lerp(col, bladeTipCyan, bladeT);
                        }

                        float ringEdgeFade = normDist > 0.88f ? (1.0f - normDist) / 0.12f : (normDist < 0.15f ? normDist / 0.15f : 1.0f);
                        pixels[y * size + x] = new Color(col.r, col.g, col.b, col.a * ringEdgeFade * (bladeIntensity * 0.7f + 0.3f));
                    }
                    else
                    {
                        pixels[y * size + x] = Color.clear;
                    }
                }
            }

            texture.SetPixels(pixels);
            texture.Apply();
            _whirlwindBladeSprite = Sprite.Create(texture, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 16);
            return _whirlwindBladeSprite;
        }

        /// <summary>
        /// 32x32 Glowing Crimson Blood Life-Steal Essence Orb Sprite.
        /// </summary>
        public static Sprite GetOrCreateBloodOrbSprite(int size = 32)
        {
            if (_bloodOrbSprite != null) return _bloodOrbSprite;

            var texture = new Texture2D(size, size, TextureFormat.RGBA32, false);
            texture.filterMode = FilterMode.Bilinear;
            texture.wrapMode = TextureWrapMode.Clamp;

            Vector2 center = new Vector2(size * 0.5f, size * 0.5f);
            float radius = size * 0.45f;

            Color corePink = new Color(1.0f, 0.6f, 0.75f, 1f);
            Color brightCrimson = new Color(1.0f, 0.12f, 0.22f, 0.95f);
            Color outerRuby = new Color(0.6f, 0.02f, 0.08f, 0.7f);

            Color[] pixels = new Color[size * size];
            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    Vector2 pos = new Vector2(x + 0.5f, y + 0.5f);
                    float dist = Vector2.Distance(pos, center);

                    if (dist <= radius)
                    {
                        float normDist = dist / radius;
                        Color col = normDist < 0.35f 
                            ? Color.Lerp(corePink, brightCrimson, normDist / 0.35f)
                            : Color.Lerp(brightCrimson, outerRuby, (normDist - 0.35f) / 0.65f);

                        float alpha = Mathf.Clamp01((1.0f - normDist * normDist));
                        pixels[y * size + x] = new Color(col.r, col.g, col.b, col.a * alpha);
                    }
                    else
                    {
                        pixels[y * size + x] = Color.clear;
                    }
                }
            }

            texture.SetPixels(pixels);
            texture.Apply();
            _bloodOrbSprite = Sprite.Create(texture, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 16);
            return _bloodOrbSprite;
        }
    }
}
