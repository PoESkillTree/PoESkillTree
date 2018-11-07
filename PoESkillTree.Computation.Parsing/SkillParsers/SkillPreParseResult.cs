using PoESkillTree.Computation.Common;
using PoESkillTree.Computation.Common.Builders.Conditions;
using PoESkillTree.Computation.Common.Builders.Stats;
using PoESkillTree.GameModel.Skills;

namespace PoESkillTree.Computation.Parsing.SkillParsers
{
    public class SkillPreParseResult
    {
        public SkillPreParseResult(
            SkillDefinition skillDefinition, SkillLevelDefinition levelDefinition, SkillDefinition mainSkillDefinition,
            ModifierSource.Local.Skill localSource, ModifierSource.Global globalSource,
            ModifierSource.Local.Gem gemSource,
            IStatBuilder isMainSkill, IConditionBuilder isActiveSkill)
        {
            SkillDefinition = skillDefinition;
            LevelDefinition = levelDefinition;
            MainSkillDefinition = mainSkillDefinition;
            LocalSource = localSource;
            GlobalSource = globalSource;
            GemSource = gemSource;
            IsMainSkill = isMainSkill;
            IsActiveSkill = isActiveSkill;
        }

        public SkillDefinition SkillDefinition { get; }
        public SkillLevelDefinition LevelDefinition { get; }

        /// <summary>
        /// Same as <see cref="SkillDefinition"/> when parsing active skills. The active skill when parsing support
        /// skills.
        /// </summary>
        public SkillDefinition MainSkillDefinition { get; }

        public ModifierSource.Local.Skill LocalSource { get; }
        public ModifierSource.Global GlobalSource { get; }
        public ModifierSource.Local.Gem GemSource { get; }

        public IStatBuilder IsMainSkill { get; }
        public IConditionBuilder IsActiveSkill { get; }
    }
}