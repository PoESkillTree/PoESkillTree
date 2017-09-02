using System;
using System.Collections.Generic;
using System.Linq;
using Moq;
using NUnit.Framework;
using PoESkillTree.Computation.Parsing.Builders.Conditions;
using PoESkillTree.Computation.Parsing.Builders.Forms;
using PoESkillTree.Computation.Parsing.Builders.Stats;
using PoESkillTree.Computation.Parsing.Builders.Values;
using PoESkillTree.Computation.Parsing.ModifierBuilding;

namespace PoESkillTree.Computation.Parsing.Tests.ModifierBuilding
{
    [TestFixture]
    public class ModifierBuilderTest
    {
        [Test]
        public void IsIModifierBuilder()
        {
            var sut = new ModifierBuilder();

            Assert.IsInstanceOf<IModifierBuilder>(sut);
        }

        [Test]
        public void EntriesIsEmpty()
        {
            var sut = new ModifierBuilder();

            CollectionAssert.IsEmpty(sut.Entries);
        }

        [Test]
        public void WithFormReturnsModifierBuilder()
        {
            var sut = new ModifierBuilder();
            var form = Mock.Of<IFormBuilder>();

            var actual = sut.WithForm(form);

            Assert.IsInstanceOf<ModifierBuilder>(actual);
        }

        [Test]
        public void WithFormsReturnsModifierBuilder()
        {
            var sut = new ModifierBuilder();

            var actual = sut.WithForms(Enumerable.Empty<IFormBuilder>());

            Assert.IsInstanceOf<ModifierBuilder>(actual);
        }

        [Test]
        public void WithStatReturnsModifierBuilder()
        {
            var sut = new ModifierBuilder();
            var stat = Mock.Of<IStatBuilder>();

            var actual = sut.WithStat(stat);

            Assert.IsInstanceOf<ModifierBuilder>(actual);
        }

        [Test]
        public void WithStatsReturnsModifierBuilder()
        {
            var sut = new ModifierBuilder();

            var actual = sut.WithStats(Enumerable.Empty<IStatBuilder>());

            Assert.IsInstanceOf<ModifierBuilder>(actual);
        }

        [Test]
        public void WithStatConverterReturnsModifierBuilder()
        {
            var sut = new ModifierBuilder();

            var actual = sut.WithStatConverter(s => s);

            Assert.IsInstanceOf<ModifierBuilder>(actual);
        }

        [Test]
        public void WithValueReturnsModifierBuilder()
        {
            var sut = new ModifierBuilder();
            var value = Mock.Of<IValueBuilder>();

            var actual = sut.WithValue(value);

            Assert.IsInstanceOf<ModifierBuilder>(actual);
        }

        [Test]
        public void WithValuesReturnsModifierBuilder()
        {
            var sut = new ModifierBuilder();

            var actual = sut.WithValues(Enumerable.Empty<IValueBuilder>());

            Assert.IsInstanceOf<ModifierBuilder>(actual);
        }

        [Test]
        public void WithValueConverterReturnsModifierBuilder()
        {
            var sut = new ModifierBuilder();

            var actual = sut.WithValueConverter(v => v);

            Assert.IsInstanceOf<ModifierBuilder>(actual);
        }

        [Test]
        public void WithConditionReturnsModifierBuilder()
        {
            var sut = new ModifierBuilder();
            var condition = Mock.Of<IConditionBuilder>();

            var actual = sut.WithCondition(condition);

            Assert.IsInstanceOf<ModifierBuilder>(actual);
        }

        [Test]
        public void WithConditionsReturnsModifierBuilder()
        {
            var sut = new ModifierBuilder();

            var actual = sut.WithConditions(Enumerable.Empty<IConditionBuilder>());

            Assert.IsInstanceOf<ModifierBuilder>(actual);
        }

        [Test]
        public void WithFormAddsCorrectEntryWhenEmpty()
        {
            var sut = new ModifierBuilder();
            var form = Mock.Of<IFormBuilder>();

            sut = (ModifierBuilder) sut.WithForm(form);

            Assert.That(sut.Entries,
                Has.Exactly(1).EqualTo(Entry.WithForm(form)));
        }

        [Test]
        public void WithFormCalledTwiceThrows()
        {
            var sut = new ModifierBuilder();
            var form = Mock.Of<IFormBuilder>();
            sut = (ModifierBuilder) sut.WithForm(form);

            Assert.Throws<InvalidOperationException>(() => sut.WithForm(form));
        }

        [Test]
        public void WithFormModifiesExistingEntriesCorrectly()
        {
            var sut = new ModifierBuilder();
            var stats = Many<IStatBuilder>();
            sut = (ModifierBuilder) sut.WithStats(stats);
            var form = Mock.Of<IFormBuilder>();
            var expected =
                stats.Select(s => Entry.WithStat(s).WithForm(form)).ToList();

            sut = (ModifierBuilder) sut.WithForm(form);

            CollectionAssert.AreEqual(expected, sut.Entries);
        }

        [Test]
        public void WithFormsAddsCorrectEntriesWhenEmpty()
        {
            var sut = new ModifierBuilder();
            var forms = Many<IFormBuilder>();
            var expected = forms.Select(f => Entry.WithForm(f)).ToList();

            sut = (ModifierBuilder) sut.WithForms(forms);

            CollectionAssert.AreEqual(expected, sut.Entries);
        }

