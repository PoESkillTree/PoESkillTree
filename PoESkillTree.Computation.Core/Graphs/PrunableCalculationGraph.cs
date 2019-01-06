using System.Collections;
using System.Collections.Generic;
using System.Linq;
using MoreLinq;
using PoESkillTree.Computation.Common;

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
        private readonly IGraphPruningRuleSet _ruleSet;
        private readonly HashSet<IStat> _statsConsideredForRemoval = new HashSet<IStat>();

        public PrunableCalculationGraph(ICalculationGraph decoratedGraph, IGraphPruningRuleSet ruleSet)
            => (_decoratedGraph, _ruleSet) = (decoratedGraph, ruleSet);

        public IEnumerator<IReadOnlyStatGraph> GetEnumerator() => _decoratedGraph.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        public IReadOnlyDictionary<IStat, IStatGraph> StatGraphs => _decoratedGraph.StatGraphs;

        public IReadOnlyStatGraph GetOrAdd(IStat stat)
        {
            if (!StatGraphs.ContainsKey(stat))
            {
                _statsConsideredForRemoval.Add(stat);
            }

            return _decoratedGraph.GetOrAdd(stat);
        }

        public void Remove(IStat stat)
        {
            _statsConsideredForRemoval.Remove(stat);
            _decoratedGraph.Remove(stat);
        }

        public void AddModifier(Modifier modifier)
        {
            _statsConsideredForRemoval.ExceptWith(_ruleSet.SelectStatsNoLongerConsideredForRemoval(modifier));
            _decoratedGraph.AddModifier(modifier);
        }

        public void RemoveModifier(Modifier modifier)
        {
            _decoratedGraph.RemoveModifier(modifier);
            _statsConsideredForRemoval.UnionWith(_ruleSet.SelectStatsConsideredForRemoval(modifier));
        }

        public void RemoveUnusedNodes()
        {
            _statsConsideredForRemoval.ForEach(RemoveUnusedStatGraphNodes);
            _ruleSet.SelectRemovableStats(_statsConsideredForRemoval)
                .ToList().ForEach(Remove);
        }

        private void RemoveUnusedStatGraphNodes(IStat stat)
        {
            var statGraph = StatGraphs[stat];
            _ruleSet.SelectRemovableNodesByNodeType(statGraph)
                .ForEach(statGraph.RemoveNode);
            _ruleSet.SelectRemovableNodesByForm(statGraph)
                .ForEach(statGraph.RemoveFormNodeCollection);
        }
    }
}