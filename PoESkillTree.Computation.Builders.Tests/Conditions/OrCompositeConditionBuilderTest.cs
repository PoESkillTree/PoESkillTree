using System.Collections.Generic;
using System.Linq;
using Moq;
using NUnit.Framework;
using PoESkillTree.Computation.Builders.Conditions;
using PoESkillTree.Computation.Common;
using PoESkillTree.Computation.Common.Builders.Conditions;
using PoESkillTree.Computation.Common.Tests;

namespace PoESkillTree.Computation.Builders.Tests.Conditions
{
    [TestFixture]
    public class OrCompositeConditionBuilderTest
    {
        [Test]
        public void ResolveResolvesConditions()
        {
            var expected = Helper.MockMany<IConditionBuilder>();
            var input = expected.Select(c => Mock.Of<IConditionBuilder>(b => b.Resolve(null) == c));
            var sut = CreateSut(input);

            var actual = sut.Resolve(null);

            Assert.IsInstanceOf<OrCompositeConditionBuilder>(actual);
            Assert.AreEqual(expected, ((OrCompositeConditionBuilder) actual).Conditions);
        }

        [Test]
        public void OrAddsToConditions()
        {
            var expected = Helper.MockMany<IConditionBuilder>();
            var sut = CreateSut(expected.Take(2));

            var actual = sut.Or(expected.Last());

            Assert.AreEqual(new OrCompositeConditionBuilder(expected), actual);
        }

        [Test]
        public void NotNotsConditionsIntoAndComposite()
        {
            var expected = Helper.MockMany<IConditionBuilder>();
            var input = expected.Select(c => Mock.Of<IConditionBuilder>(b => b.Not == c));
            var sut = CreateSut(input);

            var actual = sut.Not;

            Assert.AreEqual(new AndCompositeConditionBuilder(expected), actual);
        }

        [Test]
        public void AndReturnsAndComposite()
        {
            var sut = CreateSut(Helper.MockMany<IConditionBuilder>());
            var other = Mock.Of<IConditionBuilder>();

            var actual = sut.And(other);

            Assert.AreEqual(new AndCompositeConditionBuilder(sut, other), actual);
        }

        [TestCase(true)]
        [TestCase(false)]
        public void BuildReturnsTrueValueIfAnyIsTrue(bool condition)
        {
            var expected = ConditionalValue.Calculate(condition);
            var conditions = new[]
            {
                new TrueConditionBuilder().Not,
                new ValueConditionBuilder(_ => condition),
                new TrueConditionBuilder().Not,
            };
            var sut = CreateSut(conditions);

            var actual = sut.Build().value.Calculate(null);
            Assert.AreEqual(expected, actual);
        }

        private static OrCompositeConditionBuilder CreateSut(IEnumerable<IConditionBuilder> conditions) =>
            new OrCompositeConditionBuilder(conditions.ToList());
    }
}