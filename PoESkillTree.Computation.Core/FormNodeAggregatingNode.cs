using System;
using System.Collections.Generic;
using System.Linq;

namespace PoESkillTree.Computation.Core
{
    public delegate NodeValue? NodeValueAggregator(IEnumerable<NodeValue?> values);

    public class FormNodeAggregatingNode : ICalculationNode
    {
        private readonly IFormNodeCollection _formNodes;
        private readonly NodeValueAggregator _aggregator;
        private List<ICalculationNode> _subscribedNodes;

        public FormNodeAggregatingNode(IFormNodeCollection formNodes, NodeValueAggregator aggregator)
        {
            _formNodes = formNodes;
            _aggregator = aggregator;
        }

        public NodeValue? Value
        {
            get
            {
                SubscribeIfRequired();
                return _aggregator(_formNodes.Items.Select(n => n.Node.Value));
            }
        }

        public event EventHandler ValueChanged;

        public void Dispose()
        {
            _formNodes.ItemsChanged -= FormNodesOnItemsChanged;
            UnsubscribeFromFormNodes();
        }

        private void FormNodesOnItemsChanged(object sender, EventArgs args)
        {
            UnsubscribeFromFormNodes();
            SubscribeToFormNodes();
            OnValueChanged(sender, args);
        }

        private void SubscribeIfRequired()
        {
            if (_subscribedNodes == null)
            {
                _formNodes.ItemsChanged += FormNodesOnItemsChanged;
                SubscribeToFormNodes();
            }
        }

        private void UnsubscribeFromFormNodes()
        {
            foreach (var node in _subscribedNodes)
            {
                node.ValueChanged -= OnValueChanged;
            }
            _subscribedNodes = new List<ICalculationNode>();
        }

        private void SubscribeToFormNodes()
        {
            _subscribedNodes = new List<ICalculationNode>();
            foreach (var item in _formNodes.Items)
            {
                item.Node.ValueChanged += OnValueChanged;
                _subscribedNodes.Add(item.Node);
            }
        }

        private void OnValueChanged(object sender, EventArgs args)
        {
            ValueChanged?.Invoke(this, EventArgs.Empty);
        }
    }
}