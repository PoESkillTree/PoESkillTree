using System;
using Moq;
using NUnit.Framework;
using PoESkillTree.Computation.Common;
using PoESkillTree.Computation.Common.Tests;
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

        public static IDisposableNodeViewProvider MockDisposableNodeProvider() =>
            Mock.Of<IDisposableNodeViewProvider>(p =>
                p.DefaultView == MockNode(0) && p.BufferingView == MockNode(0));

        public static IBufferingEventViewProvider<ICalculationNode> MockNodeProvider(
            ICalculationNode defaultNode = null, ICalculationNode bufferingView = null)
        {
            defaultNode = defaultNode ?? MockNode();
            bufferingView = bufferingView ?? MockNode();
            return Mock.Of<IBufferingEventViewProvider<ICalculationNode>>(
                p => p.DefaultView == defaultNode && p.BufferingView == bufferingView);
        }

        public static PathDefinition NotMainPath => new PathDefinition(new ModifierSource.Global(), new StatStub());
    }
}