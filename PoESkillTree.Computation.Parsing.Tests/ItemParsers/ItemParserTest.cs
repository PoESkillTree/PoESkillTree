using System.Collections.Generic;
using Moq;
using NUnit.Framework;
using PoESkillTree.Computation.Builders;
using PoESkillTree.Computation.Builders.Stats;
using PoESkillTree.Computation.Common;
using PoESkillTree.Computation.Parsing.ItemParsers;
using PoESkillTree.GameModel;
using PoESkillTree.GameModel.Items;
using PoESkillTree.GameModel.Modifiers;
using PoESkillTree.GameModel.Skills;
using static PoESkillTree.Computation.Parsing.Tests.ParserTestUtils;

namespace PoESkillTree.Computation.Parsing.Tests.ItemParsers
{
    [TestFixture]
    public class ItemParserTest
    {
        [Test]
        public void ParseReturnsCorrectResultForGlobalModifier()
        {
            var parserParam = CreateItem(ItemSlot.BodyArmour, "+42 to maximum Life");
            var source = CreateGlobalSource(parserParam);
            var baseItemDefinition = CreateBaseItemDefinition(parserParam.Item, default);
            var expected = CreateModifier("", Form.BaseAdd, 2, source);
            var coreParser = Mock.Of<ICoreParser>(p =>
                p.Parse(new CoreParserParameter("+42 to maximum Life", source, Entity.Character))
                == ParseResult.Success(new[] { expected }));
            var sut = CreateSut(baseItemDefinition, coreParser);

            var result = sut.Parse(parserParam);

            Assert.That(result.Modifiers, Has.Member(expected));
        }

        [TestCase(Tags.BodyArmour)]
        [TestCase(Tags.BodyArmour | Tags.Armour | Tags.StrArmour)]
        public void ParseReturnsCorrectItemTagsModifier(Tags tags)
        {
            var parserParam = CreateItem(ItemSlot.BodyArmour);
            var baseItemDefinition = CreateBaseItemDefinition(parserParam.Item, ItemClass.BodyArmour, tags);
            var expected = CreateModifier($"{parserParam.ItemSlot}.ItemTags", Form.BaseSet, tags.EncodeAsDouble());
            var sut = CreateSut(baseItemDefinition);

            var result = sut.Parse(parserParam);

            Assert.That(result.Modifiers, Has.Member(expected));
        }

        [TestCase(ItemClass.BodyArmour)]
        [TestCase(ItemClass.Belt)]
        public void ParseReturnsCorrectItemClassModifier(ItemClass itemClass)
        {
            var parserParam = CreateItem(ItemSlot.BodyArmour);
            var baseItemDefinition = CreateBaseItemDefinition(parserParam.Item, itemClass);
            var expected = CreateModifier($"{parserParam.ItemSlot}.ItemClass", Form.BaseSet, (double) itemClass);
            var sut = CreateSut(baseItemDefinition);

            var result = sut.Parse(parserParam);

            Assert.That(result.Modifiers, Has.Member(expected));
        }

        [TestCase(FrameType.Rare)]
        [TestCase(FrameType.Magic)]
        public void ParseReturnsCorrectFrameTypeModifier(FrameType frameType)
        {
            var parserParam = CreateItem(ItemSlot.BodyArmour, frameType: frameType);
            var baseItemDefinition = CreateBaseItemDefinition(parserParam.Item, ItemClass.BodyArmour);
            var expected = CreateModifier($"{parserParam.ItemSlot}.ItemFrameType", Form.BaseSet, (double) frameType);
            var sut = CreateSut(baseItemDefinition);

            var result = sut.Parse(parserParam);

            Assert.That(result.Modifiers, Has.Member(expected));
        }

        [Test]
        public void ParseReturnsCorrectCorruptedModifierIfCorrupted()
        {
            var parserParam = CreateItem(ItemSlot.BodyArmour, isCorrupted: true);
            var baseItemDefinition = CreateBaseItemDefinition(parserParam.Item, ItemClass.BodyArmour);
            var expected = CreateModifier($"{parserParam.ItemSlot}.ItemIsCorrupted", Form.BaseSet, 1);
            var sut = CreateSut(baseItemDefinition);

            var result = sut.Parse(parserParam);

            Assert.That(result.Modifiers, Has.Member(expected));
        }

