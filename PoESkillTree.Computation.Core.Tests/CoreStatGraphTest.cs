using Moq;
using NUnit.Framework;
using PoESkillTree.Computation.Common;
using static PoESkillTree.Computation.Core.Tests.NodeHelper;

namespace PoESkillTree.Computation.Core.Tests
{
    [TestFixture]
    public class CoreStatGraphTest
    {
        [Test]
        public void SutIsStatGraph()
        {
            var sut = CreateSut();

            Assert.IsInstanceOf<IStatGraph>(sut);
        }

        [Test]
        public void GetNodeReturnsInjectedNodeFactoryGetNode()
        {
            var expected = MockNodeViewProvider();
            var nodeFactory = Mock.Of<INodeFactory>(f => f.Create(Stat, NodeType.Base) == expected);
            var sut = CreateSut(nodeFactory);

            var actual = sut.GetNode(NodeType.Base);

            Assert.AreSame(expected, actual);
        }

        [Test]
        public void GetNodeCachesResult()
        {
            var expected = MockNodeViewProvider();
            var nodeFactoryMock = new Mock<INodeFactory>();
            nodeFactoryMock.Setup(f => f.Create(Stat, NodeType.Base)).Returns(expected);
            var sut = CreateSut(nodeFactoryMock.Object);
            sut.GetNode(NodeType.Base);
            nodeFactoryMock.Setup(f => f.Create(Stat, NodeType.Base)).Returns(MockNodeViewProvider);

            var actual = sut.GetNode(NodeType.Base);

            Assert.AreSame(expected, actual);
        }

        [Test]
        public void GetFormNodecollectionReturnsInjectedNodeCollectionFactoryGetFormNodeCollection()
        {
            var expected = MockModifierNodeCollection();
            var nodeCollectionFactory = Mock.Of<INodeCollectionFactory>(f => f.Create() == expected);
            var sut = CreateSut(nodeCollectionFactory: nodeCollectionFactory);

            var actual = sut.GetFormNodeCollection(Form.More);

            Assert.AreSame(expected, actual);
        }

        [Test]
        public void GetFormNodeCollectionCachesResult()
        {
            var expected = MockModifierNodeCollection();
            var factoryMock = new Mock<INodeCollectionFactory>();
            factoryMock.Setup(f => f.Create()).Returns(expected);
            var sut = CreateSut(nodeCollectionFactory: factoryMock.Object);
            sut.GetFormNodeCollection(Form.More);
            factoryMock.Setup(f => f.Create()).Returns(MockModifierNodeCollection);

            var actual = sut.GetFormNodeCollection(Form.More);

            Assert.AreSame(expected, actual);
        }

        [Test]
        public void NodesReturnsEmptyDictionary()
        {
            var sut = CreateSut();

            var actual = sut.Nodes;

            CollectionAssert.IsEmpty(actual);
        }

        [Test]
        public void NodesReturnsInternalDictionary()
        {
            var expected = MockNodeViewProvider();
            var nodeFactory = Mock.Of<INodeFactory>(f => f.Create(Stat, NodeType.Base) == expected);
            var sut = CreateSut(nodeFactory);
            var dict = sut.Nodes;
            sut.GetNode(NodeType.Base);

            var actual = dict[NodeType.Base];

            Assert.AreSame(expected, actual);
        }

        [Test]
        public void FormNodeCollectionsReturnsEmptyDictionary()
        {
            var sut = CreateSut();

            var actual = sut.FormNodeCollections;

            CollectionAssert.IsEmpty(actual);
        }

        [Test]
        public void FormNodeCollectionsReturnsInternalDictionary()
        {
            var expected = MockModifierNodeCollection();
            var nodeCollectionFactory = Mock.Of<INodeCollectionFactory>(f => f.Create() == expected);
            var sut = CreateSut(nodeCollectionFactory: nodeCollectionFactory);
            var dict = sut.FormNodeCollections;
            sut.GetFormNodeCollection(Form.More);

            var actual = dict[Form.More];

            Assert.AreSame(expected, actual);
        }

        [Test]
        public void RemoveNodeRemoves()
        {
            var nodeFactory = Mock.Of<INodeFactory>(f => f.Create(Stat, NodeType.Base) == MockNodeViewProvider());
            var sut = CreateSut(nodeFactory);
            sut.GetNode(NodeType.Base);

            sut.RemoveNode(NodeType.Base);

            CollectionAssert.IsEmpty(sut.Nodes);
        }

        [Test]
        public void RemoveNodesDisposesNode()
        {
            var nodeMock = new Mock<ISuspendableEventViewProvider<IDisposableNode>>();
            nodeMock.Setup(p => p.DefaultView.Dispose()).Verifiable();
            nodeMock.Setup(p => p.SuspendableView.Dispose()).Verifiable();
            var nodeFactory = Mock.Of<INodeFactory>(f => f.Create(Stat, NodeType.Base) == nodeMock.Object);
            var sut = CreateSut(nodeFactory);
            sut.GetNode(NodeType.Base);

            sut.RemoveNode(NodeType.Base);

            nodeMock.Verify();
        }

        [Test]
        public void RemoveNodeDoesNothingIfNodeTypeIsUnknown()
        {
            var nodeFactory = Mock.Of<INodeFactory>(f => f.Create(Stat, NodeType.BaseAdd) == MockNodeViewProvider());
            var sut = CreateSut(nodeFactory);
            sut.GetNode(NodeType.BaseAdd);

            sut.RemoveNode(NodeType.Base);
        }

