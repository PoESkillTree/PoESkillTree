using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using MoreLinq;
using PoESkillTree.Common.Utils.Extensions;
using PoESkillTree.Computation.Common;

namespace PoESkillTree.Computation.Core.Graphs
{
    public class CoreCalculationGraph : ICalculationGraph
    {
        private readonly Func<IStat, IStatGraph> _statGraphFactory;
        private readonly Dictionary<IStat, IStatGraph> _statGraphs = new Dictionary<IStat, IStatGraph>();

        public CoreCalculationGraph(Func<IStat, IStatGraph> statGraphFactory)
        {
            _statGraphFactory = statGraphFactory;
        }

        public IEnumerator<IReadOnlyStatGraph> GetEnumerator() => 
            _statGraphs.Values.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        private IStatGraph GetOrAddStatGraph(IStat stat) => 
            _statGraphs.GetOrAdd(stat, _statGraphFactory);

        public IReadOnlyStatGraph GetOrAdd(IStat stat) => GetOrAddStatGraph(stat);

        public IReadOnlyDictionary<IStat, IStatGraph> StatGraphs => _statGraphs;

        public void Remove(IStat stat) => _statGraphs.Remove(stat);

        public void AddModifier(Modifier modifier)
        {
            modifier.Stats
                .Select(GetOrAddStatGraph)
                .ForEach(g => g.AddModifier(modifier));
        }

        public void RemoveModifier(Modifier modifier)
        {
            modifier.Stats
                .Where(s => _statGraphs.ContainsKey(s))
                .Select(s => _statGraphs[s])
                .ForEach(g => g.RemoveModifier(modifier));
        }
    }
}