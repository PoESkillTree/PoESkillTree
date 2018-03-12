using System;
using Moq;
using NUnit.Framework;
using PoESkillTree.Computation.Core.Nodes;

namespace PoESkillTree.Computation.Core.Tests.Nodes
{
    [TestFixture]
    public class WrappingNodeTest
    {
        [Test]
        public void SutIsCalculationNode()
        {
            var sut = CreateSut();

            Assert.IsInstanceOf<ICalculationNode>(sut);
        }

        [TestCase(0)]
        [TestCase(42)]
        public void ValueReturnsInjectedResult(double expected)
        {
            var sut = CreateSut(NodeHelper.MockNode(expected));

            sut.AssertValueEquals(expected);
        }

        [Test]
        public void ValueChangedIsRaisedWhenInjectedNodeRaises()
        {
            var nodeMock = new Mock<ICalculationNode>();
            var sut = CreateSut(nodeMock.Object);
            var raised = false;
            sut.SubscribeToValueChanged(() => raised = true);

            nodeMock.Raise(n => n.ValueChanged += null, EventArgs.Empty);

            Assert.IsTrue(raised);
        }

        [Test]
        public void ValueChangedIsNotRaisedAfterDispose()
        {
            var nodeMock = new Mock<ICalculationNode>();
            var sut = CreateSut(nodeMock.Object);

            sut.Dispose();

            sut.AssertValueChangedWillNotBeInvoked();
            nodeMock.Raise(n => n.ValueChanged += null, EventArgs.Empty);
        }

        private static WrappingNode CreateSut(ICalculationNode decoratedNode = null) =>
            new WrappingNode(decoratedNode ?? NodeHelper.MockNode());
    }
}