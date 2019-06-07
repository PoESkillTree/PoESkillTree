using Moq;
using NUnit.Framework;
using PoESkillTree.Computation.Common;
using PoESkillTree.Computation.Common.Builders.Conditions;
using PoESkillTree.Computation.Common.Builders.Stats;

namespace PoESkillTree.Computation.Builders.Conditions
{
    [TestFixture]
    public class ValueConditionBuilderTest
    {
        [Test]
        public void ResolveReturnsSelf()
        {
            var sut = CreateSut();

            Assert.AreSame(sut, sut.Resolve(null));
        }

        [Test]
        public void BuildReturnsIdentityStatConverter()
        {
            var expected = Mock.Of<IStatBuilder>();
            var sut = CreateSut();

            var actual = sut.Build().StatConverter(expected);

            Assert.AreSame(expected, actual);
        }

        [TestCase(true)]
        [TestCase(false)]
        public void BuildReturnsCorrectValueConverter(bool condition)
        {
            var expected = (NodeValue?) condition;
            var sut = CreateSut(condition);

            var actual = sut.Build().Value.Calculate(null);

            Assert.AreEqual(expected, actual);
        }

        [TestCase(true)]
        [TestCase(false)]
        public void NotBuildsToCorrectValueConverter(bool condition)
        {
            var expected = (NodeValue?) !condition;
            var sut = CreateSut(condition);

            var actual = sut.Not.Build().Value.Calculate(null);

            Assert.AreEqual(expected, actual);
        }

        private static IConditionBuilder CreateSut(bool condition = false) =>
            ConstantConditionBuilder.Create(condition);
    }
}