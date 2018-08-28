using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace PoESkillTree.GameModel.StatTranslation
{
    /// <summary>
    /// Enum of the index handlers used in the GGPK's stat description files/RePoE's stat_translations.
    /// The EnumMember annotations specify how the handlers appear in the json file.
    /// </summary>
    /// <remarks>
    /// The names and their effects were taken from
    /// https://github.com/OmegaK2/PyPoE/blob/dev/PyPoE/poe/file/translations.py#L1890
    /// </remarks>
    [JsonConverter(typeof(StringEnumConverter))]
    public enum IndexHandler
    {
        [EnumMember(Value = "60%_of_value")]
        Percent60OfValue,
        [EnumMember(Value = "deciseconds_to_seconds")]
        DecisecondsToSeconds,
        [EnumMember(Value = "divide_by_one_hundred")]
        DivideBy100,
        [EnumMember(Value = "divide_by_one_hundred_and_negate")]
        DivideBy100AndNegate,
        [EnumMember(Value = "divide_by_one_hundred_2dp")]
        DivideBy100Precision2,
        [EnumMember(Value = "divide_by_ten_0dp")]
        DivideBy10Precision0,
        [EnumMember(Value = "divide_by_two_0dp")]
        DivideBy2Precision0,
        [EnumMember(Value = "divide_by_fifteen_0dp")]
        DivideBy15Precision0,
        [EnumMember(Value = "divide_by_twenty_then_double_0dp")]
        DivideBy20ThenDoublePrecision0,
        [EnumMember(Value = "milliseconds_to_seconds")]
        MillisecondsToSeconds,
        [EnumMember(Value = "milliseconds_to_seconds_0dp")]
        MillisecondsToSecondsPrecision0,
        [EnumMember(Value = "milliseconds_to_seconds_2dp")]
        MillisecondsToSecondsPrecision2,
        [EnumMember(Value = "multiplicative_damage_modifier")]
        MultiplicativeDamageModifier,
        [EnumMember(Value = "multiplicative_permyriad_damage_modifier")]
        MultiplicativePermyriadDamageModifier,
        [EnumMember(Value = "negate")]
        Negate,
        [EnumMember(Value = "old_leech_percent")]
        OldLeechPercent,
        [EnumMember(Value = "old_leech_permyriad")]
        OldLeechPermyriad,
        [EnumMember(Value = "per_minute_to_per_second")]
        PerMinuteToPerSecondPrecision1,
        [EnumMember(Value = "per_minute_to_per_second_0dp")]
        PerMinuteToPerSecondPrecision0,
        [EnumMember(Value = "per_minute_to_per_second_2dp")]
        PerMinuteToPerSecondPrecision2,
        [EnumMember(Value = "per_minute_to_per_second_2dp_if_required")]
        PerMinuteToPerSecondPrecision2IfRequired,
        [EnumMember(Value = "mod_value_to_item_class")]
        ModValueToItemClass,
        [EnumMember(Value = "tempest_mod_text")]
        TempestModText,
    }

    public static class IndexHandlerExtensions
    {
        private static readonly IReadOnlyDictionary<IndexHandler, Func<double, double>> Handlers
            = new Dictionary<IndexHandler, Func<double, double>>
            {
                { IndexHandler.Percent60OfValue, d => d * 0.6 },
                { IndexHandler.DecisecondsToSeconds, d => d / 10 },
                { IndexHandler.DivideBy100, d => d / 100 },
                { IndexHandler.DivideBy100AndNegate, d => -d / 100 },
                { IndexHandler.DivideBy100Precision2, d => Math.Round(d / 100, 2) },
                { IndexHandler.DivideBy10Precision0, d => Math.Round(d / 10, 0) },
                { IndexHandler.DivideBy2Precision0, d => Math.Round(d / 2, 0) },
                { IndexHandler.DivideBy15Precision0, d => Math.Round(d / 15, 0) },
                { IndexHandler.DivideBy20ThenDoublePrecision0, d => Math.Round(d / 20, 0) * 2 },
                { IndexHandler.MillisecondsToSeconds, d => d / 1000 },
                { IndexHandler.MillisecondsToSecondsPrecision0, d => Math.Round(d / 1000, 0) },
                { IndexHandler.MillisecondsToSecondsPrecision2, d => Math.Round(d / 1000, 2) },
                { IndexHandler.MultiplicativeDamageModifier, d => d + 100 },
                { IndexHandler.MultiplicativePermyriadDamageModifier, d => d / 100 + 100 },
                { IndexHandler.Negate, d => -d },
                { IndexHandler.OldLeechPercent, d => d / 5 },
                { IndexHandler.OldLeechPermyriad, d => d / 500 },
                { IndexHandler.PerMinuteToPerSecondPrecision0, d => Math.Round(d / 60, 0) },
                { IndexHandler.PerMinuteToPerSecondPrecision1, d => Math.Round(d / 60, 1) },
                { IndexHandler.PerMinuteToPerSecondPrecision2, d => Math.Round(d / 60, 2) },
                {
                    IndexHandler.PerMinuteToPerSecondPrecision2IfRequired,
                    // Handler says it shouldn't be rounded if it doesn't need to be, but in that
                    // case rounding doesn't do anything anyway.
                    d => Math.Round(d / 60, 2)
                },
                // this appears on a unique map, we don't support map crafting
                { IndexHandler.ModValueToItemClass, d => { throw new NotSupportedException(); } },
                // not sure where this appears, at least not on anything we need to support
                { IndexHandler.TempestModText, d => { throw new NotSupportedException(); } },
            };

        /// <summary>
        /// Applies this handler to the given value and returns the handled value.
        /// </summary>
        public static double Apply(this IndexHandler indexHandler, double value)
            => Handlers[indexHandler](value);
    }
}