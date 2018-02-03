using Moq;
using NUnit.Framework;
using PoESkillTree.Computation.Common;
using PoESkillTree.Computation.Core.Events;
using PoESkillTree.Computation.Core.NodeCollections;
using static PoESkillTree.Computation.Core.Tests.NodeHelper;

namespace PoESkillTree.Computation.Core.Tests.NodeCollections
{
    [TestFixture]
    public class ModifierNodeCollectionTest
    {
        [Test]
        public void DefaultViewReturnsConstructorParameter()
        {
            var defaultView = new NodeCollection<Modifier>();
            var sut = CreateSut(defaultView, null);

            Assert.AreSame(defaultView, sut.DefaultView);
        }

        [Test]
        public void SuspendableViewReturnsConstructorParameter()
        {
            var suspendableView = new SuspendableNodeCollection<Modifier>();
            var sut = CreateSut(null, suspendableView);

            Assert.AreSame(suspendableView, sut.SuspendableView);
        }

        [Test]
        public void SuspenderSuspendEventsCallsProvidedSuspender()
        {
            var suspenderMock = new Mock<ISuspendableEvents>();
            var provider = Mock.Of<ISuspendableEventViewProvider<NodeCollection<Modifier>>>(
                p => p.Suspender == suspenderMock.Object);
            var sut = CreateSut(provider);

            sut.Suspender.SuspendEvents();

            suspenderMock.Verify(s => s.SuspendEvents());
        }

        [TestCase(0)]
        [TestCase(42)]
        public void SubscriberCountReturnsInjectedResult(int expected)
        {
            var provider = Mock.Of<ISuspendableEventViewProvider<NodeCollection<Modifier>>>(
                p => p.SubscriberCount == expected);
            var sut = CreateSut(provider);

            var actual = sut.SubscriberCount;

            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void AddAddsToDefaultView()
        {
            var sut = CreateSut();
            var modifier = MockModifier();
            var defaultNode = MockNode();
            var node = MockNodeProvider(defaultNode);

            sut.Add(node, modifier);
            
            CollectionAssert.Contains(sut.DefaultView, defaultNode);
            Assert.AreSame(modifier, sut.DefaultView.NodeProperties[defaultNode]);
        }

        [Test]
        public void AddAddsToSuspendableView()
        {
            var sut = CreateSut();
            var modifier = MockModifier();
            var suspendableNode = MockNode();
            var node = MockNodeProvider(suspendableNode: suspendableNode);
            
            sut.Add(node, modifier);
            
            CollectionAssert.Contains(sut.SuspendableView, suspendableNode);
            Assert.AreSame(modifier, sut.SuspendableView.NodeProperties[suspendableNode]);
        }

        [Test]
        public void AddAddsToSuspender()
        {
            var sut = CreateSut();
            var suspenderMock = new Mock<ISuspendableEvents>();
            var node = MockNodeProvider(suspender: suspenderMock.Object);

            sut.Add(node, MockModifier());

            sut.Suspender.SuspendEvents();
            suspenderMock.Verify(s => s.SuspendEvents());
        }

        [Test]
        public void RemoveRemovesFromViews()
        {
            var sut = CreateSut();
            var defaultNode = MockNode();
            var suspendableNode = MockNode();
            var node = MockNodeProvider(defaultNode, suspendableNode);
            sut.Add(node, MockModifier());

            sut.Remove(node);
            
            CollectionAssert.DoesNotContain(sut.DefaultView, defaultNode);
            CollectionAssert.DoesNotContain(sut.SuspendableView, suspendableNode);
        }

        [Test]
        public void RemoveRemovesFromSuspender()
        {
            var sut = CreateSut();
            var suspenderMock = new Mock<ISuspendableEvents>();
            var node = MockNodeProvider(suspender: suspenderMock.Object);
            sut.Add(node, MockModifier());

            sut.Remove(node);

            sut.Suspender.SuspendEvents();
            suspenderMock.Verify(s => s.SuspendEvents(), Times.Never);
        }

        private static ModifierNodeCollection CreateSut() =>
            CreateSut(new NodeCollection<Modifier>(), new SuspendableNodeCollection<Modifier>());

        private static ModifierNodeCollection CreateSut(
            NodeCollection<Modifier> defaultView, SuspendableNodeCollection<Modifier> suspendableView) => 
            CreateSut(SuspendableEventViewProvider.Create(defaultView, suspendableView));

        private static ModifierNodeCollection CreateSut(
            ISuspendableEventViewProvider<NodeCollection<Modifier>> viewProvider) =>
            new ModifierNodeCollection(viewProvider);
    }
}