using System;
using System.Collections.Generic;
using System.Linq;
using PoESkillTree.Computation.Common;
using PoESkillTree.Computation.Common.Builders;
using PoESkillTree.Computation.Common.Builders.Entities;
using PoESkillTree.Computation.Common.Builders.Resolving;
using PoESkillTree.Computation.Common.Builders.Stats;

namespace PoESkillTree.Computation.Builders.Stats
{
    internal class StatBuilderWithStatConverter : ICoreStatBuilder
    {
        private readonly ICoreStatBuilder _inner;
        private readonly Func<ModifierSource, IStat, IStat> _statConverter;

        public StatBuilderWithStatConverter(ICoreStatBuilder inner, Func<ModifierSource, IStat, IStat> statConverter)
            => (_inner, _statConverter) = (inner, statConverter);

        public ICoreStatBuilder Resolve(ResolveContext context) =>
            new StatBuilderWithStatConverter(_inner.Resolve(context), _statConverter);

        public ICoreStatBuilder WithEntity(IEntityBuilder entityBuilder) =>
            new StatBuilderWithStatConverter(_inner.WithEntity(entityBuilder), _statConverter);

        public IEnumerable<StatBuilderResult> Build(BuildParameters parameters) =>
            from r in _inner.Build(parameters)
            let stats = r.Stats.Select(s => _statConverter(r.ModifierSource, s)).ToList()
            select new StatBuilderResult(stats, r.ModifierSource, r.ValueConverter);
    }
}