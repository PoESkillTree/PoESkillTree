using System;
using Moq;
using NUnit.Framework;
using PoESkillTree.Common.Utils.Extensions;
using PoESkillTree.Computation.Builders.Values;
using PoESkillTree.Computation.Common;

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

        [TestCase(1, 2)]
        [TestCase(5, -3)]
        [TestCase(6, null)]
        [TestCase(null, 7)]
        [TestCase(null, null)]
        public void AddBuildsToCorrectValue(double? leftValue, double? rightValue)
        {
            var expected = (NodeValue?) new[] { leftValue, rightValue }.AggregateOnValues((l, r) => l + r);
            var sut = CreateSut(leftValue);

            var actual = sut.Add(new ValueBuilderImpl(rightValue)).Build().Calculate(null);

            Assert.AreEqual(expected, actual);
        }

        [TestCase(1, 2)]
        [TestCase(5, -3)]
        [TestCase(6, null)]
        public void MultiplyBuildsToCorrectValue(double? leftValue, double? rightValue)
        {
            var expected = (NodeValue?) new[] { leftValue, rightValue }.AggregateOnValues((l, r) => l * r);
            var sut = CreateSut(leftValue);

            var actual = sut.Multiply(new ValueBuilderImpl(rightValue)).Build().Calculate(null);

            Assert.AreEqual(expected, actual);
        }

        [TestCase(1, 2)]
        [TestCase(5, -3)]
        [TestCase(6, null)]
        [TestCase(null, 7)]
        public void DivideByBuildsToCorrectValue(double? leftValue, double? rightValue)
        {
            var expected = (NodeValue?) (leftValue / (rightValue ?? 1));
            var sut = CreateSut(leftValue);

            var actual = sut.DivideBy(new ValueBuilderImpl(rightValue)).Build().Calculate(null);

            Assert.AreEqual(expected, actual);
        }

        [TestCase(1)]
        [TestCase(1.5)]
        public void SelectReturnsCorrectValue(double? value)
        {
            var expected = (NodeValue?) (value is null ? (double?) null : Math.Round(value.Value));
            var sut = CreateSut(value);

            var actual = sut.Select(Math.Round).Build().Calculate(null);

            Assert.AreEqual(expected, actual);
        }

        [TestCase(5, 5, true)]
        [TestCase(6, 5, false)]
        [TestCase(5, 5 + 1e-11, true)]
        public void EqBuildsToCorrectValue(double? leftValue, double? rightValue, bool expected)
        {
            var sut = CreateSut(leftValue);

            var actual = sut.Eq(new ValueBuilderImpl(rightValue)).Build().value.Calculate(null);

            Assert.AreEqual(ConditionalValue.Calculate(expected), actual);
        }

        [TestCase(5, 5, false)]
        [TestCase(6, 5, true)]
        public void GreaterThanBuildsToCorrectValue(double? leftValue, double? rightValue, bool expected)
        {
            var sut = CreateSut(leftValue);

            var actual = sut.GreaterThan(new ValueBuilderImpl(rightValue)).Build().value.Calculate(null);

            Assert.AreEqual(ConditionalValue.Calculate(expected), actual);
        }

        private static ValueBuilderImpl CreateSut(double? value = null) => new ValueBuilderImpl(value);

        private static ValueBuilderImpl CreateSut(IValue value) => new ValueBuilderImpl(value);
    }
}