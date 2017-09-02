using System.Linq;
using Moq;
using NUnit.Framework;
using PoESkillTree.Computation.Data.Collections;
using PoESkillTree.Computation.Parsing.Builders.Conditions;
using PoESkillTree.Computation.Parsing.Builders.Stats;
using PoESkillTree.Computation.Parsing.Builders.Values;

namespace PoESkillTree.Computation.Data.Tests.Collections
{
    [TestFixture]
    public class StatMatcherCollectionTest
    {
        private const string Regex = "regex";

        private Mock<IValueBuilders> _valueFactory;
        private StatMatcherCollection _sut;

        [SetUp]
        public void SetUp()
        {
            _valueFactory = new Mock<IValueBuilders>();
            _sut = new StatMatcherCollection(new ModifierBuilderStub(), _valueFactory.Object);
        }

        [Test]
        public void IsEmpty()
        {
            Assert.IsEmpty(_sut);
        }

        [Test]
        public void AddSingle()
        {
            var stat = Mock.Of<IStatBuilder>();

            _sut.Add(Regex, stat);

            var builder = _sut.AssertSingle(Regex);
            Assert.That(builder.Stats, Has.Exactly(1).SameAs(stat));
        }

        [Test]
        public void AddArray()
        {
            var stat1 = Mock.Of<IStatBuilder>();
            var stat2 = Mock.Of<IStatBuilder>();
            var stat3 = Mock.Of<IStatBuilder>();

            _sut.Add(Regex, stat1, stat2, stat3);

            var builder = _sut.AssertSingle(Regex);
            CollectionAssert.AreEqual(new[] {stat1, stat2, stat3}, builder.Stats);
        }

        [Test]
        public void AddEnumerable()
        {
            var stats = Enumerable.Empty<IStatBuilder>();

            _sut.Add(Regex, stats);

            var builder = _sut.AssertSingle(Regex);
            Assert.AreSame(stats, builder.Stats);
        }

        [Test]
        public void AddWithSubstitution()
        {
            var stat = Mock.Of<IStatBuilder>();

            _sut.Add(Regex, stat, "substitution");

            var builder = _sut.AssertSingle(Regex, "substitution");
            Assert.That(builder.Stats, Has.Exactly(1).SameAs(stat));
        }

        [Test]
        public void AddWithCondition()
        {
            var stat = Mock.Of<IStatBuilder>();
            var condition = Mock.Of<IConditionBuilder>();

            _sut.Add(Regex, stat, condition);

            var builder = _sut.AssertSingle(Regex);
            Assert.That(builder.Stats, Has.Exactly(1).SameAs(stat));
            Assert.That(builder.Conditions, Has.Exactly(1).SameAs(condition));
        }

        [Test]
        public void AddWithConverter()
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
            var condition = Mock.Of<IConditionBuilder>();
            var converter = _valueFactory.SetupConverter();

            _sut.Add(Regex, stat);
            _sut.Add(Regex, stat, stat);
            _sut.Add(Regex, Enumerable.Empty<IStatBuilder>());
            _sut.Add(Regex, stat, "substitution");
            _sut.Add(Regex, stat, condition);
            _sut.Add(Regex, stat, converter);

            Assert.AreEqual(6, _sut.Count());
        }
    }
}