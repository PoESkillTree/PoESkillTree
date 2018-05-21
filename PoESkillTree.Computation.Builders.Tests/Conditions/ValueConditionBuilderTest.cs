using Moq;
using NUnit.Framework;
using PoESkillTree.Computation.Builders.Conditions;
using PoESkillTree.Computation.Common;
using PoESkillTree.Computation.Common.Builders.Conditions;
using PoESkillTree.Computation.Common.Builders.Stats;

namespace PoESkillTree.Computation.Builders.Tests.Conditions
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

            var actual = sut.Build().statConverter(expected);

            Assert.AreSame(expected, actual);
        }

        [TestCase(true)]
        [TestCase(false)]
        public void BuildReturnsCorrectValueConverter(bool condition)
        {
            var expected = ConditionalValue.Calculate(condition);
            var sut = CreateSut(condition);

            var actual = sut.Build().value.Calculate(null);

            Assert.AreEqual(expected, actual);
        }

        [TestCase(true)]
        [TestCase(false)]
        public void NotBuildsToCorrectValueConverter(bool condition)
        {
            var expected = ConditionalValue.Calculate(!condition);
            var sut = CreateSut(condition);

            var actual = sut.Not.Build().value.Calculate(null);

            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void AndCreatesCompositeConditionBuilder()
        {
            var sut = CreateSut();
            var other = Mock.Of<IConditionBuilder>();

            var actual = sut.And(other);

            Assert.AreEqual(new AndCompositeConditionBuilder(sut, other), actual);
        }

        [Test]
        public void OrCreatesCompositeConditionBuilder()
        {
            var sut = CreateSut();
            var other = Mock.Of<IConditionBuilder>();

            var actual = sut.Or(other);

            Assert.AreEqual(new OrCompositeConditionBuilder(sut, other), actual);
        }

        private static ValueConditionBuilder CreateSut(bool condition = false) =>
            new ValueConditionBuilder(_ => condition);
    }
}