using System;
using Moq;
using NUnit.Framework;

namespace PoESkillTree.Computation.Core.Tests
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

            var actual = sut.Value;

            Assert.AreEqual(value, actual);
        }

        [Test]
        public void ValueChangedIsRaisedWhenAdaptedNodesValueChangeReceivedIsRaised()
        {
            var adaptedNodeMock = new Mock<ICachingNode>();
            var sut = CreateSut(adaptedNodeMock.Object);
            var valueChangedFired = false;
            sut.ValueChanged += (sender, args) =>
            {
                Assert.AreSame(sut, sender);
                valueChangedFired = true;
            };

            adaptedNodeMock.Raise(n => n.ValueChangeReceived += null, EventArgs.Empty);

            Assert.IsTrue(valueChangedFired);
        }

        [Test]
        public void DisposeUnSubscribesFromAdaptedNode()
        {
            var adaptedNodeMock = new Mock<ICachingNode>();
            var sut = CreateSut(adaptedNodeMock.Object);
            sut.ValueChanged += (sender, args) => Assert.Fail();

            sut.Dispose();
            adaptedNodeMock.Raise(n => n.ValueChangeReceived += null, EventArgs.Empty);
        }


        private static CachingNodeAdapter CreateSut(double? recalculatableNodeValue = 0)
        {
            var recalculatableNode = Mock.Of<ICachingNode>(n => n.Value == recalculatableNodeValue);
            return CreateSut(recalculatableNode);
        }

        private static CachingNodeAdapter CreateSut(ICachingNode adaptedNode)
        {
            return new CachingNodeAdapter(adaptedNode);
        }
    }
}