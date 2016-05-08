using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using MB.Algodat;

namespace POESKillTree.Model.Items
{
    public class ItemModTier : IEquatable<ItemModTier>
    {
        public Affix ParentAffix { get; set; }
        public string Name { get; private set; }
        private int Level { get; set; }
        public IReadOnlyList<Stat> Stats { get; private set; }
        public bool IsMasterCrafted { get; private set; }
        public int Tier { get; private set; }

        public ItemModTier(string name, int level, IEnumerable<Stat> stats)
        {
            Name = name;
            Level = level;
            Stats = stats.Select(s => new Stat(s.Name, s.Range) {ParentTier = this }).ToList();
        }

        public ItemModTier(XmlTier xmlTier)
        {
            IsMasterCrafted = Regex.IsMatch(xmlTier.Name, @" lvl: \d+");
            Tier = xmlTier.Tier;
            Name = xmlTier.Name;
            Level = xmlTier.ItemLevel;
            Stats = xmlTier.Stats.Select(s => new Stat(s)).ToList();
        }

        public Range<float> Range(string mod)
        {
            return Stats.First(s => mod.Contains(s.Name)).Range;
        }

        public override string ToString()
        {
            return "" + (ParentAffix != null ? ParentAffix.ModType == ModType.Prefix ? "P" : "S" : "") + Tier + " " + Name + " - " + string.Join("; ", Stats.Select(s => s.Name + " {" + s.Range + "} "));
        }
        
        public bool Equals(ItemModTier other)
        {
            var x = this;
            var y = other;
            return x.Level == y.Level && x.Name == y.Name && x.IsMasterCrafted == y.IsMasterCrafted
                && x.Stats.Zip(y.Stats, (xs, ys) => xs.Name == ys.Name && xs.Range.CompareTo(ys.Range) == 0).All(z => z);
        }

        public override int GetHashCode()
        {
            var obj = this;
            return obj.Level.GetHashCode() ^ obj.Name.GetHashCode() ^ obj.IsMasterCrafted.GetHashCode()
                ^ obj.Stats.Aggregate(0, (a, s) => a ^ s.Name.GetHashCode() ^ s.Range.GetHashCode());
        }

    }

}
