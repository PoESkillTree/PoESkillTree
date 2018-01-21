using System;
using System.Collections.Generic;
using System.Linq;

namespace PoESkillTree.Computation.Core
{
    // For both BaseOverride and TotalOverride
    public class OverrideNode : ICalculationNode
    {
        private readonly IFormNodeCollection _formNodes;
        private List<ICalculationNode> _subscribedNodes;

        public OverrideNode(IFormNodeCollection formNodes)
        {
            _formNodes = formNodes;
        }

        public NodeValue? Value
        {
            get
            {
                if (_subscribedNodes == null)
                {
                    _formNodes.ItemsChanged += FormNodesOnItemsChanged;
                    SubscribeToFormNodes();
                }
                switch (_formNodes.Items.Count)
                {
                    case 0:
                        return null;
                    case 1:
                        return _formNodes.Items[0].Node.Value;
                    default:
                        return CalculateValueFromManyItems();
                }
            }
        }

        private NodeValue? CalculateValueFromManyItems() =>
            IsAnyItemZero()
                ? new NodeValue(0)
                : throw new NotSupportedException();

        private bool IsAnyItemZero() =>
            _formNodes.Items
                .Select(i => i.Node.Value)
                .Any(v => v.HasValue && v.Value.AlmostEquals(0, 1e-10));

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