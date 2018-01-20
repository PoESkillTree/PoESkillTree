using System;

namespace PoESkillTree.Computation.Core
{
    public class TotalNode : ICalculationNode
    {
        private readonly ICalculationNode _subtotal;
        private readonly ICalculationNode _totalOverride;

        public TotalNode(ICalculationNode subtotal, ICalculationNode totalOverride)
        {
            _subtotal = subtotal;
            _totalOverride = totalOverride;
        }

        public NodeValue? Value
        {
            get
            {
                Unsubscribe();
                var totalOverrideValue = _totalOverride.Value;
                var subtotalValue = _subtotal.Value;
                SubscribeWhereRequired(totalOverrideValue);
                return totalOverrideValue ?? subtotalValue;
            }
        }

        public event EventHandler ValueChanged;

        public void Dispose()
        {
            Unsubscribe();
        }

        private void SubscribeWhereRequired(NodeValue? totalOverrideValue)
        {
            _totalOverride.ValueChanged += OnValueChanged;
            if (totalOverrideValue == null)
            {
                _subtotal.ValueChanged += OnValueChanged;
            }
        }

        private void Unsubscribe()
        {
            _subtotal.ValueChanged -= OnValueChanged;
            _totalOverride.ValueChanged -= OnValueChanged;
        }

        private void OnValueChanged(object sender, EventArgs args)
        {
            ValueChanged?.Invoke(this, EventArgs.Empty);
        }
    }
}