using System;
using Moq;
using NUnit.Framework;
using PoESkillTree.Computation.Common;
using PoESkillTree.Computation.Core.Events;
using PoESkillTree.Computation.Core.Graphs;
using PoESkillTree.Computation.Core.NodeCollections;
using PoESkillTree.Computation.Core.Nodes;

namespace PoESkillTree.Computation.Core.Tests.Graphs
{
    [TestFixture]
    public class StatRegistryTest
    {
        [Test]
        public void SutIsDeterminesNodeRemoval()
        {
            var sut = CreateSut();

            Assert.IsInstanceOf<IDeterminesNodeRemoval>(sut);
        }

        [Test]
        public void AddAddsCorrectNodeToNodeCollection()
        {
            var stat = new StatStub { IsRegisteredExplicitly = true };
            var coreNode = Mock.Of<ICalculationNode>();
            var expected = Mock.Of<IDisposableNode>();
            var nodeRepository = Mock.Of<INodeRepository>(r => r.GetNode(stat, NodeType.Total) == coreNode);
            var nodeCollection = new NodeCollection<IStat>();
            var sut = CreateSut(nodeCollection, nodeRepository, n => n == coreNode ? expected : null);

            sut.Add(stat);

            CollectionAssert.Contains(nodeCollection, expected);
        }

        [Test]
        public void RemoveRemovesCorrectNodeFromNodeCollection()
        {
            var stat = new StatStub { IsRegisteredExplicitly = true };
            var coreNode = Mock.Of<ICalculationNode>();
            var expected = Mock.Of<IDisposableNode>();
            var nodeRepository = Mock.Of<INodeRepository>(r => r.GetNode(stat, NodeType.Total) == coreNode);
            var nodeCollection = new NodeCollection<IStat>();
            var sut = CreateSut(nodeCollection, nodeRepository, n => n == coreNode ? expected : null);
            sut.Add(stat);

            sut.Remove(stat);

            CollectionAssert.DoesNotContain(nodeCollection, expected);
        }

        [Test]
        public void AddDoesNotAddIfStatIsNotRegisteredExplicitly()
        {
            var stat = new StatStub { IsRegisteredExplicitly = false };
            var nodeCollection = new NodeCollection<IStat>();
            var sut = CreateSut(nodeCollection);

            sut.Add(stat);

            CollectionAssert.IsEmpty(nodeCollection);
        }

        [Test]
        public void RemoveDoesNothingIfStatWasNotAdded()
        {
            var stat = new StatStub { IsRegisteredExplicitly = true };
            var sut = CreateSut();

            sut.Remove(stat);
        }

        [TestCase(1, ExpectedResult = false)]
        [TestCase(0, ExpectedResult = true)]
        public bool CanBeRemovedWithCountsSubscribersReturnsCorrectResult(int subscriberCount)
        {
            var node = Mock.Of<ICountsSubsribers>(c => c.SubscriberCount == subscriberCount);
            var sut = CreateSut();

            return sut.CanBeRemoved(node);
        }

        [TestCase(1, ExpectedResult = false)]
        [TestCase(0, ExpectedResult = true)]
        public bool CanBeRemovedWithUnknownNodeReturnsCorrectResult(int subscriberCount)
        {
            var node =
                Mock.Of<ISuspendableEventViewProvider<ICalculationNode>>(c => c.SubscriberCount == subscriberCount);
            var sut = CreateSut();

            return sut.CanBeRemoved(node);
        }

        [TestCase(1, ExpectedResult = true)]
        [TestCase(2, ExpectedResult = false)]
        public bool CanBeRemovedWithKnownNodeReturnsCorrectResult(int subscriberCount)
        {
            var stat = new StatStub { IsRegisteredExplicitly = true };
            var coreNode = Mock.Of<ICalculationNode>();
            var nodeRepository = Mock.Of<INodeRepository>(r => r.GetNode(stat, NodeType.Total) == coreNode);
            var sut = CreateSut(nodeRepository: nodeRepository);
            sut.Add(stat);
            var node = Mock.Of<ISuspendableEventViewProvider<ICalculationNode>>(c =>
                c.SubscriberCount == subscriberCount && c.SuspendableView == coreNode);

            return sut.CanBeRemoved(node);
        }

        [TestCase(1, ExpectedResult = false)]
        public bool CanBeRemovedWithRemovedNodeReturnsCorrectResult(int subscriberCount)
        {
            var stat = new StatStub { IsRegisteredExplicitly = true };
            var coreNode = Mock.Of<ICalculationNode>();
            var nodeRepository = Mock.Of<INodeRepository>(r => r.GetNode(stat, NodeType.Total) == coreNode);
            var sut = CreateSut(nodeRepository: nodeRepository);
            sut.Add(stat);
            sut.Remove(stat);
            var node = Mock.Of<ISuspendableEventViewProvider<ICalculationNode>>(c =>
                c.SubscriberCount == subscriberCount && c.SuspendableView == coreNode);

            return sut.CanBeRemoved(node);
        }

        [Test]
        public void RemoveDisposesWrappingNode()
        {
            var stat = new StatStub { IsRegisteredExplicitly = true };
            var wrappedNode = Mock.Of<IDisposableNode>();
            var nodeRepository = 
                Mock.Of<INodeRepository>(r => r.GetNode(stat, NodeType.Total) == Mock.Of<ICalculationNode>());
            var sut = CreateSut(nodeRepository: nodeRepository, nodeWrapper: _ => wrappedNode);
            sut.Add(stat);

            sut.Remove(stat);

            Mock.Get(wrappedNode).Verify(n => n.Dispose());
        }

        private static StatRegistry CreateSut(
            NodeCollection<IStat> nodeCollection = null,
            INodeRepository nodeRepository = null,
            Func<ICalculationNode, IDisposableNode> nodeWrapper = null)
        {
            return new StatRegistry(
                nodeCollection ?? new NodeCollection<IStat>(),
                nodeWrapper ?? (_ => Mock.Of<IDisposableNode>()))
            {
                NodeRepository = nodeRepository
            };
        }
    }
}