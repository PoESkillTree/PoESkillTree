using System;
using PoESkillTree.Computation.Common;

namespace PoESkillTree.Computation.Core
{
    public class ValueNode : IDisposableNode
    {
        private readonly INodeRepository _nodeRepository;
        private readonly IValue _value;
        private ValueCalculationContext _context;

        public ValueNode(INodeRepository nodeRepository, IValue value)
        {
            _nodeRepository = nodeRepository;
            _value = value;
        }

        public NodeValue? Value
        {
            get
            {
                CreateContextIfNull();
                Unsubscribe();
                var value = _value.Calculate(_context);
                Subscribe();
                return value;
            }
        }

        private void CreateContextIfNull()
        {
            if (_context == null)
            {
                _context = new ValueCalculationContext(_nodeRepository);
            }
        }

        public event EventHandler ValueChanged;

        public void Dispose()
        {
            if (_context != null)
            {
                Unsubscribe();
            }
        }

        private void Unsubscribe()
        {
            foreach (var (stat, nodeType) in _context.Calls)
            {
                _nodeRepository.GetNode(stat, nodeType).ValueChanged -= OnValueChanged;
            }

            _context.Clear();
        }

        private void Subscribe()
        {
            foreach (var (stat, nodeType) in _context.Calls)
            {
                _nodeRepository.GetNode(stat, nodeType).ValueChanged += OnValueChanged;
            }
        }

        private void OnValueChanged(object sender, EventArgs args)
        {
            ValueChanged?.Invoke(this, args);
        }
    }
}