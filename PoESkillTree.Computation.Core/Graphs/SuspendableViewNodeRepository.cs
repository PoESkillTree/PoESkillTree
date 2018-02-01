using PoESkillTree.Computation.Common;

namespace PoESkillTree.Computation.Core.Graphs
{
    public class SuspendableViewNodeRepository : INodeRepository
    {
        private readonly IReadOnlyStatGraphCollection _statGraphCollection;

        public SuspendableViewNodeRepository(IReadOnlyStatGraphCollection statGraphCollection) =>
            _statGraphCollection = statGraphCollection;

        public ICalculationNode GetNode(IStat stat, NodeType nodeType) =>
            _statGraphCollection.GetOrAdd(stat).GetNode(nodeType).SuspendableView;

        public INodeCollection<Modifier> GetFormNodeCollection(IStat stat, Form form) =>
            _statGraphCollection.GetOrAdd(stat).GetFormNodeCollection(form).SuspendableView;
    }
}