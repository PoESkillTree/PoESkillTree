using Moq;
using NUnit.Framework;
using PoESkillTree.Computation.Common;

namespace PoESkillTree.Computation.Core.Tests
{
    [TestFixture]
    public class TotalValueTest
    {
        [Test]
        public void SutIsValue()
        {
            var sut = CreateSut();

            Assert.IsInstanceOf<IValue>(sut);
        }

        [TestCase(42, null, 42)]
        [TestCase(null, null, null)]
        [TestCase(42, 43, 43)]
        public void CalculateReturnsCorrectResult(double? subtotal, double? totalOverride, double? expected)
        {
            var stat = new StatStub();
            var context = Mock.Of<IValueCalculationContext>(c =>
                    c.GetValue(stat, NodeType.Subtotal) == (NodeValue?) subtotal &&
                    c.GetValue(stat, NodeType.TotalOverride) == (NodeValue?) totalOverride);
            var sut = CreateSut(stat);

            var actual = sut.Calculate(context);

            Assert.AreEqual((NodeValue?) expected, actual);
        }

        private static TotalValue CreateSut(IStat stat = null) =>
            new TotalValue(stat);
    }
}