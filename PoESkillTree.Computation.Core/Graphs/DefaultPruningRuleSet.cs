using System.Collections.Generic;
using System.Linq;
using PoESkillTree.Computation.Common;
using PoESkillTree.Utils.Extensions;

namespace PoESkillTree.Computation.Core.Graphs
{
    public class DefaultPruningRuleSet : IGraphPruningRuleSet
    {
        private readonly IReadOnlyDictionary<IStat, IStatGraph> _statGraphs;
        private readonly IDeterminesNodeRemoval _nodeRemovalDeterminer;

        public DefaultPruningRuleSet(
            IReadOnlyDictionary<IStat, IStatGraph> statGraphs, IDeterminesNodeRemoval nodeRemovalDeterminer)
            => (_statGraphs, _nodeRemovalDeterminer) = (statGraphs, nodeRemovalDeterminer);

        public IEnumerable<IStat> SelectStatsConsideredForRemoval(Modifier modifier)
            => modifier.Stats
                .Where(s => _statGraphs.ContainsKey(s))
                .Where(s => _statGraphs[s].ModifierCount == 0);

        public IEnumerable<IStat> SelectStatsNoLongerConsideredForRemoval(Modifier modifier)
            => modifier.Stats;

        public IEnumerable<NodeSelector> SelectRemovableNodesByNodeType(IReadOnlyStatGraph statGraph)
            => statGraph.Nodes.OrderBy(p => p.Key.NodeType).ToList()
                .Where(p => _nodeRemovalDeterminer.CanBeRemoved(p.Value))
                .Select(p => p.Key);

        public IEnumerable<FormNodeSelector> SelectRemovableNodesByForm(IReadOnlyStatGraph statGraph)
            => statGraph.FormNodeCollections.ToList()
                .Where(p => _nodeRemovalDeterminer.CanBeRemoved(p.Value))
                .Select(p => p.Key);

        public IEnumerable<IStat> SelectRemovableStats(IEnumerable<IStat> stats)
            => stats.Where(s => CanStatGraphBeRemoved(_statGraphs[s]));

        private bool CanStatGraphBeRemoved(IReadOnlyStatGraph statGraph)
            => statGraph.Nodes.IsEmpty() && statGraph.FormNodeCollections.IsEmpty()
                                         && _nodeRemovalDeterminer.CanBeRemoved(statGraph.Paths);
    }
}