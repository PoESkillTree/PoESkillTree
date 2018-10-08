using System.Collections.Generic;
using System.Linq;
using PoESkillTree.Computation.Common;
using PoESkillTree.Computation.Common.Builders;
using PoESkillTree.GameModel.Skills;

namespace PoESkillTree.Computation.Parsing.SkillParsers
{
    public class TranslatingSkillParser
    {
        public delegate IParser<UntranslatedStatParserParameter> StatParserFactory(string statTranslationFileName);

        private readonly StatParserFactory _statParserFactory;

        public TranslatingSkillParser(StatParserFactory statParserFactory)
            => _statParserFactory = statParserFactory;

        public ParseResult Parse(
            Skill skill, SkillPreParseResult preParseResult, PartialSkillParseResult partialResult)
        {
            var level = preParseResult.LevelDefinition;
            var qualityStats =
                level.QualityStats.Select(s => new UntranslatedStat(s.StatId, s.Value * skill.Quality / 1000));
            var levelStats = level.Stats.Except(partialResult.ParsedStats);
            return ParseResult.Aggregate(new[]
            {
                ParseResult.Success(partialResult.ParsedModifiers.ToList()),
                TranslateAndParse(preParseResult, qualityStats),
                TranslateAndParse(preParseResult, levelStats)
            });
        }

        private ParseResult TranslateAndParse(SkillPreParseResult preParseResult, IEnumerable<UntranslatedStat> stats)
        {
            var isMainSkillValue = preParseResult.IsMainSkill.Value
                .Build(new BuildParameters(null, Entity.Character, default));
            var statParser = _statParserFactory(preParseResult.SkillDefinition.StatTranslationFile);
            var parserParameter = new UntranslatedStatParserParameter(preParseResult.LocalSource, stats);
            return ApplyCondition(statParser.Parse(parserParameter), isMainSkillValue);
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