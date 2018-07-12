using System;
using System.Collections.Generic;
using System.Linq;

namespace PoESkillTree.Computation.Common
{
    public abstract class StringIdentityValue : IValue
    {
        private readonly string _identity;

        protected StringIdentityValue(object identity) => 
            _identity = identity?.ToString() ?? "null";

        public abstract NodeValue? Calculate(IValueCalculationContext context);

        public override bool Equals(object obj) =>
            (obj == this) || (obj is StringIdentityValue other && Equals(other));

        private bool Equals(StringIdentityValue other) =>
            GetType() == other.GetType() && _identity.Equals(other._identity);

        public override int GetHashCode() =>
            _identity.GetHashCode();

        public override string ToString() =>
            _identity;
    }

    public class Constant : StringIdentityValue
    {
        private readonly NodeValue? _value;

        public Constant(double? value) : this((NodeValue?) value)
        {
        }

        public Constant(bool value) : this((NodeValue?) value)
        {
        }

        public Constant(NodeValue? value) : base(value) =>
            _value = value;

        public override NodeValue? Calculate(IValueCalculationContext valueCalculationContext) =>
            _value;
    }


    public class StatValue : FunctionalValue
    {
        public StatValue(IStat stat) : base(c => c.GetValue(stat), $"{stat}.Value")
        {
        }
    }


    public class FunctionalValue : StringIdentityValue
    {
        private readonly Func<IValueCalculationContext, NodeValue?> _calculate;

        public FunctionalValue(Func<IValueCalculationContext, NodeValue?> calculate, string identity) 
            : base(identity) =>
            _calculate = calculate;

        public override NodeValue? Calculate(IValueCalculationContext context) =>
            _calculate(context);
    }

    public class NotValue : ConditionalValue
    {
        public NotValue(IValue value) : base(c => !value.Calculate(c).IsTrue(), $"Not({value})")
        {
        }
    }

    public class ConditionalValue : StringIdentityValue
    {
        private readonly Predicate<IValueCalculationContext> _calculate;

        public ConditionalValue(Predicate<IValueCalculationContext> calculate, string identity) 
            : base(identity) =>
            _calculate = calculate;

        public override NodeValue? Calculate(IValueCalculationContext context) =>
            (NodeValue?) _calculate(context);
    }

    public class CountingValue : StringIdentityValue
    {
        private readonly IReadOnlyList<IValue> _values;

        public CountingValue(IReadOnlyList<IValue> values)
            : base($"Count({string.Join(", ", values)})")
        {
            _values = values;
        }

        public override NodeValue? Calculate(IValueCalculationContext context) =>
            _values.Select(v => v.Calculate(context))
                .Select(v => new NodeValue(v.IsTrue() ? 1 : 0))
                .Sum();
    }
}