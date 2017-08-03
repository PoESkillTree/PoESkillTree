using System.Collections;
using System.Collections.Generic;
using JetBrains.Annotations;

namespace PoESkillTree.Computation.Data.Collections
{
    public class ReferencedMatcherCollection<T> : IEnumerable<ReferencedMatcherData<T>>
    {
        private readonly List<ReferencedMatcherData<T>> _matchers =
            new List<ReferencedMatcherData<T>>();

        public void Add([RegexPattern] string regex, T element)
        {
            _matchers.Add(new ReferencedMatcherData<T>(regex, element));
        }

        public IEnumerator<ReferencedMatcherData<T>> GetEnumerator()
        {
            return _matchers.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}