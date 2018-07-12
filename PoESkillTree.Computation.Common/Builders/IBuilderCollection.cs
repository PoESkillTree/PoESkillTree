using System;
using PoESkillTree.Computation.Common.Builders.Conditions;
using PoESkillTree.Computation.Common.Builders.Resolving;
using PoESkillTree.Computation.Common.Builders.Values;

namespace PoESkillTree.Computation.Common.Builders
{
    /// <summary>
    /// Represents a collection of builders that has Count and Any methods similar to LINQ but using
    /// <see cref="IConditionBuilder"/>s instead of booleans.
    /// </summary>
    /// <typeparam name="T">The type of the builders.</typeparam>
    public interface IBuilderCollection<out T> : IBuilderCollection
    {
        /// <returns>The number of elements in this collection matching the given predicate.</returns>
        ValueBuilder Count(Func<T, IConditionBuilder> predicate);

        /// <returns>True if any element in this collection matches the given predicate. True if there are any elements
        /// at all in this collection if no predicate is given.</returns>
        IConditionBuilder Any(Func<T, IConditionBuilder> predicate);
    }
    
    /// <summary>
    /// Represents a collection of builders that has Count and Any methods similar to LINQ but using
    /// <see cref="IConditionBuilder"/>s instead of booleans.
    /// </summary>
    public interface IBuilderCollection : IResolvable<IBuilderCollection>
    {
        /// <returns>The number of elements in this collection.</returns>
        ValueBuilder Count();
        
        /// <returns>True if there are any elements at all in this collection.</returns>
        IConditionBuilder Any();
    }
}