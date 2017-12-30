using System.Collections;
using System.Collections.Generic;
using PoESkillTree.Computation.Parsing.Builders.Forms;
using PoESkillTree.Computation.Parsing.Builders.Stats;
using PoESkillTree.Computation.Parsing.Data;

namespace PoESkillTree.Computation.Data.Collections
{
    /// <summary>
    /// Collection of <see cref="StatReplacerData"/> that allows collection initialization syntax for adding entries.
    /// </summary>
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

        public void Add(IFormBuilder form, IStatBuilder stat, double value)
        {
            _data.Add(new GivenStatData(form, stat, value));
        }
    }
}