using System.Linq;
using Moq;
using NUnit.Framework;
using PoESkillTree.Computation.Common;
using PoESkillTree.Computation.Core.Nodes;

namespace PoESkillTree.Computation.Core.Tests.Nodes
{
    [TestFixture]
    public class MultiPathFormAggregatingValueTest
    {
        [Test]
        public void SutIsValue()
        {
            var sut = CreateSut();

            Assert.IsInstanceOf<IValue>(sut);
        }

        [TestCase(2)]
        [TestCase(3)]
        public void CalculateReturnsCorrectResult(int valueCount)
        {
            var expected = new NodeValue(2 * valueCount);

            var stats = new IStat[] { new StatStub(), new StatStub() };
            var source = new ModifierSourceStub(influencingSources: new ModifierSourceStub());
            var path = new PathDefinition(source, stats.Skip(1).ToList());
            var sut = CreateSut(stats[0], path);

            var paths = new[]
            {
                (stats[0], new PathDefinition(source.InfluencingSources[0])),
                (stats[1], new PathDefinition(source.InfluencingSources[0])),
                (stats[0], new PathDefinition(source.InfluencingSources[1])),
                (stats[1], new PathDefinition(source.InfluencingSources[1])),
            };
            var values = Enumerable.Repeat(new NodeValue(2), valueCount).Cast<NodeValue?>();
            var context = Mock.Of<IValueCalculationContext>(c => c.GetValues(Form.More, paths) == values);

            var actual = sut.Calculate(context);

            Assert.AreEqual(expected, actual);
        }

        private static MultiPathFormAggregatingValue CreateSut(IStat stat = null, PathDefinition path = null) => 
            new MultiPathFormAggregatingValue(stat, Form.More, path, NodeValueExtensions.Sum);
    }
}