using Moq;
using NUnit.Framework;
using PoESkillTree.Computation.Common;

namespace PoESkillTree.Computation.Core.Nodes
{
    [TestFixture]
    public class SubtotalValueTest
    {
        [Test]
        public void SutIsValue()
        {
            var sut = CreateSut();

            Assert.IsInstanceOf<IValue>(sut);
        }

        [TestCase(null, null, null, null)]
        [TestCase(42, null, null, 42)]
        [TestCase(9, 4, 8, 8)]
        [TestCase(1, 4, 8, 4)]
        [TestCase(0, 4, 8, 0)]
        [TestCase(1e-20, 0, 8, 1e-20)]
        [TestCase(null, 4, 8, null)]
        public void CalculateReturnsCorrectResult(double? uncapped, double? min, double? max, double? expected)
        {
            var minStat = new StatStub();
            var maxStat = new StatStub();
            var stat = new StatStub(minStat, maxStat);
            var context = Mock.Of<IValueCalculationContext>(c =>
                c.GetValue(stat, NodeType.UncappedSubtotal, Path) == (NodeValue?) uncapped &&
                c.GetValue(minStat, NodeType.Total, Path) == (NodeValue?) min &&
                c.GetValue(maxStat, NodeType.Total, Path) == (NodeValue?) max);
            var sut = CreateSut(stat);

            var actual = sut.Calculate(context);

            Assert.AreEqual((NodeValue?) expected, actual);
        }

        [Test]
        public void CalculateHandlesNullRangeStat()
        {
            var expected = new NodeValue(5);
            var stat = new StatStub();
            var context = Mock.Of<IValueCalculationContext>(c =>
                c.GetValue(stat, NodeType.UncappedSubtotal, Path) == expected &&
                c.GetValue(null, NodeType.Total, Path) == new NodeValue(0));
            var sut = CreateSut(stat);

            var actual = sut.Calculate(context);

            Assert.AreEqual(expected, actual);
        }

        private static SubtotalValue CreateSut(IStat stat = null) =>
            new SubtotalValue(stat);

        private static readonly PathDefinition Path = PathDefinition.MainPath;
    }
}