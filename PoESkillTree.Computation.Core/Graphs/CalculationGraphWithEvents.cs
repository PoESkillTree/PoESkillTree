using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using PoESkillTree.Computation.Common;

namespace PoESkillTree.Computation.Core.Graphs
{
    public class CalculationGraphWithEvents : ICalculationGraph
    {
        private readonly ICalculationGraph _decoratedGraph;

        public CalculationGraphWithEvents(ICalculationGraph decoratedGraph)
        {
            _decoratedGraph = decoratedGraph;
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
                StatAdded?.Invoke(this, new StatAddedEventArgs(stat));
            }
            return statGraph;
        }

        public void AddModifier(Modifier modifier)
        {
            var newStats = modifier.Stats.Where(s => !StatGraphs.ContainsKey(s)).ToList();
            _decoratedGraph.AddModifier(modifier);
            foreach (var stat in newStats)
            {
                StatAdded?.Invoke(this, new StatAddedEventArgs(stat));
            }
        }

        public void RemoveModifier(Modifier modifier) => _decoratedGraph.RemoveModifier(modifier);

        public void Remove(IStat stat)
        {
            _decoratedGraph.Remove(stat);
            StatRemoved?.Invoke(this, new StatRemovedEventArgs(stat));
        }

        public event EventHandler<StatAddedEventArgs> StatAdded;
        public event EventHandler<StatRemovedEventArgs> StatRemoved;
    }

    public class StatAddedEventArgs : EventArgs
    {
        public StatAddedEventArgs(IStat stat)
        {
            Stat = stat;
        }

        public IStat Stat { get; }
    }

    public class StatRemovedEventArgs : EventArgs
    {
        public StatRemovedEventArgs(IStat stat)
        {
            Stat = stat;
        }

        public IStat Stat { get; }
    }
}