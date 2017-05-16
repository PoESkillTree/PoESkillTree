using System;
using POESKillTree.Model.Items.Enums;

namespace POESKillTree.Model.Items.Mods
{
    public static class StatLocalityChecker
    {
        public static bool DetermineLocal(ItemClass itemclass, ModLocation location, string attr)
        {
            if (location == ModLocation.Property || location == ModLocation.Requirement)
            {
                // local or not doesn't really apply to properties and requirements
                return true;
            }

            if (attr == "#% reduced Attribute Requirements"
                || attr.Contains("+# to Level of Socketed "))
            {
                return true;
            }
            if (attr == "+#% Chance to Block")
            {
                // Chance to Block is only local on shields.
                return itemclass == ItemClass.Shield;
            }
            switch (itemclass)
            {
                case ItemClass.Belt:
                case ItemClass.Ring:
                case ItemClass.Amulet:
                case ItemClass.Quiver:
                case ItemClass.Jewel:
                case ItemClass.ActiveSkillGem:
                case ItemClass.SupportSkillGem:
                    // These item classes have no local mods.
                    return false;
                case ItemClass.OneHandSword:
                case ItemClass.ThrustingOneHandSword:
                case ItemClass.OneHandAxe:
                case ItemClass.OneHandMace:
                case ItemClass.Sceptre:
                case ItemClass.Dagger:
                case ItemClass.Claw:
                case ItemClass.Wand:
                case ItemClass.FishingRod:
                case ItemClass.TwoHandSword:
                case ItemClass.TwoHandAxe:
                case ItemClass.TwoHandMace:
                case ItemClass.Bow:
                case ItemClass.Staff:
                    return DetermineWeaponLocal(location, attr);
                case ItemClass.Shield:
                case ItemClass.Boots:
                case ItemClass.BodyArmour:
                case ItemClass.Gloves:
                case ItemClass.Helmet:
                    return DetermineArmourLocal(attr);
                case ItemClass.LifeFlask:
                case ItemClass.ManaFlask:
                case ItemClass.HybridFlask:
                case ItemClass.UtilityFlask:
                case ItemClass.CriticalUtilityFlask:
                case ItemClass.Unknown:
                case ItemClass.Any:
                    // These item classes are not supported.
                    return false;
                default:
                    throw new ArgumentOutOfRangeException(nameof(itemclass), itemclass, "Unsupported item class");
            }
        }

        private static bool DetermineWeaponLocal(ModLocation location, string attr)
        {
            if (attr == "#% increased Physical Damage")
            {
                // Implicit increased physical damage is global
                return location != ModLocation.Implicit;
            }
            if (attr == "+# to Accuracy Rating")
            {
                // Crafted accuracy is global
                return location != ModLocation.Crafted;
            }
            if (attr.StartsWith("Adds ") 
                && (attr.EndsWith(" Damage") || attr.EndsWith(" Damage in Main Hand") || attr.EndsWith(" Damage in Off Hand")))
            {
                return true;
            }
            return attr == "#% increased Attack Speed"
                   || attr == "#% increased Accuracy Rating"
                   || attr == "#% increased Critical Strike Chance"
                   || attr.Contains("Damage Leeched as")
                   || attr.Contains("Critical Strike Chance with this Weapon")
                   || attr.Contains("Critical Strike Damage Multiplier with this Weapon")
                   || attr == "+# to Weapon range";
        }

        private static bool DetermineArmourLocal(string attr)
        {
            return (attr.Contains("Armour") && !attr.EndsWith("Armour against Projectiles"))
                   || attr.Contains("Evasion Rating")
                   || (attr.Contains("Energy Shield") && !attr.EndsWith("Energy Shield Recharge"));
        }
    }
}