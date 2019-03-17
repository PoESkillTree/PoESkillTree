using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using MB.Algodat;

namespace PoESkillTree.Model.Items.Mods
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
        /// All stat ids of all tiers. The valueIndex parameters to methods of Affix are indices of this array.
        /// </summary>
        public IReadOnlyList<string> StatIds { get; }

        /// <summary>
        /// The ranges of this selector. The array index specifies the stat.
        /// The list then specifies the ranges of the different tiers for this stat.
        /// </summary>
        private readonly IReadOnlyList<Range<int>>[] _ranges;

        private readonly IRangeTree<int, ModWrapper>[] _trees;

        private readonly IEnumerable<IMod> _allMods;

        public IMod DefaultMod => _allMods.First();

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
                StatIds = new string[0];
                _ranges = new IReadOnlyList<Range<int>>[0];
                _trees = new IRangeTree<int, ModWrapper>[0];
                return;
            }

            StatIds = mods.SelectMany(m => m.Stats).Select(s => s.Id).Distinct().ToList();
            var valueCount = StatIds.Count;

            var comparer = new ModWrapperComparer();
            _trees = new IRangeTree<int, ModWrapper>[valueCount];
            _ranges = new IReadOnlyList<Range<int>>[valueCount];
            for (int i = 0; i < valueCount; i++)
            {
                var wrapper = mods.Select(t => new ModWrapper(t, SelectStat(t, i).Range)).ToList();
                _trees[i] = new RangeTree<int, ModWrapper>(wrapper, comparer);
                _ranges[i] = wrapper.Select(w => w.Range).ToList();
            }

            _allMods = mods.ToList();
        }

        public Stat SelectStat(IMod mod, int valueIndex)
        {
            var statId = StatIds[valueIndex];
            var stat = mod.Stats.FirstOrDefault(s => s.Id == statId);
            return stat ?? new Stat(statId, 0, 0);
        }

        public IEnumerable<IMod> QueryModsSingleValue(int valueIndex, int value)
        {
            return _trees[valueIndex].Query(value).Select(mw => mw.Mod);
        }

        public IEnumerable<IMod> QueryMods(IEnumerable<(int valueIndex, int value)> values)
        {
            if (!_trees.Any())
                return _allMods;

            // for each value: query matching tiers
            return values.Select(t => QueryModsSingleValue(t.valueIndex, t.value))
                // aggregate to keep only tiers that match all values of all stats
                .Aggregate(_allMods, (a, n) => a.Intersect(n))
                .OrderBy(m => m.RequiredLevel);
        }

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