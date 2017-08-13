using System.Collections;
using System.Collections.Generic;
using PoESkillTree.Computation.Providers.Stats;

namespace PoESkillTree.Computation.Data.Collections
{
    public class FlagStatCollection : IEnumerable<FlagStatData>
    {
        private readonly List<FlagStatData> _data = new List<FlagStatData>();

        public IEnumerator<FlagStatData> GetEnumerator()
        {
            return _data.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public void Add(IFlagStatProvider stat, params string[] stats)
        {
            _data.Add(new FlagStatData(stat, stats));
        }
    }
}