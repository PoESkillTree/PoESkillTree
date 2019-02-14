using System;
using System.Collections.Generic;
using System.Diagnostics;
using PoESkillTree.Computation.Common;
using PoESkillTree.Computation.Core.Events;

namespace PoESkillTree.Computation.Core.Nodes
{
    /// <summary>
    /// Implementation of <see cref="ICachingNode"/>.
    /// Also uses <see cref="ICycleGuard"/> to guard against cycles in the calculation graph.
    /// </summary>
    [DebuggerDisplay("{" + nameof(_value) + "}")]
    public class CachingNode : SubscriberCountingNode, IDisposable, ICachingNode, IBufferableEvent<EventArgs>
    {
        private readonly ICalculationNode _decoratedNode;
        private readonly ICycleGuard _cycleGuard;
        private readonly IEventBuffer _eventBuffer;

        private bool _calculatedValue;
        private NodeValue? _value;

        public CachingNode(ICalculationNode decoratedNode, ICycleGuard cycleGuard, IEventBuffer eventBuffer)
        {
            _decoratedNode = decoratedNode;
            _cycleGuard = cycleGuard;
            _eventBuffer = eventBuffer;
            _decoratedNode.ValueChanged += DecoratedNodeOnValueChanged;
        }

        public override NodeValue? Value
        {
            get
            {
                if (!_calculatedValue)
                {
                    CalculateValue();
                }

                return _value;
            }
        }

        private void CalculateValue()
        {
            using (_cycleGuard.Guard())
            {
                _value = _decoratedNode.Value;
            }
            _calculatedValue = true;
        }

        public event EventHandler ValueChangeReceived;

        public void Dispose()
        {
            _decoratedNode.ValueChanged -= DecoratedNodeOnValueChanged;
        }

        private void DecoratedNodeOnValueChanged(object sender, EventArgs args)
        {
            if (!_calculatedValue)
            {
                return;
            }

            _calculatedValue = false;
            ValueChangeReceived?.Invoke(this, EventArgs.Empty);
            OnValueChanged();
        }

        protected override void OnValueChanged()
            => _eventBuffer.Buffer(this, EventArgs.Empty);

        public void Invoke(IReadOnlyList<EventArgs> args)
            => base.OnValueChanged();
    }
}