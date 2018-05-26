using PoESkillTree.Common.Model.Items.Enums;
using PoESkillTree.Computation.Common.Builders.Conditions;
using PoESkillTree.Computation.Common.Builders.Resolving;

namespace PoESkillTree.Computation.Common.Builders.Equipment
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
    }
}