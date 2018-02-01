using System;
using PoESkillTree.Computation.Core.Events;

namespace PoESkillTree.Computation.Core.Nodes
{
    public interface ICachingNode : IDisposableNode, ISuspendableEvents
    {
        // ValueChangeReceived is exempt from ISuspendableEvents.
        // This allows caching nodes to still propagate events through the graph.
        event EventHandler ValueChangeReceived;
    }
}