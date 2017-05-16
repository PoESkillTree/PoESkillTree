using System;
using System.Collections.Generic;

namespace POESKillTree.Model.Items.Enums
{
    // taken from GGPK's ItemClasses.dat ("id" field) (except Unknown and Any)
    public enum ItemClass
    {
        Unknown,
        // For crafting display purposes
        Any,

        OneHandSword,
        // Considered a subclass of OneHandSword for gem supporting and mod application
        ThrustingOneHandSword,
        OneHandAxe,
        OneHandMace,
        // Considered a subclass of OneHandMace for gem supporting and mod application
        Sceptre,
        Dagger,
        Claw,
        Wand,
        //Unarmed,
        FishingRod,

        TwoHandSword,
        TwoHandAxe,
        TwoHandMace,
        Bow,
        Staff,

        Belt,
        Ring,
        Amulet,
        Quiver,

        Shield,
        Boots,
        BodyArmour,
        Gloves,
        Helmet,

        LifeFlask,
        ManaFlask,
        HybridFlask,
        UtilityFlask,
        CriticalUtilityFlask,

        Jewel,

        ActiveSkillGem,
        SupportSkillGem,
    }

    public static class ItemClassEx
    {
        private static readonly IReadOnlyDictionary<ItemClass, Tags> ItemClassToTags = new Dictionary<ItemClass, Tags>
        {
            { ItemClass.Unknown, Tags.Default },
            { ItemClass.Any, Tags.Default },
            { ItemClass.OneHandSword, Tags.Sword | Tags.OneHandWeapon },
            { ItemClass.ThrustingOneHandSword, Tags.Rapier | Tags.Sword | Tags.OneHandWeapon },
            { ItemClass.OneHandAxe, Tags.Axe | Tags.OneHandWeapon },
            { ItemClass.OneHandMace, Tags.Mace | Tags.OneHandWeapon },
            { ItemClass.Sceptre, Tags.Sceptre | Tags.OneHandWeapon },
            { ItemClass.Dagger, Tags.Dagger | Tags.OneHandWeapon },
            { ItemClass.Claw, Tags.Claw | Tags.OneHandWeapon },
            { ItemClass.Wand, Tags.Wand | Tags.OneHandWeapon | Tags.Ranged },
            { ItemClass.TwoHandSword, Tags.Sword | Tags.TwoHandWeapon },
            { ItemClass.TwoHandAxe, Tags.Axe | Tags.TwoHandWeapon },
            { ItemClass.TwoHandMace, Tags.Mace | Tags.TwoHandWeapon },
            { ItemClass.Bow, Tags.Bow | Tags.TwoHandWeapon | Tags.Ranged },
            { ItemClass.Staff, Tags.Staff | Tags.TwoHandWeapon },
            { ItemClass.FishingRod, Tags.FishingRod | Tags.TwoHandWeapon },
            { ItemClass.Belt, Tags.Belt },
            { ItemClass.Ring, Tags.Ring },
            { ItemClass.Amulet, Tags.Amulet },
            { ItemClass.Quiver, Tags.Quiver },
            { ItemClass.Shield, Tags.Shield | Tags.Armour },
            { ItemClass.Boots, Tags.Boots | Tags.Armour },
            { ItemClass.BodyArmour, Tags.BodyArmour | Tags.Armour },
            { ItemClass.Gloves, Tags.Gloves | Tags.Armour },
            { ItemClass.Helmet, Tags.Helmet | Tags.Armour },
            { ItemClass.LifeFlask, Tags.LifeFlask | Tags.Flask },
            { ItemClass.ManaFlask, Tags.ManaFlask | Tags.Flask },
            { ItemClass.HybridFlask, Tags.HybridFlask | Tags.Flask },
            { ItemClass.UtilityFlask, Tags.UtilityFlask | Tags.Flask },
            { ItemClass.CriticalUtilityFlask, Tags.CriticalUtilityFlask | Tags.UtilityFlask | Tags.Flask },
            { ItemClass.Jewel, Tags.Jewel },
            { ItemClass.ActiveSkillGem, Tags.Gem },
            { ItemClass.SupportSkillGem, Tags.SupportGem | Tags.Gem }
        };

        public static Tags ToTags(this ItemClass itemClass)
        {
            return ItemClassToTags[itemClass];
        }

        public static bool TryParse(string ggpkItemClass, out ItemClass itemClass)
        {
            return Enum.TryParse(ggpkItemClass.Replace(" ", ""), true, out itemClass);
        }

        public static ItemClass ItemClassForGem(string gemName)
        {
            return gemName.EndsWith(" Support") ? ItemClass.SupportSkillGem : ItemClass.ActiveSkillGem;
        }

        /// <summary>
        /// Returns all <see cref="ItemSlot"/>s items of this class can be slotted into.
        /// </summary>
        public static ItemSlot ItemSlots(this ItemClass itemClass)
        {
            switch (itemClass)
            {
                case ItemClass.OneHandSword:
                case ItemClass.ThrustingOneHandSword:
                case ItemClass.OneHandAxe:
                case ItemClass.OneHandMace:
                case ItemClass.Sceptre:
                case ItemClass.Dagger:
                case ItemClass.Claw:
                case ItemClass.Wand:
                    return ItemSlot.MainHand | ItemSlot.OffHand;
                case ItemClass.FishingRod:
                case ItemClass.TwoHandSword:
                case ItemClass.TwoHandAxe:
                case ItemClass.TwoHandMace:
                case ItemClass.Bow:
                case ItemClass.Staff:
                    return ItemSlot.MainHand;
                case ItemClass.Belt:
                    return ItemSlot.Belt;
                case ItemClass.Ring:
                    return ItemSlot.Ring | ItemSlot.Ring2;
                case ItemClass.Amulet:
                    return ItemSlot.Amulet;
                case ItemClass.Quiver:
                case ItemClass.Shield:
                    return ItemSlot.OffHand;
                case ItemClass.Boots:
                    return ItemSlot.Boots;
                case ItemClass.BodyArmour:
                    return ItemSlot.BodyArmour;
                case ItemClass.Gloves:
                    return ItemSlot.Gloves;
                case ItemClass.Helmet:
                    return ItemSlot.Helm;
                case ItemClass.ActiveSkillGem:
                case ItemClass.SupportSkillGem:
                    return ItemSlot.Gem;
                case ItemClass.LifeFlask:
                case ItemClass.ManaFlask:
                case ItemClass.HybridFlask:
                case ItemClass.UtilityFlask:
                case ItemClass.CriticalUtilityFlask:
                case ItemClass.Jewel:
                case ItemClass.Unknown:
                case ItemClass.Any:
                    return ItemSlot.Unequipable;
                default:
                    throw new ArgumentOutOfRangeException(nameof(itemClass), itemClass, null);
            }
        }
    }
}