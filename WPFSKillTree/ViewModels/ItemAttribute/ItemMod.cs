using System.Collections.Generic;
using System.Globalization;
using System.Text.RegularExpressions;

namespace POESKillTree.ViewModels.ItemAttribute
{
    public class ItemMod
    {
        public enum ValueColoring
        {
            White = 0,
            LocallyAffected = 1,

            Fire = 4,
            Cold = 5,
            Lightning = 6
        }

        private string _Attribute;

        public string Attribute
        {
            get { return _Attribute; }
            set { _Attribute = value; }
        }

        public List<float> Value;
        public List<ValueColoring> ValueColor = new List<ValueColoring>();

        public bool isLocal = false;
        private Item.ItemClass itemclass;


        public static ItemMod CreateMod(Item item, string attribute, Regex numberfilter)
        {
            Item.ItemClass ic = item.Class;
            var mod = new ItemMod();
            var values = new List<float>();
            foreach (Match match in numberfilter.Matches(attribute))
            {
                values.Add(float.Parse(match.Value, CultureInfo.InvariantCulture));
            }
            string at = numberfilter.Replace(attribute, "#");

            mod = new ItemMod
            {
                itemclass = ic,
                Value = values,
                _Attribute = at,
                isLocal = DetermineLocal(item, at)
            };

            return mod;
        }

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
                    _Attribute = "+# to Strength"
                });
                mods.Add(new ItemMod
                {
                    itemclass = ic,
                    Value = values,
                    _Attribute = "+# to Dexterity"
                });
                mods.Add(new ItemMod
                {
                    itemclass = ic,
                    Value = values,
                    _Attribute = "+# to Intelligence"
                });
            }
            else
            {
                mods.Add(new ItemMod
                {
                    itemclass = ic,
                    Value = values,
                    _Attribute = at,
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
                          || attr.StartsWith("Adds ") && (attr.EndsWith(" Damage") || attr.EndsWith(" Damage in Main Hand") || attr.EndsWith(" Damage in Off Hand"))
                          || attr == "#% increased Physical Damage"
                          || attr == "#% increased Critical Strike Chance");
        }

        private enum ValueType
        {
            Flat,
            Percentage,
            FlatMinMax
        }
    }
}
