using System.Collections.Generic;
using PoESkillTree.Computation.Common;

namespace PoESkillTree.Computation.Core.Graphs
{
    public interface ICalculationGraph
        : IReadOnlyStatGraphCollection, IEnumerable<IReadOnlyStatGraph>, IModifierCollection
    {
        IReadOnlyDictionary<IStat, IStatGraph> StatGraphs { get; }
        void Remove(IStat stat);
    }

    public interface IReadOnlyStatGraphCollection
    {
        IReadOnlyStatGraph GetOrAdd(IStat stat);
    }
}