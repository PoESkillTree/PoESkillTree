using System.Collections.Generic;
using PoESkillTree.Computation.Common;
using PoESkillTree.Computation.Common.Builders;
using PoESkillTree.GameModel;
using PoESkillTree.GameModel.Skills;

namespace PoESkillTree.Computation.Parsing.SkillParsers
{
    /// <summary>
    /// Parser for support skills
    /// </summary>
    public class SupportSkillParser : IParser<SupportSkillParserParameter>
    {
        private readonly SkillPreParser _preParser;
        private readonly IReadOnlyList<IPartialSkillParser> _partialParsers;
        private readonly TranslatingSkillParser _translatingParser;

        public SupportSkillParser(
            SkillDefinitions skillDefinitions, IBuilderFactories builderFactories,
            UntranslatedStatParserFactory statParserFactory)
        {
            _preParser = new SkillPreParser(skillDefinitions, builderFactories.MetaStatBuilders);
            _partialParsers = new[]
            {
                new SupportSkillGeneralParser(builderFactories),
                SkillKeywordParser.CreateSupport(builderFactories),
                SkillTypeParser.CreateSupport(builderFactories),
                new SupportSkillLevelParser(builderFactories), 
                new GemRequirementParser(builderFactories),
                new SkillStatParser(builderFactories),
            };
            _translatingParser = new TranslatingSkillParser(builderFactories, statParserFactory);
        }

        public ParseResult Parse(SupportSkillParserParameter parameter)
        {
            var (active, support) = parameter;
            var modifiers = new List<Modifier>();
            var parsedStats = new List<UntranslatedStat>();

            var preParseResult = _preParser.ParseSupport(active, support);

            foreach (var partialParser in _partialParsers)
            {
                var (newlyParsedModifiers, newlyParsedStats) = partialParser.Parse(active, support, preParseResult);
                modifiers.AddRange(newlyParsedModifiers);
                parsedStats.AddRange(newlyParsedStats);
            }

            return _translatingParser.Parse(support, preParseResult,
                new PartialSkillParseResult(modifiers, parsedStats));
        }
    }

    public static class SupportSkillParserExtensions
    {
        public static ParseResult Parse(
            this IParser<SupportSkillParserParameter> @this, Skill activeSkill, Skill supportSkill)
            => @this.Parse(new SupportSkillParserParameter(activeSkill, supportSkill));
    }

    public class SupportSkillParserParameter
    {
        public SupportSkillParserParameter(Skill activeSkill, Skill supportSkill)
            => (ActiveSkill, SupportSkill) = (activeSkill, supportSkill);

        public void Deconstruct(out Skill activeSkill, out Skill supportSkill)
            => (activeSkill, supportSkill) = (ActiveSkill, SupportSkill);

        public Skill ActiveSkill { get; }
        public Skill SupportSkill { get; }
    }
}