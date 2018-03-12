using System;
using MoreLinq;
using PoESkillTree.Computation.Common;
using PoESkillTree.Computation.Core.Events;
using PoESkillTree.Computation.Core.Graphs;
using PoESkillTree.Computation.Core.Nodes;

namespace PoESkillTree.Computation.Core
{
    public class NodeFactory : INodeFactory
    {
        public INodeRepository NodeRepository { private get; set; }

        public IDisposableNodeViewProvider Create(IValue value)
        {
            var coreNode = new ValueNode(new ValueCalculationContext(NodeRepository), value);
            var cachingNode = new CachingNode(coreNode, new CycleGuard());
            var cachingNodeAdapter = new CachingNodeAdapter(cachingNode);
            return new DisposableNodeViewProvider(cachingNodeAdapter, cachingNode, coreNode);
        }


        private class DisposableNodeViewProvider : IDisposableNodeViewProvider
        {
            private readonly CachingNodeAdapter _defaultView;
            private readonly CachingNode _suspendableView;
            private readonly IDisposable[] _disposables;

            public DisposableNodeViewProvider(
                CachingNodeAdapter defaultView, CachingNode suspendableView, params IDisposable[] disposables)
            {
                _defaultView = defaultView;
                _suspendableView = suspendableView;
                _disposables = disposables;
            }

            public int SubscriberCount => _defaultView.SubscriberCount + _suspendableView.SubscriberCount;
            public ICalculationNode DefaultView => _defaultView;
            public ICalculationNode SuspendableView => _suspendableView;
            public ISuspendableEvents Suspender => _suspendableView;

            public void Dispose()
            {
                _defaultView.Dispose();
                _suspendableView.Dispose();
                _disposables.ForEach(d => d.Dispose());
                Disposed?.Invoke(this, EventArgs.Empty);
            }

            public event EventHandler Disposed;
        }
    }
}