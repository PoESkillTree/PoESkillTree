using Moq;
using NUnit.Framework;
using PoESkillTree.Computation.Common;

namespace PoESkillTree.Computation.Core.Nodes
{
    [TestFixture]
    public class BaseValueTest
    {
        [Test]
        public void SutIsValue()
        {
            var sut = CreateSut();

            Assert.IsInstanceOf<IValue>(sut);
        }

        [TestCase(null, null, null)]
        [TestCase(42, null, 42)]
        [TestCase(42, 8, 50)]
        [TestCase(null, 8, 8)]
        public void CalculateReturnsCorrectResult(double? baseSet, double? baseAdd, double? expected)
        {
            var stat = new StatStub();
            var context = Mock.Of<IValueCalculationContext>(c =>
                c.GetValue(stat, NodeType.BaseSet, Path) == (NodeValue?) baseSet &&
                c.GetValue(stat, NodeType.BaseAdd, Path) == (NodeValue?) baseAdd);
            var sut = CreateSut(stat);

            var actual = sut.Calculate(context);

            Assert.AreEqual((NodeValue?) expected, actual);
        }

        private static BaseValue CreateSut(IStat stat = null) =>
            new BaseValue(stat, Path);

        private static readonly PathDefinition Path = NodeHelper.NotMainPath;
    }
}