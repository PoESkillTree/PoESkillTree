using System.Linq;
using Moq;
using NUnit.Framework;
using PoESkillTree.Computation.Common.Builders.Forms;
using PoESkillTree.Computation.Common.Builders.Stats;
using PoESkillTree.Computation.Common.Builders.Values;
using PoESkillTree.Computation.Data.Collections;

namespace PoESkillTree.Computation.Data.Tests.Collections
{
    [TestFixture]
    public class GivenStatCollectionTest
    {
        private Mock<IValueBuilders> _valueFactory;
        private GivenStatCollection _sut;

        [SetUp]
        public void SetUp()
        {
            _valueFactory = new Mock<IValueBuilders>();
            _sut = new GivenStatCollection(new ModifierBuilderStub(), _valueFactory.Object);
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
            var value = Mock.Of<IValueBuilder>();
            _valueFactory.Setup(v => v.Create(3)).Returns(value);

            _sut.Add(form, stat, 3);

            var builder = _sut.AssertSingle();
            Assert.That(builder.Forms, Has.Exactly(1).SameAs(form));
            Assert.That(builder.Stats, Has.Exactly(1).SameAs(stat));
            Assert.That(builder.Values, Has.Exactly(1).SameAs(value));
        }

        [Test]
        public void AddManyAddsToCount()
        {
            var form = Mock.Of<IFormBuilder>();
            var stat = Mock.Of<IStatBuilder>();
            var value = Mock.Of<IValueBuilder>();
            _valueFactory.Setup(v => v.Create(3)).Returns(value);

            _sut.Add(form, stat, 3);
            _sut.Add(form, stat, 3);
            _sut.Add(form, stat, 3);

            Assert.AreEqual(3, _sut.Count());
        }
    }
}