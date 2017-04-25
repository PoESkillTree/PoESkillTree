using System;
using System.Collections.Generic;
using System.Linq;
using MB.Algodat;
using POESKillTree.Model.Items.Enums;
using POESKillTree.Utils.Extensions;

namespace POESKillTree.Model.Items.Affixes
{
    public class Stat : IEquatable<Stat>
    {
        public string Name { get; }

        public IReadOnlyList<Range<float>> Ranges { get; }

        private readonly ItemType _itemType;

        private readonly ModGroup _modGroup;
        private readonly int _level;

        public Stat(Stat stat)
        {
            Name = stat.Name;
            Ranges = stat.Ranges;
            _itemType = stat._itemType;
            _modGroup = stat._modGroup;
            _level = stat._level;
        }

        public Stat(XmlStat xmlStat, ItemType itemType, ModGroup modGroup, int level = 0)
        {
            Name = xmlStat.Name;
            var ranges = new List<Range<float>>();
            for (var i = 0; i < xmlStat.From.Count; i++)
            {
                var from = xmlStat.From[i];
                var to = xmlStat.To[i];
                // RangeTrees don't like from > to.
                ranges.Add(Range.Create(Math.Min(from, to), Math.Max(from, to)));
            }
            Ranges = ranges;
            _itemType = itemType;
            _modGroup = modGroup;
            _level = level;
        }

        private Stat(string name, ItemType itemType, ModGroup modGroup, Range<float> range)
        {
            Name = name;
            _itemType = itemType;
            _modGroup = modGroup;
            Ranges = new[] { range };
        }

        public static Stat CreateProperty(string name, Range<float> range)
        {
            return new Stat(name, ItemType.Unknown, ModGroup.Property, range);
        }

        public ItemMod ToItemMod(IReadOnlyList<float> values)
        {
            if (values.Count != Ranges.Count)
                throw new ArgumentException("There must be one value for each range");
            return ToItemMod(Name, values.ToList());
        }

        public ItemMod AsPropertyToItemMod()
        {
            if (Ranges.Count != 1)
            {
                throw new InvalidOperationException(
                    "Only stats with a single range can be converted to ItemMods as properties");
            }

            var range = Ranges[0];
            var values = new List<float> { range.From };
            if (!range.From.AlmostEquals(range.To, 1e-5))
            {
                values.Add(range.To);
            }

            string attribute;
            if (Name.EndsWith(" %"))
                attribute = Name.Substring(0, Name.Length - 2) + ": #%";
            else
                attribute = Name + ": #";
            if (values.Count > 1)
            {
                attribute = attribute.Replace("#", "#-#");
            }

            return ToItemMod(attribute, values);
        }

        private ItemMod ToItemMod(string attribute, List<float> values)
        {
            // replace "+#" by "#" if the value for that placeholder is negative
            var attr = "";
            var parts = attribute.Split('#');
            for (var i = 0; i < values.Count; i++)
            {
                var part = parts[i];
                if (part.EndsWith("+") && values[i] < 0)
                {
                    attr += part.Substring(0, part.Length - 1);
                }
                else
                {
                    attr += part;
                }
                attr += "#";
            }
            attr += parts.Last();
            return new ItemMod(_itemType, attr, _modGroup, _level)
            {
                Value = values,
                ValueColor = values.Select(_ => ItemMod.ValueColoring.White).ToList()
            };
        }

        public override string ToString()
        {
            return Name + " {" + string.Join(", ", Ranges) + "}";
        }

        public bool Equals(Stat other)
        {
            if (ReferenceEquals(this, other))
                return true;
            if (ReferenceEquals(null, other))
                return false;

            if (Name != other.Name
                || _level != other._level
                || _modGroup != other._modGroup
                || Ranges.Count != other.Ranges.Count)
                return false;

            return Ranges.Zip(other.Ranges, (x, y) => x.CompareTo(y) == 0).All(b => b);
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as Stat);
        }

        public override int GetHashCode()
        {
            int h = 23;
            h = h * 37 + Name.GetHashCode();
            h = h * 37 + _level.GetHashCode();
            h = h * 37 + _modGroup.GetHashCode();
            return Ranges.Aggregate(h, (a, r) => a * 37 + r.GetHashCode());
        }
    }
}