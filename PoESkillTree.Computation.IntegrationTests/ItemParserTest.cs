using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Moq;
using NUnit.Framework;
using PoESkillTree.Computation.Builders.Stats;
using PoESkillTree.Computation.Common;
using PoESkillTree.Computation.Parsing;
using PoESkillTree.GameModel;
using PoESkillTree.GameModel.Items;
using PoESkillTree.GameModel.Modifiers;
using PoESkillTree.GameModel.StatTranslation;
using static PoESkillTree.Computation.IntegrationTests.ParsingTestUtils;

namespace PoESkillTree.Computation.IntegrationTests
{
    [TestFixture]
    public class ItemParserTest : CompositionRootTestBase
    {
        private static Task<XmlUniqueList> _uniqueDefinitionsTask;

        private ModifierDefinitions _modifierDefinitions;
        private BaseItemDefinitions _baseItemDefinitions;
        private IStatTranslator _statTranslator;
        private IParser _parser;

        [OneTimeSetUp]
        public static void OneTimeSetUp()
        {
            _uniqueDefinitionsTask = DataUtils.LoadXmlAsync<XmlUniqueList>("Equipment.Uniques.xml");
        }

        [SetUp]
        public async Task SetUpAsync()
        {
            _modifierDefinitions = await GameData.Modifiers.ConfigureAwait(false);
            _baseItemDefinitions = await GameData.BaseItems.ConfigureAwait(false);
            _statTranslator = (await GameData.StatTranslators.ConfigureAwait(false))[StatTranslationFileNames.Main];
            _parser = await ParserTask.ConfigureAwait(false);
        }

        [Test]
        public void ParseRareAstralPlateReturnsCorrectResult()
        {
            var mods = new[]
            {
                "+1% to Fire Resistance", "+50 to maximum Life", "+32 to Strength", "10% increased Armour"
            };
            var item = new Item("Metadata/Items/Armours/BodyArmours/BodyStr15",
                "Hypnotic Keep Astral Plate", 20, 62, FrameType.Rare, false, mods, true);
            var definition = _baseItemDefinitions.GetBaseItemById(item.BaseMetadataId);
            var local = new ModifierSource.Local.Item(ItemSlot.BodyArmour, item.Name);
            var global = new ModifierSource.Global(local);
            var armourPropertyStat = new Stat("BodyArmour.Armour");
            var valueCalculationContext = Mock.Of<IValueCalculationContext>(c =>
                c.GetValue(armourPropertyStat, NodeType.Total, PathDefinition.MainPath) == (NodeValue?) 5);
            var expectedModifiers =
                new (string stat, Form form, double? value, ModifierSource source)[]
                {
                    ("BodyArmour.ItemTags", Form.TotalOverride, definition.Tags.EncodeAsDouble(), global),
                    ("BodyArmour.ItemClass", Form.TotalOverride, (double) definition.ItemClass, global),
                    ("BodyArmour.ItemFrameType", Form.TotalOverride, (double) FrameType.Rare, global),
                    ("Armour", Form.BaseSet, 5, local),
                    ("Evasion", Form.BaseSet, null, local),
                    ("EnergyShield", Form.BaseSet, null, local),
                    ("MovementSpeed", Form.BaseSet, null, local),
                    ("Level.Required", Form.BaseSet, null, local),
                    ("Dexterity.Required", Form.BaseSet, null, local),
                    ("Intelligence.Required", Form.BaseSet, null, local),
                    ("Strength.Required", Form.BaseSet, null, local),
                    ("BodyArmour.Armour", Form.BaseSet, definition.Properties[0].Value, local),
                    ("BodyArmour.MovementSpeed", Form.BaseSet, definition.Properties[1].Value, local),
                    ("BodyArmour.Armour", Form.Increase, 20, local),
                    ("BodyArmour.Evasion", Form.Increase, 20, local),
                    ("BodyArmour.EnergyShield", Form.Increase, 20, local),
                    ("BodyArmour.Level.Required", Form.BaseSet, 62, local),
                    ("BodyArmour.Strength.Required", Form.BaseSet, definition.Requirements.Strength, local),
                    ("Fire.Resistance", Form.BaseAdd, 1, global),
                    ("Life", Form.BaseAdd, 50, global),
                    ("Strength", Form.BaseAdd, 32, global),
                    ("BodyArmour.Armour", Form.Increase, 10, local),
                }.Select(t => (t.stat, t.form, (NodeValue?) t.value, t.source)).ToArray();

            var actual = _parser.ParseItem(item, ItemSlot.BodyArmour);

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
                "Some Corsair Sword", 20, 58, FrameType.Rare, false, mods, true);
            var definition = _baseItemDefinitions.GetBaseItemById(item.BaseMetadataId);
            var baseDamageValue = new NodeValue(definition.Properties[3].Value, definition.Properties[2].Value);
            var local = new ModifierSource.Local.Item(ItemSlot.MainHand, item.Name);
            var global = new ModifierSource.Global(local);
            var expectedModifiers =
                new (string stat, Form form, double? value, ModifierSource source)[]
                {
                    ("MainHand.ItemTags", Form.TotalOverride, definition.Tags.EncodeAsDouble(), global),
                    ("MainHand.ItemClass", Form.TotalOverride, (double) definition.ItemClass, global),
                    ("MainHand.ItemFrameType", Form.TotalOverride, (double) FrameType.Rare, global),
                    ("CriticalStrike.Chance.Attack.MainHand.Skill", Form.BaseSet, null, local),
                    ("CastRate.Attack.MainHand.Skill", Form.BaseSet, null, local),
                    ("MainHand.CastRate.Attack.MainHand.Skill", Form.BaseSet, null, local),
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
                    ("BaseCastTime.Attack.MainHand.Skill", Form.BaseSet,
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
                    ("MainHand.Physical.Damage.Attack.OffHand.Skill", Form.BaseAdd, 20, local),
                    ("MainHand.Physical.Damage.Spell.Skill", Form.BaseAdd, 20, local),
                    ("MainHand.Physical.Damage.Secondary.Skill", Form.BaseAdd, 20, local),
                    ("MainHand.CastRate.Attack.MainHand.Skill", Form.Increase, 20, local),
                    ("MainHand.CastRate.Attack.OffHand.Skill", Form.Increase, 20, local),
                    ("Accuracy.Attack.MainHand.Skill", Form.BaseAdd, 10, local),
                    ("Accuracy.Attack.OffHand.Skill", Form.BaseAdd, null, local),
                    ("Dexterity", Form.BaseAdd, 42, global),
                }.Select(
                    t => t.stat == "base phys"
                        ? ("MainHand.Physical.Damage.Attack.MainHand.Skill", Form.BaseSet, baseDamageValue, local)
                        : (t.stat, t.form, (NodeValue?) t.value, t.source)
                ).ToArray();

            var actual = _parser.ParseItem(item, ItemSlot.MainHand);

            AssertCorrectModifiers(Mock.Of<IValueCalculationContext>(), expectedModifiers, actual);
        }

