using System;

namespace PoESkillTree.Computation.Core
{
    public class CachingNode : ICachingNode
    {
        private readonly ICalculationNode _decoratedNode;

        private bool _calculatedValue;
        private double? _value;
        private bool _propagatedValueChange;

        public CachingNode(ICalculationNode decoratedNode)
        {
            _decoratedNode = decoratedNode;
            _decoratedNode.ValueChanged += DecoratedNodeOnValueChanged;
        }

        public double? Value
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

        public void RaiseValueChanged()
        {
            if (!_propagatedValueChange)
            {
                _propagatedValueChange = true;
                ValueChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        public event EventHandler ValueChanged;

        public event EventHandler ValueChangeReceived;

        public void Dispose()
        {
            _decoratedNode.ValueChanged -= DecoratedNodeOnValueChanged;
        }

        private void DecoratedNodeOnValueChanged(object sender, EventArgs args)
        {
            var raiseValueChangeReceived = _calculatedValue || _propagatedValueChange;
            _calculatedValue = false;
            _propagatedValueChange = false;
            if (raiseValueChangeReceived)
            {
                ValueChangeReceived?.Invoke(this, EventArgs.Empty);
            }
        }
    }
}