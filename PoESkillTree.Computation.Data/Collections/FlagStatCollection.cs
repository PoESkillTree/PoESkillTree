using System.Collections;
using System.Collections.Generic;
using PoESkillTree.Computation.Parsing.Builders;
using PoESkillTree.Computation.Parsing.Builders.Stats;
using PoESkillTree.Computation.Parsing.Data;

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

        public void Add(IFlagStatBuilder stat, params string[] stats)
        {
            _data.Add(new FlagStatData(stat, stats));
        }
    }
}