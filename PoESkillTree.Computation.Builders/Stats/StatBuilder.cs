using System;
using System.Collections.Generic;
using System.Linq;
using PoESkillTree.Computation.Builders.Conditions;
using PoESkillTree.Computation.Builders.Values;
using PoESkillTree.Computation.Common;
using PoESkillTree.Computation.Common.Builders;
using PoESkillTree.Computation.Common.Builders.Conditions;
using PoESkillTree.Computation.Common.Builders.Entities;
using PoESkillTree.Computation.Common.Builders.Resolving;
using PoESkillTree.Computation.Common.Builders.Stats;
using PoESkillTree.Computation.Common.Builders.Values;
using PoESkillTree.Computation.Common.Parsing;
using PoESkillTree.Utils.Extensions;

namespace PoESkillTree.Computation.Builders.Stats
{
    public class StatBuilder : IStatBuilder
    {
        protected IStatFactory StatFactory { get; }
        protected ICoreStatBuilder CoreStatBuilder { get; }

        public StatBuilder(IStatFactory statFactory, ICoreStatBuilder coreStatBuilder)
        {
            StatFactory = statFactory;
            CoreStatBuilder = coreStatBuilder;
        }

        protected virtual IStatBuilder With(ICoreStatBuilder coreStatBuilder) =>
            WithUntyped(coreStatBuilder);

        private IStatBuilder WithUntyped(ICoreStatBuilder coreStatBuilder) =>
            new StatBuilder(StatFactory, coreStatBuilder);

        protected virtual IStatBuilder WithStatConverter(Func<IStat, IStat> statConverter) =>
            With(new StatBuilderWithStatConverter(CoreStatBuilder, statConverter));

        public virtual IStatBuilder Resolve(ResolveContext context) => With(CoreStatBuilder.Resolve(context));

        public IStatBuilder Minimum => WithStatConverter(s => s.Minimum);
        public IStatBuilder Maximum => WithStatConverter(s => s.Maximum);

        public ValueBuilder Value => ValueFor(NodeType.Total);

        public ValueBuilder ValueFor(NodeType nodeType, ModifierSource modifierSource = null)
            => new ValueBuilder(new ValueBuilderImpl(
                ps => BuildValue(nodeType, modifierSource ?? new ModifierSource.Global(), ps),
                c => Resolve(c).ValueFor(nodeType, modifierSource)));

        private IValue BuildValue(NodeType nodeType, ModifierSource modifierSource, BuildParameters parameters)
        {
            var stats = Build(parameters).Select(r => r.Stats).Flatten().ToList();
            if (stats.Count != 1)
                throw new ParseException("Can only access the value of stat builders that represent a single stat");

            return new StatValue(stats.Single(), nodeType, modifierSource);
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

        public IStatBuilder WithCondition(IConditionBuilder condition) =>
            WithUntyped(new StatBuilderAdapter(this, condition));

        public IStatBuilder CombineWith(IStatBuilder other) =>
            WithUntyped(new CompositeCoreStatBuilder(new StatBuilderAdapter(this), new StatBuilderAdapter(other)));

        public IStatBuilder Concat(IStatBuilder other) =>
            WithUntyped(new ConcatCompositeCoreStatBuilder(new StatBuilderAdapter(this), new StatBuilderAdapter(other)));

        public virtual IEnumerable<StatBuilderResult> Build(BuildParameters parameters) =>
            CoreStatBuilder.Build(parameters);
    }
}