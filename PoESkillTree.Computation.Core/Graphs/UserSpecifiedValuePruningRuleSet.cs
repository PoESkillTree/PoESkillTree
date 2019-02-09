using System.Collections.Generic;
using System.Linq;
using PoESkillTree.Computation.Common;
using PoESkillTree.Utils.Extensions;

namespace PoESkillTree.Computation.Core.Graphs
{
    public class UserSpecifiedValuePruningRuleSet : IGraphPruningRuleSet
    {
        private readonly IGraphPruningRuleSet _defaultRuleSet;
        private readonly IDeterminesNodeRemoval _nodeRemovalDeterminer;

        public UserSpecifiedValuePruningRuleSet(
            IGraphPruningRuleSet defaultRuleSet, IDeterminesNodeRemoval nodeRemovalDeterminer)
            => (_defaultRuleSet, _nodeRemovalDeterminer) = (defaultRuleSet, nodeRemovalDeterminer);

        public bool CanStatBeConsideredForRemoval(IStat stat, IReadOnlyStatGraph statGraph)
            => stat.ExplicitRegistrationType is ExplicitRegistrationType.UserSpecifiedValue;

        public IEnumerable<NodeSelector> SelectRemovableNodesByNodeType(IReadOnlyStatGraph statGraph)
            => _defaultRuleSet.SelectRemovableNodesByNodeType(statGraph);

        public IEnumerable<FormNodeSelector> SelectRemovableNodesByForm(IReadOnlyStatGraph statGraph)
            => _defaultRuleSet.SelectRemovableNodesByForm(statGraph)
                .Where(s => statGraph.FormNodeCollections[s].DefaultView.IsEmpty());

        public bool CanStatGraphBeRemoved(IReadOnlyStatGraph statGraph)
            => statGraph.Nodes.IsEmpty() &&
               _defaultRuleSet.SelectRemovableNodesByForm(statGraph).Count() == statGraph.FormNodeCollections.Count &&
               _nodeRemovalDeterminer.CanBeRemoved(statGraph.Paths);
    }
}