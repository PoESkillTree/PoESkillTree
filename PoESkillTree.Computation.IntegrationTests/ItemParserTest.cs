using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Moq;
using NUnit.Framework;
using PoESkillTree.Computation.Common;
using PoESkillTree.Computation.Parsing;
using PoESkillTree.Computation.Parsing.ItemParsers;
using PoESkillTree.GameModel;
using PoESkillTree.GameModel.Items;
using PoESkillTree.GameModel.Modifiers;

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
            _baseItemDefinitions = await BaseItemJsonDeserializer.DeserializeAsync();
            _itemParser = new ItemParser(await CompositionRoot.CoreParser);
        }

        [Test]
        public void ParseRareAstralPlateReturnsCorrectResult()
        {
            var modDict = new Dictionary<ModLocation, IReadOnlyList<string>>
            {
                [ModLocation.Implicit] =
                    new[] { "5% reduced Movement Speed (Hidden)", "+11% to all Elemental Resistances" },
                [ModLocation.Corruption] = new[] { "+1 to maximum Mana" },
                [ModLocation.Enchantment] = new[] { "+1% to Fire Resistance" },
                [ModLocation.Explicit] = new[] { "+50 to maximum Life", "+32 to Strength" },
                [ModLocation.Crafted] = new[] { "6% increased maximum Life" }
            };
            var item = new Item("Metadata/Items/Armours/BodyArmours/BodyStr15",
                "Hypnotic Keep Astral Plate", 20, 62, modDict);
            var definition = _baseItemDefinitions.GetBaseItemById(item.BaseMetadataId);
            var local = new ModifierSource.Local.Item(ItemSlot.BodyArmour, item.Name);
            var global = new ModifierSource.Global(local);
            var valueCalculationContextMock = new Mock<IValueCalculationContext>();
            var expectedModifiers =
                new (string stat, Form form, double? value, ModifierSource source)[]
                {
                    ("MovementSpeed", Form.Increase, -5, global),
                    ("Fire.Resistance", Form.BaseAdd, 11, global),
                    ("Mana", Form.BaseAdd, 1, global),
                    ("Fire.Resistance", Form.BaseAdd, 1, global),
                    ("Life", Form.BaseAdd, 50, global),
                    ("Strength", Form.BaseAdd, 32, global),
                    ("Life", Form.Increase, 6, global),
                }.Select(t => (t.stat, t.form, (NodeValue?) t.value, t.source)).ToArray();

            var actual = _itemParser.Parse(new ItemParserParameter(item, ItemSlot.BodyArmour));

            AssertCorrectModifiers(valueCalculationContextMock.Object, expectedModifiers, actual);
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