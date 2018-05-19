using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using PoESkillTree.Computation.Common;

namespace PoESkillTree.Computation.Core.Graphs
{
    /// <summary>
    /// Decorating implementation of <see cref="ICalculationGraph"/> that triggers actions when stat subgraphs are
    /// added or removed.
    /// </summary>
    public class CalculationGraphWithEvents : ICalculationGraph
    {
        private readonly ICalculationGraph _decoratedGraph;
        private readonly Action<IStat> _statAddedAction;
        private readonly Action<IStat> _statRemovedAction;

        public CalculationGraphWithEvents(ICalculationGraph decoratedGraph,
            Action<IStat> statAddedAction, Action<IStat> statRemovedAction)
        {
            _decoratedGraph = decoratedGraph;
            _statAddedAction = statAddedAction;
            _statRemovedAction = statRemovedAction;
        }

        public IEnumerator<IReadOnlyStatGraph> GetEnumerator() => _decoratedGraph.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        public IReadOnlyDictionary<IStat, IStatGraph> StatGraphs => _decoratedGraph.StatGraphs;

        public IReadOnlyStatGraph GetOrAdd(IStat stat)
        {
            var statIsNew = !StatGraphs.ContainsKey(stat);
            var statGraph = _decoratedGraph.GetOrAdd(stat);
            if (statIsNew)
            {
                _statAddedAction(stat);
            }
            return statGraph;
        }

        public void AddModifier(Modifier modifier)
        {
            var newStats = modifier.Stats.Where(s => !StatGraphs.ContainsKey(s)).ToList();
            _decoratedGraph.AddModifier(modifier);
            foreach (var stat in newStats)
            {
                _statAddedAction(stat);
            }
        }

        public void RemoveModifier(Modifier modifier) => _decoratedGraph.RemoveModifier(modifier);

        public void Remove(IStat stat)
        {
            _decoratedGraph.Remove(stat);
            _statRemovedAction(stat);
        }
    }
}