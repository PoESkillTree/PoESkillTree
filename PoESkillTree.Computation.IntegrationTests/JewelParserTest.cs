using System.Linq;
using System.Threading.Tasks;
using Moq;
using NUnit.Framework;
using PoESkillTree.Computation.Common;
using PoESkillTree.Computation.Parsing;
using PoESkillTree.GameModel.Items;
using static PoESkillTree.Computation.IntegrationTests.ParsingTestUtils;

namespace PoESkillTree.Computation.IntegrationTests
{
    [TestFixture]
    public class JewelParserTest : CompositionRootTestBase
    {
        private IParser _parser;

        [SetUp]
        public async Task SetUpAsync()
        {
            _parser = await ParserTask.ConfigureAwait(false);
        }

        [Test]
        public void ParseRareJewelInBodyArmourReturnsCorrectResult()
        {
            var mods = new[]
            {
                "+120 to Armour",
                "+27 to maximum Life",
                "+13% to Fire Resistance",
            };
            var item = new Item("Metadata/Items/Jewels/JewelAbyssRanged",
                "Grim Arbiter Searching Eye Jewel", 0, 54, FrameType.Rare, false, mods, true);
            var local = new ModifierSource.Local.Item(ItemSlot.BodyArmour, item.Name);
            var global = new ModifierSource.Global(local);
            var valueCalculationContext = Mock.Of<IValueCalculationContext>();
            var expectedModifiers = new (string stat, Form form, double? value)[]
            {
                ("Armour", Form.BaseAdd, 120),
                ("Life", Form.BaseAdd, 27),
                ("Fire.Resistance", Form.BaseAdd, 13),
            }.Select(t => (t.stat, t.form, (NodeValue?) t.value, (ModifierSource) global)).ToArray();

            var actual = _parser.ParseJewelSocketedInItem(item, ItemSlot.BodyArmour);

            AssertCorrectModifiers(valueCalculationContext, expectedModifiers, actual);
        }

        [Test]
        public void ParseRareJewelInSkillTreeReturnsCorrectResult()
        {
            var mods = new[]
            {
                "4% increased maximum Energy Shield",
                "6% increased maximum Life",
                "+13 to Intelligence",
            };
            var item = new Item("Metadata/Items/Jewels/JewelInt",
                "Entropy Splinter Cobalt Jewel", 0, 0, FrameType.Rare, false, mods, true);
            var local = new ModifierSource.Local.Item(ItemSlot.SkillTree, item.Name);
            var global = new ModifierSource.Global(local);
            var valueCalculationContext = Mock.Of<IValueCalculationContext>();
            var expectedModifiers = new (string stat, Form form, double? value)[]
            {
                ("EnergyShield", Form.Increase, 4),
                ("Life", Form.Increase, 6),
                ("Intelligence", Form.BaseAdd, 13),
            }.Select(t => (t.stat, t.form, (NodeValue?) t.value, (ModifierSource) global)).ToArray();

            var actual = _parser.ParseJewelSocketedInSkillTree(item, JewelRadius.None, 0);

            AssertCorrectModifiers(valueCalculationContext, expectedModifiers, actual);
        }
    }
}