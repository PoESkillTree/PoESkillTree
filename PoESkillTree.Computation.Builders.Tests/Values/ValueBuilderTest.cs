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
        public void AddBuilderBuildsToCorrectValue(double? leftValue, double? rightValue)
        {
            var expected = (NodeValue?) new[] { leftValue, rightValue }.AggregateOnValues((l, r) => l + r);
            var sut = CreateSut(leftValue);

            var actual = sut.Add(new ValueBuilderImpl(rightValue)).Build().Calculate(null);

            Assert.AreEqual(expected, actual);
        }

        [TestCase(1, 2)]
        public void AddDoubleBuildsToCorrectValue(double? leftValue, double rightValue)
        {
            var expected = (NodeValue?) new[] { leftValue, rightValue }.AggregateOnValues((l, r) => l + r);
            var sut = CreateSut(leftValue);

            var actual = sut.Add(rightValue).Build().Calculate(null);

            Assert.AreEqual(expected, actual);
        }

        [TestCase(1, 2)]
        [TestCase(5, -3)]
        [TestCase(6, null)]
        public void MultiplyBuilderBuildsToCorrectValue(double? leftValue, double? rightValue)
        {
            var expected = (NodeValue?) new[] { leftValue, rightValue }.AggregateOnValues((l, r) => l * r);
            var sut = CreateSut(leftValue);

            var actual = sut.Multiply(new ValueBuilderImpl(rightValue)).Build().Calculate(null);

            Assert.AreEqual(expected, actual);
        }

        [TestCase(1, 2)]
        public void MultiplyDoubleBuildsToCorrectValue(double? leftValue, double rightValue)
        {
            var expected = (NodeValue?) new[] { leftValue, rightValue }.AggregateOnValues((l, r) => l * r);
            var sut = CreateSut(leftValue);

            var actual = sut.Multiply(rightValue).Build().Calculate(null);

            Assert.AreEqual(expected, actual);
        }

        [TestCase(1, 2)]
        [TestCase(5, -3)]
        [TestCase(6, null)]
        [TestCase(null, 7)]
        public void AsDividendBuilderBuilderBuildsToCorrectValue(double? leftValue, double? rightValue)
        {
            var expected = (NodeValue?) (leftValue / (rightValue ?? 1));
            var sut = CreateSut(leftValue);

            var actual = sut.AsDividend(new ValueBuilderImpl(rightValue)).Build().Calculate(null);

            Assert.AreEqual(expected, actual);
        }

        [TestCase(1, 2)]
        [TestCase(5, -3)]
        [TestCase(null, 7)]
        public void AsDividendDoubleBuilderBuildsToCorrectValue(double? leftValue, double rightValue)
        {
            var expected = (NodeValue?) (leftValue / rightValue);
            var sut = CreateSut(leftValue);

            var actual = sut.AsDividend(rightValue).Build().Calculate(null);

            Assert.AreEqual(expected, actual);
        }

        [TestCase(1, 2)]
        [TestCase(5, -3)]
        [TestCase(6, null)]
        public void AsDivisorDoubleBuilderBuildsToCorrectValue(double leftValue, double? rightValue)
        {
            var expected = (NodeValue?) (leftValue / (rightValue ?? 1));
            var sut = CreateSut(rightValue);

            var actual = sut.AsDivisor(leftValue).Build().Calculate(null);

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

        private static ValueBuilderImpl CreateSut(double? value = null) => new ValueBuilderImpl(value);

        private static ValueBuilderImpl CreateSut(IValue value) => new ValueBuilderImpl(value);
    }
}