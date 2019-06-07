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

        private static JewelInSkillTreeParser CreateSut(ICoreParser coreParser = null)
            => new JewelInSkillTreeParser(
                new PassiveTreeDefinition(new[] { CreateNode(0) }),
                CreateBuilderFactories(),
                coreParser ?? Mock.Of<ICoreParser>());

        private static JewelInSkillTreeParserParameter CreateItem(string mod, bool isEnabled = true)
        {
            var item = new Item("metadataId", "itemName", 0, 0, default,
                false, new[] { mod }, isEnabled);
            return new JewelInSkillTreeParserParameter(item, default, 0);
        }

        private static ModifierSource.Global CreateGlobalSource(JewelInSkillTreeParserParameter parserParam)
            => new ModifierSource.Global(CreateLocalSource(parserParam));

        private static ModifierSource.Local.Jewel CreateLocalSource(JewelInSkillTreeParserParameter parserParam)
            => new ModifierSource.Local.Jewel(parserParam.JewelRadius, parserParam.PassiveNodeId, parserParam.Item.Name);

        private static PassiveNodeDefinition CreateNode(ushort id)
            => new PassiveNodeDefinition(id, default, "", false, false,
                0, default, new string[0]);
    }
}