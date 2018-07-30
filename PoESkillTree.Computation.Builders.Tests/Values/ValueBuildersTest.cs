using Moq;
using NUnit.Framework;
using PoESkillTree.Computation.Builders.Conditions;
using PoESkillTree.Computation.Builders.Values;
using PoESkillTree.Computation.Common;
using PoESkillTree.Computation.Common.Builders.Conditions;
using PoESkillTree.Computation.Common.Builders.Stats;
using PoESkillTree.Computation.Common.Builders.Values;
using PoESkillTree.Computation.Common.Parsing;

namespace PoESkillTree.Computation.Builders.Tests.Values
{
    [TestFixture]
    public class ValueBuildersTest
    {
        [TestCase(null, null)]
        [TestCase(1, 2)]
        public void FromMinAndMaxReturnsCorrectResult(double? min, double? max)
        {
            var expected = min.HasValue && max.HasValue ? new NodeValue(min.Value, max.Value) : (NodeValue?) null;
            var minBuilder = new ValueBuilderImpl(min);
            var maxBuilder = new ValueBuilderImpl(max);
            var sut = CreateSut();

            var actual = sut.FromMinAndMax(minBuilder, maxBuilder).Build().Calculate(null);

            Assert.AreEqual(expected, actual);
        }

        [TestCase(0)]
        [TestCase(1)]
        [TestCase(2)]
        public void IfBuildsToCorrectValue(int trueBranch)
        {
            var sut = CreateSut();

            IValueBuilder valueBuilder = sut
                .If(ConstantConditionBuilder.Create(trueBranch == 0))
                .Then(0)
                .ElseIf(ConstantConditionBuilder.Create(trueBranch == 1))
                .Then(new ValueBuilderImpl(1))
                .Else(2);

            var actual = valueBuilder.Build().Calculate(null);
            Assert.AreEqual(new NodeValue(trueBranch), actual);
        }

        [Test]
        public void IfThenElseThrowsIfConditionStatConverterIsNotIdentity()
        {
            var sut = CreateSut();
            var conditionMock = new Mock<IConditionBuilder>();
            conditionMock.Setup(b => b.Build(default))
                .Returns(new ConditionBuilderResult(_ => Mock.Of<IStatBuilder>(), new Constant(0)));

            IValueBuilder valueBuilder = sut.If(conditionMock.Object).Then(1).Else(0);

            Assert.Throws<ParseException>(() => valueBuilder.Build());
        }

        private static ValueBuilders CreateSut() => new ValueBuilders();
    }
}