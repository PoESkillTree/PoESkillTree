using System;
using System.Collections.Generic;
using System.Linq;
using Moq;
using NUnit.Framework;
using PoESkillTree.Computation.Builders.Stats;
using PoESkillTree.Computation.Common;

namespace PoESkillTree.Computation.Builders.Behaviors
{
    [TestFixture]
    public class MaximumFormAggregatingValueTest
    {
        [Test]
        public void CalculateReturnsNullWithOnlyNullValues()
        {
            var sut = CreateSut();
            var context = MockContext(null, null);

            var actual = sut.Calculate(context);

            Assert.IsNull(actual);
        }

        [TestCase(0, ExpectedResult = 0)]
        [TestCase(0, null, 1, 3, 4, 2, ExpectedResult = 4)]
        public double CalculateReturnsCorrectResult(params int?[] values)
        {
            var sut = CreateSut();
            var context = MockContext(values);

            var actual = sut.Calculate(context);

            return actual.Single();
        }

        [Test]
        public void CalculateChecksForSameForm()
        {
            var transformedValue = new FunctionalValue(c => c.GetValues(Form.BaseSet, Paths).First(), "");
            var sut = CreateSut(Stat, Form.More, transformedValue);
            var context = MockContext(2, 4);

            var actual = sut.Calculate(context);

            Assert.AreEqual(2, actual.Single());
        }

        [Test]
        public void CalculateChecksForSameSta()
        {
            var transformedValue = new FunctionalValue(c => c.GetValues(Form.BaseSet, Paths).First(), "");
            var sut = CreateSut(new Stat("a"), Form.BaseSet, transformedValue);
            var context = MockContext(2, 4);

            var actual = sut.Calculate(context);

            Assert.AreEqual(2, actual.Single());
        }

        [Test]
        public void CalculateThrowsIfPathsContainsSameAndDifferentStats()
        {
            var paths = new[] { (Stat, PathDefinition.MainPath), (new Stat("a"), PathDefinition.MainPath) };
            var transformedValue = new FunctionalValue(c => c.GetValues(Form.BaseSet, paths).Single(), "");
            var sut = CreateSut(Stat, Form.BaseSet, transformedValue);
            var nodeValues = new List<NodeValue?>();
            var context = Mock.Of<IValueCalculationContext>(c => c.GetValues(Form.BaseSet, paths) == nodeValues);

            Assert.Throws<InvalidOperationException>(() => sut.Calculate(context));
        }

        private static readonly IStat Stat = new Stat("s");

        private static readonly (IStat, PathDefinition)[] Paths =
        {
            (Stat, PathDefinition.MainPath), (Stat, new PathDefinition(new ModifierSource.Local.Given()))
        };

        private static MaximumFormAggregatingValue CreateSut()
        {
            var transformedValue = new FunctionalValue(c => c.GetValues(Form.BaseSet, Paths).Single(), "");
            return CreateSut(Stat, Form.BaseSet, transformedValue);
        }

        private static MaximumFormAggregatingValue CreateSut(IStat stat, Form form, IValue transformedValue)
            => new MaximumFormAggregatingValue(stat, form, transformedValue);

        private static IValueCalculationContext MockContext(params int?[] values)
        {
            var nodeValues = values.Select(v => (NodeValue?) v).ToList();
            return Mock.Of<IValueCalculationContext>(c => c.GetValues(Form.BaseSet, Paths) == nodeValues);
        }
    }
}