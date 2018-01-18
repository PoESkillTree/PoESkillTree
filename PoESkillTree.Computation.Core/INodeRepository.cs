using PoESkillTree.Computation.Common;

namespace PoESkillTree.Computation.Core
{
    public interface INodeRepository
    {
        // Asking for non-existing nodes leads to their creation

        // NodeType: {Total, Subtotal, Uncapped Subtotal, Base} or a Form

        // stat selects the stat subgraph and the NodeType in it.
        // With conversions and/or sources:
        // - Increase, More: the node on the unconverted, Global path.
        // - Base, Base Override, Base Set, Base Add: the unconverted base node.
        // - Uncapped Subtotal: The node that sums all paths.
        // - Subtotal, Total Override, Total: There should only be one.
        ICalculationNode GetNodeForStat(IStat stat);

        // stat selects the stat subgraph and the NodeType in it.
        // Only one NodeType from Total, Subtotal and Uncapped Subtotal make sense, probably Uncapped Subtotal as
        // that's where these path subgraphs end up. BaseOverride, BaseSet, BaseAdd and TotalOverride don't make sense.
        // Returns all nodes by conversion path and source.
        IPathNodeCollection GetPathNodesForStat(IStat stat);

        // Returns the form node collection of stat
        IFormNodeCollection GetFormNodesForStat(IStat stat, Form form);
    }
}