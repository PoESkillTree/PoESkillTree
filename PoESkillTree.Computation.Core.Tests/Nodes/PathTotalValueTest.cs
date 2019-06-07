using Moq;
using NUnit.Framework;
using PoESkillTree.Computation.Common;

namespace PoESkillTree.Computation.Core.Nodes
{
    [TestFixture]
    public class PathTotalValueTest
    {
        [Test]
        public void SutIsValue()
        {
            var sut = CreateSut();

            Assert.IsInstanceOf<IValue>(sut);
        }

        [TestCase(null, null, null, null)]
        [TestCase(42, null, null, 42)]
        [TestCase(1.5, 1.0, null, 3)]
        [TestCase(1.5, 1.0, 0.5, 1.5)]
        public void CalculateReturnsCorrectResult(double? @base, double? increase, double? more, double? expected)
        {
            var stat = new StatStub();
            var context = Mock.Of<IValueCalculationContext>(c =>
                c.GetValue(stat, NodeType.Base, Path) == (NodeValue?) @base &&
                c.GetValue(stat, NodeType.Increase, Path) == (NodeValue?) increase &&
                c.GetValue(stat, NodeType.More, Path) == (NodeValue?) more);
            var sut = CreateSut(stat);

            var actual = sut.Calculate(context);

            Assert.AreEqual((NodeValue?) expected, actual);
        }

        private static PathTotalValue CreateSut(IStat stat = null) =>
            new PathTotalValue(stat, Path);

        private static readonly PathDefinition Path = NodeHelper.NotMainPath;
    }
}