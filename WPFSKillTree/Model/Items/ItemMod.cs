using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using Newtonsoft.Json.Linq;

namespace POESKillTree.Model.Items
{
    public class ItemMod
    {
        public enum ValueColoring
        {
            White = 0,
            LocallyAffected = 1,

            Fire = 4,
            Cold = 5,
            Lightning = 6,
            Chaos = 7
        }


        public Stat Parent { get; set; }

        public string Attribute { get; set; }

        public List<float> Value { get; set; }

        public List<ValueColoring> ValueColor { get; set; }

        public bool IsLocal { get; private set; }

        private ItemClass _itemclass;

        public static ItemMod CreateMod(Item item, JObject obj, Regex numberfilter)
        {
            int dmode = (obj["displayMode"] != null) ? obj["displayMode"].Value<int>() : 0;
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
                    float val;
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

                var cols = floats.Select(f => (ValueColoring)((JArray)a)[1].Value<int>()).ToList();
                return new { floats, cols };
            }).ToList();

            return new ItemMod
            {
                _itemclass = item.Class,
                Value = parsed.Select(p => p.floats).SelectMany(v => v).ToList(),
                ValueColor = parsed.Select(p => p.cols).SelectMany(v => v).ToList(),
                Attribute = at,
                IsLocal = DetermineLocal(item, at)
            };
        }

        public static ItemMod CreateMod(Item item, string attribute, Regex numberfilter)
        {
            var values = new List<float>();
            foreach (Match match in numberfilter.Matches(attribute))
            {
                values.Add(float.Parse(match.Value, CultureInfo.InvariantCulture));
            }
            string at = numberfilter.Replace(attribute, "#");

            return new ItemMod
            {
                _itemclass = item.Class,
                Value = values,
                Attribute = at,
                IsLocal = DetermineLocal(item, at)
            };
        }

        public static IEnumerable<ItemMod> CreateMods(Item item, string attribute, Regex numberfilter)
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
                    _itemclass = ic,
                    Value = values,
                    Attribute = "+# to Strength"
                });
                mods.Add(new ItemMod
                {
                    _itemclass = ic,
                    Value = values,
                    Attribute = "+# to Dexterity"
                });
                mods.Add(new ItemMod
                {
                    _itemclass = ic,
                    Value = values,
                    Attribute = "+# to Intelligence"
                });
            }
            else
            {
                mods.Add(new ItemMod
                {
                    _itemclass = ic,
                    Value = values,
                    Attribute = at,
                    IsLocal = DetermineLocal(item, at)
                });
            }
            return mods;
        }

        public bool DetermineLocalFor(Item itm)
        {
            return DetermineLocal(itm, Attribute);
        }

        // Returns true if property/mod is local, false otherwise.
        private static bool DetermineLocal(Item item, string attr)
        {
            return item.ItemGroup != ItemGroup.Amulet && item.ItemGroup != ItemGroup.Ring &&
                    item.ItemGroup != ItemGroup.Belt
                   && ((attr.Contains("Armour") && !attr.EndsWith("Armour against Projectiles"))
                       || attr.Contains("Evasion")
                       || (attr.Contains("Energy Shield") && !attr.EndsWith("Energy Shield Recharge"))
                       || attr.Contains("Weapon Class")
                       || attr.Contains("Critical Strike Chance with this Weapon")
                       || attr.Contains("Critical Strike Damage Multiplier with this Weapon"))
                   || (item.ItemGroup == ItemGroup.OneHandedWeapon || item.ItemGroup == ItemGroup.TwoHandedWeapon)
                      && (attr == "#% increased Attack Speed"
                          || attr == "#% increased Accuracy Rating"
                          || attr == "+# to Accuracy Rating"
                          || attr.StartsWith("Adds ") && (attr.EndsWith(" Damage") || attr.EndsWith(" Damage in Main Hand") || attr.EndsWith(" Damage in Off Hand"))
                          || attr == "#% increased Physical Damage"
                          || attr == "#% increased Critical Strike Chance")
                   || (item.ItemGroup == ItemGroup.Shield && attr == "+#% Chance to Block");
        }

        public ItemMod()
        {
            ValueColor = new List<ValueColoring>();
        }

        public ItemMod Sum(ItemMod m)
        {
            return new ItemMod
            {
                Attribute = Attribute,
                _itemclass = _itemclass,
                IsLocal = IsLocal,
                Parent = Parent,
                ValueColor = ValueColor.ToList(),
                Value = Value.Zip(m.Value, (f1, f2) => f1 + f2).ToList()
            };
        }

        public JToken ToJobject(bool asMod = false)
        {
            string defaultFormat = "###0.##";
            if (asMod)
            {
                int index = 0;
                return new JValue(ItemAttributes.Attribute.Backreplace.Replace(Attribute, m => Value[index++].ToString(defaultFormat)));
            }
            var j = new JObject();

            if (Value != null && Value.Count > 2)
                j.Add("displayMode", 3);
            else
                j.Add("displayMode", 0);



            if (Value == null || Value.Count == 0)
            {
                j.Add("name", Attribute);
                j.Add("values", new JArray());
            }
            else if (Value.Count == 1)
            {
                if (Attribute.EndsWith(": #"))
                {
                    j.Add("name", Attribute.Substring(0, Attribute.Length - 3));
                    j.Add("values", new JArray((object)new JArray(Value[0], ValueColor[0])));
                }
                else if (Attribute.EndsWith(" #%"))
                {
                    j.Add("name", Attribute.Substring(0, Attribute.Length - 3));
                    j.Add("values", new JArray((object)new JArray(Value[0] + "%", ValueColor[0])));
                }
                else if (Attribute.StartsWith("# "))
                {
                    j.Add("name", Attribute.Substring(2));
                    j.Add("values", new JArray((object)new JArray(Value[0].ToString(defaultFormat), ValueColor[0])));
                }
                else
                    throw new NotSupportedException();

            }
            else if (Value.Count == 2)
            {
                if (Attribute.EndsWith(": #-#") && ValueColor.All(v => v == ValueColor[0]))
                {
                    j.Add("name", Attribute.Substring(0, Attribute.Length - 5));
                    j.Add("values", new JArray((object)new JArray(string.Join("-", Value), ValueColor[0])));
                }
                else
                    throw new NotSupportedException();
            }
            else
            {
                var str = Attribute;
                while (str.EndsWith(" #-#"))
                {
                    str = str.Substring(0, str.Length - 4);
                    str = str.Trim(',', ':', ' ');
                }
                j.Add("name", str);

                JArray vals = new JArray();
                for (int i = 0; i < Value.Count; i += 2)
                {
                    var val = string.Format("{0}-{1}", Value[i], Value[i + 1]);

                    if (ValueColor[i] != ValueColor[i + 1])
                        throw new NotSupportedException();
                    vals.Add(new JArray(val, ValueColor[i]));
                }

                j.Add("values", vals);

            }

            return j;
        }

    }
}