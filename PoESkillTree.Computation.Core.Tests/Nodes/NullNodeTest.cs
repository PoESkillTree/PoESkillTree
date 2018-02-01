using NUnit.Framework;
using PoESkillTree.Computation.Core.Nodes;

namespace PoESkillTree.Computation.Core.Tests.Nodes
{
    [TestFixture]
    public class NullNodeTest
    {
        [Test]
        public void SutIsCalculationNode()
        {
            var sut = new NullNode();

            Assert.IsInstanceOf<ICalculationNode>(sut);
        }

        [Test]
        public void ValueIsNull()
        {
            var sut = new NullNode();

            var actual = sut.Value;

            Assert.IsNull(actual);
        }
    }
}