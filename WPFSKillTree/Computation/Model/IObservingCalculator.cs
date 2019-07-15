using System;
using PoESkillTree.Engine.Computation.Core;

namespace PoESkillTree.Computation.Model
{
    public interface IObservingCalculator
    {
        void SubscribeTo(IObservable<CalculatorUpdate> updateObservable);
    }
}