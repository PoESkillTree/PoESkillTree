using System;
using System.Collections.Generic;
using PoESkillTree.Computation.Builders.Conditions;
using PoESkillTree.Computation.Builders.Values;
using PoESkillTree.Computation.Common;
using PoESkillTree.Computation.Common.Builders;
using PoESkillTree.Computation.Common.Builders.Conditions;
using PoESkillTree.Computation.Common.Builders.Entities;
using PoESkillTree.Computation.Common.Builders.Resolving;
using PoESkillTree.Computation.Common.Builders.Stats;
using PoESkillTree.Computation.Common.Builders.Values;

namespace PoESkillTree.Computation.Builders.Stats
{
    public class StatBuilder : IFlagStatBuilder
    {
        private readonly IStatFactory _statFactory;
        private readonly ICoreStatBuilder _coreStatBuilder;

        public StatBuilder(IStatFactory statFactory, ICoreStatBuilder coreStatBuilder)
        {
            _statFactory = statFactory;
            _coreStatBuilder = coreStatBuilder;
        }

        protected IFlagStatBuilder FromIdentity(string identity, Type dataType) =>
            With(LeafCoreStatBuilder.FromIdentity(_statFactory, identity, dataType));

        private StatBuilder With(ICoreStatBuilder coreStatBuilder) => new StatBuilder(_statFactory, coreStatBuilder);

        private IStatBuilder WithStatConverter(Func<IStat, IStat> statConverter) =>
            With(_coreStatBuilder.WithStatConverter(statConverter));

        public IStatBuilder Resolve(ResolveContext context) => With(_coreStatBuilder.Resolve(context));

        public IStatBuilder Minimum => WithStatConverter(s => s.Minimum);
        public IStatBuilder Maximum => WithStatConverter(s => s.Maximum);

        public ValueBuilder Value =>
            new ValueBuilder(new ValueBuilderImpl(_coreStatBuilder.BuildValue, c => Resolve(c).Value));

        public IConditionBuilder IsSet =>
            ValueConditionBuilder.Create(Value, v => v.IsTrue());

        public IStatBuilder ConvertTo(IStatBuilder stat) =>
            With(new ConversionStatBuilder(_statFactory.ConvertTo, _coreStatBuilder, new StatBuilderAdapter(stat)));

        public IStatBuilder GainAs(IStatBuilder stat) =>
            With(new ConversionStatBuilder(_statFactory.GainAs, _coreStatBuilder, new StatBuilderAdapter(stat)));

        public IStatBuilder ChanceToDouble => WithStatConverter(_statFactory.ChanceToDouble);

        public IStatBuilder For(IEntityBuilder entity) => With(_coreStatBuilder.WithEntity(entity));

        public IStatBuilder WithCondition(IConditionBuilder condition) =>
            With(new StatBuilderAdapter(this, condition));

        public IStatBuilder CombineWith(IStatBuilder other) =>
            With(new CompositeCoreStatBuilder(_coreStatBuilder, new StatBuilderAdapter(other)));

        public IEnumerable<StatBuilderResult> Build(
            BuildParameters parameters, ModifierSource originalModifierSource) =>
            _coreStatBuilder.Build(parameters, originalModifierSource);
    }
}