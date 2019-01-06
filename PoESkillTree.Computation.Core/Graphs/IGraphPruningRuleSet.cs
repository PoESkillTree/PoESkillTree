using System.Collections.Generic;
using PoESkillTree.Computation.Common;

namespace PoESkillTree.Computation.Core.Graphs
{
    public interface IGraphPruningRuleSet
    {
        IEnumerable<IStat> SelectStatsConsideredForRemoval(Modifier modifier);

        IEnumerable<IStat> SelectStatsNoLongerConsideredForRemoval(Modifier modifier);

        IEnumerable<NodeSelector> SelectRemovableNodesByNodeType(IReadOnlyStatGraph statGraph);

        IEnumerable<FormNodeSelector> SelectRemovableNodesByForm(IReadOnlyStatGraph statGraph);

        IEnumerable<IStat> SelectRemovableStats(IEnumerable<IStat> stats);
    }
}