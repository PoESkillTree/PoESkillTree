using System.Collections.Generic;
using System.Linq;
using Moq;
using NUnit.Framework;
using PoESkillTree.Common.Utils;
using PoESkillTree.Computation.Builders.Conditions;
using PoESkillTree.Computation.Common;
using PoESkillTree.Computation.Common.Builders;
using PoESkillTree.Computation.Common.Builders.Conditions;
using PoESkillTree.Computation.Common.Builders.Stats;
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
            var expected = (NodeValue?) condition;
            var conditions = new[]
            {
                ConstantConditionBuilder.False,
                ConstantConditionBuilder.Create(condition),
                ConstantConditionBuilder.False,
            };
            var sut = CreateSut(conditions);

            var actual = sut.Build().Value.Calculate(null);
            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void BuildReturnsCombinedStatConverters()
        {
            var expected = Mock.Of<IStatBuilder>();
            var stats = Helper.MockMany<IStatBuilder>();
            Mock.Get(stats[1]).Setup(s => s.CombineWith(stats[2])).Returns(expected);
            var converters = new (StatConverter, IValue)[]
            {
                (s => s == stats[0] ? stats[1] : null, new Constant(1)),
                (s => s, new Constant(1)),
                (s => s == stats[0] ? stats[2] : null, new Constant(1)),
            };
            var conditions = converters.Select(CreateCondition);
            var sut = CreateSut(conditions);

            var actual = sut.Build().StatConverter(stats[0]);

            Assert.AreEqual(expected, actual);

            IConditionBuilder CreateCondition((StatConverter, IValue) buildResult)
            {
                var cond = new Mock<IConditionBuilder>();
                cond.Setup(c => c.Build(default)).Returns(buildResult);
                return cond.Object;
            }
        }

        [Test]
        public void BuildDoesNotHaveStatConverterIfNoPartHas()
        {
            var sut = CreateSut(new[] { ConstantConditionBuilder.True, });

            var actual = sut.Build();

            Assert.IsFalse(actual.HasStatConverter);
        }

        [Test]
        public void BuildDoesNotHaveValueIfNoPartHas()
        {
            var sut = CreateSut(new[] { new StatConvertingConditionBuilder(Funcs.Identity), });

            var actual = sut.Build();

            Assert.IsFalse(actual.HasValue);
        }

        private static OrCompositeConditionBuilder CreateSut(IEnumerable<IConditionBuilder> conditions) =>
            new OrCompositeConditionBuilder(conditions.ToList());
    }
}