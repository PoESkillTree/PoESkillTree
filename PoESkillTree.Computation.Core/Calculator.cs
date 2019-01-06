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
        public static ICalculator Create()
        {
            var innerNodeFactory = new NodeFactory();
            var nodeFactory = new TransformableNodeFactory(innerNodeFactory, v => new TransformableValue(v));
            var statRegistryCollection = new NodeCollection<IStat>();
            var statRegistry = new StatRegistry(statRegistryCollection);
            var valueTransformer = new ValueTransformer();

            var (graph, pruner) = CreateCalculationGraph(nodeFactory, statRegistry, valueTransformer);

            var defaultView = new DefaultViewNodeRepository(graph);
            var suspendableView = new SuspendableViewNodeRepository(graph);
            innerNodeFactory.NodeRepository = defaultView;
            statRegistry.NodeRepository = suspendableView;

            var suspender = new SuspendableEventsComposite();
            suspender.Add(new StatGraphCollectionSuspender(graph));
            suspender.Add(statRegistryCollection);

            return new Calculator(suspender, graph, pruner, suspendableView, statRegistryCollection);
        }

        private static (ICalculationGraph, ICalculationGraphPruner) CreateCalculationGraph(
            TransformableNodeFactory nodeFactory, StatRegistry statRegistry, ValueTransformer valueTransformer)
        {
            var coreGraph =
                new CoreCalculationGraph(s => CreateStatGraph(nodeFactory, valueTransformer, s), nodeFactory);
            var eventGraph = new CalculationGraphWithEvents(coreGraph);

            var defaultPruningRuleSet = new DefaultPruningRuleSet(statRegistry);
            var defaultPruner = new CalculationGraphPruner(eventGraph, defaultPruningRuleSet);
            var userSpecifiedValuePruningRuleSet =
                new UserSpecifiedValuePruningRuleSet(defaultPruningRuleSet, statRegistry);
            var userSpecifiedValuePruner =
                new CalculationGraphPruner(eventGraph, userSpecifiedValuePruningRuleSet);
            var pruner = new CompositeCalculationGraphPruner(defaultPruner, userSpecifiedValuePruner);

            eventGraph.StatAdded += StatAdded;
            eventGraph.StatRemoved += StatRemoved;
            eventGraph.ModifierAdded += ModifierAdded;
            eventGraph.ModifierRemoved += ModifierRemoved;
            return (eventGraph, pruner);

            void StatAdded(IStat stat)
            {
                statRegistry.Add(stat);
                valueTransformer.AddBehaviors(stat.Behaviors);
                defaultPruner.StatAdded(stat);
                userSpecifiedValuePruner.StatAdded(stat);
            }

            void StatRemoved(IStat stat)
            {
                statRegistry.Remove(stat);
                valueTransformer.RemoveBehaviors(stat.Behaviors);
                defaultPruner.StatRemoved(stat);
                userSpecifiedValuePruner.StatRemoved(stat);
            }

            void ModifierAdded(Modifier modifier)
            {
                defaultPruner.ModifierAdded(modifier);
                userSpecifiedValuePruner.ModifierAdded(modifier);
            }

            void ModifierRemoved(Modifier modifier)
            {
                defaultPruner.ModifierRemoved(modifier);
                userSpecifiedValuePruner.ModifierRemoved(modifier);
            }
        }

        private static IStatGraph CreateStatGraph(
            TransformableNodeFactory nodeFactory, ValueTransformer valueTransformer, IStat stat)
        {
            var paths = new PathDefinitionCollection(SuspendableEventViewProvider.Create(
                new ObservableCollection<PathDefinition>(), new SuspendableObservableCollection<PathDefinition>()));
            var coreGraph = new CoreStatGraph(new StatNodeFactory(nodeFactory, stat), paths);
            return new StatGraphWithEvents(coreGraph, NodeAdded, NodeRemoved);

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