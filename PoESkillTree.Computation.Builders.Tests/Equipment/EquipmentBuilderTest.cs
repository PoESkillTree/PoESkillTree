using Moq;
using NUnit.Framework;
using PoESkillTree.Common.Model.Items.Enums;
using PoESkillTree.Computation.Builders.Equipment;
using PoESkillTree.Computation.Builders.Stats;
using PoESkillTree.Computation.Common;

namespace PoESkillTree.Computation.Builders.Tests.Equipment
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
                f.FromIdentity("BodyArmour.ItemTags", default, typeof(Tags), false) == tagsStat);
            var sut = new EquipmentBuilder(statFactory, ItemSlot.BodyArmour);
            var context = Mock.Of<IValueCalculationContext>(c =>
                c.GetValue(tagsStat, NodeType.Total, PathDefinition.MainPath) == new NodeValue((double) slotTags));

            var value = sut.Has(queryTags).Build(default).value;
            var actual = value.Calculate(context);

            Assert.AreEqual(slotTags.HasFlag(queryTags), actual.IsTrue());
        }
    }
}