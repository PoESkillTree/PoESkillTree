using System.Collections.Generic;
using PoESkillTree.Computation.Common;

namespace PoESkillTree.Computation.Core
{
    public interface ICalculationGraph : IReadOnlyStatGraphCollection, IModifierCollection
    {
        IReadOnlyDictionary<IStat, IStatGraph> StatGraphs { get; }
        void Remove(IStat stat);
    }

    public interface IReadOnlyStatGraphCollection : IEnumerable<IReadOnlyStatGraph>
    {
        IReadOnlyStatGraph GetOrAdd(IStat stat);
    }
}