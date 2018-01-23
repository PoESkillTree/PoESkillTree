using System;
using System.Collections.Generic;
using System.Linq;
using Moq;
using NUnit.Framework;
using static PoESkillTree.Computation.Core.Tests.NodeHelper;

namespace PoESkillTree.Computation.Core.Tests
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
            var nodes = MockNodeCollection(values);
            var sut = CreateSut(nodes, AggregateBySum);

            sut.AssertValueEquals(expected);
        }

        [TestCase(-8, 1.0, null, 4.0, -2.0)]
        public void ValueIsProductAggregationResult(double? expected, params double?[] values)
        {
            var nodes = MockNodeCollection(values);
            var sut = CreateSut(nodes, AggregateByProduct);

            sut.AssertValueEquals(expected);
        }

        [Test]
        public void ValueChangedIsRaisedWhenFormItemsChangedIsRaised()
        {
            var nodes = MockNodeCollection(42);
            var sut = CreateSut(nodes);
            var _ = sut.Value;
            var raised = false;
            sut.SubscribeToValueChanged(() => raised = true);

            Mock.Get(nodes).Raise(c => c.ItemsChanged += null, EventArgs.Empty);

            Assert.IsTrue(raised);
        }

        [Test]
        public void ValueChangedIsNotRaisedBeforeValueWasAccessed()
        {
            var nodes = MockNodeCollection(42);
            var sut = CreateSut(nodes);

            sut.AssertValueChangedWillNotBeInvoked();
            Mock.Get(nodes).Raise(c => c.ItemsChanged += null, EventArgs.Empty);
        }

        [Test]
        public void DisposeUnsubscribesFromItemsChanged()
        {
            var nodes = MockNodeCollection(42);
            var sut = CreateSut(nodes);
            var _ = sut.Value;

            sut.Dispose();

            sut.AssertValueChangedWillNotBeInvoked();
            Mock.Get(nodes).Raise(c => c.ItemsChanged += null, EventArgs.Empty);
            Mock.Get(nodes.Items[0].Node).Raise(n => n.ValueChanged += null, EventArgs.Empty);
        }

        [Test]
        public void HandlersAreNotSubscribedToMultipleTimes()
        {
            var nodes = MockNodeCollection(42);
            var sut = CreateSut(nodes);
            var incovations = 0;
            sut.SubscribeToValueChanged(() => incovations++);
            var _ = sut.Value;
            _ = sut.Value;

            Mock.Get(nodes).Raise(c => c.ItemsChanged += null, EventArgs.Empty);
            Mock.Get(nodes.Items[0].Node).Raise(n => n.ValueChanged += null, EventArgs.Empty);

            Assert.AreEqual(2, incovations);
        }
        
        [Test]
        public void ValueChangedIsRaisedWhenAnItemsValueChangedIsRaised()
        {
            var nodes = MockNodeCollection(42, 0);
            var sut = CreateSut(nodes);
            var _ = sut.Value;
            var raised = false;
            sut.SubscribeToValueChanged(() => raised = true);

            Mock.Get(nodes.Items[1].Node).Raise(n => n.ValueChanged += null, EventArgs.Empty);

            Assert.IsTrue(raised);
        }

        [Test]
        public void NodesItemsChangedCausesResubscribingToChildren()
        {
            var nodes = MockNodeCollection(0, 1);
            var sut = CreateSut(nodes);
            var _ = sut.Value;
            var incovations = 0;
            sut.SubscribeToValueChanged(() => incovations++);
            var removedNode = nodes.Items[0].Node;
            var keptNode = nodes.Items[1].Node;
            var addedNode = new NodeCollectionItem(MockNode(2));
            var updatedNodes = nodes.Items.Skip(1).Union(new[] { addedNode }).ToList();
            var nodesMock = Mock.Get(nodes);
            nodesMock.Setup(c => c.Items).Returns(updatedNodes);

            nodesMock.Raise(c => c.ItemsChanged += null, EventArgs.Empty);

            incovations = 0;
            Mock.Get(removedNode).Raise(n => n.ValueChanged += null, EventArgs.Empty);
            Assert.AreEqual(0, incovations);
            Mock.Get(keptNode).Raise(n => n.ValueChanged += null, EventArgs.Empty);
            Mock.Get(addedNode.Node).Raise(n => n.ValueChanged += null, EventArgs.Empty);
            Assert.AreEqual(2, incovations);
        }

        private static AggregatingNode CreateSut(
            INodeCollection nodes = null, NodeValueAggregator aggregator = null)
        {
            return new AggregatingNode(nodes, aggregator ?? AggregateBySum);
        }

        private static NodeValue? AggregateBySum(IEnumerable<NodeValue?> values) => 
            values.OfType<NodeValue>().Aggregate(new NodeValue(), (l, r) => l + r);

        private static NodeValue? AggregateByProduct(IEnumerable<NodeValue?> values) => 
            values.OfType<NodeValue>().Aggregate(new NodeValue(1), (l, r) => l * r);
    }
}