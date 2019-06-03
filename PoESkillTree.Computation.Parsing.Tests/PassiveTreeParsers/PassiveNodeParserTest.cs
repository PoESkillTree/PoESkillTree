using System.Linq;
using FluentAssertions;
using Moq;
using MoreLinq;
using NUnit.Framework;
using PoESkillTree.Computation.Builders.Stats;
using PoESkillTree.Computation.Common;
using PoESkillTree.GameModel;
using PoESkillTree.GameModel.PassiveTree;
using static PoESkillTree.Computation.Parsing.ParserTestUtils;

namespace PoESkillTree.Computation.Parsing.PassiveTreeParsers
{
    [TestFixture]
    public class PassiveNodeParserTest
    {
        [Test]
        public void ParsesModifiersCorrectly()
        {
            var definition = CreateNode("+5 to maximum Life");
            var source = CreateGlobalSource(definition);
            var coreResult = CreateModifier("Life", Form.BaseAdd, 2, CreateGlobalSource(definition));
            var expected = CreateConditionalModifier(definition, "Life", Form.BaseAdd, 2);
            var coreParser = Mock.Of<ICoreParser>(p =>
                p.Parse(new CoreParserParameter("+5 to maximum Life", source, Entity.Character))
                == ParseResult.Success(new[] { coreResult }));
            var sut = CreateSut(definition, coreParser);

            var result = sut.Parse(definition.Id);

            Assert.That(result.Modifiers, Has.Member(expected));
        }

        [TestCase(true)]
        [TestCase(false)]
        public void AddsToPassivePointsIfCostsPoint(bool costsPassivePoint)
        {
            var definition = CreateNode(false, 0, costsPassivePoint);
            var expected = CreateConditionalModifier(definition, "PassivePoints", Form.BaseAdd, 1);
            var sut = CreateSut(definition);

            var result = sut.Parse(definition.Id);

            Assert.That(result.Modifiers,
                costsPassivePoint ? Has.Member(expected) : Has.No.Member(expected));
        }

        [Test]
        public void AddsToAscendancyPassivePointsIfAscendancyNode()
        {
            var definition = CreateNode(true, 0, true);
            var expected =
                CreateConditionalModifier(definition, "AscendancyPassivePoints", Form.BaseAdd, 1);
            var sut = CreateSut(definition);

            var result = sut.Parse(definition.Id);

            Assert.That(result.Modifiers, Has.Member(expected));
        }

        [Test]
        public void AddsPassivePointsGrantedToPassivePointsMaximum()
        {
            var definition = CreateNode(false, 3, true);
            var expected = CreateConditionalModifier(definition, "PassivePoints.Maximum", Form.BaseAdd, 3);
            var sut = CreateSut(definition);

            var result = sut.Parse(definition.Id);

            Assert.That(result.Modifiers, Has.Member(expected));
        }

        [Test]
        public void SetsNodeSkilledToFalse()
        {
            var definition = CreateNode();
            var expected = CreateModifier($"{definition.Id}.Skilled", Form.BaseSet, (NodeValue?) false,
                CreateGlobalSource(definition));
            var sut = CreateSut(definition);

            var result = sut.Parse(definition.Id);

            Assert.That(result.Modifiers, Has.Member(expected));
        }

        [TestCase("Strength")]
        [TestCase("Dexterity")]
        [TestCase("Intelligence")]
        public void SetsUpAttributeProperties(string attribute)
        {
            var definition = CreateNode();
            var expected = new[]
            {
                CreateConditionalModifier(definition, $"{attribute}", Form.BaseSet,
                    $"Character.{definition.Id}.{attribute}.Value(Total, Global)"),
                CreateModifier($"{definition.Id}.{attribute}", Form.BaseSet,
                    new StatValue(new Stat($"{definition.Id}.{attribute}.Base")),
                    CreateGlobalSource(definition)),
            };
            var sut = CreateSut(definition);

            var result = sut.Parse(definition.Id);

            result.Modifiers.Should().Contain(expected);
        }

        [TestCase("Strength")]
        [TestCase("Intelligence")]
        [TestCase("Strength", "Dexterity")]
        public void ParsesPropertyModifiersCorrectly(params string[] attributes)
        {
            var modifier = "+1 to " + attributes.ToDelimitedString(" and ");
            var definition = CreateNode(modifier);
            var source = CreateGlobalSource(definition);
            var expected = attributes
                .Select(a => CreateModifier(a, Form.BaseAdd, 1, source))
                .ToList();
            var coreParser = Mock.Of<ICoreParser>(p =>
                p.Parse(new CoreParserParameter(modifier + " (AsPassiveNodeBaseProperty)", source, Entity.Character))
                == ParseResult.Success(expected));
            var sut = CreateSut(definition, coreParser);

            var result = sut.Parse(definition.Id);

            result.Modifiers.Should().Contain(expected);
        }

        [Test]
        public void SetsEffectivenessToSkilled()
        {
            var definition = CreateNode();
            var expected = CreateModifier($"{definition.Id}.Effectiveness", Form.BaseSet,
                new StatValue(new Stat($"{definition.Id}.Skilled")),
                CreateGlobalSource(definition));
            var sut = CreateSut(definition);

            var result = sut.Parse(definition.Id);

            Assert.That(result.Modifiers, Has.Member(expected));
        }

        private static PassiveNodeParser CreateSut(PassiveNodeDefinition nodeDefinition, ICoreParser coreParser = null)
        {
            coreParser = coreParser ?? Mock.Of<ICoreParser>();
            var treeDefinition = new PassiveTreeDefinition(new[] { nodeDefinition });
            return new PassiveNodeParser(treeDefinition, CreateBuilderFactories(), coreParser);
        }

        private static PassiveNodeDefinition CreateNode(params string[] modifiers)
            => CreateNode(false, 0, true, modifiers);

        private static PassiveNodeDefinition CreateNode(
            bool isAscendancyNode, int passivePointsGranted, bool costsPassivePoint, params string[] modifiers)
            => new PassiveNodeDefinition(42, PassiveNodeType.Normal, "node", isAscendancyNode, costsPassivePoint,
                passivePointsGranted, default, modifiers);

        private static Modifier CreateConditionalModifier(
            PassiveNodeDefinition nodeDefinition, string stat, Form form, double value)
            => CreateConditionalModifier(nodeDefinition, stat, form, $"{value}");

        private static Modifier CreateConditionalModifier(
            PassiveNodeDefinition nodeDefinition, string stat, Form form, string value)
            => CreateModifier(stat, form, new FunctionalValue(null,
                    $"{value} * Character.{nodeDefinition.Id}.Effectiveness.Value(Total, Global)"),
                CreateGlobalSource(nodeDefinition));

        private static ModifierSource.Global CreateGlobalSource(PassiveNodeDefinition nodeDefinition)
            => new ModifierSource.Global(CreateLocalSource(nodeDefinition));

        private static ModifierSource.Local.PassiveNode CreateLocalSource(PassiveNodeDefinition nodeDefinition)
            => new ModifierSource.Local.PassiveNode(nodeDefinition.Id, nodeDefinition.Name);
    }
}