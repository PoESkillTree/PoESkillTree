using PoESkillTree.Computation.Common.Builders.Conditions;
using PoESkillTree.Computation.Common.Builders.Resolving;
using PoESkillTree.Computation.Common.Builders.Stats;
using PoESkillTree.GameModel.Items;

namespace PoESkillTree.Computation.Common.Builders.Equipment
{
    /// <summary>
    /// Represents a single equipment slot.
    /// </summary>
    public interface IEquipmentBuilder : IResolvable<IEquipmentBuilder>
    {
        /// <summary>
        /// The stat representing the tags of the item in this slot. In most cases,
        /// <see cref="Has(Tags)"/> should be used instead.
        /// </summary>
        IStatBuilder ItemTags { get; }

        /// <summary>
        /// Returns a condition that is satisfied if this slot holds an item having the given tags.
        /// </summary>
        IConditionBuilder Has(Tags tag);

        /// <summary>
        /// Returns a condition that is satisfied if this slot holds an item having the given item class.
        /// </summary>
        IConditionBuilder Has(ItemClass itemClass);

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