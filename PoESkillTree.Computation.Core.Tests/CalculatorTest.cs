using Moq;
using MoreLinq;
using NUnit.Framework;
using PoESkillTree.Computation.Common;

namespace PoESkillTree.Computation.Core.Tests
{
    [TestFixture]
    public class CalculatorTest
    {
        [Test]
        public void SutIsCalculator()
        {
            var sut = CreateSut();

            Assert.IsInstanceOf<ICalculator>(sut);
        }

        [Test]
        public void NodeRepositoryReturnsInjectedInstance()
        {
            var expected = Mock.Of<INodeRepository>();
            var sut = CreateSut(expected);

            var actual = sut.NodeRepository;

            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void UpdateCallsInjectedInstancesInCorrectSequence()
        {
            var stat = new StatStub();
            var modifier = new Modifier(new[] { stat }, Form.BaseAdd, Mock.Of<IValue>());
            var suspenderMock = new Mock<ISuspendableEvents>(MockBehavior.Strict);
            var modifierCollectionMock = new Mock<IModifierCollection>(MockBehavior.Strict);
            var graphPrunerMock = new Mock<ICalculationGraphPruner>(MockBehavior.Strict);
            var seq = new MockSequence();
            suspenderMock.InSequence(seq).Setup(s => s.SuspendEvents());
            modifierCollectionMock.InSequence(seq).Setup(c => c.AddModifier(stat, modifier));
            graphPrunerMock.InSequence(seq).Setup(p => p.RemoveUnusedNodes());
            suspenderMock.InSequence(seq).Setup(s => s.ResumeEvents());
            var sut = CreateSut(null, suspenderMock.Object, modifierCollectionMock.Object, graphPrunerMock.Object);

            sut.NewBatchUpdate().AddModifier(modifier).DoUpdate();

            suspenderMock.Verify(s => s.ResumeEvents());
        }

        [Test]
        public void UpdateCallsInjectedModifierCollectionCorrectly()
        {
            var addedStats = new[] { new StatStub(), new StatStub(), new StatStub() };
            var removedStats = new[] { new StatStub(), new StatStub(), new StatStub() };
            var addedModifiers = new[]
            {
                new Modifier(new[] { addedStats[0], addedStats[1] }, Form.More, Mock.Of<IValue>()),
                new Modifier(new[] { addedStats[2] }, Form.More, Mock.Of<IValue>()),
            };
            var removedModifiers =  new[]
            {
                new Modifier(new[] { removedStats[0] }, Form.More, Mock.Of<IValue>()),
                new Modifier(new[] { removedStats[1], removedStats[2] }, Form.More, Mock.Of<IValue>()),
            };
            var modifierCollectionMock = new Mock<IModifierCollection>();
            var sut = CreateSut(
                null, Mock.Of<ISuspendableEvents>(), modifierCollectionMock.Object, Mock.Of<ICalculationGraphPruner>());

            sut.Update(new CalculatorUpdate(addedModifiers, removedModifiers));

            modifierCollectionMock.Verify(c => c.AddModifier(addedStats[0], addedModifiers[0]));
            modifierCollectionMock.Verify(c => c.AddModifier(addedStats[1], addedModifiers[0]));
            modifierCollectionMock.Verify(c => c.AddModifier(addedStats[2], addedModifiers[1]));
            modifierCollectionMock.Verify(c => c.RemoveModifier(removedStats[0], removedModifiers[0]));
            modifierCollectionMock.Verify(c => c.RemoveModifier(removedStats[1], removedModifiers[1]));
            modifierCollectionMock.Verify(c => c.RemoveModifier(removedStats[2], removedModifiers[1]));
        }

        private static Calculator CreateSut(
            INodeRepository nodeRepository = null, ISuspendableEvents suspender = null,
            IModifierCollection modifierCollection = null, ICalculationGraphPruner graphPruner = null) => 
            new Calculator(nodeRepository, suspender, modifierCollection, graphPruner);
    }
}