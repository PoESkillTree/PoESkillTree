using System.Linq;
using Moq;
using NUnit.Framework;
using PoESkillTree.Computation.Common;
using PoESkillTree.Computation.Common.Tests;
using PoESkillTree.Computation.Core.Nodes;

namespace PoESkillTree.Computation.Core.Tests.Nodes
{
    [TestFixture]
    public class ConvertedBaseValueTest
    {
        [Test]
        public void SutIsValue()
        {
            var sut = CreateSut();

            Assert.IsInstanceOf<IValue>(sut);
        }

        [TestCase(5)]
        [TestCase(6)]
        public void CalculateReturnsCorrectResult(double value)
        {
            var expected = new NodeValue(value);
            var stats = new IStat[] { new StatStub(), new StatStub(), new StatStub(), };
            var path = new PathDefinition(new GlobalModifierSource(), stats);
            var innerPath = new PathDefinition(new GlobalModifierSource(), stats.Skip(1).ToArray());
            var context = Mock.Of<IValueCalculationContext>(
                c => c.GetValue(stats[0], NodeType.Base, innerPath) == expected);
            var sut = CreateSut(path);

            var actual = sut.Calculate(context);

            Assert.AreEqual(expected, actual);
        }

        private static ConvertedBaseValue CreateSut(PathDefinition path = null) =>
            new ConvertedBaseValue(path);
    }
}