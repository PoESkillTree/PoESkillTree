using System.Linq;
using PoESkillTree.Computation.Common.Builders.Conditions;
using PoESkillTree.GameModel.Items;

namespace PoESkillTree.Computation.Common.Builders.Equipment
{
    /// <summary>
    /// Represents a collection of equipment slots.
    /// </summary>
    public interface IEquipmentBuilderCollection : IBuilderCollection<IEquipmentBuilder>
    {
        /// <summary>
        /// Gets the equipment of the given slot.
        /// </summary>
        IEquipmentBuilder this[ItemSlot slot] { get; }
    }

    public static class EquipmentBuilderCollectionExtensions
    {
        public static IConditionBuilder IsAnyFlaskActive(this IEquipmentBuilderCollection @this)
            => ItemSlotExtensions.Flasks.Select(s => @this[s].HasItem).Aggregate((l, r) => l.Or(r));
    }
}