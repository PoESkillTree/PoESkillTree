using System;
using System.Collections.Generic;
using System.Linq;
using MoreLinq;
using PoESkillTree.Computation.Common;
using PoESkillTree.Computation.Common.Builders;
using PoESkillTree.Computation.Common.Builders.Conditions;
using PoESkillTree.Computation.Common.Builders.Stats;
using PoESkillTree.GameModel;
using PoESkillTree.GameModel.Skills;
using PoESkillTree.Utils.Extensions;

namespace PoESkillTree.Computation.Parsing.SkillParsers
{
    public class TranslatingSkillParser
    {
        public delegate IParser<UntranslatedStatParserParameter> StatParserFactory(string statTranslationFileName);

        private readonly IMetaStatBuilders _metaStatBuilders;
        private readonly StatParserFactory _statParserFactory;

        public TranslatingSkillParser(IMetaStatBuilders metaStatBuilders, StatParserFactory statParserFactory)
            => (_metaStatBuilders, _statParserFactory) = (metaStatBuilders, statParserFactory);

        public ParseResult Parse(
            Skill skill, SkillPreParseResult preParseResult, PartialSkillParseResult partialResult)
        {
            var isMainSkill = preParseResult.IsMainSkill.IsSet;
            var level = preParseResult.LevelDefinition;
            var qualityStats =
                level.QualityStats.Select(s => new UntranslatedStat(s.StatId, s.Value * skill.Quality / 1000));
            var levelStats = level.Stats.Except(partialResult.ParsedStats);
            var parseResults = new List<ParseResult>
            {
                ParseResult.Success(partialResult.ParsedModifiers.ToList()),
                TranslateAndParse(preParseResult, qualityStats, isMainSkill),
                TranslateAndParse(preParseResult, levelStats, isMainSkill)
            };

            var additionalLevelStats = level.AdditionalStatsPerPart.Select(s => s.Except(partialResult.ParsedStats));
            foreach (var (partIndex, stats) in additionalLevelStats.Index())
            {
                var condition = isMainSkill.And(_metaStatBuilders.MainSkillPart.Value.Eq(partIndex));
                var result = TranslateAndParse(preParseResult, stats, condition);
                parseResults.Add(result);
            }

            return ParseResult.Aggregate(parseResults);
        }

        private ParseResult TranslateAndParse(
            SkillPreParseResult preParseResult, IEnumerable<UntranslatedStat> stats, IConditionBuilder condition)
        {
            var conditionResult = condition.Build(new BuildParameters(null, Entity.Character, default));
            if (conditionResult.HasStatConverter)
                throw new InvalidOperationException("Can only handle value conditions");
            var statParser = _statParserFactory(preParseResult.SkillDefinition.StatTranslationFile);
            var parserParameter = new UntranslatedStatParserParameter(preParseResult.LocalSource, stats);
            return ApplyCondition(statParser.Parse(parserParameter), conditionResult.Value);
        }

        private static ParseResult ApplyCondition(ParseResult result, IValue conditionalValue)
        {
            return result.ApplyToModifiers(m => new Modifier(m.Stats, m.Form, ApplyCondition(m.Value), m.Source));

            IValue ApplyCondition(IValue value)
                => new FunctionalValue(c => conditionalValue.Calculate(c).IsTrue() ? value.Calculate(c) : null,
                    $"{conditionalValue}.IsTrue ? {value} : null");
        }
    }
}