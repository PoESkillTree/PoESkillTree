namespace PoESkillTree.Computation.Core
{
    public class Calculator : ICalculator
    {
        private readonly ISuspendableEvents _suspender;
        private readonly IModifierCollection _modifierCollection;
        private readonly ICalculationGraphPruner _graphPruner;

        public Calculator(
            INodeRepository nodeRepository, ISuspendableEvents suspender, 
            IModifierCollection modifierCollection, ICalculationGraphPruner graphPruner)
        {
            _suspender = suspender;
            _modifierCollection = modifierCollection;
            _graphPruner = graphPruner;
            NodeRepository = nodeRepository;
        }

        public INodeRepository NodeRepository { get; }

        public void Update(CalculatorUpdate update)
        {
            _suspender.SuspendEvents();
            // If the remove/add order matters for performance, ordering logic could be added.
            foreach (var modifier in update.RemovedModifiers)
            {
                _modifierCollection.RemoveModifier(modifier);
            }
            foreach (var modifier in update.AddedModifiers)
            {
                _modifierCollection.AddModifier(modifier);
            }
            _graphPruner.RemoveUnusedNodes();
            _suspender.ResumeEvents();
        }
        
        // An overload without the SuspendableCalculationGraph would lead to a no-op _suspender.
        // That might be a significant performance improvement for "preview" calculations, where events are not used.
        public static Calculator CreateCalculator()
        {
            var nodeFactory = new NodeFactory();
            var coreGraph = new CoreCalculationGraph(nodeFactory, new NodeCollectionFactory());
            var suspendableGraph = new SuspendableCalculationGraph(coreGraph);
            var prunableGraph = new PrunableCalculationGraph(suspendableGraph);
            suspendableGraph.TopGraph = prunableGraph;

            var nodeRepositoryViewProvider = new NodeRepositoryViewProvider(prunableGraph);
            nodeFactory.NodeRepository = nodeRepositoryViewProvider.DefaultView;
            return new Calculator(
                nodeRepositoryViewProvider.SuspendableView, nodeRepositoryViewProvider.Suspender,
                prunableGraph, prunableGraph);
        }
    }
}