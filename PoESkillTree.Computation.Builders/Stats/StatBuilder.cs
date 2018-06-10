using System;
using System.Collections.Generic;
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
    public class StatBuilder : IStatBuilder
    {
        private readonly ICoreStatBuilder _coreStatBuilder;

        public StatBuilder(ICoreStatBuilder coreStatBuilder)
        {
            _coreStatBuilder = coreStatBuilder;
        }

        public IStatBuilder Resolve(ResolveContext context) => new StatBuilder(_coreStatBuilder.Resolve(context));

        public IStatBuilder Minimum => new StatBuilder(_coreStatBuilder.WithStatConverter(s => s.Minimum));
        public IStatBuilder Maximum => new StatBuilder(_coreStatBuilder.WithStatConverter(s => s.Maximum));

        public ValueBuilder Value =>
            new ValueBuilder(new ValueBuilderImpl(_coreStatBuilder.BuildValue, c => Resolve(c).Value));

        public IStatBuilder ConvertTo(IStatBuilder stat) => throw new NotImplementedException();
        public IStatBuilder GainAs(IStatBuilder stat) => throw new NotImplementedException();

        public IFlagStatBuilder ApplyModifiersTo(IStatBuilder stat, IValueBuilder percentOfTheirValue) =>
            throw new NotImplementedException();

        public IStatBuilder ChanceToDouble => throw new NotImplementedException();

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