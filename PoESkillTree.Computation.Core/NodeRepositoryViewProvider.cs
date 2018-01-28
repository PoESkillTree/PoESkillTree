using PoESkillTree.Computation.Common;

namespace PoESkillTree.Computation.Core
{
    public class NodeRepositoryViewProvider : ISuspendableEventViewProvider<INodeRepository>
    {
        private readonly INodeViewProviderRepository _providerRepository;

        public NodeRepositoryViewProvider(INodeViewProviderRepository providerRepository)
        {
            _providerRepository = providerRepository;
            DefaultView = new DefaultViewNodeRepository(_providerRepository);
            SuspendableView = new SuspendableViewNodeRepository(_providerRepository);
        }

        public INodeRepository DefaultView { get; }

        public INodeRepository SuspendableView { get; }

        public ISuspendableEvents Suspender => _providerRepository.Suspender;


        private class DefaultViewNodeRepository : INodeRepository
        {
            private readonly INodeViewProviderRepository _providerRepository;

            public DefaultViewNodeRepository(INodeViewProviderRepository providerRepository) => 
                _providerRepository = providerRepository;

            public ICalculationNode GetNode(IStat stat, NodeType nodeType) => 
                _providerRepository.GetNode(stat, nodeType).DefaultView;

            public INodeCollection<Modifier> GetFormNodeCollection(IStat stat, Form form) =>
                _providerRepository.GetFormNodeCollection(stat, form).DefaultView;
        }


        private class SuspendableViewNodeRepository : INodeRepository
        {
            private readonly INodeViewProviderRepository _providerRepository;

            public SuspendableViewNodeRepository(INodeViewProviderRepository providerRepository) => 
                _providerRepository = providerRepository;

            public ICalculationNode GetNode(IStat stat, NodeType nodeType) => 
                _providerRepository.GetNode(stat, nodeType).SuspendableView;

            public INodeCollection<Modifier> GetFormNodeCollection(IStat stat, Form form) => 
                _providerRepository.GetFormNodeCollection(stat, form).SuspendableView;
        }
    }
}