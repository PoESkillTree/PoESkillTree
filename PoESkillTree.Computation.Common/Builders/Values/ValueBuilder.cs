using System;
using PoESkillTree.Computation.Common.Builders.Conditions;
using PoESkillTree.Computation.Common.Builders.Resolving;

namespace PoESkillTree.Computation.Common.Builders.Values
{
    /// <inheritdoc />
    /// <summary>
    /// Implementation of <see cref="IValueBuilder" /> that overloads conditional and arithmetic operators to allow
    /// much better readable interaction with values.
    /// </summary>
    public sealed class ValueBuilder : IValueBuilder
    {
        private readonly IValueBuilder _value;

        public ValueBuilder(IValueBuilder value)
        {
            _value = value ?? throw new ArgumentNullException(nameof(value));
        }

        private static ValueBuilder Wrap(IValueBuilder value)
        {
            return new ValueBuilder(value);
        }

        public IValueBuilder MinimumOnly => Wrap(_value.MinimumOnly);
        public IValueBuilder MaximumOnly => Wrap(_value.MaximumOnly);

        public override bool Equals(object obj) => 
            ReferenceEquals(this, obj) || (obj is ValueBuilder other && Equals(_value, other._value));

        public override int GetHashCode() => _value.GetHashCode();

        public static IConditionBuilder operator ==(ValueBuilder left, ValueBuilder right) => 
            Eq(left, right);

        public static IConditionBuilder operator ==(ValueBuilder left, double right) => 
            Eq(left, right);

        public static IConditionBuilder operator ==(double left, ValueBuilder right) => 
            Eq(right, left);

        public static IConditionBuilder operator !=(ValueBuilder left, ValueBuilder right) => 
            (left == right).Not;

        public static IConditionBuilder operator !=(ValueBuilder left, double right) => 
            (left == right).Not;

        public static IConditionBuilder operator !=(double left, ValueBuilder right) => 
            (left == right).Not;

        private static IConditionBuilder Eq(IValueBuilder left, IValueBuilder right) =>
            left.Eq(right);

        private static IConditionBuilder Eq(IValueBuilder left, double right) => 
            left.Eq(right);

        IConditionBuilder IValueBuilder.Eq(IValueBuilder other) => _value.Eq(other);

        IConditionBuilder IValueBuilder.Eq(double other) => _value.Eq(other);

        public static IConditionBuilder operator >(ValueBuilder left, ValueBuilder right) => 
            left._value.GreaterThan(right);

        public static IConditionBuilder operator >(ValueBuilder left, double right) => 
            left._value.GreaterThan(right);

        public static IConditionBuilder operator >=(ValueBuilder left, ValueBuilder right) => 
            (left == right).Or(left > right);

        public static IConditionBuilder operator >=(ValueBuilder left, double right) => 
            (left == right).Or(left > right);

        public static IConditionBuilder operator <=(ValueBuilder left, ValueBuilder right) => 
            (left > right).Not;

        public static IConditionBuilder operator <=(ValueBuilder left, double right) => 
            (left > right).Not;

        public static IConditionBuilder operator <(ValueBuilder left, ValueBuilder right) => 
            (left >= right).Not;

        public static IConditionBuilder operator <(ValueBuilder left, double right) => 
            (left >= right).Not;

        public static IConditionBuilder operator >=(double left, ValueBuilder right) => 
            right < left;

        public static IConditionBuilder operator <=(double left, ValueBuilder right) => 
            right > left;

        public static IConditionBuilder operator >(double left, ValueBuilder right) => 
            right <= left;

        public static IConditionBuilder operator <(double left, ValueBuilder right) => 
            right >= left;

        IConditionBuilder IValueBuilder.GreaterThan(IValueBuilder other) =>
            _value.GreaterThan(other);

        IConditionBuilder IValueBuilder.GreaterThan(double other) => 
            _value.GreaterThan(other);

        public static ValueBuilder operator *(ValueBuilder left, ValueBuilder right) => 
            Wrap(left._value.Multiply(right));

        public static ValueBuilder operator *(ValueBuilder left, double right) => 
            Wrap(left._value.Multiply(right));

        public static ValueBuilder operator *(double left, ValueBuilder right) => 
            right * left;

        public static ValueBuilder operator /(ValueBuilder left, ValueBuilder right) => 
            Wrap(left._value.AsDividend(right));

        public static ValueBuilder operator /(ValueBuilder left, double right) => 
            Wrap(left._value.AsDividend(right));

        public static ValueBuilder operator /(double left, ValueBuilder right) => 
            Wrap(right._value.AsDivisor(left));

        public static ValueBuilder operator -(ValueBuilder left, ValueBuilder right) => 
            left + (-right);

        public static ValueBuilder operator -(ValueBuilder left, double right) => 
            left + (-right);

        public static ValueBuilder operator -(double left, ValueBuilder right) => 
            (-right) + left;

        public static ValueBuilder operator -(ValueBuilder value) => 
            value * -1;

        public static ValueBuilder operator +(ValueBuilder left, ValueBuilder right) => 
            Wrap(left._value.Add(right));

        public static ValueBuilder operator +(ValueBuilder left, double right) => 
            Wrap(left._value.Add(right));

        public static ValueBuilder operator +(double left, ValueBuilder right) => 
            right + left;

        IValueBuilder IValueBuilder.Add(IValueBuilder other)=> _value.Add(other);

        IValueBuilder IValueBuilder.Add(double other) => _value.Add(other);

        IValueBuilder IValueBuilder.Multiply(IValueBuilder other) => _value.Multiply(other);

        IValueBuilder IValueBuilder.Multiply(double other) => _value.Multiply(other);

        IValueBuilder IValueBuilder.AsDividend(IValueBuilder divisor) => _value.AsDividend(divisor);

        IValueBuilder IValueBuilder.AsDividend(double divisor) => _value.AsDividend(divisor);

        IValueBuilder IValueBuilder.AsDivisor(double dividend) => _value.AsDivisor(dividend);

        /// <summary>
        /// Divides this value by 100.
        /// </summary>
        public ValueBuilder AsPercentage => this / 100;

        /// <summary>
        /// Divides 1 by this value.
        /// </summary>
        public ValueBuilder Invert => 1 / this;

        IValueBuilder IValueBuilder.Round => _value.Round;
        public ValueBuilder Round => Wrap(_value.Round);

        IValueBuilder IValueBuilder.Floor => _value.Floor;
        public ValueBuilder Floor => Wrap(_value.Floor);

        IValueBuilder IValueBuilder.Ceiling => _value.Ceiling;

        public ValueBuilder Ceiling => Wrap(_value.Ceiling);

        public IValueBuilder Resolve(ResolveContext context) => 
            _value.Resolve(context);

        IValue IValueBuilder.Build() => _value.Build();

        public override string ToString() => _value.ToString();
    }
}