using System;
using System.Collections.Generic;
using System.Linq;
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

            var results = new List<ParseResult>(1 + item.Modifiers.Count)
                { ParseBuffStats(itemTags, localSource, baseItemDefinition.BuffStats) };
            foreach (var modifier in item.Modifiers)
            {
                if (ModifierLocalityTester.AffectsProperties(modifier, itemTags))
                    results.Add(ParsePropertyModifier(localSource, modifier));
                else if (ModifierLocalityTester.IsLocal(modifier, itemTags))
                    results.Add(ParseLocalModifier(itemTags, localSource, modifier));
                else
                    results.Add(ParseGlobalModifier(itemTags, globalSource, modifier));
            }
            return ParseResult.Aggregate(results);
        }

        private ParseResult ParseBuffStats(
            Tags itemTags, ModifierSource.Local source, IReadOnlyList<UntranslatedStat> buffStats)
        {
            if (buffStats.IsEmpty())
                return ParseResult.Empty;
            if (!itemTags.HasFlag(Tags.Flask))
                throw new NotSupportedException("Buff stats are only supported for flasks");

            var result = _untranslatedStatParser.Parse(source, Entity.Character, buffStats);
            return MultiplyValuesByFlaskEffect(result);
        }

        private ParseResult ParsePropertyModifier(ModifierSource.Local source, string modifier)
            => Parse(modifier + " (AsItemProperty)", source);

        private ParseResult ParseLocalModifier(Tags itemTags, ModifierSource.Local source, string modifier)
        {
            if (itemTags.HasFlag(Tags.Weapon))
                modifier = "Attacks with this Weapon have " + modifier;
            return Parse(modifier, source);
        }

        private ParseResult ParseGlobalModifier(Tags itemTags, ModifierSource.Global source, string modifier)
        {
            var result = Parse(modifier, source);
            if (itemTags.HasFlag(Tags.Flask))
                result = MultiplyValuesByFlaskEffect(result);
            return result;
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