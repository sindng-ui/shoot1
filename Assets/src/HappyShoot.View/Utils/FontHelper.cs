using UnityEngine;

namespace HappyShoot.View.Utils
{
    /// <summary>
    /// Utility helper providing dynamic Korean font loading across all presentation UI views.
    /// Safely falls back across OS fonts (Malgun Gothic, NanumGothic, AppleGothic, Arial).
    /// </summary>
    public static class FontHelper
    {
        private static Font _koreanFont;

        public static Font GetKoreanFont()
        {
            if (_koreanFont != null) return _koreanFont;

            // Preferred font list for crisp Korean character rendering
            string[] fontNames = new string[]
            {
                "Malgun Gothic",
                "맑은 고딕",
                "NanumGothic",
                "AppleGothic",
                "Arial",
                "Helvetica"
            };

            _koreanFont = Font.CreateDynamicFontFromOSFont(fontNames, 16);

            // Ultimate fallback to unity builtin runtime font if OS font fails
            if (_koreanFont == null)
            {
                _koreanFont = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            }

            return _koreanFont;
        }

        public static Font GetSystemDefaultFont() => GetKoreanFont();
    }
}
