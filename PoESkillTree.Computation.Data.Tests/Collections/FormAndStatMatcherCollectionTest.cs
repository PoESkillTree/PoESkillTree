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
        public void AddFormStatAndDoubleValueAddsCorrectMatcherData()
        {
            var form = Mock.Of<IFormBuilder>();
            var stat = Mock.Of<IStatBuilder>();
            var value = Mock.Of<IValueBuilder>();
            _valueFactory.Setup(v => v.Create(3)).Returns(value);

            _sut.Add(Regex, form, 3, stat);

            var builder = _sut.AssertSingle(Regex);
            Assert.That(builder.Forms, Has.Exactly(1).SameAs(form));
            Assert.That(builder.Stats, Has.Exactly(1).SameAs(stat));
            Assert.That(builder.Values, Has.Exactly(1).SameAs(value));
        }

        [Test]
        public void AddFormStatDoubleValueAndConditionAddsCorrectMatcherData()
        {
            var form = Mock.Of<IFormBuilder>();
            var stat = Mock.Of<IStatBuilder>();
            var value = Mock.Of<IValueBuilder>();
            _valueFactory.Setup(v => v.Create(3)).Returns(value);
            var condition = Mock.Of<IConditionBuilder>();

            _sut.Add(Regex, form, 3, stat, condition);

            var builder = _sut.AssertSingle(Regex);
            Assert.That(builder.Forms, Has.Exactly(1).SameAs(form));
            Assert.That(builder.Stats, Has.Exactly(1).SameAs(stat));
            Assert.That(builder.Values, Has.Exactly(1).SameAs(value));
            Assert.That(builder.Conditions, Has.Exactly(1).SameAs(condition));
        }

        [Test]
        public void AddFormValueBuilderAndManyStatsAddsCorrectMatcherData()
        {
            var form = Mock.Of<IFormBuilder>();
            var value = new ValueBuilder(Mock.Of<IValueBuilder>());
            var stat1 = Mock.Of<IStatBuilder>();
            var stat2 = Mock.Of<IStatBuilder>();
            var stat3 = Mock.Of<IStatBuilder>();

            _sut.Add(Regex, form, value, stat1, stat2, stat3);

            var builder = _sut.AssertSingle(Regex);
            Assert.That(builder.Forms, Has.Exactly(1).SameAs(form));
            Assert.That(builder.Values, Has.Exactly(1).SameAs(value));
            CollectionAssert.AreEqual(new[] { stat1, stat2, stat3 }, builder.Stats);
        }

        [Test]
        public void AddFormValueBuilderAndSingleStatAddsCorrectMatcherData()
        {
            var form = Mock.Of<IFormBuilder>();
            var value = new ValueBuilder(Mock.Of<IValueBuilder>());
            var stat = Mock.Of<IStatBuilder>();

            _sut.Add(Regex, form, value, stat);

            var builder = _sut.AssertSingle(Regex);
            Assert.That(builder.Forms, Has.Exactly(1).SameAs(form));
            Assert.That(builder.Values, Has.Exactly(1).SameAs(value));
            Assert.That(builder.Stats, Has.Exactly(1).SameAs(stat));
        }

        [Test]
        public void AddFormDoubleValueAndEnumerableStatsAddsCorrectMatcherData()
        {
            var form = Mock.Of<IFormBuilder>();
            var value = Mock.Of<IValueBuilder>();
            _valueFactory.Setup(v => v.Create(3)).Returns(value);
            var stats = Enumerable.Empty<IStatBuilder>();

            _sut.Add(Regex, form, 3, stats);

            var builder = _sut.AssertSingle(Regex);
            Assert.That(builder.Forms, Has.Exactly(1).SameAs(form));
            Assert.That(builder.Values, Has.Exactly(1).SameAs(value));
            Assert.AreSame(stats, builder.Stats);
        }

        [Test]
        public void AddWithSubstitutionAddsCorrectMatcherData()
        {
            var form = Mock.Of<IFormBuilder>();
            var value = new ValueBuilder(Mock.Of<IValueBuilder>());
            var stat = Mock.Of<IStatBuilder>();

            _sut.Add(Regex, form, value, stat, "substitution");

            var data = _sut.Single();
            Assert.AreEqual(Regex, data.Regex);
            Assert.IsInstanceOf<ModifierBuilderStub>(data.ModifierResult);
            var builder = (ModifierBuilderStub) data.ModifierResult;
            Assert.That(builder.Forms, Has.Exactly(1).SameAs(form));
            Assert.That(builder.Values, Has.Exactly(1).SameAs(value));
            Assert.That(builder.Stats, Has.Exactly(1).SameAs(stat));
            Assert.AreEqual("substitution", data.MatchSubstitution);
        }

        [Test]
        public void AddWithConverter()
        {
            var form = Mock.Of<IFormBuilder>();
            var value = new ValueBuilder(Mock.Of<IValueBuilder>());
            var stat = Mock.Of<IStatBuilder>();
            var converter = _valueFactory.SetupConverter();

            _sut.Add(Regex, form, value, stat, converter);

            var builder = _sut.AssertSingle(Regex);
            Assert.That(builder.Forms, Has.Exactly(1).SameAs(form));
            Assert.That(builder.Values, Has.Exactly(1).SameAs(value));
            Assert.That(builder.Stats, Has.Exactly(1).SameAs(stat));
            Assert.AreSame(converter, builder.ValueConverter);
        }

        [Test]
        public void AddTuple()
        {
            var firstForm = Mock.Of<IFormBuilder>();
            var secondForm = Mock.Of<IFormBuilder>();
            var firstValue = new ValueBuilder(Mock.Of<IValueBuilder>());
            var secondValue = new ValueBuilder(Mock.Of<IValueBuilder>());
            var stat = Mock.Of<IStatBuilder>();

            _sut.Add(Regex, (firstForm, secondForm), (firstValue, secondValue), stat);

            var builder = _sut.AssertSingle(Regex);
            CollectionAssert.AreEqual(new[] { firstForm, secondForm }, builder.Forms);
            CollectionAssert.AreEqual(new[] { firstValue, secondValue }, builder.Values);
            Assert.That(builder.Stats, Has.Exactly(1).SameAs(stat));
        }

        [Test]
        public void AddManyAddsToCount()
        {
            var form = Mock.Of<IFormBuilder>();
            var stat = Mock.Of<IStatBuilder>();
            var value = Mock.Of<IValueBuilder>();
            var valueBuilder = new ValueBuilder(value);
            _valueFactory.Setup(v => v.Create(5)).Returns(value);
            var converter = _valueFactory.SetupConverter();

            _sut.Add(Regex, form, 5, stat);
            _sut.Add(Regex, form, valueBuilder, stat, stat);
            _sut.Add(Regex, form, 5, new[] {stat, stat});
            _sut.Add(Regex, form, valueBuilder, stat, "substitution");
            _sut.Add(Regex, form, valueBuilder, stat, converter);
            _sut.Add(Regex, (form, form), (valueBuilder, valueBuilder), stat);

            Assert.AreEqual(6, _sut.Count());
        }
    }
}