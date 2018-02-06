using System.Collections.Generic;
using Moq;
using MoreLinq;
using NUnit.Framework;
using PoESkillTree.Computation.Common;
using PoESkillTree.Computation.Core.Events;
using PoESkillTree.Computation.Core.Graphs;

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
            var modifier = NodeHelper.MockModifier();
            var graphMock = new Mock<ICalculationGraph>();
            var sut = CreateSut(graphMock.Object);

            sut.AddModifier(modifier);

            graphMock.Verify(g => g.AddModifier(modifier));
        }

        [Test]
        public void RemoveModifierCallsInjectedGraph()
        {
            var modifier = NodeHelper.MockModifier();
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

            sut.AddModifier(new Modifier(new[] { stat }, Form.More, null));

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

            sut.RemoveModifier(new Modifier(new[] { stat }, Form.More, null));

            sut.RemoveUnusedNodes();
            graphMock.Verify(g => g.Remove(stat));
        }

        [Test]
        public void RemoveModifierDoesNotAddRemovalMarkIfStatGraphDoesNotContainStat()
        {
            var stat = new StatStub();
            var graphMock = MockGraph();
            var sut = CreateSut(graphMock.Object);

            sut.RemoveModifier(new Modifier(new[] { stat }, Form.More, null));

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

            sut.RemoveModifier(new Modifier(new[] { stat }, Form.More, null));

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
            var nodes = new Dictionary<NodeType, ISuspendableEventViewProvider<ICalculationNode>>
            {
                { NodeType.Base, Mock.Of<ISuspendableEventViewProvider<ICalculationNode>>() }
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
            var formNodeCollection = new Dictionary<Form, ISuspendableEventViewProvider<INodeCollection<Modifier>>>
            {
                { Form.More, Mock.Of<ISuspendableEventViewProvider<INodeCollection<Modifier>>>() }
            };
            var statGraph = Mock.Of<IStatGraph>(g => 
                g.Nodes == new Dictionary<NodeType, ISuspendableEventViewProvider<ICalculationNode>>() &&
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
            var nodes = new Dictionary<NodeType, ISuspendableEventViewProvider<ICalculationNode>>
            {
                { NodeType.Base, MockProvider<ICalculationNode>() },
                { NodeType.More, MockProvider<ICalculationNode>(5) },
                { NodeType.Total, MockProvider<ICalculationNode>() },
            };
            statGraphMock.SetupGet(g => g.Nodes).Returns(nodes);
            var formNodeCollection = new Dictionary<Form, ISuspendableEventViewProvider<INodeCollection<Modifier>>>
            {
                { Form.Increase, MockProvider<INodeCollection<Modifier>>(2) },
                { Form.More, MockProvider<INodeCollection<Modifier>>() },
            };
            statGraphMock.SetupGet(g => g.FormNodeCollections).Returns(formNodeCollection);

            var seq = new MockSequence();
            statGraphMock.InSequence(seq).Setup(g => g.RemoveNode(NodeType.Total)).Verifiable();
            statGraphMock.InSequence(seq).Setup(g => g.RemoveNode(NodeType.Base)).Verifiable();
            statGraphMock.InSequence(seq).Setup(g => g.RemoveFormNodeCollection(Form.More)).Verifiable();

            sut.RemoveUnusedNodes();

            // Verify last step of sequence to force previous calls in sequence
            statGraphMock.Verify(g => g.RemoveFormNodeCollection(Form.More));
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
                g.Nodes == new Dictionary<NodeType, ISuspendableEventViewProvider<ICalculationNode>>() &&
                g.FormNodeCollections ==
                new Dictionary<Form, ISuspendableEventViewProvider<INodeCollection<Modifier>>>());

        private static ISuspendableEventViewProvider<T> MockProvider<T>(int subscriberCount = 0) =>
            Mock.Of<ISuspendableEventViewProvider<T>>(p => p.SubscriberCount == subscriberCount);
    }
}