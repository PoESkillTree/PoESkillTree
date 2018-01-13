using System.Linq;
using Moq;
using NUnit.Framework;
using PoESkillTree.Computation.Common.Builders.Stats;
using PoESkillTree.Computation.Common.Builders.Values;
using PoESkillTree.Computation.Data.Collections;

namespace PoESkillTree.Computation.Data.Tests.Collections
{
    [TestFixture]
    public class PropertyMatcherCollectionTest
    {
        private const string Regex = "regex";

        private Mock<IValueBuilders> _valueFactory;
        private PropertyMatcherCollection _sut;

        [SetUp]
        public void SetUp()
        {
            _valueFactory = new Mock<IValueBuilders>();
            _sut = new PropertyMatcherCollection(new ModifierBuilderStub(), _valueFactory.Object);
        }

        [Test]
        public void IsEmpty()
        {
            Assert.IsEmpty(_sut);
        }

        [Test]
        public void AddWithoutStat()
        {
            _sut.Add(Regex);

            _sut.AssertSingle(Regex);
        }

        [Test]
        public void AddWithStat()
        {
            var stat = Mock.Of<IStatBuilder>();

            _sut.Add(Regex, stat);

            var builder = _sut.AssertSingle(Regex);
            Assert.That(builder.Stats, Has.Exactly(1).SameAs(stat));
        }

        [Test]
        public void AddWithStatAndConverter()
        {
            var stat = Mock.Of<IStatBuilder>();
            var converter = _valueFactory.SetupConverter();

            _sut.Add(Regex, stat, converter);

            var builder = _sut.AssertSingle(Regex);
            Assert.That(builder.Stats, Has.Exactly(1).SameAs(stat));
            Assert.AreSame(converter, builder.ValueConverter);
        }

        [Test]
        public void AddManyAddsToCount()
        {
            var stat = Mock.Of<IStatBuilder>();
            var converter = _valueFactory.SetupConverter();

            _sut.Add(Regex);
            _sut.Add(Regex, stat);
            _sut.Add(Regex, stat, converter);

            Assert.AreEqual(3, _sut.Count());
        }
    }
}