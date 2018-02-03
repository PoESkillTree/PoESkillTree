using System;
using Moq;
using NUnit.Framework;
using PoESkillTree.Computation.Common;
using PoESkillTree.Computation.Core.Events;
using PoESkillTree.Computation.Core.Nodes;

namespace PoESkillTree.Computation.Core.Tests
{
    internal static class NodeHelper
    {
        public static IDisposableNode MockNode(double? value) => 
            MockNode((NodeValue?) value);

        public static IDisposableNode MockNode(NodeValue? value = null) => 
            Mock.Of<IDisposableNode>(n => n.Value == value);

        public static void AssertValueEquals(this ICalculationNode node, double? expected) => 
            Assert.AreEqual((NodeValue?) expected, node.Value);

        public static void SubscribeToValueChanged(this ICalculationNode node, Action handler) =>
            node.ValueChanged += (sender, args) =>
            {
                Assert.AreEqual(node, sender);
                handler();
            };

        public static void AssertValueChangedWillNotBeInvoked(this ICalculationNode node) => 
            node.SubscribeToValueChanged(Assert.Fail);

        public static void RaiseValueChanged(this Mock<IDisposableNode> nodeMock)
        {
            nodeMock.Raise(n => n.ValueChanged += null, EventArgs.Empty);
        }

        public static Modifier[] MockManyModifiers() => new[] { MockModifier(), MockModifier(), MockModifier() };

        public static Modifier MockModifier() => new Modifier(new IStat[0], Form.BaseAdd, Mock.Of<IValue>());

        public static ISuspendableEventViewProvider<IDisposableNode> MockNodeProvider() =>
            MockNodeProvider(null);

        public static ISuspendableEventViewProvider<IDisposableNode> MockNodeProvider(
            IDisposableNode defaultNode = null, IDisposableNode suspendableNode = null,
            ISuspendableEvents suspender = null)
        {
            defaultNode = defaultNode ?? MockNode();
            suspendableNode = suspendableNode ?? MockNode();
            suspender = suspender ?? Mock.Of<ISuspendableEvents>();
            return Mock.Of<ISuspendableEventViewProvider<IDisposableNode>>(
                p => p.DefaultView == defaultNode && p.SuspendableView == suspendableNode && p.Suspender == suspender);
        }
    }
}