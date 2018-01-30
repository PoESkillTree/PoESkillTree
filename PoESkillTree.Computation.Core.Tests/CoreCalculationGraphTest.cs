using System;
using Moq;
using NUnit.Framework;
using PoESkillTree.Computation.Common;
using static PoESkillTree.Computation.Core.Tests.NodeHelper;

namespace PoESkillTree.Computation.Core.Tests
{
    [TestFixture]
    public class CoreCalculationGraphTest
    {
        [Test]
        public void SuspenderIsNullSuspendableEvents()
        {
            var sut = CreateSut();

            Assert.IsInstanceOf<NullSuspendableEvents>(sut.Suspender);
        }

        [Test]
        public void GetNodeReturnsInjectedNodeFactoryGetNode()
        {
            var expected = MockNodeViewProvider();
            var stat = new StatStub();
            var nodeFactory = Mock.Of<INodeFactory>(f => f.Create(stat, NodeType.Base) == expected);
            var sut = CreateSut(nodeFactory);

            var actual = sut.GetNode(stat, NodeType.Base);

            Assert.AreSame(expected, actual);
        }

        [Test]
        public void GetNodeCachesResult()
        {
            var expected = MockNodeViewProvider();
            var stat = new StatStub();
            var nodeFactoryMock = new Mock<INodeFactory>();
            nodeFactoryMock.Setup(f => f.Create(stat, NodeType.Base)).Returns(expected);
            var sut = CreateSut(nodeFactoryMock.Object);
            sut.GetNode(stat, NodeType.Base);
            nodeFactoryMock.Setup(f => f.Create(stat, NodeType.Base)).Returns(MockNodeViewProvider);

            var actual = sut.GetNode(stat, NodeType.Base);

            Assert.AreSame(expected, actual);
        }

        [Test]
        public void GetFormNodecollectionReturnsInjectedNodeCollectionFactorGetFormNodeCollection()
        {
            var expected = MockModifierNodeCollection();
            var nodeCollectionFactory = Mock.Of<INodeCollectionFactory>(f => f.Create() == expected);
            var sut = CreateSut(nodeCollectionFactory: nodeCollectionFactory);

            var actual = sut.GetFormNodeCollection(new StatStub(), Form.More);

            Assert.AreSame(expected, actual);
        }

        [Test]
        public void GetFormNodeCollectionCachesResult()
        {
            var expected = MockModifierNodeCollection();
            var stat = new StatStub();
            var factoryMock = new Mock<INodeCollectionFactory>();
            factoryMock.Setup(f => f.Create()).Returns(expected);
            var sut = CreateSut(nodeCollectionFactory: factoryMock.Object);
            sut.GetFormNodeCollection(stat, Form.More);
            factoryMock.Setup(f => f.Create()).Returns(MockModifierNodeCollection);

            var actual = sut.GetFormNodeCollection(stat, Form.More);

            Assert.AreSame(expected, actual);
        }

        [Test]
        public void GetNodesWithUnknownStatReturnsEmptyDictionary()
        {
            var sut = CreateSut();

            var actual = sut.GetNodes(new StatStub());

            CollectionAssert.IsEmpty(actual);
        }

        [Test]
        public void GetNodesReturnsInternalDictionary()
        {
            var expected = MockNodeViewProvider();
            var stat = new StatStub();
            var nodeFactory = Mock.Of<INodeFactory>(f => f.Create(stat, NodeType.Base) == expected);
            var sut = CreateSut(nodeFactory);
            var dict = sut.GetNodes(stat);
            sut.GetNode(stat, NodeType.Base);

            var actual = dict[NodeType.Base];

            Assert.AreSame(expected, actual);
        }

        [Test]
        public void GetFormNodesCollectionWithUnknownStatReturnsEmptyDictionary()
        {
            var sut = CreateSut();

            var actual = sut.GetFormNodeCollections(new StatStub());

            CollectionAssert.IsEmpty(actual);
        }

        [Test]
        public void GetFormNodeCollectionsReturnsInternalDictionary()
        {
            var expected = MockModifierNodeCollection();
            var stat = new StatStub();
            var nodeCollectionFactory = Mock.Of<INodeCollectionFactory>(f => f.Create() == expected);
            var sut = CreateSut(nodeCollectionFactory: nodeCollectionFactory);
            var dict = sut.GetFormNodeCollections(stat);
            sut.GetFormNodeCollection(stat, Form.More);

            var actual = dict[Form.More];

            Assert.AreSame(expected, actual);
        }

