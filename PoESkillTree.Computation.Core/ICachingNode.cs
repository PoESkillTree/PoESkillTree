using System;

namespace PoESkillTree.Computation.Core
{
    public interface ICachingNode : ICalculationNode
    {
        void RaiseValueChanged();

        event EventHandler ValueChangeReceived;
    }
}