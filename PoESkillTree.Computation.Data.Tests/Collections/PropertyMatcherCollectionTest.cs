using System.Linq;
using Moq;
using NUnit.Framework;
using PoESkillTree.Computation.Data.Collections;
using PoESkillTree.Computation.Providers.Stats;
using PoESkillTree.Computation.Providers.Values;

namespace PoESkillTree.Computation.Data.Tests.Collections
{
    [TestFixture]
    public class PropertyMatcherCollectionTest
    {
        private const string Regex = "regex";

        private PropertyMatcherCollection _sut;

        [SetUp]
        public void SetUp()
        {
            _sut = new PropertyMatcherCollection(new MatchBuilderStub());
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
            var stat = Mock.Of<IStatProvider>();

            _sut.Add(Regex, stat);

            var builder = _sut.AssertSingle(Regex);
            Assert.That(builder.Stats, Has.Exactly(1).SameAs(stat));
        }

        [Test]
        public void AddWithStatAndConverter()
        {
            var stat = Mock.Of<IStatProvider>();
            ValueFunc converter = v => null;

            _sut.Add(Regex, stat, converter);

            var builder = _sut.AssertSingle(Regex);
            Assert.That(builder.Stats, Has.Exactly(1).SameAs(stat));
            Assert.AreSame(converter, builder.ValueConverter);
        }

        [Test]
        public void AddManyAddsToCount()
        {
            var stat = Mock.Of<IStatProvider>();
            ValueFunc converter = v => null;

            _sut.Add(Regex);
            _sut.Add(Regex, stat);
            _sut.Add(Regex, stat, converter);

            Assert.AreEqual(3, _sut.Count());
        }
    }
}