        [Test]
        public void RemoveNodeRemoves()
        {
            var stat = new StatStub();
            var nodeFactory = Mock.Of<INodeFactory>(f => f.Create(stat, NodeType.Base) == MockNodeViewProvider());
            var sut = CreateSut(nodeFactory);
            sut.GetNode(stat, NodeType.Base);

            sut.RemoveNode(stat, NodeType.Base);

            CollectionAssert.IsEmpty(sut.GetNodes(stat));
        }

        [Test]
        public void RemoveNodeDoesNothingIfStatIsUnknown()
        {
            var sut = CreateSut();

            sut.RemoveNode(new StatStub(), NodeType.Base);
        }

        [Test]
        public void RemoveNodesDisposesNode()
        {
            var nodeMock = new Mock<ISuspendableEventViewProvider<ICalculationNode>>();
            nodeMock.Setup(p => p.DefaultView.Dispose()).Verifiable();
            nodeMock.Setup(p => p.SuspendableView.Dispose()).Verifiable();
            var stat = new StatStub();
            var nodeFactory = Mock.Of<INodeFactory>(f => f.Create(stat, NodeType.Base) == nodeMock.Object);
            var sut = CreateSut(nodeFactory);
            sut.GetNode(stat, NodeType.Base);

            sut.RemoveNode(stat, NodeType.Base);

            nodeMock.Verify();
        }

        [Test]
        public void RemoveNodeDoesNothingIfNodeTypeIsUnknown()
        {
            var stat = new StatStub();
            var nodeFactory = Mock.Of<INodeFactory>(f => f.Create(stat, NodeType.BaseAdd) == MockNodeViewProvider());
            var sut = CreateSut(nodeFactory);
            sut.GetNode(stat, NodeType.BaseAdd);

            sut.RemoveNode(stat, NodeType.Base);
        }

        [Test]
        public void RemoveFormNodeCollectionRemoves()
        {
            var stat = new StatStub();
            var nodeCollectionFactory =
                Mock.Of<INodeCollectionFactory>(f => f.Create() == MockModifierNodeCollection());
            var sut = CreateSut(nodeCollectionFactory: nodeCollectionFactory);
            sut.GetFormNodeCollection(stat, Form.More);

            sut.RemoveFormNodeCollection(stat, Form.More);

            CollectionAssert.IsEmpty(sut.GetFormNodeCollections(stat));
        }

        [Test]
        public void RemoveFormNodeCollectionDoesNothingIfStatIsUnknown()
        {
            var sut = CreateSut();

            sut.RemoveFormNodeCollection(new StatStub(), Form.More);
        }

        [Test]
        public void RemoveStatThrowsIfStatHasSubgraphNode()
        {
            var stat = new StatStub();
            var nodeFactory = Mock.Of<INodeFactory>(f => f.Create(stat, NodeType.Base) == MockNodeViewProvider());
            var sut = CreateSut(nodeFactory);
            sut.GetNode(stat, NodeType.Base);

            Assert.Throws<ArgumentException>(() => sut.RemoveStat(stat));
        }

        [Test]
        public void RemoveStatThrowsIfStatHasFormCollection()
        {
            var stat = new StatStub();
            var nodeCollectionFactory =
                Mock.Of<INodeCollectionFactory>(f => f.Create() == MockModifierNodeCollection());
            var sut = CreateSut(nodeCollectionFactory: nodeCollectionFactory);
            sut.GetFormNodeCollection(stat, Form.More);

            Assert.Throws<ArgumentException>(() => sut.RemoveStat(stat));
        }

        [Test]
        public void RemoveStatRemovesNodeDictionary()
        {
            var stat = new StatStub();
            var nodeFactory = Mock.Of<INodeFactory>(f => f.Create(stat, NodeType.Base) == MockNodeViewProvider());
            var sut = CreateSut(nodeFactory);
            sut.GetNode(stat, NodeType.Base);
            sut.RemoveNode(stat, NodeType.Base);
            var unexpected = sut.GetNodes(stat);

            sut.RemoveStat(stat);

            var actual = sut.GetNodes(stat);
            Assert.AreNotSame(unexpected, actual);
        }

