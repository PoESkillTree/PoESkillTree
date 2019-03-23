using System.Linq;
using Moq;
using NUnit.Framework;
using PoESkillTree.Computation.Builders.Stats;
using PoESkillTree.Computation.Common;
using PoESkillTree.GameModel.Items;

namespace PoESkillTree.Computation.Builders.Behaviors
{
    [TestFixture]
    public class RequirementUncappedSubtotalValueTest
    {
        [TestCase(-1.0, null, 1.0)]
        [TestCase(42.0, 43.0, 41.0, 43.0, 40.0)]
        public void CalculateReturnsCorrectResult(params double?[] values)
        {
            var expected = (NodeValue?) values.Max();
            var transformedStat = new Stat("transformed");
            var paths = values
                .Select((_, i) => new PathDefinition(new ModifierSource.Local.Gem(ItemSlot.Helm, i)))
                .ToList();
            var contextMock = new Mock<IValueCalculationContext>();
            contextMock.Setup(c => c.GetPaths(transformedStat)).Returns(paths);
            for (var i = 0; i < paths.Count; i++)
            {
                var path = paths[i];
                contextMock.Setup(c => c.GetValue(transformedStat, NodeType.PathTotal, path))
                    .Returns((NodeValue?) values[i]);
            }
            var transformedValue = new FunctionalValue(c => c.GetValues(transformedStat, NodeType.PathTotal).Sum(), "");
            var sut = new RequirementUncappedSubtotalValue(transformedStat, transformedValue);

            var actual = sut.Calculate(contextMock.Object);

            Assert.AreEqual(expected, actual);
        }
    }
}