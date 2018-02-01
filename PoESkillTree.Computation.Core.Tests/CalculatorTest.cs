using Moq;
using MoreLinq;
using NUnit.Framework;
using PoESkillTree.Computation.Core.Events;
using PoESkillTree.Computation.Core.Graphs;

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
            var modifier = NodeHelper.MockModifier();
            var suspenderMock = new Mock<ISuspendableEvents>(MockBehavior.Strict);
            var modifierCollectionMock = new Mock<IModifierCollection>(MockBehavior.Strict);
            var graphPrunerMock = new Mock<ICalculationGraphPruner>(MockBehavior.Strict);
            var seq = new MockSequence();
            suspenderMock.InSequence(seq).Setup(s => s.SuspendEvents());
            modifierCollectionMock.InSequence(seq).Setup(c => c.AddModifier(modifier));
            graphPrunerMock.InSequence(seq).Setup(p => p.RemoveUnusedNodes());
            suspenderMock.InSequence(seq).Setup(s => s.ResumeEvents());
            var sut = CreateSut(null, suspenderMock.Object, modifierCollectionMock.Object, graphPrunerMock.Object);

            sut.NewBatchUpdate().AddModifier(modifier).DoUpdate();

            suspenderMock.Verify(s => s.ResumeEvents());
        }

        [Test]
        public void UpdateCallsInjectedModifierCollectionCorrectly()
        {
            var addedModifiers = NodeHelper.MockManyModifiers(); 
            var removedModifiers = NodeHelper.MockManyModifiers(); 
            var modifierCollectionMock = new Mock<IModifierCollection>();
            var sut = CreateSut(
                null, Mock.Of<ISuspendableEvents>(), modifierCollectionMock.Object, Mock.Of<ICalculationGraphPruner>());

            sut.Update(new CalculatorUpdate(addedModifiers, removedModifiers));

            addedModifiers.ForEach(m => modifierCollectionMock.Verify(c => c.AddModifier(m))); 
            removedModifiers.ForEach(m => modifierCollectionMock.Verify(c => c.RemoveModifier(m))); 
        }

        private static Calculator CreateSut(
            INodeRepository nodeRepository = null, ISuspendableEvents suspender = null,
            IModifierCollection modifierCollection = null, ICalculationGraphPruner graphPruner = null) => 
            new Calculator(nodeRepository, suspender, modifierCollection, graphPruner);
    }
}