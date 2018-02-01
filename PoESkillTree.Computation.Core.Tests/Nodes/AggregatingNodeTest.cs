using System;
using System.Collections.Generic;
using System.Linq;
using Moq;
using NUnit.Framework;
using PoESkillTree.Computation.Common;
using PoESkillTree.Computation.Core.Nodes;

namespace PoESkillTree.Computation.Core.Tests.Nodes
{
    [TestFixture]
    public class AggregatingNodeTest
    {
        [Test]
        public void SutIsCalculationNode()
        {
            var sut = CreateSut();

            Assert.IsInstanceOf<ICalculationNode>(sut);
        }

        [TestCase(3, 1.0, null, 4.0, -2.0)]
        [TestCase(1, 5.0, -4.0)]
        [TestCase(0)]
        public void ValueIsSumAggregationResult(double? expected, params double?[] values)
        {
            var nodes = NodeHelper.MockNodeCollection(values);
            var sut = CreateSut(nodes, AggregateBySum);

            sut.AssertValueEquals(expected);
        }

        [TestCase(-8, 1.0, null, 4.0, -2.0)]
        public void ValueIsProductAggregationResult(double? expected, params double?[] values)
        {
            var nodes = NodeHelper.MockNodeCollection(values);
            var sut = CreateSut(nodes, AggregateByProduct);

            sut.AssertValueEquals(expected);
        }

        [Test]
        public void ValueChangedIsRaisedWhenCollectionChangedIsRaised()
        {
            var nodes = NodeHelper.MockNodeCollection(42);
            var sut = CreateSut(nodes);
            var _ = sut.Value;
            var raised = false;
            sut.SubscribeToValueChanged(() => raised = true);

            Mock.Get(nodes).Raise(c => c.CollectionChanged += null, DefaultCollectionChangeArgs);

            Assert.IsTrue(raised);
        }

        [Test]
        public void ValueChangedIsNotRaisedBeforeValueWasAccessed()
        {
            var nodes = NodeHelper.MockNodeCollection(42);
            var sut = CreateSut(nodes);

            sut.AssertValueChangedWillNotBeInvoked();
            Mock.Get(nodes).Raise(c => c.CollectionChanged += null, DefaultCollectionChangeArgs);
        }

        [Test]
        public void DisposeUnsubscribesFromCollectionChanged()
        {
            var nodes = NodeHelper.MockNodeCollection(42);
            var sut = CreateSut(nodes);
            var _ = sut.Value;

            sut.Dispose();

            sut.AssertValueChangedWillNotBeInvoked();
            Mock.Get(nodes).Raise(c => c.CollectionChanged += null, DefaultCollectionChangeArgs);
            Mock.Get(nodes.First()).Raise(n => n.ValueChanged += null, EventArgs.Empty);
        }

        [Test]
        public void HandlersAreNotSubscribedToMultipleTimes()
        {
            var nodes = NodeHelper.MockNodeCollection(42);
            var sut = CreateSut(nodes);
            var invocations = 0;
            sut.SubscribeToValueChanged(() => invocations++);
            var _ = sut.Value;
            _ = sut.Value;

            Mock.Get(nodes).Raise(c => c.CollectionChanged += null, DefaultCollectionChangeArgs);
            Mock.Get(nodes.First()).Raise(n => n.ValueChanged += null, EventArgs.Empty);

            Assert.AreEqual(2, invocations);
        }
        
        [Test]
        public void ValueChangedIsRaisedWhenAnItemsValueChangedIsRaised()
        {
            var nodes = NodeHelper.MockNodeCollection(42, 0);
            var sut = CreateSut(nodes);
            var _ = sut.Value;
            var raised = false;
            sut.SubscribeToValueChanged(() => raised = true);

            Mock.Get(nodes.Last()).Raise(n => n.ValueChanged += null, EventArgs.Empty);

            Assert.IsTrue(raised);
        }

        [Test]
        public void NodesCollectionChangedWithResetCausesResubscribingToChildren()
        {
            var nodes = NodeHelper.MockNodeCollection(0, 1);
            var sut = CreateSut(nodes);
            var _ = sut.Value;
            var removedNode = nodes.First();
            var keptNode = nodes.Last();
            var addedNode = NodeHelper.MockNode(2);
            var updatedNodes = nodes.Skip(1).Union(new[] { addedNode }).ToList();
            var nodesMock = Mock.Get(nodes);
            nodesMock.Setup(c => c.GetEnumerator()).Returns(() => updatedNodes.GetEnumerator());

            nodesMock.Raise(c => c.CollectionChanged += null, DefaultCollectionChangeArgs);
            
            var invocations = 0;
            sut.SubscribeToValueChanged(() => invocations++);
            Mock.Get(removedNode).Raise(n => n.ValueChanged += null, EventArgs.Empty);
            Assert.AreEqual(0, invocations);
            Mock.Get(keptNode).Raise(n => n.ValueChanged += null, EventArgs.Empty);
            Mock.Get(addedNode).Raise(n => n.ValueChanged += null, EventArgs.Empty);
            Assert.AreEqual(2, invocations);
        }

        [Test]
        public void NodesCollectionChangedWithAddCausesSubscribingToAddedNode()
        {
            var nodes = NodeHelper.MockNodeCollection(0, 1);
            var sut = CreateSut(nodes);
            var _ = sut.Value;
            var addedNode = NodeHelper.MockNode(1);
            var nodesMock = Mock.Get(nodes);

            nodesMock.Raise(c => c.CollectionChanged += null, 
                new NodeCollectionChangeEventArgs(NodeCollectionChangeAction.Add, addedNode));
            
            var invocations = 0;
            sut.SubscribeToValueChanged(() => invocations++);
            Mock.Get(addedNode).Raise(n => n.ValueChanged += null, EventArgs.Empty);
            Assert.AreEqual(1, invocations);
        }

        [Test]
        public void NodesCollectionChangedWithRemoveCausesUnsubscribingFromRemovedNode()
        {
            var nodes = NodeHelper.MockNodeCollection(0, 1);
            var sut = CreateSut(nodes);
            var _ = sut.Value;
            var removedNode = nodes.First();
            var nodesMock = Mock.Get(nodes);

            nodesMock.Raise(c => c.CollectionChanged += null, 
                new NodeCollectionChangeEventArgs(NodeCollectionChangeAction.Remove, removedNode));

            sut.AssertValueChangedWillNotBeInvoked();
            Mock.Get(removedNode).Raise(n => n.ValueChanged += null, EventArgs.Empty);
        }

        [Test]
        public void DisposeDoesNothingIfValueWasNotAccessed()
        {
            var sut = CreateSut(NodeHelper.MockNodeCollection());
            sut.AssertValueChangedWillNotBeInvoked();

            sut.Dispose();
        }

        private static readonly NodeCollectionChangeEventArgs DefaultCollectionChangeArgs =
            new NodeCollectionChangeEventArgs(NodeCollectionChangeAction.Reset, null);

        private static AggregatingNode CreateSut(
            INodeCollection nodes = null, NodeValueAggregator aggregator = null) => 
            new AggregatingNode(nodes, aggregator ?? AggregateBySum);

        private static NodeValue? AggregateBySum(IEnumerable<NodeValue?> values) => 
            values.OfType<NodeValue>().Aggregate(new NodeValue(), (l, r) => l + r);

        private static NodeValue? AggregateByProduct(IEnumerable<NodeValue?> values) => 
            values.OfType<NodeValue>().Aggregate(new NodeValue(1), (l, r) => l * r);
    }
}