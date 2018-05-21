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

        public ValueBuilderImpl(Func<IValue> buildValue)
        {
            _buildValue = buildValue;
        }

        public IValueBuilder Resolve(ResolveContext context) => this;

        public IValueBuilder MaximumOnly => 
            Create(this, (o, c) => o.Select(v => new NodeValue(0, v.Maximum)));

        public IConditionBuilder Eq(IValueBuilder other) =>
            ValueConditionBuilder.Create(this, other, (left, right, c) => left == right);

        public IConditionBuilder GreaterThan(IValueBuilder other) =>
            ValueConditionBuilder.Create(this, other, (left, right, c) => left > right);

        public IValueBuilder Add(IValueBuilder other) =>
            Create(this, other, (left, right, c) => new[] { left, right }.Sum());

        public IValueBuilder Multiply(IValueBuilder other) =>
            Create(this, other, (left, right, c) => new[] { left, right }.Product());

        public IValueBuilder DivideBy(IValueBuilder divisor) =>
            Create(this, divisor, (left, right, c) => left / (right ?? new NodeValue(1)));

        public IValueBuilder Select(Func<double, double> selector) =>
            Create(this, (o, c) => o.Select(selector));

        public IValueBuilder Create(double value) => new ValueBuilderImpl(value);

        public IValue Build() => _buildValue();


        public static IValueBuilder Create(
            IValueBuilder operand, Func<NodeValue?, IValueCalculationContext, NodeValue?> calculate) =>
            new ValueBuilderImpl(() => Build(operand, calculate));

        public static IValueBuilder Create(
            IValueBuilder operand1, IValueBuilder operand2,
            Func<NodeValue?, NodeValue?, IValueCalculationContext, NodeValue?> calculate) =>
            new ValueBuilderImpl(() => Build(operand1, operand2, calculate));

        private static IValue Build(
            IValueBuilder operand, Func<NodeValue?, IValueCalculationContext, NodeValue?> calculate)
        {
            var builtOperand = operand.Build();
            return new FunctionalValue(c => calculate(builtOperand.Calculate(c), c));
        }

        private static IValue Build(
            IValueBuilder operand1, IValueBuilder operand2,
            Func<NodeValue?, NodeValue?, IValueCalculationContext, NodeValue?> calculate)
        {
            var o1 = operand1.Build();
            var o2 = operand2.Build();
            return new FunctionalValue(c => calculate(o1.Calculate(c), o2.Calculate(c), c));
        }
    }
}