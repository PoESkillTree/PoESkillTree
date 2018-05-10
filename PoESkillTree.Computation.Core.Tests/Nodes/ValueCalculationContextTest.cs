using System.Linq;
using Moq;
using NUnit.Framework;
using PoESkillTree.Computation.Common;
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

            var actual = sut.GetValue(stat, NodeType.Base);

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

            var actual = sut.GetValues(Form.Increase, stat);

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

            var actual = sut.GetValues(Form.More, stats);

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

            sut.GetValue(stats[0], NodeType.Base);
            sut.GetValues(Form.Increase, stats);
            var actual = sut.UsedNodes;

            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void ClearClearsUsedNodes()
        {
            var stat = new StatStub();
            var nodeRepository = Mock.Of<INodeRepository>(r => r.GetNode(stat, NodeType.Total, Path) == MockNode(0));
            var sut = CreateSut(nodeRepository);
            sut.GetValue(stat);

            sut.Clear();

            CollectionAssert.IsEmpty(sut.UsedNodes);
        }

        [Test]
        public void UsedNodeCollectionsReturnsCorrectResult()
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
            
            sut.GetValues(Form.Increase, stats);
            var actual = sut.UsedNodeCollections;

            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void ClearClearsUsedNodeCollections()
        {
            var stat = new StatStub();
            var nodeRepository =
                Mock.Of<INodeRepository>(r => r.GetFormNodeCollection(stat, Form.Increase, Path) == MockNodeCollection());
            var sut = CreateSut(nodeRepository);
            sut.GetValues(Form.Increase, stat);

            sut.Clear();

            CollectionAssert.IsEmpty(sut.UsedNodeCollections);
        }

        [Test]
        public void GetValueReturnsNullIfStatIsNull()
        {
            var sut = CreateSut();

            var actual = sut.GetValue(null);

            Assert.IsNull(actual);
        }


        private static ValueCalculationContext CreateSut(INodeRepository nodeRepository = null) =>
            new ValueCalculationContext(nodeRepository);

        private static INodeCollection<Modifier> MockNodeCollection(params ICalculationNode[] nodes)
        {
            var mock = new Mock<INodeCollection<Modifier>>();
            mock.Setup(c => c.GetEnumerator())
                .Returns(() => nodes.Select(n => (n, (Modifier) null)).AsEnumerable().GetEnumerator());
            return mock.Object;
        }

        private static readonly PathDefinition Path = PathDefinition.MainPath;
    }
}