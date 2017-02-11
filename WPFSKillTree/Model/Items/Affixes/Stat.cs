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

        private readonly ItemModTier _parentTier;

        private readonly ItemType _itemType;

        public Stat(string name, Range<float> range, ItemType itemType, ItemModTier parentTier)
        {
            Name = name;
            Ranges = new[] { range };
            _parentTier = parentTier;
            _itemType = itemType;
        }

        public Stat(Stat stat, ItemModTier parentTier)
        {
            Name = stat.Name;
            Ranges = stat.Ranges;
            _itemType = stat._itemType;
            _parentTier = parentTier;
        }

        public Stat(XmlStat xmlStat, ItemType itemType, ItemModTier parentTier = null)
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
            _parentTier = parentTier;
            _itemType = itemType;
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
            return new ItemMod(_itemType, attribute, _parentTier)
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

            if (Name != other.Name)
                return false;
            if (Ranges.Count != other.Ranges.Count)
                return false;

            return Ranges.Zip(other.Ranges, (x, y) => x.CompareTo(y) == 0).All(b => b);
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as Stat);
        }

        public override int GetHashCode()
        {
            var h = 23;
            h = h * 37 + Name.GetHashCode();
            return Ranges.Aggregate(h, (a, r) => a * 37 + r.GetHashCode());
        }
    }
}