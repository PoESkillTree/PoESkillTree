using JetBrains.Annotations;
using PoESkillTree.Computation.Common;

namespace PoESkillTree.Computation.Core
{
    public interface INodeFactory
    {
        ISuspendableEventViewProvider<IDisposableNode> Create(IValue value);
        ISuspendableEventViewProvider<IDisposableNode> Create([CanBeNull] IStat stat, NodeType nodeType);
    }
}