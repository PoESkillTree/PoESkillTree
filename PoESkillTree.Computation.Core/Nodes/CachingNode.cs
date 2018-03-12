using System;
using PoESkillTree.Computation.Common;

namespace PoESkillTree.Computation.Core.Nodes
{
    public class CachingNode : SubscriberCountingNode, IDisposable, ICachingNode
    {
        private readonly ICalculationNode _decoratedNode;
        private readonly ICycleGuard _cycleGuard;

        private bool _calculatedValue;
        private NodeValue? _value;
        private bool _suspendEvents;
        private bool _suppressedValueChanged;

        public CachingNode(ICalculationNode decoratedNode, ICycleGuard cycleGuard)
        {
            _decoratedNode = decoratedNode;
            _cycleGuard = cycleGuard;
            _decoratedNode.ValueChanged += DecoratedNodeOnValueChanged;
        }

        public override NodeValue? Value
        {
            get
            {
                if (!_calculatedValue)
                {
                    CalculateValue();
                }

                return _value;
            }
        }

        private void CalculateValue()
        {
            using (_cycleGuard.Guard())
            {
                _value = _decoratedNode.Value;
            }
            _calculatedValue = true;
        }

        public event EventHandler ValueChangeReceived;

        public void Dispose()
        {
            _decoratedNode.ValueChanged -= DecoratedNodeOnValueChanged;
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