using PoESkillTree.Computation.Common;
using PoESkillTree.Computation.Core.Events;
using PoESkillTree.Computation.Core.Graphs;
using PoESkillTree.Computation.Core.NodeCollections;
using PoESkillTree.Computation.Core.Nodes;

namespace PoESkillTree.Computation.Core
{
    /// <summary>
    /// Implementation of <see cref="ICalculator"/> and composition root of Computation.Core.
    /// </summary>
    public class Calculator : ICalculator
    {
        private readonly ISuspendableEvents _suspender;
        private readonly IModifierCollection _modifierCollection;
        private readonly ICalculationGraphPruner _graphPruner;

        public Calculator(
            ISuspendableEvents suspender, IModifierCollection modifierCollection,
            ICalculationGraphPruner graphPruner, INodeRepository nodeRepository,
            INodeCollection<IStat> explicitlyRegisteredStats)
        {
            _suspender = suspender;
            _modifierCollection = modifierCollection;
            _graphPruner = graphPruner;
            NodeRepository = nodeRepository;
            ExplicitlyRegisteredStats = explicitlyRegisteredStats;
        }

        public INodeRepository NodeRepository { get; }

        public INodeCollection<IStat> ExplicitlyRegisteredStats { get; }

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

        /// <summary>
        /// Creates an <see cref="ICalculator"/>.
        /// </summary>
        public static ICalculator CreateCalculator()
        {
            var innerNodeFactory = new NodeFactory();
            var nodeFactory = new TransformableNodeFactory(innerNodeFactory, v => new TransformableValue(v));
            var statRegistryCollection = new NodeCollection<IStat>();
            var statRegistry = new StatRegistry(statRegistryCollection);
            var valueTransformer = new ValueTransformer();

            var prunableGraph = CreateCalculationGraph(nodeFactory, statRegistry, valueTransformer);

            var defaultView = new DefaultViewNodeRepository(prunableGraph);
            var suspendableView = new SuspendableViewNodeRepository(prunableGraph);
            innerNodeFactory.NodeRepository = defaultView;
            statRegistry.NodeRepository = suspendableView;

            var suspender = new SuspendableEventsComposite();
            suspender.Add(new StatGraphCollectionSuspender(prunableGraph));
            suspender.Add(statRegistryCollection);

            return new Calculator(suspender, prunableGraph, prunableGraph, suspendableView, statRegistryCollection);
        }

        private static PrunableCalculationGraph CreateCalculationGraph(
            TransformableNodeFactory nodeFactory, StatRegistry statRegistry, ValueTransformer valueTransformer)
        {
            var coreGraph =
                new CoreCalculationGraph(s => CreateStatGraph(nodeFactory, valueTransformer, s), nodeFactory);
            var eventGraph = new CalculationGraphWithEvents(coreGraph, StatAdded, StatRemoved);
            return new PrunableCalculationGraph(eventGraph, statRegistry);

            void StatAdded(IStat stat)
            {
                statRegistry.Add(stat);
                valueTransformer.AddBehaviors(stat.Behaviors);
            }

            void StatRemoved(IStat stat)
            {
                statRegistry.Remove(stat);
                valueTransformer.RemoveBehaviors(stat.Behaviors);
            }
        }

        private static IStatGraph CreateStatGraph(
            TransformableNodeFactory nodeFactory, ValueTransformer valueTransformer, IStat stat)
        {
            var paths = new PathDefinitionCollection(SuspendableEventViewProvider.Create(
                new ObservableCollection<PathDefinition>(), new SuspendableObservableCollection<PathDefinition>()));
            var coreGraph = new CoreStatGraph(new StatNodeFactory(nodeFactory, stat), paths);
            var eventGraph = new StatGraphWithEvents(coreGraph, NodeAdded, NodeRemoved);
            return new SelectorValidatingStatGraph(eventGraph);

            void NodeAdded(NodeSelector selector)
            {
                var node = coreGraph.Nodes[selector];
                var transformable = nodeFactory.TransformableDictionary[node];
                valueTransformer.AddTransformable(stat, selector, transformable);
            }

            void NodeRemoved(NodeSelector selector) => valueTransformer.RemoveTransformable(stat, selector);
        }
    }
}