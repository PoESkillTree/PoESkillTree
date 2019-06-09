using System;
using System.Collections.Generic;
using System.Linq;
using Moq;
using NUnit.Framework;
using PoESkillTree.Computation.Common.Builders.Conditions;
using PoESkillTree.Computation.Common.Builders.Forms;
using PoESkillTree.Computation.Common.Builders.Stats;
using PoESkillTree.Computation.Common.Builders.Values;
using PoESkillTree.GameModel;
using PoESkillTree.Utils;

namespace PoESkillTree.Computation.Common.Builders.Modifiers
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
            var entryRight = CreateFilledEntry(condition: rightCondition);
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
            var rightCondition = Mock.Of<IConditionBuilder>();
            var leftCondition = Mock.Of<IConditionBuilder>(
                c => c.And(rightCondition) == leftAndRightCondition);
            var rightEntry = EmptyEntry
                .WithStat(stat)
                .WithCondition(rightCondition);
            var left = CreateResult(
                EmptyEntry.WithCondition(leftCondition),
                EmptyEntry.WithForm(form1),
                EmptyEntry);
            var right = CreateResult(rightEntry);
            var expected = new[]
            {
                EmptyEntry.WithStat(stat).WithCondition(leftAndRightCondition),
                EmptyEntry.WithForm(form1).WithStat(stat).WithCondition(rightCondition),
                EmptyEntry.WithStat(stat).WithCondition(rightCondition)
            };

            var result = left.MergeWith(right);

            CollectionAssert.AreEqual(expected, result.Entries);
        }

        [Test]
        public void MergeWithDoesNotSwapLeftAndRight()
        {
            var expected = Mock.Of<IConditionBuilder>();
            var rightCondition = Mock.Of<IConditionBuilder>();
            var leftCondition = Mock.Of<IConditionBuilder>(
                c => c.And(rightCondition) == expected);
            var leftEntry = EmptyEntry.WithCondition(leftCondition);
            var left = CreateResult(leftEntry, leftEntry);
            var rightEntry = EmptyEntry.WithCondition(rightCondition);
            var right = CreateResult(rightEntry);

            var result = left.MergeWith(right);
            var actual = result.Entries[0].Condition;

            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void BuildSkipsEntriesWithNullStats()
        {
            var input = CreateResult(DefaultEntry.WithStat(null));

            var result = input.Build(Source, Entity);

            CollectionAssert.IsEmpty(result);
        }

        [Test]
        public void BuildSkipsEntriesWithNullForms()
        {
            var input = CreateResult(DefaultEntry.WithForm(null));

            var result = input.Build(Source, Entity);

            CollectionAssert.IsEmpty(result);
        }

        [Test]
        public void BuildSkipsEntriesWithNullValues()
        {
            var input = CreateResult(DefaultEntry.WithValue(null));

            var result = input.Build(Source, Entity);

            CollectionAssert.IsEmpty(result);
        }

        [Test]
        public void BuildReturnsCorrectModifiers()
        {
            var conditionBuilder = Mock.Of<IConditionBuilder>();
            var buildParameters = new BuildParameters(Source, Entity, Form.More);

            var value = Mock.Of<IValue>();
            var valueBuilder = Mock.Of<IValueBuilder>();
            var convertedValueBuilder = Mock.Of<IValueBuilder>();
            var statConvertedValueBuilder = Mock.Of<IValueBuilder>();
            var formConvertedValueBuilder = Mock.Of<IValueBuilder>(b => b.Build(buildParameters) == value);

            var stats = new[] { Mock.Of<IStat>() };
            var source = new ModifierSource.Local.Given();
            IValueBuilder StatConvertValue(IValueBuilder v) =>
                v == convertedValueBuilder ? statConvertedValueBuilder : v;
            var statBuilderResult = new StatBuilderResult(stats, source, StatConvertValue);
            var convertedStatBuilder =
                Mock.Of<IStatBuilder>(b => b.Build(buildParameters) == new[] { statBuilderResult });
            var statBuilderWithCondition = Mock.Of<IStatBuilder>();
            var statBuilder = Mock.Of<IStatBuilder>(s => s.WithCondition(conditionBuilder) == statBuilderWithCondition);

            IValueBuilder FormConvertValue(IValueBuilder v) =>
                v == statConvertedValueBuilder ? formConvertedValueBuilder : v;
            var formBuilderMock = new Mock<IFormBuilder>();
            formBuilderMock.Setup(b => b.Build()).Returns((buildParameters.ModifierForm, FormConvertValue));
            var formBuilder = formBuilderMock.Object;

            var entry = EmptyEntry
                .WithStat(statBuilder)
                .WithForm(formBuilder)
                .WithValue(valueBuilder)
                .WithCondition(conditionBuilder);

            var input = CreateResult(
                new[] { entry },
                s => s == statBuilderWithCondition ? convertedStatBuilder : s,
                v => v == valueBuilder ? convertedValueBuilder : v);

            var result = input.Build(Source, Entity);

            Assert.AreEqual(1, result.Count);
            var item = result[0];
            Assert.AreEqual(stats, item.Stats);
            Assert.AreEqual(buildParameters.ModifierForm, item.Form);
            Assert.AreEqual(value, item.Value);
            Assert.AreSame(source, item.Source);
        }

        [Test]
        public void BuildThrowsForLocalTotalOverride()
        {
            var statBuilderResult =
                new StatBuilderResult(new IStat[0], new ModifierSource.Local.Skill(""), Funcs.Identity);
            var statBuilder = Mock.Of<IStatBuilder>(
                b => b.Build(It.IsAny<BuildParameters>()) == new[] { statBuilderResult });
            var formBuilderMock = new Mock<IFormBuilder>();
            formBuilderMock.Setup(b => b.Build()).Returns((Form.TotalOverride, (ValueConverter) Funcs.Identity));
            var formBuilder = formBuilderMock.Object;
            var entry = EmptyEntry
                .WithStat(statBuilder)
                .WithForm(formBuilder)
                .WithValue(Mock.Of<IValueBuilder>());
            var input = CreateResult(entry);

            Assert.Throws<ArgumentException>(() => input.Build(Source, Entity));
        }

        private static readonly IntermediateModifierEntry EmptyEntry = new IntermediateModifierEntry();

        private static readonly IntermediateModifierEntry DefaultEntry = CreateFilledEntry();

        private static IntermediateModifierEntry[] CreateManyEntries()
            => new[] { CreateFilledEntry(), CreateFilledEntry(), CreateFilledEntry() };

        private static IntermediateModifierEntry CreateFilledEntry(
            IFormBuilder form = null, IStatBuilder stat = null, IValueBuilder value = null,
            IConditionBuilder condition = null)
            => new IntermediateModifierEntry(
                form ?? Mock.Of<IFormBuilder>(),
                stat ?? Mock.Of<IStatBuilder>(),
                value ?? Mock.Of<IValueBuilder>(),
                condition ?? Mock.Of<IConditionBuilder>());

        private static readonly ModifierSource Source = new ModifierSource.Global();

        private static readonly Entity Entity = Entity.Character;

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