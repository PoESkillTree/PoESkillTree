using System.Linq;
using Moq;
using NUnit.Framework;
using PoESkillTree.Computation.Data.Collections;
using PoESkillTree.Computation.Providers.Conditions;
using PoESkillTree.Computation.Providers.Forms;
using PoESkillTree.Computation.Providers.Stats;
using PoESkillTree.Computation.Providers.Values;

namespace PoESkillTree.Computation.Data.Tests.Collections
{
    [TestFixture]
    public class FormAndStatMatcherCollectionTest
    {
        private const string Regex = "regex";

        private Mock<IValueProviderFactory> _valueFactory;
        private FormAndStatMatcherCollection _sut;

        [SetUp]
        public void SetUp()
        {
            _valueFactory = new Mock<IValueProviderFactory>();
            _sut = new FormAndStatMatcherCollection(new MatchBuilderStub(), _valueFactory.Object);
        }

        [Test]
        public void IsEmpty()
        {
            Assert.IsEmpty(_sut);
        }

        [Test]
        public void AddFormStatAndValueAddsCorrectMatcherData()
        {
            var form = Mock.Of<IFormProvider>();
            var stat = Mock.Of<IStatProvider>();
            var value = new ValueProvider(Mock.Of<IValueProvider>());
            _valueFactory.Setup(v => v.Create(3)).Returns(value);

            _sut.Add(Regex, form, stat, 3);

            var builder = AssertMatcherData();
            Assert.That(builder.Forms, Has.Exactly(1).SameAs(form));
            Assert.That(builder.Stats, Has.Exactly(1).SameAs(stat));
            Assert.That(builder.Values, Has.Exactly(1).SameAs(value));
        }

        [Test]
        public void AddFormStatValueAndConditionAddsCorrectMatcherData()
        {
            var form = Mock.Of<IFormProvider>();
            var stat = Mock.Of<IStatProvider>();
            var value = new ValueProvider(Mock.Of<IValueProvider>());
            _valueFactory.Setup(v => v.Create(3)).Returns(value);
            var condition = Mock.Of<IConditionProvider>();

            _sut.Add(Regex, form, stat, 3, condition);

            var builder = AssertMatcherData();
            Assert.That(builder.Forms, Has.Exactly(1).SameAs(form));
            Assert.That(builder.Stats, Has.Exactly(1).SameAs(stat));
            Assert.That(builder.Values, Has.Exactly(1).SameAs(value));
            Assert.AreSame(condition, builder.Condition);
        }

        [Test]
        public void AddFormAndManyStatsAddsCorrectMatcherData()
        {
            var form = Mock.Of<IFormProvider>();
            var stat1 = Mock.Of<IStatProvider>();
            var stat2 = Mock.Of<IStatProvider>();
            var stat3 = Mock.Of<IStatProvider>();

            _sut.Add(Regex, form, stat1, stat2, stat3);

            var builder = AssertMatcherData();
            Assert.That(builder.Forms, Has.Exactly(1).SameAs(form));
            CollectionAssert.AreEqual(new[] { stat1, stat2, stat3 }, builder.Stats);
        }

        [Test]
        public void AddFormAndSingleStatAddsCorrectMatcherData()
        {
            var form = Mock.Of<IFormProvider>();
            var stat = Mock.Of<IStatProvider>();

            _sut.Add(Regex, form, stat);

            var builder = AssertMatcherData();
            Assert.That(builder.Forms, Has.Exactly(1).SameAs(form));
            Assert.That(builder.Stats, Has.Exactly(1).SameAs(stat));
        }

        [Test]
        public void AddFormAndEnumerableStatsAddsCorrectMatcherData()
        {
            var form = Mock.Of<IFormProvider>();
            var stats = Enumerable.Empty<IStatProvider>();

            _sut.Add(Regex, form, stats);

            var builder = AssertMatcherData();
            Assert.That(builder.Forms, Has.Exactly(1).SameAs(form));
            Assert.AreSame(stats, builder.Stats);
        }

        [Test]
        public void AddWithSubstitutionAddsCorrectMatcherData()
        {
            var form = Mock.Of<IFormProvider>();
            var stat = Mock.Of<IStatProvider>();

            _sut.Add(Regex, form, stat, "substitution");

            var data = _sut.Single();
            Assert.AreEqual(Regex, data.Regex);
            Assert.IsInstanceOf<MatchBuilderStub>(data.MatchBuilder);
            var builder = (MatchBuilderStub) data.MatchBuilder;
            Assert.That(builder.Forms, Has.Exactly(1).SameAs(form));
            Assert.That(builder.Stats, Has.Exactly(1).SameAs(stat));
            Assert.AreEqual("substitution", data.MatchSubstitution);
        }

        [Test]
        public void AddWithConverter()
        {
            var form = Mock.Of<IFormProvider>();
            var stat = Mock.Of<IStatProvider>();
            ValueFunc converter = v => null;

            _sut.Add(Regex, form, stat, converter);

            var builder = AssertMatcherData();
            Assert.That(builder.Forms, Has.Exactly(1).SameAs(form));
            Assert.That(builder.Stats, Has.Exactly(1).SameAs(stat));
            Assert.AreSame(converter, builder.ValueConverter);
        }

        [Test]
        public void AddFormTuple()
        {
            var firstForm = Mock.Of<IFormProvider>();
            var secondForm = Mock.Of<IFormProvider>();
            var stat = Mock.Of<IStatProvider>();

            _sut.Add(Regex, (firstForm, secondForm), stat);

            var builder = AssertMatcherData();
            CollectionAssert.AreEqual(new[] {firstForm, secondForm}, builder.Forms);
            Assert.That(builder.Stats, Has.Exactly(1).SameAs(stat));
        }

        [Test]
        public void AddManyAddsToCount()
        {
            var form = Mock.Of<IFormProvider>();
            var stat = Mock.Of<IStatProvider>();
            var value = new ValueProvider(Mock.Of<IValueProvider>());
            _valueFactory.Setup(v => v.Create(5)).Returns(value);

            _sut.Add(Regex, form, stat, 5);
            _sut.Add(Regex, form, stat, stat);
            _sut.Add(Regex, form, new[] {stat, stat});
            _sut.Add(Regex, form, stat, "substitution");
            _sut.Add(Regex, form, stat, v => null);
            _sut.Add(Regex, (form, form), stat);

            Assert.AreEqual(6, _sut.Count());
        }

        private MatchBuilderStub AssertMatcherData()
        {
            var data = _sut.Single();
            Assert.AreEqual(Regex, data.Regex);
            Assert.IsInstanceOf<MatchBuilderStub>(data.MatchBuilder);
            Assert.AreEqual(string.Empty, data.MatchSubstitution);
            return (MatchBuilderStub) data.MatchBuilder;
        }
    }
}