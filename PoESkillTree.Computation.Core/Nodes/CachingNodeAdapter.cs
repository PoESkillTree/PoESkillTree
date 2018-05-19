using System;
using PoESkillTree.Computation.Common;

namespace PoESkillTree.Computation.Core.Nodes
{
    /// <summary>
    /// Adapts an <see cref="ICachingNode"/> for usage in the calculation graph itself.
    /// <para>
    /// That means subscribing to its <see cref="ICachingNode.ValueChangeReceived"/> event instead of
    /// <see cref="ICalculationNode.ValueChanged"/> for raising this node's value changed event.
    /// </para>
    /// </summary>
    public class CachingNodeAdapter : SubscriberCountingNode, IDisposable
    {
        private readonly ICachingNode _adaptedNode;

        public CachingNodeAdapter(ICachingNode adaptedNode)
        {
            _adaptedNode = adaptedNode;
            _adaptedNode.ValueChangeReceived += AdaptedNodeOnValueChangeReceived;
        }

        public override NodeValue? Value => _adaptedNode.Value;

        public void Dispose()
        {
            _adaptedNode.ValueChangeReceived -= AdaptedNodeOnValueChangeReceived;
        }

        private void AdaptedNodeOnValueChangeReceived(object sender, EventArgs args) => OnValueChanged();
    }
}