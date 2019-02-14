using System.Collections.Generic;
using System.Linq;
using Moq;
using MoreLinq;
using NUnit.Framework;
using PoESkillTree.Computation.Common;
using PoESkillTree.Computation.Common.Tests;
using PoESkillTree.Computation.Core.Events;
using PoESkillTree.Computation.Core.Graphs;
using static PoESkillTree.Computation.Common.Tests.Helper;
using static PoESkillTree.Computation.Core.Tests.Graphs.NodeSelectorHelper;

namespace PoESkillTree.Computation.Core.Tests.Graphs
{
    [TestFixture]
    public class PrunableCalculationGraphTest
    {
        [Test]
        public void RemoveUnusedNodesRemovesAddedStat()
        {
            var stat = new StatStub();
            var graphMock = MockGraph(stat);
            var sut = CreateSut(graphMock.Object);
            sut.StatAdded(stat);

            sut.RemoveUnusedNodes();

            graphMock.Verify(g => g.Remove(stat));
        }

        [Test]
        public void RemoveUnusedNodeDoesNotRemoveStatsTwice()
        {
            var stat = new StatStub();
            var graphMock = MockGraph(stat);
            var sut = CreateSut(graphMock.Object);
            graphMock.Setup(g => g.Remove(stat)).Callback(() => sut.StatRemoved(stat));
            sut.StatAdded(stat);

            sut.RemoveUnusedNodes();
            sut.RemoveUnusedNodes();

            graphMock.Verify(g => g.Remove(stat), Times.Once);
        }

        [Test]
        public void RemoveUnusedNodesRemovesAllAddedStats()
        {
            var stat1 = new StatStub();
            var stat2 = new StatStub();
            var graphMock = MockGraph(stat1, stat2);
            var sut = CreateSut(graphMock.Object);
            sut.StatAdded(stat1);
            sut.StatAdded(stat2);

            sut.RemoveUnusedNodes();

            graphMock.Verify(g => g.Remove(stat1));
            graphMock.Verify(g => g.Remove(stat2));
        }

        [Test]
        public void ModifierAddedRemovesRemovalMarks()
        {
            var stat = new StatStub();
            var statGraphMock = new Mock<IStatGraph>();
            var graphMock = MockGraph(stat, statGraphMock.Object);
            var sut = CreateSut(graphMock.Object);
            sut.StatAdded(stat);
            statGraphMock.Setup(g => g.ModifierCount).Returns(1);

            sut.ModifierAdded(MockModifier(stat));

            sut.RemoveUnusedNodes();
            graphMock.Verify(g => g.Remove(stat), Times.Never);
        }

        [Test]
        public void ModifierRemovedAddsRemovalMark()
        {
            var stat = new StatStub();
            var graphMock = MockGraph(stat);
            var sut = CreateSut(graphMock.Object);

            sut.ModifierRemoved(MockModifier(stat));

            sut.RemoveUnusedNodes();
            graphMock.Verify(g => g.Remove(stat));
        }

        [Test]
        public void ModifierRemovedDoesNotAddRemovalMarkIfStatGraphDoesNotContainStat()
        {
            var stat = new StatStub();
            var graphMock = MockGraph();
            var sut = CreateSut(graphMock.Object);

            sut.ModifierRemoved(MockModifier(stat));

            sut.RemoveUnusedNodes();
            graphMock.Verify(g => g.Remove(stat), Times.Never);
        }

        [Test]
        public void ModifierRemovedDoesNotAddRemovalMarkIfStatGraphHasModifiers()
        {
            var stat = new StatStub();
            var graphMock = MockGraph(stat, Mock.Of<IStatGraph>(g => g.ModifierCount == 5));
            var sut = CreateSut(graphMock.Object);

            sut.ModifierRemoved(MockModifier(stat));

            sut.RemoveUnusedNodes();
            graphMock.Verify(g => g.Remove(stat), Times.Never);
        }

        [Test]
        public void RemoveUnusedNodesDoesNotRemoveStatsWithNodes()
        {
            var stat = new StatStub();
            var nodes = new Dictionary<NodeSelector, IBufferingEventViewProvider<ICalculationNode>>
            {
                { Selector(NodeType.Base), Mock.Of<IBufferingEventViewProvider<ICalculationNode>>() }
            };
            var graphMock = MockGraph(stat, MockStatGraph(nodes));
            var sut = CreateSut(graphMock.Object);
            sut.StatAdded(stat);

            sut.RemoveUnusedNodes();

            graphMock.Verify(g => g.Remove(stat), Times.Never);
        }