        [Test]
        public void ParseReturnsNoCorruptedModifierIfNotCorrupted()
        {
            var parserParam = CreateItem(ItemSlot.BodyArmour);
            var baseItemDefinition = CreateBaseItemDefinition(parserParam.Item, ItemClass.BodyArmour);
            var sut = CreateSut(baseItemDefinition);

            var result = sut.Parse(parserParam);

            Assert.IsFalse(AnyModifierHasIdentity(result.Modifiers, $"{parserParam.ItemSlot}.ItemIsCorrupted"));
        }

        [Test]
        public void ParseReturnsCorrectRequirementModifiers()
        {
            var parserParam = CreateItem(ItemSlot.BodyArmour, requiredLevel: 42);
            var baseItemDefinition = CreateBaseItemDefinition(parserParam.Item, ItemClass.BodyArmour,
                requirements: new Requirements(4, 5, 6, 7));
            var source = new ModifierSource.Local.Item(parserParam.ItemSlot, parserParam.Item.Name);
            var expected = new[]
            {
                CreateModifier("Level.Required", Form.BaseSet, 42, source),
                CreateModifier("Dexterity.Required", Form.BaseSet, 5, source),
                CreateModifier("Intelligence.Required", Form.BaseSet, 6, source),
                CreateModifier("Strength.Required", Form.BaseSet, 7, source),
            };
            var sut = CreateSut(baseItemDefinition);

            var result = sut.Parse(parserParam);

            Assert.That(result.Modifiers, Is.SupersetOf(expected));
        }

        private static ItemParser CreateSut(BaseItemDefinition baseItemDefinition)
            => CreateSut(baseItemDefinition, Mock.Of<ICoreParser>());

        private static ItemParser CreateSut(BaseItemDefinition baseItemDefinition, ICoreParser coreParser)
        {
            var baseItemDefinitions = new BaseItemDefinitions(new[] { baseItemDefinition });
            var builderFactories =
                new BuilderFactories(new StatFactory(), new SkillDefinitions(new SkillDefinition[0]));
            return new ItemParser(baseItemDefinitions, builderFactories, coreParser);
        }

        private static ItemParserParameter CreateItem(ItemSlot itemSlot, params string[] mods)
            => CreateItem(itemSlot, 0, 0, FrameType.Rare, false, mods);

        private static ItemParserParameter CreateItem(
            ItemSlot itemSlot, int quality = 0, int requiredLevel = 0, FrameType frameType = FrameType.Rare,
            bool isCorrupted = false)
            => CreateItem(itemSlot, quality, requiredLevel, frameType, isCorrupted, new string[0]);

        private static ItemParserParameter CreateItem(
            ItemSlot itemSlot, int quality, int requiredLevel, FrameType frameType, bool isCorrupted,
            params string[] mods)
        {
            var modDict = new Dictionary<ModLocation, IReadOnlyList<string>>
            {
                { ModLocation.Explicit, mods }
            };
            var item =
                new Item("metadataId", "itemName", quality, requiredLevel, frameType, isCorrupted, modDict);
            return new ItemParserParameter(item, itemSlot);
        }

        private static BaseItemDefinition CreateBaseItemDefinition(Item item, ItemClass itemClass, Tags tags = default,
            Requirements requirements = null)
            => new BaseItemDefinition(item.BaseMetadataId, "", itemClass, new string[0], tags, null,
                null, requirements ?? new Requirements(0, 0, 0, 0),
                null, 0, 0, 0, default, "");

        private static ModifierSource.Global CreateGlobalSource(ItemParserParameter parserParam)
            => new ModifierSource.Global(CreateLocalSource(parserParam));

        private static ModifierSource.Local.Item CreateLocalSource(ItemParserParameter parserParam)
            => new ModifierSource.Local.Item(parserParam.ItemSlot, parserParam.Item.Name);
    }
}