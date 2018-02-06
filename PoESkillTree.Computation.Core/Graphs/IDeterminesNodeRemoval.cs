using PoESkillTree.Computation.Core.Events;

namespace PoESkillTree.Computation.Core.Graphs
{
    public interface IDeterminesNodeRemoval
    {
        bool CanBeRemoved(ISuspendableEventViewProvider<ICalculationNode> node);

        bool CanBeRemoved(ICountsSubsribers node);
    }
}