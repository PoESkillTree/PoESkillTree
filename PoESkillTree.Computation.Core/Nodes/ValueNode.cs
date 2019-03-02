using System;
using System.Linq;
using PoESkillTree.Computation.Common;

namespace PoESkillTree.Computation.Core.Nodes
{
    /// <summary>
    /// The core implementation of <see cref="ICalculationNode"/> that uses an <see cref="IValue"/> to calculate
    /// its value. Saves the nodes and collections used in that calculation to subscribe to them after the calculation.
    /// </summary>
    public class ValueNode : ICalculationNode, IDisposable
    {
        private readonly IValue _value;
        private ValueCalculationContext _previousContext;
        private ValueCalculationContext _currentContext;

        public ValueNode(ValueCalculationContext context1, ValueCalculationContext context2, IValue value)
        {
            _value = value;
            _previousContext = context1;
            _currentContext = context2;
        }

        public NodeValue? Value
        {
            get
            {
                (_previousContext, _currentContext) = (_currentContext, _previousContext);
                var value = _value.Calculate(_currentContext);
                UpdateSubscriptions();
                return value;
            }
        }

        public event EventHandler ValueChanged;

        public void Dispose()
        {
            foreach (var node in _currentContext.UsedNodes)
            {
                node.ValueChanged -= OnValueChanged;
            }

            foreach (var collection in _currentContext.UsedCollections)
            {
                collection.UntypedCollectionChanged -= OnValueChanged;
            }

            (_previousContext, _currentContext) = (_currentContext, _previousContext);
            _currentContext.Clear();
            UpdateSubscriptions();
        }

        private void UpdateSubscriptions()
        {
            foreach (var node in _previousContext.UsedNodes)
            {
                if (!_currentContext.UsedNodes.Contains(node))
                    node.ValueChanged -= OnValueChanged;
            }
            foreach (var collection in _previousContext.UsedCollections)
            {
                if (!_currentContext.UsedCollections.Contains(collection))
                    collection.UntypedCollectionChanged -= OnValueChanged;
            }

            foreach (var node in _currentContext.UsedNodes)
            {
                if (!_previousContext.UsedNodes.Contains(node))
                    node.ValueChanged += OnValueChanged;
            }
            foreach (var collection in _currentContext.UsedCollections)
            {
                if (!_previousContext.UsedCollections.Contains(collection))
                    collection.UntypedCollectionChanged += OnValueChanged;
            }

            _previousContext.Clear();
        }

        private void OnValueChanged(object sender, EventArgs args) => OnValueChanged();

        // Public to allow invocation on conditions this class can't know about (e.g. state changes in _value)
        public void OnValueChanged() => ValueChanged?.Invoke(this, EventArgs.Empty);
    }
}