using System;
using System.Linq;
using Moq;
using NUnit.Framework;
using PoESkillTree.Computation.Common;

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

        public static INodeCollection MockNodeCollection(params double?[] values)
        {
            var items = values.Select(MockNode).ToList();
            var mock = new Mock<INodeCollection>();
            mock.Setup(c => c.GetEnumerator()).Returns(() => items.GetEnumerator());
            mock.Setup(c => c.Count).Returns(items.Count);
            return mock.Object;
        }

        public static void RaiseValueChanged(this Mock<IDisposableNode> nodeMock)
        {
            nodeMock.Raise(n => n.ValueChanged += null, EventArgs.Empty);
        }

        public static Modifier[] MockManyModifiers() => new[] { MockModifier(), MockModifier(), MockModifier() };

        public static Modifier MockModifier() => new Modifier(new IStat[0], Form.BaseAdd, Mock.Of<IValue>());
    }
}