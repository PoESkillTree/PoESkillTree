using System.Collections.Generic;
using System.Linq;
using Moq;
using NUnit.Framework;
using PoESkillTree.Computation.Common;
using PoESkillTree.Computation.Core.Events;
using PoESkillTree.Computation.Core.Graphs;

namespace PoESkillTree.Computation.Core.Tests.Graphs
{
    [TestFixture]
    public class StatGraphCollectionSuspenderTest
    {
        [Test]
        public void SuspenderSuspendEventsIteratesStatGraphCollection()
        {
            var mocks = new List<Mock>();
            var graphCollection = MockStatGraphCollection(mocks, false);
            var sut = new StatGraphCollectionSuspender(graphCollection);

            sut.SuspendEvents();

            mocks.ForEach(m => m.Verify());
        }

        [Test]
        public void SuspenderResumeEventsIteratesStatGraphCollection()
        {
            var mocks = new List<Mock>();
            var graphCollection = MockStatGraphCollection(mocks, true);
            var sut = new StatGraphCollectionSuspender(graphCollection);

            sut.ResumeEvents();

            mocks.ForEach(m => m.Verify());
        }

        private static IEnumerable<IReadOnlyStatGraph> MockStatGraphCollection(List<Mock> mocks, bool setupResume)
        {
            var graphs = Enumerable.Range(0, 3).Select(_ => MockStatGraph(mocks, setupResume)).ToList();
            var graphCollectionMock = new Mock<IEnumerable<IReadOnlyStatGraph>>();
            graphCollectionMock.Setup(c => c.GetEnumerator()).Returns(() => graphs.GetEnumerator());
            return graphCollectionMock.Object;
        }

        private static IReadOnlyStatGraph MockStatGraph(List<Mock> mocks, bool setupResume)
        {
            var nodes = new Dictionary<NodeType, ISuspendableEventViewProvider<ICalculationNode>>
            {
                { NodeType.Base, MockProvider<ICalculationNode>(mocks, setupResume) },
                { NodeType.Total, MockProvider<ICalculationNode>(mocks, setupResume) },
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