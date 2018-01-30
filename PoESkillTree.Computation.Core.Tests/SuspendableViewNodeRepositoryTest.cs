using Moq;
using NUnit.Framework;
using PoESkillTree.Computation.Common;

namespace PoESkillTree.Computation.Core.Tests
{
    [TestFixture]
    public class SuspendableViewNodeRepositoryTest
    {
        [Test]
        public void SuspendableViewGetNodeReturnsInjectedGetNodesSuspendableView()
        {
            var expected = Mock.Of<ICalculationNode>();
            var stat = Mock.Of<IStat>();
            var graphCollection = Mock.Of<IReadOnlyStatGraphCollection>(
                c => c.GetOrAdd(stat).GetNode(NodeType.Total).SuspendableView == expected);
            var sut = new SuspendableViewNodeRepository(graphCollection);

            var actual = sut.GetNode(stat, NodeType.Total);

            Assert.AreSame(expected, actual);
        }

        [Test]
        public void SuspendableViewGetFormNodeCollectionReturnsInjectedGetFormNodeCollectionsSuspendableView()
        {
            var expected = Mock.Of<INodeCollection<Modifier>>();
            var stat = Mock.Of<IStat>();
            var graphCollection = Mock.Of<IReadOnlyStatGraphCollection>(
                c => c.GetOrAdd(stat).GetFormNodeCollection(Form.BaseAdd).SuspendableView == expected);
            var sut = new SuspendableViewNodeRepository(graphCollection);

            var actual = sut.GetFormNodeCollection(stat, Form.BaseAdd);

            Assert.AreSame(expected, actual);
        }
    }
}