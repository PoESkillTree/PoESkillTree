using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using MoreLinq;
using PoESkillTree.Common.Utils.Extensions;
using PoESkillTree.Computation.Common;

namespace PoESkillTree.Computation.Core
{
    public class PrunableCalculationGraph
        : ICalculationGraph, ICalculationGraphPruner
    {
        private readonly ICalculationGraph _decoratedGraph;
        private readonly HashSet<IStat> _statsWithoutModifiers = new HashSet<IStat>();

        public PrunableCalculationGraph(ICalculationGraph decoratedGraph)
        {
            _decoratedGraph = decoratedGraph;
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
            modifier.Stats
                .ForEach(s => _statsWithoutModifiers.Remove(s));
            _decoratedGraph.AddModifier(modifier);
        }

        public void RemoveModifier(Modifier modifier)
        {
            modifier.Stats
                .Where(s => StatGraphs.ContainsKey(s))
                .Where(s => StatGraphs[s].ModifierCount == 0)
                .ForEach(s => _statsWithoutModifiers.Add(s));
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

        private static IEnumerable<NodeType> SelectRemovableNodesByNodeType(IReadOnlyStatGraph statGraph) =>
            from nodeType in Enum.GetValues(typeof(NodeType)).Cast<NodeType>()
            where statGraph.Nodes.ContainsKey(nodeType)
            where statGraph.Nodes[nodeType].SubscriberCount == 0
            select nodeType;

        private static IEnumerable<Form> SelectRemovableNodesByForm(IReadOnlyStatGraph statGraph) =>
            from pair in statGraph.FormNodeCollections
            where pair.Value.SubscriberCount == 0
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