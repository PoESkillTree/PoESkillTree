using PoESkillTree.Computation.Common;
using PoESkillTree.Computation.Core.Events;
using PoESkillTree.Computation.Core.Nodes;

namespace PoESkillTree.Computation.Core.Graphs
{
    public interface INodeFactory
    {
        ISuspendableEventViewProvider<IDisposableNode> Create(IValue value);
    }
}