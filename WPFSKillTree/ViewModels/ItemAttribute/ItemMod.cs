using System.Collections.Generic;
using System.Globalization;
using System.Text.RegularExpressions;

namespace POESKillTree.ViewModels.ItemAttribute
{
    public class ItemMod
    {
        public string Attribute;
        public List<float> Value;
        public bool isLocal = false;
        private Item.ItemClass itemclass;

        public static List<ItemMod> CreateMods(Item item, string attribute, Regex numberfilter)
        {
            Item.ItemClass ic = item.Class;
            var mods = new List<ItemMod>();
            var values = new List<float>();
            foreach (Match match in numberfilter.Matches(attribute))
            {
                values.Add(float.Parse(match.Value, CultureInfo.InvariantCulture));
            }
            string at = numberfilter.Replace(attribute, "#");
            if (at == "+# to all Attributes")
            {
                mods.Add(new ItemMod
                {
                    itemclass = ic,
                    Value = values,
                    Attribute = "+# to Strength"
                });
                mods.Add(new ItemMod
                {
                    itemclass = ic,
                    Value = values,
                    Attribute = "+# to Dexterity"
                });
                mods.Add(new ItemMod
                {
                    itemclass = ic,
                    Value = values,
                    Attribute = "+# to Intelligence"
                });
            }
            else
            {
                mods.Add(new ItemMod
                {
                    itemclass = ic,
                    Value = values,
                    Attribute = at,
                    isLocal = DetermineLocal(item, at)
                });
            }
            return mods;
        }

        // Returns true if property/mod is local, false otherwise.
        private static bool DetermineLocal(Item item, string attr)
        {
            return (item.Class != Item.ItemClass.Amulet && item.Class != Item.ItemClass.Ring &&
                    item.Class != Item.ItemClass.Belt)
                   && ((attr.Contains("Armour") && !attr.EndsWith("Armour against Projectiles"))
                       || attr.Contains("Evasion")
                       || (attr.Contains("Energy Shield") && !attr.EndsWith("Energy Shield Recharge"))
                       || attr.Contains("Weapon Class")
                       || attr.Contains("Critical Strike Chance with this Weapon")
                       || attr.Contains("Critical Strike Damage Multiplier with this Weapon"))
                   || (item.Class == Item.ItemClass.MainHand || item.Class == Item.ItemClass.OffHand)
                   && item.Keywords != null // Only weapons have keyword.
                   && (attr == "#% increased Attack Speed"
                       || attr == "#% increased Accuracy Rating"
                       || attr == "+# to Accuracy Rating"
                       ||
                       attr.StartsWith("Adds ") &&
                       (attr.EndsWith(" Damage") || attr.EndsWith(" Damage in Main Hand") ||
                        attr.EndsWith(" Damage in Off Hand"))
                       || attr == "#% increased Physical Damage");
        }

        private enum ValueType
        {
            Flat,
            Percentage,
            FlatMinMax
        }
    }
}
