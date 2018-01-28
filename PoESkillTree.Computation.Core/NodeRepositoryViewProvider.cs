namespace PoESkillTree.Computation.Core
{
    public class NodeRepositoryViewProvider : ISuspendableEventViewProvider<INodeRepository>
    {
        // Passed to NodeFactory.SetNodeRepository()
        // GetNode(): INodeViewProviderRepository.GetNode().DefaultView
        // GetFormNodeCollection(): INodeViewProviderRepository.GetFormNodeCollection().DefaultView
        public INodeRepository DefaultView { get; }

        // Returned by ICalculationGraph.NodeRepository
        // GetNode(): INodeViewProviderRepository.GetNode().SuspendableView
        // GetFormNodeCollection(): INodeViewProviderRepository.GetFormNodeCollection().SuspendableView
        public INodeRepository SuspendableView { get; }

        // Used in ICalculationGraph.Update()
        // INodeViewProviderRepository.Suspender
        public ISuspendableEvents Suspender { get; }
    }
}