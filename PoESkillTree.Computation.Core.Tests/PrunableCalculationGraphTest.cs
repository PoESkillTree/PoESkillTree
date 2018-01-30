using System.Collections.Generic;
using Moq;
using NUnit.Framework;
using PoESkillTree.Computation.Common;

namespace PoESkillTree.Computation.Core.Tests
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
            var graph = Mock.Of<ICalculationGraph>(g => g.GetOrAdd(stat) == expected);
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

        private static PrunableCalculationGraph CreateSut(ICalculationGraph decoratedGraph) =>
            new PrunableCalculationGraph(decoratedGraph);
    }
}