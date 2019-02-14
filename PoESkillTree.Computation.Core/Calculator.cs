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
        private readonly EventBuffer _eventBuffer;
        private readonly IModifierCollection _modifierCollection;
        private readonly ICalculationGraphPruner _graphPruner;

        public Calculator(
            EventBuffer eventBuffer, IModifierCollection modifierCollection,
            ICalculationGraphPruner graphPruner, INodeRepository nodeRepository,
            INodeCollection<IStat> explicitlyRegisteredStats)
        {
            _eventBuffer = eventBuffer;
            _modifierCollection = modifierCollection;
            _graphPruner = graphPruner;
            NodeRepository = nodeRepository;
            ExplicitlyRegisteredStats = explicitlyRegisteredStats;
        }

        public INodeRepository NodeRepository { get; }

        public INodeCollection<IStat> ExplicitlyRegisteredStats { get; }

        public void Update(CalculatorUpdate update)
        {
            _eventBuffer.StartBuffering();
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
            _eventBuffer.Flush();
            _eventBuffer.StopBuffering();
        }

        public void RemoveUnusedNodes() => _graphPruner.RemoveUnusedNodes();

        /// <summary>
        /// Creates an <see cref="ICalculator"/>.
        /// </summary>
        public static ICalculator Create()
        {
            var eventBuffer = new EventBuffer();
            var innerNodeFactory = new NodeFactory(eventBuffer);
            var nodeFactory = new TransformableNodeFactory(innerNodeFactory, v => new TransformableValue(v));
            var statRegistryCollection = new NodeCollection<IStat>(eventBuffer);
            var statRegistry = new StatRegistry(statRegistryCollection);
            var valueTransformer = new ValueTransformer();

            var (graph, pruner) = CreateCalculationGraph(nodeFactory, statRegistry, valueTransformer, eventBuffer);

            var defaultView = new DefaultViewNodeRepository(graph);
            var bufferingView = new BufferingViewNodeRepository(graph);
            innerNodeFactory.NodeRepository = defaultView;
            statRegistry.NodeRepository = bufferingView;

            return new Calculator(eventBuffer, graph, pruner, bufferingView, statRegistryCollection);
        }

        private static (ICalculationGraph, ICalculationGraphPruner) CreateCalculationGraph(
            TransformableNodeFactory nodeFactory, StatRegistry statRegistry, ValueTransformer valueTransformer,
            IEventBuffer eventBuffer)
        {
            var coreGraph = new CoreCalculationGraph(
                s => CreateStatGraph(nodeFactory, valueTransformer, eventBuffer, s), nodeFactory);
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
            TransformableNodeFactory nodeFactory, ValueTransformer valueTransformer, IEventBuffer eventBuffer,
            IStat stat)
        {
            var paths = new PathDefinitionCollection(BufferingEventViewProvider.Create(
                new ObservableCollection<PathDefinition>(),
                new EventBufferingObservableCollection<PathDefinition>(eventBuffer)));
            var coreGraph = new CoreStatGraph(new StatNodeFactory(eventBuffer, nodeFactory, stat), paths);
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