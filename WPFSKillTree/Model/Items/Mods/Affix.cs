using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using MB.Algodat;
using MoreLinq;

namespace POESKillTree.Model.Items.Mods
{
    /// <summary>
    /// Collection of <see cref="IMod"/>s with each mod representing a tier of this affix.
    /// The affix can be queried with values to get the mod(s) matching those values.
    /// </summary>
    [DebuggerDisplay("{" + nameof(Name) + "}")]
    public class Affix
    {
        public string Name { get; }

        /// <summary>
        /// Gets the number of values mods of this affix have
        /// </summary>
        public int ValueCount { get; }

        /// <summary>
        /// Gets the stats of the first tier mod
        /// </summary>
        public IReadOnlyList<IStat> FirstTierStats { get; }

        /// <summary>
        /// The ranges of this selector. The array index specifies the stat.
        /// The list then specifies the ranges of the different tiers for this stat.
        /// </summary>
        private readonly IReadOnlyList<Range<int>>[] _ranges;

        private readonly IRangeTree<int, ModWrapper>[] _trees;

        public Affix()
            : this(new IMod[0])
        {
            Name = "";
        }

        public Affix(IMod mod)
            : this(new[] { mod })
        {
            Name = "";
        }

        public Affix(IReadOnlyList<IMod> mods, string name)
            : this(mods)
        {
            if (!mods.Any())
            {
                throw new ArgumentException("Can not create named Affix with no mods", nameof(mods));
            }
            Name = name;
        }

        private Affix(IReadOnlyList<IMod> mods)
        {
            if (!mods.Any())
            {
                ValueCount = 0;
                FirstTierStats = new IStat[0];
                _ranges = new IReadOnlyList<Range<int>>[0];
                _trees = new IRangeTree<int, ModWrapper>[0];
                return;
            }

            var firstMod = mods[0];
            ValueCount = firstMod.Stats.Count;
            FirstTierStats = firstMod.Stats;
            if (mods.Any(m => m.Stats.Count != ValueCount))
            {
                throw new NotSupportedException("Mods must all have the same amount of stats");
            }

            var comparer = new ModWrapperComparer();
            _trees = new IRangeTree<int, ModWrapper>[ValueCount];
            _ranges = new IReadOnlyList<Range<int>>[ValueCount];
            for (int i = 0; i < ValueCount; i++)
            {
                var wrapper = mods.Select(t => new ModWrapper(t, t.Stats[i].Range)).ToList();
                _trees[i] = new RangeTree<int, ModWrapper>(wrapper, comparer);
                _ranges[i] = wrapper.Select(w => w.Range).ToList();
            }
        }

        public IEnumerable<IMod> QueryModsSingleValue(int valueIndex, int value)
        {
            return _trees[valueIndex].Query(value).Select(mw => mw.Mod);
        }

        public IEnumerable<IMod> QueryMods(IEnumerable<int> values)
        {
            if (!_trees.Any())
            {
                return Enumerable.Empty<IMod>();
            }
            // for each value: query matching tiers
            return values.EquiZip(_trees, QueryForTree)
                // aggregate to keep only tiers that match all values of all stats
                .Aggregate((a, n) => a.Intersect(n))
                .OrderBy(m => m.RequiredLevel);
        }

        private static IEnumerable<IMod> QueryForTree(int value, IRangeTree<int, ModWrapper> tree)
            => tree.Query(value).Select(w => w.Mod);

        public IReadOnlyList<Range<int>> GetRanges(int valueIndex)
            => _ranges[valueIndex];


        private class ModWrapper : IRangeProvider<int>
        {

            public IMod Mod { get; }

            public Range<int> Range { get; }

            public ModWrapper(IMod mod, Range<int> range)
            {
                Mod = mod;
                Range = range;
            }
        }

        private class ModWrapperComparer : IComparer<ModWrapper>
        {
            public int Compare(ModWrapper x, ModWrapper y)
            {
                return x.Range.CompareTo(y.Range);
            }
        }
    }
}