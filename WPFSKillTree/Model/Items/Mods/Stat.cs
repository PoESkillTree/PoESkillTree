using System;
using System.Collections.Generic;
using System.Diagnostics;
using MB.Algodat;
using POESKillTree.Utils.Extensions;

namespace POESKillTree.Model.Items.Mods
{
    public interface IStat
    {
        // todo everything needed in Affix for sliders
        Range<int> Range { get; }
    }

    [DebuggerDisplay("{" + nameof(Id) + "}")]
    public class Stat : IStat
    {
        public string Id { get; }
        public Range<int> Range { get; }

        public Stat(JsonStat jsonStat)
        {
            Id = jsonStat.Id;
            var min = jsonStat.Min;
            var max = jsonStat.Max;
            Range = new Range<int>(Math.Min(min, max), Math.Max(min, max));
        }
    }

    public class PreTranslatedStat : IStat
    {
        private readonly string _name;
        public Range<int> Range { get; }

        public PreTranslatedStat(string name, int from, int to)
        {
            _name = name;
            Range = new Range<int>(from, to);
        }

        public ItemMod ToItemMod(int value)
        {
            return new ItemMod(_name, true, 0, new float[] {value}, new[] {ItemMod.ValueColoring.White });
        }
    }

    public class Property
    {
        private readonly string _name;
        private readonly float _from;
        private readonly float _to;

        public Property(XmlProperty xmlProperty)
        {
            _name = xmlProperty.Name;
            _from = xmlProperty.From;
            _to = xmlProperty.To;
        }

        protected Property(string name, int from, int to)
        {
            _name = name;
            _from = from;
            _to = to;
        }

        public ItemMod ToItemMod()
        {
            var values = new List<float> { _from };
            var valueColors = new List<ItemMod.ValueColoring> { ItemMod.ValueColoring.White };
            if (!_from.AlmostEquals(_to, 1e-5))
            {
                values.Add(_to);
                valueColors.Add(ItemMod.ValueColoring.White);
            }

            string attribute;
            if (_name.EndsWith(" %"))
            {
                attribute = _name.Substring(0, _name.Length - 2) + ": #%";
            }
            else
            {
                attribute = _name + ": #";
            }
            if (values.Count > 1)
            {
                attribute = attribute.Replace("#", "#-#");
            }

            return new ItemMod(attribute, true, 0, values, valueColors);
        }
    }
}