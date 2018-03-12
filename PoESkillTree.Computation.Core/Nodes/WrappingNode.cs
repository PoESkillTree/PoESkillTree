using System;
using PoESkillTree.Computation.Common;

namespace PoESkillTree.Computation.Core.Nodes
{
    // Wrapped nodes will have a fixed number of subscribers from clients of this node (exactly 1 -- this node),
    // instead of the unpredictable number that subscribes to this node.
    // This is necessary for nodes that are subscribed to when being exposed, independently of whether they would be
    // used without being exposed explicitly (through ICalculator.ExplicitlyRegisteredStats).
    public class WrappingNode : ICalculationNode, IDisposable
    {
        private readonly ICalculationNode _decoratedNode;

        public WrappingNode(ICalculationNode decoratedNode)
        {
            _decoratedNode = decoratedNode;
            _decoratedNode.ValueChanged += OnValueChanged;
        }

        public NodeValue? Value => _decoratedNode.Value;

        public event EventHandler ValueChanged;

        public void Dispose() => _decoratedNode.ValueChanged -= OnValueChanged;

        private void OnValueChanged(object sender, EventArgs args) => ValueChanged?.Invoke(this, EventArgs.Empty);
    }
}