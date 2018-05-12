using System;
using System.Linq;
using Moq;
using NUnit.Framework;
using PoESkillTree.Computation.Common;
using PoESkillTree.Computation.Core.Events;
using PoESkillTree.Computation.Core.Nodes;

namespace PoESkillTree.Computation.Core.Tests
{
    internal static class NodeHelper
    {
        public static ICalculationNode MockNode(double? value) => 
            MockNode((NodeValue?) value);

        public static ICalculationNode MockNode(NodeValue? value = null) => 
            Mock.Of<ICalculationNode>(n => n.Value == value);

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

        public static void RaiseValueChanged(this Mock<ICalculationNode> nodeMock)
        {
            nodeMock.Raise(n => n.ValueChanged += null, EventArgs.Empty);
        }

        public static Modifier[] MockManyModifiers() => new[] { MockModifier(), MockModifier(), MockModifier() };

        public static Modifier MockModifier() => new Modifier(new IStat[0], Form.BaseAdd, Mock.Of<IValue>());

        public static IDisposableNodeViewProvider MockDisposableNodeProvider() =>
            Mock.Of<IDisposableNodeViewProvider>(p =>
                p.DefaultView == MockNode(0) && p.SuspendableView == MockNode(0) &&
                p.Suspender == Mock.Of<ISuspendableEvents>());

        public static ISuspendableEventViewProvider<ICalculationNode> MockNodeProvider(
            ICalculationNode defaultNode = null, ICalculationNode suspendableNode = null,
            ISuspendableEvents suspender = null)
        {
            defaultNode = defaultNode ?? MockNode();
            suspendableNode = suspendableNode ?? MockNode();
            suspender = suspender ?? Mock.Of<ISuspendableEvents>();
            return Mock.Of<ISuspendableEventViewProvider<ICalculationNode>>(
                p => p.DefaultView == defaultNode && p.SuspendableView == suspendableNode && p.Suspender == suspender);
        }

        public static T[] MockMany<T>(int count = 3) where T : class =>
            Enumerable.Range(0, count).Select(_ => Mock.Of<T>()).ToArray();

        public static PathDefinition NotMainPath => new PathDefinition(new GlobalModifierSource(), new StatStub());
    }
}