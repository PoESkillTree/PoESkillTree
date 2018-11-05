using System;
using System.Collections.Generic;
using PoESkillTree.Computation.Common;
using PoESkillTree.Computation.Common.Builders;
using PoESkillTree.Computation.Common.Builders.Modifiers;
using PoESkillTree.Computation.Common.Builders.Stats;
using PoESkillTree.GameModel.Skills;

namespace PoESkillTree.Computation.Parsing.SkillParsers
{
    public class SkillTypeParser : IPartialSkillParser
    {
        private readonly IBuilderFactories _builderFactories;
        private readonly IMetaStatBuilders _metaStatBuilders;
        private readonly IModifierBuilder _modifierBuilder = new ModifierBuilder();
        private readonly Func<SkillDefinition, IEnumerable<string>> _selectTypes;

        private SkillTypeParser(
            IBuilderFactories builderFactories, IMetaStatBuilders metaStatBuilders,
            Func<SkillDefinition, IEnumerable<string>> selectTypes)
            => (_builderFactories, _metaStatBuilders, _selectTypes) =
                (builderFactories, metaStatBuilders, selectTypes);

        public static IPartialSkillParser CreateActive(
            IBuilderFactories builderFactories, IMetaStatBuilders metaStatBuilders)
            => new SkillTypeParser(builderFactories, metaStatBuilders, d => d.ActiveSkill.ActiveSkillTypes);

        public static IPartialSkillParser CreateSupport(
            IBuilderFactories builderFactories, IMetaStatBuilders metaStatBuilders)
            => new SkillTypeParser(builderFactories, metaStatBuilders, d => d.SupportSkill.AddedActiveSkillTypes);

        public PartialSkillParseResult Parse(Skill skill, SkillPreParseResult preParseResult)
        {
            var modifiers = new List<Modifier>();

            foreach (var type in _selectTypes(preParseResult.SkillDefinition))
            {
                var intermediateModifier = _modifierBuilder
                        // TODO won't work for supports
                    .WithStat(_metaStatBuilders.SkillHasType(skill.ItemSlot, skill.SocketIndex, type))
                    .WithForm(_builderFactories.FormBuilders.TotalOverride)
                    .WithValue(_builderFactories.ValueBuilders.Create(1))
                    .Build();
                modifiers.AddRange(intermediateModifier.Build(preParseResult.GlobalSource, Entity.Character));
            }

            return new PartialSkillParseResult(modifiers, new UntranslatedStat[0]);
        }
    }
}