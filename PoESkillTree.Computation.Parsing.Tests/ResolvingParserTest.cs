using System.Collections.Generic;
using Moq;
using NUnit.Framework;
using PoESkillTree.Computation.Parsing.Builders.Matching;
using PoESkillTree.Computation.Parsing.Builders.Stats;
using PoESkillTree.Computation.Parsing.Builders.Values;
using PoESkillTree.Computation.Parsing.Data;
using PoESkillTree.Computation.Parsing.ModifierBuilding;
using PoESkillTree.Computation.Parsing.Referencing;

namespace PoESkillTree.Computation.Parsing.Tests
{
    [TestFixture]
    public class ResolvingParserTest
    {
        [Test(ExpectedResult = false)]
        public bool TryParseReturnsFalseIfInnerReturnsFalse()
        {
            var sut = CreateFailingSut();

            return sut.TryParse(FailingStat, out var _, out var _);
        }

        [TestCase("failRemaining")]
        public void TryParseOutputsInnerRemainingIfInnerReturnsFalse(string remaining)
        {
            var sut = CreateFailingSut(remaining);

            var _ = sut.TryParse(FailingStat, out var actual, out var _);

            Assert.AreEqual(remaining, actual);
        }

        [Test]
        public void TryParseOutputsCorrectResultIfInnerReturnsFalse()
        {
            var parseResult = new MatcherDataParseResult(DefaultModifier, null);
            var sut = CreateFailingSut(result: parseResult);

            var _ = sut.TryParse(FailingStat, out var _, out var actual);

            Assert.AreSame(DefaultModifier, actual);
        }

        [Test(ExpectedResult = true)]
        public bool TryParseReturnsTrueIfInnerReturnsTrue()
        {
            var sut = CreateSuccessfulSut();

            return sut.TryParse(SuccessfulStat, out var _, out var _);
        }

        [TestCase("remaining")]
        public void TryParseOutputsInnerRemainingIfInnerReturnsTrue(string remaining)
        {
            var sut = CreateSuccessfulSut(remaining);

            var _ = sut.TryParse(SuccessfulStat, out var actual, out var _);

            Assert.AreEqual(remaining, actual);
        }

        [Test]
        public void TryParseReturnsCorrectResultIfInnerReturnsTrueAndNoGroups()
        {
            var sut = CreateSuccessfulSut();

            var _ = sut.TryParse(SuccessfulStat, out var _, out var actual);

            Assert.AreSame(ResolvedModifier, actual);
        }

        [Test]
        public void TryParseReturnsCorrectResultIfInnerReturnsTrue()
        {
            // Recursion + lots of dependencies -> a lot of setup required. Not nice, but necessary somewhere as I
            // don't see a way of separating dependency usage and recursion.

            var groups = new Dictionary<string, string>();
            var rootValues = MockMany<IValueBuilder>();
            var nestedValues = MockMany<IValueBuilder>();
            var rootReferences = new[]
            {
                ("r0", 0, "p0"),
                ("r1", 1, "p1"),
                ("r2", 2, "p2"),
            };
            const string rootReferencedMatch = "rootMatch";
            var rootReferencedModifier = Mock.Of<IIntermediateModifier>();
            var nestedReferences = new[]
            {
                ("r3", 3, "p3"),
                ("r4", 4, "p4"),
                ("r5", 5, "p5"),
            };
            const string nestedReferencedMatch = "nestedMatch";
            var nestedReferencedModifier = Mock.Of<IIntermediateModifier>();

            var remaining = "";
            var result = new MatcherDataParseResult(DefaultModifier, groups);
            var innerParser = Mock.Of<IParser<MatcherDataParseResult>>(p =>
                p.TryParse(SuccessfulStat, out remaining, out result) == true);

            var rootReferencedMatcherData = new ReferencedMatcherData("", rootReferencedMatch);
            var rootMatcherData = new MatcherData("", rootReferencedModifier);
            var nestedReferencedMatcherData = new ReferencedMatcherData("", nestedReferencedMatch);
            var nestedMatcherData = new MatcherData("", nestedReferencedModifier);
            var referenceManager = Mock.Of<IReferenceToMatcherDataResolver>(m =>
                m.TryGetReferencedMatcherData("r0", 0, out rootReferencedMatcherData) &&
                m.TryGetMatcherData("r1", 1, out rootMatcherData) &&
                m.TryGetReferencedMatcherData("r2", 2, out rootReferencedMatcherData) &&
                m.TryGetMatcherData("r3", 3, out nestedMatcherData) &&
                m.TryGetReferencedMatcherData("r4", 4, out nestedReferencedMatcherData) &&
                m.TryGetReferencedMatcherData("r5", 5, out nestedReferencedMatcherData));

            var rootResolvedBuilder = Mock.Of<IStatBuilder>();
            var rootReferenceConverters = new[]
            {
                new ReferenceConverter(rootReferencedMatch),
                new ReferenceConverter(rootResolvedBuilder),
                new ReferenceConverter(rootReferencedMatch),
            };
            var context = new ResolveContext(
                new ResolvedMatchContext<IValueBuilder>(rootValues),
                new ResolvedMatchContext<IReferenceConverter>(rootReferenceConverters));

            var nestedResolvedBuilder = Mock.Of<IStatBuilder>();
            var nestedReferenceConverters = new[]
            {
                new ReferenceConverter(nestedResolvedBuilder),
                new ReferenceConverter(nestedReferencedMatch),
                new ReferenceConverter(nestedReferencedMatch),
            };
            var nestedContext = new ResolveContext(
                new ResolvedMatchContext<IValueBuilder>(nestedValues),
                new ResolvedMatchContext<IReferenceConverter>(nestedReferenceConverters));
            var twiceNestedContext = new ResolveContext(
                new ResolvedMatchContext<IValueBuilder>(new IValueBuilder[0]), 
                new ResolvedMatchContext<IReferenceConverter>(new IReferenceConverter[0]));
            var modifierResolver = Mock.Of<IIntermediateModifierResolver>(r =>
                r.Resolve(result.Modifier, context) == ResolvedModifier &&
                r.ResolveToReferencedBuilder(rootReferencedModifier, nestedContext) == rootResolvedBuilder &&
                r.ResolveToReferencedBuilder(nestedReferencedModifier, twiceNestedContext) == nestedResolvedBuilder);

            var regexGroupParser = Mock.Of<IRegexGroupParser>(p =>
                p.ParseValues(groups, "") == rootValues &&
                p.ParseReferences(groups.Keys, "") == rootReferences &&
                p.ParseValues(groups, "p1") == nestedValues &&
                p.ParseReferences(groups.Keys, "p1") == nestedReferences);

            var sut = new ResolvingParser(innerParser, referenceManager, modifierResolver, regexGroupParser);

            var _ = sut.TryParse(SuccessfulStat, out var _, out var actual);

            Assert.AreSame(ResolvedModifier, actual);
        }

