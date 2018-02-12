using System;
using System.ComponentModel;

namespace PoESkillTree.Common.Model.Items.Enums
{
    /// <summary>
    /// Tags of an item as in the GGPK's Tags.dat. Mostly used to define which mods can spawn on the item.
    /// </summary>
    [Flags]
    public enum Tags : ulong
    {
        [Description("Any")]
        Default = 0,

        Weapon = 1 << 0,
        OneHand = 1 << 1,
        TwoHand = 1 << 2,
        Ranged = 1 << 3,
        // Fishing Rod is the only item where these definitions are incorrect (it has Weapon and TwoHand but not 
        // TwoHandWeapon). By ignoring that, these tags can be defined as a combination.
        OneHandWeapon = Weapon | OneHand,
        TwoHandWeapon = Weapon | TwoHand,

        Bow = 1 << 4,
        Wand = 1 << 5,
        Staff = 1 << 6,
        Mace = 1 << 7,
        Sword = 1 << 8,
        Dagger = 1 << 9,
        Claw = 1 << 10,
        Axe = 1 << 11,
        Sceptre = 1 << 12,
        Rapier = 1 << 13,
        FishingRod = 1 << 14,

        Armour = 1 << 15,
        Shield = 1 << 16,
        Boots = 1 << 17,
        BodyArmour = 1 << 18,
        Gloves = 1 << 19,
        Helmet = 1 << 20,

        [Description("Armour")]
        StrArmour = 1 << 21,
        [Description("Evasion")]
        DexArmour = 1 << 22,
        [Description("Energy Shield")]
        IntArmour = 1 << 23,
        [Description("Armour and Evasion")]
        StrDexArmour = 1 << 24,
        [Description("Armour and Energy Shield")]
        StrIntArmour = 1 << 25,
        [Description("Evasion and Energy Shield")]
        DexIntArmour = 1 << 26,
        [Description("Armour, Evasion and Energy Shield")]
        StrDexIntArmour = 1 << 27,

        StrShield = Shield | StrArmour,
        DexShield = Shield | DexArmour,
        // Called "focus" on intelligence shields
        Focus = Shield | IntArmour,
        StrDexShield = Shield | StrDexArmour,
        StrIntShield = Shield | StrIntArmour,
        DexIntShield = Shield | DexIntArmour,

        Belt = 1 << 28,
        Ring = 1 << 29,
        Amulet = 1 << 30,
        Quiver = 1u << 31,

        TwoStoneRing = 1L << 32,
        UnsetRing = 1L << 33,

        Flask = 1L << 34,
        LifeFlask = 1L << 35,
        ManaFlask = 1L << 36,
        HybridFlask = 1L << 37,
        UtilityFlask = 1L << 38,
        CriticalUtilityFlask = 1L << 39,

        Jewel = 1L << 41,
        StrJewel = 1L << 42,
        DexJewel = 1L << 43,
        IntJewel = 1L << 44,
        NotStr = 1L << 45,
        NotDex = 1L << 46,
        NotInt = 1L << 47,

        Gem = 1L << 48,
        SupportGem = 1L << 49,

        // Prevents master signature mods from appearing on items that are not sold by vendors
        NotForSale = 1L << 50,
        // Prevents Diamond Flask from getting the increased effect mod
        NoEffectFlaskMod = 1L << 51,

        AbyssJewelMelee = 1L << 52,
        AbyssJewelRanged = 1L << 53,
        AbyssJewelCaster = 1L << 54,
        AbyssJewelSummoner = 1L << 55,
    }


    public static class TagsEx
    {
        /// <summary>
        /// Tries to convert a string from the GGPK's Tags.dat to an <see cref="Tags"/>
        /// instance.
        /// </summary>
        public static bool TryParse(string ggpkTag, out Tags tag)
        {
            return Enum.TryParse(ggpkTag.Replace("_", ""), true, out tag);
        }
    }
}