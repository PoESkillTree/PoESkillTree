using System;

namespace PoESkillTree.Computation.Core
{
    public interface ICachingNode : IDisposableNode, ISuspendableEvents
    {
        // ValueChangeReceived is exempt from ISuspendableEvents.
        // This allows caching nodes to still propagate events through the graph.
        event EventHandler ValueChangeReceived;
    }
}