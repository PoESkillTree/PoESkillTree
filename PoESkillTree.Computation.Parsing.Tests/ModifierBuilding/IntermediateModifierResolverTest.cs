using System;
using System.Collections.Generic;
using System.Linq;
using Moq;
using NUnit.Framework;
using PoESkillTree.Common.Utils;
using PoESkillTree.Computation.Parsing.Builders.Conditions;
using PoESkillTree.Computation.Parsing.Builders.Forms;
using PoESkillTree.Computation.Parsing.Builders.Matching;
using PoESkillTree.Computation.Parsing.Builders.Stats;
using PoESkillTree.Computation.Parsing.Builders.Values;
using PoESkillTree.Computation.Parsing.ModifierBuilding;

namespace PoESkillTree.Computation.Parsing.Tests.ModifierBuilding
{
    [TestFixture]
    public class IntermediateModifierResolverTest
    {
        [Test]
        public void ResolveReturnsCorrectValueConverter()
        {
            var values = DefaultValues;
            var resultMock = new Mock<IIntermediateModifier>();
            resultMock.SetupGet(r => r.Entries).Returns(new IntermediateModififerEntry[0]);
            resultMock.SetupGet(r => r.ValueConverter).Returns(v => v == values[0] ? values[1] : values[0]);
            resultMock.SetupGet(r => r.StatConverter).Returns(Funcs.Identity);

            var sut = CreateSut();

            var resolved = sut.Resolve(resultMock.Object, DefaultContext);

            Assert.AreSame(values[2], resolved.ValueConverter(values[0]));
            Assert.AreSame(values[1], resolved.ValueConverter(values[1]));
            Assert.AreSame(values[1], resolved.ValueConverter(values[2]));
        }

        [Test]
        public void ResolveReturnsCorrectStatConverter()
        {
            var stats = DefaultStats;
            var resultMock = new Mock<IIntermediateModifier>();
            resultMock.SetupGet(r => r.Entries).Returns(new IntermediateModififerEntry[0]);
            resultMock.SetupGet(r => r.ValueConverter).Returns(Funcs.Identity);
            resultMock.SetupGet(r => r.StatConverter).Returns(s => s == stats[0] ? stats[1] : stats[0]);

            var sut = CreateSut();

            var resolved = sut.Resolve(resultMock.Object, DefaultContext);

            Assert.AreSame(stats[2], resolved.StatConverter(stats[0]));
            Assert.AreSame(stats[1], resolved.StatConverter(stats[1]));
            Assert.AreSame(stats[1], resolved.StatConverter(stats[2]));
        }

        [Test]
        public void ResolveReturnsCorrectNumberOfEntries()
        {
            var sut = CreateSut();

            var resolved = sut.Resolve(DefaultModifier, DefaultContext);

            Assert.That(resolved.Entries, Has.Exactly(3).Items);
        }

        private static readonly
            (Func<IReadOnlyList<object>> expectedSelector, Func<IntermediateModififerEntry, object> actualSelector)[]
            ResolveReturnsCorrectEntryElementsCases =
            {
                (() => DefaultValues, e => e.Value),
                (() => DefaultForms, e => e.Form),
                (() => DefaultStats, e => e.Stat),
                (() => DefaultConditions, e => e.Condition),
            };
        [TestCase(0, TestName = "ResolveReturnsCorrectValues")]
        [TestCase(1, TestName = "ResolveReturnsCorrectForms")]
        [TestCase(2, TestName = "ResolveReturnsCorrectStats")]
        [TestCase(3, TestName = "ResolveReturnsCorrectConditions")]
        public void ResolveReturnsCorrectEntryElements(int testCaseIndex)
        {
            var expectedElements = ResolveReturnsCorrectEntryElementsCases[testCaseIndex].expectedSelector();
            var actualSelector = ResolveReturnsCorrectEntryElementsCases[testCaseIndex].actualSelector;
            var sut = CreateSut();

            var resolved = sut.Resolve(DefaultModifier, DefaultContext);

            var entries = resolved.Entries;
            Assert.AreSame(expectedElements[1], actualSelector(entries[0]));
            Assert.AreSame(expectedElements[2], actualSelector(entries[1]));
            Assert.AreSame(expectedElements[2], actualSelector(entries[2]));
        }

        [Test]
        public void ResolveToReferencedBuilderThrowsIfNoEntries()
        {
            var result = Mock.Of<IIntermediateModifier>(r =>
                r.Entries == new IntermediateModififerEntry[0] &&
                r.ValueConverter == Funcs.Identity &&
                r.StatConverter == Funcs.Identity);
            var sut = CreateSut();

            Assert.Throws<ParseException>(() => sut.ResolveToReferencedBuilder(result, DefaultContext));
        }

        [Test]
        public void ResolveToReferencedBuilderThrowsIfMultipleEntries()
        {
            var result = Mock.Of<IIntermediateModifier>(r =>
                r.Entries == DefaultEntries &&
                r.ValueConverter == Funcs.Identity &&
                r.StatConverter == Funcs.Identity);
            var sut = CreateSut();

            Assert.Throws<ParseException>(() => sut.ResolveToReferencedBuilder(result, DefaultContext));
        }

