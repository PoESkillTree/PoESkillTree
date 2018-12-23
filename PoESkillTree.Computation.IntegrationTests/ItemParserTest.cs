using System.Collections.Generic;
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
            var definitionsTask = BaseItemJsonDeserializer.DeserializeAsync();
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
                    ("BodyArmour.Armour", Form.BaseSet, definition.Properties[0].Value, local),
                    ("Armour", Form.BaseSet, 5, local),
                    ("BodyArmour.Armour", Form.Increase, 20, local),
                    ("BodyArmour.Evasion", Form.Increase, 20, local),
                    ("BodyArmour.EnergyShield", Form.Increase, 20, local),
                    ("Level.Required", Form.BaseSet, 62, local),
                    ("Strength.Required", Form.BaseSet, definition.Requirements.Strength, local),
                    ("Armour", Form.Increase, 10, local),
                    ("MovementSpeed", Form.Increase, -5, global),
                    ("Fire.Resistance", Form.BaseAdd, 1, global),
                    ("Life", Form.BaseAdd, 50, global),
                    ("Strength", Form.BaseAdd, 32, global),
                }.Select(t => (t.stat, t.form, (NodeValue?) t.value, t.source)).ToArray();

            var actual = _itemParser.Parse(new ItemParserParameter(item, ItemSlot.BodyArmour));

            AssertCorrectModifiers(valueCalculationContext, expectedModifiers, actual);
        }

        private static void AssertCorrectModifiers(
            IValueCalculationContext context,
            (string stat, Form form, NodeValue? value, ModifierSource source)[] expectedModifiers,
            ParseResult result)
        {
            var (failedLines, remainingSubstrings, modifiers) = result;

            Assert.IsEmpty(failedLines);
            Assert.IsEmpty(remainingSubstrings);
            Assert.AreEqual(expectedModifiers.Length, modifiers.Count);
            for (var i = 0; i < modifiers.Count; i++)
            {
                var expected = expectedModifiers[i];
                var actual = modifiers[i];
                Assert.AreEqual(expected.stat, actual.Stats[0].Identity);
                Assert.AreEqual(Entity.Character, actual.Stats[0].Entity);
                Assert.AreEqual(expected.form, actual.Form);
                Assert.AreEqual(expected.source, actual.Source);

                var expectedValue = expected.value;
                var actualValue = actual.Value.Calculate(context);
                Assert.AreEqual(expectedValue, actualValue);
            }
        }
    }
}