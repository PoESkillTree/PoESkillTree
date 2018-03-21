using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using PoESkillTree.Computation.Common;
using PoESkillTree.Computation.Core.Nodes;

namespace PoESkillTree.Computation.Core.Tests.Nodes
{
    [TestFixture]
    public class NodeValueAggregatorsTest
    {
        [TestCase(null)]
        [TestCase(42, 42.0)]
        [TestCase(0, 42.0, 43.0, 0.0, 4.0)]
        [TestCase(43, null, 43.0, null)]
        public void CalculateOverrideReturnsCorrectResult(double? expected, params double?[] values)
        {
            AssertReturnsCorrectResult(NodeValueAggregators.CalculateOverride, expected, values);
        }

        [Test]
        public void CalculateOverrideThrowsExceptionIfNoValueIsZero()
        {
            var values = new double?[] { 42, 43, null, 4, -3 }.Select(v => (NodeValue?) v);

            Assert.Throws<NotSupportedException>(() => NodeValueAggregators.CalculateOverride(values));
        }

        [TestCase(null)]
        [TestCase(1.42, 42.0)]
        [TestCase(1.5, 50.0, 100.0, -50.0)]
        public void CalculateMoreReturnsCorrectResult(double? expected, params double?[] values)
        {
            AssertReturnsCorrectResult(NodeValueAggregators.CalculateMore, expected, values);
        }

        [TestCase(null)]
        [TestCase(0.42, 42.0)]
        [TestCase(1, 50.0, 100.0, -50.0)]
        public void CalculateIncreaseReturnsCorrectResult(double? expected, params double?[] values)
        {
            AssertReturnsCorrectResult(NodeValueAggregators.CalculateIncrease, expected, values);
        }

        [TestCase(null)]
        [TestCase(42, 42.0)]
        [TestCase(100, 50.0, 100.0, -50.0)]
        public void CalculateBaseAddReturnsCorrectResult(double? expected, params double?[] values)
        {
            AssertReturnsCorrectResult(NodeValueAggregators.CalculateBaseAdd, expected, values);
        }
        
        [TestCase(0)]
        [TestCase(42, 42.0)]
        public void CalculateBaseSetReturnsCorrectResult(double? expected, params double?[] values)
        {
            AssertReturnsCorrectResult(NodeValueAggregators.CalculateBaseSet, expected, values);
        }

        [Test]
        public void CalculateBaseSetThrowsExceptionIfMultipleNonZeroValuesArePassed()
        {
            var values = new double?[] { 42, 0, 43 }.Select(v => (NodeValue?) v);

            Assert.Throws<NotSupportedException>(() => NodeValueAggregators.CalculateBaseSet(values));
        }

        [Test]
        public void CalculateBaseSetThrowsExceptionIfMultipleValuesWithNonUZeroMaximumArePassed()
        {
            var values = new NodeValue?[] { new NodeValue(0, -5), new NodeValue(0, 44)};

            Assert.Throws<NotSupportedException>(() => NodeValueAggregators.CalculateBaseSet(values));
        }

        [Test]
        public void CalculateBaseSetCorrectlyAggregatesValuesThatArePartlyZero()
        {
            var values = new NodeValue?[] { new NodeValue(42, 0), new NodeValue(0, -5), new NodeValue(0)};

            var actual = NodeValueAggregators.CalculateBaseSet(values);

            Assert.AreEqual(new NodeValue(42, -5), actual);
        }

        private static void AssertReturnsCorrectResult(
            NodeValueAggregator aggregator, double? expected, IEnumerable<double?> values)
        {
            var actual = aggregator(values.Select(v => (NodeValue?) v));

            Assert.AreEqual((NodeValue?) expected, actual);
        }
    }
}