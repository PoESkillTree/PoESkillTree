using Moq;
using NUnit.Framework;
using PoESkillTree.Computation.Common;
using static PoESkillTree.Computation.Core.Tests.NodeHelper;

namespace PoESkillTree.Computation.Core.Tests
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

            sut.Add(modifier, node);
            
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

            sut.Add(modifier, node);
            
            CollectionAssert.Contains(sut.SuspendableView, suspendableNode);
            Assert.AreSame(modifier, sut.SuspendableView.NodeProperties[suspendableNode]);
        }

        [Test]
        public void AddAddsToSuspender()
        {
            var sut = CreateSut();
            var suspenderMock = new Mock<ISuspendableEvents>();
            var node = MockNodeProvider(suspender: suspenderMock.Object);

            sut.Add(MockModifier(), node);

            sut.Suspender.SuspendEvents();
            suspenderMock.Verify(s => s.SuspendEvents());
        }

        [Test]
        public void RemoveReturnsNullIfParameterWasNotAdded()
        {
            var sut = CreateSut();
            var modifier = MockModifier();

            var actual = sut.Remove(modifier);

            Assert.IsNull(actual);
        }

        [Test]
        public void RemoveReturnsNodeIfModifierWasAdded()
        {
            var sut = CreateSut();
            var modifier = MockModifier();
            var node = MockNodeProvider();
            sut.Add(modifier, node);

            var actual = sut.Remove(modifier);

            Assert.AreSame(node, actual);
        }

        [Test]
        public void RemoveRemovesFromViews()
        {
            var sut = CreateSut();
            var modifier = MockModifier();
            var defaultNode = MockNode();
            var suspendableNode = MockNode();
            var node = MockNodeProvider(defaultNode, suspendableNode);
            sut.Add(modifier, node);

            sut.Remove(modifier);
            
            CollectionAssert.DoesNotContain(sut.DefaultView, defaultNode);
            CollectionAssert.DoesNotContain(sut.SuspendableView, suspendableNode);
        }

        [Test]
        public void RemoveRemovesFromSuspender()
        {
            var sut = CreateSut();
            var suspenderMock = new Mock<ISuspendableEvents>();
            var modifier = MockModifier();
            var node = MockNodeProvider(suspender: suspenderMock.Object);
            sut.Add(modifier, node);

            sut.Remove(modifier);

            sut.Suspender.SuspendEvents();
            suspenderMock.Verify(s => s.SuspendEvents(), Times.Never);
        }

        [Test]
        public void RemoveReturnsNullIfModifierWasRemovedBefore()
        {
            var sut = CreateSut();
            var modifier = MockModifier();
            var node = MockNodeProvider();
            sut.Add(modifier, node);
            sut.Remove(modifier);

            var actual = sut.Remove(modifier);

            Assert.IsNull(actual);
        }

        [Test]
        public void ModifiersAddedMultipleTimesAreRemovedLiFo()
        {
            var sut = CreateSut();
            var modifier = MockModifier();
            var node1 = MockNodeProvider();
            sut.Add(modifier, node1);
            var node2 = MockNodeProvider();
            sut.Add(modifier, node2);

            Assert.AreSame(node2, sut.Remove(modifier));
            Assert.AreSame(node1, sut.Remove(modifier));
        }

        private static ModifierNodeCollection CreateSut() =>
            CreateSut(new NodeCollection<Modifier>(), new SuspendableNodeCollection<Modifier>());

        private static ModifierNodeCollection CreateSut(
            NodeCollection<Modifier> defaultView, SuspendableNodeCollection<Modifier> suspendableView) => 
            CreateSut(SuspendableEventViewProvider.Create(defaultView, suspendableView));

        private static ModifierNodeCollection CreateSut(
            ISuspendableEventViewProvider<NodeCollection<Modifier>> viewProvider) =>
            new ModifierNodeCollection(viewProvider);

        private static ISuspendableEventViewProvider<ICalculationNode> MockNodeProvider(
            ICalculationNode defaultNode = null, ICalculationNode suspendableNode = null, 
            ISuspendableEvents suspender = null)
        {
            defaultNode = defaultNode ?? MockNode();
            suspendableNode = suspendableNode ?? MockNode();
            suspender = suspender ?? Mock.Of<ISuspendableEvents>();
            return Mock.Of<ISuspendableEventViewProvider<ICalculationNode>>(
                p => p.DefaultView == defaultNode && p.SuspendableView == suspendableNode && p.Suspender == suspender);
        }
    }
}