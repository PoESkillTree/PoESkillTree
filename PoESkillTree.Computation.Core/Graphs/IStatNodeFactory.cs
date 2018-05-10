using PoESkillTree.Computation.Core.NodeCollections;
using PoESkillTree.Computation.Core.Nodes;

namespace PoESkillTree.Computation.Core.Graphs
{
    public interface IStatNodeFactory
    {
        IDisposableNodeViewProvider Create(NodeSelector selector);
        ModifierNodeCollection Create(FormNodeSelector selector);
    }
}