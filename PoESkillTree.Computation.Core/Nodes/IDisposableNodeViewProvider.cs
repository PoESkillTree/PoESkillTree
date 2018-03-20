using System;
using PoESkillTree.Computation.Core.Events;

namespace PoESkillTree.Computation.Core.Nodes
{
    public interface IDisposableNodeViewProvider : ISuspendableEventViewProvider<ICalculationNode>, IDisposable
    {
        event EventHandler Disposed;

        void RaiseValueChanged();
    }
}