using System;
using System.Collections.Generic;
using PoESkillTree.Computation.Common;
using PoESkillTree.Computation.Common.Builders;
using PoESkillTree.Computation.Common.Builders.Stats;
using PoESkillTree.GameModel;
using PoESkillTree.GameModel.Skills;

namespace PoESkillTree.Computation.Parsing.SkillParsers
{
    /// <summary>
    /// Partial parser of <see cref="ActiveSkillParser"/> and <see cref="SupportSkillParser"/> that parses
    /// active skill types.
    /// </summary>
    public class SkillTypeParser : IPartialSkillParser
    {
        private readonly IBuilderFactories _builderFactories;
        private readonly IMetaStatBuilders _metaStatBuilders;
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

        public PartialSkillParseResult Parse(Skill mainSkill, Skill parsedSkill, SkillPreParseResult preParseResult)
        {
            var modifiers = new ModifierCollection(_builderFactories, preParseResult.LocalSource);

            foreach (var type in _selectTypes(preParseResult.SkillDefinition))
            {
                modifiers.AddGlobal(_metaStatBuilders.SkillHasType(mainSkill.ItemSlot, mainSkill.SocketIndex, type),
                    Form.TotalOverride, 1);
            }

            return new PartialSkillParseResult(modifiers, new UntranslatedStat[0]);
        }
    }
}