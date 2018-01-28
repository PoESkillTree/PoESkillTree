using Moq;
using NUnit.Framework;
using PoESkillTree.Computation.Common;

namespace PoESkillTree.Computation.Core.Tests
{
    [TestFixture]
    public class NodeRepositoryViewProviderTest
    {
        [Test]
        public void SuspenderReturnsInjectedSuspender()
        {
            var suspender = Mock.Of<ISuspendableEvents>();
            var providerRepository = Mock.Of<INodeViewProviderRepository>(
                r => r.Suspender == suspender);
            var sut = new NodeRepositoryViewProvider(providerRepository);

            var actual = sut.Suspender;

            Assert.AreSame(suspender, actual);
        }

        [Test]
        public void DefaultViewGetNodeReturnsInjectedGetNodesDefaultView()
        {
            var expected = Mock.Of<ICalculationNode>();
            var stat = Mock.Of<IStat>();
            var providerRepository = Mock.Of<INodeViewProviderRepository>(
                r => r.GetNode(stat, NodeType.Base).DefaultView == expected);
            var sut = new NodeRepositoryViewProvider(providerRepository);

            var actual = sut.DefaultView.GetNode(stat, NodeType.Base);

            Assert.AreSame(expected, actual);
        }

        [Test]
        public void SuspendableViewGetNodeReturnsInjectedGetNodesSuspendableView()
        {
            var expected = Mock.Of<ICalculationNode>();
            var stat = Mock.Of<IStat>();
            var providerRepository = Mock.Of<INodeViewProviderRepository>(
                r => r.GetNode(stat, NodeType.Total).SuspendableView == expected);
            var sut = new NodeRepositoryViewProvider(providerRepository);

            var actual = sut.SuspendableView.GetNode(stat);

            Assert.AreSame(expected, actual);
        }

        [Test]
        public void DefaultViewGetFormNodeCollectionReturnsInjectedGetFormNodeCollectionsDefaultView()
        {
            var expected = Mock.Of<INodeCollection<Modifier>>();
            var stat = Mock.Of<IStat>();
            var providerRepository = Mock.Of<INodeViewProviderRepository>(
                r => r.GetFormNodeCollection(stat, Form.BaseAdd).DefaultView == expected);
            var sut = new NodeRepositoryViewProvider(providerRepository);

            var actual = sut.DefaultView.GetFormNodeCollection(stat, Form.BaseAdd);

            Assert.AreSame(expected, actual);
        }

        [Test]
        public void SuspendableViewGetFormNodeCollectionReturnsInjectedGetFormNodeCollectionsSuspendableView()
        {
            var expected = Mock.Of<INodeCollection<Modifier>>();
            var stat = Mock.Of<IStat>();
            var providerRepository = Mock.Of<INodeViewProviderRepository>(
                r => r.GetFormNodeCollection(stat, Form.BaseAdd).SuspendableView == expected);
            var sut = new NodeRepositoryViewProvider(providerRepository);

            var actual = sut.SuspendableView.GetFormNodeCollection(stat, Form.BaseAdd);

            Assert.AreSame(expected, actual);
        }
    }
}