using JetBrains.Annotations;
using PoESkillTree.Computation.Common;
using PoESkillTree.Computation.Core.Events;

namespace PoESkillTree.Computation.Core.Nodes
{
    public interface INodeFactory
    {
        ISuspendableEventViewProvider<IDisposableNode> Create(IValue value);
        ISuspendableEventViewProvider<IDisposableNode> Create([CanBeNull] IStat stat, NodeType nodeType);
    }
}