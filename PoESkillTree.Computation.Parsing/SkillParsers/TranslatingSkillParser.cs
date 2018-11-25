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

        private SkillPreParseResult _preParseResult;
        private IEnumerable<UntranslatedStat> _parsedStats;

        public TranslatingSkillParser(IBuilderFactories builderFactories, StatParserFactory statParserFactory)
            => (_builderFactories, _statParserFactory) = (builderFactories, statParserFactory);

        public ParseResult Parse(
            Skill skill, SkillPreParseResult preParseResult, PartialSkillParseResult partialResult)
        {
            _preParseResult = preParseResult;
            _parsedStats = partialResult.ParsedStats.ToHashSet();

            var isMainSkill = preParseResult.IsMainSkill.IsSet;
            var level = preParseResult.LevelDefinition;
            var qualityStats = level.QualityStats.Select(s => ApplyQuality(s, skill));
            var parseResults = new List<ParseResult>
            {
                ParseResult.Success(partialResult.ParsedModifiers.ToList()),
                TranslateAndParse(qualityStats, isMainSkill),
                TranslateAndParse(level.Stats, isMainSkill)
            };

            foreach (var (partIndex, stats) in level.AdditionalStatsPerPart.Index())
            {
                var condition = isMainSkill.And(_builderFactories.StatBuilders.MainSkillPart.Value.Eq(partIndex));
                var result = TranslateAndParse(stats, condition);
                parseResults.Add(result);
            }

            var qualityBuffStats =
                level.QualityBuffStats.Select(s => new BuffStat(ApplyQuality(s.Stat, skill), s.AffectedEntities));
            parseResults.Add(TranslateAndParseBuff(qualityBuffStats));
            parseResults.Add(TranslateAndParseBuff(level.BuffStats));

            var qualityPassiveStats = level.QualityPassiveStats.Select(s => ApplyQuality(s, skill));
            parseResults.Add(TranslateAndParse(qualityPassiveStats, _preParseResult.IsActiveSkill));
            parseResults.Add(TranslateAndParse(level.PassiveStats, _preParseResult.IsActiveSkill));

            _preParseResult = null;
            _parsedStats = null;
            return ParseResult.Aggregate(parseResults);
        }

        private ParseResult TranslateAndParse(IEnumerable<UntranslatedStat> stats, IConditionBuilder condition)
        {
            var result = TranslateAndParse(_preParseResult.SkillDefinition.StatTranslationFile,
                _preParseResult.LocalSource, stats);
            return ApplyCondition(result, condition);
        }

        private ParseResult TranslateAndParseBuff(IEnumerable<BuffStat> buffStats)
        {
            var results = new List<ParseResult>();
            var buffBuilder = _builderFactories.SkillBuilders.FromId(_preParseResult.SkillDefinition.Id).Buff;
            foreach (var (stat, affectedEntities) in buffStats)
            {
                foreach (var affectedEntity in affectedEntities)
                {
                    var result = Parse(StatTranslationLoader.MainFileName, stat, affectedEntity);
                    if (result.SuccessfullyParsed && result.Modifiers.IsEmpty())
                        result = Parse(StatTranslationLoader.SkillFileName, stat, affectedEntity);
                    result = ApplyCondition(result, _preParseResult.IsActiveSkill);

                    var buildParameters = new BuildParameters(_preParseResult.GlobalSource, affectedEntity, default);
                    var multiplier = buffBuilder.BuildAddStatMultiplier(buildParameters, new[] { Entity.Character });
                    result = ApplyMultiplier(result, multiplier);
                    results.Add(result);
                }
            }

            return ParseResult.Aggregate(results);

            ParseResult Parse(string statTranslationFileName, UntranslatedStat stat, Entity affectedEntity)
                => TranslateAndParse(statTranslationFileName, _preParseResult.LocalSource, new[] { stat },
                    affectedEntity);
        }

        private ParseResult TranslateAndParse(
            string statTranslationFileName,
            ModifierSource.Local.Skill localModifierSource,
            IEnumerable<UntranslatedStat> stats,
            Entity modifierSourceEntity = Entity.Character)
        {
            var unparsedStats = stats.Except(_parsedStats);
            var statParser = _statParserFactory(statTranslationFileName);
            var parserParameter =
                new UntranslatedStatParserParameter(localModifierSource, modifierSourceEntity, unparsedStats);
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

        private static UntranslatedStat ApplyQuality(UntranslatedStat qualityStat, Skill skill)
            => new UntranslatedStat(qualityStat.StatId, qualityStat.Value * skill.Quality / 1000);
    }
}