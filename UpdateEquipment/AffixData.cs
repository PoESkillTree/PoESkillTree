using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Serialization;

namespace UpdateEquipment
{
    public enum ModType
    {
        Prefix,
        Suffix
    }

    public enum ItemGroup
    {
        OneHandWeapon,
        TwoHandWeapon,
        Jewelry,
        Chest,
        Boots,
        Gloves,
        Helm,
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
        //Jewelry
        Belts,
        Amulets,
        Rings,
        Quivers,
        //Chests
        ChestsArmour,
        ChestsEnergy,
        ChestsEvasion,
        ChestsArmourEvasion,
        ChestsArmourEnergy,
        ChestsEvasionEnergy,
        ChestsArmourEvasionEnergy,
        //Boots
        BootsArmour,
        BootsEvasion,
        BootsEnergy,
        BootsArmourEnergy,
        BootsArmourEvasion,
        BootsEvasionEnergy,
        //Gloves
        GlovesArmour,
        GlovesEvasion,
        GlovesEnergy,
        GlovesArmourEnergy,
        GlovesArmourEvasion,
        GlovesEvasionEnergy,
        //Helms
        HelmsArmour,
        HelmsEvasion,
        HelmsEnergy,
        HelmsArmourEnergy,
        HelmsArmourEvasion,
        HelmsEvasionEnergy,
        //Shields
        ShieldsArmour,
        ShieldsEvasion,
        ShieldsEnergy,
        ShieldsArmourEnergy,
        ShieldsArmourEvasion,
        ShieldsEvasionEnergy,
        //Jewels
        CobaltJewels,
        CrimsonJewels,
        ViridianJewels,
    }

    public static class ItemTypExtensions
    {
        private static readonly Dictionary<ItemGroup, HashSet<ItemType>> Groups = new Dictionary<ItemGroup, HashSet<ItemType>>
        {
            {ItemGroup.OneHandWeapon, new HashSet<ItemType>
            {
                ItemType.OneHandSwords, ItemType.OneHandAxes, ItemType.OneHandMaces, ItemType.Daggers, ItemType.Claws, ItemType.Sceptres, ItemType.Wands, ItemType.ThrustingOneHandSwords
            }},
            {ItemGroup.TwoHandWeapon, new HashSet<ItemType>
            {
                ItemType.TwoHandMaces, ItemType.TwoHandAxes, ItemType.TwoHandSwords, ItemType.Bows, ItemType.Staves
            }},
            {ItemGroup.Jewelry, new HashSet<ItemType>
            {
                ItemType.Belts, ItemType.Amulets, ItemType.Rings, ItemType.Quivers
            }},
            {ItemGroup.Chest, new HashSet<ItemType>
            {
                ItemType.ChestsArmour, ItemType.ChestsArmourEnergy, ItemType.ChestsArmourEvasion, ItemType.ChestsArmourEvasionEnergy, ItemType.ChestsEnergy, ItemType.ChestsEvasion, ItemType.ChestsEvasionEnergy
            }},
            {ItemGroup.Boots, new HashSet<ItemType>
            {
                ItemType.BootsArmour, ItemType.BootsArmourEnergy, ItemType.BootsArmourEvasion, ItemType.BootsEnergy, ItemType.BootsEvasion, ItemType.BootsEvasionEnergy
            }},
            {ItemGroup.Gloves, new HashSet<ItemType>
            {
                ItemType.GlovesArmour, ItemType.GlovesArmourEnergy, ItemType.GlovesArmourEvasion, ItemType.GlovesEnergy, ItemType.GlovesEvasion, ItemType.GlovesEvasionEnergy
            }},
            {ItemGroup.Helm, new HashSet<ItemType>
            {
                ItemType.HelmsArmourEnergy, ItemType.HelmsArmour, ItemType.HelmsArmourEvasion, ItemType.HelmsEnergy, ItemType.HelmsEvasion, ItemType.HelmsEvasionEnergy
            }},
            {ItemGroup.Shield, new HashSet<ItemType>
            {
                ItemType.ShieldsArmour, ItemType.ShieldsArmourEnergy, ItemType.ShieldsArmourEvasion, ItemType.ShieldsEnergy, ItemType.ShieldsEvasion, ItemType.ShieldsEvasionEnergy
            }},
            {ItemGroup.Jewel, new HashSet<ItemType>
            {
                ItemType.CobaltJewels, ItemType.CrimsonJewels, ItemType.ViridianJewels
            }}
        };

        public static ItemGroup Group(this ItemType type)
        {
            return Groups.First(g => g.Value.Contains(type)).Key;
        }
    }

    [Serializable]
    [XmlType(AnonymousType = true)]
    [XmlRoot(Namespace = "", IsNullable = false)]
    public class AffixList
    {
        [XmlElement("Affix")]
        public Affix[] Affix { get; set; }
    }

    [Serializable]
    [XmlType(AnonymousType = true)]
    public class Affix
    {
        [XmlElement("Tier")]
        public AffixTier[] Tier { get; set; }

        [XmlAttribute]
        public string Name { get; set; }

        [XmlAttribute]
        public ModType ModType { get; set; }

        [XmlAttribute]
        public bool Global { get; set; }

        [XmlAttribute]
        public ItemType ItemType { get; set; }

        [XmlAttribute]
        public string CraftedAs { get; set; }
    }

    [Serializable]
    [XmlType(AnonymousType = true)]
    public class AffixTier
    {
        [XmlElement("Stat")]
        public AffixTierStat[] Stat { get; set; }

        [XmlAttribute]
        public int ItemLevel { get; set; }

        [XmlAttribute]
        public string Name { get; set; }
    }

    [Serializable]
    [XmlType(AnonymousType = true)]
    public class AffixTierStat
    {
        [XmlAttribute]
        public float From { get; set; }

        [XmlAttribute]
        public float To { get; set; }
    }
}