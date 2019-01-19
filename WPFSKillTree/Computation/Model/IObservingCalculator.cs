using System;
using PoESkillTree.Computation.Core;

namespace POESKillTree.Computation.Model
{
    public interface IObservingCalculator
    {
        IDisposable SubscribeTo(IObservable<CalculatorUpdate> updateObservable, Action<Exception> onError);
    }
}