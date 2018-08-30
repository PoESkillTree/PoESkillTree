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
        private readonly IReadOnlyList<IPartialSkillParser> _partialParsers;

        public ActiveSkillParser(
            SkillDefinitions skillDefinitions, IBuilderFactories builderFactories, IMetaStatBuilders metaStatBuilders,
            StatParserFactory statParserFactory)
        {
            _statParserFactory = statParserFactory;
            _preParser = new ActiveSkillPreParser(skillDefinitions, metaStatBuilders);
            _partialParsers = new IPartialSkillParser[]
            {
                new ActiveSkillGeneralParser(builderFactories, metaStatBuilders),
                new ActiveSkillKeywordParser(builderFactories, metaStatBuilders),
                new ActiveSkillLevelParser(builderFactories, metaStatBuilders),
                new GemRequirementParser(builderFactories),
                new ActiveSkillStatParser(builderFactories, metaStatBuilders),
            };
        }

        public ParseResult Parse(Skill skill)
        {
            var modifiers = new List<Modifier>();
            var parsedStats = new List<UntranslatedStat>();

            var (preParseResult, preParsedStats) = _preParser.Parse(skill);
            parsedStats.AddRange(preParsedStats);
            foreach (var partialParser in _partialParsers)
            {
                var (newlyParsedModifiers, newlyParsedStats) = partialParser.Parse(skill, preParseResult);
                modifiers.AddRange(newlyParsedModifiers);
                parsedStats.AddRange(newlyParsedStats);
            }

            var level = preParseResult.LevelDefinition;
            var qualityStats =
                level.QualityStats.Select(s => new UntranslatedStat(s.StatId, s.Value * skill.Quality / 1000));
            var levelStats = level.Stats.Except(parsedStats);
            return ParseResult.Aggregate(new[]
            {
                ParseResult.Success(modifiers),
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