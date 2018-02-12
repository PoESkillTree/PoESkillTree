using System;
using PoESkillTree.Computation.Common.Builders.Conditions;
using PoESkillTree.Computation.Common.Builders.Resolving;

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
        /// Returns a value converter that behaves the same as the given converter but creates a 
        /// <see cref="ValueBuilder"/> from parameters that are <see cref="IValueBuilder"/>s and not 
        /// <see cref="ValueBuilder"/>s.
        /// </summary>
        /// <remarks>
        /// This method can be used when passing converters created in matcher collections (using 
        /// <see cref="ValueBuilder"/> as type) to <see cref="Builders.Modifiers.IModifierBuilder"/> (which uses
        /// <see cref="IValueBuilder"/>).
        /// </remarks>
        Func<IValueBuilder, IValueBuilder> WrapValueConverter(Func<ValueBuilder, ValueBuilder> converter);
    }

    public interface IThenBuilder : IResolvable<IThenBuilder>
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

    public interface IConditionalValueBuilder : IResolvable<IConditionalValueBuilder>
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