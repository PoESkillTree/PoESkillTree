using System.Collections;
using System.Collections.Generic;
using PoESkillTree.Computation.Providers.Effects;
using PoESkillTree.Computation.Providers.Stats;

namespace PoESkillTree.Computation.Data.Collections
{
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

        public void Add(IEffectProvider effect, params string[] stats)
        {
            _data.Add(new EffectStatData(effect, stats));
        }

        public void Add(IEffectProvider effect, params IFlagStatProvider[] stats)
        {
            _data.Add(new EffectStatData(effect, stats));
        }
    }
}