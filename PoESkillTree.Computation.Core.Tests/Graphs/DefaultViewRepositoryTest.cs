using Moq;
using NUnit.Framework;
using PoESkillTree.Computation.Common;

namespace PoESkillTree.Computation.Core.Graphs
{
    [TestFixture]
    public class DefaultViewRepositoryTest
    {
        [Test]
        public void GetNodeReturnsInjectedGetNodesDefaultView()
        {
            var expected = Mock.Of<ICalculationNode>();
            var stat = Mock.Of<IStat>();
            var path = PathDefinition.MainPath;
            var graphCollection = Mock.Of<IStatGraphCollection>(
                c => c.GetOrAdd(stat).GetNode(new NodeSelector(NodeType.Base, path)).DefaultView == expected);
            var sut = new DefaultViewNodeRepository(graphCollection);

            var actual = sut.GetNode(stat, NodeType.Base, path);

            Assert.AreSame(expected, actual);
        }

        [Test]
        public void GetFormNodeCollectionReturnsInjectedGetFormNodeCollectionsDefaultView()
        {
            var expected = Mock.Of<INodeCollection<Modifier>>();
            var stat = Mock.Of<IStat>();
            var path = PathDefinition.MainPath;
            var graphCollection = Mock.Of<IStatGraphCollection>(
                c => c.GetOrAdd(stat).GetFormNodeCollection(new FormNodeSelector(Form.BaseAdd, path)).DefaultView 
                     == expected);
            var sut = new DefaultViewNodeRepository(graphCollection);

            var actual = sut.GetFormNodeCollection(stat, Form.BaseAdd, path);

            Assert.AreSame(expected, actual);
        }

        [Test]
        public void GetPathReturnsInjectedPathsDefaultView()
        {
            var expected = Mock.Of<IObservableCollection<PathDefinition>>();
            var stat = Mock.Of<IStat>();
            var graphCollection = Mock.Of<IStatGraphCollection>(
                c => c.GetOrAdd(stat).Paths.DefaultView == expected);
            var sut = new DefaultViewNodeRepository(graphCollection);

            var actual = sut.GetPaths(stat);

            Assert.AreSame(actual, expected);
        }
    }
}