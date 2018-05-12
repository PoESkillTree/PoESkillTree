using System.Linq;
using Moq;
using NUnit.Framework;
using PoESkillTree.Computation.Common;
using PoESkillTree.Computation.Common.Tests;
using PoESkillTree.Computation.Core.Nodes;

namespace PoESkillTree.Computation.Core.Tests.Nodes
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
            var context = Mock.Of<IValueCalculationContext>(c => c.GetValues(stat, NodeType.PathTotal) == values);
            var sut = CreateSut(stat);

            var actual = sut.Calculate(context);

            Assert.AreEqual((NodeValue?) expected, actual);
        }

        private static UncappedSubtotalValue CreateSut(IStat stat = null) => new UncappedSubtotalValue(stat);
    }
}