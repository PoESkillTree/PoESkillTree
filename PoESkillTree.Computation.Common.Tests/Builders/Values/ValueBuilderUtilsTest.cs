using System;
using System.Collections.Generic;
using Moq;
using NUnit.Framework;
using PoESkillTree.Computation.Common.Builders.Conditions;
using PoESkillTree.Computation.Common.Builders.Resolving;
using PoESkillTree.Computation.Common.Builders.Stats;

namespace PoESkillTree.Computation.Common.Builders.Values
{
    [TestFixture]
    public class ValueBuilderUtilsTest
    {
        [TestCaseSource(nameof(LinearScaleReturnsCorrectValue_TestCases))]
        public double LinearScaleReturnsCorrectValue(double input, (double x, double multiplier)[] points)
        {
            var valueBuilders = new ValueBuildersStub();
            var inputStat = CreateStatWithValue(input);

            var result = valueBuilders.LinearScale(inputStat, points);

            return Round(ValueBuilderStub.Convert(result));
        }

        private static IEnumerable<TestCaseData> LinearScaleReturnsCorrectValue_TestCases()
        {
            (double x, double y)[] simple = { (0, 1), (100, 1) };
            yield return new TestCaseData(-100, simple).Returns(1);
            yield return new TestCaseData(-1, simple).Returns(1);
            yield return new TestCaseData(0, simple).Returns(1);
            yield return new TestCaseData(42, simple).Returns(1);
            yield return new TestCaseData(100, simple).Returns(1);
            yield return new TestCaseData(101, simple).Returns(1);
            yield return new TestCaseData(200, simple).Returns(1);

            (double x, double y)[] farShot = { (0, 0), (150, 1) };
            yield return new TestCaseData(-100, farShot).Returns(0);
            yield return new TestCaseData(-1, farShot).Returns(0);
            yield return new TestCaseData(0, farShot).Returns(0);
            yield return new TestCaseData(50, farShot).Returns(Round(1.0 / 3));
            yield return new TestCaseData(75, farShot).Returns(Round(1.0 / 2));
            yield return new TestCaseData(100, farShot).Returns(Round(2.0 / 3));
            yield return new TestCaseData(150, farShot).Returns(1);
            yield return new TestCaseData(151, farShot).Returns(1);
            yield return new TestCaseData(200, simple).Returns(1);

            (double x, double y)[] pointBlank = { (0, 1), (10, 1), (35, 0), (150, -1) };
            yield return new TestCaseData(-1, pointBlank).Returns(1);
            yield return new TestCaseData(0, pointBlank).Returns(1);
            yield return new TestCaseData(4, pointBlank).Returns(1);
            yield return new TestCaseData(10, pointBlank).Returns(1);
            yield return new TestCaseData(35, pointBlank).Returns(0);
            yield return new TestCaseData(50, pointBlank).Returns(Round((50.0 - 35) / (150 - 35) * -1));
            yield return new TestCaseData(145, pointBlank).Returns(Round((145.0 - 35) / (150 - 35) * -1));
            yield return new TestCaseData(150, pointBlank).Returns(-1);
            yield return new TestCaseData(151, pointBlank).Returns(-1);
        }

        private static double Round(double value) => 
            Math.Round(value, 12);

        [Test]
        public void LinearScaleThrowsWithoutPoints()
        {
            var valueBuilders = new ValueBuildersStub();
            var inputStat = CreateStatWithValue(42);
            var points = new (double x, double y)[0];

            Assert.Throws<ArgumentException>(() => valueBuilders.LinearScale(inputStat, points));
        }

        [Test]
        public void LinearScaleThrowsWithOnePoint()
        {
            var valueBuilders = new ValueBuildersStub();
            var inputStat = CreateStatWithValue(42);
            (double x, double y)[] points = { (0, 1) };

            Assert.Throws<ArgumentException>(() => valueBuilders.LinearScale(inputStat, points));
        }

        [Test]
        public void LinearScaleThrowsWithUnorderedPoints()
        {
            var valueBuilders = new ValueBuildersStub();
            var inputStat = CreateStatWithValue(42);
            (double x, double y)[] points = { (0, 0), (1, 1), (3, 3), (2, 2) };

            Assert.Throws<ArgumentException>(() => valueBuilders.LinearScale(inputStat, points));
        }

        private static IStatBuilder CreateStatWithValue(double value)
        {
            var valueBuilder = new ValueBuilder(new ValueBuilderStub(value));
            var statMock = new Mock<IStatBuilder>();
            statMock.SetupGet(s => s.Value).Returns(valueBuilder);
            return statMock.Object;
        }

