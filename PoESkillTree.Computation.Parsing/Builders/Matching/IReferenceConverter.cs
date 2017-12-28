using PoESkillTree.Computation.Parsing.Builders.Actions;
using PoESkillTree.Computation.Parsing.Builders.Charges;
using PoESkillTree.Computation.Parsing.Builders.Damage;
using PoESkillTree.Computation.Parsing.Builders.Effects;
using PoESkillTree.Computation.Parsing.Builders.Equipment;
using PoESkillTree.Computation.Parsing.Builders.Skills;
using PoESkillTree.Computation.Parsing.Builders.Stats;

namespace PoESkillTree.Computation.Parsing.Builders.Matching
{
    /// <summary>
    /// Converts objects referenced from other matcher collections to their types.
    /// <para>Because getting the actual referenced objects requires matching the stat line, these methods can not
    /// throw exceptions on invalid casts when first called. Exceptions will only be thrown once the context has been
    /// resolved.</para>
    /// </summary>
    public interface IReferenceConverter
    {
        IDamageTypeBuilder AsDamageType { get; }

        IChargeTypeBuilder AsChargeType { get; }

        IAilmentBuilder AsAilment { get; }

        IKeywordBuilder AsKeyword { get; }

        IItemSlotBuilder AsItemSlot { get; }

        IActionBuilder AsAction { get; }

        IStatBuilder AsStat { get; }

        IFlagStatBuilder AsFlagStat { get; }

        IPoolStatBuilder AsPoolStat { get; }

        IDamageStatBuilder AsDamageStat { get; }

        ISkillBuilder AsSkill { get; }
    }
}