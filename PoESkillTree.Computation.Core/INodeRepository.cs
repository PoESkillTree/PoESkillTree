using PoESkillTree.Computation.Common;

namespace PoESkillTree.Computation.Core
{
    public interface INodeRepository
    {
        // Asking for non-existing nodes leads to their creation

        // stat selects the stat subgraph, nodeType the node in it.
        // With conversions and/or sources:
        // - Increase, More: the node on the unconverted, Global path.
        // - Base, BaseOverride, BaseSet, EffectiveBaseAdd, Base Add: the unconverted base node.
        // - UncappedSubtotal: The node that sums all paths.
        // - Subtotal, TotalOverride, Total: There should only be one.
        ICalculationNode GetNodeForStat(IStat stat, NodeType nodeType = NodeType.Total);

        // stat selects the stat subgraph, nodeType the node in it.
        // Only one NodeType from Total, Subtotal and UncappedSubtotal make sense, probably Uncapped Subtotal as
        // that's where these path subgraphs end up. BaseOverride, BaseSet, BaseAdd, EffectiveBaseAdd and
        // TotalOverride don't make sense.
        // Returns all nodes by conversion path and source.
        IPathNodeCollection GetPathNodesForStat(IStat stat, NodeType nodeType = NodeType.Total);

        // Returns the form node collection of stat
        IFormNodeCollection GetFormNodesForStat(IStat stat, Form form);
    }
}