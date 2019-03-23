using System.Collections.Generic;
using PoESkillTree.Computation.Common;
using PoESkillTree.Computation.Common.Builders;
using PoESkillTree.GameModel;
using PoESkillTree.GameModel.Skills;

namespace PoESkillTree.Computation.Parsing.SkillParsers
{
    /// <summary>
    /// Parser for active skills
    /// </summary>
    public class ActiveSkillParser : IParser<Skill>
    {
        private readonly SkillDefinitions _skillDefinitions;
        private readonly IBuilderFactories _builderFactories;
        private readonly UntranslatedStatParserFactory _statParserFactory;

        public ActiveSkillParser(
            SkillDefinitions skillDefinitions, IBuilderFactories builderFactories,
            UntranslatedStatParserFactory statParserFactory)
            => (_skillDefinitions, _builderFactories, _statParserFactory) =
                (skillDefinitions, builderFactories, statParserFactory);

        public ParseResult Parse(Skill skill)
        {
            if (!skill.IsEnabled)
                return ParseResult.Empty;

            var modifiers = new List<Modifier>();
            var parsedStats = new List<UntranslatedStat>();

            var preParser = new SkillPreParser(_skillDefinitions, _builderFactories.MetaStatBuilders);
            var preParseResult = preParser.ParseActive(skill);

            foreach (var partialParser in CreatePartialParsers())
            {
                var (newlyParsedModifiers, newlyParsedStats) = partialParser.Parse(skill, skill, preParseResult);
                modifiers.AddRange(newlyParsedModifiers);
                parsedStats.AddRange(newlyParsedStats);
            }

            var translatingParser = new TranslatingSkillParser(_builderFactories, _statParserFactory);
            return translatingParser.Parse(skill, preParseResult, new PartialSkillParseResult(modifiers, parsedStats));
        }

        private IPartialSkillParser[] CreatePartialParsers()
            => new[]
            {
                new ActiveSkillGeneralParser(_builderFactories),
                SkillKeywordParser.CreateActive(_builderFactories),
                SkillTypeParser.CreateActive(_builderFactories),
                new ActiveSkillLevelParser(_builderFactories),
                new GemRequirementParser(_builderFactories),
                new SkillStatParser(_builderFactories),
            };
    }
}