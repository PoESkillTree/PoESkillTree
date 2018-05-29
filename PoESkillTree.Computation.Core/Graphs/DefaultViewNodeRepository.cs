using PoESkillTree.Computation.Common;

namespace PoESkillTree.Computation.Core.Graphs
{
    /// <summary>
    /// Implementation of <see cref="INodeRepository"/> using the default views in an
    /// <see cref="IStatGraphCollection"/>.
    /// </summary>
    public class DefaultViewNodeRepository : INodeRepository
    {
        private readonly IStatGraphCollection _statGraphCollection;

        public DefaultViewNodeRepository(IStatGraphCollection statGraphCollection) =>
            _statGraphCollection = statGraphCollection;

        public IObservableCollection<PathDefinition> GetPaths(IStat stat) => 
            _statGraphCollection.GetOrAdd(stat).Paths.DefaultView;

        public ICalculationNode GetNode(IStat stat, NodeType nodeType, PathDefinition path) =>
            _statGraphCollection.GetOrAdd(stat).GetNode(new NodeSelector(nodeType, path)).DefaultView;

        public INodeCollection<Modifier> GetFormNodeCollection(IStat stat, Form form, PathDefinition path) =>
            _statGraphCollection.GetOrAdd(stat).GetFormNodeCollection(new FormNodeSelector(form, path)).DefaultView;
    }
}