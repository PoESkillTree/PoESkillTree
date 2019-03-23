using Moq;
using NUnit.Framework;
using PoESkillTree.Computation.Common;

namespace PoESkillTree.Computation.Core.Graphs
{
    [TestFixture]
    public class BufferingViewNodeRepositoryTest
    {
        [Test]
        public void BufferingViewGetNodeReturnsInjectedGetNodesBufferingView()
        {
            var expected = Mock.Of<ICalculationNode>();
            var stat = Mock.Of<IStat>();
            var path = PathDefinition.MainPath;
            var graphCollection = Mock.Of<IStatGraphCollection>(
                c => c.GetOrAdd(stat).GetNode(new NodeSelector(NodeType.Total, path)).BufferingView == expected);
            var sut = new BufferingViewNodeRepository(graphCollection);

            var actual = sut.GetNode(stat, NodeType.Total, path);

            Assert.AreSame(expected, actual);
        }

        [Test]
        public void BufferingViewGetFormNodeCollectionReturnsInjectedGetFormNodeCollectionsBufferingView()
        {
            var expected = Mock.Of<INodeCollection<Modifier>>();
            var stat = Mock.Of<IStat>();
            var path = PathDefinition.MainPath;
            var graphCollection = Mock.Of<IStatGraphCollection>(
                c => c.GetOrAdd(stat).GetFormNodeCollection(new FormNodeSelector(Form.BaseAdd, path)).BufferingView 
                     == expected);
            var sut = new BufferingViewNodeRepository(graphCollection);

            var actual = sut.GetFormNodeCollection(stat, Form.BaseAdd, path);

            Assert.AreSame(expected, actual);
        }

        [Test]
        public void GetPathReturnsInjectedPathsDefaultView()
        {
            var expected = Mock.Of<IObservableCollection<PathDefinition>>();
            var stat = Mock.Of<IStat>();
            var graphCollection = Mock.Of<IStatGraphCollection>(
                c => c.GetOrAdd(stat).Paths.BufferingView == expected);
            var sut = new BufferingViewNodeRepository(graphCollection);

            var actual = sut.GetPaths(stat);

            Assert.AreSame(actual, expected);
        }
    }
}