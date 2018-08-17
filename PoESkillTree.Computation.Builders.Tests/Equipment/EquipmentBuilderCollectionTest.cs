using System;
using System.Linq;
using Moq;
using NUnit.Framework;
using PoESkillTree.Common.Utils;
using PoESkillTree.Computation.Builders.Conditions;
using PoESkillTree.Computation.Builders.Equipment;
using PoESkillTree.Computation.Builders.Stats;
using PoESkillTree.Computation.Builders.Tests.Stats;
using PoESkillTree.Computation.Common;
using PoESkillTree.Computation.Common.Parsing;
using PoESkillTree.GameModel.Items;

namespace PoESkillTree.Computation.Builders.Tests.Equipment
{
    [TestFixture]
    public class EquipmentBuilderCollectionTest
    {
        [Test]
        public void IndexerIsCached()
        {
            var sut = CreateSut();

            Assert.AreSame(sut[ItemSlot.Amulet], sut[ItemSlot.Amulet]);
        }

        [Test]
        public void CountWithoutPredicateIsItemSlotCount()
        {
            var expected = new NodeValue(Enum.GetValues(typeof(ItemSlot)).Length);
            var sut = CreateSut();

            var actual = sut.Count().Build().Calculate(null);

            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void CountWithPredicateCountsIsTrue()
        {
            var expected = new NodeValue();
            var sut = CreateSut();
            var contextMock = new Mock<IValueCalculationContext>();
            foreach (var itemSlot in Enum.GetValues(typeof(ItemSlot)).Cast<ItemSlot>())
            {
                var stat = sut[itemSlot].ItemTags.BuildToSingleStat();
                var value = itemSlot.ToString().StartsWith("A") ? (NodeValue?) (int) Tags.Amulet : null;
                contextMock.Setup(c => c.GetValue(stat, NodeType.Total, PathDefinition.MainPath)).Returns(value);
                if (value.IsTrue())
                    expected += 1;
            }

            var builder = sut.Count(b => b.HasItem);
            var actual = builder.Build().Calculate(contextMock.Object);

            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void CountBuildThrowsIfConditionHasNoValue()
        {
            var condition = new StatConvertingConditionBuilder(Funcs.Identity);
            var sut = CreateSut();

            var builder = sut.Count(_ => condition);

            Assert.Throws<ParseException>(() => builder.Build());
        }

        [Test]
        public void AnyWithoutPredicatesIsTrue()
        {
            var sut = CreateSut();

            var actual = sut.Any().Build().Value.Calculate(null);

            Assert.IsTrue(actual.IsTrue());
        }

        [TestCase("Amulet")]
        [TestCase("NotExisting")]
        public void AnyWithPredicatesIsOr(string truthySlot)
        {
            var expected = false;
            var sut = CreateSut();
            var contextMock = new Mock<IValueCalculationContext>();
            foreach (var itemSlot in Enum.GetValues(typeof(ItemSlot)).Cast<ItemSlot>())
            {
                var stat = sut[itemSlot].ItemTags.BuildToSingleStat();
                var value = itemSlot.ToString() == truthySlot ? (NodeValue?) (int) Tags.Amulet : null;
                contextMock.Setup(c => c.GetValue(stat, NodeType.Total, PathDefinition.MainPath)).Returns(value);
                expected |= value.IsTrue();
            }

            var builder = sut.Any(b => b.HasItem);
            var actual = builder.Build().Value.Calculate(contextMock.Object);

            Assert.AreEqual(expected, actual.IsTrue());
        }

        private static EquipmentBuilderCollection CreateSut() => new EquipmentBuilderCollection(new StatFactory());
    }
}