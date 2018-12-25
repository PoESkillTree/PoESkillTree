using System;
using System.Linq;
using System.Threading.Tasks;
using Moq;
using NUnit.Framework;
using PoESkillTree.Computation.Builders.Stats;
using PoESkillTree.Computation.Common;
using PoESkillTree.Computation.Parsing;
using PoESkillTree.Computation.Parsing.ItemParsers;
using PoESkillTree.GameModel;
using PoESkillTree.GameModel.Items;
using PoESkillTree.GameModel.StatTranslation;

namespace PoESkillTree.Computation.IntegrationTests
{
    [TestFixture]
    public class ItemParserTest : CompositionRootTestBase
    {
        private BaseItemDefinitions _baseItemDefinitions;
        private IParser<ItemParserParameter> _itemParser;

        [SetUp]
        public async Task SetUpAsync()
        {
            var definitionsTask = CompositionRoot.BaseItemDefinitions;
            var builderFactoriesTask = CompositionRoot.BuilderFactories;
            var coreParserTask = CompositionRoot.CoreParser;
            var statTranslatorTask = StatTranslationLoader.LoadAsync(StatTranslationLoader.MainFileName);
            _baseItemDefinitions = await definitionsTask.ConfigureAwait(false);
            _itemParser = new ItemParser(_baseItemDefinitions,
                await builderFactoriesTask.ConfigureAwait(false),
                await coreParserTask.ConfigureAwait(false),
                await statTranslatorTask.ConfigureAwait(false));
        }

        [Test]
        public void ParseRareAstralPlateReturnsCorrectResult()
        {
            var mods = new[]
            {
                "5% reduced Movement Speed (Hidden)",
                "+1% to Fire Resistance", "+50 to maximum Life", "+32 to Strength", "10% increased Armour"
            };
            var item = new Item("Metadata/Items/Armours/BodyArmours/BodyStr15",
                "Hypnotic Keep Astral Plate", 20, 62, FrameType.Rare, false, mods);
            var definition = _baseItemDefinitions.GetBaseItemById(item.BaseMetadataId);
            var local = new ModifierSource.Local.Item(ItemSlot.BodyArmour, item.Name);
            var global = new ModifierSource.Global(local);
            var armourPropertyStat = new Stat("BodyArmour.Armour");
            var valueCalculationContext = Mock.Of<IValueCalculationContext>(c =>
                c.GetValue(armourPropertyStat, NodeType.Total, PathDefinition.MainPath) == (NodeValue?) 5);
            var expectedModifiers =
                new (string stat, Form form, double? value, ModifierSource source)[]
                {
                    ("BodyArmour.ItemTags", Form.BaseSet, definition.Tags.EncodeAsDouble(), global),
                    ("BodyArmour.ItemClass", Form.BaseSet, (double) definition.ItemClass, global),
                    ("BodyArmour.ItemFrameType", Form.BaseSet, (double) FrameType.Rare, global),
                    ("Armour", Form.BaseSet, 5, local),
                    ("Evasion", Form.BaseSet, null, local),
                    ("EnergyShield", Form.BaseSet, null, local),
                    ("Level.Required", Form.BaseSet, null, local),
                    ("Dexterity.Required", Form.BaseSet, null, local),
                    ("Intelligence.Required", Form.BaseSet, null, local),
                    ("Strength.Required", Form.BaseSet, null, local),
                    ("BodyArmour.Armour", Form.BaseSet, definition.Properties[0].Value, local),
                    ("BodyArmour.Armour", Form.Increase, 20, local),
                    ("BodyArmour.Evasion", Form.Increase, 20, local),
                    ("BodyArmour.EnergyShield", Form.Increase, 20, local),
                    ("BodyArmour.Level.Required", Form.BaseSet, 62, local),
                    ("BodyArmour.Strength.Required", Form.BaseSet, definition.Requirements.Strength, local),
                    ("BodyArmour.Armour", Form.Increase, 10, local),
                    ("MovementSpeed", Form.Increase, -5, global),
                    ("Fire.Resistance", Form.BaseAdd, 1, global),
                    ("Life", Form.BaseAdd, 50, global),
                    ("Strength", Form.BaseAdd, 32, global),
                }.Select(t => (t.stat, t.form, (NodeValue?) t.value, t.source)).ToArray();

            var actual = _itemParser.Parse(new ItemParserParameter(item, ItemSlot.BodyArmour));

            AssertCorrectModifiers(valueCalculationContext, expectedModifiers, actual);
        }

