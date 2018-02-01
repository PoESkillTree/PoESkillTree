using System;
using System.Collections.Generic;
using System.Linq;
using PoESkillTree.Computation.Common;

namespace PoESkillTree.Computation.Core.Nodes
{
    public delegate NodeValue? NodeValueAggregator(IEnumerable<NodeValue?> values);

    public class AggregatingNode : IDisposableNode
    {
        private readonly INodeCollection _nodes;
        private readonly NodeValueAggregator _aggregator;
        private ICollection<ICalculationNode> _subscribedNodes;

        public AggregatingNode(INodeCollection nodes, NodeValueAggregator aggregator)
        {
            _nodes = nodes;
            _aggregator = aggregator;
        }

        public NodeValue? Value
        {
            get
            {
                SubscribeIfRequired();
                return _aggregator(_nodes.Select(n => n.Value));
            }
        }

        private void SubscribeIfRequired()
        {
            if (_subscribedNodes == null)
            {
                _nodes.CollectionChanged += NodesOnCollectionChanged;
                _subscribedNodes = new HashSet<ICalculationNode>();
                SubscribeToNodes();
            }
        }

        public event EventHandler ValueChanged;

        public void Dispose()
        {
            if (_subscribedNodes != null)
            {
                _nodes.CollectionChanged -= NodesOnCollectionChanged;
                UnsubscribeFromNodes();
            }
        }

        private void NodesOnCollectionChanged(object sender, NodeCollectionChangeEventArgs args)
        {
            switch (args.Action)
            {
                case NodeCollectionChangeAction.Add:
                    SubscribeTo(args.Element);
                    break;
                case NodeCollectionChangeAction.Remove:
                    UnsubscribeFrom(args.Element);
                    break;
                default:
                    ResubscribeToNodes();
                    break;
            }
            OnValueChanged(sender, args);
        }

        private void ResubscribeToNodes()
        {
            UnsubscribeFromNodes();
            SubscribeToNodes();
        }

        private void UnsubscribeFromNodes()
        {
            foreach (var node in _subscribedNodes.ToList())
            {
                UnsubscribeFrom(node);
            }
        }

        private void UnsubscribeFrom(ICalculationNode node)
        {
            node.ValueChanged -= OnValueChanged;
            _subscribedNodes.Remove(node);
        }

        private void SubscribeToNodes()
        {
            foreach (var node in _nodes)
            {
                SubscribeTo(node);
            }
        }

        private void SubscribeTo(ICalculationNode node)
        {
            node.ValueChanged += OnValueChanged;
            _subscribedNodes.Add(node);
        }

        private void OnValueChanged(object sender, EventArgs args)
        {
            ValueChanged?.Invoke(this, EventArgs.Empty);
        }
    }
}