using PoESkillTree.Common.Model.Items.Enums;

namespace PoESkillTree.Computation.Parsing.Builders.Equipment
{
    public interface IEquipmentBuilderCollection : IBuilderCollection<IEquipmentBuilder>
    {
        IEquipmentBuilder this[ItemSlot slot] { get; }
    }
}