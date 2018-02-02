using System;
using PoESkillTree.Computation.Common;

namespace PoESkillTree.Computation.Core.Nodes
{
    public class ValueNode : IDisposableNode
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

        private void OnValueChanged(object sender, EventArgs args)
        {
            ValueChanged?.Invoke(this, args);
        }
    }
}