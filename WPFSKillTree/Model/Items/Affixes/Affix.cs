using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using MB.Algodat;
using POESKillTree.Model.Items.Enums;

namespace POESKillTree.Model.Items.Affixes
{
    public class Affix
    {
        public ItemType ItemType { get; }

        public ModType ModType { get; }

        public IReadOnlyList<string> StatNames { get; }

        [UsedImplicitly(ImplicitUseKindFlags.Access)] // accessed in ModSelectorView
        public string Name { get; }

        public IReadOnlyList<IReadOnlyList<Range<float>>> RangesPerTreeAndTier { get; }

        private readonly IReadOnlyList<RangeTree<float, ModWrapper>> _trees;
        private readonly IReadOnlyList<Tuple<int, int>> _treeIndexToStatAndRange;

        private readonly IReadOnlyList<Stat> _firstTierStats;

        public Affix(params ItemModTier[] tiers)
            : this(ItemType.Unknown, tiers)
        {
            Name = string.Join(",", StatNames);
        }

        public Affix(XmlAffix xmlAffix)
            : this(xmlAffix.ItemType, xmlAffix.Tiers.Select(el => new ItemModTier(el, xmlAffix.ItemType)))
        {
            if (!xmlAffix.Tiers.Any())
                throw new NotSupportedException("There should not be any Affix without tiers");
            ModType = xmlAffix.ModType;
            Name = xmlAffix.Name;

        }

        private Affix(ItemType itemType, IEnumerable<ItemModTier> tiers)
        {
            ItemType = itemType;
            var tierList = tiers.ToList();

            if (!tierList.Any())
            {
                StatNames = new string[0];
                _firstTierStats = new Stat[0];
                return;
            }

            var firstTier = tierList[0];
            _firstTierStats = firstTier.Stats;
            if (tierList.Any(t => t.Stats.Count != _firstTierStats.Count))
            {
                throw new NotSupportedException("Tiers must all have the same amount of stats");
            }

            var comparer = new ItemModComparer();
            var mods = new List<string>();
            var trees = new List<RangeTree<float, ModWrapper>>();
            var rangesPerTree = new List<IReadOnlyList<Range<float>>>();
            var treeIndexToIj = new List<Tuple<int, int>>();
            for (var i = 0; i < _firstTierStats.Count; i++)
            {
                var stat = _firstTierStats[i];
                var rangeCount = stat.Ranges.Count;

                if (tierList.Any(t => t.Stats[i].Ranges.Count != rangeCount))
                {
                    throw new NotSupportedException(
                        $"Tiers of stat {stat.Name} must all have the same amount of ranges");
                }

                var name = stat.Name;
                for (var j = 0; j < rangeCount; j++)
                {
                    mods.Add(name);
                    var wrapper = tierList.Select(t => new ModWrapper(t, t.Stats[i].Ranges[j])).ToList();
                    rangesPerTree.Add(wrapper.Select(w => w.Range).ToList());
                    trees.Add(new RangeTree<float, ModWrapper>(wrapper, comparer));
                    treeIndexToIj.Add(Tuple.Create(i, j));
                }
            }
            StatNames = mods;
            _trees = trees;
            RangesPerTreeAndTier = rangesPerTree;
            _treeIndexToStatAndRange = treeIndexToIj;
        }

        public IEnumerable<ItemModTier> QueryMod(int index, float value)
        {
            return _trees[index].Query(value).Select(mw => mw.ItemModTier);
        }

        public IEnumerable<ItemModTier> Query(IEnumerable<float> values)
        {
            var valueList = values.ToList();
            if (_trees.Count != valueList.Count)
            {
                throw new ArgumentException("different number of values than trees");
            }
            if (!_trees.Any())
            {
                return Enumerable.Empty<ItemModTier>();
            }

            var matches = valueList.Select((v, i) => _trees[i].Query(v).Select(w => w.ItemModTier));
            return matches.Aggregate((a, n) => a.Intersect(n)).OrderByDescending(c => c.Tier);
        }

        public IEnumerable<ItemMod> ToItemMods(IEnumerable<float> values)
        {
            var remaining = values.ToList();
            if (_trees.Count != remaining.Count)
            {
                throw new ArgumentException("different number of values than trees");
            }
            foreach (var stat in _firstTierStats)
            {
                var rangeCount = stat.Ranges.Count;
                yield return stat.ToItemMod(remaining.Take(rangeCount).ToList());
                remaining = remaining.Skip(rangeCount).ToList();
            }
        }

        public Range<float> GetRange(ItemModTier tier, int index)
        {
            var statAndRange = _treeIndexToStatAndRange[index];
            return tier.Stats[statAndRange.Item1].Ranges[statAndRange.Item2];
        }

        public override string ToString()
        {
            return "Mod(" + (ModType == ModType.Suffix ? "S" : "P") + "): " + Name;
        }

        private class ModWrapper : IRangeProvider<float>
        {

            public ItemModTier ItemModTier { get; }

            public Range<float> Range { get; }

            public ModWrapper(ItemModTier tier, Range<float> range)
            {
                Range = range;
                ItemModTier = tier;
            }
        }

        private class ItemModComparer : IComparer<ModWrapper>
        {
            public int Compare(ModWrapper x, ModWrapper y)
            {
                return x.Range.CompareTo(y.Range);
            }
        }
    }
}
