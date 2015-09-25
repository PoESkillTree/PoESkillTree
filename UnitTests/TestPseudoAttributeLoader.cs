using Microsoft.VisualStudio.TestTools.UnitTesting;
using POESKillTree.TreeGenerator.Model.PseudoAttributes;

namespace UnitTests
{
    [TestClass]
    public class TestPseudoAttributeLoader
    {
        [TestMethod]
        public void TestCorrectCount()
        {
            var l = new PseudoAttributeLoader();
            var t = l.LoadPseudoAttributes();
            Assert.IsTrue(t.Count == 22);
        }
    }
}