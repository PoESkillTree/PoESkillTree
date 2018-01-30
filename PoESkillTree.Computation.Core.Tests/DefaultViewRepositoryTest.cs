using Moq;
using NUnit.Framework;
using PoESkillTree.Computation.Common;

namespace PoESkillTree.Computation.Core.Tests
{
    [TestFixture]
    public class DefaultViewRepositoryTest
    {
        [Test]
        public void GetNodeReturnsInjectedGetNodesDefaultView()
        {
            var expected = Mock.Of<ICalculationNode>();
            var stat = Mock.Of<IStat>();
            var graphCollection = Mock.Of<IReadOnlyStatGraphCollection>(
                c => c.GetOrAdd(stat).GetNode(NodeType.Base).DefaultView == expected);
            var sut = new DefaultViewNodeRepository(graphCollection);

            var actual = sut.GetNode(stat, NodeType.Base);

            Assert.AreSame(expected, actual);
        }

        [Test]
        public void GetFormNodeCollectionReturnsInjectedGetFormNodeCollectionsDefaultView()
        {
            var expected = Mock.Of<INodeCollection<Modifier>>();
            var stat = Mock.Of<IStat>();
            var graphCollection = Mock.Of<IReadOnlyStatGraphCollection>(
                c => c.GetOrAdd(stat).GetFormNodeCollection(Form.BaseAdd).DefaultView == expected);
            var sut = new DefaultViewNodeRepository(graphCollection);

            var actual = sut.GetFormNodeCollection(stat, Form.BaseAdd);

            Assert.AreSame(expected, actual);
        }
    }
}