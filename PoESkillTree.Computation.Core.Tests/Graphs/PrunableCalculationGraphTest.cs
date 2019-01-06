using System.Collections.Generic;
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
        public void GetEnumeratorReturnsInjectedResult()
        {
            var expected = Mock.Of<IEnumerator<IReadOnlyStatGraph>>();
            var graphMock = new Mock<ICalculationGraph>();
            graphMock.Setup(g => g.GetEnumerator()).Returns(expected);
            var sut = CreateSut(graphMock.Object);

            var actual = sut.GetEnumerator();

            Assert.AreSame(expected, actual);
        }

        [Test]
        public void StatGraphsReturnsInjectedResult()
        {
            var expected = Mock.Of<IReadOnlyDictionary<IStat, IStatGraph>>();
            var graph = Mock.Of<ICalculationGraph>(g => g.StatGraphs == expected);
            var sut = CreateSut(graph);

            var actual = sut.StatGraphs;

            Assert.AreSame(expected, actual);
        }

        [Test]
        public void GetOrAddReturnsInjectedResult()
        {
            var expected = Mock.Of<IReadOnlyStatGraph>();
            var stat = new StatStub();
            var graph = Mock.Of<ICalculationGraph>(
                g => g.GetOrAdd(stat) == expected && g.StatGraphs.ContainsKey(stat));
            var sut = CreateSut(graph);

            var actual = sut.GetOrAdd(stat);

            Assert.AreSame(expected, actual);
        }

        [Test]
        public void RemoveCallsInjectedGraph()
        {
            var stat = new StatStub();
            var graphMock = new Mock<ICalculationGraph>();
            var sut = CreateSut(graphMock.Object);

            sut.Remove(stat);

            graphMock.Verify(g => g.Remove(stat));
        }

        [Test]
        public void AddModifierCallsInjectedGraph()
        {
            var modifier = MockModifier();
            var graphMock = new Mock<ICalculationGraph>();
            var sut = CreateSut(graphMock.Object);

            sut.AddModifier(modifier);

            graphMock.Verify(g => g.AddModifier(modifier));
        }

        [Test]
        public void RemoveModifierCallsInjectedGraph()
        {
            var modifier = MockModifier();
            var graphMock = new Mock<ICalculationGraph>();
            var sut = CreateSut(graphMock.Object);

            sut.RemoveModifier(modifier);

            graphMock.Verify(g => g.RemoveModifier(modifier));
        }

        [Test]
        public void RemoveUnusedNodesRemovesGetOrAddedStat()
        {
            var stat = new StatStub();
            var graphMock = MockGraph();
            var sut = CreateSut(graphMock.Object);
            sut.GetOrAdd(stat);
            SetStats(graphMock, stat);

            sut.RemoveUnusedNodes();

            graphMock.Verify(g => g.Remove(stat));
        }

        [Test]
        public void RemoveUnusuedNodeDoesNotRemoveStatsTwice()
        {
            var stat = new StatStub();
            var graphMock = MockGraph();
            var sut = CreateSut(graphMock.Object);
            sut.GetOrAdd(stat);
            SetStats(graphMock, stat);

            sut.RemoveUnusedNodes();
            sut.RemoveUnusedNodes();

            graphMock.Verify(g => g.Remove(stat), Times.Once);
        }

        [Test]
        public void RemoveUnusedNodesRemovesAllGetOrAddedStats()
        {
            var stat1 = new StatStub();
            var stat2 = new StatStub();
            var graphMock = MockGraph();
            var sut = CreateSut(graphMock.Object);
            sut.GetOrAdd(stat1);
            sut.GetOrAdd(stat2);
            SetStats(graphMock, stat1, stat2);

            sut.RemoveUnusedNodes();

            graphMock.Verify(g => g.Remove(stat1));
            graphMock.Verify(g => g.Remove(stat2));
        }

        [Test]
        public void GetOrAddDoesNotMarkStatForRemovalIfStatGraphsContainsIt()
        {
            var stat = new StatStub();
            var graphMock = MockGraph(stat);
            var sut = CreateSut(graphMock.Object);

            sut.GetOrAdd(stat);

            sut.RemoveUnusedNodes();
            graphMock.Verify(g => g.Remove(stat), Times.Never);
        }

        [Test]
        public void AddModifierRemovesRemovalMarks()
        {
            var stat = new StatStub();
            var graphMock = MockGraph();
            var sut = CreateSut(graphMock.Object);
            sut.GetOrAdd(stat);

            sut.AddModifier(MockModifier(stat));

            sut.RemoveUnusedNodes();
            graphMock.Verify(g => g.Remove(stat), Times.Never);
        }

        [Test]
        public void RemoveModifierAddsRemovalMark()
        {
            var stat = new StatStub();
            var graphMock = MockGraph();
            var sut = CreateSut(graphMock.Object);
            SetStats(graphMock, stat);

            sut.RemoveModifier(MockModifier(stat));

            sut.RemoveUnusedNodes();
            graphMock.Verify(g => g.Remove(stat));
        }

        [Test]
        public void RemoveModifierDoesNotAddRemovalMarkIfStatGraphDoesNotContainStat()
        {
            var stat = new StatStub();
            var graphMock = MockGraph();
            var sut = CreateSut(graphMock.Object);

            sut.RemoveModifier(MockModifier(stat));

            sut.RemoveUnusedNodes();
            graphMock.Verify(g => g.Remove(stat), Times.Never);
        }

        [Test]
        public void RemoveModifierDoesNotAddRemovalMarkIfStatGrapHasModifiers()
        {
            var stat = new StatStub();
            var graphMock = new Mock<ICalculationGraph>();
            graphMock.Setup(g => g.StatGraphs.ContainsKey(stat)).Returns(true);
            graphMock.Setup(g => g.StatGraphs[stat].ModifierCount).Returns(5);
            var sut = CreateSut(graphMock.Object);

            sut.RemoveModifier(MockModifier(stat));

            sut.RemoveUnusedNodes();
            graphMock.Verify(g => g.Remove(stat), Times.Never);
        }

        [Test]
        public void RemoveUnusedNodesDoesNotRemoveStatsWithNodes()
        {
            var stat = new StatStub();
            var graphMock = MockGraph();
            var sut = CreateSut(graphMock.Object);
            sut.GetOrAdd(stat);
            var nodes = new Dictionary<NodeSelector, ISuspendableEventViewProvider<ICalculationNode>>
            {
                { Selector(NodeType.Base), Mock.Of<ISuspendableEventViewProvider<ICalculationNode>>() }
            };
            var statGraph = Mock.Of<IStatGraph>(g => g.Nodes == nodes);
            var statGraphs = new Dictionary<IStat, IStatGraph> { { stat, statGraph } };
            graphMock.SetupGet(g => g.StatGraphs).Returns(statGraphs);

            SetStats(graphMock, stat);
            Mock.Get(graphMock.Object.StatGraphs[stat])
                .SetupGet(g => g.Nodes).Returns(nodes);

            sut.RemoveUnusedNodes();

            graphMock.Verify(g => g.Remove(stat), Times.Never);
        }

        [Test]
        public void RemoveUnusedNodesDoesNotRemoveStatsWithFormNodeCollections()
        {
            var stat = new StatStub();
            var graphMock = MockGraph();
            var sut = CreateSut(graphMock.Object);
            sut.GetOrAdd(stat);
            var formNodeCollection = new Dictionary<FormNodeSelector, ISuspendableEventViewProvider<INodeCollection<Modifier>>>
            {
                { Selector(Form.More), Mock.Of<ISuspendableEventViewProvider<INodeCollection<Modifier>>>() }
            };
            var statGraph = Mock.Of<IStatGraph>(g => 
                g.Nodes == new Dictionary<NodeSelector, ISuspendableEventViewProvider<ICalculationNode>>() &&
                g.FormNodeCollections == formNodeCollection);
            var statGraphs = new Dictionary<IStat, IStatGraph> { { stat, statGraph } };
            graphMock.SetupGet(g => g.StatGraphs).Returns(statGraphs);

            sut.RemoveUnusedNodes();

            graphMock.Verify(g => g.Remove(stat), Times.Never);
        }

        [Test]
        public void RemoveUnusedNodesRemoveCorrectNodes()
        {
            var stat = new StatStub();
            var graphMock = MockGraph();
            var sut = CreateSut(graphMock.Object);
            sut.GetOrAdd(stat);
            var statGraphMock = new Mock<IStatGraph>(MockBehavior.Strict);
            var statGraphs = new Dictionary<IStat, IStatGraph> { { stat, statGraphMock.Object } };
            graphMock.SetupGet(g => g.StatGraphs).Returns(statGraphs);
            var nodeSelectors = new[] { Selector(NodeType.Base), Selector(NodeType.More), Selector(NodeType.Total) };
            var nodes = new Dictionary<NodeSelector, ISuspendableEventViewProvider<ICalculationNode>>
            {
                { nodeSelectors[0], MockProvider<ICalculationNode>() },
                { nodeSelectors[1], MockProvider<ICalculationNode>(5) },
                { nodeSelectors[2], MockProvider<ICalculationNode>() },
            };
            statGraphMock.SetupGet(g => g.Nodes).Returns(nodes);
            var formNodeSelectors = new[] { Selector(Form.Increase), Selector(Form.More) };
            var formNodeCollection = new Dictionary<FormNodeSelector, ISuspendableEventViewProvider<INodeCollection<Modifier>>>
            {
                { formNodeSelectors[0], MockProvider<INodeCollection<Modifier>>(2) },
                { formNodeSelectors[1], MockProvider<INodeCollection<Modifier>>() },
            };
            statGraphMock.SetupGet(g => g.FormNodeCollections).Returns(formNodeCollection);

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
            var graphMock = MockGraph();
            var sut = CreateSut(graphMock.Object);
            sut.GetOrAdd(stat);
            var statGraph = MockStatGraph();
            Mock.Get(statGraph).Setup(g => g.Paths.SubscriberCount).Returns(1);
            var statGraphs = new Dictionary<IStat, IStatGraph> { { stat, statGraph } };
            graphMock.SetupGet(g => g.StatGraphs).Returns(statGraphs);

            sut.RemoveUnusedNodes();

            graphMock.Verify(g => g.Remove(stat), Times.Never);
        }

        [Test]
        public void RemoveModifierCallsDecoratedGraphFirst()
        {
            var stat = new StatStub();
            var modifier = MockModifier(stat);
            var graphMock = new Mock<ICalculationGraph>();
            graphMock.Setup(g => g.RemoveModifier(modifier)).Callback(() => SetStats(graphMock));
            var sut = CreateSut(graphMock.Object);

            sut.RemoveModifier(modifier);

            graphMock.Verify(g => g.RemoveModifier(modifier));
        }

        [Test]
        public void RemoveUnusedNodesCanRemoveMultipleNodeTypesAtOnce()
        {
            var stat = new StatStub();
            var graphMock = MockGraph();
            var sut = CreateSut(graphMock.Object);
            sut.GetOrAdd(stat);
            var statGraphMock = new Mock<IStatGraph>();
            var statGraphs = new Dictionary<IStat, IStatGraph> { { stat, statGraphMock.Object } };
            graphMock.SetupGet(g => g.StatGraphs).Returns(statGraphs);
            var nodeSelectors = new[] { Selector(NodeType.Total), Selector(NodeType.Subtotal) };
            var subTotalProviderMock = new Mock<ISuspendableEventViewProvider<ICalculationNode>>();
            var subTotalSubscriberCount = 1;
            subTotalProviderMock.SetupGet(p => p.SubscriberCount).Returns(() => subTotalSubscriberCount);
            var nodes = new Dictionary<NodeSelector, ISuspendableEventViewProvider<ICalculationNode>>
            {
                { nodeSelectors[0], MockProvider<ICalculationNode>() },
                { nodeSelectors[1], subTotalProviderMock.Object },
            };
            statGraphMock.SetupGet(g => g.Nodes).Returns(nodes);
            var formNodeCollection =
                new Dictionary<FormNodeSelector, ISuspendableEventViewProvider<INodeCollection<Modifier>>>();
            statGraphMock.SetupGet(g => g.FormNodeCollections).Returns(formNodeCollection);
            statGraphMock.Setup(g => g.RemoveNode(nodeSelectors[0])).Callback(() => subTotalSubscriberCount = 0);

            sut.RemoveUnusedNodes();

            statGraphMock.Verify(g => g.RemoveNode(nodeSelectors[0]));
            statGraphMock.Verify(g => g.RemoveNode(nodeSelectors[1]));
        }

        private static PrunableCalculationGraph CreateSut(ICalculationGraph decoratedGraph) =>
            new PrunableCalculationGraph(decoratedGraph, MockDeterminesNodeRemoval());

        private static IDeterminesNodeRemoval MockDeterminesNodeRemoval()
        {
            var mock = new Mock<IDeterminesNodeRemoval>();
            mock.Setup(o => o.CanBeRemoved(It.IsAny<ICountsSubsribers>()))
                .Returns((ICountsSubsribers c) => c.SubscriberCount == 0);
            mock.Setup(o => o.CanBeRemoved(It.IsAny<ISuspendableEventViewProvider<ICalculationNode>>()))
                .Returns((ICountsSubsribers c) => c.SubscriberCount == 0);
            return mock.Object;
        }

        private static Mock<ICalculationGraph> MockGraph(params IStat[] containedStats)
        {
            var statGraphs = new Dictionary<IStat, IStatGraph>();
            containedStats.ForEach(s => statGraphs.Add(s, MockStatGraph()));
            var graphMock = new Mock<ICalculationGraph>();
            graphMock.SetupGet(g => g.StatGraphs).Returns(statGraphs);
            return graphMock;
        }

        private static void SetStats(Mock<ICalculationGraph> graphMock, params IStat[] stats)
        {
            var statGraphs = new Dictionary<IStat, IStatGraph>();
            stats.ForEach(s => statGraphs.Add(s, MockStatGraph()));
            graphMock.SetupGet(g => g.StatGraphs).Returns(statGraphs);
        }

        private static IStatGraph MockStatGraph() =>
            Mock.Of<IStatGraph>(g =>
                g.Nodes == new Dictionary<NodeSelector, ISuspendableEventViewProvider<ICalculationNode>>() &&
                g.FormNodeCollections ==
                new Dictionary<FormNodeSelector, ISuspendableEventViewProvider<INodeCollection<Modifier>>>() &&
                g.Paths.SubscriberCount == 0);

        private static ISuspendableEventViewProvider<T> MockProvider<T>(int subscriberCount = 0) =>
            Mock.Of<ISuspendableEventViewProvider<T>>(p => p.SubscriberCount == subscriberCount);
    }
}