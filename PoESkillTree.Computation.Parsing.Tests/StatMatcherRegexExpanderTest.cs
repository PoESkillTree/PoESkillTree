using System.Linq;
using Moq;
using NUnit.Framework;
using PoESkillTree.Computation.Parsing.Data;
using PoESkillTree.Computation.Parsing.ModifierBuilding;
using static PoESkillTree.Computation.Parsing.StatMatcherRegexExpander;

namespace PoESkillTree.Computation.Parsing.Tests
{
    [TestFixture]
    public class StatMatcherRegexExpanderTest
    {
        [Test]
        public void OnlyExpandsOnce()
        {
            var statMatchers =
                Mock.Of<IStatMatchers>(s => s.Matchers == Enumerable.Empty<MatcherData>());
            var sut = new StatMatcherRegexExpander(statMatchers);

            sut.GetEnumerator();
            sut.GetEnumerator();

            Mock.Get(statMatchers).VerifyGet(s => s.Matchers, Times.Once);
        }

        [TestCase(true, ExpectedResult = "^test$")]
        [TestCase(false, ExpectedResult = LeftDelimiterRegex + "test" + RightDelimiterRegex)]
        public string RespectsMatchesWholeLineOnlyProperty(bool matchesWholeLineOnly)
        {
            var statMatchers = Mock.Of<IStatMatchers>(s =>
                s.Matchers == new[] { DefaultMatcherData }
                && s.MatchesWholeLineOnly == matchesWholeLineOnly);
            var sut = new StatMatcherRegexExpander(statMatchers);

            var data = sut.First();

            return data.Regex;
        }

        [Test]
        public void DoesNotModifyModifierBuilder()
        {
            var statMatchers = Mock.Of<IStatMatchers>(s =>
                s.Matchers == new[] { DefaultMatcherData });
            var sut = new StatMatcherRegexExpander(statMatchers);

            var data = sut.First();

            Assert.AreSame(DefaultMatcherData.ModifierBuilder, data.ModifierBuilder);
        }

        [Test]
        public void DoesNotModifyMatchSubstitution()
        {
            var statMatchers = Mock.Of<IStatMatchers>(s =>
                s.Matchers == new[] { DefaultMatcherData });
            var sut = new StatMatcherRegexExpander(statMatchers);

            var data = sut.First();

            Assert.AreSame(DefaultMatcherData.MatchSubstitution, data.MatchSubstitution);
        }

        [TestCase("test", ExpectedResult = "^test$")]
        [TestCase("test1 (.*) test2", ExpectedResult = "^test1 (.*) test2$")]
        [TestCase("#", ExpectedResult = "^(?<value0>" + ValueRegex + ")$")]
        [TestCase("# #", ExpectedResult =
            "^(?<value0>" + ValueRegex + ") (?<value1>" + ValueRegex + ")$")]
        [TestCase("test # (.*) # (test2)+", ExpectedResult =
            "^test (?<value0>" + ValueRegex + ") (.*) (?<value1>" + ValueRegex + ") (test2)+$")]
        public string ExpandsCorrectly(string inputRegex)
        {
            var inputData = new MatcherData(inputRegex, new ModifierBuilder());
            var statMatchers = Mock.Of<IStatMatchers>(s =>
                s.Matchers == new[] { inputData }
                && s.MatchesWholeLineOnly);
            var sut = new StatMatcherRegexExpander(statMatchers);

            var data = sut.First();

            return data.Regex;
        }

        private static readonly MatcherData DefaultMatcherData =
            new MatcherData("test", new ModifierBuilder(), "substitution");
    }
}