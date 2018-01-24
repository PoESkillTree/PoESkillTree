using Moq;
using NUnit.Framework;
using PoESkillTree.Computation.Common;

namespace PoESkillTree.Computation.Core.Tests
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

        [TestCase(null, null, null, null)]
        [TestCase(42, null, null, 42)]
        [TestCase(42, 8, null, 50)]
        [TestCase(42, 8, 3, 3)]
        [TestCase(null, 8, null, 8)]
        public void CalculateReturnsCorrectResult(double? baseSet, double? baseAdd, double? baseOverride, double? expected)
        {
            var stat = new StatStub();
            var context = Mock.Of<IValueCalculationContext>(c =>
                c.GetValue(stat, NodeType.BaseSet) == (NodeValue?) baseSet &&
                c.GetValue(stat, NodeType.BaseAdd) == (NodeValue?) baseAdd &&
                c.GetValue(stat, NodeType.BaseOverride) == (NodeValue?) baseOverride);
            var sut = CreateSut(stat);

            var actual = sut.Calculate(context);

            Assert.AreEqual((NodeValue?) expected, actual);
        }

        private static BaseValue CreateSut(IStat stat = null) =>
            new BaseValue(stat);
    }
}