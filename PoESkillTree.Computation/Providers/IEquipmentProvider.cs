using PoESkillTree.Common.Model.Items.Enums;

namespace PoESkillTree.Computation.Providers
{
    public interface IEquipmentProvider
    {
        IConditionProvider Has(Tags tag);
        IConditionProvider Has(FrameType frameType);
        IConditionProvider HasItem { get; }
        IConditionProvider IsCorrupted { get; }

        IFlagStatProvider AppliesToSelf { get; } // default: 1
        IFlagStatProvider AppliesToMinions { get; }
    }

    public interface IEquipmentProviderCollection : IProviderCollection<IEquipmentProvider>
    {
        IEquipmentProvider this[ItemSlot slot] { get; }
    }

    public static class EquipmentProviders
    {
        // does not include flasks and jewels
        public static readonly IEquipmentProviderCollection Equipment;

        public static readonly IEquipmentProvider MainHand = Equipment[ItemSlot.MainHand];
        public static readonly IEquipmentProvider OffHand = Equipment[ItemSlot.OffHand];
        // Hand the mod line comes from. Does not do anything if mod is not on a weapon.
        public static readonly IEquipmentProvider LocalHand;
    }
}