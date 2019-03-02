using NUnit.Framework;
using PoESkillTree.Computation.Common;
using PoESkillTree.Computation.Common.Tests;

namespace PoESkillTree.Computation.Core.Tests
{
    [TestFixture]
    public class CalculatorUpdateTest
    {
        [Test]
        public void AccumulateCombinesDistinctUpdatesCorrectly()
        {
            var modifiers = Helper.MockManyModifiers(4);
            var l = new CalculatorUpdate(new[] { modifiers[0] }, new[] { modifiers[1] });
            var r = new CalculatorUpdate(new[] { modifiers[2] }, new[] { modifiers[3] });
            var expected =
                new CalculatorUpdate(new[] { modifiers[0], modifiers[2] }, new[] { modifiers[1], modifiers[3] });

            var actual = CalculatorUpdate.Accumulate(l, r);

            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void AccumulateRemovesWhenRemovedAndAddedInSameUpdate()
        {
            var modifiers = Helper.MockManyModifiers(4);
            var l =
                new CalculatorUpdate(new[] { modifiers[0], modifiers[1] }, new[] { modifiers[1], modifiers[2] });
            var expected = new CalculatorUpdate(new[] { modifiers[0] }, new[] { modifiers[2] });

            var actual = CalculatorUpdate.Accumulate(l, CalculatorUpdate.Empty);

            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void AccumulateRemovesWhenRemovedInFirstAndAddedInSecondUpdate()
        {
            var modifiers = Helper.MockManyModifiers(4);
            var l = new CalculatorUpdate(new[] { modifiers[1] }, new[] { modifiers[0] });
            var r = new CalculatorUpdate(new[] { modifiers[0] }, new[] { modifiers[2] });
            var expected = new CalculatorUpdate(new[] { modifiers[1] }, new[] { modifiers[2] });

            var actual = CalculatorUpdate.Accumulate(l, r);

            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void AccumulateRemovesWhenAddedInFirstAndRemovedInSecondUpdate()
        {
            var modifiers = Helper.MockManyModifiers(4);
            var l = new CalculatorUpdate(new[] { modifiers[0] }, new[] { modifiers[2] });
            var r = new CalculatorUpdate(new[] { modifiers[1] }, new[] { modifiers[0] });
            var expected = new CalculatorUpdate(new[] { modifiers[1] }, new[] { modifiers[2] });

            var actual = CalculatorUpdate.Accumulate(l, r);

            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void AccumulateKeepsTwiceRemovedModifierTwice()
        {
            var modifiers = Helper.MockManyModifiers(4);
            var expected = new CalculatorUpdate(new Modifier[0], new[] { modifiers[0], modifiers[0] });

            var actual = CalculatorUpdate.Accumulate(expected, CalculatorUpdate.Empty);

            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void AccumulateKeepsTwiceAddedModifierTwice()
        {
            var modifiers = Helper.MockManyModifiers(4);
            var expected = new CalculatorUpdate(new[] { modifiers[0], modifiers[0] }, new Modifier[0]);

            var actual = CalculatorUpdate.Accumulate(expected, CalculatorUpdate.Empty);

            Assert.AreEqual(expected, actual);
        }
    }
}