using System.Collections;
using System.Collections.Generic;
using JetBrains.Annotations;
using PoESkillTree.Computation.Common.Data;

namespace PoESkillTree.Computation.Data.Collections
{
    /// <summary>
    /// Collection of <see cref="ReferencedMatcherData"/> that allows collection initialization syntax for adding 
    /// entries.
    /// See <see cref="ReferencedMatcherData"/> for documentation of <see cref="Add"/>'s parameters.
    /// <para>Ensures type safety of <see cref="ReferencedMatcherData.Match"/> via the generic type parameter.</para>
    /// </summary>
    /// <typeparam name="T">The type of values of <see cref="ReferencedMatcherData.Match"/></typeparam>
    public class ReferencedMatcherCollection<T> : IReadOnlyList<ReferencedMatcherData>
    {
        private readonly List<ReferencedMatcherData> _matchers = new List<ReferencedMatcherData>();

        public void Add([RegexPattern] string regex, T element)
            => _matchers.Add(new ReferencedMatcherData(regex, element));

        public IEnumerator<ReferencedMatcherData> GetEnumerator() => _matchers.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        public int Count => _matchers.Count;

        public ReferencedMatcherData this[int index] => _matchers[index];
    }
}