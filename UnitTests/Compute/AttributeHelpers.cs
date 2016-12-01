using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UnitTests.Compute
{
    public class AttributeHelpers
    {
        public static void CheckEquality(IEnumerable<float> expected, IEnumerable<float> actual)
        {
            for (int i = 0; i < expected.Count(); i++)
                Assert.AreEqual(expected.Skip(i).First(), actual.Skip(i).First());
        }

        public static void CheckEquality(float expected, IEnumerable<float> actual)
        {
            Assert.AreEqual(expected, actual.First());
        }

        public static void CheckEquality(float expected, float actual)
        {
            Assert.AreEqual(expected, actual);
        }

        public static void EqualWithinMargin(float expected, float actual, float margin = 0.01f)
        {
            Assert.IsTrue(expected - actual < margin);
        }
    }
}