        [Test]
        public void WithFormsCalledTwiceThrows()
        {
            var sut = new ModifierBuilder();
            var forms = Many<IFormBuilder>();
            sut = (ModifierBuilder) sut.WithForms(forms);

            Assert.Throws<InvalidOperationException>(() => sut.WithForms(forms));
        }

        [Test]
        public void WithFormsModifiesExistingSingleEntryCorrectly()
        {
            var sut = new ModifierBuilder();
            var stat = Mock.Of<IStatBuilder>();
            sut = (ModifierBuilder) sut.WithStat(stat);
            var forms = Many<IFormBuilder>();
            var expected = forms.Select(f => Entry.WithStat(stat).WithForm(f)).ToList();

            sut = (ModifierBuilder) sut.WithForms(forms);

            CollectionAssert.AreEqual(expected, sut.Entries);
        }

        [Test]
        public void WithFormsModifiesExistingMultipleEntriesCorrectly()
        {
            var sut = new ModifierBuilder();
            var stats = Many<IStatBuilder>();
            sut = (ModifierBuilder) sut.WithStats(stats);
            var forms = Many<IFormBuilder>();
            var expected = forms.Zip(stats, (f, s) => Entry.WithForm(f).WithStat(s)).ToList();

            sut = (ModifierBuilder) sut.WithForms(forms);

            CollectionAssert.AreEqual(expected, sut.Entries);
        }

        [TestCase(2)]
        [TestCase(4)]
        public void WithFormsThrowsIfDifferentAmountOfExistingEntries(int existingCount)
        {
            var sut = new ModifierBuilder();
            var stats = Many<IStatBuilder>(existingCount);
            sut = (ModifierBuilder) sut.WithStats(stats);
            var forms = Many<IFormBuilder>();

            Assert.Throws<ArgumentException>(() => sut.WithForms(forms));
        }

        [Test]
        public void WithFormsStatsValuesAndConditionsCreatesCorrectEntries()
        {
            var sut = new ModifierBuilder();
            var forms = Many<IFormBuilder>();
            var stats = Many<IStatBuilder>();
            var values = Many<IValueBuilder>();
            var conditions = Many<IConditionBuilder>();
            var expected = forms.Select(f => Entry.WithForm(f))
                .Zip(stats, (e, s) => e.WithStat(s))
                .Zip(values, (e, v) => e.WithValue(v))
                .Zip(conditions, (e, c) => e.WithCondition(c))
                .ToList();

            sut = (ModifierBuilder) sut
                .WithForms(forms)
                .WithStats(stats)
                .WithValues(values)
                .WithConditions(conditions);

            CollectionAssert.AreEqual(expected, sut.Entries);
        }

        [Test]
        public void WithStatFormsValueAndConditionCreatesCorrectEntries()
        {
            var sut = new ModifierBuilder();
            var forms = Many<IFormBuilder>();
            var stat = Mock.Of<IStatBuilder>();
            var value = Mock.Of<IValueBuilder>();
            var condition = Mock.Of<IConditionBuilder>();
            var expexted = forms
                .Select(f =>
                    Entry.WithForm(f).WithStat(stat).WithValue(value).WithCondition(condition))
                .ToList();

            sut = (ModifierBuilder) sut
                .WithStat(stat)
                .WithForms(forms)
                .WithValue(value)
                .WithCondition(condition);

            CollectionAssert.AreEqual(expexted, sut.Entries);
        }

        [Test]
        public void WithStatConverterSetsStatConverter()
        {
            var sut = new ModifierBuilder();
            Func<IStatBuilder, IStatBuilder> statConverter = s => null;

            sut = (ModifierBuilder) sut.WithStatConverter(statConverter);

            Assert.AreSame(statConverter, sut.StatConverter);
        }

        [Test]
        public void WithValueConverterSetsValueConverter()
        {
            var sut = new ModifierBuilder();
            Func<IValueBuilder, IValueBuilder> valueConverter = v => null;

            sut = (ModifierBuilder) sut.WithValueConverter(valueConverter);

            Assert.AreSame(valueConverter, sut.ValueConverter);
        }

        [Test]
        public void InitialStatConverterIsIdentity()
        {
            var sut = new ModifierBuilder();
            var stat = Mock.Of<IStatBuilder>();

            var actual = sut.StatConverter(stat);

            Assert.AreEqual(stat, actual);
        }

        [Test]
        public void InitialValueConverterIsIdentity()
        {
            var sut = new ModifierBuilder();
            var value = Mock.Of<IValueBuilder>();

            var actual = sut.ValueConverter(value);

            Assert.AreEqual(value, actual);
        }

        [Test]
        public void IsIModifierBuilderResult()
        {
            var sut = new ModifierBuilder();

            Assert.IsInstanceOf<IModifierResult>(sut);
        }

        [Test]
        public void CreateReturnsSelf()
        {
            var sut = new ModifierBuilder();

            var actual = sut.Build();

            Assert.AreSame(sut, actual);
        }

        private static ModifierBuilderEntry Entry => new ModifierBuilderEntry();

        private static IReadOnlyList<T> Many<T>(int count = 3) where T : class =>
            Enumerable.Range(0, count).Select(_ => Mock.Of<T>()).ToList();
    }
}