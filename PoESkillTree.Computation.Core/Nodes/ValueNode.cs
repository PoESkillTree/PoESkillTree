using System;
using PoESkillTree.Computation.Common;

namespace PoESkillTree.Computation.Core.Nodes
{
    public class ValueNode : ICalculationNode, IDisposable
    {
        private readonly IValue _value;
        private readonly ValueCalculationContext _context;

        public ValueNode(ValueCalculationContext context, IValue value)
        {
            _value = value;
            _context = context;
        }

        public NodeValue? Value
        {
            get
            {
                Unsubscribe();
                var value = _value.Calculate(_context);
                Subscribe();
                return value;
            }
        }

        public event EventHandler ValueChanged;

        public void Dispose()
        {
            Unsubscribe();
        }

        private void Unsubscribe()
        {
            foreach (var node in _context.UsedNodes)
            {
                node.ValueChanged -= OnValueChanged;
            }

            foreach (var nodeCollection in _context.UsedNodeCollections)
            {
                nodeCollection.CollectionChanged -= OnValueChanged;
            }

            _context.Clear();
        }

        private void Subscribe()
        {
            foreach (var node in _context.UsedNodes)
            {
                node.ValueChanged += OnValueChanged;
            }

            foreach (var nodeCollection in _context.UsedNodeCollections)
            {
                nodeCollection.CollectionChanged += OnValueChanged;
            }
        }

        private void OnValueChanged(object sender, EventArgs args) => OnValueChanged();

        // Public to allow invocation on conditions this class can't know about (e.g. state changes in _value)
        public void OnValueChanged() => ValueChanged?.Invoke(this, EventArgs.Empty);
    }
}