using PoESkillTree.Computation.Common;
using PoESkillTree.Computation.Core.Events;
using PoESkillTree.Computation.Core.NodeCollections;
using PoESkillTree.Computation.Core.Nodes;

namespace PoESkillTree.Computation.Core.Graphs
{
    public interface IStatNodeFactory
    {
        ISuspendableEventViewProvider<IDisposableNode> Create(NodeType nodeType);
        ModifierNodeCollection Create(Form form);
    }
}