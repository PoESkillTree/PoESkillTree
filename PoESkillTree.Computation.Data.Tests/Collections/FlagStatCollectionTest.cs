using System.Linq;
using Moq;
using NUnit.Framework;
using PoESkillTree.Computation.Common.Builders.Stats;
using PoESkillTree.Computation.Data.Collections;

namespace PoESkillTree.Computation.Data.Tests.Collections
{
    [TestFixture]
    public class FlagStatCollectionTest
    {
        private FlagStatCollection _sut;

        [SetUp]
        public void SetUp()
        {
            _sut = new FlagStatCollection();
        }

        [Test]
        public void IsEmpty()
        {
            CollectionAssert.IsEmpty(_sut);
        }

        [Test]
        public void AddAddsCorrectData()
        {
            var flag = Mock.Of<IFlagStatBuilder>();

            _sut.Add(flag, "s1", "s2", "s3");

            var data = _sut.Single();
            Assert.AreSame(flag, data.Flag);
            CollectionAssert.AreEqual(new[] { "s1", "s2", "s3" }, data.StatLines);
        }

        [Test]
        public void AddManyAddsToCount()
        {
            var flag = Mock.Of<IFlagStatBuilder>();

            _sut.Add(flag);
            _sut.Add(flag, "s1");
            _sut.Add(flag, "s1", "s2", "s3");

            Assert.AreEqual(3, _sut.Count());
        }
    }
}