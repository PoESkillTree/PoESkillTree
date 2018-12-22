using NUnit.Framework;
using PoESkillTree.Computation.Builders;
using PoESkillTree.Computation.Builders.Stats;
using PoESkillTree.Computation.Common;
using PoESkillTree.Computation.Parsing.ItemParsers;
using PoESkillTree.GameModel.Items;
using PoESkillTree.GameModel.Skills;
using static PoESkillTree.Computation.Parsing.Tests.ParserTestUtils;

namespace PoESkillTree.Computation.Parsing.Tests
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
        {
            var builderFactories =
                new BuilderFactories(new StatFactory(), new SkillDefinitions(new SkillDefinition[0]));
            return new EmptyItemSlotParser(builderFactories);
        }
    }
}