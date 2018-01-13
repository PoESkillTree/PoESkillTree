using System.Collections.Generic;

namespace PoESkillTree.Computation.Common.Builders.Effects
{
    /// <summary>
    /// Represents a collection of ailments. Implements <see cref="IEnumerable{T}"/> because the elements of
    /// this collection are always known (not dependent on data or resolving).
    /// </summary>
    public interface IAilmentBuilderCollection 
        : IBuilderCollection<IAilmentBuilder>, IEnumerable<IAilmentBuilder>
    {
    }
}