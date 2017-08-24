using System.Collections;
using System.Collections.Generic;
using JetBrains.Annotations;
using PoESkillTree.Computation.Parsing.Builders;
using PoESkillTree.Computation.Parsing.Data;

namespace PoESkillTree.Computation.Data.Collections
{
    public class StatReplacerCollection : IEnumerable<StatReplacerData>
    {
        private readonly List<StatReplacerData> _data = new List<StatReplacerData>();

        public IEnumerator<StatReplacerData> GetEnumerator()
        {
            return _data.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public void Add([RegexPattern] string originalStat, params string[] replacements)
        {
            _data.Add(new StatReplacerData(originalStat, replacements));
        }
    }
}