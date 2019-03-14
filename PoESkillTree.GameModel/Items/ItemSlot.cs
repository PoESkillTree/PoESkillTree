using System;
using System.Collections.Generic;
using EnumsNET;

namespace PoESkillTree.GameModel.Items
{
    /// <summary>
    /// Defines the slots which can be filled with items.
    /// </summary>
    [Flags]
    public enum ItemSlot
    {
        Unequipable = 0,
        BodyArmour = 1 << 0,
        MainHand = 1 << 1,
        OffHand = 1 << 2,
        Ring = 1 << 3,
        Ring2 = 1 << 4,
        Amulet = 1 << 5,
        Helm = 1 << 6,
        Gloves = 1 << 7,
        Boots = 1 << 8,
        Belt = 1 << 9,
        Flask1 = 1 << 10,
        Flask2 = 1 << 11,
        Flask3 = 1 << 12,
        Flask4 = 1 << 13,
        Flask5 = 1 << 14,
        SkillTree = 1 << 15,
    }

    public static class ItemSlotExtensions
    {
        public const ItemSlot Flask
            = ItemSlot.Flask1 | ItemSlot.Flask2 | ItemSlot.Flask3 | ItemSlot.Flask4 | ItemSlot.Flask5;

        public static IEnumerable<ItemSlot> Flasks
            => Flask.GetFlags();

        public static bool IsFlask(this ItemSlot @this)
            => @this.CommonFlags(Flask) > 0;
    }
}
