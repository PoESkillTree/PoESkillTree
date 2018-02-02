using System.Collections.Generic;
using System.Linq;
using Moq;
using NUnit.Framework;
using PoESkillTree.Computation.Common;
using PoESkillTree.Computation.Core.Nodes;

namespace PoESkillTree.Computation.Core.Tests.Nodes
{
    [TestFixture]
    public class FormAggregatingValueTest
    {
        [Test]
        public void SutIsValue()
        {
            var sut = CreateSut();

            Assert.IsInstanceOf<IValue>(sut);
        }

        [TestCase(0, Form.Increase)]
        [TestCase(45, Form.More, 42.0, 3.0)]
        public void CalculateReturnsCorrectResult(double expected, Form form, params double[] values)
        {
            var stat = new StatStub();
            var vs = values.Select(v => new NodeValue(v)).Cast<NodeValue?>();
            var context = Mock.Of<IValueCalculationContext>(c => c.GetValues(form, stat) == vs);
            var sut = CreateSut(stat);

            var actual = sut.Calculate(context);

            Assert.AreEqual((NodeValue?) expected, actual);
        }

        private static FormAggregatingValue CreateSut(IStat stat = null, Form form = Form.More) =>
            new FormAggregatingValue(stat ?? new StatStub(), form, Aggregate);

        private static NodeValue? Aggregate(IEnumerable<NodeValue?> vs) =>
            vs.OfType<NodeValue>().Aggregate(new NodeValue(0), (l, r) => l + r);
    }
}