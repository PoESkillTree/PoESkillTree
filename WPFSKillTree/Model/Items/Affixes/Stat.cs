using System;
using System.Collections.Generic;
using System.Linq;
using MB.Algodat;
using POESKillTree.Model.Items.Enums;
using POESKillTree.Utils.Extensions;

namespace POESKillTree.Model.Items.Affixes
{
	using CSharpGlobalCode.GlobalCode_ExperimentalCode;
    public class Stat : IEquatable<Stat>
    {
        public string Name { get; }
#if (PoESkillTree_UseSmallDec_ForAttributes && PoESkillTree_UseSmallDec_ForGeneratorBars)
        public IReadOnlyList<Range<SmallDec>> Ranges { get; }
#else
		public IReadOnlyList<Range<float>> Ranges { get; }
#endif
        private readonly ItemModTier _parentTier;

        private readonly ItemType _itemType;

#if (PoESkillTree_UseSmallDec_ForAttributes && PoESkillTree_UseSmallDec_ForGeneratorBars)
		public Stat(string name, Range<SmallDec> range, ItemType itemType, ItemModTier parentTier)
#else
		public Stat(string name, Range<float> range, ItemType itemType, ItemModTier parentTier)
#endif
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
#if (PoESkillTree_UseSmallDec_ForAttributes && PoESkillTree_UseSmallDec_ForGeneratorBars)
            var ranges = new List<Range<SmallDec>>();
#else
			var ranges = new List<Range<float>>();
#endif
            for (var i = 0; i < xmlStat.From.Count; i++)
            {
                var from = xmlStat.From[i];
                var to = xmlStat.To[i];
				// RangeTrees don't like from > to.
#if (PoESkillTree_UseSmallDec_ForAttributes && PoESkillTree_UseSmallDec_ForGeneratorBars)
                ranges.Add(Range.Create(SmallDec.Min(from, to), SmallDec.Max(from, to)));
#elif (PoESkillTree_UseSmallDec_ForAttributes)
                ranges.Add(Range.Create((float)SmallDec.Min(from, to), (float)SmallDec.Max(from, to)));
#else
				ranges.Add(Range.Create(Math.Min(from, to), Math.Max(from, to)));
#endif
            }
            Ranges = ranges;
            _parentTier = parentTier;
            _itemType = itemType;
        }

#if (PoESkillTree_UseSmallDec_ForAttributes && PoESkillTree_UseSmallDec_ForGeneratorBars)
		public ItemMod ToItemMod(IReadOnlyList<SmallDec> values)
		{
			if (values.Count != Ranges.Count)
				throw new ArgumentException("There must be one value for each range");
			return ToItemMod(Name, values.ToList());
		}
#else
		public ItemMod ToItemMod(IReadOnlyList<float> values)
        {
            if (values.Count != Ranges.Count)
                throw new ArgumentException("There must be one value for each range");
            return ToItemMod(Name, values.ToList());
        }
#endif

		public ItemMod AsPropertyToItemMod()
        {
            if (Ranges.Count != 1)
            {
                throw new InvalidOperationException(
                    "Only stats with a single range can be converted to ItemMods as properties");
            }

            var range = Ranges[0];
#if (PoESkillTree_UseSmallDec_ForAttributes && PoESkillTree_UseSmallDec_ForGeneratorBars)
            var values = new List<SmallDec> { range.From };
#else
			var values = new List<float> { range.From };
#endif
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

#if (PoESkillTree_UseSmallDec_ForAttributes && PoESkillTree_UseSmallDec_ForGeneratorBars)
		private ItemMod ToItemMod(string attribute, List<SmallDec> values)
#else
		private ItemMod ToItemMod(string attribute, List<float> values)
#endif
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
            return new ItemMod(_itemType, attr, _parentTier)
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
            int h = 23;
            h = h * 37 + Name.GetHashCode();
            return Ranges.Aggregate(h, (a, r) => a * 37 + r.GetHashCode());
        }
    }
}