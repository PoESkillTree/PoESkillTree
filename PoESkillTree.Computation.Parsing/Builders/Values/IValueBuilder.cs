using PoESkillTree.Computation.Parsing.Builders.Conditions;
using PoESkillTree.Computation.Parsing.Builders.Matching;

namespace PoESkillTree.Computation.Parsing.Builders.Values
{
    /// <summary>
    /// Represents a value.
    /// </summary>
    /// <remarks>
    /// Values have a specific amount of significant digits depending on the stat they are for. This amount is relevant
    /// for the rounding methods.
    /// </remarks>
    public interface IValueBuilder : IResolvable<IValueBuilder>
    {
        /// <summary>
        /// Returns a conditiosn that is satisifed if this value is equal to the given value.
        /// </summary>
        IConditionBuilder Eq(IValueBuilder other);

        /// <summary>
        /// Returns a conditiosn that is satisifed if this value is equal to the given value.
        /// </summary>
        IConditionBuilder Eq(double other);

        /// <summary>
        /// Returns a conditiosn that is satisifed if this value is greater than to the given value.
        /// </summary>
        IConditionBuilder GreaterThan(IValueBuilder other);

        /// <summary>
        /// Returns a conditiosn that is satisifed if this value is greater than to the given value.
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
        /// Returns a value that is equal to this value rounded to its amount of significant digits.
        /// </summary>
        IValueBuilder Rounded { get; }

        /// <summary>
        /// Returns a value that is equal to this value floored to its amount of significant digits. It is first
        /// rounded to a higher amount of significant digits to prevent issues with floating point accuracy 
        /// like 0.99999 being floored to 0.
        /// </summary>
        IValueBuilder Floored { get; }

        /// <summary>
        /// Returns a value that is equal to this value ceiled to its amount of significant digits. It is first
        /// rounded to a higher amount of significant digits to prevent issues with floating point accuracy
        /// like 1.000001 being ceiled to 2.
        /// </summary>
        IValueBuilder Ceiled { get; }
    }
}