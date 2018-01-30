using System;
using PoESkillTree.Computation.Common;

namespace PoESkillTree.Computation.Core
{
    public abstract class SubscriberCountingNode : ICalculationNode, ICountsSubsribers
    {
        public abstract NodeValue? Value { get; }

        public int SubscriberCount => ValueChanged?.GetInvocationList().Length ?? 0;

        public event EventHandler ValueChanged;

        public abstract void Dispose();

        protected virtual void OnValueChanged()
        {
            ValueChanged?.Invoke(this, EventArgs.Empty);
        }
    }
}