using PoESkillTree.Computation.Common;
using PoESkillTree.Computation.Common.Builders.Stats;
using PoESkillTree.GameModel.Skills;

namespace PoESkillTree.Computation.Parsing.SkillParsers
{
    public class SkillPreParser
    {
        private readonly SkillDefinitions _skillDefinitions;
        private readonly IMetaStatBuilders _metaStatBuilders;

        public SkillPreParser(SkillDefinitions skillDefinitions, IMetaStatBuilders metaStatBuilders)
            => (_skillDefinitions, _metaStatBuilders) = (skillDefinitions, metaStatBuilders);

        public SkillPreParseResult ParseActive(Skill activeSkill)
        {
            var definition = _skillDefinitions.GetSkillById(activeSkill.Id);
            var displayName = definition.BaseItem?.DisplayName ?? definition.ActiveSkill.DisplayName;
            return Parse(activeSkill, activeSkill, displayName);
        }

        public SkillPreParseResult ParseSupport(Skill activeSkill, Skill supportSkill)
        {
            var definition = _skillDefinitions.GetSkillById(supportSkill.Id);
            var displayName = definition.BaseItem?.DisplayName ?? supportSkill.Id;
            return Parse(activeSkill, supportSkill, displayName);
        }

        private SkillPreParseResult Parse(Skill mainSkill, Skill parsedSkill, string displayName)
        {
            var mainSkillDefinition = _skillDefinitions.GetSkillById(mainSkill.Id);
            var parsedSkillDefinition = _skillDefinitions.GetSkillById(parsedSkill.Id);
            var parsedSkillLevel = parsedSkillDefinition.Levels[parsedSkill.Level];

            var localSource = new ModifierSource.Local.Skill(displayName);
            var globalSource = new ModifierSource.Global(localSource);
            var gemSource = new ModifierSource.Local.Gem(parsedSkill.ItemSlot, parsedSkill.SocketIndex, displayName);

            var isMainSkillStat = _metaStatBuilders.SkillIsMain(mainSkill.ItemSlot, mainSkill.SocketIndex);

            var activeSkillItemSlot = _metaStatBuilders.ActiveSkillItemSlot(mainSkill.Id);
            var activeSkillSocketIndex = _metaStatBuilders.ActiveSkillSocketIndex(mainSkill.Id);
            var isActiveSkill = activeSkillItemSlot.Value.Eq((double) mainSkill.ItemSlot)
                .And(activeSkillSocketIndex.Value.Eq(mainSkill.SocketIndex));

            return new SkillPreParseResult(parsedSkillDefinition, parsedSkillLevel, mainSkillDefinition,
                localSource, globalSource, gemSource,
                isMainSkillStat, isActiveSkill);
        }
    }
}