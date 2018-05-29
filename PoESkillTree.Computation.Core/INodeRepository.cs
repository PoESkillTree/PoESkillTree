using PoESkillTree.Computation.Common;

namespace PoESkillTree.Computation.Core
{
    /// <summary>
    /// Repository of nodes in the calculation graph. Querying non-existing nodes leads to their creation.
    /// </summary>
    public interface INodeRepository
    {
        /// <summary>
        /// Returns an observable collection of all paths of the given stat.
        /// </summary>
        IObservableCollection<PathDefinition> GetPaths(IStat stat);

        /// <summary>
        /// Returns the node of type <paramref name="nodeType"/> in the path <paramref name="path"/> of
        /// <paramref name="stat"/>'s calculation subgraph.
        /// </summary>
        /// <remarks>
        /// <see cref="NodeType.Total"/>, <see cref="NodeType.Subtotal"/>, <see cref="NodeType.UncappedSubtotal"/>
        /// and <see cref="NodeType.TotalOverride"/> may only be used as <paramref name="nodeType"/> if
        /// <see cref="PathDefinition.IsMainPath"/> for <paramref name="path"/>.
        /// </remarks>
        ICalculationNode GetNode(IStat stat, NodeType nodeType, PathDefinition path);

        /// <summary>
        /// Returns the form node collection of type <paramref name="form"/> in the path <paramref name="path"/> of
        /// <paramref name="stat"/>'s calculation subgraph.
        /// </summary>
        /// <remarks>
        /// <see cref="Form.TotalOverride"/> may only be used as <paramref name="form"/> if
        /// <see cref="PathDefinition.IsMainPath"/> for <paramref name="path"/>.
        /// </remarks>
        INodeCollection<Modifier> GetFormNodeCollection(IStat stat, Form form, PathDefinition path);
    }


    public static class NodeRepositoryExtensions
    {
        /// <summary>
        /// Returns the node of type <paramref name="nodeType"/> in the main path of
        /// <paramref name="stat"/>'s calculation subgraph.
        /// </summary>
        public static ICalculationNode GetNode(
            this INodeRepository repo, IStat stat, NodeType nodeType = NodeType.Total) =>
            repo.GetNode(stat, nodeType, PathDefinition.MainPath);

        /// <summary>
        /// Returns the form node collection of type <paramref name="form"/> in the main path of
        /// <paramref name="stat"/>'s calculation subgraph.
        /// </summary>
        public static INodeCollection<Modifier> GetFormNodeCollection(
            this INodeRepository repo, IStat stat, Form form) =>
            repo.GetFormNodeCollection(stat, form, PathDefinition.MainPath);
    }
}