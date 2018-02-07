using System;

namespace PoESkillTree.Computation.Core.Nodes
{
    public interface ICycleGuard
    {
        IDisposable Guard();
    }
}