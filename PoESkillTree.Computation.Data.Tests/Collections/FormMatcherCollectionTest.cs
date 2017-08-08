using System.Linq;
using Moq;
using NUnit.Framework;
using PoESkillTree.Computation.Providers.Forms;
using PoESkillTree.Computation.Providers.Values;
using PoESkillTree.Computation.Data.Collections;

namespace PoESkillTree.Computation.Data.Tests.Collections
{
    [TestFixture]
    public class FormMatcherCollectionTest
    {
        private Mock<IValueProviderFactory> _valueFactory;
        private FormMatcherCollection _sut;

        [SetUp]
        public void SetUp()
        {
            _valueFactory = new Mock<IValueProviderFactory>();
            _sut = new FormMatcherCollection(new MatchBuilderStub(), _valueFactory.Object);
        }

        [Test]
        public void IsEmpty()
        {
            Assert.IsEmpty(_sut);
        }

        [Test]
        public void AddAddsToCount()
        {
            var form = Mock.Of<IFormProvider>();
            _sut.Add("", form);
            _sut.Add("", form);
            _sut.Add("", form);

            Assert.AreEqual(3, _sut.Count());
        }

        [Test]
        public void AddWithoutValueAddsCorrectMatcherData()
        {
            var form = Mock.Of<IFormProvider>();

            _sut.Add("regex", form);

            var data = _sut.Single();
            Assert.AreEqual("regex", data.Regex);
            Assert.IsInstanceOf<MatchBuilderStub>(data.MatchBuilder);
            var builder = (MatchBuilderStub) data.MatchBuilder;
            Assert.That(builder.Forms, Has.Exactly(1).SameAs(form));
        }

        [Test]
        public void AddWithValueAddsCorrectMatcherData()
        {
            var form = Mock.Of<IFormProvider>();
            var value = new ValueProvider(Mock.Of<IValueProvider>());
            _valueFactory.Setup(v => v.Create(3)).Returns(value);

            _sut.Add("regex", form, 3);

            var data = _sut.Single();
            Assert.AreEqual("regex", data.Regex);
            Assert.IsInstanceOf<MatchBuilderStub>(data.MatchBuilder);
            var builder = (MatchBuilderStub) data.MatchBuilder;
            Assert.That(builder.Forms, Has.Exactly(1).SameAs(form));
            Assert.AreEqual(1, builder.Values.Count());
            Assert.AreSame(value, builder.Values.Single());
        }
    }
}