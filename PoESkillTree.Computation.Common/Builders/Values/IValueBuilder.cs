using System;
using PoESkillTree.Computation.Common.Builders.Conditions;
using PoESkillTree.Computation.Common.Builders.Resolving;

namespace PoESkillTree.Computation.Common.Builders.Values
{
    /// <summary>
    /// Represents a value.
    /// </summary>
    public interface IValueBuilder : IResolvable<IValueBuilder>
    {
        /// <summary>
        /// Gets a value equivalent to this value but only affecting the maximum value of stats instead of both.
        /// </summary>
        IValueBuilder MaximumOnly { get; }

        /// <summary>
        /// Gets a value equal to the average of this value's minimum and maximum.
        /// </summary>
        IValueBuilder Average { get; }

        /// <summary> 
        /// Returns a condition that is satisfied if this value is equal to the given value. 
        /// </summary> 
        IConditionBuilder Eq(IValueBuilder other);

        /// <summary>
        /// Returns a condition that is satisfied if this value is greater than the given value.
        /// Null values are interpreted as 0.
        /// </summary>
        IConditionBuilder GreaterThan(IValueBuilder other);

        /// <summary>
        /// Returns a new value that is equal to the sum of this and the given value.
        /// If this or other has a value of null, the non-null value is used. If both have null values, the result is
        /// also null.
        /// </summary>
        IValueBuilder Add(IValueBuilder other);

        /// <summary>
        /// Returns a new value that is equal to the product of this and the given value.
        /// If one or both operands are null, the result is also null.
        /// </summary>
        IValueBuilder Multiply(IValueBuilder other);

        /// <summary>
        /// Returns a new value that is equal to this value divided by the given value.
        /// If one or both operands are null, the result is also null.
        /// </summary>
        IValueBuilder DivideBy(IValueBuilder divisor);

        /// <summary>
        /// Returns a new value that is equal to this value if the given value is true and null otherwise.
        /// </summary>
        IValueBuilder If(IValue condition);

        /// <summary>
        /// Returns a value that is equal to this value passed to <paramref name="selector"/>.
        /// </summary>
        IValueBuilder Select(Func<double, double> selector, Func<IValue, string> identity);

        /// <summary>
        /// Creates a value using the same implementation as this instance.
        /// </summary>
        IValueBuilder Create(double value);

        /// <summary>
        /// Builds this instance into an <see cref="IValue"/>.
        /// </summary>
        IValue Build(BuildParameters parameters);
    }
}