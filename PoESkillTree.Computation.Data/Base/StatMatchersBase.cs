using System;
using System.Collections.Generic;
using PoESkillTree.Computation.Common.Builders;
using PoESkillTree.Computation.Common.Data;

namespace PoESkillTree.Computation.Data.Base
{
    /// <summary>
    /// Base class for <see cref="IStatMatchers"/> implementations. Provides access to <see cref="IBuilderFactories"/>.
    /// <para>Sub classes only need to implement <see cref="CreateCollection"/>, which is evaluated and converted
    /// to a list lazily to implement <see cref="IEnumerable{T}"/>.</para>
    /// <para><see cref="IStatMatchers.ReferenceNames"/> is implemented as "can't be referenced" and
    /// <see cref="IStatMatchers.MatchesWholeLineOnly"/> as false, both can be overridden.</para>
    /// </summary>
    public abstract class StatMatchersBase : UsesMatchContext, IStatMatchers
    {
        private readonly Lazy<IReadOnlyList<MatcherData>> _lazyMatchers;

        public IReadOnlyList<MatcherData> Data => _lazyMatchers.Value;

        public virtual IReadOnlyList<string> ReferenceNames { get; } = new string[0];

        public virtual bool MatchesWholeLineOnly { get; } = false;

        protected StatMatchersBase(IBuilderFactories builderFactories)
            : base(builderFactories)
            => _lazyMatchers = new Lazy<IReadOnlyList<MatcherData>>(CreateCollection);

        protected abstract IReadOnlyList<MatcherData> CreateCollection();
    }
}