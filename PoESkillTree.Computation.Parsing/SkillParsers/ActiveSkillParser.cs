using System.Collections.Generic;
using PoESkillTree.Computation.Common;
using PoESkillTree.Computation.Common.Builders;
using PoESkillTree.Computation.Common.Builders.Stats;
using PoESkillTree.GameModel.Skills;

namespace PoESkillTree.Computation.Parsing.SkillParsers
{
    public class ActiveSkillParser : IParser<Skill>
    {
        private readonly SkillPreParser _preParser;
        private readonly IReadOnlyList<IPartialSkillParser> _partialParsers;
        private readonly TranslatingSkillParser _translatingParser;

        public ActiveSkillParser(
            SkillDefinitions skillDefinitions, IBuilderFactories builderFactories, IMetaStatBuilders metaStatBuilders,
            UntranslatedStatParserFactory statParserFactory)
        {
            _preParser = new SkillPreParser(skillDefinitions, metaStatBuilders);
            _partialParsers = new[]
            {
                new ActiveSkillGeneralParser(builderFactories, metaStatBuilders),
                SkillKeywordParser.CreateActive(builderFactories, metaStatBuilders),
                SkillTypeParser.CreateActive(builderFactories, metaStatBuilders),
                new ActiveSkillLevelParser(builderFactories, metaStatBuilders),
                new GemRequirementParser(builderFactories),
                new SkillStatParser(builderFactories, metaStatBuilders),
            };
            _translatingParser = new TranslatingSkillParser(builderFactories, statParserFactory);
        }

        public ParseResult Parse(Skill skill)
        {
            var modifiers = new List<Modifier>();
            var parsedStats = new List<UntranslatedStat>();

            var preParseResult = _preParser.ParseActive(skill);
            foreach (var partialParser in _partialParsers)
            {
                var (newlyParsedModifiers, newlyParsedStats) = partialParser.Parse(skill, skill, preParseResult);
                modifiers.AddRange(newlyParsedModifiers);
                parsedStats.AddRange(newlyParsedStats);
            }

            return _translatingParser.Parse(skill, preParseResult, new PartialSkillParseResult(modifiers, parsedStats));
        }
    }
}