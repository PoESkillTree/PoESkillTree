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
        //ICalculationNode GetNode(IStat stat, NodeType nodeType, PathProperty path);

        // Returns a collection (with change events) of the paths of the given stat.
        //ISomethingCollection<PathProperty> GetPaths(IStat stat);

        // Returns the form node collection of stat on the main path
        INodeCollection<Modifier> GetFormNodeCollection(IStat stat, Form form);
        // Like above but path-specific. Makes above obsolete.
        //INodeCollection<Modifier> GetFormNodeCollection(IStat stat, Form form, PathProperty path);

        // PathProperty: Contains the path's definition
        // - Its IModifierSource (only with the information that is the same for all modifiers of the path)
        // - The IStats on its conversion path (empty if unconverted)
        // - Main path: IModifierSource is Global, the conversion path is empty
    }
}