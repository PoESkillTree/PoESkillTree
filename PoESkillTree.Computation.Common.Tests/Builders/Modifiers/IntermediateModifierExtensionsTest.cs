using System;
using System.Collections.Generic;
using System.Linq;
using Moq;
using NUnit.Framework;
using PoESkillTree.Computation.Common.Builders;
using PoESkillTree.Computation.Common.Builders.Conditions;
using PoESkillTree.Computation.Common.Builders.Forms;
using PoESkillTree.Computation.Common.Builders.Modifiers;
using PoESkillTree.Computation.Common.Builders.Stats;
using PoESkillTree.Computation.Common.Builders.Values;

namespace PoESkillTree.Computation.Common.Tests.Builders.Modifiers
{
    [TestFixture]
    public class IntermediateModifierExtensionsTest
    {
        [Test]
        public void MergeChainsStatConverters()
        {
            var leftInput = Mock.Of<IStatBuilder>();
            var leftOutput = Mock.Of<IStatBuilder>();
            var rightOutput = Mock.Of<IStatBuilder>();
            IStatBuilder LeftConverter(IStatBuilder s) => s == leftInput ? leftOutput : s;
            IStatBuilder RightConverter(IStatBuilder s) => s == leftOutput ? rightOutput : s;
            var left = CreateResult(LeftConverter);
            var right = CreateResult(RightConverter);

            var result = left.MergeWith(right);
            var output = result.StatConverter(leftInput);

            Assert.AreEqual(rightOutput, output);
        }

        [Test]
        public void MergeChainsValueConverters()
        {
            var leftInput = Mock.Of<IValueBuilder>();
            var leftOutput = Mock.Of<IValueBuilder>();
            var rightOutput = Mock.Of<IValueBuilder>();
            IValueBuilder LeftConverter(IValueBuilder s) => s == leftInput ? leftOutput : s;
            IValueBuilder RightConverter(IValueBuilder s) => s == leftOutput ? rightOutput : s;
            var left = CreateResult(LeftConverter);
            var right = CreateResult(RightConverter);

            var result = left.MergeWith(right);
            var output = result.ValueConverter(leftInput);

            Assert.AreEqual(rightOutput, output);
        }

        [Test]
        public void MergeWithLeftEmptyReturnsRightEntries()
        {
            var entries = CreateManyEntries();
            var left = SimpleIntermediateModifier.Empty;
            var right = CreateResult(entries);

            var result = left.MergeWith(right);

            CollectionAssert.AreEqual(entries, result.Entries);
        }

        [Test]
        public void MergeWithRightEmptyReturnsLeftEntries()
        {
            var entries = CreateManyEntries();
            var left = CreateResult(entries);
            var right = SimpleIntermediateModifier.Empty;

            var result = left.MergeWith(right);

            CollectionAssert.AreEqual(entries, result.Entries);
        }

        [Test]
        public void MergeWithBothMultipleEntriesThrows()
        {
            var left = CreateResult(CreateManyEntries());
            var right = CreateResult(CreateManyEntries());

            Assert.Throws<ArgumentException>(() => left.MergeWith(right));
        }

        [Test]
        public void MergeWithBothSingleEntryReturnsSingleEntry()
        {
            var entryLeft = EmptyEntry;
            var entryRight = DefaultEntry;
            var left = CreateResult(entryLeft);
            var right = CreateResult(entryRight);

            var result = left.MergeWith(right);

            Assert.That(result.Entries, Has.Exactly(1).Items);
        }

        [Test]
        public void MergeWithBothSingleEntryAndNullPropertiesInLeftReturnsCorrectEntry()
        {
            var entryLeft = EmptyEntry;
            var entryRight = DefaultEntry;
            var left = CreateResult(entryLeft);
            var right = CreateResult(entryRight);

            var result = left.MergeWith(right);

            Assert.AreEqual(entryRight, result.Entries.Single());
        }

        [Test]
        public void MergeWithBothSingleEntryAndNullPropertiesInRightReturnsCorrectEntry()
        {
            var entryLeft = DefaultEntry;
            var entryRight = EmptyEntry;
            var left = CreateResult(entryLeft);
            var right = CreateResult(entryRight);

            var result = left.MergeWith(right);

            Assert.AreEqual(entryLeft, result.Entries.Single());
        }

