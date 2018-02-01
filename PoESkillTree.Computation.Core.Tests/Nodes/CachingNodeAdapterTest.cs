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
    public class CachingNodeAdapterTest
    {
        [Test]
        public void SutIsCalculationNode()
        {
            var sut = CreateSut();

            Assert.IsInstanceOf<ICalculationNode>(sut);
        }

        [TestCase(42)]
        [TestCase(null)]
        public void ValueReturnsAdaptedNodesValue(double? value)
        {
            var sut = CreateSut(value);

            sut.AssertValueEquals(value);
        }

        [Test]
        public void ValueChangedIsRaisedWhenAdaptedNodesValueChangeReceivedIsRaised()
        {
            var adaptedNodeMock = new Mock<ICachingNode>();
            var sut = CreateSut(adaptedNodeMock.Object);
            var raised = false;
            sut.SubscribeToValueChanged(() => raised = true);

            adaptedNodeMock.Raise(n => n.ValueChangeReceived += null, EventArgs.Empty);

            Assert.IsTrue(raised);
        }

        [Test]
        public void DisposeUnSubscribesFromAdaptedNode()
        {
            var adaptedNodeMock = new Mock<ICachingNode>();
            var sut = CreateSut(adaptedNodeMock.Object);
            sut.AssertValueChangedWillNotBeInvoked();

            sut.Dispose();
            adaptedNodeMock.Raise(n => n.ValueChangeReceived += null, EventArgs.Empty);
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


        private static CachingNodeAdapter CreateSut(double? recalculatableNodeValue = 0)
        {
            var mock = new Mock<ICachingNode>();
            mock.SetupGet(n => n.Value).Returns((NodeValue?) recalculatableNodeValue);
            return CreateSut(mock.Object);
        }

        private static CachingNodeAdapter CreateSut(ICachingNode adaptedNode)
        {
            return new CachingNodeAdapter(adaptedNode);
        }
    }
}