using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using PoESkillTree.Computation.Parsing.Builders;
using PoESkillTree.Computation.Parsing.Builders.Matching;
using PoESkillTree.Computation.Parsing.Data;

namespace PoESkillTree.Computation.Data.Base
{
    /// <summary>
    /// Base class for <see cref="IStatMatchers"/> implementations. Provides access to <see cref="IBuilderFactories"/>
    /// and <see cref="IMatchContexts"/> properties (see <see cref="UsesMatchContext"/>).
    /// <para>Sub classes only need to implement <see cref="CreateCollection"/>, which is evaluated and converted
    /// to a list lazily to implement <see cref="IEnumerable{T}"/>.</para>
    /// <para><see cref="IStatMatchers.ReferenceNames"/> is implemented as "can't be referenced" and
    /// <see cref="IStatMatchers.MatchesWholeLineOnly"/> as false, both can be overridden.</para>
    /// </summary>
    public abstract class StatMatchersBase : UsesMatchContext, IStatMatchers
    {
        private readonly Lazy<IReadOnlyList<MatcherData>> _lazyMatchers;

        public virtual IReadOnlyList<string> ReferenceNames { get; } = new string[0];

        public virtual bool MatchesWholeLineOnly { get; } = false;

        protected StatMatchersBase(
            IBuilderFactories builderFactories, IMatchContexts matchContexts)
            : base(builderFactories, matchContexts)
        {
            _lazyMatchers = new Lazy<IReadOnlyList<MatcherData>>(() => CreateCollection().ToList());
        }

        public IEnumerator<MatcherData> GetEnumerator()
        {
            return _lazyMatchers.Value.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        protected abstract IEnumerable<MatcherData> CreateCollection();
    }
}