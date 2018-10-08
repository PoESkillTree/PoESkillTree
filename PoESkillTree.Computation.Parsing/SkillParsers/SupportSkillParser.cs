using System.Collections.Generic;
using PoESkillTree.Computation.Common;
using PoESkillTree.Computation.Common.Builders;
using PoESkillTree.Computation.Common.Builders.Modifiers;
using PoESkillTree.Computation.Common.Builders.Stats;
using PoESkillTree.GameModel.Skills;

namespace PoESkillTree.Computation.Parsing.SkillParsers
{
    public class SupportSkillParser : IParser<SupportSkillParserParameter>
    {
        private readonly SkillPreParser _preParser;
        private readonly IReadOnlyList<IPartialSkillParser> _partialParsers;
        private readonly TranslatingSkillParser _translatingParser;
        private readonly IBuilderFactories _builderFactories;
        private readonly IModifierBuilder _modifierBuilder = new ModifierBuilder();

        public SupportSkillParser(
            SkillDefinitions skillDefinitions, IBuilderFactories builderFactories, IMetaStatBuilders metaStatBuilders,
            TranslatingSkillParser.StatParserFactory statParserFactory)
        {
            _builderFactories = builderFactories;
            _preParser = new SkillPreParser(skillDefinitions, metaStatBuilders);
            _partialParsers = new IPartialSkillParser[]
            {
                new GemRequirementParser(builderFactories),
            };
            _translatingParser = new TranslatingSkillParser(statParserFactory);
        }

        public ParseResult Parse(SupportSkillParserParameter parameter)
        {
            var (active, support) = parameter;
            var modifiers = new List<Modifier>();
            var parsedStats = new List<UntranslatedStat>();

            var preParseResult = _preParser.ParseSupport(active, support);

            if (preParseResult.LevelDefinition.ManaMultiplier is double multiplier)
            {
                var intermediateModifier = _modifierBuilder
                    .WithStat(_builderFactories.StatBuilders.Pool.From(Pool.Mana).Cost)
                    .WithForm(_builderFactories.FormBuilders.From( Form.More))
                    .WithValue(_builderFactories.ValueBuilders.Create(multiplier * 100 - 100))
                    .WithCondition(preParseResult.IsMainSkill.IsSet)
                    .Build();
                modifiers.AddRange(intermediateModifier.Build(preParseResult.GlobalSource, Entity.Character));
            }

            foreach (var partialParser in _partialParsers)
            {
                var (newlyParsedModifiers, newlyParsedStats) = partialParser.Parse(support, preParseResult);
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