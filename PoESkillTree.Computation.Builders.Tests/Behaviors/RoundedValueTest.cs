using NUnit.Framework;
using PoESkillTree.Computation.Builders.Behaviors;
using PoESkillTree.Computation.Common;

namespace PoESkillTree.Computation.Builders.Tests.Behaviors
{
    [TestFixture]
    public class RoundedValueTest
    {
        [TestCase(0, 2, ExpectedResult = 0)]
        [TestCase(0.001, 2, ExpectedResult = 0)]
        [TestCase(1.5, 0, ExpectedResult = 2)]
        public double? CalculateReturnsCorrectResult(double? input, int decimals)
        {
            var sut = new RoundedValue(new Constant(input), decimals);

            return sut.Calculate(null).SingleOrNull();
        }
    }
}