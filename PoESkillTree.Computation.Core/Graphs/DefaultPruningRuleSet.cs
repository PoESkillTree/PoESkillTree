using System.Collections.Generic;
using System.Linq;
using EnumsNET;
using PoESkillTree.Computation.Common;
using PoESkillTree.Computation.Core.Events;
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
        {
            var nodeTypeCount = Enums.GetMemberCount<NodeType>();
            var nodes =
                new List<KeyValuePair<NodeSelector, IBufferingEventViewProvider<ICalculationNode>>>[nodeTypeCount];
            for (int i = 0; i < nodeTypeCount; i++)
            {
                nodes[i] = new List<KeyValuePair<NodeSelector, IBufferingEventViewProvider<ICalculationNode>>>();
            }
            foreach (KeyValuePair<NodeSelector, IBufferingEventViewProvider<ICalculationNode>> node in statGraph.Nodes)
            {
                nodes[(int) node.Key.NodeType].Add(node);
            }
            return nodes.Flatten()
                .Where(p => _nodeRemovalDeterminer.CanBeRemoved(p.Value))
                .Select(p => p.Key);
        }

        public IEnumerable<FormNodeSelector> SelectRemovableNodesByForm(IReadOnlyStatGraph statGraph)
            => statGraph.FormNodeCollections.ToList()
                .Where(p => _nodeRemovalDeterminer.CanBeRemoved(p.Value))
                .Select(p => p.Key);

        public bool CanStatGraphBeRemoved(IReadOnlyStatGraph statGraph)
            => statGraph.Nodes.IsEmpty() && statGraph.FormNodeCollections.IsEmpty()
                                         && _nodeRemovalDeterminer.CanBeRemoved(statGraph.Paths);
    }
}