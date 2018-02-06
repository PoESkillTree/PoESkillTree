using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using MoreLinq;
using PoESkillTree.Common.Utils.Extensions;
using PoESkillTree.Computation.Common;

namespace PoESkillTree.Computation.Core.Graphs
{
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
            _statsWithoutModifiers.UnionWith(modifier.Stats
                .Where(s => StatGraphs.ContainsKey(s))
                .Where(s => StatGraphs[s].ModifierCount == 0));
            _decoratedGraph.RemoveModifier(modifier);
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
                .ToList().ForEach(statGraph.RemoveNode);
            SelectRemovableNodesByForm(statGraph)
                .ToList().ForEach(statGraph.RemoveFormNodeCollection);
        }

        private IEnumerable<NodeType> SelectRemovableNodesByNodeType(IReadOnlyStatGraph statGraph) =>
            from nodeType in Enum.GetValues(typeof(NodeType)).Cast<NodeType>()
            where statGraph.Nodes.ContainsKey(nodeType)
            where _nodeRemovalDeterminer.CanBeRemoved(statGraph.Nodes[nodeType])
            select nodeType;

        private IEnumerable<Form> SelectRemovableNodesByForm(IReadOnlyStatGraph statGraph) =>
            from pair in statGraph.FormNodeCollections
            where _nodeRemovalDeterminer.CanBeRemoved(pair.Value)
            select pair.Key;

        private IEnumerable<IStat> SelectRemovableStats() =>
            from stat in _statsWithoutModifiers
            let statGraph = StatGraphs[stat]
            where CanStatGraphBeRemoved(statGraph)
            select stat;

        private static bool CanStatGraphBeRemoved(IReadOnlyStatGraph statGraph) =>
            statGraph.Nodes.IsEmpty() && statGraph.FormNodeCollections.IsEmpty();
    }
}