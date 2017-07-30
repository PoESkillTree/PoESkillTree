using PoESkillTree.Common.Model.Items.Enums;

namespace PoESkillTree.Computation.Providers.Equipment
{
    public interface IEquipmentProviderCollection : IProviderCollection<IEquipmentProvider>
    {
        IEquipmentProvider this[ItemSlot slot] { get; }
    }
}