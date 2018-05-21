using System.Collections.Generic;
using System.Linq;
using Moq;
using NUnit.Framework;
using PoESkillTree.Computation.Builders.Conditions;
using PoESkillTree.Computation.Common;
using PoESkillTree.Computation.Common.Builders;
using PoESkillTree.Computation.Common.Builders.Conditions;
using PoESkillTree.Computation.Common.Builders.Stats;
using PoESkillTree.Computation.Common.Tests;

namespace PoESkillTree.Computation.Builders.Tests.Conditions
{
    [TestFixture]
    public class AndCompositeConditionBuilderTest
    {
        [Test]
        public void ResolveResolvesConditions()
        {
            var expected = Helper.MockMany<IConditionBuilder>();
            var input = expected.Select(c => Mock.Of<IConditionBuilder>(b => b.Resolve(null) == c));
            var sut = CreateSut(input);

            var actual = sut.Resolve(null);

            Assert.IsInstanceOf<AndCompositeConditionBuilder>(actual);
            Assert.AreEqual(expected, ((AndCompositeConditionBuilder) actual).Conditions);
        }

        [Test]
        public void AndAddsToConditions()
        {
            var expected = Helper.MockMany<IConditionBuilder>();
            var sut = CreateSut(expected.Take(2));

            var actual = sut.And(expected.Last());

            Assert.AreEqual(new AndCompositeConditionBuilder(expected), actual);
        }

        [Test]
        public void NotNotsConditionsIntoOrComposite()
        {
            var expected = Helper.MockMany<IConditionBuilder>();
            var input = expected.Select(c => Mock.Of<IConditionBuilder>(b => b.Not == c));
            var sut = CreateSut(input);

            var actual = sut.Not;

            Assert.AreEqual(new OrCompositeConditionBuilder(expected), actual);
        }

        [Test]
        public void OrReturnsOrComposite()
        {
            var sut = CreateSut(Helper.MockMany<IConditionBuilder>());
            var other = Mock.Of<IConditionBuilder>();

            var actual = sut.Or(other);

            Assert.AreEqual(new OrCompositeConditionBuilder(sut, other), actual);
        }

        [Test]
        public void BuildReturnsAggregatedStatConverters()
        {
            var stats = Helper.MockMany<IStatBuilder>();
            var converters = new (StatConverter, IValue)[]
            {
                (s => s == stats[0] ? stats[1] : null, new Constant(1)),
                (s => s == stats[1] ? stats[2] : null, new Constant(1)),
            };
            var condition1 = new Mock<IConditionBuilder>();
            condition1.Setup(c => c.Build()).Returns(converters[0]);
            var condition2 = new Mock<IConditionBuilder>();
            condition2.Setup(c => c.Build()).Returns(converters[1]);
            var conditions = new[] { condition1.Object, condition2.Object };
            var sut = CreateSut(conditions);

            var actual = sut.Build().statConverter(stats[0]);

            Assert.AreEqual(stats[2], actual);
        }

        [TestCase(true)]
        [TestCase(false)]
        public void BuildReturnsTrueValueIfAllAreTrue(bool condition)
        {
            var expected = (NodeValue?) condition;
            var conditions = new IConditionBuilder[]
            {
                new TrueConditionBuilder(),
                new ValueConditionBuilder(condition),
                new TrueConditionBuilder(),
            };
            var sut = CreateSut(conditions);

            var actual = sut.Build().value.Calculate(null);

            Assert.AreEqual(expected, actual);
        }

        private static AndCompositeConditionBuilder CreateSut(IEnumerable<IConditionBuilder> conditions) =>
            new AndCompositeConditionBuilder(conditions.ToList());
    }
}