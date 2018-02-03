using Moq;
using NUnit.Framework;
using PoESkillTree.Computation.Common;
using PoESkillTree.Computation.Core.Events;
using PoESkillTree.Computation.Core.Graphs;
using PoESkillTree.Computation.Core.NodeCollections;
using PoESkillTree.Computation.Core.Nodes;
using static PoESkillTree.Computation.Core.Tests.NodeHelper;

namespace PoESkillTree.Computation.Core.Tests.Graphs
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
            var expected = MockNodeProvider();
            var nodeFactory = Mock.Of<IStatNodeFactory>(f => f.Create(NodeType.Base) == expected);
            var sut = CreateSut(nodeFactory);

            var actual = sut.GetNode(NodeType.Base);

            Assert.AreSame(expected, actual);
        }

        [Test]
        public void GetNodeCachesResult()
        {
            var expected = MockNodeProvider();
            var nodeFactoryMock = new Mock<IStatNodeFactory>();
            nodeFactoryMock.Setup(f => f.Create(NodeType.Base)).Returns(expected);
            var sut = CreateSut(nodeFactoryMock.Object);
            sut.GetNode(NodeType.Base);
            nodeFactoryMock.Setup(f => f.Create(NodeType.Base)).Returns(MockNodeProvider);

            var actual = sut.GetNode(NodeType.Base);

            Assert.AreSame(expected, actual);
        }

        [Test]
        public void GetFormNodecollectionReturnsInjectedNodeCollectionFactoryGetFormNodeCollection()
        {
            var expected = MockModifierNodeCollection();
            var nodeFactory = Mock.Of<IStatNodeFactory>(f => f.Create(Form.More) == expected);
            var sut = CreateSut(nodeFactory);

            var actual = sut.GetFormNodeCollection(Form.More);

            Assert.AreSame(expected, actual);
        }

        [Test]
        public void GetFormNodeCollectionCachesResult()
        {
            var expected = MockModifierNodeCollection();
            var factoryMock = new Mock<IStatNodeFactory>();
            factoryMock.Setup(f => f.Create(Form.More)).Returns(expected);
            var sut = CreateSut(factoryMock.Object);
            sut.GetFormNodeCollection(Form.More);
            factoryMock.Setup(f => f.Create(Form.More)).Returns(MockModifierNodeCollection);

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
            var expected = MockNodeProvider();
            var nodeFactory = Mock.Of<IStatNodeFactory>(f => f.Create(NodeType.Base) == expected);
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
            var nodeFactory = Mock.Of<IStatNodeFactory>(f => f.Create(Form.More) == expected);
            var sut = CreateSut(nodeFactory);
            var dict = sut.FormNodeCollections;
            sut.GetFormNodeCollection(Form.More);

            var actual = dict[Form.More];

            Assert.AreSame(expected, actual);
        }

        [Test]
        public void RemoveNodeRemoves()
        {
            var nodeFactory = Mock.Of<IStatNodeFactory>(f => f.Create(NodeType.Base) == MockNodeProvider());
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
            var nodeFactory = Mock.Of<IStatNodeFactory>(f => f.Create(NodeType.Base) == nodeMock.Object);
            var sut = CreateSut(nodeFactory);
            sut.GetNode(NodeType.Base);

            sut.RemoveNode(NodeType.Base);

            nodeMock.Verify();
        }

        [Test]
        public void RemoveNodeDoesNothingIfNodeTypeIsUnknown()
        {
            var nodeFactory = Mock.Of<IStatNodeFactory>(f => f.Create(NodeType.BaseAdd) == MockNodeProvider());
            var sut = CreateSut(nodeFactory);
            sut.GetNode(NodeType.BaseAdd);

            sut.RemoveNode(NodeType.Base);
        }

        [Test]
        public void RemoveFormNodeCollectionRemoves()
        {
            var nodeFactory =
                Mock.Of<IStatNodeFactory>(f => f.Create(Form.More) == MockModifierNodeCollection());
            var sut = CreateSut(nodeFactory);
            sut.GetFormNodeCollection(Form.More);

            sut.RemoveFormNodeCollection(Form.More);

            CollectionAssert.IsEmpty(sut.FormNodeCollections);
        }

        [Test]
        public void AddModifierAddsCorrectly()
        {
            var value = Mock.Of<IValue>();
            var modifier = new Modifier(new IStat[0], Form.BaseAdd, value);

            var node = MockNodeProvider();
            var collection = CreateModifierNodeCollection();
            var factory = Mock.Of<IStatNodeFactory>(f => f.Create(Form.BaseAdd) == collection);
            var sut = CreateSut(factory);

            sut.AddModifier(node, modifier);

            Assert.That(collection.DefaultView,
                Has.Exactly(1).Items.SameAs(node.DefaultView));
        }

        [Test]
        public void RemoveModifierRemovesCorrectly()
        {
            var value = Mock.Of<IValue>();
            var modifier = new Modifier(new IStat[0], Form.BaseAdd, value);

            var node = MockNodeProvider();
            var collection = CreateModifierNodeCollection();
            var factory = Mock.Of<IStatNodeFactory>(f => f.Create(Form.BaseAdd) == collection);
            var sut = CreateSut(factory);
            sut.AddModifier(node, modifier);

            sut.RemoveModifier(node, modifier);

            CollectionAssert.IsEmpty(collection.DefaultView);
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

            var nodes = new[] { MockNodeProvider(), MockNodeProvider() };
            var factory = Mock.Of<IStatNodeFactory>(f => f.Create(Form.More) == CreateModifierNodeCollection());
            var sut = CreateSut(factory);

            sut.AddModifier(nodes[0], modifiers[0]);
            sut.AddModifier(nodes[1], modifiers[1]);
            sut.RemoveModifier(nodes[0], modifiers[0]);
            var actual = sut.ModifierCount;

            Assert.AreEqual(1, actual);
        }

        private static CoreStatGraph CreateSut(IStatNodeFactory nodeFactory = null) =>
            new CoreStatGraph(nodeFactory);

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