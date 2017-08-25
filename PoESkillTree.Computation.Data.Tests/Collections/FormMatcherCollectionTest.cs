using System.Linq;
using Moq;
using NUnit.Framework;
using PoESkillTree.Computation.Data.Collections;
using PoESkillTree.Computation.Parsing.Builders.Forms;
using PoESkillTree.Computation.Parsing.Builders.Values;

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
            _sut.Add("", form);
            _sut.Add("", form);
            _sut.Add("", form);

            Assert.AreEqual(3, _sut.Count());
        }

        [Test]
        public void AddWithoutValueAddsCorrectMatcherData()
        {
            var form = Mock.Of<IFormBuilder>();

            _sut.Add("regex", form);

            var data = _sut.Single();
            Assert.AreEqual("regex", data.Regex);
            Assert.IsInstanceOf<ModifierBuilderStub>(data.ModifierBuilder);
            var builder = (ModifierBuilderStub) data.ModifierBuilder;
            Assert.That(builder.Forms, Has.Exactly(1).SameAs(form));
        }

        [Test]
        public void AddWithValueAddsCorrectMatcherData()
        {
            var form = Mock.Of<IFormBuilder>();
            var value = new ValueBuilder(Mock.Of<IValueBuilder>(), null);
            _valueFactory.Setup(v => v.Create(3)).Returns(value);

            _sut.Add("regex", form, 3);

            var data = _sut.Single();
            Assert.AreEqual("regex", data.Regex);
            Assert.IsInstanceOf<ModifierBuilderStub>(data.ModifierBuilder);
            var builder = (ModifierBuilderStub) data.ModifierBuilder;
            Assert.That(builder.Forms, Has.Exactly(1).SameAs(form));
            Assert.AreEqual(1, builder.Values.Count());
            Assert.AreSame(value, builder.Values.Single());
        }
    }
}