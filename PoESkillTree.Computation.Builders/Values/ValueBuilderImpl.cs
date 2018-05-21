using System;
using PoESkillTree.Common.Utils.Extensions;
using PoESkillTree.Computation.Builders.Conditions;
using PoESkillTree.Computation.Common;
using PoESkillTree.Computation.Common.Builders.Conditions;
using PoESkillTree.Computation.Common.Builders.Resolving;
using PoESkillTree.Computation.Common.Builders.Values;

namespace PoESkillTree.Computation.Builders.Values
{
    // ("Impl" suffix to avoid confusion with ValueBuilder in Common)
    public class ValueBuilderImpl : IValueBuilder
    {
        private readonly Func<IValue> _buildValue;

        public ValueBuilderImpl(double? value) : this(new Constant(value))
        {
        }

        public ValueBuilderImpl(IValue value) : this(() => value)
        {
        }

        public ValueBuilderImpl(Func<IValueCalculationContext, NodeValue?> calculateValue)
            : this(() => new FunctionalValue(calculateValue))
        {
        }

        private ValueBuilderImpl(Func<IValue> buildValue)
        {
            _buildValue = buildValue;
        }

        public IValueBuilder Resolve(ResolveContext context) => this;

        public IValueBuilder MaximumOnly =>
            new ValueBuilderImpl(c => Calculate(c).Select(v => new NodeValue(0, v.Maximum)));

        public IConditionBuilder Eq(IValueBuilder other) =>
            new ValueConditionBuilder(c => Calculate(c) == other.Build().Calculate(c));

        public IConditionBuilder GreaterThan(IValueBuilder other) =>
            new ValueConditionBuilder(c => Calculate(c) > other.Build().Calculate(c));

        public IValueBuilder Add(IValueBuilder other) =>
            new ValueBuilderImpl(c =>
            {
                var left = Calculate(c);
                var right = other.Build().Calculate(c);
                return new[] { left, right }.Sum();
            });

        public IValueBuilder Multiply(IValueBuilder other) =>
            new ValueBuilderImpl(c =>
            {
                var left = Calculate(c);
                var right = other.Build().Calculate(c);
                return new[] { left, right }.Product();
            });

        public IValueBuilder DivideBy(IValueBuilder divisor) =>
            new ValueBuilderImpl(c =>
            {
                var left = Calculate(c);
                var right = divisor.Build().Calculate(c) ?? new NodeValue(1);
                return left / right;
            });

        public IValueBuilder Select(Func<double, double> selector) =>
            new ValueBuilderImpl(c => Calculate(c).Select(selector));

        public IValueBuilder Create(double value) => new ValueBuilderImpl(value);

        public IValue Build() => _buildValue();

        private NodeValue? Calculate(IValueCalculationContext context) => Build().Calculate(context);
    }
}