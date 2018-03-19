using Moq;
using NUnit.Framework;
using PoESkillTree.Computation.Common;
using PoESkillTree.Computation.Core.Nodes;

namespace PoESkillTree.Computation.Core.Tests.Nodes
{
    [TestFixture]
    public class TransformableValueTest
    {
        [Test]
        public void SutIsValue()
        {
            var sut = CreateSut();

            Assert.IsInstanceOf<IValue>(sut);
        }

        [TestCase(0)]
        [TestCase(42)]
        public void CalculateReturnsInjectedResult(double expectedValue)
        {
            var expected = new NodeValue(expectedValue);
            var context = Mock.Of<IValueCalculationContext>();
            var value = Mock.Of<IValue>(v => v.Calculate(context) == expected);
            var sut = CreateSut(value);

            var actual = sut.Calculate(context);

            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void SutIsValueTransformable()
        {
            var sut = CreateSut();

            Assert.IsInstanceOf<IValueTransformable>(sut);
        }

        [Test]
        public void CalculateAppliesTransformations()
        {
            var expected = new NodeValue(42);
            var context = Mock.Of<IValueCalculationContext>();
            var values = NodeHelper.MockMany<IValue>();
            var sut = CreateSut(values[0]);
            Mock.Get(values[2]).Setup(v => v.Calculate(context)).Returns(expected);

            sut.Add(Mock.Of<IValueTransformation>(t => t.Transform(values[0]) == values[1]));
            sut.Add(Mock.Of<IValueTransformation>(t => t.Transform(values[1]) == values[2]));
            var actual = sut.Calculate(context);

            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void RemoveRemovesTransformation()
        {
            var expected = new NodeValue(42);
            var context = Mock.Of<IValueCalculationContext>();
            var values = NodeHelper.MockMany<IValue>();
            var sut = CreateSut(values[0]);
            var transformations = new[]
            {
                Mock.Of<IValueTransformation>(t => t.Transform(values[0]) == values[1]),
                Mock.Of<IValueTransformation>(t => t.Transform(values[1]) == values[2])
            };
            sut.Add(transformations[0]);
            sut.Add(transformations[1]);
            Mock.Get(values[1]).Setup(v => v.Calculate(context)).Returns(expected);

            sut.Remove(transformations[1]);
            var actual = sut.Calculate(context);

            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void RemoveAllResetsToInitialValue()
        {
            var expected = new NodeValue(42);
            var context = Mock.Of<IValueCalculationContext>();
            var value = Mock.Of<IValue>(v => v.Calculate(context) == expected);
            var sut = CreateSut(value);
            sut.Add(Mock.Of<IValueTransformation>());

            sut.RemoveAll();
            var actual = sut.Calculate(context);

            Assert.AreEqual(expected, actual);
        }

        private static TransformableValue CreateSut(IValue value = null) => new TransformableValue(value);
    }
}