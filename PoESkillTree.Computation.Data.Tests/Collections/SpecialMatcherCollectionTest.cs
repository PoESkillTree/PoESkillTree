﻿using Moq;
using NUnit.Framework;
using PoESkillTree.Computation.Data.Collections;
using PoESkillTree.Computation.Parsing.Builders.Conditions;
using PoESkillTree.Computation.Parsing.Builders.Forms;
using PoESkillTree.Computation.Parsing.Builders.Stats;
using PoESkillTree.Computation.Parsing.Builders.Values;

namespace PoESkillTree.Computation.Data.Tests.Collections
{
    [TestFixture]
    public class SpecialMatcherCollectionTest
    {
        private const string Regex = "regex";

        private Mock<IValueBuilders> _valueFactory;
        private SpecialMatcherCollection _sut;

        [SetUp]
        public void SetUp()
        {
            _valueFactory = new Mock<IValueBuilders>();
            _sut = new SpecialMatcherCollection(new ModifierBuilderStub(), _valueFactory.Object);
        }

        [Test]
        public void IsEmpty()
        {
            Assert.IsEmpty(_sut);
        }

        [Test]
        public void AddFormStat()
        {
            var form = Mock.Of<IFormBuilder>();
            var stat = Mock.Of<IStatBuilder>();

            _sut.Add(Regex, form, stat);

            var builder = _sut.AssertSingle(Regex);
            Assert.That(builder.Forms, Has.Exactly(1).SameAs(form));
            Assert.That(builder.Stats, Has.Exactly(1).SameAs(stat));
        }

        [Test]
        public void AddFormStatCondition()
        {
            var form = Mock.Of<IFormBuilder>();
            var stat = Mock.Of<IStatBuilder>();
            var condition = Mock.Of<IConditionBuilder>();

            _sut.Add(Regex, form, stat, condition);

            var builder = _sut.AssertSingle(Regex);
            Assert.That(builder.Forms, Has.Exactly(1).SameAs(form));
            Assert.That(builder.Stats, Has.Exactly(1).SameAs(stat));
            Assert.That(builder.Conditions, Has.Exactly(1).SameAs(condition));
        }

        [Test]
        public void AddFormStatValue()
        {
            var form = Mock.Of<IFormBuilder>();
            var stat = Mock.Of<IStatBuilder>();
            var value = new ValueBuilder(Mock.Of<IValueBuilder>(), null);

            _sut.Add(Regex, form, stat, value);

            var builder = _sut.AssertSingle(Regex);
            Assert.That(builder.Forms, Has.Exactly(1).SameAs(form));
            Assert.That(builder.Stats, Has.Exactly(1).SameAs(stat));
            Assert.That(builder.Values, Has.Exactly(1).SameAs(value));
        }

        [Test]
        public void AddFormStatValueCondition()
        {
            var form = Mock.Of<IFormBuilder>();
            var stat = Mock.Of<IStatBuilder>();
            var value = new ValueBuilder(Mock.Of<IValueBuilder>(), null);
            var condition = Mock.Of<IConditionBuilder>();

            _sut.Add(Regex, form, stat, value, condition);

            var builder = _sut.AssertSingle(Regex);
            Assert.That(builder.Forms, Has.Exactly(1).SameAs(form));
            Assert.That(builder.Stats, Has.Exactly(1).SameAs(stat));
            Assert.That(builder.Values, Has.Exactly(1).SameAs(value));
            Assert.That(builder.Conditions, Has.Exactly(1).SameAs(condition));
        }

        [Test]
        public void AddFormStatDoubleValueCondition()
        {
            var form = Mock.Of<IFormBuilder>();
            var stat = Mock.Of<IStatBuilder>();
            var value = new ValueBuilder(Mock.Of<IValueBuilder>(), null);
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
        public void AddTuplesWithValueProviders()
        {
            var forms = new[]
                { Mock.Of<IFormBuilder>(), Mock.Of<IFormBuilder>(), Mock.Of<IFormBuilder>() };
            var stats = new[]
                { Mock.Of<IStatBuilder>(), Mock.Of<IStatBuilder>(), Mock.Of<IStatBuilder>() };
            var values = new[]
            {
                new ValueBuilder(Mock.Of<IValueBuilder>(), null),
                new ValueBuilder(Mock.Of<IValueBuilder>(), null),
                new ValueBuilder(Mock.Of<IValueBuilder>(), null)
            };
            var conditions = new[]
            {
                Mock.Of<IConditionBuilder>(), Mock.Of<IConditionBuilder>(),
                Mock.Of<IConditionBuilder>()
            };

            _sut.Add(Regex, 
                (forms[0], stats[0], values[0], conditions[0]),
                (forms[1], stats[1], values[1], conditions[1]),
                (forms[2], stats[2], values[2], conditions[2]));

            var builder = _sut.AssertSingle(Regex);
            CollectionAssert.AreEqual(forms, builder.Forms);
            CollectionAssert.AreEqual(stats, builder.Stats);
            CollectionAssert.AreEqual(values, builder.Values);
            CollectionAssert.AreEqual(conditions, builder.Conditions);
        }

        [Test]
        public void AddTuplesWithDoubleValues()
        {
            var forms = new[]
                { Mock.Of<IFormBuilder>(), Mock.Of<IFormBuilder>(), Mock.Of<IFormBuilder>() };
            var stats = new[]
                { Mock.Of<IStatBuilder>(), Mock.Of<IStatBuilder>(), Mock.Of<IStatBuilder>() };
            var values = new[]
            {
                new ValueBuilder(Mock.Of<IValueBuilder>(), null),
                new ValueBuilder(Mock.Of<IValueBuilder>(), null),
                new ValueBuilder(Mock.Of<IValueBuilder>(), null)
            };
            _valueFactory.Setup(v => v.Create(0)).Returns(values[0]);
            _valueFactory.Setup(v => v.Create(1)).Returns(values[1]);
            _valueFactory.Setup(v => v.Create(2)).Returns(values[2]);
            var conditions = new[]
            {
                Mock.Of<IConditionBuilder>(), Mock.Of<IConditionBuilder>(),
                Mock.Of<IConditionBuilder>()
            };

            _sut.Add(Regex,
                (forms[0], stats[0], 0, conditions[0]),
                (forms[1], stats[1], 1, conditions[1]),
                (forms[2], stats[2], 2, conditions[2]));

            var builder = _sut.AssertSingle(Regex);
            CollectionAssert.AreEqual(forms, builder.Forms);
            CollectionAssert.AreEqual(stats, builder.Stats);
            CollectionAssert.AreEqual(values, builder.Values);
            CollectionAssert.AreEqual(conditions, builder.Conditions);
        }
    }
}