        [TestCaseSource(nameof(ReadParseableBaseItems))]
        public void BaseItemIsParsedSuccessfully(string metadataId)
        {
            var actual = Parse(metadataId);

            AssertIsParsedSuccessfully(actual, NotParseableStatLines.Value);
        }

        private ParseResult Parse(string metadataId)
        {
            var definition = _baseItemDefinitions.GetBaseItemById(metadataId);
            var mods = Translate(definition.ImplicitModifiers, _statTranslator);
            var item = new Item(metadataId, definition.Name, 20, definition.Requirements.Level,
                FrameType.White, false, mods, true);
            var slot = SlotForClass(definition.ItemClass);
            return _parser.ParseItem(item, slot);
        }

        private static IEnumerable<string> ReadParseableBaseItems()
            => ReadDataLines("ParseableBaseItems");

        [TestCaseSource(nameof(ReadParseableUniqueItems))]
        public async Task UniqueItemIsParsedSuccessfully(string uniqueName)
        {
            var uniqueDefinitions = await _uniqueDefinitionsTask.ConfigureAwait(false);
            var uniques = uniqueDefinitions.Uniques.Where(u => u.Name == uniqueName).ToList();
            var unique = uniques.First(u => u.Name == uniqueName);
            var explicitMods = uniques
                .SelectMany(u => u.Explicit)
                .SelectMany(m => _modifierDefinitions.GetModifierById(m).Stats);

            var actual = Parse(unique, explicitMods);

            AssertIsParsedSuccessfully(actual, NotParseableStatLines.Value);
        }

        private ParseResult Parse(XmlUnique unique, IEnumerable<CraftableStat> explicitMods)
        {
            var definition = _baseItemDefinitions.GetBaseItemById(unique.BaseMetadataId);
            var craftableStats = definition.ImplicitModifiers.Concat(explicitMods);
            var mods = Translate(craftableStats, _statTranslator);
            var item = new Item(unique.BaseMetadataId, unique.Name, 20, unique.Level,
                FrameType.Unique, false, mods, true);
            var slot = SlotForClass(definition.ItemClass);
            return _parser.ParseItem(item, slot);
        }

        private static IEnumerable<string> ReadParseableUniqueItems()
            => ReadDataLines("ParseableUniqueItems");

        private static ItemSlot SlotForClass(ItemClass itemClass)
        {
            var slot = itemClass.ItemSlots();
            if (slot.HasFlag(ItemSlot.MainHand))
                slot = ItemSlot.MainHand;
            else if (slot.HasFlag(ItemSlot.Ring))
                slot = ItemSlot.Ring;
            return slot;
        }
    }
}