        [Test]
        public void RemoveUnusedNodesDoesNotRemoveStatsWithFormNodeCollections()
        {
            var stat = new StatStub();
            var formNodeCollection =
                new Dictionary<FormNodeSelector, IBufferingEventViewProvider<INodeCollection<Modifier>>>
                {
                    { Selector(Form.More), Mock.Of<IBufferingEventViewProvider<INodeCollection<Modifier>>>() }
                };
            var graphMock = MockGraph(stat, MockStatGraph(formNodes: formNodeCollection));
            var sut = CreateSut(graphMock.Object);
            sut.StatAdded(stat);

            sut.RemoveUnusedNodes();

            graphMock.Verify(g => g.Remove(stat), Times.Never);
        }

        [Test]
        public void RemoveUnusedNodesRemoveCorrectNodes()
        {
            var stat = new StatStub();
            var statGraphMock = new Mock<IStatGraph>(MockBehavior.Strict);
            statGraphMock.SetupGet(g => g.ModifierCount).Returns(0);
            var nodeSelectors = new[] { Selector(NodeType.Base), Selector(NodeType.More), Selector(NodeType.Total) };
            var nodes = new Dictionary<NodeSelector, IBufferingEventViewProvider<ICalculationNode>>
            {
                { nodeSelectors[0], MockProvider<ICalculationNode>() },
                { nodeSelectors[1], MockProvider<ICalculationNode>(5) },
                { nodeSelectors[2], MockProvider<ICalculationNode>() },
            };
            statGraphMock.SetupGet(g => g.Nodes).Returns(nodes);
            var formNodeSelectors = new[] { Selector(Form.Increase), Selector(Form.More) };
            var formNodeCollection = new Dictionary<FormNodeSelector, IBufferingEventViewProvider<INodeCollection<Modifier>>>
            {
                { formNodeSelectors[0], MockProvider<INodeCollection<Modifier>>(2) },
                { formNodeSelectors[1], MockProvider<INodeCollection<Modifier>>() },
            };
            statGraphMock.SetupGet(g => g.FormNodeCollections).Returns(formNodeCollection);
            var graphMock = MockGraph(stat, statGraphMock.Object);
            var sut = CreateSut(graphMock.Object);
            sut.StatAdded(stat);

            var seq = new MockSequence();
            statGraphMock.InSequence(seq).Setup(g => g.RemoveNode(nodeSelectors[2])).Verifiable();
            statGraphMock.InSequence(seq).Setup(g => g.RemoveNode(nodeSelectors[0])).Verifiable();
            statGraphMock.InSequence(seq).Setup(g => g.RemoveFormNodeCollection(formNodeSelectors[1])).Verifiable();

            sut.RemoveUnusedNodes();

            // Verify last step of sequence to force previous calls in sequence
            statGraphMock.Verify(g => g.RemoveFormNodeCollection(formNodeSelectors[1]));
        }

        [Test]
        public void RemoveUnusedNodesDoesNotRemoveStatsWherePathsCantBeRemoved()
        {
            var stat = new StatStub();
            var statGraph = MockStatGraph();
            Mock.Get(statGraph).Setup(g => g.Paths.SubscriberCount).Returns(1);
            var graphMock = MockGraph(stat, statGraph);
            var sut = CreateSut(graphMock.Object);
            sut.StatAdded(stat);

            sut.RemoveUnusedNodes();

            graphMock.Verify(g => g.Remove(stat), Times.Never);
        }

        [Test]
        public void RemoveUnusedNodesCanRemoveMultipleNodeTypesAtOnce()
        {
            var stat = new StatStub();
            var statGraphMock = new Mock<IStatGraph>();
            var nodeSelectors = new[] { Selector(NodeType.Total), Selector(NodeType.Subtotal) };
            var subTotalProviderMock = new Mock<IBufferingEventViewProvider<ICalculationNode>>();
            var subTotalSubscriberCount = 1;
            subTotalProviderMock.SetupGet(p => p.SubscriberCount).Returns(() => subTotalSubscriberCount);
            var nodes = new Dictionary<NodeSelector, IBufferingEventViewProvider<ICalculationNode>>
            {
                { nodeSelectors[0], MockProvider<ICalculationNode>() },
                { nodeSelectors[1], subTotalProviderMock.Object },
            };
            statGraphMock.SetupGet(g => g.Nodes).Returns(nodes);
            var formNodeCollection =
                new Dictionary<FormNodeSelector, IBufferingEventViewProvider<INodeCollection<Modifier>>>();
            statGraphMock.SetupGet(g => g.FormNodeCollections).Returns(formNodeCollection);
            statGraphMock.Setup(g => g.RemoveNode(nodeSelectors[0])).Callback(() => subTotalSubscriberCount = 0);
            var graphMock = MockGraph(stat, statGraphMock.Object);
            var sut = CreateSut(graphMock.Object);
            sut.StatAdded(stat);

            sut.RemoveUnusedNodes();

            statGraphMock.Verify(g => g.RemoveNode(nodeSelectors[0]));
            statGraphMock.Verify(g => g.RemoveNode(nodeSelectors[1]));
        }

