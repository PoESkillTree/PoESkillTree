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
        private readonly ICoreStatBuilder _coreStatBuilder;

        public StatBuilder(ICoreStatBuilder coreStatBuilder)
        {
            _coreStatBuilder = coreStatBuilder;
        }

        private IStatBuilder WithStatConverter(Func<IStat, IStat> statConverter) =>
            new StatBuilder(_coreStatBuilder.WithStatConverter(statConverter));

        public IStatBuilder Resolve(ResolveContext context) => new StatBuilder(_coreStatBuilder.Resolve(context));

        public IStatBuilder Minimum => WithStatConverter(s => s.Minimum);
        public IStatBuilder Maximum => WithStatConverter(s => s.Maximum);

        public ValueBuilder Value =>
            new ValueBuilder(new ValueBuilderImpl(_coreStatBuilder.BuildValue, c => Resolve(c).Value));

        public IConditionBuilder IsSet =>
            ValueConditionBuilder.Create(Value, v => v.IsTrue());

        public IStatBuilder ConvertTo(IStatBuilder stat) =>
            new StatBuilder(new ConversionStatBuilder(_coreStatBuilder, new StatBuilderAdapter(stat)));

        public IStatBuilder GainAs(IStatBuilder stat) =>
            new StatBuilder(new ConversionStatBuilder(_coreStatBuilder, new StatBuilderAdapter(stat),
                ConversionStatBuilder.Mode.GainAs));

        public IStatBuilder ChanceToDouble =>
            WithStatConverter(s => Stat.CopyWithSuffix(s, nameof(ChanceToDouble), dataType: typeof(int)));

        public IStatBuilder For(IEntityBuilder entity) => new StatBuilder(_coreStatBuilder.WithEntity(entity));

        public IStatBuilder WithCondition(IConditionBuilder condition) =>
            new StatBuilder(new StatBuilderAdapter(this, condition));

        public IStatBuilder CombineWith(IStatBuilder other) =>
            new StatBuilder(new CompositeCoreStatBuilder(_coreStatBuilder, new StatBuilderAdapter(other)));

        public (IReadOnlyList<IStat> stats, ModifierSource modifierSource, ValueConverter valueConverter)
            Build(ModifierSource originalModifierSource, Entity modifierSourceEntity)
        {
            var result = _coreStatBuilder.Build(originalModifierSource, modifierSourceEntity);
            return (result.Stats, result.ModifierSource, result.ValueConverter);
        }
    }
}