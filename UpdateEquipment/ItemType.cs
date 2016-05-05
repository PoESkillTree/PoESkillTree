using System;
using System.Collections.Generic;
using System.Linq;

namespace UpdateEquipment
{

    public enum ItemGroup
    {
        OneHandWeapon,
        TwoHandWeapon,
        Accessories,
        BodyArmour,
        Boots,
        Gloves,
        Helmet,
        Shield,
        Jewel
    }

    public enum ItemType
    {
        //One Handed weapons
        OneHandSwords,
        OneHandAxes,
        OneHandMaces,
        Daggers,
        Claws,
        Sceptres,
        Wands,
        ThrustingOneHandSwords,
        //Two Handed weapons
        TwoHandMaces,
        TwoHandAxes,
        TwoHandSwords,
        Bows,
        Staves,
        //Accessories
        Belts,
        Amulets,
        Rings,
        Quivers,
        //Body armours
        BodyArmoursArmour,
        BodyArmoursEnergyShield,
        BodyArmoursEvasion,
        BodyArmoursArmourEvasion,
        BodyArmoursArmourEnergyShield,
        BodyArmoursEvasionEnergyShield,
        BodyArmoursArmourEvasionEnergyShield,
        //Boots
        BootsArmour,
        BootsEvasion,
        BootsEnergyShield,
        BootsArmourEnergyShield,
        BootsArmourEvasion,
        BootsEvasionEnergyShield,
        //Gloves
        GlovesArmour,
        GlovesEvasion,
        GlovesEnergyShield,
        GlovesArmourEnergyShield,
        GlovesArmourEvasion,
        GlovesEvasionEnergyShield,
        //Helms
        HelmetsArmour,
        HelmetsEvasion,
        HelmetsEnergyShield,
        HelmetsArmourEnergyShield,
        HelmetsArmourEvasion,
        HelmetsEvasionEnergyShield,
        //Shields
        ShieldsArmour,
        ShieldsEvasion,
        ShieldsEnergyShield,
        ShieldsArmourEnergyShield,
        ShieldsArmourEvasion,
        ShieldsEvasionEnergyShield,
        //Jewels
        CobaltJewels,
        CrimsonJewels,
        ViridianJewels,
    }

    public static class ItemTypExtensions
    {
        private static readonly IReadOnlyDictionary<ItemGroup, IReadOnlyList<ItemType>> Groups = new Dictionary<ItemGroup, IReadOnlyList<ItemType>>
        {
            {ItemGroup.OneHandWeapon, new List<ItemType>
            {
                ItemType.OneHandSwords, ItemType.OneHandAxes, ItemType.OneHandMaces, ItemType.Daggers, ItemType.Claws, ItemType.Sceptres, ItemType.Wands, ItemType.ThrustingOneHandSwords
            }},
            {ItemGroup.TwoHandWeapon, new List<ItemType>
            {
                ItemType.TwoHandMaces, ItemType.TwoHandAxes, ItemType.TwoHandSwords, ItemType.Bows, ItemType.Staves
            }},
            {ItemGroup.Accessories, new List<ItemType>
            {
                ItemType.Belts, ItemType.Amulets, ItemType.Rings, ItemType.Quivers
            }},
            {ItemGroup.BodyArmour, new List<ItemType>
            {
                ItemType.BodyArmoursArmour, ItemType.BodyArmoursArmourEnergyShield, ItemType.BodyArmoursArmourEvasion, ItemType.BodyArmoursArmourEvasionEnergyShield, ItemType.BodyArmoursEnergyShield, ItemType.BodyArmoursEvasion, ItemType.BodyArmoursEvasionEnergyShield
            }},
            {ItemGroup.Boots, new List<ItemType>
            {
                ItemType.BootsArmour, ItemType.BootsArmourEnergyShield, ItemType.BootsArmourEvasion, ItemType.BootsEnergyShield, ItemType.BootsEvasion, ItemType.BootsEvasionEnergyShield
            }},
            {ItemGroup.Gloves, new List<ItemType>
            {
                ItemType.GlovesArmour, ItemType.GlovesArmourEnergyShield, ItemType.GlovesArmourEvasion, ItemType.GlovesEnergyShield, ItemType.GlovesEvasion, ItemType.GlovesEvasionEnergyShield
            }},
            {ItemGroup.Helmet, new List<ItemType>
            {
                ItemType.HelmetsArmourEnergyShield, ItemType.HelmetsArmour, ItemType.HelmetsArmourEvasion, ItemType.HelmetsEnergyShield, ItemType.HelmetsEvasion, ItemType.HelmetsEvasionEnergyShield
            }},
            {ItemGroup.Shield, new List<ItemType>
            {
                ItemType.ShieldsArmour, ItemType.ShieldsArmourEnergyShield, ItemType.ShieldsArmourEvasion, ItemType.ShieldsEnergyShield, ItemType.ShieldsEvasion, ItemType.ShieldsEvasionEnergyShield
            }},
            {ItemGroup.Jewel, new List<ItemType>
            {
                ItemType.CobaltJewels, ItemType.CrimsonJewels, ItemType.ViridianJewels
            }}
        };

        private static readonly IReadOnlyDictionary<ItemType, ItemGroup> TypeToGroup = 
            (from t in Enum.GetValues(typeof(ItemType)).Cast<ItemType>()
             let g = Groups.First(g => g.Value.Contains(t)).Key
             select new {t, g})
            .ToDictionary(t => t.t, t => t.g);

        public static ItemGroup Group(this ItemType type)
        {
            return TypeToGroup[type];
        }

        public static IReadOnlyList<ItemType> Types(this ItemGroup group)
        {
            return Groups[group];
        }
    }
}