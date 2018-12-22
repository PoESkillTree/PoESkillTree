using System.Collections.Generic;
using System.Linq;
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
using PoESkillTree.GameModel.StatTranslation;
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

        [Test]
        public void ParseReturnsCorrectBuffModifiers()
        {
            var parserParam = CreateItem(ItemSlot.BodyArmour);
            var baseItemDefinition = CreateBaseItemDefinition(parserParam.Item, ItemClass.UtilityFlask, Tags.Flask,
                buffStats: new[]
                {
                    new UntranslatedStat("base_cold_damage_resistance_%", 50),
                    new UntranslatedStat("utility_flask_cold_damage_taken_+%_final", -20),
                });
            var translatorResult = new StatTranslatorResult(
                new[] { "cold resistance", "cold damage taken" }, new UntranslatedStat[0]);
            var translator = Mock.Of<IStatTranslator>(t =>
                t.Translate(baseItemDefinition.BuffStats) == translatorResult);
            var parserParameters = translatorResult.TranslatedStats
                .Select(r => new CoreParserParameter(r, new ModifierSource.Global(), Entity.Character))
                .ToList();
            var modifiers = new[]
            {
                CreateModifier("Cold.Resistance", Form.BaseAdd, 50),
                CreateModifier("Cold.Damage.Attack.MainHand.Skill.Taken", Form.More, -20),
            };
            var coreParser = Mock.Of<ICoreParser>(p =>
                p.Parse(parserParameters[0]) == ParseResult.Success(new[] { modifiers[0] }) &&
                p.Parse(parserParameters[1]) == ParseResult.Success(new[] { modifiers[1] }));
            var flaskEffect = 2;
            var flaskEffectStat = new Stat("Flask.Effect");
            var context = Mock.Of<IValueCalculationContext>(c =>
                c.GetValue(flaskEffectStat, NodeType.Total, PathDefinition.MainPath) == (NodeValue?) flaskEffect);
            var sut = CreateSut(baseItemDefinition, coreParser, translator);

            var (_, _, actualModifiers) = sut.Parse(parserParam);

            foreach (var modifier in modifiers)
            {
                var identity = modifier.Stats[0].Identity;
                Assert.IsTrue(AnyModifierHasIdentity(actualModifiers, identity));
                var actual = GetFirstModifierWithIdentity(actualModifiers, identity);
                Assert.AreEqual(modifier.Form, actual.Form);
                Assert.AreEqual(modifier.Source, actual.Source);
                var expectedValue = modifier.Value.Calculate(context) * flaskEffect;
                Assert.AreEqual(expectedValue, actual.Value.Calculate(context));
            }
        }

        private static ItemParser CreateSut(
            BaseItemDefinition baseItemDefinition, ICoreParser coreParser = null, IStatTranslator statTranslator = null)
        {
            coreParser = coreParser ?? Mock.Of<ICoreParser>();

            var baseItemDefinitions = new BaseItemDefinitions(new[] { baseItemDefinition });
            var builderFactories =
                new BuilderFactories(new StatFactory(), new SkillDefinitions(new SkillDefinition[0]));
            return new ItemParser(baseItemDefinitions, builderFactories, coreParser, statTranslator);
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
            IReadOnlyList<UntranslatedStat> buffStats = null, Requirements requirements = null)
            => new BaseItemDefinition(item.BaseMetadataId, "", itemClass, new string[0], tags, null,
                buffStats ?? new UntranslatedStat[0],
                requirements ?? new Requirements(0, 0, 0, 0),
                null, 0, 0, 0, default, "");

        private static ModifierSource.Global CreateGlobalSource(ItemParserParameter parserParam)
            => new ModifierSource.Global(CreateLocalSource(parserParam));

        private static ModifierSource.Local.Item CreateLocalSource(ItemParserParameter parserParam)
            => new ModifierSource.Local.Item(parserParam.ItemSlot, parserParam.Item.Name);
    }
}