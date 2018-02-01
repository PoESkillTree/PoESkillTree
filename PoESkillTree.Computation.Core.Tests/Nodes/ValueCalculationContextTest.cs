using Moq;
using MoreLinq;
using NUnit.Framework;
using PoESkillTree.Computation.Common;
using PoESkillTree.Computation.Core.Nodes;

namespace PoESkillTree.Computation.Core.Tests.Nodes
{
    [TestFixture]
    public class ValueCalculationContextTest
    {
        [Test]
        public void SutIsValueCalculationContext()
        {
            var sut = CreateSut();

            Assert.IsInstanceOf<IValueCalculationContext>(sut);
        }

        [TestCase(42)]
        [TestCase(-5)]
        public void GetValueReturnsNodeRepositoryValue(double value)
        {
            var expected = new NodeValue(value);
            var node = NodeHelper.MockNode(expected);
            var stat = Mock.Of<IStat>();
            var nodeRepository = Mock.Of<INodeRepository>(r => r.GetNode(stat, NodeType.Base) == node);
            var sut = CreateSut(nodeRepository);

            var actual = sut.GetValue(stat, NodeType.Base);

            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void CallsIsEmptyIfGetValueWasNotCalled()
        {
            var sut = CreateSut();

            var actual = sut.Calls;

            CollectionAssert.IsEmpty(actual);
        }

        [Test]
        public void CallsStoresGetValueCall()
        {
            var sut = CreateSut();
            var stat = new StatStub();

            sut.GetValue(stat, NodeType.Base);

            Assert.That(sut.Calls, Has.Exactly(1).Items.EqualTo((stat, NodeType.Base)));
        }

        [Test]
        public void CallsStoresAllGetValueCalls()
        {
            var sut = CreateSut();
            var expected = new[]
            {
                (new StatStub(), NodeType.Base),
                (new StatStub(), NodeType.Total),
                (new StatStub(), NodeType.TotalOverride),
            };
            
            expected.ForEach(t => sut.GetValue(t.Item1, t.Item2));

            Assert.AreEqual(expected, sut.Calls);
        }

        [Test]
        public void CallsIsSet()
        {
            var sut = CreateSut();
            var stat = new StatStub();

            sut.GetValue(stat);
            sut.GetValue(stat);

            Assert.That(sut.Calls, Has.Exactly(1).Items.EqualTo((stat, NodeType.Total)));
        }

        [Test]
        public void ClearClearsCalls()
        {
            var sut = CreateSut();
            sut.GetValue(new StatStub());

            sut.Clear();

            CollectionAssert.IsEmpty(sut.Calls);
        }


        private static ValueCalculationContext CreateSut(INodeRepository nodeRepository = null) =>
            new ValueCalculationContext(nodeRepository ?? Mock.Of<INodeRepository>());
    }
}