using MB.Algodat;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;

namespace POESKillTree.ViewModels.Items
{
    public class Stat
    {
        private string p;
        private string armour;

        public string Name { get; set; }
        public string NameAlias
        {
            get
            {
                if (this.ParentTier == null)
                    return Name;

                else
                {
                    return ParentTier.ParentAffix.Dealias(this.Name);

                }
                throw new NotImplementedException();
            }
        }




        public Range<float> Range { get; set; }

        public ItemModTier ParentTier { get; set; }

        public Stat(string name, Range<float> range)
        {
            this.Name = name;
            this.Range = range;
        }

        public Stat(XElement e)
        {
            if (e.Name != "stat")
                throw new ArgumentException();

            this.Name = e.Attribute("name").Value;

            this.Range = new Range<float>(XmlConvert.ToSingle(e.Attribute("from").Value), XmlConvert.ToSingle(e.Attribute("to").Value));
        }

        public Stat(string name, string value)
        {
            Name = name;
            Range = Range<float>.Parse(value);
        }

        public XElement Serialize()
        {
            return new XElement("stat", new XAttribute("name", this.Name), new XAttribute("from", this.Range.From), new XAttribute("to", this.Range.To));
        }


        public ItemMod ToItemMod(params float[] values)
        {
            if (values.Length != 0 && values.All(v => v == values[0]))
            {
                values = new float[] { values[0] };
            }
            else if (values.Length == 0)
            {
                if (Range.From == Range.To)
                    values = new float[] { Range.To };
                else
                    values = new float[] { Range.From, Range.To };
            }

            if (values.Length == 0 || values.Length > 2)
                throw new NotImplementedException();

            bool singleVal = values.Length == 1;

            string modval = singleVal ? "#" : "#-#";

            var im = new ItemMod(){Parent = this};

            string name = this.NameAlias;


            int insetpos = name.IndexOf('%');
            if (insetpos < 0)
            {
                insetpos = name.IndexOf("  ");
                if (insetpos >= 0)
                    insetpos++;
                else
                {
                    insetpos = name.IndexOf(" - ");
                    if (insetpos >= 0)
                    {
                        name = name.Replace(" - ", "  ");
                        insetpos++;
                    }
                }
            }

            if (insetpos >= 0)
            {
                im.Attribute = name.Insert(insetpos, modval);
            }
            else if (name[0] == ' ' || Char.IsLower(name[0]))
            {
                im.Attribute = modval + name;
            }
            else if (Char.IsUpper(Name[0])) //value at end
            {
                im.Attribute = name + ": " + modval;
            }
            else
                throw new NotImplementedException();

            im.Value = values.ToList();
            im.ValueColor = values.Select(_ => ItemMod.ValueColoring.White).ToList();

            return im;
        }
    }

    public class ItemModTier : IEquatable<ItemModTier>
    {
        public Affix ParentAffix { get; set; }
        public string Name { get; set; }
        public int Level { get; set; }
        public List<Stat> Stats { get; set; }
        public bool IsMasterCrafted { get; set; }
        public int Tier { get; set; }

        public ItemModTier(string name, int level, string stat, Range<float> val)
        {
            this.Name = name;
            this.Level = level;
            this.Stats = new List<Stat>() { new Stat(stat, val) };
        }

        public ItemModTier(string name, int level, IEnumerable<string> stat, IEnumerable<Range<float>> val)
        {
            this.Name = name;
            this.Level = level;
            this.Stats = new List<Stat>(stat.Zip(val, (s, v) => new Stat(s, v)));
        }

        public ItemModTier(string name, int level, IEnumerable<Stat> stats)
        {
            this.Name = name;
            this.Level = level;
            this.Stats = stats.Select(s => new Stat(s.Name, s.Range) {ParentTier = this }).ToList();
        }

        public ItemModTier(XElement e)
        {
            if (e.Name != "mod")
                throw new ArgumentException();

            if (e.Attribute("mastercrafted") != null)
                this.IsMasterCrafted = XmlConvert.ToBoolean(e.Attribute("mastercrafted").Value);

            if (e.Attribute("tier") != null)
                this.Tier = XmlConvert.ToInt32(e.Attribute("tier").Value);

            this.Name = e.Attribute("name").Value;
            this.Level = XmlConvert.ToInt32(e.Attribute("level").Value);
            this.Stats = e.Element("stats").Elements().Select(el => new Stat(el) { ParentTier = this }).ToList();
        }

        public XElement Serialize()
        {
            return new XElement("mod",
                    new XAttribute("name", this.Name),
                    new XAttribute("level", this.Level),
                    new XAttribute("mastercrafted", this.IsMasterCrafted),
                    new XAttribute("tier", this.Tier),
                    new XElement("stats", Stats.Select(s => s.Serialize()))
                );
        }

        public Range<float> Range(string mod)
        {
            return Stats.First(s => mod.Contains(s.Name)).Range;
        }

        public override string ToString()
        {
            return "" + (ParentAffix != null ? ParentAffix.ModType == ModType.Prefix ? "P" : "S" : "") + this.Tier + " " + Name + " - " + string.Join("; ", Stats.Select(s => s.Name + " {" + s.Range + "} "));
        }

        public static readonly Regex ParseRegex = new Regex(@"(?<!\d)[-+]?\d*\.?\d+", RegexOptions.Compiled);

        public static string StripModValues(string mod)
        {
            return ParseRegex.Replace(mod, "");
        }

        public static IEnumerable<string> StripValues(IEnumerable<string> values)
        {
            return values.Select(v => StripModValues(v));
        }

        public static float[] GetModValues(string mod)
        {
            var res = ParseRegex.Matches(mod).Cast<Match>().Select(m => XmlConvert.ToSingle(m.Value)).ToArray();
            if (res.Length == 0) //some mods defauls to 1-1 range ("removes curse on use" ...)
                return new[] { 1f };
            return res;
        }


        public bool Equals(ItemModTier other)
        {
            var x = this;
            var y = other;
            bool equals = x.Level == y.Level && x.Name == y.Name && x.IsMasterCrafted == y.IsMasterCrafted && x.Stats.Zip(y.Stats, (xs, ys) => xs.Name == ys.Name && xs.Range.CompareTo(ys.Range) == 0).All(z => z);
            return equals;
        }

        public override int GetHashCode()
        {
            var obj = this;
            return obj.Level.GetHashCode() ^ obj.Name.GetHashCode() ^ obj.IsMasterCrafted.GetHashCode() ^ obj.Stats.Aggregate(0, (a, s) => a ^ s.Name.GetHashCode() ^ s.Range.GetHashCode());
        }

    }

}
