using System.Linq;
using Moq;
using NUnit.Framework;
using PoESkillTree.Computation.Data.Collections;
using PoESkillTree.Computation.Parsing.Builders.Conditions;
using PoESkillTree.Computation.Parsing.Builders.Forms;
using PoESkillTree.Computation.Parsing.Builders.Stats;
using PoESkillTree.Computation.Parsing.Builders.Values;

namespace PoESkillTree.Computation.Data.Tests.Collections
{
    [TestFixture]
    public class FormAndStatMatcherCollectionTest
    {
        private const string Regex = "regex";

        private Mock<IValueBuilders> _valueFactory;
        private FormAndStatMatcherCollection _sut;

        [SetUp]
        public void SetUp()
        {
            _valueFactory = new Mock<IValueBuilders>();
            _sut = new FormAndStatMatcherCollection(new ModifierBuilderStub(), _valueFactory.Object);
        }

        [Test]
        public void IsEmpty()
        {
            Assert.IsEmpty(_sut);
        }

        [Test]
        public void AddFormStatAndValueAddsCorrectMatcherData()
        {
            var form = Mock.Of<IFormBuilder>();
            var stat = Mock.Of<IStatBuilder>();
            var value = Mock.Of<IValueBuilder>();
            _valueFactory.Setup(v => v.Create(3)).Returns(value);

            _sut.Add(Regex, form, stat, 3);

            var builder = _sut.AssertSingle(Regex);
            Assert.That(builder.Forms, Has.Exactly(1).SameAs(form));
            Assert.That(builder.Stats, Has.Exactly(1).SameAs(stat));
            Assert.That(builder.Values, Has.Exactly(1).SameAs(value));
        }

        [Test]
        public void AddFormStatValueAndConditionAddsCorrectMatcherData()
        {
            var form = Mock.Of<IFormBuilder>();
            var stat = Mock.Of<IStatBuilder>();
            var value = Mock.Of<IValueBuilder>();
            _valueFactory.Setup(v => v.Create(3)).Returns(value);
            var condition = Mock.Of<IConditionBuilder>();

            _sut.Add(Regex, form, stat, 3, condition);

            var builder = _sut.AssertSingle(Regex);
            Assert.That(builder.Forms, Has.Exactly(1).SameAs(form));
            Assert.That(builder.Stats, Has.Exactly(1).SameAs(stat));
            Assert.That(builder.Values, Has.Exactly(1).SameAs(value));
            Assert.That(builder.Conditions, Has.Exactly(1).SameAs(condition));
        }

        [Test]
        public void AddFormAndManyStatsAddsCorrectMatcherData()
        {
            var form = Mock.Of<IFormBuilder>();
            var stat1 = Mock.Of<IStatBuilder>();
            var stat2 = Mock.Of<IStatBuilder>();
            var stat3 = Mock.Of<IStatBuilder>();

            _sut.Add(Regex, form, stat1, stat2, stat3);

            var builder = _sut.AssertSingle(Regex);
            Assert.That(builder.Forms, Has.Exactly(1).SameAs(form));
            CollectionAssert.AreEqual(new[] { stat1, stat2, stat3 }, builder.Stats);
        }

        [Test]
        public void AddFormAndSingleStatAddsCorrectMatcherData()
        {
            var form = Mock.Of<IFormBuilder>();
            var stat = Mock.Of<IStatBuilder>();

            _sut.Add(Regex, form, stat);

            var builder = _sut.AssertSingle(Regex);
            Assert.That(builder.Forms, Has.Exactly(1).SameAs(form));
            Assert.That(builder.Stats, Has.Exactly(1).SameAs(stat));
        }

        [Test]
        public void AddFormAndEnumerableStatsAddsCorrectMatcherData()
        {
            var form = Mock.Of<IFormBuilder>();
            var stats = Enumerable.Empty<IStatBuilder>();

            _sut.Add(Regex, form, stats);

            var builder = _sut.AssertSingle(Regex);
            Assert.That(builder.Forms, Has.Exactly(1).SameAs(form));
            Assert.AreSame(stats, builder.Stats);
        }

        [Test]
        public void AddWithSubstitutionAddsCorrectMatcherData()
        {
            var form = Mock.Of<IFormBuilder>();
            var stat = Mock.Of<IStatBuilder>();

            _sut.Add(Regex, form, stat, "substitution");

            var data = _sut.Single();
            Assert.AreEqual(Regex, data.Regex);
            Assert.IsInstanceOf<ModifierBuilderStub>(data.ModifierBuilder);
            var builder = (ModifierBuilderStub) data.ModifierBuilder;
            Assert.That(builder.Forms, Has.Exactly(1).SameAs(form));
            Assert.That(builder.Stats, Has.Exactly(1).SameAs(stat));
            Assert.AreEqual("substitution", data.MatchSubstitution);
        }

        [Test]
        public void AddWithConverter()
        {
            var form = Mock.Of<IFormBuilder>();
            var stat = Mock.Of<IStatBuilder>();
            var converter = _valueFactory.SetupConverter();

            _sut.Add(Regex, form, stat, converter);

            var builder = _sut.AssertSingle(Regex);
            Assert.That(builder.Forms, Has.Exactly(1).SameAs(form));
            Assert.That(builder.Stats, Has.Exactly(1).SameAs(stat));
            Assert.AreSame(converter, builder.ValueConverter);
        }

        [Test]
        public void AddFormTuple()
        {
            var firstForm = Mock.Of<IFormBuilder>();
            var secondForm = Mock.Of<IFormBuilder>();
            var stat = Mock.Of<IStatBuilder>();

            _sut.Add(Regex, (firstForm, secondForm), stat);

            var builder = _sut.AssertSingle(Regex);
            CollectionAssert.AreEqual(new[] {firstForm, secondForm}, builder.Forms);
            Assert.That(builder.Stats, Has.Exactly(1).SameAs(stat));
        }

        [Test]
        public void AddManyAddsToCount()
        {
            var form = Mock.Of<IFormBuilder>();
            var stat = Mock.Of<IStatBuilder>();
            var value = Mock.Of<IValueBuilder>();
            _valueFactory.Setup(v => v.Create(5)).Returns(value);
            var converter = _valueFactory.SetupConverter();

            _sut.Add(Regex, form, stat, 5);
            _sut.Add(Regex, form, stat, stat);
            _sut.Add(Regex, form, new[] {stat, stat});
            _sut.Add(Regex, form, stat, "substitution");
            _sut.Add(Regex, form, stat, converter);
            _sut.Add(Regex, (form, form), stat);

            Assert.AreEqual(6, _sut.Count());
        }
    }
}