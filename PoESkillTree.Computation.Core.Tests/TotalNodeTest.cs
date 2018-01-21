﻿using System;
using Moq;
using NUnit.Framework;

namespace PoESkillTree.Computation.Core.Tests
{
    [TestFixture]
    public class TotalNodeTest
    {
        [Test]
        public void SutIsCalculationNode()
        {
            var sut = CreateSut();

            Assert.IsInstanceOf<ICalculationNode>(sut);
        }

        [TestCase(42)]
        [TestCase(null)]
        public void ValueIsSubtotalValueIfTotalOverrideValueIsNull(double? value)
        {
            var subtotal = NodeHelper.MockNode(value);
            var sut = CreateSut(subtotal);

            sut.AssertValueEquals(value);
        }

        [TestCase(32)]
        public void ValueIsTotalOverrideValueIfNotNull(double? value)
        {
            var totalOverride = NodeHelper.MockNode(value);
            var sut = CreateSut(null, totalOverride);

            sut.AssertValueEquals(value);
        }

        [Test]
        public void ValueChangedIsRaisedWhenTotalOverrideValueChangedIsRaised()
        {
            var totalOverrideMock = new Mock<ICalculationNode>();
            var sut = CreateSut(null, totalOverrideMock.Object);
            var raised = false;
            sut.SubscribeToValueChanged(() => raised = true);
            
            var _ = sut.Value;
            totalOverrideMock.Raise(n => n.ValueChanged += null, EventArgs.Empty);

            Assert.IsTrue(raised);
        }

        [Test]
        public void ValueChangedIsRaisedWhenSubtotalValueChangedIsRaised()
        {
            var subtotalMock = new Mock<ICalculationNode>();
            var sut = CreateSut(subtotalMock.Object);
            var raised = false;
            sut.SubscribeToValueChanged(() => raised = true);

            var _ = sut.Value;
            subtotalMock.Raise(n => n.ValueChanged += null, EventArgs.Empty);

            Assert.IsTrue(raised);
        }

        [Test]
        public void ValueChangedIsNotRaisedBeforeValueWasAccessed()
        {
            var totalOverrideMock = new Mock<ICalculationNode>();
            var sut = CreateSut(null, totalOverrideMock.Object);
            sut.AssertValueChangedWillNotBeInvoked();

            totalOverrideMock.Raise(n => n.ValueChanged += null, EventArgs.Empty);
        }

        [Test]
        public void SubtotalValueChangedIsNotSubscribedToIfTotalOverrideIsNotNull()
        {
            var subtotalMock = new Mock<ICalculationNode>();
            var sut = CreateSut(subtotalMock.Object);
            sut.AssertValueChangedWillNotBeInvoked();

            subtotalMock.Raise(n => n.ValueChanged += null, EventArgs.Empty);
        }

        [Test]
        public void DisposeUnsubscribesFromSubtotal()
        {
            var subtotalMock = new Mock<ICalculationNode>();
            var sut = CreateSut(subtotalMock.Object);
            var _ = sut.Value;

            sut.Dispose();

            sut.AssertValueChangedWillNotBeInvoked();
            subtotalMock.Raise(n => n.ValueChanged += null, EventArgs.Empty);
        }

        [Test]
        public void DisposeUnsubscribesFromTotalOverride()
        {
            var totalOverrideMock = new Mock<ICalculationNode>();
            var sut = CreateSut(null, totalOverrideMock.Object);
            var _ = sut.Value;

            sut.Dispose();

            sut.AssertValueChangedWillNotBeInvoked();
            totalOverrideMock.Raise(n => n.ValueChanged += null, EventArgs.Empty);
        }

        [Test]
        public void SubtotalIsUnsubscribedFromIfNoLongerRequiredForValueCalculation()
        {
            var subtotalMock = new Mock<ICalculationNode>();
            var totalOverrideMock = new Mock<ICalculationNode>();
            var sut = CreateSut(subtotalMock.Object, totalOverrideMock.Object);
            var _ = sut.Value;

            totalOverrideMock.SetupGet(n => n.Value).Returns(new NodeValue(5));
            _ = sut.Value;

            sut.AssertValueChangedWillNotBeInvoked();
            subtotalMock.Raise(n => n.ValueChanged += null, EventArgs.Empty);
        }

        [Test]
        public void NodesAreNotSubscribedToMultipleTimes()
        {
            var subtotalMock = new Mock<ICalculationNode>();
            var totalOverrideMock = new Mock<ICalculationNode>();
            var sut = CreateSut(subtotalMock.Object, totalOverrideMock.Object);
            var invocations = 0;
            sut.SubscribeToValueChanged(() => invocations++);
            var _ = sut.Value;
            _ = sut.Value;

            subtotalMock.Raise(n => n.ValueChanged += null, EventArgs.Empty);
            totalOverrideMock.Raise(n => n.ValueChanged += null, EventArgs.Empty);

            Assert.AreEqual(2, invocations);
        }

        private static TotalNode CreateSut(ICalculationNode subtotal = null, ICalculationNode totalOverride = null) => 
            new TotalNode(subtotal ?? Mock.Of<ICalculationNode>(), totalOverride ?? Mock.Of<ICalculationNode>());
    }
}