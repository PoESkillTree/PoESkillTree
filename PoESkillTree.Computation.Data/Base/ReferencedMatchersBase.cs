using System;
using System.Collections.Generic;
using System.Linq;
using PoESkillTree.Computation.Parsing.Data;

namespace PoESkillTree.Computation.Data.Base
{
    public abstract class ReferencedMatchersBase<T> : IReferencedMatchers<T>
    {
        private readonly Lazy<IReadOnlyList<ReferencedMatcherData<T>>> _lazyMatchers;

        protected ReferencedMatchersBase()
        {
            _lazyMatchers =
                new Lazy<IReadOnlyList<ReferencedMatcherData<T>>>(() => CreateCollection()
                    .ToList());
        }

        public string ReferenceName => GetType().Name;

        public IReadOnlyList<ReferencedMatcherData<T>> Matchers => _lazyMatchers.Value;

        protected abstract IEnumerable<ReferencedMatcherData<T>> CreateCollection();
    }
}