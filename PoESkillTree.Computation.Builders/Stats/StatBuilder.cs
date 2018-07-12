using System;
using System.Collections.Generic;
using System.Linq;
using PoESkillTree.Common.Utils.Extensions;
using PoESkillTree.Computation.Builders.Conditions;
using PoESkillTree.Computation.Builders.Values;
using PoESkillTree.Computation.Common;
using PoESkillTree.Computation.Common.Builders;
using PoESkillTree.Computation.Common.Builders.Conditions;
using PoESkillTree.Computation.Common.Builders.Entities;
using PoESkillTree.Computation.Common.Builders.Resolving;
using PoESkillTree.Computation.Common.Builders.Skills;
using PoESkillTree.Computation.Common.Builders.Stats;
using PoESkillTree.Computation.Common.Builders.Values;
using PoESkillTree.Computation.Common.Parsing;

namespace PoESkillTree.Computation.Builders.Stats
{
    public class StatBuilder : IFlagStatBuilder
    {
        protected IStatFactory StatFactory { get; }
        protected ICoreStatBuilder CoreStatBuilder { get; }

        public StatBuilder(IStatFactory statFactory, ICoreStatBuilder coreStatBuilder)
        {
            StatFactory = statFactory;
            CoreStatBuilder = coreStatBuilder;
        }

        protected IFlagStatBuilder FromIdentity(
            string identity, Type dataType, ExplicitRegistrationType explicitRegistrationType = null) =>
            With(LeafCoreStatBuilder.FromIdentity(StatFactory, identity, dataType, explicitRegistrationType));

        protected virtual IFlagStatBuilder With(ICoreStatBuilder coreStatBuilder) =>
            WithUntyped(coreStatBuilder);

        private IFlagStatBuilder WithUntyped(ICoreStatBuilder coreStatBuilder) =>
            new StatBuilder(StatFactory, coreStatBuilder);

        protected virtual IStatBuilder WithStatConverter(Func<IStat, IStat> statConverter) =>
            With(new StatBuilderWithStatConverter(CoreStatBuilder, statConverter));

        public virtual IStatBuilder Resolve(ResolveContext context) => With(CoreStatBuilder.Resolve(context));

        public IStatBuilder Minimum => WithStatConverter(s => s.Minimum);
        public IStatBuilder Maximum => WithStatConverter(s => s.Maximum);

        public ValueBuilder Value =>
            new ValueBuilder(new ValueBuilderImpl(BuildVallue, c => Resolve(c).Value));

        private IValue BuildVallue(BuildParameters parameters)
        {
            var stats = Build(parameters, null).Select(r => r.Stats).Flatten().ToList();
            if (stats.Count != 1)
                throw new ParseException("Can only access the value of stat builders that represent a single stat");

            return new StatValue(stats.Single());
        }

        public IConditionBuilder IsSet =>
            ValueConditionBuilder.Create(Value, v => v.IsTrue(), v => $"{v}.IsSet");

        public IStatBuilder ConvertTo(IStatBuilder stat) =>
            WithUntyped(new ConversionStatBuilder(StatFactory.ConvertTo,
                new StatBuilderAdapter(this), new StatBuilderAdapter(stat)));

        public IStatBuilder GainAs(IStatBuilder stat) =>
            WithUntyped(new ConversionStatBuilder(StatFactory.GainAs,
                new StatBuilderAdapter(this), new StatBuilderAdapter(stat)));

        public IStatBuilder ChanceToDouble => WithStatConverter(StatFactory.ChanceToDouble);

        public IStatBuilder For(IEntityBuilder entity) => With(CoreStatBuilder.WithEntity(entity));

        public virtual IStatBuilder With(IKeywordBuilder keyword) => WithCondition(KeywordCondition(keyword));

        public virtual IStatBuilder NotWith(IKeywordBuilder keyword) => WithCondition(KeywordCondition(keyword).Not);

        private IConditionBuilder KeywordCondition(IKeywordBuilder keyword) =>
            ValueConditionBuilder.Create(BuildKeywordStat, keyword);

        protected virtual IStat BuildKeywordStat(BuildParameters parameters, IKeywordBuilder keyword) =>
            StatFactory.ActiveSkillPartHasKeyword(parameters.ModifierSourceEntity, keyword.Build());

        public IStatBuilder WithCondition(IConditionBuilder condition) =>
            WithUntyped(new StatBuilderAdapter(this, condition));

        public IStatBuilder CombineWith(IStatBuilder other) =>
            WithUntyped(new CompositeCoreStatBuilder(new StatBuilderAdapter(this), new StatBuilderAdapter(other)));

        public virtual IEnumerable<StatBuilderResult> Build(
            BuildParameters parameters, ModifierSource originalModifierSource) =>
            CoreStatBuilder.Build(parameters, originalModifierSource);
    }
}