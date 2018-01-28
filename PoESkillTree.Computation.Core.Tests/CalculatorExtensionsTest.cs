using Moq;
using NUnit.Framework;
using PoESkillTree.Computation.Common;

namespace PoESkillTree.Computation.Core.Tests
{
    [TestFixture]
    public class CalculatorExtensionsTest
    {
        [Test]
        public void EndBatchUpdateCallsGraphUpdate()
        {
            var graphMock = new Mock<ICalculator>();

            graphMock.Object.NewBatchUpdate()
                .DoUpdate();

            graphMock.Verify(g => g.Update(It.IsAny<CalculatorUpdate>()));
        }

        [Test]
        public void DoUpdateWithoutAddingModifiersIsEmptyUpdate()
        {
            var graphMock = new Mock<ICalculator>();

            graphMock.Object.NewBatchUpdate()
                .DoUpdate();

            graphMock.Verify(g => g.Update(EmptyUpdate));
        }

        [Test]
        public void DoUpdateAfterAddModifierIsCorrectUpdate()
        {
            var graphMock = new Mock<ICalculator>();
            var mod = MockModifier();

            graphMock.Object.NewBatchUpdate()
                .AddModifier(mod)
                .DoUpdate();

            graphMock.Verify(g => g.Update(ExpectUpdateFor(new[] { mod }, null)));
        }

        [Test]
        public void DoUpdateAfterRemoveModifierIsCorrectUpdate()
        {
            var graphMock = new Mock<ICalculator>();
            var mod = MockModifier();

            graphMock.Object.NewBatchUpdate()
                .RemoveModifier(mod)
                .DoUpdate();

            graphMock.Verify(g => g.Update(ExpectUpdateFor(null, new[] { mod })));
        }

        [Test]
        public void DoUpdateAfterManyAddsAndRemovesIsCorrectUpdate()
        {
            var graphMock = new Mock<ICalculator>();
            var added = MockManyModifiers();
            var removed = MockManyModifiers();

            graphMock.Object.NewBatchUpdate()
                .AddModifier(added[0])
                .RemoveModifier(removed[0])
                .RemoveModifier(removed[1])
                .AddModifier(added[1])
                .AddModifier(added[2])
                .RemoveModifier(removed[2])
                .DoUpdate();

            graphMock.Verify(g => g.Update(ExpectUpdateFor(added, removed)));
        }

        [Test]
        public void DoUpdateAfterAddModifiersIsCorrectUpdate()
        {
            var graphMock = new Mock<ICalculator>();
            var added = MockManyModifiers();

            graphMock.Object.NewBatchUpdate()
                .AddModifiers(added)
                .DoUpdate();

            graphMock.Verify(g => g.Update(ExpectUpdateFor(added, null)));
        }

        [Test]
        public void DoUpdateAfterRemoveModifiersIsCorrectUpdate()
        {
            var graphMock = new Mock<ICalculator>();
            var removed = MockManyModifiers();

            graphMock.Object.NewBatchUpdate()
                .RemoveModifiers(removed)
                .DoUpdate();

            graphMock.Verify(g => g.Update(ExpectUpdateFor(null, removed)));
        }


        private static readonly CalculatorUpdate EmptyUpdate =
            new CalculatorUpdate(new Modifier[0], new Modifier[0]);

        private static CalculatorUpdate ExpectUpdateFor(Modifier[] adds = null, Modifier[] removes = null) => 
            new CalculatorUpdate(adds ?? new Modifier[0], removes ?? new Modifier[0]);

        private static Modifier[] MockManyModifiers() =>
            new[] { MockModifier(), MockModifier(), MockModifier() };

        private static Modifier MockModifier() =>
            new Modifier(new IStat[0], Form.Increase, Mock.Of<IValue>());
    }
}