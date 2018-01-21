using System;
using System.Linq;
using Moq;
using NUnit.Framework;

namespace PoESkillTree.Computation.Core.Tests
{
    [TestFixture]
    public class OverrideNodeTest
    {
        [Test]
        public void SutIsCalculationNode()
        {
            var sut = CreateSut();

            Assert.IsInstanceOf<ICalculationNode>(sut);
        }

        [Test]
        public void ValueReturnsNullWithoutChildren()
        {
            var sut = CreateSut();

            Assert.IsNull(sut.Value);
        }

        [TestCase(42)]
        public void ValueReturnsValueOfSingleChildWithOneChild(int value)
        {
            var formNodes = MockFormNodeCollection(value);
            var sut = CreateSut(formNodes);

            sut.AssertValueEquals(value);
        }

        [Test]
        public void ValueReturns0IfAChildHasValue0()
        {
            var formNodes = MockFormNodeCollection(42, 43, 0, 4);
            var sut = CreateSut(formNodes);

            sut.AssertValueEquals(0);
        }

        [Test]
        public void ValueThrowsExceptionIfItHasMultipleChildrenAndNoneHasValue0()
        {
            var formNodes = MockFormNodeCollection(42, 43, null, 4, -3);
            var sut = CreateSut(formNodes);

            Assert.Throws<NotSupportedException>(() => { var _ = sut.Value; });
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
            var addedNode = new FormNodeCollectionItem(NodeHelper.MockNode(2), null, null);
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

        private static IFormNodeCollection MockFormNodeCollection(params double?[] values)
        {
            var items = values
                .Select(v => new FormNodeCollectionItem(NodeHelper.MockNode(v), null, null))
                .ToList();
            return Mock.Of<IFormNodeCollection>(c => c.Items == items);
        }

        private static OverrideNode CreateSut(IFormNodeCollection formNodes = null) =>
            new OverrideNode(formNodes ?? MockFormNodeCollection());
    }
}