        [Test]
        public void TryParseThrowsOnUnknownReference()
        {
            var groups = new Dictionary<string, string>();
            var references = new[] { ("r0", 0, "p0") };
            var remaining = "";
            var result = new MatcherDataParseResult(DefaultModifier, groups);
            var innerParser = Mock.Of<IParser<MatcherDataParseResult>>(p =>
                p.TryParse(SuccessfulStat, out remaining, out result) == true);
            var regexGroupParser = Mock.Of<IRegexGroupParser>(p =>
                p.ParseValues(groups, "") == new IValueBuilder[0] &&
                p.ParseReferences(groups.Keys, "") == references);

            var sut = new ResolvingParser(innerParser, Mock.Of<IReferenceToMatcherDataResolver>(),
                Mock.Of<IIntermediateModifierResolver>(), regexGroupParser);

            Assert.Throws<ParseException>(() => sut.TryParse(SuccessfulStat, out var _, out var _));
        }

        private const string FailingStat = "fail";
        private const string SuccessfulStat = "success";

        private static readonly IIntermediateModifier DefaultModifier = Mock.Of<IIntermediateModifier>();
        private static readonly IIntermediateModifier ResolvedModifier = Mock.Of<IIntermediateModifier>();

        private static IParser<IIntermediateModifier> CreateFailingSut(
            string remaining = "", MatcherDataParseResult result = null)
        {
            var innerParser = Mock.Of<IParser<MatcherDataParseResult>>(p =>
                p.TryParse(FailingStat, out remaining, out result) == false);

            return new ResolvingParser(innerParser, null, null, null);
        }

        private static IParser<IIntermediateModifier> CreateSuccessfulSut(
            string remaining = "")
        {
            var result = new MatcherDataParseResult(DefaultModifier, new Dictionary<string, string>());
            var innerParser = Mock.Of<IParser<MatcherDataParseResult>>(p =>
                p.TryParse(SuccessfulStat, out remaining, out result) == true);
            var context = new ResolveContext(
                new ResolvedMatchContext<IValueBuilder>(new IValueBuilder[0]),
                new ResolvedMatchContext<IReferenceConverter>(new IReferenceConverter[0]));
            var modifierResultResolver = Mock.Of<IIntermediateModifierResolver>(r =>
                r.Resolve(result.Modifier, context) == ResolvedModifier);
            var regexGroupParser = Mock.Of<IRegexGroupParser>();

            return new ResolvingParser(innerParser, null, modifierResultResolver, regexGroupParser);
        }

        private static T[] MockMany<T>() where T: class => 
            new[] { Mock.Of<T>(), Mock.Of<T>(), Mock.Of<T>() };
    }
}