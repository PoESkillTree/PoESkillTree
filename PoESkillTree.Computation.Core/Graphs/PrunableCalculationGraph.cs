using System.Collections;
using System.Collections.Generic;
using System.Linq;
using MoreLinq;
using PoESkillTree.Computation.Common;
using PoESkillTree.Utils.Extensions;

namespace PoESkillTree.Computation.Core.Graphs
{
    /// <summary>
    /// Decorating implementation of <see cref="ICalculationGraph"/> that stores stat subgraphs without modifiers and
    /// implements <see cref="ICalculationGraphPruner"/> to remove their unused nodes.
    /// </summary>
    public class PrunableCalculationGraph
        : ICalculationGraph, ICalculationGraphPruner
    {
        private readonly ICalculationGraph _decoratedGraph;
        private readonly IDeterminesNodeRemoval _nodeRemovalDeterminer;
        private readonly HashSet<IStat> _statsWithoutModifiers = new HashSet<IStat>();

        public PrunableCalculationGraph(ICalculationGraph decoratedGraph, IDeterminesNodeRemoval nodeRemovalDeterminer)
        {
            _decoratedGraph = decoratedGraph;
            _nodeRemovalDeterminer = nodeRemovalDeterminer;
        }

        public IEnumerator<IReadOnlyStatGraph> GetEnumerator() => _decoratedGraph.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        public IReadOnlyDictionary<IStat, IStatGraph> StatGraphs => _decoratedGraph.StatGraphs;

        public IReadOnlyStatGraph GetOrAdd(IStat stat)
        {
            if (!StatGraphs.ContainsKey(stat))
            {
                _statsWithoutModifiers.Add(stat);
            }

            return _decoratedGraph.GetOrAdd(stat);
        }

        public void Remove(IStat stat)
        {
            _statsWithoutModifiers.Remove(stat);
            _decoratedGraph.Remove(stat);
        }

        public void AddModifier(Modifier modifier)
        {
            _statsWithoutModifiers.ExceptWith(modifier.Stats);
            _decoratedGraph.AddModifier(modifier);
        }

        public void RemoveModifier(Modifier modifier)
        {
            _decoratedGraph.RemoveModifier(modifier);
            _statsWithoutModifiers.UnionWith(modifier.Stats
                .Where(s => StatGraphs.ContainsKey(s))
                .Where(s => StatGraphs[s].ModifierCount == 0));
        }

        public void RemoveUnusedNodes()
        {
            _statsWithoutModifiers.ForEach(RemoveUnusedStatGraphNodes);
            SelectRemovableStats()
                .ToList().ForEach(Remove);
        }

        private void RemoveUnusedStatGraphNodes(IStat stat)
        {
            var statGraph = StatGraphs[stat];
            SelectRemovableNodesByNodeType(statGraph)
                .ForEach(statGraph.RemoveNode);
            SelectRemovableNodesByForm(statGraph)
                .ForEach(statGraph.RemoveFormNodeCollection);
        }

        private IEnumerable<NodeSelector> SelectRemovableNodesByNodeType(IReadOnlyStatGraph statGraph)
            => statGraph.Nodes.OrderBy(p => p.Key.NodeType).ToList()
                .Where(p => _nodeRemovalDeterminer.CanBeRemoved(p.Value))
                .Select(p => p.Key);

        private IEnumerable<FormNodeSelector> SelectRemovableNodesByForm(IReadOnlyStatGraph statGraph)
            => statGraph.FormNodeCollections.ToList()
                .Where(p => _nodeRemovalDeterminer.CanBeRemoved(p.Value))
                .Select(p => p.Key);

        private IEnumerable<IStat> SelectRemovableStats()
            => _statsWithoutModifiers.Where(s => CanStatGraphBeRemoved(StatGraphs[s]));

        private bool CanStatGraphBeRemoved(IReadOnlyStatGraph statGraph) =>
            statGraph.Nodes.IsEmpty() && statGraph.FormNodeCollections.IsEmpty()
                                      && _nodeRemovalDeterminer.CanBeRemoved(statGraph.Paths);
    }
}