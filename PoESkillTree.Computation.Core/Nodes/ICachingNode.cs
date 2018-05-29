using System;
using PoESkillTree.Computation.Core.Events;

namespace PoESkillTree.Computation.Core.Nodes
{
    /// <summary>
    /// <see cref="ICalculationNode"/> that caches <see cref="ICalculationNode.Value"/> of a decorated node and
    /// implements <see cref="ISuspendableEvents"/> to suspend its <see cref="ICalculationNode.ValueChanged"/> event.
    /// The cache gets invalidated when the decorated node raises <see cref="ICalculationNode.ValueChanged"/>.
    /// </summary>
    public interface ICachingNode : ICalculationNode, ISuspendableEvents
    {
        /// <summary>
        /// Event that is raised when the decorated node raises <see cref="ICalculationNode.ValueChanged"/>.
        /// As opposed to this node'S <see cref="ICalculationNode.ValueChanged"/>,
        /// <see cref="ISuspendableEvents.SuspendEvents"/> does not suspend this event.
        /// </summary>
        /// <remarks>
        /// This event and its exemption from <see cref="ISuspendableEvents.SuspendEvents"/> is required because
        /// while nodes of this interface are surfaced to the user of <see cref="ICalculator"/> (requiring the event
        /// to be suspendable), they are still part of the calculation graph itself, meaning they do not to
        /// propagate change events through the graph. <see cref="CachingNodeAdapter"/> decorates
        /// <see cref="ICachingNode"/>s to allow their integration in the graph.
        /// </remarks>
        event EventHandler ValueChangeReceived;
    }
}