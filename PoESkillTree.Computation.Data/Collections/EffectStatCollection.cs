using System.Collections;
using System.Collections.Generic;
using PoESkillTree.Computation.Common.Builders.Effects;
using PoESkillTree.Computation.Common.Builders.Forms;
using PoESkillTree.Computation.Common.Builders.Stats;
using PoESkillTree.Computation.Common.Data;

namespace PoESkillTree.Computation.Data.Collections
{
    /// <summary>
    /// Collection of <see cref="GivenStatData"/> that allows collection initialization syntax for adding entries.
    /// Uses <see cref="IEffectBuilder.AddStat"/> for adding stats.
    /// </summary>
    public class EffectStatCollection : IEnumerable<GivenStatData>
    {
        private readonly List<GivenStatData> _data = new List<GivenStatData>();

        public IEnumerator<GivenStatData> GetEnumerator() => _data.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        public void Add(IEffectBuilder effect, IFormBuilder form, IStatBuilder stat, double value) =>
            _data.Add(new GivenStatData(form, effect.AddStat(stat), value));
    }
}