using System;
using System.Globalization;
using System.IO;

namespace POESKillTree.Utils.Extensions
{
    public static class StringExtensions
    {
        public static bool Contains(this string source, string toCheck, StringComparison comp)
        {
            return source.IndexOf(toCheck, comp) >= 0;
        }

        public static string EnsureTrailingDirectorySeparator(this string path)
        {
            return path.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar) +
                   Path.DirectorySeparatorChar;
        }

        /// <summary>
        /// Equal to <code>int.Parse(s, CultureInfo.InvariantCulture)</code>
        /// </summary>
        public static int ParseInt(this string s)
        {
            return int.Parse(s, CultureInfo.InvariantCulture);
        }

        /// <summary>
        /// Equal to <code>int.TryParse(s, NumberStyles.Integer, CultureInfo.InvariantCulture, out i)</code>
        /// </summary>
        public static bool TryParseInt(this string s, out int i)
        {
            return int.TryParse(s, NumberStyles.Integer, CultureInfo.InvariantCulture, out i);
        }

        /// <summary>
        /// Equal to <code>int.Parse(s, CultureInfo.InvariantCulture)</code>
        /// </summary>
        public static uint ParseUint(this string s)
        {
            return uint.Parse(s, CultureInfo.InvariantCulture);
        }

        /// <summary>
        /// Equal to <code>int.TryParse(s, NumberStyles.Integer, CultureInfo.InvariantCulture, out i)</code>
        /// </summary>
        public static bool TryParseUint(this string s, out uint i)
        {
            return uint.TryParse(s, NumberStyles.Integer, CultureInfo.InvariantCulture, out i);
        }

        /// <summary>
        /// Equal to <code>float.Parse(s, CultureInfo.InvariantCulture)</code>
        /// </summary>
        public static float ParseFloat(this string s)
        {
            return float.Parse(s, CultureInfo.InvariantCulture);
        }

        /// <summary>
        /// Equal to <code>float.TryParse(s, NumberStyles.Float, CultureInfo.InvariantCulture, out f)</code>
        /// </summary>
        public static bool TryParseFloat(this string s, out float f)
        {
            return float.TryParse(s, NumberStyles.Float, CultureInfo.InvariantCulture, out f);
        }

        /// <summary>
        /// Equal to <code>float.Parse(s, CultureInfo.InvariantCulture)</code>
        /// </summary>
        public static double ParseDouble(this string s)
        {
            return double.Parse(s, CultureInfo.InvariantCulture);
        }

        /// <summary>
        /// Equal to <code>float.TryParse(s, NumberStyles.Float, CultureInfo.InvariantCulture, out f)</code>
        /// </summary>
        public static bool TryParseDouble(this string s, out double f)
        {
            return double.TryParse(s, NumberStyles.Float, CultureInfo.InvariantCulture, out f);
        }
    }
}