        [Test]
        public void MergeWithBothSingleEntryAndFormInBothThrows()
        {
            var entryLeft = DefaultEntry;
            var entryRight = EmptyEntry
                .WithForm(Mock.Of<IFormBuilder>());
            var left = CreateResult(entryLeft);
            var right = CreateResult(entryRight);

            Assert.Throws<ArgumentException>(() => left.MergeWith(right));
        }

        [Test]
        public void MergeWithBothSingleEntryAndStatInBothThrows()
        {
            var entryLeft = DefaultEntry;
            var entryRight = EmptyEntry
                .WithStat(Mock.Of<IStatBuilder>());
            var left = CreateResult(entryLeft);
            var right = CreateResult(entryRight);

            Assert.Throws<ArgumentException>(() => left.MergeWith(right));
        }

        [Test]
        public void MergeWithBothSingleEntryAndValueInBothThrows()
        {
            var entryLeft = DefaultEntry;
            var entryRight = EmptyEntry
                .WithValue(Mock.Of<IValueBuilder>());
            var left = CreateResult(entryLeft);
            var right = CreateResult(entryRight);

            Assert.Throws<ArgumentException>(() => left.MergeWith(right));
        }

        [Test]
        public void MergeWithBothSingleEntryAndConditionInBothAndsConditions()
        {
            var expected = Mock.Of<IConditionBuilder>();
            var rightCondition = Mock.Of<IConditionBuilder>();
            var leftCondition = Mock.Of<IConditionBuilder>(c => c.And(rightCondition) == expected);
            var entryLeft = EmptyEntry
                .WithCondition(leftCondition);
            var entryRight = DefaultEntry
                .WithCondition(rightCondition);
            var left = CreateResult(entryLeft);
            var right = CreateResult(entryRight);

            var result = left.MergeWith(right);

            Assert.AreEqual(expected, result.Entries.Single().Condition);
        }

        [Test]
        public void MergeWithLeftMultipleEntriesReturnsCorrectNumberOfEntries()
        {
            var left = CreateResult(CreateManyEntries());
            var right = CreateResult(EmptyEntry);

            var result = left.MergeWith(right);

            Assert.That(result.Entries, Has.Exactly(left.Entries.Count).Items);
        }

        [Test]
        public void MergeWithRightMultipleEntriesReturnsCorrectNumberOfEntries()
        {
            var left = CreateResult(EmptyEntry);
            var right = CreateResult(CreateManyEntries());

            var result = left.MergeWith(right);

            Assert.That(result.Entries, Has.Exactly(right.Entries.Count).Items);
        }

        [Test]
        public void MergeWithLeftMultipleEntriesReturnsCorrectEntries()
        {
            var stat = Mock.Of<IStatBuilder>();
            var form1 = Mock.Of<IFormBuilder>();
            var leftAndRightCondition = Mock.Of<IConditionBuilder>();
            var leftCondition = Mock.Of<IConditionBuilder>();
            var rightCondition = Mock.Of<IConditionBuilder>(
                c => c.And(leftCondition) == leftAndRightCondition);
            var entry0 = EmptyEntry
                .WithCondition(leftCondition);
            var entry1 = EmptyEntry
                .WithForm(form1);
            var entry2 = EmptyEntry;
            var rightEntry = EmptyEntry
                .WithStat(stat)
                .WithCondition(rightCondition);
            var left = CreateResult(entry0, entry1, entry2);
            var right = CreateResult(rightEntry);
            var expected = new[]
            {
                entry0.WithStat(stat).WithCondition(leftAndRightCondition),
                entry1.WithStat(stat).WithCondition(rightCondition),
                entry2.WithStat(stat).WithCondition(rightCondition)
            };

            var result = left.MergeWith(right);

            CollectionAssert.AreEqual(expected, result.Entries);
        }

        [Test]
        public void BuildSkipsEntriesWithNullStats()
        {
            var input = CreateResult(DefaultEntry.WithStat(null));

            var result = input.Build();

            CollectionAssert.IsEmpty(result);
        }

