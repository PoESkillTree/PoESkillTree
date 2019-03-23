using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using PoESkillTree.Computation.Common;
using PoESkillTree.GameModel;
using PoESkillTree.GameModel.Items;
using PoESkillTree.GameModel.PassiveTree;
using static PoESkillTree.Computation.Parsing.ParserTestUtils;

namespace PoESkillTree.Computation.Parsing.JewelParsers
{
    [TestFixture]
    public class JewelInSkillTreeParserTest
    {
        [TestCase("+42 to maximum Life")]
        [TestCase("Adds 14 to 21 Cold Damage to Wand Attacks")]
        public void ParseReturnsCorrectModifier(string modifier)
        {
            var parserParam = CreateItem(modifier);
            var source = CreateGlobalSource(parserParam);
            var expected = CreateModifier("", Form.BaseAdd, 2, source);
            var coreParser = Mock.Of<ICoreParser>(p =>
                p.Parse(new CoreParserParameter(modifier, source, Entity.Character))
                == ParseResult.Success(new[] { expected }));
            var sut = CreateSut(coreParser);

            var result = sut.Parse(parserParam);

            result.Modifiers.Should().Contain(expected);
        }

        [Test]
        public void ParseReturnsEmptyResultForDisabledItem()
        {
            var parserParam = CreateItem("+1 to Strength", isEnabled: false);
            var sut = CreateSut();

            var result = sut.Parse(parserParam);

            result.Modifiers.Should().BeEmpty();
        }

        [TestCase("at least 24 Strength", JewelRadius.Medium, 14)]
        [TestCase("at least 24 strength", JewelRadius.Medium, 14)]
        [TestCase("at least 23 dexterity", JewelRadius.Small, 14)]
        [TestCase("at least 17 intelligence", JewelRadius.Small, 14)]
        [TestCase("40 total intelligence and dexterity", JewelRadius.Small, 14)]
        [TestCase("17 intelligence", JewelRadius.Small, 14)]
        public void ParseReturnsCorrectModifierIfThresholdIsMet(string modifier, JewelRadius radius, int nodeId)
        {
            var parserParam = CreateItem($"With {modifier} in Radius, +1 to A", radius, (ushort) nodeId);
            var source = CreateGlobalSource(parserParam);
            var expected = CreateModifier("", Form.BaseAdd, 2, source);
            var coreParser = Mock.Of<ICoreParser>(p =>
                p.Parse(new CoreParserParameter("+1 to A", source, Entity.Character))
                == ParseResult.Success(new[] { expected }));
            var sut = CreateSut(coreParser);

            var result = sut.Parse(parserParam);

            result.Modifiers.Should().Contain(expected);
        }
        
        [TestCase("at least 25 Strength", JewelRadius.Medium, 14)]
        [TestCase("at least 24 Strength", JewelRadius.Small, 14)]
        [TestCase("at least 24 Strength", JewelRadius.Medium, 0)]
        [TestCase("at least 24 dexterity", JewelRadius.Small, 14)]
        [TestCase("at least 18 intelligence", JewelRadius.Small, 14)]
        [TestCase("41 total intelligence and dexterity", JewelRadius.Small, 14)]
        public void ParseReturnsNoModifierIfThresholdIsNotMet(string modifier, JewelRadius radius, int nodeId)
        {
            var parserParam = CreateItem($"With {modifier} in Radius, +1 to A", radius, (ushort) nodeId);
            var coreParser = Mock.Of<ICoreParser>();
            var sut = CreateSut(coreParser);

            var result = sut.Parse(parserParam);

            result.Modifiers.Should().BeEmpty();
        }

        [Test]
        public void ParseRespectsNewlines()
        {
            var parserParam = CreateItem("With at least 1 strength in Radius, +1 to\nA", JewelRadius.Small, 0);
            var source = CreateGlobalSource(parserParam);
            var coreParser = Mock.Of<ICoreParser>(p =>
                p.Parse(new CoreParserParameter("+1 to\nA", source, Entity.Character))
                == ParseResult.Success(new[] { CreateModifier("", default, 2, source) }));
            var sut = CreateSut(coreParser);

            var result = sut.Parse(parserParam);

            result.Modifiers.Should().NotBeEmpty();
        }

        private static JewelInSkillTreeParser CreateSut(ICoreParser coreParser = null)
            => new JewelInSkillTreeParser(CreateThresholdTree(), coreParser ?? Mock.Of<ICoreParser>());

        private static JewelInSkillTreeParserParameter CreateItem(
            string mod, JewelRadius radius = JewelRadius.None, ushort nodeId = 0, bool isEnabled = true)
        {
            var item = new Item("metadataId", "itemName", 0, 0, default,
                false, new[] { mod }, isEnabled);
            return new JewelInSkillTreeParserParameter(item, radius, nodeId);
        }

        private static ModifierSource.Global CreateGlobalSource(JewelInSkillTreeParserParameter parserParam)
            => new ModifierSource.Global(CreateLocalSource(parserParam));

        private static ModifierSource.Local.Jewel CreateLocalSource(JewelInSkillTreeParserParameter parserParam)
            => new ModifierSource.Local.Jewel(parserParam.JewelRadius, parserParam.PassiveNodeId, parserParam.Item.Name);

        private static PassiveTreeDefinition CreateThresholdTree()
            => new PassiveTreeDefinition(CreateThresholdNodes().ToList());

        private static IEnumerable<PassiveNodeDefinition> CreateThresholdNodes()
        {
            string[] attributes =
            {
                "strength",
                "dexterity",
                "intelligence",
                "Strength and dexterity",
                "dexterity and Intelligence",
            };

            for (ushort x = 0; x < 5; x++)
            {
                for (ushort y = 0; y < 5; y++)
                {
                    var modifier = $"+{x + 1} to {attributes[y % attributes.Length]}";
                    yield return CreateNode((ushort) (x + y * 5), new NodePosition(x * 400, y * 400), modifier);
                }
            }
        }

        private static PassiveNodeDefinition CreateNode(ushort id, NodePosition position, string modifier)
            => new PassiveNodeDefinition(id, default, "", false, false,
                0, position, new[] { modifier });
    }
}