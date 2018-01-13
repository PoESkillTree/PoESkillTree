using PoESkillTree.Computation.Common.Builders.Actions;
using PoESkillTree.Computation.Common.Builders.Charges;
using PoESkillTree.Computation.Common.Builders.Damage;
using PoESkillTree.Computation.Common.Builders.Effects;
using PoESkillTree.Computation.Common.Builders.Equipment;
using PoESkillTree.Computation.Common.Builders.Skills;
using PoESkillTree.Computation.Common.Builders.Stats;

namespace PoESkillTree.Computation.Common.Builders.Resolving
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