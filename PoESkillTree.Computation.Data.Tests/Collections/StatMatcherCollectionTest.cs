using System.Linq;
using Moq;
using NUnit.Framework;
using PoESkillTree.Computation.Data.Collections;
using PoESkillTree.Computation.Providers.Conditions;
using PoESkillTree.Computation.Providers.Stats;
using PoESkillTree.Computation.Providers.Values;

namespace PoESkillTree.Computation.Data.Tests.Collections
{
    [TestFixture]
    public class StatMatcherCollectionTest
    {
        private const string Regex = "regex";

        private StatMatcherCollection _sut;

        [SetUp]
        public void SetUp()
        {
            _sut = new StatMatcherCollection(new MatchBuilderStub());
        }

        [Test]
        public void IsEmpty()
        {
            Assert.IsEmpty(_sut);
        }

        [Test]
        public void AddSingle()
        {
            var stat = Mock.Of<IStatProvider>();

            _sut.Add(Regex, stat);

            var builder = _sut.AssertSingle(Regex);
            Assert.That(builder.Stats, Has.Exactly(1).SameAs(stat));
        }

        [Test]
        public void AddArray()
        {
            var stat1 = Mock.Of<IStatProvider>();
            var stat2 = Mock.Of<IStatProvider>();
            var stat3 = Mock.Of<IStatProvider>();

            _sut.Add(Regex, stat1, stat2, stat3);

            var builder = _sut.AssertSingle(Regex);
            CollectionAssert.AreEqual(new[] {stat1, stat2, stat3}, builder.Stats);
        }

        [Test]
        public void AddEnumerable()
        {
            var stats = Enumerable.Empty<IStatProvider>();

            _sut.Add(Regex, stats);

            var builder = _sut.AssertSingle(Regex);
            Assert.AreSame(stats, builder.Stats);
        }

        [Test]
        public void AddWithSubstitution()
        {
            var stat = Mock.Of<IStatProvider>();

            _sut.Add(Regex, stat, "substitution");

            var builder = _sut.AssertSingle(Regex, "substitution");
            Assert.That(builder.Stats, Has.Exactly(1).SameAs(stat));
        }

        [Test]
        public void AddWithCondition()
        {
            var stat = Mock.Of<IStatProvider>();
            var condition = Mock.Of<IConditionProvider>();

            _sut.Add(Regex, stat, condition);

            var builder = _sut.AssertSingle(Regex);
            Assert.That(builder.Stats, Has.Exactly(1).SameAs(stat));
            Assert.That(builder.Conditions, Has.Exactly(1).SameAs(condition));
        }

        [Test]
        public void AddWithConverter()
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
            var condition = Mock.Of<IConditionProvider>();
            ValueFunc converter = v => null;

            _sut.Add(Regex, stat);
            _sut.Add(Regex, stat, stat);
            _sut.Add(Regex, Enumerable.Empty<IStatProvider>());
            _sut.Add(Regex, stat, "substitution");
            _sut.Add(Regex, stat, condition);
            _sut.Add(Regex, stat, converter);

            Assert.AreEqual(6, _sut.Count());
        }
    }
}