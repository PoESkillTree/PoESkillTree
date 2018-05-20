using System.Linq;
using Moq;
using NUnit.Framework;
using PoESkillTree.Common.Utils;
using PoESkillTree.Computation.Common.Builders.Stats;
using PoESkillTree.Computation.Common.Builders.Values;
using PoESkillTree.Computation.Data.Collections;

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
            _sut = new PropertyMatcherCollection(new ModifierBuilderStub());
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
            var inputValue = new ValueBuilder(Mock.Of<IValueBuilder>());
            var expectedValue = new ValueBuilder(Mock.Of<IValueBuilder>());

            _sut.Add(Regex, stat, _ => expectedValue);

            var builder = _sut.AssertSingle(Regex);
            Assert.That(builder.Stats, Has.Exactly(1).SameAs(stat));
            var actualValue = builder.ValueConverter(inputValue);
            Assert.AreEqual(expectedValue, actualValue);
        }

        [Test]
        public void AddManyAddsToCount()
        {
            var stat = Mock.Of<IStatBuilder>();

            _sut.Add(Regex);
            _sut.Add(Regex, stat);
            _sut.Add(Regex, stat, Funcs.Identity);

            Assert.AreEqual(3, _sut.Count());
        }
    }
}