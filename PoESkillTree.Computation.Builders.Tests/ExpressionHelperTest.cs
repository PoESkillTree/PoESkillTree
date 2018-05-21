using System;
using System.Linq.Expressions;
using NUnit.Framework;

namespace PoESkillTree.Computation.Builders.Tests
{
    [TestFixture]
    public class ExpressionHelperTest
    {
        [Test]
        public void ToStringSimple()
        {
            Expression<Func<int, int, int>> expression = (x, y) => 2 * x + y;

            var actual = expression.ToString(1, 2);

            Assert.AreEqual("((2 * (1)) + (2))", actual);
        }

        [Test]
        public void ToStringProperlyReplacesParameters()
        {
            Expression<Func<int, int>> expression = b => Math.Abs(b);

            var actual = expression.ToString(3);

            Assert.AreEqual("Abs((3))", actual);
        }
    }
}