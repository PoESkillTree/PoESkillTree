using System;
using System.ComponentModel;

namespace POESKillTree.Model.Items.Enums
{
    /*
     * First weapon property to item class: (if base lookup fails)
     * - remove spaces
     * - replace "Handed" with "Hand"
     * - take matching class
     * (fails for sceptres and rapiers)
     * 
     * Item class to first weapon property:
     * - replace "ThrustingOneHandSword" with "OneHandSword"
     * - replace "Sceptre" with "OneHandMace"
     * - replace "Hand" with "Handed"
     * - add spaces in front of each capital letter
     * 
     * Item class to tag and vice versa: (if base lookup fails)
     * Bow = Bow | TwoHandWeapon | Ranged
     * Wand = Wand | OneHandWeapon | Ranged
     * Staff = Staff | TwoHandWeapon
     * OneHandMace = Mace | OneHandWeapon
     * TwoHandMace = Mace | TwoHandWeapon
     * ThrustingOneHandSword = Rapier | Sword | OneHandWeapon
     * OneHandSword = Sword | OneHandWeapon
     * TwoHandSword = Sword | TwoHandWeapon
     * Dagger = Dagger | OneHandWeapon
     * Claw = Claw | OneHandWeapon
     * OneHandAxe = Axe | OneHandWeapon
     * TwoHandAxe = Axe | TwoHandWeapon
     * Sceptre = Sceptre | OneHandWeapon
     * FishingRod = FishingRod | TwoHandWeapon
     * SupportSkillGem = Gem | SupportGem
     * ActiveSkillGem = Gem
     * not listed above: take exact match or Default if no exact match
     * 
     * Item class "Thrusting One Hand Sword" is considered a subclass of "One Hand Sword"
     * Item class "Sceptre" is considered a subclass of "One Hand Mace"
     * (for gem supporting and modifier application purposes)
     */

    // taken from the GGPK's Tags.dat
    [Flags]
    public enum Tags : ulong
    {
        Default = 0,

        Weapon = 1 << 0,
        OneHand = 1 << 1,
        TwoHand = 1 << 2,
        Ranged = 1 << 3,
        // these tags are in the game data but seem redundant
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
    }


    public static class TagsEx
    {
        public static bool TryParse(string ggpkTag, out Tags tag)
        {
            return Enum.TryParse(ggpkTag.Replace("_", ""), true, out tag);
        }
    }
}