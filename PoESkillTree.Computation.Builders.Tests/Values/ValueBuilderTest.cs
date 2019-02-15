using System;
using Moq;
using NUnit.Framework;
using PoESkillTree.Computation.Builders.Values;
using PoESkillTree.Computation.Common;
using PoESkillTree.Computation.Common.Builders.Values;
using PoESkillTree.Utils;
using PoESkillTree.Utils.Extensions;

namespace PoESkillTree.Computation.Builders.Tests.Values
{
    [TestFixture]
    public class ValueBuilderTest
    {
        [Test]
        public void ResolveReturnsSelf()
        {
            var sut = CreateSut();

            Assert.AreSame(sut, sut.Resolve(null));
        }

        [Test]
        public void BuildReturnsInjectedValue()
        {
            var expected = Mock.Of<IValue>();
            var sut = CreateSut(expected);

            var actual = sut.Build();

            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void MaximumOnlyBuildsToCorrectValue()
        {
            var value = new Constant(new NodeValue(1, 2));
            var expected = new NodeValue(0, 2);
            var sut = CreateSut(value);

            var actual = sut.MaximumOnly.Build().Calculate(null);

            Assert.AreEqual(expected, actual);
        }

        [TestCase(1, 2, 3)]
        [TestCase(5, -3, 2)]
        [TestCase(6, null, 6)]
        [TestCase(null, 7, 7)]
        [TestCase(null, null, null)]
        public void AddBuildsToCorrectValue(double? left, double? right, double? expected)
        {
            var sut = CreateSut(left);

            var actual = sut.Add(new ValueBuilderImpl(right)).Build().Calculate(null);

            Assert.AreEqual((NodeValue?) expected, actual);
        }

        [TestCase(1, 2, 2)]
        [TestCase(5, -3, -15)]
        [TestCase(6, null, null)]
        [TestCase(null, 7, null)]
        [TestCase(null, null, null)]
        public void MultiplyBuildsToCorrectValue(double? left, double? right, double? expected)
        {
            var sut = CreateSut(left);

            var actual = sut.Multiply(new ValueBuilderImpl(right)).Build().Calculate(null);

            Assert.AreEqual((NodeValue?) expected, actual);
        }

        [TestCase(1, 2, 0.5)]
        [TestCase(5, -3, -5.0/3)]
        [TestCase(6, null, null)]
        [TestCase(null, 7, null)]
        [TestCase(null, null, null)]
        public void DivideByBuildsToCorrectValue(double? left, double? right, double? expected)
        {
            var sut = CreateSut(left);

            var actual = sut.DivideBy(new ValueBuilderImpl(right)).Build().Calculate(null);

            Assert.AreEqual((NodeValue?) expected, actual);
        }

        [TestCase(1)]
        [TestCase(1.5)]
        public void SelectReturnsCorrectValue(double? value)
        {
            var expected = (NodeValue?) (value is null ? (double?) null : Math.Round(value.Value));
            var sut = CreateSut(value);

            var actual = sut.Select(v => v.Select(Math.Round), _ => "").Build().Calculate(null);

            Assert.AreEqual(expected, actual);
        }

        [TestCase(5, 5, true)]
        [TestCase(6, 5, false)]
        [TestCase(5, 5 + 1e-11, true)]
        public void EqBuildsToCorrectValue(double? leftValue, double? rightValue, bool expected)
        {
            var sut = CreateSut(leftValue);

            var actual = sut.Eq(new ValueBuilderImpl(rightValue)).Build().Value.Calculate(null);

            Assert.AreEqual((NodeValue?) expected, actual);
        }

        [TestCase(5, 5, false)]
        [TestCase(6, 5, true)]
        [TestCase(1, null, true)]
        [TestCase(null, -1, true)]
        public void GreaterThanBuildsToCorrectValue(double? leftValue, double? rightValue, bool expected)
        {
            var sut = CreateSut(leftValue);

            var actual = sut.GreaterThan(new ValueBuilderImpl(rightValue)).Build().Value.Calculate(null);

            Assert.AreEqual((NodeValue?) expected, actual);
        }

        [Test]
        public void SelectBuildsToCorrectToString()
        {
            var sut = CreateSut(5);

            var value = sut.Select(Funcs.Identity, d => $"(2 * {d})").Build();

            Assert.AreEqual("(2 * 5)", value.ToString());
        }

        [TestCase(1, 2)]
        [TestCase(5, -3)]
        public void AddResolveBuildsToCorrectValue(double? leftValue, double? rightValue)
        {
            var expected = ((NodeValue?) leftValue).SumWhereNotNull((NodeValue?) rightValue);
            var context = BuildersHelper.MockResolveContext();
            var right = Mock.Of<IValueBuilder>(b => b.Resolve(context) == new ValueBuilderImpl(rightValue));
            var sut = CreateSut(leftValue);

            var resolved = sut.Add(right).Resolve(context);
            var actual = resolved.Build().Calculate(null);

            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void MultiplyOnlyCalculatesRightIfLeftCalculatesToNonNull()
        {
            var sut = CreateSut().Multiply(CreateSut(new ThrowingValue()));

            var actual = sut.Build().Calculate(null);

            Assert.IsNull(actual);
        }

        [Test]
        public void DivideByOnlyCalculatesRightIfLeftCalculatesToNonNull()
        {
            var sut = CreateSut().DivideBy(CreateSut(new ThrowingValue()));

            var actual = sut.Build().Calculate(null);

            Assert.IsNull(actual);
        }

        [Test]
        public void IfOnlyCalculatesValueIfConditionIsTrue()
        {
            var sut = CreateSut(new ThrowingValue()).If(new Constant(false));

            var actual = sut.Build().Calculate(null);

            Assert.IsNull(actual);
        }

        private static ValueBuilderImpl CreateSut(double? value = null) => new ValueBuilderImpl(value);

        private static ValueBuilderImpl CreateSut(IValue value) => new ValueBuilderImpl(value);

        private class ThrowingValue : IValue
        {
            public NodeValue? Calculate(IValueCalculationContext context)
                => throw new AssertionException("Expected value not to be calculated");
        }
    }
}