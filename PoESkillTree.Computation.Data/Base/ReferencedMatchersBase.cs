using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using PoESkillTree.Computation.Parsing.Data;

namespace PoESkillTree.Computation.Data.Base
{
    /// <summary>
    /// Base class for <see cref="IReferencedMatchers"/> implementations.
    /// <para>Sub classes only need to implement <see cref="CreateCollection"/>, which is evaluated and converted
    /// to a list lazily to implement <see cref="IEnumerable{T}"/>.
    /// <see cref="IReferencedMatchers.ReferenceName"/> is implemented as the instantiated class' name.
    /// <see cref="IReferencedMatchers.MatchType"/> is implemented using <typeparamref name="T"/>.</para>
    /// </summary>
    /// <typeparam name="T">The type of <see cref="ReferencedMatcherData.Match"/> values.</typeparam>
    public abstract class ReferencedMatchersBase<T> : IReferencedMatchers
    {
        private readonly Lazy<IReadOnlyList<ReferencedMatcherData>> _lazyMatchers;

        protected ReferencedMatchersBase()
        {
            _lazyMatchers = new Lazy<IReadOnlyList<ReferencedMatcherData>>(() => CreateCollection().ToList());
        }

        public string ReferenceName => GetType().Name;

        public Type MatchType => typeof(T);

        public IEnumerator<ReferencedMatcherData> GetEnumerator()
        {
            return _lazyMatchers.Value.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        protected abstract IEnumerable<ReferencedMatcherData> CreateCollection();
    }
}