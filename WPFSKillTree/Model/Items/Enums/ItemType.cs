using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace POESKillTree.Model.Items.Enums
{
    /// <summary>
    /// The ItemType of an item defines which mod can be crafted on it.
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
        [Description("Armour")]
        BodyArmourArmour,
        [Description("Evasion")]
        BodyArmourEvasion,
        [Description("Energy Shield")]
        BodyArmourEnergyShield,
        [Description("Armour and Energy Shield")]
        BodyArmourArmourEnergyShield,
        [Description("Armour and Evasion")]
        BodyArmourArmourEvasion,
        [Description("Evasion and Energy Shield")]
        BodyArmourEvasionEnergyShield,
        [Description("Armour, Evasion and Energy Shield")]
        BodyArmourArmourEvasionEnergyShield,
        // Boots
        [Description("Armour")]
        BootsArmour,
        [Description("Evasion")]
        BootsEvasion,
        [Description("Energy Shield")]
        BootsEnergyShield,
        [Description("Armour and Energy Shield")]
        BootsArmourEnergyShield,
        [Description("Armour and Evasion")]
        BootsArmourEvasion,
        [Description("Evasion and Energy Shield")]
        BootsEvasionEnergyShield,
        // Gloves
        [Description("Armour")]
        GlovesArmour,
        [Description("Evasion")]
        GlovesEvasion,
        [Description("Energy Shield")]
        GlovesEnergyShield,
        [Description("Armour and Energy Shield")]
        GlovesArmourEnergyShield,
        [Description("Armour and Evasion")]
        GlovesArmourEvasion,
        [Description("Evasion and Energy Shield")]
        GlovesEvasionEnergyShield,
        // Helms
        [Description("Armour")]
        HelmetArmour,
        [Description("Evasion")]
        HelmetEvasion,
        [Description("Energy Shield")]
        HelmetEnergyShield,
        [Description("Armour and Energy Shield")]
        HelmetArmourEnergyShield,
        [Description("Armour and Evasion")]
        HelmetArmourEvasion,
        [Description("Evasion and Energy Shield")]
        HelmetEvasionEnergyShield,
        // Shields
        [Description("Armour")]
        ShieldArmour,
        [Description("Evasion")]
        ShieldEvasion,
        [Description("Energy Shield")]
        ShieldEnergyShield,
        [Description("Armour and Energy Shield")]
        ShieldArmourEnergyShield,
        [Description("Armour and Evasion")]
        ShieldArmourEvasion,
        [Description("Evasion and Energy Shield")]
        ShieldEvasionEnergyShield,
        // Jewels
        CobaltJewel,
        CrimsonJewel,
        ViridianJewel,
        // Other
        Gem
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
            {ItemGroup.OneHandedWeapon, new List<ItemType>
            {
                ItemType.OneHandedSword, ItemType.ThrustingOneHandedSword, ItemType.OneHandedAxe, ItemType.OneHandedMace,
                ItemType.Sceptre, ItemType.Dagger, ItemType.Claw, ItemType.Wand
            }},
            {ItemGroup.TwoHandedWeapon, new List<ItemType>
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
            {ItemGroup.Gem, new [] {ItemType.Gem}},
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

        /// <summary>
        /// Returns all <see cref="ItemSlot"/>s items of this group can be slotted into.
        /// </summary>
        public static ItemSlot ItemSlots(this ItemGroup group)
        {
            switch (group)
            {
                case ItemGroup.BodyArmour:
                    return ItemSlot.Armor;
                case ItemGroup.Boots:
                    return ItemSlot.Boots;
                case ItemGroup.Gloves:
                    return ItemSlot.Gloves;
                case ItemGroup.Helmet:
                    return ItemSlot.Helm;
                case ItemGroup.Shield:
                case ItemGroup.Quiver:
                    return ItemSlot.OffHand;
                case ItemGroup.OneHandedWeapon:
                    return ItemSlot.MainHand | ItemSlot.OffHand;
                case ItemGroup.TwoHandedWeapon:
                    return ItemSlot.MainHand;
                case ItemGroup.Belt:
                    return ItemSlot.Belt;
                case ItemGroup.Ring:
                    return ItemSlot.Ring | ItemSlot.Ring2;
                case ItemGroup.Amulet:
                    return ItemSlot.Amulet;
                case ItemGroup.Gem:
                    return ItemSlot.Gem;
                case ItemGroup.Jewel:
                case ItemGroup.Unknown:
                    return ItemSlot.Unequipable;
                default:
                    return ItemSlot.Unequipable;
            }
        }
    }
}