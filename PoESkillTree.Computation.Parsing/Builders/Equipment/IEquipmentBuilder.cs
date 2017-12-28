using PoESkillTree.Common.Model.Items.Enums;
using PoESkillTree.Computation.Parsing.Builders.Conditions;
using PoESkillTree.Computation.Parsing.Builders.Matching;
using PoESkillTree.Computation.Parsing.Builders.Stats;

namespace PoESkillTree.Computation.Parsing.Builders.Equipment
{
    /// <summary>
    /// Represents a single equipment slot.
    /// </summary>
    public interface IEquipmentBuilder : IResolvable<IEquipmentBuilder>
    {
        /// <summary>
        /// Returns a condition that is satisfied if this slot holds an item having the given tags.
        /// </summary>
        IConditionBuilder Has(Tags tag);

        /// <summary>
        /// Returns a condition that is satisfied if this slot holds an item having the given frame type.
        /// </summary>
        IConditionBuilder Has(FrameType frameType);

        /// <summary>
        /// Returns a condition that is satisfied if this slot holds an item.
        /// </summary>
        IConditionBuilder HasItem { get; }

        /// <summary>
        /// Returns a condition that is satisfied if this slot holds a corrupted item.
        /// </summary>
        IConditionBuilder IsCorrupted { get; }

        /// <summary>
        /// Returns a flag stat representing whether stats of items in this slot apply to Self.
        /// </summary>
        IFlagStatBuilder AppliesToSelf { get; } // default: 1

        /// <summary>
        /// Returns a flag stat representing whether stats of items in this slot apply to Self's minions.
        /// </summary>
        IFlagStatBuilder AppliesToMinions { get; }
    }
}