using PoESkillTree.Computation.Common;
using PoESkillTree.Computation.Common.Builders.Damage;
using PoESkillTree.Computation.Common.Builders.Stats;
using PoESkillTree.GameModel.Skills;

namespace PoESkillTree.Computation.Parsing.SkillParsers
{
    public class SkillPreParseResult
    {
        public SkillPreParseResult(
            SkillDefinition skillDefinition, SkillLevelDefinition levelDefinition,
            ModifierSource.Local.Skill localSource, ModifierSource.Global globalSource,
            ModifierSource.Local.Gem gemSource, DamageSource? hitDamageSource, bool hasSkillDamageOverTime,
            IStatBuilder isMainSkill)
        {
            SkillDefinition = skillDefinition;
            LevelDefinition = levelDefinition;
            LocalSource = localSource;
            GlobalSource = globalSource;
            GemSource = gemSource;
            HitDamageSource = hitDamageSource;
            HasSkillDamageOverTime = hasSkillDamageOverTime;
            IsMainSkill = isMainSkill;
        }

        public SkillDefinition SkillDefinition { get; }
        public SkillLevelDefinition LevelDefinition { get; }

        public ModifierSource.Local.Skill LocalSource { get; }
        public ModifierSource.Global GlobalSource { get; }
        public ModifierSource.Local.Gem GemSource { get; }

        public DamageSource? HitDamageSource { get; }
        public bool HasSkillDamageOverTime { get; }

        public IStatBuilder IsMainSkill { get; }
    }
}