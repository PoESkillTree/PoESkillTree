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
        /// Returns a condition that is satisfied if this value is equal to the given value.
        /// </summary>
        IConditionBuilder Eq(IValueBuilder other);

        /// <summary>
        /// Returns a condition that is satisfied if this value is equal to the given value.
        /// </summary>
        IConditionBuilder Eq(double other);

        /// <summary>
        /// Returns a condition that is satisfied if this value is greater than to the given value.
        /// </summary>
        IConditionBuilder GreaterThan(IValueBuilder other);

        /// <summary>
        /// Returns a condition that is satisfied if this value is greater than to the given value.
        /// </summary>
        IConditionBuilder GreaterThan(double other);

        /// <summary>
        /// Returns a new value that is equal to the sum of this and the given value.
        /// </summary>
        IValueBuilder Add(IValueBuilder other);

        /// <summary>
        /// Returns a new value that is equal to the sum of this and the given value.
        /// </summary>
        IValueBuilder Add(double other);

        /// <summary>
        /// Returns a new value that is equal to the product of this and the given value.
        /// </summary>
        IValueBuilder Multiply(IValueBuilder other);

        /// <summary>
        /// Returns a new value that is equal to the product of this and the given value.
        /// </summary>
        IValueBuilder Multiply(double other);

        /// <summary>
        /// Returns a new value that is equal to this value divided by the given value.
        /// </summary>
        IValueBuilder AsDividend(IValueBuilder divisor);

        /// <summary>
        /// Returns a new value that is equal to this value divided by the given value.
        /// </summary>
        IValueBuilder AsDividend(double divisor);

        /// <summary>
        /// Returns a new value that is equal to the given value divided by this value.
        /// </summary>
        IValueBuilder AsDivisor(double dividend);

        /// <summary>
        /// Returns a value that is equal to this value rounded to the nearest integral value.
        /// </summary>
        IValueBuilder Round { get; }

        /// <summary>
        /// Returns a value that is equal to the largest integer less than or equal to this value.
        /// </summary>
        IValueBuilder Floor { get; }

        /// <summary>
        /// Returns a value that is equal to the smallest integer greater than or equal to this value.
        /// </summary>
        IValueBuilder Ceiling { get; }
    }
}