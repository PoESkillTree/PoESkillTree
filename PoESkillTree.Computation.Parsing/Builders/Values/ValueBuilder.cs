using System;
using PoESkillTree.Computation.Parsing.Builders.Conditions;
using PoESkillTree.Computation.Parsing.Builders.Matching;

namespace PoESkillTree.Computation.Parsing.Builders.Values
{
    public class ValueBuilder : IValueBuilder
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

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != GetType()) return false;
            return Equals(_value, ((ValueBuilder) obj)._value);
        }

        public override int GetHashCode()
        {
            return _value.GetHashCode();
        }

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

        public ValueBuilder AsPercentage => this / 100;
        public ValueBuilder Invert => 1 / this;

        IValueBuilder IValueBuilder.Rounded => _value.Rounded;
        public ValueBuilder Rounded => Wrap(_value.Rounded);

        IValueBuilder IValueBuilder.Floored => _value.Floored;
        public ValueBuilder Floored => Wrap(_value.Floored);

        IValueBuilder IValueBuilder.Ceiled => _value.Ceiled;
        public ValueBuilder Ceiled => Wrap(_value.Ceiled);

        public IValueBuilder Resolve(ResolveContext context) => 
            _value.Resolve(context);

        public override string ToString()
        {
            return _value.ToString();
        }
    }
}