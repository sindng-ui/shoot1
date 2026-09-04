using System.Globalization;

namespace HappyShoot.View.UI
{
    /// <summary>
    /// Zero-allocation memoization cache for formatted damage text strings.
    /// Eliminates all runtime string allocations (GC Alloc) during intense combat by pre-generating
    /// comma-separated strings ("1,428", "2,285", "476", "1,428!") for common damage values.
    /// </summary>
    public static class DamageNumberCache
    {
        private const int CacheLimit = 3000;
        private static readonly string[] _normalStrings;
        private static readonly string[] _criticalStrings;
        private static readonly CultureInfo _culture;

        private const char MicroSpace = '\u200A'; // Unicode Hair Space (ultrafine 1/12em spacing)

        static DamageNumberCache()
        {
            _culture = CultureInfo.InvariantCulture;
            _normalStrings = new string[CacheLimit + 1];
            _criticalStrings = new string[CacheLimit + 1];

            for (int i = 0; i <= CacheLimit; i++)
            {
                // Format with thousand separators (e.g. 1,428)
                string formatted = i.ToString("N0", _culture);
                string spaced = InsertMicroSpacing(formatted);
                _normalStrings[i] = spaced;
                _criticalStrings[i] = spaced + "!";
            }
        }

        /// <summary>
        /// Inserts an ultrafine Hair Space (\u200A) between consecutive digits for clean, legible breathing room.
        /// </summary>
        private static string InsertMicroSpacing(string str)
        {
            if (string.IsNullOrEmpty(str) || str.Length <= 1) return str;

            var sb = new System.Text.StringBuilder(str.Length * 2);
            for (int i = 0; i < str.Length; i++)
            {
                sb.Append(str[i]);
                if (i < str.Length - 1 && char.IsDigit(str[i]) && char.IsDigit(str[i + 1]))
                {
                    sb.Append(MicroSpace);
                }
            }
            return sb.ToString();
        }

        /// <summary>
        /// Retrieves a zero-allocation formatted string for the given damage value.
        /// </summary>
        public static string GetString(float damage, bool isCritical)
        {
            int intDmg = (int)(damage + 0.5f);
            if (intDmg < 0) intDmg = 0;

            if (intDmg <= CacheLimit)
            {
                return isCritical ? _criticalStrings[intDmg] : _normalStrings[intDmg];
            }

            // Fallback for massive overkill numbers above CacheLimit
            string largeFormatted = InsertMicroSpacing(intDmg.ToString("N0", _culture));
            return isCritical ? (largeFormatted + "!") : largeFormatted;
        }
    }
}
