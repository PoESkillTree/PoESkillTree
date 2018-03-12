using System;
using PoESkillTree.Computation.Common;
using PoESkillTree.Computation.Core.Events;

namespace PoESkillTree.Computation.Core.Nodes
{
    public abstract class SubscriberCountingNode : ICalculationNode, ICountsSubsribers
    {
        public abstract NodeValue? Value { get; }

        public int SubscriberCount => ValueChanged?.GetInvocationList().Length ?? 0;

        public event EventHandler ValueChanged;

        protected virtual void OnValueChanged()
        {
            ValueChanged?.Invoke(this, EventArgs.Empty);
        }
    }
}