using System;
using PoESkillTree.Computation.Common;

namespace PoESkillTree.Computation.Core
{
    public class OverwritableNode : ICalculationNode
    {
        private readonly ICalculationNode _default;
        private readonly ICalculationNode _override;

        public OverwritableNode(ICalculationNode @default, ICalculationNode @override)
        {
            _default = @default;
            _override = @override;
        }

        public NodeValue? Value
        {
            get
            {
                Unsubscribe();
                var overrideValue = _override.Value;
                var defaultValue = _default.Value;
                SubscribeWhereRequired(overrideValue);
                return overrideValue ?? defaultValue;
            }
        }

        public event EventHandler ValueChanged;

        public void Dispose()
        {
            Unsubscribe();
        }

        private void SubscribeWhereRequired(NodeValue? overrideValue)
        {
            _override.ValueChanged += OnValueChanged;
            if (overrideValue == null)
            {
                _default.ValueChanged += OnValueChanged;
            }
        }

        private void Unsubscribe()
        {
            _default.ValueChanged -= OnValueChanged;
            _override.ValueChanged -= OnValueChanged;
        }

        private void OnValueChanged(object sender, EventArgs args)
        {
            ValueChanged?.Invoke(this, EventArgs.Empty);
        }
    }
}