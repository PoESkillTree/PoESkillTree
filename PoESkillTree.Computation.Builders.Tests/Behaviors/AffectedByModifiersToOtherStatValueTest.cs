using System.Collections.Generic;
using System.Linq;
using Moq;
using NUnit.Framework;
using PoESkillTree.Computation.Builders.Stats;
using PoESkillTree.Computation.Common;
using PoESkillTree.Utils.Extensions;

namespace PoESkillTree.Computation.Builders.Behaviors
{
    [TestFixture]
    public class AffectedByModifiersToOtherStatValueTest
    {
        [TestCase(true, true)]
        [TestCase(true, false)]
        [TestCase(false, false)]
        public void CalculateReturnsCorrectResult(bool formIsAffected, bool conditionStatValue)
        {
            var expected = (NodeValue?) 42;
            var affectedForm = Form.Increase;
            var retrievedForm = formIsAffected ? affectedForm : Form.BaseAdd;

            var transformedStat = new Stat("transformed");
            var otherStat = new Stat("other");
            var conditionStat = new Stat("condition");
            var expectedPaths = new (IStat, PathDefinition)[] { (transformedStat, PathDefinition.MainPath) }
                .AsEnumerable();
            if (formIsAffected && conditionStatValue)
                expectedPaths = expectedPaths.Append((otherStat, PathDefinition.MainPath));

            var context = Mock.Of<IValueCalculationContext>(c =>
                c.GetValues(retrievedForm, expectedPaths) == new List<NodeValue?> { expected } &&
                c.GetValue(conditionStat, NodeType.Total, PathDefinition.MainPath) == (NodeValue?) conditionStatValue);
            var transformedValue = new FunctionalValue(c => c.GetValues(retrievedForm, transformedStat).Sum(), "");
            var sut = new AffectedByModifiersToOtherStatValue(transformedStat, otherStat, conditionStat, affectedForm,
                transformedValue);

            var actual = sut.Calculate(context);

            Assert.AreEqual(expected, actual);
        }
    }
}