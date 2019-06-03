using System;
using PoESkillTree.Computation.Common.Builders.Conditions;
using PoESkillTree.Computation.Common.Builders.Resolving;

namespace PoESkillTree.Computation.Common.Builders.Values
{
    public class ValueBuilderStub : IValueBuilder
    {
        public double Value { get; }

        public ValueBuilderStub(double value)
        {
            Value = value;
        }

        public static double Convert(IValueBuilder value)
        {
            if (value is ValueBuilder b)
            {
                // Easiest way to get the underlying value back.
                value = b.Resolve(null);
            }
            return ((ValueBuilderStub) value).Value;
        }

        public IValueBuilder Resolve(ResolveContext context) => this;

        public IValueBuilder MaximumOnly => throw new NotSupportedException();
        public IValueBuilder Average => throw new NotSupportedException();

        public IConditionBuilder Eq(IValueBuilder other) =>
            new ConditionBuilderStub(Value == Convert(other));

        public IConditionBuilder GreaterThan(IValueBuilder other) =>
            new ConditionBuilderStub(Value > Convert(other));

        public IValueBuilder Add(IValueBuilder other) =>
            new ValueBuilderStub(Value + Convert(other));

        public IValueBuilder Multiply(IValueBuilder other) =>
            new ValueBuilderStub(Value * Convert(other));

        public IValueBuilder DivideBy(IValueBuilder divisor) =>
            new ValueBuilderStub(Value / Convert(divisor));

        public IValueBuilder If(IValue condition) => throw new NotSupportedException();

        public IValueBuilder Select(Func<NodeValue, NodeValue> selector, Func<IValue, string> identity) => 
            new ValueBuilderStub(selector(new NodeValue(Value)).Single);

        public IValueBuilder Create(double value) => new ValueBuilderStub(value);

        public IValue Build(BuildParameters parameters) => new Constant(Value);
    }
}