using NUnit.Framework;
using PoESkillTree.Common.Model.Items.Enums;
using PoESkillTree.Computation.Builders.Equipment;

namespace PoESkillTree.Computation.Builders.Tests.Equipment
{
    [TestFixture]
    public class ItemSlotBuildersTest
    {
        [Test]
        public void FromResolvesToSelf()
        {
            var sut = new ItemSlotBuilders();
            var builder = sut.From(ItemSlot.Amulet);

            var actual = builder.Resolve(BuildersHelper.MockResolveContext());

            Assert.AreEqual(builder, actual);
        }

        [TestCase(ItemSlot.Amulet)]
        [TestCase(ItemSlot.Belt)]
        public void FromBuildsToPassedItemSlot(ItemSlot expected)
        {
            var sut = new ItemSlotBuilders();
            var builder = sut.From(expected);

            var actual = builder.Build();

            Assert.AreEqual(expected, actual);
        }
    }
}