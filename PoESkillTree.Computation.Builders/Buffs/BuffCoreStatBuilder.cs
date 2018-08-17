using System;
using System.Collections.Generic;
using System.Linq;
using PoESkillTree.Computation.Builders.Stats;
using PoESkillTree.Computation.Common.Builders;
using PoESkillTree.Computation.Common.Builders.Buffs;
using PoESkillTree.Computation.Common.Builders.Entities;
using PoESkillTree.Computation.Common.Builders.Resolving;
using PoESkillTree.Computation.Common.Builders.Stats;
using PoESkillTree.Utils;
using PoESkillTree.Utils.Extensions;

namespace PoESkillTree.Computation.Builders.Buffs
{
    internal class BuffCoreStatBuilder : ICoreStatBuilder
    {
        private readonly IReadOnlyList<BuffBuilderWithKeywords> _buffs;
        private readonly Func<IBuffBuilder, IStatBuilder> _statFactory;
        private readonly BuffRestrictionsBuilder _restrictionsBuilder;

        public BuffCoreStatBuilder(
            IReadOnlyList<BuffBuilderWithKeywords> buffs, Func<IBuffBuilder, IStatBuilder> statFactory,
            BuffRestrictionsBuilder restrictionsBuilder)
        {
            _buffs = buffs;
            _statFactory = statFactory;
            _restrictionsBuilder = restrictionsBuilder;
        }

        public ICoreStatBuilder Resolve(ResolveContext context) =>
            new BuffCoreStatBuilder(
                _buffs.Select(b => b.Resolve(context)).ToList(),
                _statFactory.AndThen(b => b.Resolve(context)),
                _restrictionsBuilder.Resolve(context));

        public ICoreStatBuilder WithEntity(IEntityBuilder entityBuilder) =>
            new BuffCoreStatBuilder(_buffs, _statFactory.AndThen(b => b.For(entityBuilder)), _restrictionsBuilder);

        public IEnumerable<StatBuilderResult> Build(BuildParameters parameters)
        {
            var restrictions = _restrictionsBuilder.Build();
            var selectedBuffs = _buffs.Where(restrictions.AllowsBuff).ToList();
            if (selectedBuffs.IsEmpty())
                return Enumerable.Empty<StatBuilderResult>();
            return selectedBuffs
                .Select(b => _statFactory(b.Buff))
                .Aggregate((l, r) => l.CombineWith(r))
                .Build(parameters);
        }
    }
}