        [Test]
        public void ParseRareCorsairSwordReturnsCorrectResult()
        {
            var mods = new[]
            {
                "Adds 20 to 20 Physical Damage", "20% increased Attack Speed", "+10 to Accuracy Rating",
                "+42 to Dexterity"
            };
            var item = new Item("Metadata/Items/Weapons/OneHandWeapons/OneHandSwords/OneHandSword17",
                "Some Corsair Sword", 20, 58, FrameType.Rare, false, mods);
            var definition = _baseItemDefinitions.GetBaseItemById(item.BaseMetadataId);
            var baseDamageValue = new NodeValue(definition.Properties[3].Value, definition.Properties[2].Value);
            var local = new ModifierSource.Local.Item(ItemSlot.MainHand, item.Name);
            var global = new ModifierSource.Global(local);
            var expectedModifiers =
                new (string stat, Form form, double? value, ModifierSource source)[]
                {
                    ("MainHand.ItemTags", Form.BaseSet, definition.Tags.EncodeAsDouble(), global),
                    ("MainHand.ItemClass", Form.BaseSet, (double) definition.ItemClass, global),
                    ("MainHand.ItemFrameType", Form.BaseSet, (double) FrameType.Rare, global),
                    ("CriticalStrike.Chance.Attack.MainHand.Skill", Form.BaseSet, null, local),
                    ("BaseCastTime.Attack.MainHand.Skill", Form.BaseSet, null, local),
                    ("Range.Attack.MainHand.Skill", Form.BaseSet, null, local),
                    ("Physical.Damage.Attack.MainHand.Skill", Form.BaseSet, null, local),
                    ("Lightning.Damage.Attack.MainHand.Skill", Form.BaseSet, null, local),
                    ("Cold.Damage.Attack.MainHand.Skill", Form.BaseSet, null, local),
                    ("Fire.Damage.Attack.MainHand.Skill", Form.BaseSet, null, local),
                    ("Chaos.Damage.Attack.MainHand.Skill", Form.BaseSet, null, local),
                    ("RandomElement.Damage.Attack.MainHand.Skill", Form.BaseSet, null, local),
                    ("Level.Required", Form.BaseSet, null, local),
                    ("Dexterity.Required", Form.BaseSet, null, local),
                    ("Intelligence.Required", Form.BaseSet, null, local),
                    ("Strength.Required", Form.BaseSet, null, local),
                    ("MainHand.BaseCastTime.Attack.MainHand.Skill", Form.BaseSet,
                        definition.Properties[0].Value / 1000D, local),
                    ("MainHand.CriticalStrike.Chance.Attack.MainHand.Skill", Form.BaseSet,
                        definition.Properties[1].Value / 100D, local),
                    ("MainHand.Range.Attack.MainHand.Skill", Form.BaseSet, definition.Properties[4].Value, local),
                    ("base phys", default, null, null),
                    ("MainHand.Physical.Damage.Attack.MainHand.Skill", Form.Increase, 20, local),
                    ("MainHand.Level.Required", Form.BaseSet, 58, local),
                    ("MainHand.Dexterity.Required", Form.BaseSet, definition.Requirements.Dexterity, local),
                    ("MainHand.Strength.Required", Form.BaseSet, definition.Requirements.Strength, local),
                    ("MainHand.Physical.Damage.Attack.MainHand.Skill", Form.BaseAdd, 20, local),
                    ("MainHand.Physical.Damage.Attack.OffHand.Skill", Form.BaseAdd, null, local),
                    ("MainHand.CastRate.Attack.MainHand.Skill", Form.Increase, 20, local),
                    ("MainHand.CastRate.Attack.OffHand.Skill", Form.Increase, null, local),
                    ("Accuracy.Attack.MainHand.Skill", Form.BaseAdd, 10, local),
                    ("Accuracy.Attack.OffHand.Skill", Form.BaseAdd, null, local),
                    ("Dexterity", Form.BaseAdd, 42, global),
                }.Select(
                    t => t.stat == "base phys"
                        ? ("MainHand.Physical.Damage.Attack.MainHand.Skill", Form.BaseSet, baseDamageValue, local)
                        : (t.stat, t.form, (NodeValue?) t.value, t.source)
                ).ToArray();

            var actual = _itemParser.Parse(new ItemParserParameter(item, ItemSlot.MainHand));

            AssertCorrectModifiers(Mock.Of<IValueCalculationContext>(), expectedModifiers, actual);
        }

        private static void AssertCorrectModifiers(
            IValueCalculationContext context,
            (string stat, Form form, NodeValue? value, ModifierSource source)[] expectedModifiers,
            ParseResult result)
        {
            var (failedLines, remainingSubstrings, modifiers) = result;

            Assert.IsEmpty(failedLines);
            Assert.IsEmpty(remainingSubstrings);
            for (var i = 0; i < Math.Min(modifiers.Count, expectedModifiers.Length); i++)
            {
                var expected = expectedModifiers[i];
                var actual = modifiers[i];
                Assert.AreEqual(expected.stat, actual.Stats[0].Identity);
                Assert.AreEqual(Entity.Character, actual.Stats[0].Entity, expected.stat);
                Assert.AreEqual(expected.form, actual.Form, expected.stat);
                Assert.AreEqual(expected.source, actual.Source, expected.stat);

                var expectedValue = expected.value;
                var actualValue = actual.Value.Calculate(context);
                Assert.AreEqual(expectedValue, actualValue, expected.stat);
            }
            Assert.AreEqual(expectedModifiers.Length, modifiers.Count);
        }
    }
}