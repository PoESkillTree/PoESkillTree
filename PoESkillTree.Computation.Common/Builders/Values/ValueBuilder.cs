using System;
using System.Linq.Expressions;
using PoESkillTree.Computation.Common.Builders.Conditions;
using PoESkillTree.Computation.Common.Builders.Resolving;

namespace PoESkillTree.Computation.Common.Builders.Values
{
    /// <inheritdoc />
    /// <summary>
    /// Implementation of <see cref="IValueBuilder" /> that overloads conditional and arithmetic operators to allow
    /// much more readable interaction with values.
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

        public IValueBuilder MaximumOnly => Wrap(_value.MaximumOnly);

        public override bool Equals(object obj) =>
            ReferenceEquals(this, obj) || (obj is ValueBuilder other && Equals(_value, other._value));

        public override int GetHashCode() => _value.GetHashCode();


        public IConditionBuilder Eq(IValueBuilder other) =>
            _value.Eq(other);

        public IConditionBuilder Eq(double other) =>
            _value.Eq(Create(other));

        public static IConditionBuilder operator >(ValueBuilder left, ValueBuilder right) =>
            left._value.GreaterThan(right);

        public static IConditionBuilder operator >=(ValueBuilder left, ValueBuilder right) =>
            (right > left).Not;

        public static IConditionBuilder operator <=(ValueBuilder left, ValueBuilder right) =>
            right >= left;

        public static IConditionBuilder operator <(ValueBuilder left, ValueBuilder right) =>
            right < left;

        public static IConditionBuilder operator >(ValueBuilder left, double right) =>
            left > left.Create(right);

        public static IConditionBuilder operator >=(ValueBuilder left, double right) =>
            left >= left.Create(right);

        public static IConditionBuilder operator <=(ValueBuilder left, double right) =>
            left <= left.Create(right);

        public static IConditionBuilder operator <(ValueBuilder left, double right) =>
            left < left.Create(right);

        public static IConditionBuilder operator >(double left, ValueBuilder right) =>
            right.Create(left) > right;

        public static IConditionBuilder operator >=(double left, ValueBuilder right) =>
            right.Create(left) >= right;

        public static IConditionBuilder operator <=(double left, ValueBuilder right) =>
            right.Create(left) <= right;

        public static IConditionBuilder operator <(double left, ValueBuilder right) =>
            right.Create(left) < right;

        IConditionBuilder IValueBuilder.GreaterThan(IValueBuilder other) =>
            _value.GreaterThan(other);


        public static ValueBuilder operator *(ValueBuilder left, ValueBuilder right) =>
            Wrap(left._value.Multiply(right));

        public static ValueBuilder operator *(ValueBuilder left, double right) =>
            left * left.Create(right);

        public static ValueBuilder operator *(double left, ValueBuilder right) =>
            right.Create(left) * right;

        public static ValueBuilder operator /(ValueBuilder left, ValueBuilder right) =>
            Wrap(left._value.DivideBy(right));

        public static ValueBuilder operator /(ValueBuilder left, double right) =>
            left / left.Create(right);

        public static ValueBuilder operator /(double left, ValueBuilder right) =>
            right.Create(left) / right;

        public static ValueBuilder operator -(ValueBuilder left, ValueBuilder right) =>
            left + (-right);

        public static ValueBuilder operator -(ValueBuilder left, double right) =>
            left - left.Create(right);

        public static ValueBuilder operator -(double left, ValueBuilder right) =>
            right.Create(left) - right;

        public static ValueBuilder operator -(ValueBuilder value) =>
            value * -1;

        public static ValueBuilder operator +(ValueBuilder left, ValueBuilder right) =>
            Wrap(left._value.Add(right));

        public static ValueBuilder operator +(ValueBuilder left, double right) =>
            left + left.Create(right);

        public static ValueBuilder operator +(double left, ValueBuilder right) =>
            right.Create(left) + right;

        IValueBuilder IValueBuilder.Add(IValueBuilder other) => _value.Add(other);

        IValueBuilder IValueBuilder.Multiply(IValueBuilder other) => _value.Multiply(other);

        IValueBuilder IValueBuilder.DivideBy(IValueBuilder divisor) => _value.DivideBy(divisor);

        IValueBuilder IValueBuilder.If(IValue condition) => _value.If(condition);


        /// <summary>
        /// Divides this value by 100.
        /// </summary>
        public ValueBuilder AsPercentage => this / 100;

        /// <summary>
        /// Divides 1 by this value.
        /// </summary>
        public ValueBuilder Invert => 1 / this;

        IValueBuilder IValueBuilder.Select(Func<double, double> selector, Func<IValue, string> identity) =>
            _value.Select(selector, identity);

        public ValueBuilder Select(Func<double, double> selector, Func<IValue, string> identity) =>
            Wrap(_value.Select(selector, identity));

        IValueBuilder IValueBuilder.Create(double value) => _value.Create(value);
        private ValueBuilder Create(double value) => Wrap(_value.Create(value));

        public IValueBuilder Resolve(ResolveContext context) =>
            _value.Resolve(context);

        public IValue Build(BuildParameters parameters) => _value.Build(parameters);

        public override string ToString() => _value.ToString();
    }
}