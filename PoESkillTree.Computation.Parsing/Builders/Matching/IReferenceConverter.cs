using PoESkillTree.Computation.Parsing.Builders.Actions;
using PoESkillTree.Computation.Parsing.Builders.Charges;
using PoESkillTree.Computation.Parsing.Builders.Damage;
using PoESkillTree.Computation.Parsing.Builders.Effects;
using PoESkillTree.Computation.Parsing.Builders.Equipment;
using PoESkillTree.Computation.Parsing.Builders.Skills;
using PoESkillTree.Computation.Parsing.Builders.Stats;

namespace PoESkillTree.Computation.Parsing.Builders.Matching
{
    public interface IReferenceConverter
    {
        IDamageTypeBuilder AsDamageType { get; }

        IChargeTypeBuilder AsChargeType { get; }

        IAilmentBuilder AsAilment { get; }

        IKeywordBuilder AsKeyword { get; }

        IItemSlotBuilder AsItemSlot { get; }

        ISelfToAnyActionBuilder AsAction { get; }

        IStatBuilder AsStat { get; }

        IFlagStatBuilder AsFlagStat { get; }

        IPoolStatBuilder AsPoolStat { get; }

        IDamageStatBuilder AsDamageStat { get; }

        ISkillBuilder AsSkill { get; }
    }
}