using System.Collections.Generic;
using System.Linq;
using Moq;
using NUnit.Framework;
using PoESkillTree.Computation.Common;

namespace PoESkillTree.Computation.Core.Tests
{
    [TestFixture]
    public class NodeRepositoryViewProviderTest
    {
        [Test]
        public void DefaultViewGetNodeReturnsInjectedGetNodesDefaultView()
        {
            var expected = Mock.Of<ICalculationNode>();
            var stat = Mock.Of<IStat>();
            var graphCollection = Mock.Of<IReadOnlyStatGraphCollection>(
                c => c.GetOrAdd(stat).GetNode(NodeType.Base).DefaultView == expected);
            var sut = new NodeRepositoryViewProvider(graphCollection);

            var actual = sut.DefaultView.GetNode(stat, NodeType.Base);

            Assert.AreSame(expected, actual);
        }

        [Test]
        public void SuspendableViewGetNodeReturnsInjectedGetNodesSuspendableView()
        {
            var expected = Mock.Of<ICalculationNode>();
            var stat = Mock.Of<IStat>();
            var graphCollection = Mock.Of<IReadOnlyStatGraphCollection>(
                c => c.GetOrAdd(stat).GetNode(NodeType.Total).SuspendableView == expected);
            var sut = new NodeRepositoryViewProvider(graphCollection);

            var actual = sut.SuspendableView.GetNode(stat);

            Assert.AreSame(expected, actual);
        }

        [Test]
        public void DefaultViewGetFormNodeCollectionReturnsInjectedGetFormNodeCollectionsDefaultView()
        {
            var expected = Mock.Of<INodeCollection<Modifier>>();
            var stat = Mock.Of<IStat>();
            var graphCollection = Mock.Of<IReadOnlyStatGraphCollection>(
                c => c.GetOrAdd(stat).GetFormNodeCollection(Form.BaseAdd).DefaultView == expected);
            var sut = new NodeRepositoryViewProvider(graphCollection);

            var actual = sut.DefaultView.GetFormNodeCollection(stat, Form.BaseAdd);

            Assert.AreSame(expected, actual);
        }

        [Test]
        public void SuspendableViewGetFormNodeCollectionReturnsInjectedGetFormNodeCollectionsSuspendableView()
        {
            var expected = Mock.Of<INodeCollection<Modifier>>();
            var stat = Mock.Of<IStat>();
            var graphCollection = Mock.Of<IReadOnlyStatGraphCollection>(
                c => c.GetOrAdd(stat).GetFormNodeCollection(Form.BaseAdd).SuspendableView == expected);
            var sut = new NodeRepositoryViewProvider(graphCollection);

            var actual = sut.SuspendableView.GetFormNodeCollection(stat, Form.BaseAdd);

            Assert.AreSame(expected, actual);
        }

        [Test]
        public void SuspenderSuspendEventsIteratesStatGraphCollection()
        {
            var mocks = new List<Mock>();
            var graphCollection = MockStatGraphCollection(mocks, false);
            var sut = new NodeRepositoryViewProvider(graphCollection);

            sut.Suspender.SuspendEvents();

            mocks.ForEach(m => m.Verify());
        }

        [Test]
        public void SuspenderResumeEventsIteratesStatGraphCollection()
        {
            var mocks = new List<Mock>();
            var graphCollection = MockStatGraphCollection(mocks, true);
            var sut = new NodeRepositoryViewProvider(graphCollection);

            sut.Suspender.ResumeEvents();

            mocks.ForEach(m => m.Verify());
        }

        private static IReadOnlyStatGraphCollection MockStatGraphCollection(List<Mock> mocks, bool setupResume)
        {
            var graphs = Enumerable.Range(0, 3).Select(_ => MockStatGraph(mocks, setupResume)).ToList();
            var graphCollectionMock = new Mock<IReadOnlyStatGraphCollection>();
            graphCollectionMock.Setup(c => c.GetEnumerator()).Returns(() => graphs.GetEnumerator());
            return graphCollectionMock.Object;
        }

        private static IReadOnlyStatGraph MockStatGraph(List<Mock> mocks, bool setupResume)
        {
            var nodes = new Dictionary<NodeType, ISuspendableEventViewProvider<ICalculationNode>>
            {
                { NodeType.Base, MockProvider<ICalculationNode>(mocks, setupResume) },
                { NodeType.Total, MockProvider<ICalculationNode>(mocks, setupResume) },
                { NodeType.More, MockProvider<ICalculationNode>(mocks, setupResume) },
            };
            var formCollections = new Dictionary<Form, ISuspendableEventViewProvider<INodeCollection<Modifier>>>
            {
                { Form.BaseAdd, MockProvider<INodeCollection<Modifier>>(mocks, setupResume) },
                { Form.TotalOverride, MockProvider<INodeCollection<Modifier>>(mocks, setupResume) },
            };
            return Mock.Of<IReadOnlyStatGraph>(g => g.Nodes == nodes && g.FormNodeCollections == formCollections);
        }

        private static ISuspendableEventViewProvider<T> MockProvider<T>(List<Mock> mocks, bool setupResume)
        {
            var mock = new Mock<ISuspendableEventViewProvider<T>>();
            if (setupResume)
                mock.Setup(p => p.Suspender.ResumeEvents()).Verifiable();
            else
                mock.Setup(p => p.Suspender.SuspendEvents()).Verifiable();
            mocks.Add(mock);
            return mock.Object;
        }
    }
}