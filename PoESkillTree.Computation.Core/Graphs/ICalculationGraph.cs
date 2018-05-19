using System.Collections.Generic;
using PoESkillTree.Computation.Common;

namespace PoESkillTree.Computation.Core.Graphs
{
    /// <summary>
    /// Representation of the calculation graph.
    /// </summary>
    public interface ICalculationGraph
        : IStatGraphCollection, IEnumerable<IReadOnlyStatGraph>, IModifierCollection
    {
        IReadOnlyDictionary<IStat, IStatGraph> StatGraphs { get; }
        void Remove(IStat stat);
    }

    public interface IStatGraphCollection
    {
        IReadOnlyStatGraph GetOrAdd(IStat stat);
    }
}