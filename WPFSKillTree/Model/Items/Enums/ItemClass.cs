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
        ThrustingOneHandSword,
        OneHandAxe,
        OneHandMace,
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
    }
}