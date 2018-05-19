using PoESkillTree.Computation.Core.NodeCollections;
using PoESkillTree.Computation.Core.Nodes;

namespace PoESkillTree.Computation.Core.Graphs
{
    /// <summary>
    /// Factory for creating the nodes and modifier node collections for stat subgraphs.
    /// </summary>
    public interface IStatNodeFactory
    {
        IDisposableNodeViewProvider Create(NodeSelector selector);
        ModifierNodeCollection Create(FormNodeSelector selector);
    }
}