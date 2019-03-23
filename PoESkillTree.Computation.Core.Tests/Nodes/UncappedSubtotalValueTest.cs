using System.Collections.Generic;
using System.Linq;
using Moq;
using NUnit.Framework;
using PoESkillTree.Computation.Common;

namespace PoESkillTree.Computation.Core.Nodes
{
    [TestFixture]
    public class UncappedSubtotalValueTest
    {
        [Test]
        public void SutIsValue()
        {
            var sut = CreateSut();
            
            Assert.IsInstanceOf<IValue>(sut);
        }

        [TestCase(null)]
        [TestCase(0, 0.0)]
        [TestCase(0, null, 0.0)]
        [TestCase(3, 1.0, 2.0)]
        public void CalculateReturnsCorrectResult(double? expected, params double?[] pathTotals)
        {
            var stat = new StatStub();
            var values = pathTotals.Select(d => (NodeValue?) d);
            var contextMock = new Mock<IValueCalculationContext>();
            contextMock.Setup(c => c.GetPaths(stat))
                .Returns(Enumerable.Repeat(PathDefinition.MainPath, pathTotals.Length).ToList());
            contextMock.Setup(c => c.GetValue(stat, NodeType.PathTotal, PathDefinition.MainPath))
                .Returns(new Queue<NodeValue?>(values).Dequeue);
            var sut = CreateSut(stat);

            var actual = sut.Calculate(contextMock.Object);

            Assert.AreEqual((NodeValue?) expected, actual);
        }

        private static UncappedSubtotalValue CreateSut(IStat stat = null) => new UncappedSubtotalValue(stat);
    }
}