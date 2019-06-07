using System.Collections.Generic;
using System.Text.RegularExpressions;
using JetBrains.Annotations;

namespace PoESkillTree.Computation.Parsing.JewelParsers
{
    public abstract class TransformationJewelParserData
    {
        public abstract Regex JewelModifierRegex { get; }

        public virtual bool CancelOutOriginalModifier
            => true;

        public virtual double GetValueMultiplier(Match jewelModifierMatch)
            => 1;

        public abstract IEnumerable<(Regex regex, string replacement)> GetNodeModifierRegexes(
            Match jewelModifierMatch);

        public static IEnumerable<TransformationJewelParserData> CreateAll()
        {
            yield return new SingleDamageTypeTransformation();
            yield return new DamageTypesToFireTransformation();
            yield return new GenericTransformation();
            yield return new LioneyesFallTransformation();
            yield return new DreamTransformation();
            yield return new NightmareTransformation();
        }

        private static Regex CreateRegex([RegexPattern] string pattern)
            => new Regex(pattern, RegexOptions.CultureInvariant | RegexOptions.IgnoreCase);

        // Cold Steel
        public class SingleDamageTypeTransformation : TransformationJewelParserData
        {
            public override Regex JewelModifierRegex { get; } = CreateRegex(
                @"^increases and reductions to (?<source>\w+) damage in radius are transformed to apply to (?<target>\w+) damage$");

            public override IEnumerable<(Regex regex, string replacement)> GetNodeModifierRegexes(
                Match jewelModifierMatch)
            {
                yield return (
                    CreateRegex($"((increased|reduced).*) {jewelModifierMatch.Groups["source"]} (.*damage)"),
                    $"$1 {jewelModifierMatch.Groups["target"]} $3");
            }
        }

        // Fireborn
        private class DamageTypesToFireTransformation : TransformationJewelParserData
        {
            public override Regex JewelModifierRegex { get; } = CreateRegex(
                @"^increases and reductions to other damage types in radius are transformed to apply to fire damage$");

            public override IEnumerable<(Regex regex, string replacement)> GetNodeModifierRegexes(
                Match jewelModifierMatch)
            {
                yield return (
                    CreateRegex("((increased|reduced).*) (physical|cold|lightning|chaos) (.*damage)"),
                    "$1 fire $3");
            }
        }

        // Energised Armour, Healthy Mind, Energy from Within
        public class GenericTransformation : TransformationJewelParserData
        {
            public override Regex JewelModifierRegex { get; } = CreateRegex(
                @"^increases and reductions to (?<source>.*) in radius are transformed to apply to (?<target>.*?)( at (?<percentage>\d+)% of their value)?$");

            public override double GetValueMultiplier(Match jewelModifierMatch)
            {
                var percentageGroup = jewelModifierMatch.Groups["percentage"];
                return percentageGroup.Success
                    ? double.Parse(percentageGroup.Value) / 100
                    : 1;
            }

            public override IEnumerable<(Regex regex, string replacement)> GetNodeModifierRegexes(
                Match jewelModifierMatch)
            {
                yield return (
                    CreateRegex($"((increased|reduced).*) {jewelModifierMatch.Groups["source"]}"),
                    $"$1 {jewelModifierMatch.Groups["target"]}");
            }
        }

        // Lioneye's Fall
        public class LioneyesFallTransformation : TransformationJewelParserData
        {
            public override Regex JewelModifierRegex { get; } = CreateRegex(
                @"^melee and melee weapon type modifiers in radius are transformed to bow modifiers$");

            public override IEnumerable<(Regex regex, string replacement)> GetNodeModifierRegexes(
                Match jewelModifierMatch)
            {
                yield return (
                    CreateRegex("with (two handed melee weapons|one handed melee weapons|melee weapons|axes|claws|daggers|maces|staves|swords)"),
                    "with bows");
                yield return (
                    CreateRegex("melee"),
                    "bow");
            }
        }

        // The (Blue|Green|Red) Dream
        public class DreamTransformation : TransformationJewelParserData
        {
            public override Regex JewelModifierRegex { get; } = CreateRegex(
                @"^passives granting (?<source>\w+) resistance or all elemental resistances in radius also grant an equal (?<target>.*)$");

            public override bool CancelOutOriginalModifier
                => false;

            public override IEnumerable<(Regex regex, string replacement)> GetNodeModifierRegexes(
                Match jewelModifierMatch)
            {
                yield return (
                    CreateRegex($"to ({jewelModifierMatch.Groups["source"]}|all elemental) resistances?"),
                    $"{jewelModifierMatch.Groups["target"]}");
            }
        }
        
        // The (Blue|Green|Red) Nightmare
        private class NightmareTransformation : DreamTransformation
        {
            public override Regex JewelModifierRegex { get; } = CreateRegex(
                @"^passives granting (?<source>\w+) resistance or all elemental resistances in radius also grant (?<target>.*) at (?<percentage>\d+)% of its value$");
            
            public override double GetValueMultiplier(Match jewelModifierMatch)
                => double.Parse(jewelModifierMatch.Groups["percentage"].Value) / 100;
        }
    }
}