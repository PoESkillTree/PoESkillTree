using System;
using System.Collections.Generic;
using System.Globalization;
using System.Runtime.Serialization;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace PoESkillTree.GameModel.StatTranslation
{
    /// <summary>
    /// Enum of the formats used in RePoE's stat_translations. The EnumMember annotations specify how the formats
    /// appear in the json.
    /// </summary>
    [JsonConverter(typeof(StringEnumConverter))]
    public enum Format
    {
        [EnumMember(Value = "#")]
        AsIs,
        [EnumMember(Value = "+#")]
        PrependPlus,
        [EnumMember(Value = "#%")]
        AppendPercent,
        [EnumMember(Value = "+#%")]
        PrependPlusAppendPercent,
        [EnumMember(Value = "ignored")]
        Ignore
    }

    public static class FormatExtensions
    {
        private static readonly IReadOnlyDictionary<Format, Func<double, string, string>> FormatFunctions
            = new Dictionary<Format, Func<double, string, string>>
            {
                {
                    Format.AsIs,
                    (d, s) => s
                },
                {
                    Format.PrependPlus,
                    (d, s) => d >= 0 ? "+" + s : s
                },
                {
                    Format.AppendPercent,
                    (d, s) => s + "%"
                },
                {
                    Format.PrependPlusAppendPercent,
                    (d, s) => (d >= 0 ? "+" + s : s) + "%"
                },
                {
                    Format.Ignore,
                    (d, s) => ""
                }
            };

        /// <summary>
        /// Applies this format onto the given value and returns the formatted string.
        /// </summary>
        public static string Apply(this Format format, double value)
            => FormatFunctions[format](value, value.ToString(CultureInfo.InvariantCulture));
    }
}