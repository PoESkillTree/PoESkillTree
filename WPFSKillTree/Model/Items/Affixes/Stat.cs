using System;
using System.Linq;
using MB.Algodat;
using POESKillTree.Model.Items.Enums;

namespace POESKillTree.Model.Items.Affixes
{
    public class Stat
    {
        public string Name { get; private set; }

        public Range<float> Range { get; private set; }

        public ItemModTier ParentTier { get; private set; }

        public ItemType ItemType { get; private set; }

        public Stat(string name, Range<float> range, ItemType itemType, ItemModTier parentTier)
        {
            Name = name;
            Range = range;
            ParentTier = parentTier;
            ItemType = itemType;
        }

        public Stat(XmlStat xmlStat, ItemType itemType, ItemModTier parentTier = null)
        {
            Name = xmlStat.Name;
            // RangeTrees don't like from > to.
            // todo
            Range = new Range<float>(Math.Min(xmlStat.From[0], xmlStat.To[0]), Math.Max(xmlStat.From[0], xmlStat.To[0]));
            ParentTier = parentTier;
            ItemType = itemType;
        }

        public ItemMod ToItemMod(bool isProperty, params float[] values)
        {
            if (values.Length != 0 && values.All(v => Math.Abs(v - values[0]) < 1e-5))
            {
                values = new[] { values[0] };
            }
            else if (values.Length == 0)
            {
                values = Math.Abs(Range.From - Range.To) < 1e-5 ? new[] { Range.To } : new[] { Range.From, Range.To };
            }

            if (values.Length == 0 || values.Length > 2)
                throw new ArgumentException();

            string attribute;
            if (isProperty)
            {
                if (Name.EndsWith(" %"))
                    attribute = Name.Substring(0, Name.Length - 2) + ": #%";
                else
                    attribute = Name + ": #";
            }
            else
            {
                attribute = Name;
            }
            var rangedToken = isProperty ? "#-#" : "# to #";
            return new ItemMod(ItemType, values.Length == 1 ? attribute : attribute.Replace("#", rangedToken), this)
            {
                Value = values.ToList(),
                ValueColor = values.Select(_ => ItemMod.ValueColoring.White).ToList()
            };
        }
    }
}