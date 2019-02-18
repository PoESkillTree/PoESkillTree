using System.Linq;
using Moq;
using NUnit.Framework;
using PoESkillTree.Computation.Common.Builders.Conditions;
using PoESkillTree.Computation.Common.Builders.Forms;
using PoESkillTree.Computation.Common.Builders.Stats;
using PoESkillTree.Computation.Common.Builders.Values;
using PoESkillTree.Computation.Data.Collections;

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
            Assert.AreEqual(stats, builder.Stats);
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
            Assert.IsInstanceOf<ModifierBuilderStub>(data.Modifier);
            var builder = (ModifierBuilderStub) data.Modifier;
            Assert.That(builder.Forms, Has.Exactly(1).SameAs(form));
            Assert.That(builder.Values, Has.Exactly(1).SameAs(value));
            Assert.That(builder.Stats, Has.Exactly(1).SameAs(stat));
            Assert.AreEqual("substitution", data.MatchSubstitution);
        }

        [Test]
        public void AddManyAddsToCount()
        {
            var form = Mock.Of<IFormBuilder>();
            var stat = Mock.Of<IStatBuilder>();
            var value = Mock.Of<IValueBuilder>();
            var valueBuilder = new ValueBuilder(value);
            _valueFactory.Setup(v => v.Create(5)).Returns(value);

            _sut.Add(Regex, form, 5, stat);
            _sut.Add(Regex, form, valueBuilder, stat, stat);
            _sut.Add(Regex, form, 5, new[] {stat, stat});
            _sut.Add(Regex, form, valueBuilder, stat, "substitution");

            Assert.AreEqual(4, _sut.Count());
        }
    }
}