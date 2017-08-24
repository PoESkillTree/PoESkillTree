using System;
using PoESkillTree.Computation.Parsing.Builders.Conditions;

namespace PoESkillTree.Computation.Parsing.Builders.Values
{
    public class ValueBuilder : IValueBuilder
    {
        public ValueBuilder(IValueBuilder value)
        {
        }

        // If the == and != overloads make implementing the class difficult, they can easily be
        // removed and usages replaced by <= or >=
        public static IConditionBuilder operator ==(ValueBuilder left, ValueBuilder right)
        {
            throw new NotImplementedException();
        }

        public static IConditionBuilder operator ==(ValueBuilder left, double right)
        {
            throw new NotImplementedException();
        }

        public static IConditionBuilder operator ==(double left, ValueBuilder right)
        {
            throw new NotImplementedException();
        }

        public static IConditionBuilder operator !=(ValueBuilder left, ValueBuilder right)
        {
            throw new NotImplementedException();
        }

        public static IConditionBuilder operator !=(ValueBuilder left, double right)
        {
            throw new NotImplementedException();
        }

        public static IConditionBuilder operator !=(double left, ValueBuilder right)
        {
            throw new NotImplementedException();
        }

        public static IConditionBuilder operator >=(ValueBuilder left, ValueBuilder right)
        {
            throw new NotImplementedException();
        }

        public static IConditionBuilder operator >=(ValueBuilder left, double right)
        {
            throw new NotImplementedException();
        }

        public static IConditionBuilder operator >=(double left, ValueBuilder right)
        {
            throw new NotImplementedException();
        }

        public static IConditionBuilder operator <=(ValueBuilder left, ValueBuilder right)
        {
            throw new NotImplementedException();
        }

        public static IConditionBuilder operator <=(ValueBuilder left, double right)
        {
            throw new NotImplementedException();
        }

        public static IConditionBuilder operator <=(double left, ValueBuilder right)
        {
            throw new NotImplementedException();
        }

        public static IConditionBuilder operator >(ValueBuilder left, ValueBuilder right)
        {
            throw new NotImplementedException();
        }

        public static IConditionBuilder operator >(ValueBuilder left, double right)
        {
            throw new NotImplementedException();
        }

        public static IConditionBuilder operator >(double left, ValueBuilder right)
        {
            throw new NotImplementedException();
        }

        public static IConditionBuilder operator <(ValueBuilder left, ValueBuilder right)
        {
            throw new NotImplementedException();
        }

        public static IConditionBuilder operator <(ValueBuilder left, double right)
        {
            throw new NotImplementedException();
        }

        public static IConditionBuilder operator <(double left, ValueBuilder right)
        {
            throw new NotImplementedException();
        }

        public static ValueBuilder operator *(ValueBuilder left, ValueBuilder right)
        {
            throw new NotImplementedException();
        }

        public static ValueBuilder operator *(ValueBuilder left, double right)
        {
            throw new NotImplementedException();
        }

        public static ValueBuilder operator *(double left, ValueBuilder right)
        {
            throw new NotImplementedException();
        }

        public static ValueBuilder operator /(ValueBuilder left, ValueBuilder right)
        {
            throw new NotImplementedException();
        }

        public static ValueBuilder operator /(ValueBuilder left, double right)
        {
            throw new NotImplementedException();
        }

        public static ValueBuilder operator /(double left, ValueBuilder right)
        {
            throw new NotImplementedException();
        }

        public static ValueBuilder operator -(ValueBuilder left, ValueBuilder right)
        {
            throw new NotImplementedException();
        }

        public static ValueBuilder operator -(ValueBuilder left, double right)
        {
            throw new NotImplementedException();
        }

        public static ValueBuilder operator -(double left, ValueBuilder right)
        {
            throw new NotImplementedException();
        }

        public static ValueBuilder operator +(ValueBuilder left, ValueBuilder right)
        {
            throw new NotImplementedException();
        }

        public static ValueBuilder operator +(ValueBuilder left, double right)
        {
            throw new NotImplementedException();
        }

        public static ValueBuilder operator +(double left, ValueBuilder right)
        {
            throw new NotImplementedException();
        }

        public ValueBuilder AsPercentage => this / 100;
        public ValueBuilder Invert => 1 / this;

        // to how many digits depends on the number of significant digits the value has
        // they also need to be rounded to more digits before floored/ceiled to avoid e.g. 0.99999 being floored to 0
        public ValueBuilder Rounded => throw new NotImplementedException();
        public ValueBuilder Floored => throw new NotImplementedException();
        public ValueBuilder Ceiled => throw new NotImplementedException();
    }
}