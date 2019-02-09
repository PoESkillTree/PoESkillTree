using System.Collections.Generic;
using System.Linq;
using PoESkillTree.Computation.Common;
using PoESkillTree.Utils.Extensions;

namespace PoESkillTree.Computation.Core.Graphs
{
    public class DefaultPruningRuleSet : IGraphPruningRuleSet
    {
        private readonly IDeterminesNodeRemoval _nodeRemovalDeterminer;

        public DefaultPruningRuleSet(IDeterminesNodeRemoval nodeRemovalDeterminer)
            =>  _nodeRemovalDeterminer = nodeRemovalDeterminer;

        public bool CanStatBeConsideredForRemoval(IStat stat, IReadOnlyStatGraph statGraph)
            => statGraph.ModifierCount == 0;

        public IEnumerable<NodeSelector> SelectRemovableNodesByNodeType(IReadOnlyStatGraph statGraph)
            => statGraph.Nodes.OrderBy(p => p.Key.NodeType).ToList()
                .Where(p => _nodeRemovalDeterminer.CanBeRemoved(p.Value))
                .Select(p => p.Key);

        public IEnumerable<FormNodeSelector> SelectRemovableNodesByForm(IReadOnlyStatGraph statGraph)
            => statGraph.FormNodeCollections.ToList()
                .Where(p => _nodeRemovalDeterminer.CanBeRemoved(p.Value))
                .Select(p => p.Key);

        public bool CanStatGraphBeRemoved(IReadOnlyStatGraph statGraph)
            => statGraph.Nodes.IsEmpty() && statGraph.FormNodeCollections.IsEmpty()
                                         && _nodeRemovalDeterminer.CanBeRemoved(statGraph.Paths);
    }
}