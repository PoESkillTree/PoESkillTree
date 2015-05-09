using System.Collections.Generic;
using System.Globalization;
using System.Text.RegularExpressions;
using System.Linq;
using POESKillTree.ViewModels.Items;
using Newtonsoft.Json.Linq;

namespace POESKillTree.ViewModels.Items
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
        private ItemClass itemclass;

        public static ItemMod CreateMod(Item item, JObject obj, Regex numberfilter)
        {
            ItemClass ic = item.Class;
            var mod = new ItemMod();

            int dmode = (obj["displayMode"]!=null) ? obj["displayMode"].Value<int>() : 0;
            string at = obj["name"].Value<string>();
            at = numberfilter.Replace(at, "#");

            var parsed = ((JArray)obj["values"]).Select(a =>
            {
                var str = ((JArray)a)[0].Value<string>();
                var floats = new List<float>();
                var parts = str.Split('-');

                if (dmode != 3)
                    if (parts.Length > 1)
                        at += ": ";
                    else
                        at += " ";

                for (int i = 0; i < parts.Length; i++)
                {
                    string v = parts[i];
                    float val = 0;
                    if (float.TryParse(v, NumberStyles.Float, CultureInfo.InvariantCulture, out val))
                    {
                        floats.Add(val);
                        if (dmode != 3)
                            at += "#";
                    }
                    else
                    {
                        foreach (Match m in numberfilter.Matches(v))
                            floats.Add(float.Parse(m.Value, CultureInfo.InvariantCulture));

                        at += " " + numberfilter.Replace(v, "#");
                    }

                    if (i < parts.Length - 1)
                    {
                        if (dmode != 3)
                            at += "-";
                    }
                }

                var cols = floats.Select(f => (ItemMod.ValueColoring)((JArray)a)[1].Value<int>()).ToList();
                return new { floats, cols };
            }).ToList();


            mod = new ItemMod
            {
                itemclass = ic,
                Value = parsed.Select(p => p.floats).SelectMany(v => v).ToList(),
                ValueColor = parsed.Select(p => p.cols).SelectMany(v => v).ToList(),
                _Attribute = at,
                isLocal = DetermineLocal(item, at)
            };

            return mod;
        }

        public static ItemMod CreateMod(Item item, string attribute, Regex numberfilter)
        {
            ItemClass ic = item.Class;
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
            ItemClass ic = item.Class;
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
            return (item.Class != ItemClass.Amulet && item.Class != ItemClass.Ring &&
                    item.Class != ItemClass.Belt)
                   && ((attr.Contains("Armour") && !attr.EndsWith("Armour against Projectiles"))
                       || attr.Contains("Evasion")
                       || (attr.Contains("Energy Shield") && !attr.EndsWith("Energy Shield Recharge"))
                       || attr.Contains("Weapon Class")
                       || attr.Contains("Critical Strike Chance with this Weapon")
                       || attr.Contains("Critical Strike Damage Multiplier with this Weapon"))
                   || (item.Class == ItemClass.MainHand || item.Class == ItemClass.OffHand)
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
