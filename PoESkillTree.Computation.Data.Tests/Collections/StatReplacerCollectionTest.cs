using System.Linq;
using NUnit.Framework;

namespace PoESkillTree.Computation.Data.Collections
{
    [TestFixture]
    public class StatReplacerCollectionTest
    {
        private StatReplacerCollection _sut;

        [SetUp]
        public void SetUp()
        {
            _sut = new StatReplacerCollection();
        }

        [Test]
        public void IsEmpty()
        {
            Assert.AreEqual(0, _sut.Count());
        }

        [Test]
        public void AddAddsCorrectData()
        {
            _sut.Add("originalStat", "r1", "r2", "r3");

            var data = _sut.Single();
            Assert.AreEqual("originalStat", data.OriginalStatRegex);
            CollectionAssert.AreEqual(new[] { "r1", "r2", "r3" }, data.Replacements);
        }

        [Test]
        public void AddManyAddsToCount()
        {
            _sut.Add("1");
            _sut.Add("2", "r1");
            _sut.Add("3", "r1", "r2");

            Assert.AreEqual(3, _sut.Count());
        }
    }
}