        [Test]
        public void BuildSkipsEntriesWithNullForms()
        {
            var input = CreateResult(DefaultEntry.WithForm(null));

            var result = input.Build();

            CollectionAssert.IsEmpty(result);
        }

        [Test]
        public void BuildSkipsEntriesWithNullValues()
        {
            var input = CreateResult(DefaultEntry.WithValue(null));

            var result = input.Build();

            CollectionAssert.IsEmpty(result);
        }

        [Test]
        public void BuildReturnsCorrectModifiers()
        {
            var conditionBuilder = Mock.Of<IConditionBuilder>();

            var value = Mock.Of<IValue>();
            var valueBuilder = Mock.Of<IValueBuilder>();
            var convertedValueBuilder = Mock.Of<IValueBuilder>();
            var statConvertedValueBuilder = Mock.Of<IValueBuilder>();
            var formConvertedValueBuilder = Mock.Of<IValueBuilder>(b => b.Build() == value);

            var stats = new[] { Mock.Of<IStat>() };
            var source = new GlobalModifierSource();
            IValueBuilder StatConvertValue(IValueBuilder v) =>
                v == convertedValueBuilder ? statConvertedValueBuilder : v;
            var statBuilderMock = new Mock<IStatBuilder>();
            statBuilderMock.Setup(b => b.Build()).Returns((stats, source, StatConvertValue));
            var statBuilderWithCondition = statBuilderMock.Object;
            var statBuilder = Mock.Of<IStatBuilder>();
            var convertedStatBuilder =
                Mock.Of<IStatBuilder>(s => s.WithCondition(conditionBuilder) == statBuilderWithCondition);

            var form = Form.More;
            IValueBuilder FormConvertValue(IValueBuilder v) =>
                v == statConvertedValueBuilder ? formConvertedValueBuilder : v;
            var formBuilderMock = new Mock<IFormBuilder>();
            formBuilderMock.Setup(b => b.Build()).Returns((form, FormConvertValue));
            var formBuilder = formBuilderMock.Object;

            var entry = EmptyEntry
                .WithStat(statBuilder)
                .WithForm(formBuilder)
                .WithValue(valueBuilder)
                .WithCondition(conditionBuilder);

            var input = CreateResult(
                new[] { entry },
                s => s == statBuilder ? convertedStatBuilder : s,
                v => v == valueBuilder ? convertedValueBuilder : v);

            var result = input.Build();

            Assert.AreEqual(1, result.Count);
            var item = result[0];
            Assert.AreEqual(stats, item.Stats);
            Assert.AreEqual(form, item.Form);
            Assert.AreEqual(value, item.Value);
            Assert.AreSame(source, item.Source);
        }

        private static readonly IntermediateModifierEntry EmptyEntry = new IntermediateModifierEntry();

        private static readonly IntermediateModifierEntry DefaultEntry = EmptyEntry
            .WithStat(Mock.Of<IStatBuilder>())
            .WithForm(Mock.Of<IFormBuilder>())
            .WithValue(Mock.Of<IValueBuilder>())
            .WithCondition(Mock.Of<IConditionBuilder>());

        private static IntermediateModifierEntry[] CreateManyEntries()
        {
            var entry0 = DefaultEntry;
            var entry1 = DefaultEntry;
            var entry2 = DefaultEntry;
            return new[] { entry0, entry1, entry2 };
        }

        private static IIntermediateModifier CreateResult(StatConverter statConverter)
        {
            return CreateResult(null, statConverter);
        }

        private static IIntermediateModifier CreateResult(ValueConverter valueConverter)
        {
            return CreateResult(null, valueConverter: valueConverter);
        }

        private static IIntermediateModifier CreateResult(params IntermediateModifierEntry[] entries)
        {
            return CreateResult(entries, null);
        }

        private static IIntermediateModifier CreateResult(IReadOnlyList<IntermediateModifierEntry> entries = null,
            StatConverter statConverter = null,
            ValueConverter valueConverter = null)
        {
            return new SimpleIntermediateModifier(entries ?? new IntermediateModifierEntry[0],
                statConverter ?? (s => s),
                valueConverter ?? (v => v));
        }
    }
}