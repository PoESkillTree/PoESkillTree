using PoESkillTree.Computation.Common.Builders.Conditions;

namespace PoESkillTree.Computation.Common.Builders.Values
{
    /// <summary>
    /// Factory interface for values.
    /// </summary>
    public interface IValueBuilders
    {
        /// <summary>
        /// Starts constructing an if/else if/else-construct with the given condition as if condition.
        /// </summary>
        IThenBuilder If(IConditionBuilder condition);

        /// <summary>
        /// Creates an <see cref="IValueBuilder"/> from the given value.
        /// </summary>
        IValueBuilder Create(double value);

        /// <summary>
        /// Creates an <see cref="IValueBuilder"/> using the first value as minimum and the second as maximum.
        /// </summary>
        IValueBuilder FromMinAndMax(IValueBuilder minimumValue, IValueBuilder maximumValue);
    }

    public interface IThenBuilder
    {
        /// <summary>
        /// Continues constructing an if/else if/else-construct with the given value as return value of the current 
        /// branch.
        /// </summary>
        IConditionalValueBuilder Then(IValueBuilder value);

        /// <summary>
        /// Continues constructing an if/else if/else-construct with the given value as return value of the current 
        /// branch.
        /// </summary>
        IConditionalValueBuilder Then(double value);
    }

    public interface IConditionalValueBuilder
    {
        /// <summary>
        /// Continues constructing an if/else if/else-construct by adding an else-if-branch with the given condition
        /// as its condition.
        /// </summary>
        IThenBuilder ElseIf(IConditionBuilder condition);

        /// <summary>
        /// Terminates constructing an if/else if/else-construct by setting the given value as return value of its
        /// else-branch.
        /// </summary>
        ValueBuilder Else(IValueBuilder value);

        /// <summary>
        /// Terminates constructing an if/else if/else-construct by setting the given value as return value of its
        /// else-branch.
        /// </summary>
        ValueBuilder Else(double value);
    }
}