using FluentAssertions;
using Moq;
using NUnit.Framework;
using PoESkillTree.GameModel.PassiveTree;

namespace PoESkillTree.Computation.Parsing.JewelParsers
{
    [TestFixture]
    public class CompositeTransformationJewelParserTest
    {
        [Test]
        public void IsTransformationJewelModifierGivenAnyComponentIs()
        {
            var component = Mock.Of<ITransformationJewelParser>(p => p.IsTransformationJewelModifier(JewelModifier));
            var sut = new CompositeTransformationJewelParser(component);

            var actual = sut.IsTransformationJewelModifier(JewelModifier);

            actual.Should().BeTrue();
        }

        [Test]
        public void IsNoTransformationJewelModifierGivenNoComponentIs()
        {
            var component = Mock.Of<ITransformationJewelParser>(p => !p.IsTransformationJewelModifier(JewelModifier));
            var sut = new CompositeTransformationJewelParser(component);

            var actual = sut.IsTransformationJewelModifier(JewelModifier);

            actual.Should().BeFalse();
        }

        [Test]
        public void TransformationResultIsEmptyGivenNoComponentMatches()
        {
            var component = Mock.Of<ITransformationJewelParser>(p => !p.IsTransformationJewelModifier(JewelModifier));
            var sut = new CompositeTransformationJewelParser(component);

            var actual = sut.ApplyTransformation(JewelModifier, new PassiveNodeDefinition[0]);

            actual.Should().BeEmpty();
        }

        [Test]
        public void TransformationResultIsThatOfMatchingComponentGivenAComponentMatches()
        {
            var expected = new[] { new TransformedNodeModifier("", null), };
            var nodesInRadius = new PassiveNodeDefinition[0];
            var component = Mock.Of<ITransformationJewelParser>(p => 
                p.IsTransformationJewelModifier(JewelModifier) &&
                p.ApplyTransformation(JewelModifier, nodesInRadius) == expected);
            var sut = new CompositeTransformationJewelParser(component);

            var actual = sut.ApplyTransformation(JewelModifier, nodesInRadius);

            actual.Should().Equal(expected);
        }

        private const string JewelModifier = nameof(JewelModifier);
    }
}