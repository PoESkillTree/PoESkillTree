using System;

namespace PoESkillTree.Computation.Core
{
    public class CachingNode : ICachingNode
    {
        private readonly ICalculationNode _decoratedNode;

        private bool _cacheInvalid = true;
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
                if (_cacheInvalid)
                {
                    _value = _decoratedNode.Value;
                    _cacheInvalid = false;
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
            _cacheInvalid = true;
            if (_propagatedValueChange)
            {
                _propagatedValueChange = false;
                ValueChangeReceived?.Invoke(this, EventArgs.Empty);
            }
        }
    }
}