        [Test]
        public void RemoveStatRemovesFormNodeDictionary()
        {
            var stat = new StatStub();
            var nodeCollectionFactory =
                Mock.Of<INodeCollectionFactory>(f => f.Create() == MockModifierNodeCollection());
            var sut = CreateSut(nodeCollectionFactory: nodeCollectionFactory);
            sut.GetFormNodeCollection(stat, Form.More);
            sut.RemoveFormNodeCollection(stat, Form.More);
            var unexpected = sut.GetFormNodeCollections(stat);

            sut.RemoveStat(stat);

            var actual = sut.GetFormNodeCollections(stat);
            Assert.AreNotSame(unexpected, actual);
        }

        [Test]
        public void AddModifierAddsCorrectly()
        {
            var stat = new StatStub();
            var value = Mock.Of<IValue>();
            var modifier = new Modifier(new[] { stat }, Form.BaseAdd, value);

            var node = MockNodeViewProvider();
            var nodeFactory = Mock.Of<INodeFactory>(f => f.Create(value) == node);
            var collection = CreateModifierNodeCollection();
            var collectionFactory = Mock.Of<INodeCollectionFactory>(f => f.Create() == collection);
            var sut = CreateSut(nodeFactory, collectionFactory);

            sut.AddModifier(stat, modifier);

            Assert.That(collection.DefaultView,
                Has.Exactly(1).Items.SameAs(node.DefaultView));
        }

        [Test]
        public void RemoveModifierRemovesCorrectly()
        {
            var stat = new StatStub();
            var value = Mock.Of<IValue>();
            var modifier = new Modifier(new[] { stat }, Form.BaseAdd, value);

            var node = MockNodeViewProvider();
            var nodeFactory = Mock.Of<INodeFactory>(f => f.Create(value) == node);
            var collection = CreateModifierNodeCollection();
            var collectionFactory = Mock.Of<INodeCollectionFactory>(f => f.Create() == collection);
            var sut = CreateSut(nodeFactory, collectionFactory);
            sut.AddModifier(stat, modifier);

            var actual = sut.RemoveModifier(stat, modifier);

            CollectionAssert.IsEmpty(collection.DefaultView);
            Assert.IsTrue(actual);
        }

        [Test]
        public void RemoveModifierDisposesNode()
        {
            var stat = new StatStub();
            var value = Mock.Of<IValue>();
            var modifier = new Modifier(new[] { stat }, Form.BaseAdd, value);

            var node = MockNodeViewProvider();
            var nodeFactory = Mock.Of<INodeFactory>(f => f.Create(value) == node);
            var collectionFactory = Mock.Of<INodeCollectionFactory>(f => f.Create() == CreateModifierNodeCollection());
            var sut = CreateSut(nodeFactory, collectionFactory);
            sut.AddModifier(stat, modifier);

            sut.RemoveModifier(stat, modifier);

            Mock.Get(node.DefaultView).Verify(n => n.Dispose());
            Mock.Get(node.SuspendableView).Verify(n => n.Dispose());
        }

        [Test]
        public void RemoveReturnsFalseIfModifierIsUnknown()
        {
            var stat = new StatStub();
            var value = Mock.Of<IValue>();
            var modifier = new Modifier(new[] { stat }, Form.BaseAdd, value);

            var collectionFactory = Mock.Of<INodeCollectionFactory>(f => f.Create() == CreateModifierNodeCollection());
            var sut = CreateSut(nodeCollectionFactory: collectionFactory);

            var actual = sut.RemoveModifier(stat, modifier);

            Assert.IsFalse(actual);
        }

        private static CoreCalculationGraph CreateSut(
            INodeFactory nodeFactory = null, INodeCollectionFactory nodeCollectionFactory = null) =>
            new CoreCalculationGraph(nodeFactory, nodeCollectionFactory);

        private static ISuspendableEventViewProvider<ICalculationNode> MockNodeViewProvider() =>
            Mock.Of<ISuspendableEventViewProvider<ICalculationNode>>(p => 
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