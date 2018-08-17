using System.Linq;
using Moq;
using NUnit.Framework;
using PoESkillTree.Computation.Common.Builders.Values;
using PoESkillTree.Computation.Data.Collections;
using PoESkillTree.Utils;

namespace PoESkillTree.Computation.Data.Tests.Collections
{
    [TestFixture]
    public class ValueConversionMatcherCollectionTest
    {
        private const string Regex = "regex";

        private ValueConversionMatcherCollection _sut;

        [SetUp]
        public void SetUp()
        {
            _sut = new ValueConversionMatcherCollection(new ModifierBuilderStub());
        }

        [Test]
        public void IsEmpty()
        {
            Assert.IsEmpty(_sut);
        }

        [Test]
        public void Add()
        {
            var inputValue = new ValueBuilder(Mock.Of<IValueBuilder>());
            var expectedValue = new ValueBuilder(Mock.Of<IValueBuilder>());

            _sut.Add(Regex, _ => expectedValue);

            var builder = _sut.AssertSingle(Regex);
            var actualValue = builder.ValueConverter(inputValue);
            Assert.AreEqual(expectedValue, actualValue);
        }

        [Test]
        public void AddManyAddsToCount()
        {
            _sut.Add(Regex, Funcs.Identity);
            _sut.Add(Regex, Funcs.Identity);
            _sut.Add(Regex, Funcs.Identity);

            Assert.AreEqual(3, _sut.Count());
        }
    }
}