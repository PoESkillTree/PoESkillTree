using System;
using System.Linq;
using MB.Algodat;

namespace POESKillTree.Model.Items
{
    public class Stat
    {
        public string Name { get; private set; }

        public Range<float> Range { get; private set; }

        public ItemModTier ParentTier { get; set; }

        public Stat(string name, Range<float> range)
        {
            Name = name;
            Range = range;
        }

        public Stat(XmlStat xmlStat)
        {
            Name = xmlStat.Name;
            // RangeTrees don't like if from > to.
            Range = new Range<float>(Math.Min(xmlStat.From, xmlStat.To), Math.Max(xmlStat.From, xmlStat.To));
        }

        public ItemMod ToItemMod(params float[] values)
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
            if (Name.Contains("#"))
            {
                attribute = Name;
            }
            else if (char.IsUpper(Name[0]))
            {
                if (Name.Last() == '%')
                    attribute = Name.Substring(0, Name.Length - 1) + ": #%";
                else
                    attribute = Name + ": #";
            }
            else
            {
                throw new NotSupportedException();
            }
            return new ItemMod
            {
                Parent = this,
                Value = values.ToList(),
                ValueColor = values.Select(_ => ItemMod.ValueColoring.White).ToList(),
                Attribute = values.Length == 1 ? attribute : attribute.Replace("#", "#-#")
            };
        }
    }
}