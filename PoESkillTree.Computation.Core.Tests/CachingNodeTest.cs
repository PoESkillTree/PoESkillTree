using System;
using Moq;
using NUnit.Framework;

namespace PoESkillTree.Computation.Core.Tests
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

            var actual = sut.Value;

            Assert.AreEqual(value, actual);
        }

        [Test]
        public void ValueCachesDecoratedNodesValue()
        {
            const int expected = 42;
            var nodeMock = new Mock<ICalculationNode>();
            var sut = CreateCachedSut(nodeMock, expected);
            nodeMock.SetupGet(n => n.Value).Returns(41);

            var actual = sut.Value;

            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void DecoratedNodesValueChangedInvalidatesCache()
        {
            const int expected = 42;
            var nodeMock = new Mock<ICalculationNode>();
            var sut = CreateCachedSut(nodeMock, 41);
            nodeMock.SetupGet(n => n.Value).Returns(expected);

            nodeMock.Raise(n => n.ValueChanged += null, EventArgs.Empty);
            var actual = sut.Value;

            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void RaiseValueChangedRaisesValueChangedy()
        {
            var sut = CreateSut();
            var raised = false;
            sut.ValueChanged += (sender, args) =>
            {
                Assert.AreEqual(sut, sender);
                raised = true;
            };

            sut.RaiseValueChanged();

            Assert.IsTrue(raised);
        }

        [Test]
        public void RaiseValueChangedDoesNotRaiseValueChangedOnSubsequentCalls()
        {
            var sut = CreateSut();
            sut.RaiseValueChanged();
            sut.ValueChanged += (sender, args) => Assert.Fail();

            sut.RaiseValueChanged();
        }

        [Test]
        public void ValueChangeReceivedIsRaisedWhenDecoratedNodeRaisesValueChanged()
        {
            var nodeMock = new Mock<ICalculationNode>();
            var sut = CreateSut(nodeMock.Object);
            sut.RaiseValueChanged();
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
        public void ValueChangeReceivedIsRaisedWhenValueWasCalled()
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
        public void ValueChangeReceivedIsNotRaisedWhenItWasNotPropagatedAndValueWasNotCalled()
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
            sut.RaiseValueChanged();
            sut.ValueChangeReceived += (sender, args) => Assert.Fail();

            sut.Dispose();
            nodeMock.Raise(n => n.ValueChanged += null, EventArgs.Empty);
        }

        private static CachingNode CreateCachedSut(Mock<ICalculationNode> decoratedNodeMock, double? cachedValue)
        {
            decoratedNodeMock.SetupGet(n => n.Value).Returns(cachedValue);
            var sut = CreateSut(decoratedNodeMock.Object);
            var _ = sut.Value;
            return sut;
        }

        private static CachingNode CreateSut(double? value = null)
        {
            var decoratedNode = Mock.Of<ICalculationNode>(n => n.Value == value);
            return CreateSut(decoratedNode);
        }

        private static CachingNode CreateSut(ICalculationNode decoratedNode)
        {
            return new CachingNode(decoratedNode);
        }
    }
}