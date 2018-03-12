using System;
using System.Linq;
using Moq;
using MoreLinq;
using NUnit.Framework;
using PoESkillTree.Computation.Common;
using PoESkillTree.Computation.Core.Events;
using PoESkillTree.Computation.Core.Nodes;

namespace PoESkillTree.Computation.Core.Tests.Nodes
{
    [TestFixture]
    public class CachingNodeTest
    {
        [Test]
        public void SutIsRecalculatableNode()
        {
            var sut = CreateSut();

            Assert.IsInstanceOf<ICachingNode>(sut);
        }

        [TestCase(42)]
        [TestCase(null)]
        public void ValueGetsDecoratedNodesValueWhenNotCached(double? value)
        {
            var sut = CreateSut(value);

            sut.AssertValueEquals(value);
        }

        [Test]
        public void ValueCachesDecoratedNodesValue()
        {
            const int expected = 42;
            var nodeMock = new Mock<ICalculationNode>();
            var sut = CreateCachedSut(nodeMock, expected);
            nodeMock.SetupGet(n => n.Value).Returns((NodeValue) 41);

            sut.AssertValueEquals(expected);
        }

        [Test]
        public void DecoratedNodesValueChangedInvalidatesCache()
        {
            const int expected = 42;
            var nodeMock = new Mock<ICalculationNode>();
            var sut = CreateCachedSut(nodeMock, 41);
            nodeMock.SetupGet(n => n.Value).Returns((NodeValue) expected);

            nodeMock.Raise(n => n.ValueChanged += null, EventArgs.Empty);

            sut.AssertValueEquals(expected);
        }

        [Test]
        public void ValueChangeReceivedIsRaisedWhenDecoratedNodeRaisesValueChanged()
        {
            var nodeMock = new Mock<ICalculationNode>();
            var sut = CreateSut(nodeMock.Object);
            var _ = sut.Value;
            var raised = false;
            sut.ValueChangeReceived += (sender, args) =>
            {
                Assert.AreEqual(sut, sender);
                raised = true;
            };

            nodeMock.Raise(n => n.ValueChanged += null, EventArgs.Empty);

            Assert.IsTrue(raised);
        }

        [Test]
        public void ValueChangeReceivedIsNotRaisedWhendValueWasNotCalled()
        {
            var nodeMock = new Mock<ICalculationNode>();
            var sut = CreateSut(nodeMock.Object);
            sut.ValueChangeReceived += (sender, args) => Assert.Fail();

            nodeMock.Raise(n => n.ValueChanged += null, EventArgs.Empty);
        }

        [Test]
        public void DisposeUnsubscribesFromDecoratedNode()
        {
            var nodeMock = new Mock<ICalculationNode>();
            var sut = CreateSut(nodeMock.Object);
            var _ = sut.Value;
            sut.ValueChangeReceived += (sender, args) => Assert.Fail();

            sut.Dispose();
            nodeMock.Raise(n => n.ValueChanged += null, EventArgs.Empty);
        }

        [Test]
        public void ValueChangedIsRaisedWhenEventsAreNotSuspended()
        {
            var nodeMock = new Mock<ICalculationNode>();
            var sut = CreateSut(nodeMock.Object);
            var _ = sut.Value;
            var raised = false;
            sut.SubscribeToValueChanged(() => raised = true);

            nodeMock.Raise(n => n.ValueChanged += null, EventArgs.Empty);

            Assert.IsTrue(raised);
        }

        [Test]
        public void ValueChangedIsNotRaisedWhenEventsAreSuspended()
        {
            var nodeMock = new Mock<ICalculationNode>();
            var sut = CreateSut(nodeMock.Object);
            var _ = sut.Value;

            sut.SuspendEvents();

            sut.AssertValueChangedWillNotBeInvoked();
            nodeMock.Raise(n => n.ValueChanged += null, EventArgs.Empty);
        }

        [Test]
        public void ResumeAllowsValueChangedToBeRaisedAgain()
        {
            var nodeMock = new Mock<ICalculationNode>();
            var sut = CreateSut(nodeMock.Object);
            var _ = sut.Value;
            var raised = false;
            sut.SubscribeToValueChanged(() => raised = true);
            sut.SuspendEvents();

            sut.ResumeEvents();

            nodeMock.Raise(n => n.ValueChanged += null, EventArgs.Empty);
            Assert.IsTrue(raised);
        }

        [Test]
        public void ResumeRaisesValueChangedIfValueChangeReceivedWasRaisedAfterSuspend()
        {
            var nodeMock = new Mock<ICalculationNode>();
            var sut = CreateSut(nodeMock.Object);
            var _ = sut.Value;
            var raised = false;
            sut.SubscribeToValueChanged(() => raised = true);
            sut.SuspendEvents();
            nodeMock.Raise(n => n.ValueChanged += null, EventArgs.Empty);

            sut.ResumeEvents();

            Assert.IsTrue(raised);
        }

        [Test]
        public void SutIsCountsSubscribers()
        {
            var sut = CreateSut();

            Assert.IsInstanceOf<ICountsSubsribers>(sut);
        }

        [TestCase(3)]
        [TestCase(0)]
        public void SubscriberCountReturnsCorrectResult(int expected)
        {
            var sut = CreateSut();
            Enumerable.Repeat(0, expected).ForEach(_ => sut.ValueChanged += (sender, args) => { });

            var actual = sut.SubscriberCount;

            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void ValueUsesCycleGuard()
        {
            var cycleGuardMock = new Mock<ICycleGuard>();
            cycleGuardMock.Setup(g => g.Guard().Dispose()).Verifiable();
            var sut = CreateSut(cycleGuard: cycleGuardMock.Object);

            var _ = sut.Value;

            cycleGuardMock.Verify();
        }

        private static CachingNode CreateCachedSut(Mock<ICalculationNode> decoratedNodeMock, double? cachedValue)
        {
            decoratedNodeMock.SetupGet(n => n.Value).Returns((NodeValue?) cachedValue);
            var sut = CreateSut(decoratedNodeMock.Object);
            var _ = sut.Value;
            return sut;
        }

        private static CachingNode CreateSut(double? value = null, ICycleGuard cycleGuard = null)
        {
            var decoratedNode = NodeHelper.MockNode(value);
            return CreateSut(decoratedNode, cycleGuard);
        }

        private static CachingNode CreateSut(ICalculationNode decoratedNode, ICycleGuard cycleGuard = null)
        {
            return new CachingNode(decoratedNode, cycleGuard ?? Mock.Of<ICycleGuard>());
        }
    }
}