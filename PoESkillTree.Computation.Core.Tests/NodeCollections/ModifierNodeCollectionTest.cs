using Moq;
using NUnit.Framework;
using PoESkillTree.Computation.Common;
using PoESkillTree.Computation.Core.Events;
using static PoESkillTree.Computation.Common.Helper;
using static PoESkillTree.Computation.Core.NodeHelper;

namespace PoESkillTree.Computation.Core.NodeCollections
{
    [TestFixture]
    public class ModifierNodeCollectionTest
    {
        [Test]
        public void DefaultViewReturnsConstructorParameter()
        {
            var defaultView = CreateNodeCollection();
            var sut = CreateSut(defaultView, null);

            Assert.AreSame(defaultView, sut.DefaultView);
        }

        [Test]
        public void BufferingViewReturnsConstructorParameter()
        {
            var bufferingView = CreateNodeCollection();
            var sut = CreateSut(null, bufferingView);

            Assert.AreSame(bufferingView, sut.BufferingView);
        }

        [TestCase(0)]
        [TestCase(42)]
        public void SubscriberCountReturnsInjectedResult(int expected)
        {
            var provider = Mock.Of<IBufferingEventViewProvider<NodeCollection<Modifier>>>(
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
            
            CollectionAssert.Contains(sut.DefaultView, (defaultNode, modifier));
        }

        [Test]
        public void AddAddsToBufferingView()
        {
            var sut = CreateSut();
            var modifier = MockModifier();
            var bufferingNode = MockNode();
            var node = MockNodeProvider(bufferingView: bufferingNode);
            
            sut.Add(node, modifier);
            
            CollectionAssert.Contains(sut.BufferingView, (bufferingNode, modifier));
        }

        [Test]
        public void RemoveRemovesFromViews()
        {
            var sut = CreateSut();
            var defaultNode = MockNode();
            var bufferingNode = MockNode();
            var node = MockNodeProvider(defaultNode, bufferingNode);
            var modifier = MockModifier();
            sut.Add(node, modifier);

            sut.Remove(node, modifier);
            
            CollectionAssert.DoesNotContain(sut.DefaultView, (node, modifier));
            CollectionAssert.DoesNotContain(sut.BufferingView, (node, modifier));
        }

        private static ModifierNodeCollection CreateSut() =>
            CreateSut(CreateNodeCollection(), CreateNodeCollection());

        private static ModifierNodeCollection CreateSut(
            NodeCollection<Modifier> defaultView, NodeCollection<Modifier> suspendableView) => 
            CreateSut(BufferingEventViewProvider.Create(defaultView, suspendableView));

        private static ModifierNodeCollection CreateSut(
            IBufferingEventViewProvider<NodeCollection<Modifier>> viewProvider) =>
            new ModifierNodeCollection(viewProvider);

        private static NodeCollection<Modifier> CreateNodeCollection()
            => new NodeCollection<Modifier>(new EventBuffer());
    }
}