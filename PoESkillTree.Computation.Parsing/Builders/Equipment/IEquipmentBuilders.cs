namespace PoESkillTree.Computation.Parsing.Builders.Equipment
{
    public interface IEquipmentBuilders
    {
        // does not include flasks and jewels
        IEquipmentBuilderCollection Equipment { get; }

        // Hand the mod line comes from. Does not do anything if mod is not on a weapon.
        IEquipmentBuilder LocalHand { get; }
    }
}