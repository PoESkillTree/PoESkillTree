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

        public ItemMod(Item item, string attribute, Regex numberfilter, IEnumerable<ValueColoring> valueColor = null)
        {
            Value = (from Match match in numberfilter.Matches(attribute)
                     select float.Parse(match.Value, CultureInfo.InvariantCulture))
                     .ToList();
            Attribute = numberfilter.Replace(attribute, "#"); ;
            IsLocal = DetermineLocal(item, Attribute);
            ValueColor = valueColor == null ? new List<ValueColoring>() : new List<ValueColoring>(valueColor);
        }

        public ItemMod()
        {
            ValueColor = new List<ValueColoring>();
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

        public ItemMod Sum(ItemMod m)
        {
            return new ItemMod
            {
                Attribute = Attribute,
                IsLocal = IsLocal,
                Parent = Parent,
                ValueColor = ValueColor.ToList(),
                Value = Value.Zip(m.Value, (f1, f2) => f1 + f2).ToList()
            };
        }

        private string InsertValues(string into, ref int index)
        {
            var indexCopy = index;
            var result = Regex.Replace(into, "#",
                m => Value[indexCopy++].ToString("###0.##", CultureInfo.InvariantCulture));
            index = indexCopy;
            return result;
        }

        private JArray[] ValueTokensToJArrays(IEnumerable<string> tokens)
        {
            var valueIndex = 0;
            return tokens.Select(t => new JArray(InsertValues(t, ref valueIndex), ValueColor[valueIndex - 1])).ToArray();
        }

        public JToken ToJobject(bool asMod = false)
        {
            if (asMod)
            {
                var index = 0;
                return new JValue(InsertValues(Attribute, ref index));
            }

            const string allowedTokens = @"(#|#%|\+#%|#-#|#/#)";
            string name;
            var tokens = new List<string>();
            int displayMode;
            if (Value == null || Value.Count == 0)
            {
                name = Attribute;
                displayMode = 0;
            }
            else if (Regex.IsMatch(Attribute, @"^[^#]*: (" + allowedTokens + @"(, |$))+"))
            {
                // displayMode 0 is for the form `Attribute = name + ": " + values.Join(", ")`
                name = Regex.Replace(Attribute, @"(: |, )" + allowedTokens + @"(?=, |$)", m =>
                {
                    tokens.Add(m.Value.TrimStart(',', ':', ' '));
                    return "";
                });
                displayMode = 0;
            }
            else
            {
                // displayMode 3 is for the form `Attribute = name.Replace("%i" with values[i])`
                var matchIndex = 0;
                name = Regex.Replace(Attribute, @"(?<=^|\s)" + allowedTokens + @"(?=$|\s|,)", m =>
                {
                    tokens.Add(m.Value);
                    return "%" + matchIndex++;
                });
                displayMode = 3;
            }
            return new JObject
            {
                {"name", name},
                {"values", new JArray(ValueTokensToJArrays(tokens))},
                {"displayMode", displayMode}
            };
        }

    }
}