using System;
using System.Linq.Expressions;
using PoESkillTree.Computation.Builders;
using PoESkillTree.Computation.Builders.Values;
using PoESkillTree.Computation.Common;
using PoESkillTree.Computation.Common.Builders.Conditions;
using PoESkillTree.Computation.Common.Builders.Resolving;
using PoESkillTree.Computation.Common.Builders.Values;
using static PoESkillTree.Computation.Console.Builders.BuilderFactory;

namespace PoESkillTree.Computation.Console.Builders
{
    public class ValueBuilderStub : BuilderStub, IValueBuilder
    {
        private readonly Resolver<IValueBuilder> _resolver;

        public ValueBuilderStub(string stringRepresentation, Resolver<IValueBuilder> resolver)
            : base(stringRepresentation)
        {
            _resolver = resolver;
        }

        private IValueBuilder This => this;

        public IValueBuilder MaximumOnly =>
            CreateValue(This, o => $"{o} (maximum value only)");

        public IConditionBuilder Eq(IValueBuilder other) =>
            CreateCondition(This, other, (l, r) => $"({l} == {r})");

        public IConditionBuilder GreaterThan(IValueBuilder other) =>
            CreateCondition(This, other, (l, r) => $"({l} > {r})");

        public IValueBuilder Add(IValueBuilder other) =>
            CreateValue(This, other, (l, r) => $"({l} + {r})");

        public IValueBuilder Multiply(IValueBuilder other) =>
            CreateValue(This, other, (l, r) => $"({l} * {r})");

        public IValueBuilder DivideBy(IValueBuilder divisor) =>
            CreateValue(This, divisor, (l, r) => $"({l} / {r})");

        public IValueBuilder Select(Expression<Func<double, double>> selector) =>
            CreateValue(This, o => selector.ToString(o));

        public IValueBuilder Create(double value) =>
            new ValueBuilderImpl(value);

        public IValueBuilder Resolve(ResolveContext context) =>
            _resolver(this, context);

        public IValue Build() => new ValueStub(this);
    }


    public class ValueStub : BuilderStub, IValue
    {
        public ValueStub(BuilderStub builderStub) : base(builderStub)
        {
        }

        public NodeValue? Calculate(IValueCalculationContext valueCalculationContext) => 
            throw new NotSupportedException();
    }
}