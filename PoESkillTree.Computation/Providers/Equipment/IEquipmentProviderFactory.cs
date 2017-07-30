namespace PoESkillTree.Computation.Providers.Equipment
{
    public interface IEquipmentProviderFactory
    {
        // does not include flasks and jewels
        IEquipmentProviderCollection Equipment { get; }

        // Hand the mod line comes from. Does not do anything if mod is not on a weapon.
        IEquipmentProvider LocalHand { get; }
    }
}