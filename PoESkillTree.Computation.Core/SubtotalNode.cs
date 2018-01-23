using System;

namespace PoESkillTree.Computation.Core
{
    public class SubtotalNode : ICalculationNode
    {
        private readonly ICalculationNode _uncappedSubtotal;
        private readonly ICalculationNode _minimum;
        private readonly ICalculationNode _maximum;

        private bool _subscribed;

        public SubtotalNode(ICalculationNode uncappedSubtotal, ICalculationNode minimum, ICalculationNode maximum)
        {
            _uncappedSubtotal = uncappedSubtotal;
            _minimum = minimum;
            _maximum = maximum;
        }

        public NodeValue? Value
        {
            get
            {
                SubscribeIfNotYetDone();

                if (_uncappedSubtotal.Value is NodeValue v && v.AlmostEquals(0))
                {
                    return new NodeValue(0);
                }

                return Max(Min(_uncappedSubtotal.Value, _maximum.Value), _minimum.Value);
            }
        }

        private void SubscribeIfNotYetDone()
        {
            if (!_subscribed)
            {
                Subscribe();
                _subscribed = true;
            }
        }

        private void Subscribe()
        {
            _uncappedSubtotal.ValueChanged += OnValueChanged;
            _minimum.ValueChanged += OnValueChanged;
            _maximum.ValueChanged += OnValueChanged;
        }

        private static NodeValue? Min(NodeValue? left, NodeValue? right) => 
            Combine(left, right, Math.Min);

        private static NodeValue? Max(NodeValue? left, NodeValue? right) => 
            Combine(left, right, Math.Max);

        private static NodeValue? Combine(NodeValue? left, NodeValue? right, Func<double, double, double> operation)
        {           
            if (!left.HasValue)
            {
                return right;
            }

            if (!right.HasValue)
            {
                return left;
            }

            return NodeValue.Combine(left.Value, right.Value, operation);
        }

        public event EventHandler ValueChanged;

        public void Dispose()
        {
            _uncappedSubtotal.ValueChanged -= OnValueChanged;
            _minimum.ValueChanged -= OnValueChanged;
            _maximum.ValueChanged -= OnValueChanged;
        }

        private void OnValueChanged(object o, EventArgs eventArgs)
        {
            ValueChanged?.Invoke(this, EventArgs.Empty);
        }
    }
}