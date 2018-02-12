using System.Collections;
using System.Collections.Generic;
using PoESkillTree.Computation.Common.Builders.Effects;
using PoESkillTree.Computation.Common.Builders.Stats;
using PoESkillTree.Computation.Common.Data;

namespace PoESkillTree.Computation.Data.Collections
{
    /// <summary>
    /// Collection of <see cref="EffectStatData"/> that allows collection initialization syntax for adding entries.
    /// </summary>
    public class EffectStatCollection : IEnumerable<EffectStatData>
    {
        private readonly List<EffectStatData> _data = new List<EffectStatData>();

        public IEnumerator<EffectStatData> GetEnumerator()
        {
            return _data.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public void Add(IEffectBuilder effect, params string[] stats)
        {
            _data.Add(new EffectStatData(effect, stats));
        }

        public void Add(IEffectBuilder effect, params IFlagStatBuilder[] stats)
        {
            _data.Add(new EffectStatData(effect, stats));
        }
    }
}