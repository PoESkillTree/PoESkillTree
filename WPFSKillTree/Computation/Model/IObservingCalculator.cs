using System;
using PoESkillTree.Computation.Core;

namespace PoESkillTree.Computation.Model
{
    public interface IObservingCalculator
    {
        void SubscribeTo(IObservable<CalculatorUpdate> updateObservable);
    }
}