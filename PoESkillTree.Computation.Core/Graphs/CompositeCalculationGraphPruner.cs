using System.Collections.Generic;
using MoreLinq;

namespace PoESkillTree.Computation.Core.Graphs
{
    public class CompositeCalculationGraphPruner : ICalculationGraphPruner
    {
        private readonly IReadOnlyList<ICalculationGraphPruner> _elements;

        public CompositeCalculationGraphPruner(params ICalculationGraphPruner[] elements)
            => _elements = elements;

        public void RemoveUnusedNodes()
            => _elements.ForEach(p => p.RemoveUnusedNodes());
    }
}