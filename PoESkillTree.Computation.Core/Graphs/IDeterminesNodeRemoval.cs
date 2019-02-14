using PoESkillTree.Computation.Core.Events;

namespace PoESkillTree.Computation.Core.Graphs
{
    /// <summary>
    /// Determines whether nodes can be removed by <see cref="ICalculationGraphPruner"/>.
    /// </summary>
    public interface IDeterminesNodeRemoval
    {
        /// <summary>
        /// Returns true if the given node can be removed from the calculation graph.
        /// </summary>
        bool CanBeRemoved(IBufferingEventViewProvider<ICalculationNode> node);

        /// <summary>
        /// Returns true if the given node (that is not a <see cref="IBufferingEventViewProvider{T}"/>) can be
        /// removed from the calculation graph.
        /// </summary>
        bool CanBeRemoved(ICountsSubsribers node);
    }
}