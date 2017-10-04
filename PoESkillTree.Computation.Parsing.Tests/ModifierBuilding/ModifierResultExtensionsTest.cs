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
    public class ModifierResultExtensionsTest
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

            var result = ModifierResultExtensions.Merge(left, right);
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

            var result = ModifierResultExtensions.Merge(left, right);
            var output = result.ValueConverter(leftInput);

            Assert.AreEqual(rightOutput, output);
        }

        [Test]
        public void MergeWithLeftEmptyReturnsRightEntries()
        {
            var entries = CreateManyEntries();
            var left = SimpleModifierResult.Empty;
            var right = CreateResult(entries);

            var result = ModifierResultExtensions.Merge(left, right);

            CollectionAssert.AreEqual(entries, result.Entries);
        }

        [Test]
        public void MergeWithRightEmptyReturnsLeftEntries()
        {
            var entries = CreateManyEntries();
            var left = CreateResult(entries);
            var right = SimpleModifierResult.Empty;

            var result = ModifierResultExtensions.Merge(left, right);

            CollectionAssert.AreEqual(entries, result.Entries);
        }

        [Test]
        public void MergeWithBothMultipleEntriesThrows()
        {
            var left = CreateResult(CreateManyEntries());
            var right = CreateResult(CreateManyEntries());

            Assert.Throws<ArgumentException>(() => ModifierResultExtensions.Merge(left, right));
        }

        [Test]
        public void MergeWithBothSingleEntryReturnsSingleEntry()
        {
            var entryLeft = EmptyEntry;
            var entryRight = DefaultEntry;
            var left = CreateResult(entryLeft);
            var right = CreateResult(entryRight);

            var result = ModifierResultExtensions.Merge(left, right);

            Assert.That(result.Entries, Has.Exactly(1).Items);
        }

        [Test]
        public void MergeWithBothSingleEntryAndNullPropertiesInLeftReturnsCorrectEntry()
        {
            var entryLeft = EmptyEntry;
            var entryRight = DefaultEntry;
            var left = CreateResult(entryLeft);
            var right = CreateResult(entryRight);

            var result = ModifierResultExtensions.Merge(left, right);

            Assert.AreEqual(entryRight, result.Entries.Single());
        }

        [Test]
        public void MergeWithBothSingleEntryAndNullPropertiesInRightReturnsCorrectEntry()
        {
            var entryLeft = DefaultEntry;
            var entryRight = EmptyEntry;
            var left = CreateResult(entryLeft);
            var right = CreateResult(entryRight);

            var result = ModifierResultExtensions.Merge(left, right);

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

            Assert.Throws<ArgumentException>(() => ModifierResultExtensions.Merge(left, right));
        }

        [Test]
        public void MergeWithBothSingleEntryAndStatInBothThrows()
        {
            var entryLeft = DefaultEntry;
            var entryRight = EmptyEntry
                .WithStat(Mock.Of<IStatBuilder>());
            var left = CreateResult(entryLeft);
            var right = CreateResult(entryRight);

            Assert.Throws<ArgumentException>(() => ModifierResultExtensions.Merge(left, right));
        }

        [Test]
        public void MergeWithBothSingleEntryAndValueInBothThrows()
        {
            var entryLeft = DefaultEntry;
            var entryRight = EmptyEntry
                .WithValue(Mock.Of<IValueBuilder>());
            var left = CreateResult(entryLeft);
            var right = CreateResult(entryRight);

            Assert.Throws<ArgumentException>(() => ModifierResultExtensions.Merge(left, right));
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

            var result = ModifierResultExtensions.Merge(left, right);

            Assert.AreEqual(expected, result.Entries.Single().Condition);
        }

        [Test]
        public void MergeWithLeftMultipleEntriesReturnsCorrectNumberOfEntries()
        {
            var left = CreateResult(CreateManyEntries());
            var right = CreateResult(EmptyEntry);

            var result = ModifierResultExtensions.Merge(left, right);

            Assert.That(result.Entries, Has.Exactly(left.Entries.Count).Items);
        }

        [Test]
        public void MergeWithRightMultipleEntriesReturnsCorrectNumberOfEntries()
        {
            var left = CreateResult(EmptyEntry);
            var right = CreateResult(CreateManyEntries());

            var result = ModifierResultExtensions.Merge(left, right);

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

            var result = ModifierResultExtensions.Merge(left, right);

            CollectionAssert.AreEqual(expected, result.Entries);
        }

        [Test]
        public void BuildDoesNotConvertNullStats()
        {
            var input = CreateResult(
                new[] { DefaultEntry.WithStat(null) },
                statConverter: s => s ?? throw new ArgumentNullException());

            var result = input.Build();

            Assert.Null(result[0].Stat);
        }

        [Test]
        public void BuildDoesNotConvertNullValues()
        {
            var input = CreateResult(
                new[] { DefaultEntry.WithValue(null) },
                valueConverter: v => v ?? throw new ArgumentNullException());

            var result = input.Build();

            Assert.Null(result[0].Value);
        }

        private static readonly ModifierResultEntry EmptyEntry = new ModifierResultEntry();

        private static readonly ModifierResultEntry DefaultEntry = EmptyEntry
            .WithStat(Mock.Of<IStatBuilder>())
            .WithForm(Mock.Of<IFormBuilder>())
            .WithValue(Mock.Of<IValueBuilder>())
            .WithCondition(Mock.Of<IConditionBuilder>());

        private static ModifierResultEntry[] CreateManyEntries()
        {
            var entry0 = DefaultEntry;
            var entry1 = DefaultEntry;
            var entry2 = DefaultEntry;
            return new[] { entry0, entry1, entry2 };
        }

        private static IModifierResult CreateResult(Func<IStatBuilder, IStatBuilder> statConverter)
        {
            return CreateResult(null, statConverter);
        }

        private static IModifierResult CreateResult(Func<IValueBuilder, IValueBuilder> valueConverter)
        {
            return CreateResult(null, valueConverter: valueConverter);
        }

        private static IModifierResult CreateResult(params ModifierResultEntry[] entries)
        {
            return CreateResult(entries, null);
        }

        private static IModifierResult CreateResult(IReadOnlyList<ModifierResultEntry> entries = null,
            Func<IStatBuilder, IStatBuilder> statConverter = null,
            Func<IValueBuilder, IValueBuilder> valueConverter = null)
        {
            return new SimpleModifierResult(entries ?? new ModifierResultEntry[0],
                statConverter ?? (s => s),
                valueConverter ?? (v => v));
        }
    }
}