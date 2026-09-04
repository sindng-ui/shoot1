using UnityEngine;
using HappyShoot.View.Utils;

namespace HappyShoot.View.UI
{
    /// <summary>
    /// Loads and caches high-impact, heavy gaming bold fonts specifically tuned for floating damage numbers.
    /// Prioritizes punchy, thick graphic typefaces across Windows, Android, and iOS devices.
    /// </summary>
    public static class DamageFontHelper
    {
        private static Font _damageFont;

        /// <summary>
        /// Retrieves the best available heavy bold graphic font for damage numbers.
        /// </summary>
        public static Font GetDamageFont()
        {
            if (_damageFont != null) return _damageFont;

            // Use Impact as the primary sharp arcade combat typeface
            string[] boldFontNames = new string[]
            {
                "Impact",               // Primary arcade combat bold font (tight condensed glyphs)
                "Arial Black",          // Heavy fallback
                "Segoe UI Black",       // Modern fallback
                "Roboto-Black",         // Android primary ultra-bold
                "sans-serif-black",     // Android fallback heavy
                "Trebuchet MS Bold",    // Playful thick arcade geometry
                "Helvetica-Bold",       // iOS crisp bold
                "Malgun Gothic Bold",   // Korean thick bold
                "맑은 고딕",
                "Arial"                 // Universal fallback
            };

            _damageFont = Font.CreateDynamicFontFromOSFont(boldFontNames, 36);

            if (_damageFont == null)
            {
                _damageFont = FontHelper.GetKoreanFont();
            }

            return _damageFont;
        }
    }
}
