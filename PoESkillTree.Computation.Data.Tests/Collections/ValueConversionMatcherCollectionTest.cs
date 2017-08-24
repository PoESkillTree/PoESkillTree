using System.Linq;
using NUnit.Framework;
using PoESkillTree.Computation.Data.Collections;
using PoESkillTree.Computation.Parsing.Builders.Values;

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
            ValueFunc converter = v => null;

            _sut.Add(Regex, converter);

            var builder = _sut.AssertSingle(Regex);
            Assert.AreSame(converter, builder.ValueConverter);
        }

        [Test]
        public void AddManyAddsToCount()
        {
            _sut.Add(Regex, v => null);
            _sut.Add(Regex, v => null);
            _sut.Add(Regex, v => null);

            Assert.AreEqual(3, _sut.Count());
        }
    }
}