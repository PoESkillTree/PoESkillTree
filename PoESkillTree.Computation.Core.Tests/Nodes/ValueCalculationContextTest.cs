using System.Linq;
using Moq;
using NUnit.Framework;
using PoESkillTree.Computation.Common;
using PoESkillTree.Computation.Common.Tests;
using PoESkillTree.Computation.Core.Nodes;
using static PoESkillTree.Computation.Core.Tests.NodeHelper;

namespace PoESkillTree.Computation.Core.Tests.Nodes
{
    [TestFixture]
    public class ValueCalculationContextTest
    {
        [Test]
        public void SutIsValueCalculationContext()
        {
            var sut = CreateSut();

            Assert.IsInstanceOf<IValueCalculationContext>(sut);
        }

        [TestCase(42)]
        [TestCase(-5)]
        public void GetValueReturnsNodeRepositoryValue(double value)
        {
            var expected = new NodeValue(value);
            var node = MockNode(expected);
            var stat = Mock.Of<IStat>();
            var nodeRepository = Mock.Of<INodeRepository>(r => r.GetNode(stat, NodeType.Base, Path) == node);
            var sut = CreateSut(nodeRepository);

            var actual = sut.GetValue(stat, NodeType.Base, Path);

            Assert.AreEqual(expected, actual);
        }

        [TestCase(42)]
        [TestCase(-5)]
        public void GetValuesReturnsNodeRepositoryValue(double value)
        {
            var expected = new NodeValue(value);
            var nodeCollection = MockNodeCollection(MockNode(expected));
            var stat = new StatStub();
            var nodeRepository =
                Mock.Of<INodeRepository>(r => r.GetFormNodeCollection(stat, Form.Increase, Path) == nodeCollection);
            var sut = CreateSut(nodeRepository);

            var actual = sut.GetValues(Form.Increase, stat, Path);

            CollectionAssert.Contains(actual, expected);
        }

