using PoESkillTree.Computation.Common;
using PoESkillTree.Computation.Core.Nodes;

namespace PoESkillTree.Computation.Core.Graphs
{
    public interface INodeFactory
    {
        IDisposableNodeViewProvider Create(IValue value);
    }
}