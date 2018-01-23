using System;
using PoESkillTree.Computation.Common;

namespace PoESkillTree.Computation.Core
{
    public class CachingNodeAdapter : ICalculationNode
    {
        private readonly ICachingNode _adaptedNode;

        public CachingNodeAdapter(ICachingNode adaptedNode)
        {
            _adaptedNode = adaptedNode;
            _adaptedNode.ValueChangeReceived += AdaptedNodeOnValueChangeReceived;
        }

        public NodeValue? Value => _adaptedNode.Value;

        public event EventHandler ValueChanged;

        public void Dispose()
        {
            _adaptedNode.ValueChangeReceived -= AdaptedNodeOnValueChangeReceived;
        }

        private void AdaptedNodeOnValueChangeReceived(object sender, EventArgs args)
        {
            ValueChanged?.Invoke(this, EventArgs.Empty);
        }
    }
}