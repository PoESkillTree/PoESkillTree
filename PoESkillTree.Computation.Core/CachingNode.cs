using System;
using PoESkillTree.Computation.Common;

namespace PoESkillTree.Computation.Core
{
    public class CachingNode : SubscriberCountingNode, ICachingNode
    {
        private readonly IDisposableNode _decoratedNode;

        private bool _calculatedValue;
        private NodeValue? _value;
        private bool _suspendEvents;
        private bool _suppressedValueChanged;

        public CachingNode(IDisposableNode decoratedNode)
        {
            _decoratedNode = decoratedNode;
            _decoratedNode.ValueChanged += DecoratedNodeOnValueChanged;
        }

        public override NodeValue? Value
        {
            get
            {
                if (!_calculatedValue)
                {
                    _value = _decoratedNode.Value;
                    _calculatedValue = true;
                }

                return _value;
            }
        }

        public event EventHandler ValueChangeReceived;

        public override void Dispose()
        {
            _decoratedNode.ValueChanged -= DecoratedNodeOnValueChanged;
            _decoratedNode.Dispose();
        }

        public void SuspendEvents()
        {
            _suspendEvents = true;
        }

        public void ResumeEvents()
        {
            _suspendEvents = false;
            if (_suppressedValueChanged)
            {
                _suppressedValueChanged = false;
                OnValueChanged();
            }
        }

        private void DecoratedNodeOnValueChanged(object sender, EventArgs args)
        {
            if (!_calculatedValue)
            {
                return;
            }

            _calculatedValue = false;
            ValueChangeReceived?.Invoke(this, EventArgs.Empty);
            OnValueChanged();
        }

        protected override void OnValueChanged()
        {
            if (_suspendEvents)
            {
                _suppressedValueChanged = true;
            }
            else
            {
                base.OnValueChanged();
            }
        }
    }
}