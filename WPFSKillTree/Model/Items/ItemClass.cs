using System;

namespace POESKillTree.Model.Items
{
    /// <summary>
    /// itemclass and itemslot values with same name must have same value
    /// </summary>
    [Flags]
    public enum ItemClass
    {
        Invalid = 0x0,
        Armor = 0x1,
        MainHand = 0x2,
        OffHand = 0x4,
        Ring = 0x18,
        Amulet = 0x20,
        Helm = 0x40,
        Gloves = 0x80,
        Boots = 0x100,
        Gem = 0x200,
        Belt = 0x400,
        TwoHand = 0x800,
        Jewel = 0x1000,
        Unequipable = 0x800000,
    }


    /// <summary>
    /// itemclass and itemslot values with same name must have same value
    /// </summary>
    [Flags]
    public enum ItemSlot
    {
        Unequipable = 0x0,
        Armor = 0x1,
        MainHand = 0x2,
        OffHand = 0x4,
        Ring = 0x8,
        Ring2 = 0x10,
        Amulet = 0x20,
        Helm = 0x40,
        Gloves = 0x80,
        Boots = 0x100,
        Gem = 0x200,
        Belt = 0x400,
        TwoHand = 0x800,
    }
}
