using System;
using System.Linq;
using Moq;
using NUnit.Framework;

namespace PoESkillTree.Computation.Core.Tests
{
    internal static class NodeHelper
    {
        public static ICalculationNode MockNode(double? value) => 
            MockNode((NodeValue?) value);

        public static ICalculationNode MockNode(NodeValue? value)
        {
            var mock = new Mock<ICalculationNode>();
            mock.SetupGet(n => n.Value).Returns(value);
            return mock.Object;
        }

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

        public static IFormNodeCollection MockFormNodeCollection(params double?[] values)
        {
            var items = values
                .Select(v => new FormNodeCollectionItem(NodeHelper.MockNode(v), null, null))
                .ToList();
            return Mock.Of<IFormNodeCollection>(c => c.Items == items);
        }
    }
}