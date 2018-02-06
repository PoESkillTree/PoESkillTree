using System.Collections.Generic;
using System.Linq;
using Moq;
using NUnit.Framework;
using PoESkillTree.Computation.Common;
using PoESkillTree.Computation.Core.Graphs;

namespace PoESkillTree.Computation.Core.Tests.Graphs
{
    [TestFixture]
    public class CalculationGraphWithEventsTest
    {
        [Test]
        public void SutIsCalculationGraph()
        {
            var sut = CreateSut();

            Assert.IsInstanceOf<ICalculationGraph>(sut);
        }

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
        public void GetOrAddRaisesStatAdded()
        {
            var stat = new StatStub();
            var sut = CreateSut();
            var raised = false;
            sut.StatAdded += (sender, args) =>
            {
                Assert.AreSame(stat, args.Stat);
                raised = true;
            };

            sut.GetOrAdd(stat);

            Assert.IsTrue(raised);
        }

        [Test]
        public void GetOrAddDoesNotRaiseStatAddedIfInjectedGraphContainsStat()
        {
            var stat = new StatStub();
            var sut = CreateSut(stat);
            sut.StatAdded += (sender, args) => { Assert.Fail(); };

            sut.GetOrAdd(stat);
        }

        [Test]
        public void AddModifierRaisesStatAdded()
        {
            var stats = new[] { new StatStub(), new StatStub(), };
            var sut = CreateSut();
            var invocations = 0;
            sut.StatAdded += (sender, args) =>
            {
                Assert.AreSame(stats[invocations], args.Stat);
                invocations++;
            };

            sut.AddModifier(new Modifier(stats, Form.More, null));

            Assert.AreEqual(2, invocations);
        }

        [Test]
        public void AddModifierDoesNotRaiseStatAddedIfInjectedGraphContainsStat()
        {
            var stat = new StatStub();
            var sut = CreateSut(stat);
            sut.StatAdded += (sender, args) => { Assert.Fail(); };

            sut.AddModifier(new Modifier(new[] { stat }, Form.More, null));
        }

        [Test]
        public void RemoveRaisesStatRemoved()
        {
            var stat = new StatStub();
            var sut = CreateSut();
            var raised = false;
            sut.StatRemoved += (sender, args) =>
            {
                Assert.AreSame(stat, args.Stat);
                raised = true;
            };

            sut.Remove(stat);

            Assert.IsTrue(raised);
        }

        [Test]
        public void GetOrAddRaisesStatAddedAfterCallingInjectedGraph()
        {
            var stat = new StatStub();
            var graphMock = new Mock<ICalculationGraph>();
            var getOrAddCalled = false;
            graphMock.Setup(g => g.GetOrAdd(stat)).Callback(() => getOrAddCalled = false);
            graphMock.Setup(g => g.StatGraphs.ContainsKey(stat)).Returns(() => getOrAddCalled);
            var sut = CreateSut(graphMock.Object);
            var raised = false;
            sut.StatAdded += (sender, args) =>
            {
                graphMock.Setup(g => g.GetOrAdd(stat))
                    .Throws(new AssertionException("GetOrAdd called after raising StatAdded"));
                raised = true;
            };

            sut.GetOrAdd(stat);

            Assert.IsTrue(raised);
        }

        [Test]
        public void AddModifierRaisesStatAddedAfterCallingInjectedGraph()
        {
            var stat = new StatStub();
            var modifier = new Modifier(new[] { stat }, Form.More, null);
            var graphMock = new Mock<ICalculationGraph>();
            var addModifierCalled = false;
            graphMock.Setup(g => g.AddModifier(modifier)).Callback(() => addModifierCalled = true);
            graphMock.Setup(g => g.StatGraphs.ContainsKey(stat)).Returns(() => addModifierCalled);
            var sut = CreateSut(graphMock.Object);
            var raised = false;
            sut.StatAdded += (sender, args) =>
            {
                graphMock.Setup(g => g.AddModifier(modifier))
                    .Throws(new AssertionException("AddModifier called after raising StatAdded"));
                raised = true;
            };

            sut.AddModifier(modifier);

            Assert.IsTrue(raised);
        }

        private static CalculationGraphWithEvents CreateSut(params IStat[] knownStats) =>
            CreateSut(MockGraph(knownStats));

        private static CalculationGraphWithEvents CreateSut(ICalculationGraph decoratedGraph) =>
            new CalculationGraphWithEvents(decoratedGraph);

        private static ICalculationGraph MockGraph(params IStat[] knownStats)
        {
            var statGraphs = knownStats.ToDictionary(s => s, _ => Mock.Of<IStatGraph>());
            return Mock.Of<ICalculationGraph>(g => g.StatGraphs == statGraphs);
        }
    }
}