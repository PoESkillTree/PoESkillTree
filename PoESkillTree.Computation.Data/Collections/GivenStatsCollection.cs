using System.Collections;
using System.Collections.Generic;
using PoESkillTree.Computation.Providers.Forms;
using PoESkillTree.Computation.Providers.Stats;

namespace PoESkillTree.Computation.Data.Collections
{
    public class GivenStatCollection : IEnumerable<GivenStatData>
    {
        private readonly List<GivenStatData> _data = new List<GivenStatData>();

        public IEnumerator<GivenStatData> GetEnumerator()
        {
            return _data.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public void Add(IFormProvider form, IStatProvider stat, double value)
        {
            _data.Add(new GivenStatData(form, stat, value));
        }
    }
}