using PoESkillTree.Computation.Common;

namespace PoESkillTree.Computation.Core.Graphs
{
    public class DefaultViewNodeRepository : INodeRepository
    {
        private readonly IReadOnlyStatGraphCollection _statGraphCollection;

        public DefaultViewNodeRepository(IReadOnlyStatGraphCollection statGraphCollection) =>
            _statGraphCollection = statGraphCollection;

        public IObservableCollection<PathDefinition> GetPaths(IStat stat) => 
            _statGraphCollection.GetOrAdd(stat).Paths.DefaultView;

        public ICalculationNode GetNode(IStat stat, NodeType nodeType, PathDefinition path) =>
            _statGraphCollection.GetOrAdd(stat).GetNode(nodeType, path).DefaultView;

        public INodeCollection<Modifier> GetFormNodeCollection(IStat stat, Form form, PathDefinition path) =>
            _statGraphCollection.GetOrAdd(stat).GetFormNodeCollection(form, path).DefaultView;
    }
}