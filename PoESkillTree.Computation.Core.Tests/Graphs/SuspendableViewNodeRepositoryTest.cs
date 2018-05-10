using Moq;
using NUnit.Framework;
using PoESkillTree.Computation.Common;
using PoESkillTree.Computation.Core.Graphs;

namespace PoESkillTree.Computation.Core.Tests.Graphs
{
    [TestFixture]
    public class SuspendableViewNodeRepositoryTest
    {
        [Test]
        public void SuspendableViewGetNodeReturnsInjectedGetNodesSuspendableView()
        {
            var expected = Mock.Of<ICalculationNode>();
            var stat = Mock.Of<IStat>();
            var path = PathDefinition.MainPath;
            var graphCollection = Mock.Of<IReadOnlyStatGraphCollection>(
                c => c.GetOrAdd(stat).GetNode(new NodeSelector(NodeType.Total, path)).SuspendableView == expected);
            var sut = new SuspendableViewNodeRepository(graphCollection);

            var actual = sut.GetNode(stat, NodeType.Total, path);

            Assert.AreSame(expected, actual);
        }

        [Test]
        public void SuspendableViewGetFormNodeCollectionReturnsInjectedGetFormNodeCollectionsSuspendableView()
        {
            var expected = Mock.Of<INodeCollection<Modifier>>();
            var stat = Mock.Of<IStat>();
            var path = PathDefinition.MainPath;
            var graphCollection = Mock.Of<IReadOnlyStatGraphCollection>(
                c => c.GetOrAdd(stat).GetFormNodeCollection(new FormNodeSelector(Form.BaseAdd, path)).SuspendableView 
                     == expected);
            var sut = new SuspendableViewNodeRepository(graphCollection);

            var actual = sut.GetFormNodeCollection(stat, Form.BaseAdd, path);

            Assert.AreSame(expected, actual);
        }

        [Test]
        public void GetPathReturnsInjectedPathsDefaultView()
        {
            var expected = Mock.Of<IObservableCollection<PathDefinition>>();
            var stat = Mock.Of<IStat>();
            var graphCollection = Mock.Of<IReadOnlyStatGraphCollection>(
                c => c.GetOrAdd(stat).Paths.SuspendableView == expected);
            var sut = new SuspendableViewNodeRepository(graphCollection);

            var actual = sut.GetPaths(stat);

            Assert.AreSame(actual, expected);
        }
    }
}