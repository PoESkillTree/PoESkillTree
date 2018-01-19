using System;

namespace PoESkillTree.Computation.Core
{
    public class CachingNode : ICachingNode
    {
        private readonly ICalculationNode _decoratedNode;

        private bool _calculatedValue;
        private double? _value;
        private double? _minValue;
        private double? _maxValue;
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
                CalculateValuesIfNecessary();
                return _value;
            }
        }

        public double? MinValue
        {
            get
            {
                CalculateValuesIfNecessary();
                return _minValue;
            }
        }

        public double? MaxValue
        {
            get
            {
                CalculateValuesIfNecessary();
                return _maxValue;
            }
        }

        private void CalculateValuesIfNecessary()
        {
            if (!_calculatedValue)
            {
                _value = _decoratedNode.Value;
                _minValue = _decoratedNode.MinValue;
                _maxValue = _decoratedNode.MaxValue;
                _calculatedValue = true;
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