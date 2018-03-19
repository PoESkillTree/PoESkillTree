using System;
using System.Collections.Generic;
using Moq;
using NUnit.Framework;
using PoESkillTree.Computation.Common;
using PoESkillTree.Computation.Core.Events;
using PoESkillTree.Computation.Core.Graphs;

namespace PoESkillTree.Computation.Core.Tests.Graphs
{
    [TestFixture]
    public class StatGraphWithEventsTest
    {
        [Test]
        public void SutIsStatGraph()
        {
            var sut = CreateSut();

            Assert.IsInstanceOf<IStatGraph>(sut);
        }

        [Test]
        public void GetNodeCallsNodeAddedAction()
        {
            var expected = NodeType.Base;
            var called = false;
            var sut = CreateSut(actual =>
            {
                Assert.AreEqual(expected, actual);
                called = true;
            });

            sut.GetNode(expected);

            Assert.IsTrue(called);
        }

        [Test]
        public void GetNodeDoesNotCallNodeAddedActionIfNodeTypeIsInNodes()
        {
            var nodeType = NodeType.BaseAdd;
            var decoratedGraph = Mock.Of<IStatGraph>(g => g.Nodes.ContainsKey(nodeType));
            var sut = CreateSut(decoratedGraph, _ => Assert.Fail());

            sut.GetNode(nodeType);
        }

        [Test]
        public void GetNodeCallsActionAfterCallingDecoratedGraph()
        {
            var nodeType = NodeType.BaseAdd;
            var graphMock = new Mock<IStatGraph>();
            graphMock.Setup(g => g.Nodes.ContainsKey(nodeType)).Returns(false);
            var sut = CreateSut(graphMock.Object, _ =>
            {
                graphMock.Setup(g => g.GetNode(nodeType)).Throws(new AssertionException("GetNode called after action"));
            });

            sut.GetNode(nodeType);
        }

        [Test]
        public void RemoveNodeCallsNodeRemovedAction()
        {
            var expected = NodeType.Base;
            var called = false;
            var sut = CreateSut(nodeRemovedAction: actual =>
            {
                Assert.AreEqual(expected, actual);
                called = true;
            });

            sut.RemoveNode(expected);

            Assert.IsTrue(called);
        }

        private static StatGraphWithEvents CreateSut(
            Action<NodeType> nodeAddedAction = null, Action<NodeType> nodeRemovedAction = null)
        {
            var nodes = new Dictionary<NodeType, ISuspendableEventViewProvider<ICalculationNode>>();
            return CreateSut(Mock.Of<IStatGraph>(g => g.Nodes == nodes), nodeAddedAction, nodeRemovedAction);
        }

        private static StatGraphWithEvents CreateSut(IStatGraph decoratedGraph, 
            Action<NodeType> nodeAddedAction = null, Action<NodeType> nodeRemovedAction = null)
        {
            return new StatGraphWithEvents(decoratedGraph, nodeAddedAction, nodeRemovedAction);
        }
    }
}