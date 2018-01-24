using System;

namespace PoESkillTree.Computation.Core
{
    public interface ICachingNode : ICalculationNode, ISuspendableNotifications
    {
        // ValueChangeReceived is exempt from ISuspendableNotifications.
        // This allows caching nodes to still propagate events through the graph.
        event EventHandler ValueChangeReceived;
    }
}