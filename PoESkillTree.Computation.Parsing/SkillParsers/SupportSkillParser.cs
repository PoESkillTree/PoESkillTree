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
        private readonly SkillDefinitions _skillDefinitions;
        private readonly IBuilderFactories _builderFactories;
        private readonly IMetaStatBuilders _metaStatBuilders;
        private readonly IModifierBuilder _modifierBuilder = new ModifierBuilder();

        public SupportSkillParser(
            SkillDefinitions skillDefinitions, IBuilderFactories builderFactories, IMetaStatBuilders metaStatBuilders)
        {
            _skillDefinitions = skillDefinitions;
            _builderFactories = builderFactories;
            _metaStatBuilders = metaStatBuilders;
        }

        public ParseResult Parse(SupportSkillParserParameter parameter)
        {
            var (active, support) = parameter;

            var definition = _skillDefinitions.GetSkillById(support.Id);
            var level = definition.Levels[support.Level];

            var displayName = definition.BaseItem?.DisplayName ??
                              (definition.IsSupport ? support.Id : definition.ActiveSkill.DisplayName);
            var localSource = new ModifierSource.Local.Skill(displayName);
            var globalSource = new ModifierSource.Global(localSource);
            var gemSource = new ModifierSource.Local.Gem(support.ItemSlot, support.SocketIndex, displayName);

            var isMainSkillStat = _metaStatBuilders.MainSkillSocket(active.ItemSlot, active.SocketIndex);
            
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