        private class ValueBuildersStub : IValueBuilders
        {
            public IThenBuilder If(IConditionBuilder condition) => 
                new ThenBuilderStub(((ConditionBuilderStub) condition).Condition, null);

            public IValueBuilder Create(double value) => 
                new ValueBuilderStub(value);

            public IValueBuilder Create(bool value) => throw new NotSupportedException();

            public IValueBuilder FromMinAndMax(IValueBuilder minimumValue, IValueBuilder maximumValue) => 
                throw new NotSupportedException();

            private class ThenBuilderStub : IThenBuilder
            {
                private readonly bool _branchCondition;
                private readonly double? _value;

                public ThenBuilderStub(bool branchCondition, double? value)
                {
                    _branchCondition = branchCondition;
                    _value = value;
                }

                public IConditionalValueBuilder Then(IValueBuilder value) =>
                    _branchCondition
                        ? new ConditionalValueBuilderStub(ValueBuilderStub.Convert(value))
                        : new ConditionalValueBuilderStub(_value);

                public IConditionalValueBuilder Then(double value) =>
                    _branchCondition
                        ? new ConditionalValueBuilderStub(value)
                        : new ConditionalValueBuilderStub(_value);
            }

            private class ConditionalValueBuilderStub : IConditionalValueBuilder
            {
                private readonly double? _value;

                public ConditionalValueBuilderStub(double? value)
                {
                    _value = value;
                }

                public IThenBuilder ElseIf(IConditionBuilder condition) => 
                    new ThenBuilderStub(((ConditionBuilderStub) condition).Condition && !_value.HasValue, _value);

                public ValueBuilder Else(IValueBuilder value) =>
                    _value is double v
                        ? new ValueBuilder(new ValueBuilderStub(v))
                        : new ValueBuilder(value);

                public ValueBuilder Else(double value) =>
                    _value is double v
                        ? new ValueBuilder(new ValueBuilderStub(v))
                        : new ValueBuilder(new ValueBuilderStub(value));
            }
        }

        private class ValueBuilderStub : IValueBuilder
        {
            public double Value { get; }

            public ValueBuilderStub(double value)
            {
                Value = value;
            }

            public static double Convert(IValueBuilder value)
            {
                if (value is ValueBuilder b)
                {
                    // Easiest way to get the underlying value back.
                    value = b.Resolve(null);
                }
                return ((ValueBuilderStub) value).Value;
            }

            public IValueBuilder Resolve(ResolveContext context) => this;

            public IValueBuilder MaximumOnly => throw new NotSupportedException();
            public IValueBuilder Average => throw new NotSupportedException();

            public IConditionBuilder Eq(IValueBuilder other) =>
                new ConditionBuilderStub(Value == Convert(other));

            public IConditionBuilder GreaterThan(IValueBuilder other) =>
                new ConditionBuilderStub(Value > Convert(other));

            public IValueBuilder Add(IValueBuilder other) =>
                new ValueBuilderStub(Value + Convert(other));

            public IValueBuilder Multiply(IValueBuilder other) =>
                new ValueBuilderStub(Value * Convert(other));

            public IValueBuilder DivideBy(IValueBuilder divisor) =>
                new ValueBuilderStub(Value / Convert(divisor));

            public IValueBuilder If(IValue condition) => throw new NotSupportedException();

            public IValueBuilder Select(Func<NodeValue, NodeValue> selector, Func<IValue, string> identity) => 
                new ValueBuilderStub(selector(new NodeValue(Value)).Single);

            public IValueBuilder Create(double value) => new ValueBuilderStub(value);

            public IValue Build(BuildParameters parameters) => throw new NotSupportedException();
        }


        private class ConditionBuilderStub : IConditionBuilder
        {
            public bool Condition { get; }

            public ConditionBuilderStub(bool condition)
            {
                Condition = condition;
            }

            public IConditionBuilder Resolve(ResolveContext context) => this;

            public IConditionBuilder And(IConditionBuilder condition) =>
                new ConditionBuilderStub(Condition && ((ConditionBuilderStub) condition).Condition);

            public IConditionBuilder Or(IConditionBuilder condition) =>
                new ConditionBuilderStub(Condition || ((ConditionBuilderStub) condition).Condition);

            public IConditionBuilder Not =>
                new ConditionBuilderStub(!Condition);

            public ConditionBuilderResult Build(BuildParameters parameters) => 
                throw new NotSupportedException();
        }
    }
}