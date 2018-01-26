using System;
using PoESkillTree.Computation.Common;

namespace PoESkillTree.Computation.Core
{
    public class CachingNode : ICachingNode
    {
        private readonly ICalculationNode _decoratedNode;

        private bool _calculatedValue;
        private NodeValue? _value;
        private bool _suspendEvents;
        private bool _suppressedValueChanged;

        public CachingNode(ICalculationNode decoratedNode)
        {
            _decoratedNode = decoratedNode;
            _decoratedNode.ValueChanged += DecoratedNodeOnValueChanged;
        }

        public NodeValue? Value
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

        public event EventHandler ValueChanged;

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

        private void OnValueChanged()
        {
            if (_suspendEvents)
            {
                _suppressedValueChanged = true;
            }
            else
            {
                ValueChanged?.Invoke(this, EventArgs.Empty);
            }
        }
    }
}