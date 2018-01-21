using System;
using System.Collections.Generic;
using System.Linq;
using Moq;
using NUnit.Framework;
using static PoESkillTree.Computation.Core.Tests.NodeHelper;

namespace PoESkillTree.Computation.Core.Tests
{
    [TestFixture]
    public class FormNodeAggregatingNodeTest
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
            var formNodes = MockFormNodeCollection(values);
            var sut = CreateSut(formNodes, AggregateBySum);

            sut.AssertValueEquals(expected);
        }

        [TestCase(-8, 1.0, null, 4.0, -2.0)]
        public void ValueIsProductAggregationResult(double? expected, params double?[] values)
        {
            var formNodes = MockFormNodeCollection(values);
            var sut = CreateSut(formNodes, AggregateByProduct);

            sut.AssertValueEquals(expected);
        }

        [Test]
        public void ValueChangedIsRaisedWhenFormItemsChangedIsRaised()
        {
            var formNodes = MockFormNodeCollection(42);
            var sut = CreateSut(formNodes);
            var _ = sut.Value;
            var raised = false;
            sut.SubscribeToValueChanged(() => raised = true);

            Mock.Get(formNodes).Raise(c => c.ItemsChanged += null, EventArgs.Empty);

            Assert.IsTrue(raised);
        }

        [Test]
        public void ValueChangedIsNotRaisedBeforeValueWasAccessed()
        {
            var formNodes = MockFormNodeCollection(42);
            var sut = CreateSut(formNodes);

            sut.AssertValueChangedWillNotBeInvoked();
            Mock.Get(formNodes).Raise(c => c.ItemsChanged += null, EventArgs.Empty);
        }

        [Test]
        public void DisposeUnsubscribesFromItemsChanged()
        {
            var formNodes = MockFormNodeCollection(42);
            var sut = CreateSut(formNodes);
            var _ = sut.Value;

            sut.Dispose();

            sut.AssertValueChangedWillNotBeInvoked();
            Mock.Get(formNodes).Raise(c => c.ItemsChanged += null, EventArgs.Empty);
            Mock.Get(formNodes.Items[0].Node).Raise(n => n.ValueChanged += null, EventArgs.Empty);
        }

        [Test]
        public void HandlersAreNotSubscribedToMultipleTimes()
        {
            var formNodes = MockFormNodeCollection(42);
            var sut = CreateSut(formNodes);
            var incovations = 0;
            sut.SubscribeToValueChanged(() => incovations++);
            var _ = sut.Value;
            _ = sut.Value;

            Mock.Get(formNodes).Raise(c => c.ItemsChanged += null, EventArgs.Empty);
            Mock.Get(formNodes.Items[0].Node).Raise(n => n.ValueChanged += null, EventArgs.Empty);

            Assert.AreEqual(2, incovations);
        }
        
        [Test]
        public void ValueChangedIsRaisedWhenAnItemsValueChangedIsRaised()
        {
            var formNodes = MockFormNodeCollection(42, 0);
            var sut = CreateSut(formNodes);
            var _ = sut.Value;
            var raised = false;
            sut.SubscribeToValueChanged(() => raised = true);

            Mock.Get(formNodes.Items[1].Node).Raise(n => n.ValueChanged += null, EventArgs.Empty);

            Assert.IsTrue(raised);
        }

        [Test]
        public void FormNodesItemsChangedCausesResubscribingToChildren()
        {
            var formNodes = MockFormNodeCollection(0, 1);
            var sut = CreateSut(formNodes);
            var _ = sut.Value;
            var incovations = 0;
            sut.SubscribeToValueChanged(() => incovations++);
            var removedNode = formNodes.Items[0].Node;
            var keptNode = formNodes.Items[1].Node;
            var addedNode = new FormNodeCollectionItem(MockNode(2), null, null);
            var updatedNodes = formNodes.Items.Skip(1).Union(new[] { addedNode }).ToList();
            var formNodesMock = Mock.Get(formNodes);
            formNodesMock.Setup(c => c.Items).Returns(updatedNodes);

            formNodesMock.Raise(c => c.ItemsChanged += null, EventArgs.Empty);

            incovations = 0;
            Mock.Get(removedNode).Raise(n => n.ValueChanged += null, EventArgs.Empty);
            Assert.AreEqual(0, incovations);
            Mock.Get(keptNode).Raise(n => n.ValueChanged += null, EventArgs.Empty);
            Mock.Get(addedNode.Node).Raise(n => n.ValueChanged += null, EventArgs.Empty);
            Assert.AreEqual(2, incovations);
        }

        private static FormNodeAggregatingNode CreateSut(
            IFormNodeCollection formNodes = null, NodeValueAggregator aggregator = null)
        {
            return new FormNodeAggregatingNode(formNodes, aggregator ?? AggregateBySum);
        }

        private static NodeValue? AggregateBySum(IEnumerable<NodeValue?> values) => 
            values.OfType<NodeValue>().Aggregate(new NodeValue(), (l, r) => l + r);

        private static NodeValue? AggregateByProduct(IEnumerable<NodeValue?> values) => 
            values.OfType<NodeValue>().Aggregate(new NodeValue(1), (l, r) => l * r);
    }
}