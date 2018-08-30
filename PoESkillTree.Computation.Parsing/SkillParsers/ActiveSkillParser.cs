using System.Collections.Generic;
using System.Linq;
using PoESkillTree.Computation.Common;
using PoESkillTree.Computation.Common.Builders;
using PoESkillTree.Computation.Common.Builders.Stats;
using PoESkillTree.GameModel.Items;
using PoESkillTree.GameModel.Skills;

namespace PoESkillTree.Computation.Parsing.SkillParsers
{
    public class ActiveSkillParser : IParser<Skill>
    {
        public delegate IParser<UntranslatedStatParserParameter> StatParserFactory(string statTranslationFileName);

        private readonly StatParserFactory _statParserFactory;

        private readonly ActiveSkillPreParser _preParser;
        private readonly ActiveSkillGeneralParser _generalParser;
        private readonly ActiveSkillKeywordParser _keywordParser;
        private readonly ActiveSkillLevelParser _levelParser;
        private readonly GemRequirementParser _requirementParser;
        private readonly ActiveSkillStatParser _statParser;

        public ActiveSkillParser(
            SkillDefinitions skillDefinitions, IBuilderFactories builderFactories, IMetaStatBuilders metaStatBuilders,
            StatParserFactory statParserFactory)
        {
            _statParserFactory = statParserFactory;
            _preParser = new ActiveSkillPreParser(skillDefinitions, metaStatBuilders);
            _generalParser = new ActiveSkillGeneralParser(builderFactories, metaStatBuilders);
            _keywordParser = new ActiveSkillKeywordParser(builderFactories, metaStatBuilders);
            _levelParser = new ActiveSkillLevelParser(builderFactories, metaStatBuilders);
            _requirementParser = new GemRequirementParser(builderFactories);
            _statParser = new ActiveSkillStatParser(builderFactories, metaStatBuilders);
        }

        public ParseResult Parse(Skill parameter)
        {
            var modifiers = new List<Modifier>();
            var (preParseResult, parsedStats) = _preParser.Parse(parameter);

            var level = preParseResult.LevelDefinition;

            var (newlyParsedModifiers, newlyParsedStats) = _generalParser.Parse(preParseResult);
            modifiers.AddRange(newlyParsedModifiers);
            parsedStats = parsedStats.Concat(newlyParsedStats);

            (newlyParsedModifiers, newlyParsedStats) = _keywordParser.Parse(preParseResult);
            modifiers.AddRange(newlyParsedModifiers);
            parsedStats = parsedStats.Concat(newlyParsedStats);

            (newlyParsedModifiers, newlyParsedStats) = _levelParser.Parse(preParseResult);
            modifiers.AddRange(newlyParsedModifiers);
            parsedStats = parsedStats.Concat(newlyParsedStats);

            (newlyParsedModifiers, newlyParsedStats) = _requirementParser.Parse(parameter, preParseResult);
            modifiers.AddRange(newlyParsedModifiers);
            parsedStats = parsedStats.Concat(newlyParsedStats);

            (newlyParsedModifiers, newlyParsedStats) = _statParser.Parse(preParseResult);
            modifiers.AddRange(newlyParsedModifiers);
            parsedStats = parsedStats.Concat(newlyParsedStats);

            var isMainSkillValue = preParseResult.IsMainSkill.Value
                .Build(new BuildParameters(null, Entity.Character, default));
            var statParser = _statParserFactory(preParseResult.SkillDefinition.StatTranslationFile);

            ParseResult Parse(IEnumerable<UntranslatedStat> stats)
            {
                var parserParameter = new UntranslatedStatParserParameter(preParseResult.LocalSource, stats);
                return ApplyCondition(statParser.Parse(parserParameter), isMainSkillValue);
            }
            
            var qualityStats =
                level.QualityStats.Select(s => new UntranslatedStat(s.StatId, s.Value * parameter.Quality / 1000));
            var levelStats = level.Stats.Except(parsedStats);
            var parseResults = new[] { ParseResult.Success(modifiers), Parse(qualityStats), Parse(levelStats) };
            return ParseResult.Aggregate(parseResults);
        }

        private ParseResult ApplyCondition(ParseResult result, IValue conditionalValue)
        {
            return result
                .ApplyToModifiers(m => new Modifier(m.Stats, m.Form, ApplyCondition(m.Value), m.Source));

            IValue ApplyCondition(IValue value)
                => new FunctionalValue(c => conditionalValue.Calculate(c).IsTrue() ? value.Calculate(c) : null,
                    $"{conditionalValue}.IsTrue ? {value} : null");
        }
    }

    public struct Skill
    {
        public Skill(string id, int level, int quality, ItemSlot itemSlot, int socketIndex, int? gemGroup)
            => (Id, Level, Quality, ItemSlot, SocketIndex, GemGroup) =
                (id, level, quality, itemSlot, socketIndex, gemGroup);

        public string Id { get; }
        public int Level { get; }
        public int Quality { get; }

        public ItemSlot ItemSlot { get; }
        public int SocketIndex { get; }

        // Null: item inherent skill
        public int? GemGroup { get; }
    }
}