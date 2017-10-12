using System.Collections;
using System.Collections.Generic;
using JetBrains.Annotations;
using PoESkillTree.Computation.Parsing.Data;

namespace PoESkillTree.Computation.Data.Collections
{
    public class ReferencedMatcherCollection<T> : IEnumerable<ReferencedMatcherData>
    {
        private readonly List<ReferencedMatcherData> _matchers =
            new List<ReferencedMatcherData>();

        public void Add([RegexPattern] string regex, T element)
        {
            _matchers.Add(new ReferencedMatcherData(regex, element));
        }

        public IEnumerator<ReferencedMatcherData> GetEnumerator()
        {
            return _matchers.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}