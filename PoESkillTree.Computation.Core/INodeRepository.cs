using PoESkillTree.Computation.Common;

namespace PoESkillTree.Computation.Core
{
    public interface INodeRepository
    {
        // Asking for non-existing nodes leads to their creation

        // stat selects the stat subgraph, nodeType the node in it.
        // With conversions and/or sources:
        // - Increase, More, Base, BaseOverride, BaseSet, Base Add: the node on the main path. (calls overload below)
        // - UncappedSubtotal: The node that sums all paths.
        // - Subtotal, TotalOverride, Total: There should only be one.
        ICalculationNode GetNode(IStat stat, NodeType nodeType = NodeType.Total);
        // Like above but path-specific. Not usable with Total, Subtotal and TotalOverride.
        //ICalculationNode GetNode(IStat stat, NodeType nodeType, PathDefinition path);

        // Returns a collection (with change events) of the paths of the given stat.
        //IObservableCollection<PathDefinition> GetPaths(IStat stat);

        // Returns the form node collection of stat on the main path
        INodeCollection<Modifier> GetFormNodeCollection(IStat stat, Form form);
        // Like above but path-specific. Makes above obsolete.
        //INodeCollection<Modifier> GetFormNodeCollection(IStat stat, Form form, PathDefinition path);
    }
}