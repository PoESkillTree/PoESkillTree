using PoESkillTree.Computation.Common;
using PoESkillTree.Computation.Common.Builders.Stats;
using PoESkillTree.GameModel.Skills;

namespace PoESkillTree.Computation.Parsing.SkillParsers
{
    /// <summary>
    /// Collects properties for <see cref="ActiveSkillParser"/> and <see cref="SupportSkillParser"/> that are used
    /// in different partial parsers.
    /// </summary>
    public class SkillPreParser
    {
        private readonly SkillDefinitions _skillDefinitions;
        private readonly IMetaStatBuilders _metaStatBuilders;

        public SkillPreParser(SkillDefinitions skillDefinitions, IMetaStatBuilders metaStatBuilders)
            => (_skillDefinitions, _metaStatBuilders) = (skillDefinitions, metaStatBuilders);

        public SkillPreParseResult ParseActive(Skill activeSkill)
            => Parse(activeSkill, activeSkill);

        public SkillPreParseResult ParseSupport(Skill activeSkill, Skill supportSkill)
            => Parse(activeSkill, supportSkill);

        private SkillPreParseResult Parse(Skill mainSkill, Skill parsedSkill)
        {
            var mainSkillDefinition = _skillDefinitions.GetSkillById(mainSkill.Id);
            var parsedSkillDefinition = _skillDefinitions.GetSkillById(parsedSkill.Id);
            var parsedSkillLevel = parsedSkillDefinition.Levels[parsedSkill.Level];

            var localSource = new ModifierSource.Local.Skill(mainSkill.Id);
            var globalSource = new ModifierSource.Global(localSource);
            var gemSource = new ModifierSource.Local.Gem(parsedSkill.ItemSlot, parsedSkill.SocketIndex, mainSkill.Id);

            var isMainSkill = _metaStatBuilders.SkillIsMain(mainSkill.ItemSlot, mainSkill.SocketIndex).IsSet;
            var isActiveSkill = _metaStatBuilders.IsActiveSkill(mainSkill);

            return new SkillPreParseResult(parsedSkillDefinition, parsedSkillLevel, mainSkillDefinition,
                localSource, globalSource, gemSource,
                isMainSkill, isActiveSkill);
        }
    }
}