        [Test]
        public void RemoveFormNodeCollectionRemoves()
        {
            var nodeCollectionFactory =
                Mock.Of<INodeCollectionFactory>(f => f.Create() == MockModifierNodeCollection());
            var sut = CreateSut(nodeCollectionFactory: nodeCollectionFactory);
            sut.GetFormNodeCollection(Form.More);

            sut.RemoveFormNodeCollection(Form.More);

            CollectionAssert.IsEmpty(sut.FormNodeCollections);
        }

        [Test]
        public void AddModifierAddsCorrectly()
        {
            var value = Mock.Of<IValue>();
            var modifier = new Modifier(new IStat[0], Form.BaseAdd, value);

            var node = MockNodeViewProvider();
            var nodeFactory = Mock.Of<INodeFactory>(f => f.Create(value) == node);
            var collection = CreateModifierNodeCollection();
            var collectionFactory = Mock.Of<INodeCollectionFactory>(f => f.Create() == collection);
            var sut = CreateSut(nodeFactory, collectionFactory);

            sut.AddModifier(modifier);

            Assert.That(collection.DefaultView,
                Has.Exactly(1).Items.SameAs(node.DefaultView));
        }

        [Test]
        public void RemoveModifierRemovesCorrectly()
        {
            var value = Mock.Of<IValue>();
            var modifier = new Modifier(new IStat[0], Form.BaseAdd, value);

            var node = MockNodeViewProvider();
            var nodeFactory = Mock.Of<INodeFactory>(f => f.Create(value) == node);
            var collection = CreateModifierNodeCollection();
            var collectionFactory = Mock.Of<INodeCollectionFactory>(f => f.Create() == collection);
            var sut = CreateSut(nodeFactory, collectionFactory);
            sut.AddModifier(modifier);

            var actual = sut.RemoveModifier(modifier);

            CollectionAssert.IsEmpty(collection.DefaultView);
            Assert.IsTrue(actual);
        }

        [Test]
        public void RemoveModifierDisposesNode()
        {
            var value = Mock.Of<IValue>();
            var modifier = new Modifier(new IStat[0], Form.BaseAdd, value);

            var node = MockNodeViewProvider();
            var nodeFactory = Mock.Of<INodeFactory>(f => f.Create(value) == node);
            var collectionFactory = Mock.Of<INodeCollectionFactory>(f => f.Create() == CreateModifierNodeCollection());
            var sut = CreateSut(nodeFactory, collectionFactory);
            sut.AddModifier(modifier);

            sut.RemoveModifier(modifier);

            Mock.Get(node.DefaultView).Verify(n => n.Dispose());
            Mock.Get(node.SuspendableView).Verify(n => n.Dispose());
        }

        [Test]
        public void RemoveReturnsFalseIfModifierIsUnknown()
        {
            var value = Mock.Of<IValue>();
            var modifier = new Modifier(new IStat[0], Form.BaseAdd, value);

            var collectionFactory = Mock.Of<INodeCollectionFactory>(f => f.Create() == CreateModifierNodeCollection());
            var sut = CreateSut(nodeCollectionFactory: collectionFactory);

            var actual = sut.RemoveModifier(modifier);

            Assert.IsFalse(actual);
        }

        [Test]
        public void ModifierCountReturnsZeroInitially()
        {
            var sut = CreateSut();

            var actual = sut.ModifierCount;

            Assert.AreEqual(0, actual);
        }

        [Test]
        public void ModifierCountReturnsCorrectResult()
        {
            var values = new[] { Mock.Of<IValue>(), Mock.Of<IValue>() };
            var modifiers = new[]
            {
                new Modifier(new IStat[0], Form.More, values[0]), new Modifier(new IStat[0], Form.More, values[1])
            };

            var nodes = new[] { MockNodeViewProvider(), MockNodeViewProvider() };
            var nodeFactory =
                Mock.Of<INodeFactory>(f => f.Create(values[0]) == nodes[0] && f.Create(values[1]) == nodes[1]);
            var collectionFactory = Mock.Of<INodeCollectionFactory>(f => f.Create() == CreateModifierNodeCollection());
            var sut = CreateSut(nodeFactory, collectionFactory);

            sut.AddModifier(modifiers[0]);
            sut.AddModifier(modifiers[1]);
            sut.RemoveModifier(modifiers[0]);
            sut.RemoveModifier(modifiers[0]);
            var actual = sut.ModifierCount;

            Assert.AreEqual(1, actual);
        }

        private static readonly IStat Stat = new StatStub();

        private static CoreStatGraph CreateSut(
            INodeFactory nodeFactory = null, INodeCollectionFactory nodeCollectionFactory = null) =>
            new CoreStatGraph(Stat, nodeFactory, nodeCollectionFactory);

        private static ISuspendableEventViewProvider<IDisposableNode> MockNodeViewProvider() =>
            Mock.Of<ISuspendableEventViewProvider<IDisposableNode>>(p => 
                p.DefaultView == MockNode(0) && p.SuspendableView == MockNode(0));

        private static ModifierNodeCollection MockModifierNodeCollection()
        {
            var nodeCollectionViewProvider = Mock.Of<ISuspendableEventViewProvider<NodeCollection<Modifier>>>();
            return new ModifierNodeCollection(nodeCollectionViewProvider);
        }

        private static ModifierNodeCollection CreateModifierNodeCollection()
        {
            var defaultView = new NodeCollection<Modifier>();
            var suspendableView = new SuspendableNodeCollection<Modifier>();
            var nodeCollectionViewProvider = SuspendableEventViewProvider.Create(defaultView, suspendableView);
            return new ModifierNodeCollection(nodeCollectionViewProvider);
        }
    }
}