        [Test]
        public void RemoveUnusedNodesRemovesRemainingModifiers()
        {
            var stat = new StatStub();
            var modifier = MockModifier(stat);
            var statGraphMock = new Mock<IStatGraph>();
            var ruleSet = Mock.Of<IGraphPruningRuleSet>(r => 
                r.CanStatBeConsideredForRemoval(stat, statGraphMock.Object) &&
                r.CanStatGraphBeRemoved(statGraphMock.Object));
            var formNodeSelector = new FormNodeSelector(Form.TotalOverride, PathDefinition.MainPath);
            var modifierNodeCollection = new StatNodeFactory(new EventBuffer(), null, stat)
                .Create(formNodeSelector);
            modifierNodeCollection.Add(MockProvider<ICalculationNode>(), modifier);
            var formNodeCollection =
                new Dictionary<FormNodeSelector, IBufferingEventViewProvider<INodeCollection<Modifier>>>
                {
                    { new FormNodeSelector(Form.TotalOverride, PathDefinition.MainPath), modifierNodeCollection }
                };
            statGraphMock.SetupGet(g => g.FormNodeCollections).Returns(formNodeCollection);
            var graphMock = MockGraph(stat, statGraphMock.Object);
            var sut = CreateSut(graphMock.Object, ruleSet);
            sut.StatAdded(stat);

            sut.RemoveUnusedNodes();

            graphMock.Verify(g => g.RemoveModifier(modifier));
            statGraphMock.Verify(g => g.RemoveFormNodeCollection(formNodeSelector));
            graphMock.Verify(g => g.Remove(stat));
        }

        private static CalculationGraphPruner CreateSut(ICalculationGraph decoratedGraph)
            => CreateSut(decoratedGraph, new DefaultPruningRuleSet(MockDeterminesNodeRemoval()));

        private static CalculationGraphPruner CreateSut(
            ICalculationGraph decoratedGraph, IGraphPruningRuleSet ruleSet)
            => new CalculationGraphPruner(decoratedGraph, ruleSet);

        private static IDeterminesNodeRemoval MockDeterminesNodeRemoval()
        {
            var mock = new Mock<IDeterminesNodeRemoval>();
            mock.Setup(o => o.CanBeRemoved(It.IsAny<ICountsSubsribers>()))
                .Returns((ICountsSubsribers c) => c.SubscriberCount == 0);
            mock.Setup(o => o.CanBeRemoved(It.IsAny<IBufferingEventViewProvider<ICalculationNode>>()))
                .Returns((ICountsSubsribers c) => c.SubscriberCount == 0);
            return mock.Object;
        }

        private static Mock<ICalculationGraph> MockGraph()
            => MockGraph(new IStat[0]);

        private static Mock<ICalculationGraph> MockGraph(params IStat[] containedStats)
            => MockGraph(containedStats.Select(s => (s, MockStatGraph())).ToArray());

        private static Mock<ICalculationGraph> MockGraph(IStat stat, IStatGraph statGraph)
            => MockGraph((stat, statGraph));

        private static Mock<ICalculationGraph> MockGraph(params (IStat stat, IStatGraph statGraph)[] containedStats)
        {
            var statGraphs = new Dictionary<IStat, IStatGraph>();
            containedStats.ForEach(t => statGraphs.Add(t.stat, t.statGraph));
            var graphMock = new Mock<ICalculationGraph>();
            graphMock.SetupGet(g => g.StatGraphs).Returns(statGraphs);
            return graphMock;
        }

        private static IStatGraph MockStatGraph(
            IReadOnlyDictionary<NodeSelector, IBufferingEventViewProvider<ICalculationNode>> nodes = null,
            IReadOnlyDictionary<FormNodeSelector, IBufferingEventViewProvider<INodeCollection<Modifier>>> formNodes
                = null)
        {
            nodes = nodes ?? new Dictionary<NodeSelector, IBufferingEventViewProvider<ICalculationNode>>();
            formNodes = formNodes ??
                        new Dictionary<FormNodeSelector, IBufferingEventViewProvider<INodeCollection<Modifier>>>();
            return Mock.Of<IStatGraph>(g =>
                g.Nodes == nodes &&
                g.FormNodeCollections == formNodes &&
                g.Paths.SubscriberCount == 0);
        }

        private static IBufferingEventViewProvider<T> MockProvider<T>(int subscriberCount = 0) =>
            Mock.Of<IBufferingEventViewProvider<T>>(p => p.SubscriberCount == subscriberCount);
    }
}