using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using PoESkillTree.Computation.Parsing.Data;

namespace PoESkillTree.Computation.Data.Base
{
    public abstract class ReferencedMatchersBase<T> : IReferencedMatchers
    {
        private readonly Lazy<IReadOnlyList<ReferencedMatcherData>> _lazyMatchers;

        protected ReferencedMatchersBase()
        {
            _lazyMatchers =
                new Lazy<IReadOnlyList<ReferencedMatcherData>>(() => CreateCollection()
                    .ToList());
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