using System;
using System.Collections;
using System.Collections.Generic;
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

        public CalculationGraphWithEvents(ICalculationGraph decoratedGraph)
            => _decoratedGraph = decoratedGraph;

        public IEnumerator<IReadOnlyStatGraph> GetEnumerator() => _decoratedGraph.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        public IReadOnlyDictionary<IStat, IStatGraph> StatGraphs => _decoratedGraph.StatGraphs;

        public IReadOnlyStatGraph GetOrAdd(IStat stat)
        {
            var statIsNew = !StatGraphs.ContainsKey(stat);
            var statGraph = _decoratedGraph.GetOrAdd(stat);
            if (statIsNew)
            {
                StatAdded?.Invoke(stat);
            }
            return statGraph;
        }

        public void AddModifier(Modifier modifier)
        {
            var newStats = new List<IStat>();
            foreach (var stat in modifier.Stats)
            {
                if (!StatGraphs.ContainsKey(stat))
                    newStats.Add(stat);
            }

            _decoratedGraph.AddModifier(modifier);

            foreach (var stat in newStats)
            {
                StatAdded?.Invoke(stat);
            }
            ModifierAdded?.Invoke(modifier);
        }

        public void RemoveModifier(Modifier modifier)
        {
            _decoratedGraph.RemoveModifier(modifier);
            ModifierRemoved?.Invoke(modifier);
        }

        public void Remove(IStat stat)
        {
            _decoratedGraph.Remove(stat);
            StatRemoved?.Invoke(stat);
        }

        public event Action<IStat> StatAdded;
        public event Action<IStat> StatRemoved;
        public event Action<Modifier> ModifierAdded;
        public event Action<Modifier> ModifierRemoved;
    }
}