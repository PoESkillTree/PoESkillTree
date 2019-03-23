using Moq;
using NUnit.Framework;
using PoESkillTree.Computation.Builders.Stats;
using PoESkillTree.Computation.Common;
using PoESkillTree.GameModel.Items;

namespace PoESkillTree.Computation.Builders.Equipment
{
    [TestFixture]
    public class EquipmentBuilderTest
    {
        [TestCase(Tags.Armour | Tags.StrArmour)]
        [TestCase(Tags.Amulet)]
        public void HasCalculatesCorrectly(Tags queryTags)
        {
            var slotTags = Tags.Armour | Tags.BodyArmour | Tags.StrArmour;
            var tagsStat = new Stat("BodyArmour.ItemTags");
            var statFactory = Mock.Of<IStatFactory>(f =>
                f.FromIdentity("BodyArmour.ItemTags", default, typeof(Tags), null) == tagsStat);
            var sut = new EquipmentBuilder(statFactory, ItemSlot.BodyArmour);
            var context = Mock.Of<IValueCalculationContext>(c =>
                c.GetValue(tagsStat, NodeType.Total, PathDefinition.MainPath) ==
                new NodeValue(slotTags.EncodeAsDouble()));

            var value = sut.Has(queryTags).Build(default).Value;
            var actual = value.Calculate(context);

            Assert.AreEqual(slotTags.HasFlag(queryTags), actual.IsTrue());
        }
    }
}