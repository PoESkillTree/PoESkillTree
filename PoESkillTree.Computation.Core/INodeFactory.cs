using JetBrains.Annotations;
using PoESkillTree.Computation.Common;

namespace PoESkillTree.Computation.Core
{
    public interface INodeFactory
    {
        ISuspendableEventViewProvider<ICalculationNode> Create(IValue value);
        ISuspendableEventViewProvider<ICalculationNode> Create([CanBeNull] IStat stat, NodeType nodeType);
    }
}