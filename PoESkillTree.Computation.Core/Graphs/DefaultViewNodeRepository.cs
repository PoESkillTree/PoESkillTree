using PoESkillTree.Computation.Common;

namespace PoESkillTree.Computation.Core.Graphs
{
    public class DefaultViewNodeRepository : INodeRepository
    {
        private readonly IReadOnlyStatGraphCollection _statGraphCollection;

        public DefaultViewNodeRepository(IReadOnlyStatGraphCollection statGraphCollection) =>
            _statGraphCollection = statGraphCollection;

        public ICalculationNode GetNode(IStat stat, NodeType nodeType) =>
            _statGraphCollection.GetOrAdd(stat).GetNode(nodeType).DefaultView;

        public INodeCollection<Modifier> GetFormNodeCollection(IStat stat, Form form) =>
            _statGraphCollection.GetOrAdd(stat).GetFormNodeCollection(form).DefaultView;
    }
}