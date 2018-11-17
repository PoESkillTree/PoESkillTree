using System;
using System.Collections.Generic;
using System.Linq;
using MoreLinq;
using PoESkillTree.Computation.Common;
using PoESkillTree.Computation.Common.Builders;
using PoESkillTree.Computation.Common.Builders.Conditions;
using PoESkillTree.GameModel;
using PoESkillTree.GameModel.Skills;
using PoESkillTree.GameModel.StatTranslation;
using PoESkillTree.Utils.Extensions;

namespace PoESkillTree.Computation.Parsing.SkillParsers
{
    public class TranslatingSkillParser
    {
        public delegate IParser<UntranslatedStatParserParameter> StatParserFactory(string statTranslationFileName);

        private readonly IBuilderFactories _builderFactories;
        private readonly StatParserFactory _statParserFactory;

        public TranslatingSkillParser(IBuilderFactories builderFactories, StatParserFactory statParserFactory)
            => (_builderFactories, _statParserFactory) = (builderFactories, statParserFactory);

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
                var condition = isMainSkill.And(_builderFactories.StatBuilders.MainSkillPart.Value.Eq(partIndex));
                var result = TranslateAndParse(preParseResult, stats, condition);
                parseResults.Add(result);
            }

            parseResults.Add(TranslateAndParseBuff(preParseResult, level.QualityBuffStats));
            parseResults.Add(TranslateAndParseBuff(preParseResult, level.BuffStats));

            return ParseResult.Aggregate(parseResults);
        }

        private ParseResult TranslateAndParse(
            SkillPreParseResult preParseResult, IEnumerable<UntranslatedStat> stats, IConditionBuilder condition)
        {
            var result = TranslateAndParse(preParseResult.SkillDefinition.StatTranslationFile,
                preParseResult.LocalSource, stats);
            return ApplyCondition(result, condition);
        }

        private ParseResult TranslateAndParseBuff(SkillPreParseResult preParseResult, IReadOnlyList<BuffStat> buffStats)
        {
            if (buffStats.IsEmpty())
                return ParseResult.Success(new Modifier[0]);

            var results = new List<ParseResult>();
            var buffBuilder = _builderFactories.SkillBuilders.FromId(preParseResult.SkillDefinition.Id).Buff;
            foreach (var (stat, affectedEntities) in buffStats)
            {
                foreach (var affectedEntity in affectedEntities)
                {
                    var result = Parse(StatTranslationLoader.MainFileName, stat, affectedEntity);
                    if (result.SuccessfullyParsed && result.Modifiers.IsEmpty())
                        result = Parse(StatTranslationLoader.SkillFileName, stat, affectedEntity);
                    result = ApplyCondition(result, preParseResult.IsActiveSkill);
                    var multiplier = buffBuilder.BuildAddStatMultiplier(new[] { Entity.Character }, affectedEntity);
                    result = ApplyMultiplier(result, multiplier);
                    results.Add(result);
                }
            }

            return ParseResult.Aggregate(results);

            ParseResult Parse(string statTranslationFileName, UntranslatedStat stat, Entity affectedEntity)
                => TranslateAndParse(statTranslationFileName, preParseResult.LocalSource, new[] { stat },
                    affectedEntity);
        }

        private ParseResult TranslateAndParse(
            string statTranslationFileName,
            ModifierSource.Local.Skill localModifierSource,
            IEnumerable<UntranslatedStat> stats,
            Entity modifierSourceEntity = Entity.Character)
        {
            var statParser = _statParserFactory(statTranslationFileName);
            var parserParameter = new UntranslatedStatParserParameter(localModifierSource, modifierSourceEntity, stats);
            return statParser.Parse(parserParameter);
        }

        private static ParseResult ApplyCondition(
            ParseResult result, IConditionBuilder condition, Entity modifierSourceEntity = Entity.Character)
        {
            var conditionalValue = BuildCondition(condition, modifierSourceEntity);
            return result.ApplyToModifiers(m => new Modifier(m.Stats, m.Form, ApplyCondition(m.Value), m.Source));

            IValue ApplyCondition(IValue value)
                => new FunctionalValue(c => conditionalValue.Calculate(c).IsTrue() ? value.Calculate(c) : null,
                    $"{conditionalValue}.IsTrue ? {value} : null");
        }

        private static IValue BuildCondition(IConditionBuilder condition, Entity modifierSourceEntity)
        {
            var conditionResult = condition.Build(new BuildParameters(null, modifierSourceEntity, default));
            if (conditionResult.HasStatConverter)
                throw new InvalidOperationException("Can only handle value conditions");
            return conditionResult.Value;
        }

        private static ParseResult ApplyMultiplier(ParseResult result, IValue multiplier)
        {
            return result.ApplyToModifiers(m => new Modifier(m.Stats, m.Form, ApplyMultiplier(m.Value), m.Source));

            IValue ApplyMultiplier(IValue value)
                => new FunctionalValue(c => value.Calculate(c) * multiplier.Calculate(c), $"{value} * {multiplier}");
        }
    }
}