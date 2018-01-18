using Moq;
using NUnit.Framework;
using PoESkillTree.Computation.Common;

namespace PoESkillTree.Computation.Core.Tests
{
    [TestFixture]
    public class CalculationGraphExtensionsTest
    {
        [Test]
        public void EndBatchUpdateCallsGraphUpdate()
        {
            var graphMock = new Mock<ICalculationGraph>();

            graphMock.Object.NewBatchUpdate()
                .DoUpdate();

            graphMock.Verify(g => g.Update(It.IsAny<CalculationGraphUpdate>()));
        }

        [Test]
        public void DoUpdateWithoutAddingModifiersIsEmptyUpdate()
        {
            var graphMock = new Mock<ICalculationGraph>();

            graphMock.Object.NewBatchUpdate()
                .DoUpdate();

            graphMock.Verify(g => g.Update(EmptyUpdate));
        }

        [Test]
        public void DoUpdateAfterAddModifierIsCorrectUpdate()
        {
            var graphMock = new Mock<ICalculationGraph>();
            var tuple = MockUpdateTuple();

            graphMock.Object.NewBatchUpdate()
                .AddModifier(tuple.modifier, tuple.source)
                .DoUpdate();

            graphMock.Verify(g => g.Update(ExpectUpdateFor(new[] { tuple }, null)));
        }

        [Test]
        public void DoUpdateAfterRemoveModifierIsCorrectUpdate()
        {
            var graphMock = new Mock<ICalculationGraph>();
            var tuple = MockUpdateTuple();

            graphMock.Object.NewBatchUpdate()
                .RemoveModifier(tuple.modifier, tuple.source)
                .DoUpdate();

            graphMock.Verify(g => g.Update(ExpectUpdateFor(null, new[] { tuple })));
        }

        [Test]
        public void DoUpdateAfterManyAddsAndRemovesIsCorrectUpdate()
        {
            var graphMock = new Mock<ICalculationGraph>();
            var added = MockManyUpdateTuples();
            var removed = MockManyUpdateTuples();

            graphMock.Object.NewBatchUpdate()
                .AddModifier(added[0].modifier, added[0].source)
                .RemoveModifier(removed[0].modifier, removed[0].source)
                .RemoveModifier(removed[1].modifier, removed[1].source)
                .AddModifier(added[1].modifier, added[1].source)
                .AddModifier(added[2].modifier, added[2].source)
                .RemoveModifier(removed[2].modifier, removed[2].source)
                .DoUpdate();

            graphMock.Verify(g => g.Update(ExpectUpdateFor(added, removed)));
        }

        [Test]
        public void DoUpdateAfterAddModifiersIsCorrectUpdate()
        {
            var graphMock = new Mock<ICalculationGraph>();
            var added = MockManyUpdateTuples();

            graphMock.Object.NewBatchUpdate()
                .AddModifiers(added)
                .DoUpdate();

            graphMock.Verify(g => g.Update(ExpectUpdateFor(added, null)));
        }

        [Test]
        public void DoUpdateAfterRemoveModifiersIsCorrectUpdate()
        {
            var graphMock = new Mock<ICalculationGraph>();
            var removed = MockManyUpdateTuples();

            graphMock.Object.NewBatchUpdate()
                .RemoveModifiers(removed)
                .DoUpdate();

            graphMock.Verify(g => g.Update(ExpectUpdateFor(null, removed)));
        }


        private static readonly CalculationGraphUpdate EmptyUpdate = new CalculationGraphUpdate(
            new (Modifier modifier, IModifierSource source)[0], new (Modifier modifier, IModifierSource source)[0]);

        private static CalculationGraphUpdate ExpectUpdateFor(
            (Modifier modifier, IModifierSource source)[] adds = null,
            (Modifier modifier, IModifierSource source)[] removes = null)
        {
            return new CalculationGraphUpdate(
                adds ?? new (Modifier modifier, IModifierSource source)[0],
                removes ?? new (Modifier modifier, IModifierSource source)[0]);
        }

        private static (Modifier modifier, IModifierSource source)[] MockManyUpdateTuples()
        {
            return new[]
            {
                MockUpdateTuple(), MockUpdateTuple(), MockUpdateTuple()
            };
        }

        private static (Modifier modifier, IModifierSource source) MockUpdateTuple()
        {
            return (
                new Modifier(new IStat[0], Form.Increase, Mock.Of<IValue>()),
                Mock.Of<IModifierSource>());
        }
    }
}