using System.Collections;
using System.Linq;
using Moq;
using NUnit.Framework;
using PoESkillTree.Computation.Parsing.Data;
using PoESkillTree.Computation.Parsing.ModifierBuilding;
using PoESkillTree.Computation.Parsing.Referencing;
using static PoESkillTree.Computation.Parsing.StatMatcherRegexExpander;

namespace PoESkillTree.Computation.Parsing.Tests
{
    [TestFixture]
    public class StatMatcherRegexExpanderTest
    {
        [Test]
        public void OnlyExpandsOnce()
        {
            var statMatchers = MockStatMatchers(false);
            var sut = new StatMatcherRegexExpander(statMatchers, DefaultReferencedRegexes);

            sut.GetEnumerator();
            sut.GetEnumerator();

            Mock.Get(statMatchers).Verify(s => s.GetEnumerator(), Times.Once);
        }

        [TestCase(true, ExpectedResult = "^test$")]
        [TestCase(false, ExpectedResult = LeftDelimiterRegex + "test" + RightDelimiterRegex)]
        public string RespectsMatchesWholeLineOnlyProperty(bool matchesWholeLineOnly)
        {
            var statMatchers = MockStatMatchers(matchesWholeLineOnly, DefaultMatcherData);
            var sut = new StatMatcherRegexExpander(statMatchers, DefaultReferencedRegexes);

            var data = sut.First();

            return data.Regex;
        }

        [Test]
        public void DoesNotModifyModifierBuilder()
        {
            var sut = new StatMatcherRegexExpander(DefaultStatMatchers,
                DefaultReferencedRegexes);

            var data = sut.First();

            Assert.AreSame(DefaultMatcherData.ModifierBuilder, data.ModifierBuilder);
        }

        [Test]
        public void DoesNotModifyMatchSubstitution()
        {
            var sut = new StatMatcherRegexExpander(DefaultStatMatchers,
                DefaultReferencedRegexes);

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
        [TestCase("({Matchers1})", ExpectedResult = 
            "^(?<reference0_Matchers1>(0+)|([1-9])|((01)+))$")]
        [TestCase("test # ({Matchers1}) (.*) # ({Matchers2}) ({Matchers1})", ExpectedResult =
            "^test (?<value0>" + ValueRegex + ")" +
            " (?<reference0_Matchers1>(0+)|([1-9])|((01)+))" +
            " (.*) (?<value1>" + ValueRegex + ")" +
            " (?<reference1_Matchers2>(a)|(b)|(c)|(d))" +
            " (?<reference2_Matchers1>(0+)|([1-9])|((01)+))$")]
        public string ExpandsCorrectly(string inputRegex)
        {
            var inputData = new MatcherData(inputRegex, new ModifierBuilder());
            var statMatchers = MockStatMatchers(true, inputData);
            var sut = new StatMatcherRegexExpander(statMatchers, DefaultReferencedRegexes);

            var data = sut.First();

            return data.Regex;
        }

        [TestCase("value")]
        [TestCase("value5_xyz")]
        [TestCase("reference")]
        [TestCase("reference_groupNameX")]
        public void ThrowsIfRegexContainsInvalidGroupNames(string groupName)
        {
            var statMatcher = MockStatMatchers($"text (?<{groupName}>stuff)");
            var sut = new StatMatcherRegexExpander(statMatcher, DefaultReferencedRegexes);

            Assert.Throws<ParseException>(() => sut.GetEnumerator());
        }

        [Test]
        public void ThrowsIfRegexContainsUnknownReference()
        {
            var statMatcher = MockStatMatchers("text ({Matchers3}) stuff");
            var sut = new StatMatcherRegexExpander(statMatcher, DefaultReferencedRegexes);

            Assert.Throws<ParseException>(() => sut.GetEnumerator());
        }

        private static readonly MatcherData DefaultMatcherData =
            new MatcherData("test", new ModifierBuilder(), "substitution");

        private static readonly IStatMatchers DefaultStatMatchers =
            MockStatMatchers(false, DefaultMatcherData);

        private static IStatMatchers MockStatMatchers(bool matchesWholeLineOnly,
            params MatcherData[] data)
        {
            var dataList = data.ToList();
            var mock = new Mock<IStatMatchers>();
            mock.SetupGet(m => m.MatchesWholeLineOnly).Returns(matchesWholeLineOnly);
            mock.Setup(m => m.GetEnumerator()).Returns(() => dataList.GetEnumerator());
            mock.As<IEnumerable>().Setup(m => m.GetEnumerator()).Returns(() => dataList.GetEnumerator());
            return mock.Object;
        }

        private static IStatMatchers MockStatMatchers(string regex)
        {
            return MockStatMatchers(false, new MatcherData(regex, new ModifierBuilder()));
        }

        private static readonly IReferencedRegexes DefaultReferencedRegexes =
            Mock.Of<IReferencedRegexes>(r =>
                r.ContainsReference("Matchers1") &&
                r.GetRegexes("Matchers1") == new[] { "0+", "[1-9]", "(01)+" } &&
                r.ContainsReference("Matchers2") &&
                r.GetRegexes("Matchers2") == new[] { "a", "b", "c", "d" });
    }
}