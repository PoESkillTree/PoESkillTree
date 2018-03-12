using PoESkillTree.Computation.Common;
using PoESkillTree.Computation.Core.NodeCollections;
using PoESkillTree.Computation.Core.Nodes;

namespace PoESkillTree.Computation.Core.Graphs
{
    public interface IStatNodeFactory
    {
        IDisposableNodeViewProvider Create(NodeType nodeType);
        ModifierNodeCollection Create(Form form);
    }
}