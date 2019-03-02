using PoESkillTree.Computation.Common;

namespace PoESkillTree.Computation.Core.Graphs
{
    /// <summary>
    /// Implementation of <see cref="INodeRepository"/> using the buffering views in an
    /// <see cref="IStatGraphCollection"/>.
    /// </summary>
    public class BufferingViewNodeRepository : INodeRepository
    {
        private readonly IStatGraphCollection _statGraphCollection;

        public BufferingViewNodeRepository(IStatGraphCollection statGraphCollection) =>
            _statGraphCollection = statGraphCollection;

        public IObservableCollection<PathDefinition> GetPaths(IStat stat) =>
            _statGraphCollection.GetOrAdd(stat).Paths.BufferingView;

        public ICalculationNode GetNode(IStat stat, NodeType nodeType, PathDefinition path) =>
            _statGraphCollection.GetOrAdd(stat).GetNode(new NodeSelector(nodeType, path)).BufferingView;

        public INodeCollection<Modifier> GetFormNodeCollection(IStat stat, Form form, PathDefinition path) =>
            _statGraphCollection.GetOrAdd(stat).GetFormNodeCollection(new FormNodeSelector(form, path)).BufferingView;
    }
}