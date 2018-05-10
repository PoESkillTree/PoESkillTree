using PoESkillTree.Computation.Common;

namespace PoESkillTree.Computation.Core
{
    public interface INodeRepository
    {
        // Asking for non-existing nodes leads to their creation.

        // Returns a collection of the paths of the given stat.
        IObservableCollection<PathDefinition> GetPaths(IStat stat);

        // Stat selects the stat subgraph, nodeType and path the node in it.
        // TODO Non-main path with Total, Subtotal, UncappedSubtotal or TotalOverride throws.
        ICalculationNode GetNode(IStat stat, NodeType nodeType, PathDefinition path);

        // Returns the form node collection of stat on the given path.
        // TODO Non-main path with TotalOverride throws.
        INodeCollection<Modifier> GetFormNodeCollection(IStat stat, Form form, PathDefinition path);
    }


    public static class NodeRepositoryExtensions
    {
        // TODO Check everything using these methods to make sure they actually want the main path

        public static ICalculationNode GetNode(
            this INodeRepository repo, IStat stat, NodeType nodeType = NodeType.Total) =>
            repo.GetNode(stat, nodeType, PathDefinition.MainPath);

        public static INodeCollection<Modifier> GetFormNodeCollection(
            this INodeRepository repo, IStat stat, Form form) =>
            repo.GetFormNodeCollection(stat, form, PathDefinition.MainPath);
    }
}