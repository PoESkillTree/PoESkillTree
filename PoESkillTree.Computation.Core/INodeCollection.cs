using System.Collections.Generic;

namespace PoESkillTree.Computation.Core
{
    public interface INodeCollection<TProperty> : IObservableCollection<ICalculationNode>
    {
        IReadOnlyDictionary<ICalculationNode, TProperty> NodeProperties { get; }
    }
}