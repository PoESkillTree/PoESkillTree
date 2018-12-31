using System;
using System.Collections.Generic;
using System.Linq;
using MoreLinq;
using PoESkillTree.Computation.Common;
using PoESkillTree.Computation.Common.Builders;
using PoESkillTree.GameModel;
using PoESkillTree.GameModel.Items;
using PoESkillTree.GameModel.Modifiers;
using PoESkillTree.GameModel.StatTranslation;
using PoESkillTree.Utils.Extensions;

namespace PoESkillTree.Computation.Parsing.ItemParsers
{
    /// <summary>
    /// Partial parser of <see cref="ItemParser"/> that parses <see cref="BaseItemDefinition.BuffStats"/>
    /// and <see cref="Item.Modifiers"/>
    /// </summary>
    public class ItemModifierParser : IParser<PartialItemParserParameter>
    {
        private readonly IBuilderFactories _builderFactories;
        private readonly ICoreParser _coreParser;
        private readonly UntranslatedStatParser _untranslatedStatParser;

        public ItemModifierParser(
            IBuilderFactories builderFactories, ICoreParser coreParser, IStatTranslator statTranslator)
        {
            (_builderFactories, _coreParser) = (builderFactories, coreParser);
            _untranslatedStatParser = new UntranslatedStatParser(statTranslator, _coreParser);
        }

        public ParseResult Parse(PartialItemParserParameter parameter)
        {
            var (item, _, baseItemDefinition, localSource, globalSource) = parameter;
            var itemTags = baseItemDefinition.Tags;

            var (propertyMods, remainingMods) =
                item.Modifiers.Partition(s => ModifierLocalityTester.AffectsProperties(s, itemTags));
            var (localMods, globalMods) =
                remainingMods.Partition(s => ModifierLocalityTester.IsLocal(s, itemTags));

            var parseResults = ParseBuffStats(itemTags, localSource, baseItemDefinition.BuffStats)
                .Concat(ParsePropertyModifiers(itemTags, localSource, propertyMods))
                .Concat(ParseLocalModifiers(itemTags, localSource, localMods))
                .Concat(ParseGlobalModifiers(itemTags, globalSource, globalMods));
            return ParseResult.Aggregate(parseResults);
        }

        private ParseResult ParseBuffStats(
            Tags itemTags, ModifierSource.Local source, IReadOnlyList<UntranslatedStat> buffStats)
        {
            if (buffStats.IsEmpty())
                return ParseResult.Success(new Modifier[0]);
            if (!itemTags.HasFlag(Tags.Flask))
                throw new NotSupportedException("Buff stats are only supported for flasks");

            var result = _untranslatedStatParser.Parse(source, Entity.Character, buffStats);
            return MultiplyValuesByFlaskEffect(result);
        }

        private IEnumerable<ParseResult> ParsePropertyModifiers(
            Tags itemTags, ModifierSource.Local source, IEnumerable<string> propertyMods)
        {
            propertyMods = propertyMods.Select(s => s + " (AsItemProperty)");
            if (itemTags.HasFlag(Tags.Weapon))
            {
                propertyMods = propertyMods.Select(s => "Attacks with this Weapon have " + s);
            }
            return propertyMods.Select(s => Parse(s, source));
        }

        private IEnumerable<ParseResult> ParseLocalModifiers(
            Tags itemTags, ModifierSource.Local source, IEnumerable<string> localMods)
        {
            if (itemTags.HasFlag(Tags.Weapon))
            {
                localMods = localMods.Select(s => "Attacks with this Weapon have " + s);
            }
            return localMods.Select(s => Parse(s, source));
        }

        private IEnumerable<ParseResult> ParseGlobalModifiers(
            Tags itemTags, ModifierSource.Global source, IEnumerable<string> globalMods)
        {
            var results = globalMods.Select(s => Parse(s, source));
            if (itemTags.HasFlag(Tags.Flask))
            {
                results = results.Select(MultiplyValuesByFlaskEffect);
            }
            return results;
        }

        private ParseResult MultiplyValuesByFlaskEffect(ParseResult result)
        {
            var multiplierBuilder = _builderFactories.StatBuilders.Flask.Effect.Value;
            return result.ApplyMultiplier(multiplierBuilder.Build);
        }

        private ParseResult Parse(string modifierLine, ModifierSource modifierSource)
            => _coreParser.Parse(modifierLine, modifierSource, Entity.Character);
    }
}