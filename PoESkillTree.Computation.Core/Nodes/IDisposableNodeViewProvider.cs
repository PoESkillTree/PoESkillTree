using System;
using PoESkillTree.Computation.Core.Events;

namespace PoESkillTree.Computation.Core.Nodes
{
    /// <summary>
    /// <see cref="IBufferingEventViewProvider{T}"/> for <see cref="ICalculationNode"/>s that allows disposing
    /// the nodes in <see cref="IBufferingEventViewProvider{T}"/> and manually raising
    /// <see cref="ICalculationNode.ValueChanged"/>.
    /// </summary>
    public interface IDisposableNodeViewProvider : IBufferingEventViewProvider<ICalculationNode>, IDisposable
    {
        /// <summary>
        /// Event that is raised after <see cref="IDisposable.Dispose"/> was called.
        /// </summary>
        event EventHandler Disposed;

        /// <summary>
        /// Manually triggers <see cref="ICalculationNode.ValueChanged"/> on contained nodes to invalidate their values.
        /// </summary>
        /// <remarks>
        /// Used in cases where the nodes themselves can't know that their values changed, e.g. when value calculations
        /// are changed from behaviors.
        /// </remarks>
        void RaiseValueChanged();
    }
}