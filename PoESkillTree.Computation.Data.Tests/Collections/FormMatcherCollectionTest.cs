using System.Linq;
using Moq;
using NUnit.Framework;
using PoESkillTree.Computation.Common.Builders.Forms;
using PoESkillTree.Computation.Common.Builders.Values;
using PoESkillTree.Computation.Data.Collections;

namespace PoESkillTree.Computation.Data.Tests.Collections
{
    [TestFixture]
    public class FormMatcherCollectionTest
    {
        private Mock<IValueBuilders> _valueFactory;
        private FormMatcherCollection _sut;

        [SetUp]
        public void SetUp()
        {
            _valueFactory = new Mock<IValueBuilders>();
            _sut = new FormMatcherCollection(new ModifierBuilderStub(), _valueFactory.Object);
        }

        [Test]
        public void IsEmpty()
        {
            Assert.IsEmpty(_sut);
        }

        [Test]
        public void AddAddsToCount()
        {
            var form = Mock.Of<IFormBuilder>();
            var value = Mock.Of<IValueBuilder>();
            _valueFactory.Setup(v => v.Create(1)).Returns(value);
            _sut.Add("", form, 1);
            _sut.Add("", form, 1);
            _sut.Add("", form, 1);

            Assert.AreEqual(3, _sut.Count());
        }

        [Test]
        public void AddWithValueBuilderAddsCorrectMatcherData()
        {
            var form = Mock.Of<IFormBuilder>();
            var value = Mock.Of<IValueBuilder>();

            _sut.Add("regex", form, value);

            var data = _sut.Single();
            Assert.AreEqual("regex", data.Regex);
            Assert.IsInstanceOf<ModifierBuilderStub>(data.Modifier);
            var builder = (ModifierBuilderStub) data.Modifier;
            Assert.That(builder.Forms, Has.Exactly(1).SameAs(form));
            Assert.AreEqual(1, builder.Values.Count());
            Assert.AreSame(value, builder.Values.Single());
        }

        [Test]
        public void AddWithDoubleValueAddsCorrectMatcherData()
        {
            var form = Mock.Of<IFormBuilder>();
            var value = Mock.Of<IValueBuilder>();
            _valueFactory.Setup(v => v.Create(3)).Returns(value);

            _sut.Add("regex", form, 3);

            var data = _sut.Single();
            Assert.AreEqual("regex", data.Regex);
            Assert.IsInstanceOf<ModifierBuilderStub>(data.Modifier);
            var builder = (ModifierBuilderStub) data.Modifier;
            Assert.That(builder.Forms, Has.Exactly(1).SameAs(form));
            Assert.AreEqual(1, builder.Values.Count());
            Assert.AreSame(value, builder.Values.Single());
        }
    }
}