        [Test]
        public void GetValuesReturnsCorrectResult()
        {
            var expected = new NodeValue?[] { new NodeValue(0), new NodeValue(1), new NodeValue(2), new NodeValue(3), };
            var nodes = expected.Select(MockNode).ToList();
            var nodeCollections = new[]
            {
                MockNodeCollection(nodes[0], nodes[1]),
                MockNodeCollection(nodes[1], nodes[2]),
                MockNodeCollection(nodes[2], nodes[3]),
            };
            var stats = new IStat[] { new StatStub(), new StatStub(), new StatStub(), };
            var nodeRepositoryMock = new Mock<INodeRepository>();
            for (var i = 0; i < stats.Length; i++)
            {
                var iClosure = i;
                nodeRepositoryMock.Setup(r => r.GetFormNodeCollection(stats[iClosure], Form.More, Path))
                    .Returns(nodeCollections[i]);
            }
            var sut = CreateSut(nodeRepositoryMock.Object);

            var actual = sut.GetValues(Form.More, stats, Path);

            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void UsedNodesReturnsCorrectResult()
        {
            var expected = new[] { MockNode(), MockNode(), MockNode(), MockNode() };
            var nodeCollections = new[]
            {
                MockNodeCollection(expected[1], expected[2]),
                MockNodeCollection(expected[2], expected[3])
            };
            var stats = new IStat[] { new StatStub(), new StatStub(), };
            var nodeRepository =
                Mock.Of<INodeRepository>(r =>
                    r.GetFormNodeCollection(stats[0], Form.Increase, Path) == nodeCollections[0] &&
                    r.GetFormNodeCollection(stats[1], Form.Increase, Path) == nodeCollections[1] &&
                    r.GetNode(stats[0], NodeType.Base, Path) == expected[0]);
            var sut = CreateSut(nodeRepository);

            sut.GetValue(stats[0], NodeType.Base, Path);
            sut.GetValues(Form.Increase, stats, Path);
            var actual = sut.UsedNodes;

            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void ClearClearsUsedNodes()
        {
            var stat = new StatStub();
            var nodeRepository = Mock.Of<INodeRepository>(r => r.GetNode(stat, NodeType.Total, Path) == MockNode(0));
            var sut = CreateSut(nodeRepository);
            sut.GetValue(stat, NodeType.Total, Path);

            sut.Clear();

            CollectionAssert.IsEmpty(sut.UsedNodes);
        }

        [Test]
        public void UsedCollectionsReturnsCorrectResult()
        {
            var expected = new[]
            {
                MockNodeCollection(MockNode()),
                MockNodeCollection(MockNode())
            };
            var stats = new IStat[] { new StatStub(), new StatStub(), };
            var nodeRepository =
                Mock.Of<INodeRepository>(r =>
                    r.GetFormNodeCollection(stats[0], Form.Increase, Path) == expected[0] &&
                    r.GetFormNodeCollection(stats[1], Form.Increase, Path) == expected[1]);
            var sut = CreateSut(nodeRepository);
            
            sut.GetValues(Form.Increase, stats, Path);
            var actual = sut.UsedCollections;

            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void ClearClearsUsedCollections()
        {
            var stat = new StatStub();
            var nodeRepository =
                Mock.Of<INodeRepository>(r => r.GetFormNodeCollection(stat, Form.Increase, Path) == MockNodeCollection());
            var sut = CreateSut(nodeRepository);
            sut.GetValues(Form.Increase, stat, Path);

            sut.Clear();

            CollectionAssert.IsEmpty(sut.UsedCollections);
        }

        [Test]
        public void GetValuesNodeTypeReturnsCorrectResult()
        {
            var nodes = new[] { MockNode(0), MockNode(1) };
            var stat = new StatStub();
            var paths = new[] { Path, PathDefinition.MainPath };
            var nodeRepository = Mock.Of<INodeRepository>(r => 
                r.GetPaths(stat) == MockPathCollection(paths) &&
                r.GetNode(stat, NodeType.Base, paths[0]) == nodes[0] &&
                r.GetNode(stat, NodeType.Base, paths[1]) == nodes[1]);
            var sut = CreateSut(nodeRepository);

            var actual = sut.GetValues(stat, NodeType.Base);

            Assert.AreEqual(nodes.Select(n => n.Value), actual);
        }

        [Test]
        public void GetValuesNodeTypeAddsToUsedCollections()
        {
            var expected = MockPathCollection();
            var stat = new StatStub();
            var nodeRepository = Mock.Of<INodeRepository>(r => 
                r.GetPaths(stat) == expected);
            var sut = CreateSut(nodeRepository);

            sut.GetValues(stat, NodeType.Base);
            
            CollectionAssert.Contains(sut.UsedCollections, expected);
        }

        [Test]
        public void GetValuesNodeTypeAddsToUsedNodes()
        {
            var expected = MockNode();
            var stat = new StatStub();
            var nodeRepository = Mock.Of<INodeRepository>(r => 
                r.GetPaths(stat) == MockPathCollection(Path) &&
                r.GetNode(stat, NodeType.Base, Path) == expected);
            var sut = CreateSut(nodeRepository);

            var values = sut.GetValues(stat, NodeType.Base);
            // Forced enumeration because it makes sense to only add those nodes that were actually enumerated
            foreach (var _ in values);

            CollectionAssert.Contains(sut.UsedNodes, expected);
        }


        private static ValueCalculationContext CreateSut(INodeRepository nodeRepository = null) =>
            new ValueCalculationContext(nodeRepository);

        private static INodeCollection<Modifier> MockNodeCollection(params ICalculationNode[] nodes)
        {
            var mock = new Mock<INodeCollection<Modifier>>();
            mock.Setup(c => c.GetEnumerator())
                .Returns(() => nodes.Select(n => (n, (Modifier) null)).GetEnumerator());
            return mock.Object;
        }

        private static IObservableCollection<PathDefinition> MockPathCollection(params PathDefinition[] paths)
        {
            var mock = new Mock<IObservableCollection<PathDefinition>>();
            mock.Setup(c => c.GetEnumerator())
                .Returns(() => paths.AsEnumerable().GetEnumerator());
            return mock.Object;
        }

        private static readonly PathDefinition Path = NotMainPath;
    }
}