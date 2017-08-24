using System.Linq;
using Moq;
using NUnit.Framework;
using PoESkillTree.Computation.Data.Collections;
using PoESkillTree.Computation.Parsing.Builders.Forms;
using PoESkillTree.Computation.Parsing.Builders.Stats;

namespace PoESkillTree.Computation.Data.Tests.Collections
{
    [TestFixture]
    public class GivenStatCollectionTest
    {
        private GivenStatCollection _sut;

        [SetUp]
        public void SetUp()
        {
            _sut = new GivenStatCollection();
        }

        [Test]
        public void IsEmpty()
        {
            Assert.AreEqual(0, _sut.Count());
        }

        [Test]
        public void AddAddsCorrectData()
        {
            var form = Mock.Of<IFormBuilder>();
            var stat = Mock.Of<IStatBuilder>();

            _sut.Add(form, stat, 3);

            var data = _sut.Single();
            Assert.AreSame(form, data.Form);
            Assert.AreSame(stat, data.Stat);
            Assert.AreEqual(3, data.Value);
        }

        [Test]
        public void AddManyAddsToCount()
        {
            var form = Mock.Of<IFormBuilder>();
            var stat = Mock.Of<IStatBuilder>();

            _sut.Add(form, stat, 1);
            _sut.Add(form, stat, 2);
            _sut.Add(form, stat, 3);

            Assert.AreEqual(3, _sut.Count());
        }
    }
}