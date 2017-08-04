using System;

namespace POESKillTree.Model.Items.Enums
{
    /// <summary>
    /// Defines the slots which can be filled with items.
    /// </summary>
    [Flags]
    public enum ItemSlot : uint//Requires 2147483648 worth of space to fit all jewel sockets as flags
    {
        Unequipable = 0x0,
        BodyArmour = 0x1,
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
        /// <summary>
        ///Jewel Slot directly north of Witch starting area (Int Threshold Jewel Slot)
        /// </summary>
        Jewel = 0x800,
        /// <summary>
        /// Jewel slot far NE of Scion Starting Area; Nearest Jewel to CI area (Int Threshold Jewel Slot)
        /// </summary>
        Jewel02 = 0x1000,
        /// <summary>
        /// NE from center jewel slot between Witch and shadow areas (Int Threshold Jewel Slot)
        /// </summary>
        Jewel03 = 0x2000,
        /// <summary>
        /// Scion jewel slot east of starting area (Non-Threshold Jewel Slot); Might be good slot for Intuitize Leap Jewel
        /// </summary>
        Jewel04 = 0x4000,
        /// <summary>
        /// Scion Jewel Slot west of starting area (Non-Threshold Jewel Slot)
        /// </summary>
        Jewel05 = 0x8000,
        /// <summary>
        /// Scion Jewel Slot south of starting area (Non-Threshold Jewel Slot)
        /// </summary>
        Jewel06 = 0x10000,
        /// <summary>
        /// Jewel Slot far east of Scion starting area between Shadow and Ranger areas; Nearest jewel slot to Acrobatics Jewel (Non-Threshold Jewel Slot)
        /// </summary>
        Jewel07 = 0x20000,
        /// <summary>
        /// Jewel Slot east of Scion starting area between Shadow and Ranger areas(above Ranger area); Nearest jewel slot to Charisma passive node
        /// (Dex Threshold Jewel Slot)
        /// </summary>
        Jewel08 = 0x40000,
        /// <summary>
        /// Jewel slot east of Shadow starting area (Both Int and Dex Threshold Jewel Slot)
        /// </summary>
        Jewel09 = 0x80000,
        /// <summary>
        /// Jewel slot east of Ranger area (Dex Threshold Jewel)
        /// </summary>
        Jewel10 = 0x100000,
        /// <summary>
        /// Jewel slot south-east of Scion area; At road between Ranger and Duelist areas (Dex Threshold Jewel Slot)
        /// </summary>
        Jewel11 = 0x200000,
        /// <summary>
        /// Jewel slot south-west of Scion area; At road between Marauder and Duelist areas 
        /// (Str Threshold Jewel Slot)
        /// </summary>
        Jewel12 = 0x400000,
        /// <summary>
        /// Jewel slot west of Scion area; At road between Marauder and Templar areas 
        /// (Str Threshold Jewel Slot)
        /// </summary>
        Jewel13 = 0x800000,
        /// <summary>
        /// Jewel slot north-west of Scion area; At road between Templar and Witch areas (Int Threshold Jewel Slot)
        /// </summary>
        Jewel14 = 0x1000000,
        /// <summary>
        /// Jewel slot far west of Scion area; At road between Marauder and Templar areas; 
        /// Nearest jewel slot to Resolute Technique
        /// (Str Threshold Jewel Slot)
        /// </summary>
        Jewel15 = 0x2000000,
        /// <summary>
        /// Jewel slot west of Templar starting area 
        /// (Both Int and Str Threshold Jewel Slot)
        /// </summary>
        Jewel16 = 0x4000000,
        /// <summary>
        /// Jewel slot south of Duelist starting area (Both Str and Dex Threshold Jewel Slot)
        /// </summary>
        Jewel17 = 0x8000000,
        /// <summary>
        /// Jewel slot far south-west of center; Located between Marauder and Duelist areas next to Iron Grip (Non-Threshold jewel slot)
        /// </summary>
        Jewel18 = 0x10000000,
        /// <summary>
        /// Jewel slot far south-east of center; Located between Duelist and Ranger areas next to Point Blank (Non-Threshold jewel slot)
        /// </summary>
        Jewel19 = 0x20000000,
        /// <summary>
        /// Jewel slot far north-west of center; Located between Templar and Witch areas next to Minion-Instability (Non-Threshold jewel slot)
        /// </summary>
        Jewel20 = 0x40000000,
        /// <summary>
        /// Jewel slot west of Marauder area (Str Threshold Jewel Slot)
        /// </summary>
        Jewel21 = 0x80000000
    }
}
