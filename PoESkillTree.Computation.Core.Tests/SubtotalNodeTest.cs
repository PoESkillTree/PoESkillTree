using System;
using Moq;
using NUnit.Framework;
using static PoESkillTree.Computation.Core.Tests.NodeHelper;

namespace PoESkillTree.Computation.Core.Tests
{
    [TestFixture]
    public class SubtotalNodeTest
    {
        [Test]
        public void SutIsCalculationNode()
        {
            var sut = CreateSut();

            Assert.IsInstanceOf<ICalculationNode>(sut);
        }

        [TestCase(null, null, null, null)]
        [TestCase(42, null, null, 42)]
        [TestCase(9, 4, 8, 8)]
        [TestCase(1, 4, 8, 4)]
        [TestCase(0, 4, 8, 0)]
        public void ValueReturnsCorrectResult(double? uncapped, double? min, double? max, double? expected)
        {
            var sut = CreateSut(uncapped, min, max);

            sut.AssertValueEquals(expected);
        }

        [Test]
        public void ValueChangedIsRaisedWhenUncappedRaisesValueChange()
        {
            var nodeMock = new Mock<ICalculationNode>();
            var sut = CreateSut(nodeMock.Object);
            var raised = false;
            sut.SubscribeToValueChanged(() => raised = true);
            var _ = sut.Value;

            nodeMock.Raise(n => n.ValueChanged += null, EventArgs.Empty);

            Assert.IsTrue(raised);
        }

        [Test]
        public void ValueChangedIsNotRaisedBeforeValueWasAccessed()
        {
            var nodeMock = new Mock<ICalculationNode>();
            var sut = CreateSut(nodeMock.Object);
            sut.AssertValueChangedWillNotBeInvoked();

            nodeMock.Raise(n => n.ValueChanged += null, EventArgs.Empty);
        }

        [Test]
        public void ValueChangedIsRaisedWhenMinimumRaisesValueChange()
        {
            var nodeMock = new Mock<ICalculationNode>();
            var sut = CreateSut(minNode: nodeMock.Object);
            var raised = false;
            sut.SubscribeToValueChanged(() => raised = true);
            var _ = sut.Value;

            nodeMock.Raise(n => n.ValueChanged += null, EventArgs.Empty);

            Assert.IsTrue(raised);
        }

        [Test]
        public void ValueChangedIsRaisedWhenMaximumRaisesValueChange()
        {
            var nodeMock = new Mock<ICalculationNode>();
            var sut = CreateSut(maxNode: nodeMock.Object);
            var raised = false;
            sut.SubscribeToValueChanged(() => raised = true);
            var _ = sut.Value;

            nodeMock.Raise(n => n.ValueChanged += null, EventArgs.Empty);

            Assert.IsTrue(raised);
        }

        [Test]
        public void ValueChangedIsSubscribedToOnlyOncePerNode()
        {
            var nodeMock = new Mock<ICalculationNode>();
            var sut = CreateSut(nodeMock.Object, nodeMock.Object, nodeMock.Object);
            var invocations = 0;
            sut.SubscribeToValueChanged(() => invocations++);
            var _ = sut.Value;
            _ = sut.Value;
            _ = sut.Value;

            nodeMock.Raise(n => n.ValueChanged += null, EventArgs.Empty);

            Assert.AreEqual(3, invocations);
        }

        [Test]
        public void DisposeUnsubscribesHandler()
        {
            var nodeMock = new Mock<ICalculationNode>();
            var sut = CreateSut(nodeMock.Object, nodeMock.Object, nodeMock.Object);
            var _ = sut.Value;

            sut.Dispose();

            sut.AssertValueChangedWillNotBeInvoked();
            nodeMock.Raise(n => n.ValueChanged += null, EventArgs.Empty);
        }

        private static SubtotalNode CreateSut(double? uncapped, double? min, double? max)
        {
            var uncappedNode = MockNode(uncapped);
            var minNode = MockNode(min);
            var maxNode = MockNode(max);
            return CreateSut(uncappedNode, minNode, maxNode);
        }

        private static SubtotalNode CreateSut(
            ICalculationNode uncappedNode = null, ICalculationNode minNode = null, ICalculationNode maxNode = null)
        {
            return new SubtotalNode(uncappedNode ?? MockNode(), minNode ?? MockNode(), maxNode ?? MockNode());
        }
    }
}