        [Test]
        public void ResolveToReferencedBuilderThrowsIfEntryHasValue()
        {
            var entry = new IntermediateModififerEntry()
                .WithValue(DefaultValues[0])
                .WithStat(DefaultStats[0]);
            var result = Mock.Of<IIntermediateModifier>(r =>
                r.Entries == new[] {entry} &&
                r.ValueConverter == Funcs.Identity &&
                r.StatConverter == Funcs.Identity);
            var sut = CreateSut();

            Assert.Throws<ParseException>(() => sut.ResolveToReferencedBuilder(result, DefaultContext));
        }

        [Test]
        public void ResolveToReferencedBuilderThrowsIfEntryHasForm()
        {
            var entry = new IntermediateModififerEntry()
                .WithForm(DefaultForms[0])
                .WithStat(DefaultStats[0]);
            var result = Mock.Of<IIntermediateModifier>(r =>
                r.Entries == new[] { entry } &&
                r.ValueConverter == Funcs.Identity &&
                r.StatConverter == Funcs.Identity);
            var sut = CreateSut();

            Assert.Throws<ParseException>(() => sut.ResolveToReferencedBuilder(result, DefaultContext));
        }

        [Test]
        public void ResolveToReferencedBuilderThrowsIfEntryHasNoStat()
        {
            var entry = new IntermediateModififerEntry()
                .WithValue(DefaultValues[0])
                .WithForm(DefaultForms[0])
                .WithCondition(DefaultConditions[0]);
            var result = Mock.Of<IIntermediateModifier>(r =>
                r.Entries == new[] { entry } &&
                r.ValueConverter == Funcs.Identity &&
                r.StatConverter == Funcs.Identity);
            var sut = CreateSut();

            Assert.Throws<ParseException>(() => sut.ResolveToReferencedBuilder(result, DefaultContext));
        }

        [Test]
        public void ResolveToReferencedBuilderReturnsResolvedStat()
        {
            var entry = new IntermediateModififerEntry()
                .WithStat(DefaultStats[0]);
            var result = Mock.Of<IIntermediateModifier>(r =>
                r.Entries == new[] { entry } &&
                r.ValueConverter == Funcs.Identity &&
                r.StatConverter == Funcs.Identity);
            var sut = CreateSut();

            var resolved = sut.ResolveToReferencedBuilder(result, DefaultContext);

            Assert.AreSame(DefaultStats[1], resolved);
        }

        [Test]
        public void ResolveToReferencedBuilderAppliesStatConverter()
        {
            var entry = new IntermediateModififerEntry()
                .WithStat(DefaultStats[0]);
            Func<IStatBuilder, IStatBuilder> statConverter = s => s == DefaultStats[0] ? DefaultStats[1] : null;
            var result = Mock.Of<IIntermediateModifier>(r =>
                r.Entries == new[] { entry } &&
                r.ValueConverter == Funcs.Identity &&
                r.StatConverter == statConverter);
            var sut = CreateSut();

            var resolved = sut.ResolveToReferencedBuilder(result, DefaultContext);

            Assert.AreSame(DefaultStats[2], resolved);
        }

        [Test]
        public void ResolveToReferencedBuilderAddsCondition()
        {
            var stat = Mock.Of<IStatBuilder>(s => s.WithCondition(DefaultConditions[0]) == DefaultStats[1]);
            var entry = new IntermediateModififerEntry()
                .WithStat(stat)
                .WithCondition(DefaultConditions[0]);
            var result = Mock.Of<IIntermediateModifier>(r =>
                r.Entries == new[] { entry } &&
                r.ValueConverter == Funcs.Identity &&
                r.StatConverter == Funcs.Identity);
            var sut = CreateSut();

            var resolved = sut.ResolveToReferencedBuilder(result, DefaultContext);

            Assert.AreSame(DefaultStats[2], resolved);
        }

        private static IIntermediateModifierResolver CreateSut() => new IntermediateModifierResolver(new ModifierBuilder());

        private static readonly ResolveContext DefaultContext = 
            new ResolveContext(
                Mock.Of<IMatchContext<IValueBuilder>>(),
                Mock.Of<IMatchContext<IReferenceConverter>>());

        private static T[] CreateMocks<T>() where T : class, IResolvable<T>
        {
            var t3 = Mock.Of<T>(s => s.Resolve(DefaultContext) == s);
            var t2 = Mock.Of<T>(s => s.Resolve(DefaultContext) == t3);
            var t1 = Mock.Of<T>(s => s.Resolve(DefaultContext) == t2);
            return new[] { t1, t2, t3 };
        }

        private static readonly IValueBuilder[] DefaultValues = CreateMocks<IValueBuilder>();
        private static readonly IFormBuilder[] DefaultForms = CreateMocks<IFormBuilder>();
        private static readonly IStatBuilder[] DefaultStats = CreateMocks<IStatBuilder>();
        private static readonly IConditionBuilder[] DefaultConditions = CreateMocks<IConditionBuilder>();

        private static readonly IReadOnlyList<IntermediateModififerEntry> DefaultEntries =
            Enumerable.Range(0, 3)
                .Select(_ => new IntermediateModififerEntry())
                .Zip(DefaultValues, (e, v) => e.WithValue(v))
                .Zip(DefaultForms, (e, f) => e.WithForm(f))
                .Zip(DefaultStats, (e, s) => e.WithStat(s))
                .Zip(DefaultConditions, (e, c) => e.WithCondition(c))
                .ToList();

        private static readonly IIntermediateModifier DefaultModifier =
            Mock.Of<IIntermediateModifier>(r =>
                r.Entries == DefaultEntries &&
                r.ValueConverter == Funcs.Identity &&
                r.StatConverter == Funcs.Identity);
    }
}