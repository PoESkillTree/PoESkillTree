using PoESkillTree.Computation.Common;

namespace PoESkillTree.Computation.Core
{
    public interface INodeViewProviderRepository
    {
        ISuspendableEventViewProvider<ICalculationNode> GetNode(IStat stat, NodeType nodeType);
        ISuspendableEventViewProvider<INodeCollection<Modifier>> GetFormNodeCollection(IStat stat, Form form);
        ISuspendableEvents Suspender { get; }
    }
}