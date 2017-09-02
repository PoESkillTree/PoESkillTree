using System.Linq;
using Moq;
using NUnit.Framework;
using PoESkillTree.Computation.Data.Collections;
using PoESkillTree.Computation.Parsing.Builders.Values;

namespace PoESkillTree.Computation.Data.Tests.Collections
{
    [TestFixture]
    public class ValueConversionMatcherCollectionTest
    {
        private const string Regex = "regex";

        private Mock<IValueBuilders> _valueFactory;
        private ValueConversionMatcherCollection _sut;

        [SetUp]
        public void SetUp()
        {
            _valueFactory = new Mock<IValueBuilders>();
            _sut = new ValueConversionMatcherCollection(new ModifierBuilderStub(), _valueFactory.Object);
        }

        [Test]
        public void IsEmpty()
        {
            Assert.IsEmpty(_sut);
        }

        [Test]
        public void Add()
        {
            var converter = _valueFactory.SetupConverter();

            _sut.Add(Regex, converter);

            var builder = _sut.AssertSingle(Regex);
            Assert.AreSame(converter, builder.ValueConverter);
        }

        [Test]
        public void AddManyAddsToCount()
        {
            var converter = _valueFactory.SetupConverter();

            _sut.Add(Regex, converter);
            _sut.Add(Regex, converter);
            _sut.Add(Regex, converter);

            Assert.AreEqual(3, _sut.Count());
        }
    }
}