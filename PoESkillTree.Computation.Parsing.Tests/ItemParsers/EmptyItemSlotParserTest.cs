using NUnit.Framework;
using PoESkillTree.Computation.Common;
using PoESkillTree.Computation.Parsing.ItemParsers;
using PoESkillTree.GameModel.Items;
using static PoESkillTree.Computation.Parsing.Tests.ParserTestUtils;

namespace PoESkillTree.Computation.Parsing.Tests.ItemParsers
{
    [TestFixture]
    public class EmptyItemSlotParserTest
    {
        [TestCase(ItemSlot.OffHand)]
        [TestCase(ItemSlot.BodyArmour)]
        public void ParseReturnsNoModifiersIfNotMainHand(ItemSlot itemSlot)
        {
            var sut = CreateSut();

            var result = sut.Parse(itemSlot);

            Assert.IsEmpty(result.Modifiers);
        }

        [Test]
        public void ParseReturnsCorrectItemClassModifierIfMainHand()
        {
            var itemSlot = ItemSlot.MainHand;
            var expected = CreateModifier($"{itemSlot}.ItemClass", Form.BaseSet, (double) ItemClass.Unarmed);
            var sut = CreateSut();

            var result = sut.Parse(itemSlot);

            Assert.That(result.Modifiers, Has.Member(expected));
        }

        private static EmptyItemSlotParser CreateSut()
            => new EmptyItemSlotParser(CreateBuilderFactories());
    }
}