using System;
using PoESkillTree.Computation.Parsing.Builders.Conditions;
using PoESkillTree.Computation.Parsing.Builders.Matching;
using PoESkillTree.Computation.Parsing.Builders.Values;

namespace PoESkillTree.Computation.Parsing.Builders
{
    /// <summary>
    /// Represents a collection of builders that has Count and Any methods similar to LINQ but using
    /// <see cref="IConditionBuilder"/>s instead of booleans.
    /// </summary>
    /// <typeparam name="T">The type of the builders.</typeparam>
    public interface IBuilderCollection<out T> : IResolvable<IBuilderCollection<T>>
    {
        /// <returns>The number of elements in this collection matching the given predicate. Total number if no
        /// predicate is given.</returns>
        ValueBuilder Count(Func<T, IConditionBuilder> predicate = null);

        /// <returns>True if any element in this collection matches the given predicate. True if there are any elements
        /// at all in this collection if no predicate is given.</returns>
        IConditionBuilder Any(Func<T, IConditionBuilder> predicate = null);
    }
}