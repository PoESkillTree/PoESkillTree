using System.Collections;
using System.Collections.Generic;
using PoESkillTree.Computation.Common;

namespace PoESkillTree.Computation.Core
{
    public class PrunableCalculationGraph
        : ICalculationGraph, ICalculationGraphPruner
    {
        private readonly ICalculationGraph _decoratedGraph;
        private readonly HashSet<IStat> _statsWithoutModifiers;

        public PrunableCalculationGraph(ICalculationGraph decoratedGraph)
        {
            _decoratedGraph = decoratedGraph;
        }

        public IEnumerator<IReadOnlyStatGraph> GetEnumerator() => _decoratedGraph.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        public IReadOnlyDictionary<IStat, IStatGraph> StatGraphs => _decoratedGraph.StatGraphs;

        public IReadOnlyStatGraph GetOrAdd(IStat stat)
        {
            /* - If !StatGraphs.Contains(stat): _statsWithoutModifiers.Add(stat)
             */
            return _decoratedGraph.GetOrAdd(stat);
        }

        public void Remove(IStat stat)
        {
            // _statsWithoutModifiers.Remove(stat)
            _decoratedGraph.Remove(stat);
        }

        public void AddModifier(Modifier modifier)
        {
            /* For each stat in modifier.Stats:
             *   _statsWithoutModifiers.Remove(stat)
             */
            _decoratedGraph.AddModifier(modifier);
        }

        public void RemoveModifier(Modifier modifier)
        {
            /* For each stat in modifier.Stats:
             *   If StatGraphs.Contains(stat) && StatGraphs[stat].ModifierCount == 0:
             *     _statsWithoutModifiers.Add(stat)
             */
            _decoratedGraph.RemoveModifier(modifier);
        }

        public void RemoveUnusedNodes()
        {
            /* - For each stat in _statsWithoutModifiers
             *   - statGraph = StatGraphs[stat]
             *   - For each NodeType (top-down):
             *     - If statGraph.Nodes.TryGetNode(stat, nodeType, out var node)
             *       - If node.SubscriberCount == 0: statGraph.RemoveNode(nodeType)
             *   - For each (form, nodeCollection) in statGraph.FormNodeCollections:
             *     - If nodeCollection.SubscriberCount == 0: statGraph.Remove(form)
             *   - If statGraph.Nodes.IsEmpty() && statGraph.FormNodeCollections.IsEmpty():
             *     - TopGraph.RemoveStat(stat)
             * (remove calls need to be done after iterating)
             */
        }
    }
}