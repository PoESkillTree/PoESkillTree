using System.Collections.Generic;
using System.Linq;
using PoESkillTree.Computation.Common;

namespace PoESkillTree.Computation.Core.Graphs
{
    /// <summary>
    /// Removes unused nodes and stats as declared by an <see cref="IGraphPruningRuleSet"/>.
    /// </summary>
    public class CalculationGraphPruner : ICalculationGraphPruner
    {
        private readonly ICalculationGraph _calculationGraph;
        private readonly IGraphPruningRuleSet _ruleSet;
        private readonly HashSet<IStat> _statsConsideredForRemoval = new HashSet<IStat>();

        public CalculationGraphPruner(ICalculationGraph calculationGraph, IGraphPruningRuleSet ruleSet)
            => (_calculationGraph, _ruleSet) = (calculationGraph, ruleSet);

        public void StatAdded(IStat stat)
        {
            if (CanBeConsideredForRemoval(stat))
            {
                _statsConsideredForRemoval.Add(stat);
            }
        }

        public void StatRemoved(IStat stat)
            => _statsConsideredForRemoval.Remove(stat);

        public void ModifierAdded(Modifier modifier)
            => _statsConsideredForRemoval.ExceptWith(modifier.Stats.Where(s => !CanBeConsideredForRemoval(s)));

        public void ModifierRemoved(Modifier modifier)
            => _statsConsideredForRemoval.UnionWith(modifier.Stats.Where(CanBeConsideredForRemoval));

        private bool CanBeConsideredForRemoval(IStat stat)
            => _calculationGraph.StatGraphs.TryGetValue(stat, out var statGraph)
               && _ruleSet.CanStatBeConsideredForRemoval(stat, statGraph);

        public void RemoveUnusedNodes()
        {
            _statsConsideredForRemoval.RemoveWhere(s => !_calculationGraph.StatGraphs.ContainsKey(s));

            foreach (var stat in _statsConsideredForRemoval)
            {
                RemoveUnusedStatGraphNodes(stat);
            }

            foreach (var stat in _statsConsideredForRemoval.ToList())
            {
                var statGraph = _calculationGraph.StatGraphs[stat];
                if (!_ruleSet.CanStatGraphBeRemoved(statGraph))
                    continue;

                ClearStatGraph(statGraph);
                _calculationGraph.Remove(stat);
            }
        }

        private void RemoveUnusedStatGraphNodes(IStat stat)
        {
            var statGraph = _calculationGraph.StatGraphs[stat];
            foreach (var selector in _ruleSet.SelectRemovableNodesByNodeType(statGraph))
                statGraph.RemoveNode(selector);
            foreach (var selector in _ruleSet.SelectRemovableNodesByForm(statGraph))
                statGraph.RemoveFormNodeCollection(selector);
        }

        private void ClearStatGraph(IStatGraph statGraph)
        {
            statGraph.FormNodeCollections.Values
                .SelectMany(p => p.DefaultView.Select(t => t.property))
                .ToList().ForEach(_calculationGraph.RemoveModifier);
            statGraph.FormNodeCollections.Keys
                .ToList().ForEach(statGraph.RemoveFormNodeCollection);
        }
    }
}