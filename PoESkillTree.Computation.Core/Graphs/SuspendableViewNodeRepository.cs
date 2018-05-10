using PoESkillTree.Computation.Common;

namespace PoESkillTree.Computation.Core.Graphs
{
    public class SuspendableViewNodeRepository : INodeRepository
    {
        private readonly IReadOnlyStatGraphCollection _statGraphCollection;

        public SuspendableViewNodeRepository(IReadOnlyStatGraphCollection statGraphCollection) =>
            _statGraphCollection = statGraphCollection;

        public IObservableCollection<PathDefinition> GetPaths(IStat stat) =>
            _statGraphCollection.GetOrAdd(stat).Paths.SuspendableView;

        public ICalculationNode GetNode(IStat stat, NodeType nodeType, PathDefinition path) =>
            _statGraphCollection.GetOrAdd(stat).GetNode(nodeType, path).SuspendableView;

        public INodeCollection<Modifier> GetFormNodeCollection(IStat stat, Form form, PathDefinition path) =>
            _statGraphCollection.GetOrAdd(stat).GetFormNodeCollection(form, path).SuspendableView;
    }
}