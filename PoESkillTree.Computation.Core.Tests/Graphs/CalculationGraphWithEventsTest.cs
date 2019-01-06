using System;
using System.Collections.Generic;
using System.Linq;
using Moq;
using NUnit.Framework;
using PoESkillTree.Computation.Common;
using PoESkillTree.Computation.Common.Tests;
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
            var modifier = Helper.MockModifier();
            var graphMock = new Mock<ICalculationGraph>();
            var sut = CreateSut(graphMock.Object);

            sut.AddModifier(modifier);

            graphMock.Verify(g => g.AddModifier(modifier));
        }

        [Test]
        public void RemoveModifierCallsInjectedGraph()
        {
            var modifier = Helper.MockModifier();
            var graphMock = new Mock<ICalculationGraph>();
            var sut = CreateSut(graphMock.Object);

            sut.RemoveModifier(modifier);

            graphMock.Verify(g => g.RemoveModifier(modifier));
        }

        [Test]
        public void GetOrAddCallsStatAddedAction()
        {
            var stat = new StatStub();
            var called = false;
            var sut = CreateSut(actual =>
            {
                Assert.AreSame(stat, actual);
                called = true;
            });

            sut.GetOrAdd(stat);

            Assert.IsTrue(called);
        }

        [Test]
        public void GetOrAddDoesNotCallStatAddedActionIfInjectedGraphContainsStat()
        {
            var stat = new StatStub();
            var sut = CreateSut(_ => Assert.Fail(), knownStats: stat);

            sut.GetOrAdd(stat);
        }

        [Test]
        public void AddModifierCallsStatAddedAction()
        {
            var stats = new[] { new StatStub(), new StatStub(), };
            var invocations = 0;
            var sut = CreateSut(actual =>
            {
                Assert.AreSame(stats[invocations], actual);
                invocations++;
            });

            sut.AddModifier(Helper.MockModifier(stats));

            Assert.AreEqual(2, invocations);
        }

        [Test]
        public void AddModifierDoesNotCallStatAddedActionIfInjectedGraphContainsStat()
        {
            var stat = new StatStub();
            var sut = CreateSut(_ => Assert.Fail(), knownStats: stat);

            sut.AddModifier(Helper.MockModifier(stat));
        }

        [Test]
        public void RemoveCallsStatRemovedAction()
        {
            var stat = new StatStub();
            var called = false;
            var sut = CreateSut(statRemovedAction: actual =>
            {
                Assert.AreSame(stat, actual);
                called = true;
            });

            sut.Remove(stat);

            Assert.IsTrue(called);
        }

        [Test]
        public void GetOrAddCallsStatAddedActionAfterCallingInjectedGraph()
        {
            var stat = new StatStub();
            var graphMock = new Mock<ICalculationGraph>();
            var getOrAddCalled = false;
            graphMock.Setup(g => g.GetOrAdd(stat)).Callback(() => getOrAddCalled = false);
            graphMock.Setup(g => g.StatGraphs.ContainsKey(stat)).Returns(() => getOrAddCalled);
            var called = false;
            var sut = CreateSut(graphMock.Object, _ =>
            {
                graphMock.Setup(g => g.GetOrAdd(stat))
                    .Throws(new AssertionException("GetOrAdd called after action"));
                called = true;
            });

            sut.GetOrAdd(stat);

            Assert.IsTrue(called);
        }

        [Test]
        public void AddModifierCallsStatAddedActionAfterCallingInjectedGraph()
        {
            var stat = new StatStub();
            var modifier = Helper.MockModifier(stat);
            var graphMock = new Mock<ICalculationGraph>();
            var addModifierCalled = false;
            graphMock.Setup(g => g.AddModifier(modifier)).Callback(() => addModifierCalled = true);
            graphMock.Setup(g => g.StatGraphs.ContainsKey(stat)).Returns(() => addModifierCalled);
            var called = false;
            var sut = CreateSut(graphMock.Object, _ =>
            {
                graphMock.Setup(g => g.AddModifier(modifier))
                    .Throws(new AssertionException("AddModifier called after action"));
                called = true;
            });

            sut.AddModifier(modifier);

            Assert.IsTrue(called);
        }

        private static CalculationGraphWithEvents CreateSut(
            Action<IStat> statAddedAction = null, Action<IStat> statRemovedAction = null, params IStat[] knownStats) =>
            CreateSut(MockGraph(knownStats), statAddedAction, statRemovedAction);

        private static CalculationGraphWithEvents CreateSut(
            ICalculationGraph decoratedGraph,
            Action<IStat> statAddedAction = null, Action<IStat> statRemovedAction = null)
        {
            var sut = new CalculationGraphWithEvents(decoratedGraph);
            if (statAddedAction != null)
                sut.StatAdded += statAddedAction;
            if (statRemovedAction != null)
                sut.StatRemoved += statRemovedAction;
            return sut;
        }

        private static ICalculationGraph MockGraph(params IStat[] knownStats)
        {
            var statGraphs = knownStats.ToDictionary(s => s, _ => Mock.Of<IStatGraph>());
            return Mock.Of<ICalculationGraph>(g => g.StatGraphs == statGraphs);
        }
    }
}