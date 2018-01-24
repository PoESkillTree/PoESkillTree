using Moq;
using NUnit.Framework;
using PoESkillTree.Computation.Common;

namespace PoESkillTree.Computation.Core.Tests
{
    [TestFixture]
    public class ValueNodeTest
    {
        [Test]
        public void SutIsCalculationNode()
        {
            var sut = CreateSut();

            Assert.IsInstanceOf<ICalculationNode>(sut);
        }

        [TestCase(42, 0)]
        [TestCase(3, 5)]
        public void ValueReturnsIValueCalculateValue(double value1, double value2)
        {
            var (expected, sut) = CreateSut(value1, value2);

            sut.AssertValueEquals(expected);
        }

        [Test]
        public void ValueChangedIsRaisedWhenNodesUsedInIValueRaiseValueChanged()
        {
            var node1 = NodeHelper.MockNode(0);
            var node2 = NodeHelper.MockNode(0);
            var sut = CreateSut(node1, node2);
            var invocations = 0;
            sut.SubscribeToValueChanged(() => invocations++);

            var _ = sut.Value;
            Mock.Get(node1).RaiseValueChanged();
            Mock.Get(node2).RaiseValueChanged();

            Assert.AreEqual(2, invocations);
        }

        [Test]
        public void ValueUnsubscribesFromNodesNoLongerUsed()
        {
            var node1 = NodeHelper.MockNode(0);
            var node2 = NodeHelper.MockNode(0);
            var node3 = NodeHelper.MockNode(0);
            var stat = new StatStub();
            var nodeRepositoryMock = new Mock<INodeRepository>();
            nodeRepositoryMock.Setup(r => r.GetNode(stat, NodeType.Total)).Returns(node1);
            nodeRepositoryMock.Setup(r => r.GetNode(stat, NodeType.Base)).Returns(node2);
            nodeRepositoryMock.Setup(r => r.GetNode(stat, NodeType.BaseSet)).Returns(node3);
            var valueMock = new Mock<IValue>();
            valueMock.Setup(v => v.Calculate(It.IsAny<IValueCalculationContext>()))
                .Returns((IValueCalculationContext c) => c.GetValue(stat) + c.GetValue(stat, NodeType.Base));
            var sut = CreateSut(nodeRepositoryMock.Object, valueMock.Object);
            var _ = sut.Value;

            valueMock.Setup(v => v.Calculate(It.IsAny<IValueCalculationContext>()))
                .Returns((IValueCalculationContext c) =>
                    c.GetValue(stat, NodeType.BaseSet) + c.GetValue(stat, NodeType.Base));
            var invocations = 0;
            sut.SubscribeToValueChanged(() => invocations++);

            _ = sut.Value;

            Mock.Get(node1).RaiseValueChanged();
            Assert.AreEqual(0, invocations);
            Mock.Get(node2).RaiseValueChanged();
            Mock.Get(node3).RaiseValueChanged();
            Assert.AreEqual(2, invocations);
        }

        [Test]
        public void DisposeUnsubscribesFromHandlers()
        {
            var node1 = NodeHelper.MockNode(0);
            var node2 = NodeHelper.MockNode(0);
            var sut = CreateSut(node1, node2);
            var _ = sut.Value;

            sut.Dispose();

            sut.AssertValueChangedWillNotBeInvoked();
            Mock.Get(node1).RaiseValueChanged();
            Mock.Get(node2).RaiseValueChanged();
        }

        [Test]
        public void DisposeDoesNothingIfValueWasNotAccessed()
        {
            var (_, sut) = CreateSut(0, 0);
            sut.AssertValueChangedWillNotBeInvoked();

            sut.Dispose();
        }

        private static (double expected, ValueNode) CreateSut(double stat1Value, double stat2Value)
        {
            var sut = CreateSut(NodeHelper.MockNode(stat1Value), NodeHelper.MockNode(stat2Value));
            return (stat1Value + stat2Value, sut);
        }

        private static ValueNode CreateSut(ICalculationNode node1, ICalculationNode node2)
        {
            var stat1 = new StatStub();
            var stat2 = new StatStub();
            var nodeRepository = Mock.Of<INodeRepository>(r =>
                r.GetNode(stat1, NodeType.Total) == node1 &&
                r.GetNode(stat2, NodeType.Base) == node2);
            var valueMock = new Mock<IValue>();
            valueMock.Setup(v => v.Calculate(It.IsAny<IValueCalculationContext>()))
                .Returns((IValueCalculationContext c) => c.GetValue(stat1) + c.GetValue(stat2, NodeType.Base));
            return CreateSut(nodeRepository, valueMock.Object);
        }

        private static ValueNode CreateSut(INodeRepository nodeRepository = null, IValue value = null) =>
            new ValueNode(nodeRepository, value);
    }
}