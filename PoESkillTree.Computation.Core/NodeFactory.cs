using System;
using PoESkillTree.Computation.Common;
using PoESkillTree.Computation.Core.Events;
using PoESkillTree.Computation.Core.Graphs;
using PoESkillTree.Computation.Core.Nodes;

namespace PoESkillTree.Computation.Core
{
    /// <summary>
    /// Implementation of <see cref="INodeFactory"/> using the node implementations of this project.
    /// <para>
    /// For each <see cref="IValue"/> (conceptual node) in the calculation graph, a stack of <see cref="ValueNode"/>,
    /// <see cref="CachingNode"/> and <see cref="CachingNodeAdapter"/> is created.
    /// </para>
    /// </summary>
    public class NodeFactory : INodeFactory
    {
        public INodeRepository NodeRepository { private get; set; }

        public IDisposableNodeViewProvider Create(IValue value, PathDefinition path)
        {
            var coreNode = new ValueNode(new ValueCalculationContext(NodeRepository, path), value);
            var cachingNode = new CachingNode(coreNode, new CycleGuard());
            var cachingNodeAdapter = new CachingNodeAdapter(cachingNode);
            return new DisposableNodeViewProvider(cachingNodeAdapter, cachingNode, coreNode);
        }


        private class DisposableNodeViewProvider : IDisposableNodeViewProvider
        {
            private readonly CachingNodeAdapter _defaultView;
            private readonly CachingNode _suspendableView;
            private readonly ValueNode _valueNode;

            public DisposableNodeViewProvider(
                CachingNodeAdapter defaultView, CachingNode suspendableView, ValueNode valueNode)
            {
                _defaultView = defaultView;
                _suspendableView = suspendableView;
                _valueNode = valueNode;
            }

            public int SubscriberCount => _defaultView.SubscriberCount + _suspendableView.SubscriberCount;
            public ICalculationNode DefaultView => _defaultView;
            public ICalculationNode SuspendableView => _suspendableView;
            public ISuspendableEvents Suspender => _suspendableView;

            public void Dispose()
            {
                _defaultView.Dispose();
                _suspendableView.Dispose();
                _valueNode.Dispose();
                Disposed?.Invoke(this, EventArgs.Empty);
            }

            public event EventHandler Disposed;

            public void RaiseValueChanged() => _valueNode.OnValueChanged();
        }
    }
}