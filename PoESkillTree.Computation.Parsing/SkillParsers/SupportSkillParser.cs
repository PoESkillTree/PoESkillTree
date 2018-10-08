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
        private readonly IBuilderFactories _builderFactories;
        private readonly IModifierBuilder _modifierBuilder = new ModifierBuilder();

        public SupportSkillParser(
            SkillDefinitions skillDefinitions, IBuilderFactories builderFactories, IMetaStatBuilders metaStatBuilders)
        {
            _builderFactories = builderFactories;
            _preParser = new SkillPreParser(skillDefinitions, metaStatBuilders);
        }

        public ParseResult Parse(SupportSkillParserParameter parameter)
        {
            var (active, support) = parameter;

            var preParseResult = _preParser.ParseSupport(active, support);
            var level = preParseResult.LevelDefinition;
            var globalSource = preParseResult.GlobalSource;
            var gemSource = preParseResult.GemSource;
            var isMainSkillStat = preParseResult.IsMainSkill;
            
            var modifiers = new List<Modifier>();

            void AddModifier(IStatBuilder stat, Form form, double value, ModifierSource source, bool mainSkillOnly = true)
            {
                var modifierBuilder = _modifierBuilder
                    .WithStat(stat)
                    .WithForm(_builderFactories.FormBuilders.From(form))
                    .WithValue(_builderFactories.ValueBuilders.Create(value));
                if (mainSkillOnly)
                    modifierBuilder = modifierBuilder.WithCondition(isMainSkillStat.IsSet);
                modifiers.AddRange(modifierBuilder.Build().Build(source, Entity.Character));
            }
            
            if (level.ManaMultiplier is double multiplier)
            {
                AddModifier(_builderFactories.StatBuilders.Pool.From(Pool.Mana).Cost, Form.More,
                    multiplier * 100 - 100, globalSource);
            }
            
            var requirementStats = _builderFactories.StatBuilders.Requirements;
            AddModifier(requirementStats.Level, Form.BaseSet, level.RequiredLevel, gemSource, false);
            if (level.RequiredDexterity > 0)
            {
                AddModifier(requirementStats.Dexterity, Form.BaseSet, level.RequiredDexterity, gemSource, false);
            }
            if (level.RequiredIntelligence > 0)
            {
                AddModifier(requirementStats.Intelligence, Form.BaseSet, level.RequiredIntelligence, gemSource, false);
            }
            if (level.RequiredStrength > 0)
            {
                AddModifier(requirementStats.Strength, Form.BaseSet, level.RequiredStrength, gemSource, false);
            }

            var coldDamage = _builderFactories.DamageTypeBuilders.Cold.Damage;
            AddModifier(coldDamage, Form.Increase, level.QualityStats[0].Value * 20 / 1000, globalSource);
            var intermediateModifier = _modifierBuilder
                .WithStat(coldDamage.WithHits)
                .WithForm(_builderFactories.FormBuilders.From(Form.BaseAdd))
                .WithValue(_builderFactories.ValueBuilders.FromMinAndMax(
                    _builderFactories.ValueBuilders.Create(level.Stats[0].Value),
                    _builderFactories.ValueBuilders.Create(level.Stats[1].Value)))
                .WithCondition(isMainSkillStat.IsSet)
                .Build();
            modifiers.AddRange(intermediateModifier.Build(globalSource, Entity.Character));

            return ParseResult.Success(modifiers);
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