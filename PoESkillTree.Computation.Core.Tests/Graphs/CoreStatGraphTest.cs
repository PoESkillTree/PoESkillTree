using Moq;
using NUnit.Framework;
using PoESkillTree.Computation.Common;
using PoESkillTree.Computation.Common.Tests;
using PoESkillTree.Computation.Core.Events;
using PoESkillTree.Computation.Core.Graphs;
using PoESkillTree.Computation.Core.NodeCollections;
using PoESkillTree.Computation.Core.Nodes;
using static PoESkillTree.Computation.Common.Tests.Helper;
using static PoESkillTree.Computation.Core.Tests.NodeHelper;
using static PoESkillTree.Computation.Core.Tests.Graphs.NodeSelectorHelper;

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
            var expected = MockDisposableNodeProvider();
            var selector = Selector(NodeType.Base);
            var nodeFactory = Mock.Of<IStatNodeFactory>(f => f.Create(selector) == expected);
            var sut = CreateSut(nodeFactory);

            var actual = sut.GetNode(selector);

            Assert.AreSame(expected, actual);
        }

        [Test]
        public void GetNodeCachesResult()
        {
            var expected = MockDisposableNodeProvider();
            var selector = Selector(NodeType.Base);
            var nodeFactoryMock = new Mock<IStatNodeFactory>();
            nodeFactoryMock.Setup(f => f.Create(selector)).Returns(expected);
            var sut = CreateSut(nodeFactoryMock.Object);
            sut.GetNode(selector);
            nodeFactoryMock.Setup(f => f.Create(selector)).Returns(MockDisposableNodeProvider);

            var actual = sut.GetNode(selector);

            Assert.AreSame(expected, actual);
        }

        [Test]
        public void GetFormNodecollectionReturnsInjectedNodeCollectionFactoryGetFormNodeCollection()
        {
            var expected = MockModifierNodeCollection();
            var selector = Selector(Form.More);
            var nodeFactory = Mock.Of<IStatNodeFactory>(f => f.Create(selector) == expected);
            var sut = CreateSut(nodeFactory);

            var actual = sut.GetFormNodeCollection(selector);

            Assert.AreSame(expected, actual);
        }

        [Test]
        public void GetFormNodeCollectionCachesResult()
        {
            var expected = MockModifierNodeCollection();
            var selector = Selector(Form.More);
            var factoryMock = new Mock<IStatNodeFactory>();
            factoryMock.Setup(f => f.Create(selector)).Returns(expected);
            var sut = CreateSut(factoryMock.Object);
            sut.GetFormNodeCollection(selector);
            factoryMock.Setup(f => f.Create(selector)).Returns(MockModifierNodeCollection);

            var actual = sut.GetFormNodeCollection(selector);

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
            var expected = MockDisposableNodeProvider();
            var selector = Selector(NodeType.Base);
            var nodeFactory = Mock.Of<IStatNodeFactory>(f => f.Create(selector) == expected);
            var sut = CreateSut(nodeFactory);
            var dict = sut.Nodes;
            sut.GetNode(selector);

            var actual = dict[selector];

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
            var selector = Selector(Form.More);
            var nodeFactory = Mock.Of<IStatNodeFactory>(f => f.Create(selector) == expected);
            var sut = CreateSut(nodeFactory);
            var dict = sut.FormNodeCollections;
            sut.GetFormNodeCollection(selector);

            var actual = dict[selector];

            Assert.AreSame(expected, actual);
        }

        [Test]
        public void RemoveNodeRemoves()
        {
            var selector = Selector(NodeType.Base);
            var nodeFactory = Mock.Of<IStatNodeFactory>(f => f.Create(selector) == MockDisposableNodeProvider());
            var sut = CreateSut(nodeFactory);
            sut.GetNode(selector);

            sut.RemoveNode(selector);

            CollectionAssert.IsEmpty(sut.Nodes);
        }

        [Test]
        public void RemoveNodesDisposesNode()
        {
            var selector = Selector(NodeType.Base);
            var nodeMock = new Mock<IDisposableNodeViewProvider>();
            var nodeFactory = Mock.Of<IStatNodeFactory>(f => f.Create(selector) == nodeMock.Object);
            var sut = CreateSut(nodeFactory);
            sut.GetNode(selector);

            sut.RemoveNode(selector);

            nodeMock.Verify(p => p.Dispose());
        }

        [Test]
        public void RemoveNodeDoesNothingIfNodeTypeIsUnknown()
        {
            var selector = Selector(NodeType.BaseAdd);
            var nodeFactory = Mock.Of<IStatNodeFactory>(f => f.Create(selector) == MockDisposableNodeProvider());
            var sut = CreateSut(nodeFactory);
            sut.GetNode(selector);

            sut.RemoveNode(Selector(NodeType.Base));
        }

        [Test]
        public void RemoveFormNodeCollectionRemoves()
        {
            var selector = Selector(Form.More);
            var nodeFactory =
                Mock.Of<IStatNodeFactory>(f => f.Create(selector) == MockModifierNodeCollection());
            var sut = CreateSut(nodeFactory);
            sut.GetFormNodeCollection(selector);

            sut.RemoveFormNodeCollection(selector);

            CollectionAssert.IsEmpty(sut.FormNodeCollections);
        }

        [Test]
        public void AddModifierAddsCorrectly()
        {
            var modifier = MockModifier();

            var node = MockNodeProvider();
            var collection = CreateModifierNodeCollection();
            var factory = Mock.Of<IStatNodeFactory>(f => f.Create(Selector(modifier.Form)) == collection);
            var sut = CreateSut(factory);

            sut.AddModifier(node, modifier);

            Assert.That(collection.DefaultView,
                Has.Exactly(1).Items.EqualTo((node.DefaultView, modifier)));
        }

        [Test]
        public void RemoveModifierRemovesCorrectly()
        {
            var modifier = MockModifier();

            var node = MockNodeProvider();
            var collection = CreateModifierNodeCollection();
            var factory = Mock.Of<IStatNodeFactory>(f => f.Create(Selector(modifier.Form)) == collection);
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
                MockModifier(form: Form.More, value: values[0]), MockModifier(form: Form.More, value: values[1])
            };

            var nodes = new[] { MockNodeProvider(), MockNodeProvider() };
            var factory = Mock.Of<IStatNodeFactory>(f => f.Create(Selector(Form.More)) == CreateModifierNodeCollection());
            var sut = CreateSut(factory);

            sut.AddModifier(nodes[0], modifiers[0]);
            sut.AddModifier(nodes[1], modifiers[1]);
            sut.RemoveModifier(nodes[0], modifiers[0]);
            var actual = sut.ModifierCount;

            Assert.AreEqual(1, actual);
        }

        [Test]
        public void PathsIsInjectedInstance()
        {
            var paths = CreatePathDefinitionCollection();
            var sut = CreateSut(paths: paths);

            Assert.AreSame(paths, sut.Paths);
        }

        [Test]
        public void GetNodeAddsPath()
        {
            var selector = Selector(NodeType.Base);
            var paths = CreatePathDefinitionCollection();
            var sut = CreateSut(paths: paths);

            sut.GetNode(selector);

            CollectionAssert.Contains(paths.DefaultView, selector.Path);
        }

        [Test]
        public void RemoveNodeRemovesPath()
        {
            var selector = Selector(NodeType.Base);
            var paths = CreatePathDefinitionCollection();
            var nodeFactory = Mock.Of<IStatNodeFactory>(f => f.Create(selector) == MockDisposableNodeProvider());
            var sut = CreateSut(nodeFactory, paths);
            sut.GetNode(selector);

            sut.RemoveNode(selector);

            CollectionAssert.IsEmpty(paths.DefaultView);
        }

        [Test]
        public void GetFormNodeCollectionAddsPath()
        {
            var selector = Selector(Form.More);
            var paths = CreatePathDefinitionCollection();
            var sut = CreateSut(paths: paths);

            sut.GetFormNodeCollection(selector);

            CollectionAssert.Contains(paths.DefaultView, selector.Path);
        }

        [Test]
        public void RemoveFormNodeCollectionRemovesPath()
        {
            var selector = Selector(Form.More);
            var paths = CreatePathDefinitionCollection();
            var sut = CreateSut(paths: paths);
            sut.GetFormNodeCollection(selector);

            sut.RemoveFormNodeCollection(selector);

            CollectionAssert.IsEmpty(paths.DefaultView);
        }

        [Test]
        public void AddModifierUsesCorrectPath()
        {
            var node = MockNodeProvider();
            var canonicalSource = new ModifierSourceStub();
            var modifier = MockModifier(source: new ModifierSourceStub { CanonicalSource = canonicalSource });
            var collection = CreateModifierNodeCollection();
            var selector = new FormNodeSelector(modifier.Form, new PathDefinition(canonicalSource));
            var factory = Mock.Of<IStatNodeFactory>(f => f.Create(selector) == collection);
            var sut = CreateSut(factory);

            sut.AddModifier(node, modifier);

            CollectionAssert.Contains(collection.DefaultView, (node.DefaultView, modifier));
        }

        private static CoreStatGraph CreateSut(
            IStatNodeFactory nodeFactory = null, PathDefinitionCollection paths = null) =>
            new CoreStatGraph(nodeFactory ?? Mock.Of<IStatNodeFactory>(), paths ?? CreatePathDefinitionCollection());

        private static ModifierNodeCollection MockModifierNodeCollection()
        {
            var nodeCollectionViewProvider = Mock.Of<ISuspendableEventViewProvider<NodeCollection<Modifier>>>();
            return new ModifierNodeCollection(nodeCollectionViewProvider);
        }

        private static ModifierNodeCollection CreateModifierNodeCollection()
        {
            var defaultView = new NodeCollection<Modifier>();
            var suspendableView = new NodeCollection<Modifier>();
            var nodeCollectionViewProvider = SuspendableEventViewProvider.Create(defaultView, suspendableView);
            return new ModifierNodeCollection(nodeCollectionViewProvider);
        }

        private static PathDefinitionCollection CreatePathDefinitionCollection()
        {
            var viewProvider = SuspendableEventViewProvider.Create(new ObservableCollection<PathDefinition>(),
                new SuspendableObservableCollection<PathDefinition>());
            return new PathDefinitionCollection(viewProvider);
        }
    }
}