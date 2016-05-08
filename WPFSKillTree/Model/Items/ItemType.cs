using System;
using System.Collections.Generic;
using System.Linq;

namespace POESKillTree.Model.Items
{
    /// <summary>
    /// The ItemType of an item defines which mod can be crafted on it and in
    /// which eqipment slots it can go.
    /// </summary>
    public enum ItemType
    {
        Unknown,
        // One Handed weapons
        OneHandedSword,
        OneHandedAxe,
        OneHandedMace,
        Dagger,
        Claw,
        Sceptre,
        Wand,
        ThrustingOneHandedSword,
        // Two Handed weapons
        TwoHandedMace,
        TwoHandedAxe,
        TwoHandedSword,
        Bow,
        Staff,
        // Accessories
        Belt,
        Amulet,
        Ring,
        Quiver,
        // Body armours
        BodyArmourArmour,
        BodyArmourEnergyShield,
        BodyArmourEvasion,
        BodyArmourArmourEvasion,
        BodyArmourArmourEnergyShield,
        BodyArmourEvasionEnergyShield,
        BodyArmourArmourEvasionEnergyShield,
        // Boots
        BootsArmour,
        BootsEvasion,
        BootsEnergyShield,
        BootsArmourEnergyShield,
        BootsArmourEvasion,
        BootsEvasionEnergyShield,
        // Gloves
        GlovesArmour,
        GlovesEvasion,
        GlovesEnergyShield,
        GlovesArmourEnergyShield,
        GlovesArmourEvasion,
        GlovesEvasionEnergyShield,
        // Helms
        HelmetArmour,
        HelmetEvasion,
        HelmetEnergyShield,
        HelmetArmourEnergyShield,
        HelmetArmourEvasion,
        HelmetEvasionEnergyShield,
        // Shields
        ShieldArmour,
        ShieldEvasion,
        ShieldEnergyShield,
        ShieldArmourEnergyShield,
        ShieldArmourEvasion,
        ShieldEvasionEnergyShield,
        // Jewels
        CobaltJewel,
        CrimsonJewel,
        ViridianJewel
    }

    /// <summary>
    /// Extension methods for <see cref="ItemType"/> and <see cref="ItemGroup"/>.
    /// </summary>
    public static class ItemTypExtensions
    {
        private static readonly IReadOnlyDictionary<ItemGroup, IReadOnlyList<ItemType>> Groups = new Dictionary<ItemGroup, IReadOnlyList<ItemType>>
        {
            // The ItemTypes in a group must be declared in the order in which they are listed on the wiki page for the group
            // if all ItemTypes of a ItemGroup a listed on one wiki page.
            {ItemGroup.OneHandWeapon, new List<ItemType>
            {
                ItemType.OneHandedSword, ItemType.ThrustingOneHandedSword, ItemType.OneHandedAxe, ItemType.OneHandedMace,
                ItemType.Sceptre, ItemType.Dagger, ItemType.Claw, ItemType.Wand
            }},
            {ItemGroup.TwoHandWeapon, new List<ItemType>
            {
                ItemType.TwoHandedMace, ItemType.TwoHandedAxe, ItemType.TwoHandedSword, ItemType.Staff, ItemType.Bow
            }},
            {ItemGroup.Belt, new [] {ItemType.Belt}},
            {ItemGroup.Amulet, new [] {ItemType.Amulet}},
            {ItemGroup.Ring, new [] {ItemType.Ring}},
            {ItemGroup.Quiver, new [] {ItemType.Quiver}},
            {ItemGroup.BodyArmour, new List<ItemType>
            {
                ItemType.BodyArmourArmour, ItemType.BodyArmourEvasion, ItemType.BodyArmourEnergyShield,
                ItemType.BodyArmourArmourEvasion, ItemType.BodyArmourArmourEnergyShield, ItemType.BodyArmourEvasionEnergyShield,
                ItemType.BodyArmourArmourEvasionEnergyShield
            }},
            {ItemGroup.Boots, new List<ItemType>
            {
                ItemType.BootsArmour, ItemType.BootsEvasion, ItemType.BootsEnergyShield,
                ItemType.BootsArmourEvasion, ItemType.BootsArmourEnergyShield, ItemType.BootsEvasionEnergyShield
            }},
            {ItemGroup.Gloves, new List<ItemType>
            {
                ItemType.GlovesArmour, ItemType.GlovesEvasion, ItemType.GlovesEnergyShield,
                ItemType.GlovesArmourEvasion, ItemType.GlovesArmourEnergyShield, ItemType.GlovesEvasionEnergyShield
            }},
            {ItemGroup.Helmet, new List<ItemType>
            {
                ItemType.HelmetArmour, ItemType.HelmetEvasion, ItemType.HelmetEnergyShield,
                ItemType.HelmetArmourEvasion, ItemType.HelmetArmourEnergyShield, ItemType.HelmetEvasionEnergyShield
            }},
            {ItemGroup.Shield, new List<ItemType>
            {
                ItemType.ShieldArmour, ItemType.ShieldEvasion, ItemType.ShieldEnergyShield,
                ItemType.ShieldArmourEvasion, ItemType.ShieldArmourEnergyShield, ItemType.ShieldEvasionEnergyShield
            }},
            {ItemGroup.Jewel, new List<ItemType>
            {
                ItemType.CobaltJewel, ItemType.CrimsonJewel, ItemType.ViridianJewel
            }},
            {ItemGroup.Unknown, new [] {ItemType.Unknown}}
        };

        private static readonly IReadOnlyDictionary<ItemType, ItemGroup> TypeToGroup =
            (from t in Enum.GetValues(typeof(ItemType)).Cast<ItemType>()
             let g = Groups.First(g => g.Value.Contains(t)).Key
             select new { t, g })
            .ToDictionary(t => t.t, t => t.g);

        /// <summary>
        /// Returns the <see cref="ItemGroup"/> this <see cref="ItemType"/> belongs to.
        /// </summary>
        public static ItemGroup Group(this ItemType type)
        {
            return TypeToGroup[type];
        }

        /// <summary>
        /// Returns all <see cref="ItemType"/>s that belong to this <see cref="ItemGroup"/>.
        /// </summary>
        public static IReadOnlyList<ItemType> Types(this ItemGroup group)
        {
            return Groups[group];
        }

        public static ItemClass ToItemClass(this ItemType type)
        {
            switch (type.Group())
            {
                case ItemGroup.BodyArmour:
                    return ItemClass.Armor;
                case ItemGroup.Boots:
                    return ItemClass.Boots;
                case ItemGroup.Gloves:
                    return ItemClass.Gloves;
                case ItemGroup.Helmet:
                    return ItemClass.Helm;
                case ItemGroup.Shield:
                    return ItemClass.OffHand;
                case ItemGroup.Jewel:
                    return ItemClass.Jewel;
                case ItemGroup.OneHandWeapon:
                    return ItemClass.MainHand;
                case ItemGroup.TwoHandWeapon:
                    return ItemClass.TwoHand;
                case ItemGroup.Belt:
                    return ItemClass.Belt;
                case ItemGroup.Ring:
                    return ItemClass.Ring;
                case ItemGroup.Amulet:
                    return ItemClass.Amulet;
                case ItemGroup.Quiver:
                    return ItemClass.OffHand;
                default:
                    return ItemClass.Invalid;
            }
        }
    }
}