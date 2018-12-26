using System.Collections.Generic;
using System.Linq;
using Moq;
using NUnit.Framework;
using PoESkillTree.Computation.Common.Builders.Modifiers;
using PoESkillTree.Computation.Common.Data;
using PoESkillTree.Computation.Parsing.Referencing;

namespace PoESkillTree.Computation.Parsing.Tests.Referencing
{
    [TestFixture]
    public class StatMatcherRegexExpanderTest
    {
        [Test]
        public void OnlyExpandsOnce()
        {
            var statMatchers = MockStatMatchers(false);
            var sut = CreateSut(statMatchers);

            sut.GetEnumerator();
            sut.GetEnumerator();

            Mock.Get(statMatchers).Verify(s => s.GetEnumerator(), Times.Once);
        }

        [TestCase(true, ExpectedResult = "^test$")]
        [TestCase(false, ExpectedResult = StatMatcherRegexExpander.LeftDelimiterRegex + "test" + StatMatcherRegexExpander.RightDelimiterRegex)]
        public string RespectsMatchesWholeLineOnlyProperty(bool matchesWholeLineOnly)
        {
            var statMatchers = MockStatMatchers(matchesWholeLineOnly, DefaultMatcherData);
            var sut = CreateSut(statMatchers);

            var data = sut.First();

            return data.Regex;
        }

        [Test]
        public void DoesNotModifyModifierBuilder()
        {
            var sut = CreateSut(DefaultStatMatchers);

            var data = sut.First();

            Assert.AreSame(DefaultMatcherData.Modifier, data.Modifier);
        }

        [Test]
        public void DoesNotModifyMatchSubstitution()
        {
            var sut = CreateSut(DefaultStatMatchers);

            var data = sut.First();

            Assert.AreSame(DefaultMatcherData.MatchSubstitution, data.MatchSubstitution);
        }

        [TestCase("test", ExpectedResult = "^test$")]
        [TestCase("test1 (.*) test2", ExpectedResult = "^test1 (.*) test2$")]
        [TestCase("#", ExpectedResult = "^(?<value0>" + StatMatcherRegexExpander.ValueRegex + ")$")]
        [TestCase("# #", ExpectedResult =
            "^(?<value0>" + StatMatcherRegexExpander.ValueRegex + ") (?<value1>" + StatMatcherRegexExpander.ValueRegex + ")$")]
        [TestCase("test # (.*) # (test2)+", ExpectedResult =
            "^test (?<value0>" + StatMatcherRegexExpander.ValueRegex + ") (.*) (?<value1>" + StatMatcherRegexExpander.ValueRegex + ") (test2)+$")]
        [TestCase("({Matchers1})", ExpectedResult =
            "^((?<reference0_Matchers1_1>[1-9])|(?<reference0_Matchers1_2>(01)+)|(?<reference0_Matchers1_0>0+))$")]
        [TestCase("test # ({Matchers1}) (.*) # ({Matchers2}) ({Matchers1})", ExpectedResult =
            "^test (?<value0>" + StatMatcherRegexExpander.ValueRegex + ")" +
            " ((?<reference0_Matchers1_1>[1-9])|(?<reference0_Matchers1_2>(01)+)|(?<reference0_Matchers1_0>0+))" +
            " (.*) (?<value1>" + StatMatcherRegexExpander.ValueRegex + ")" +
            " ((?<reference1_Matchers2_0>a)|(?<reference1_Matchers2_1>b)|(?<reference1_Matchers2_2>c)|(?<reference1_Matchers2_3>d))" +
            " ((?<reference2_Matchers1_1>[1-9])|(?<reference2_Matchers1_2>(01)+)|(?<reference2_Matchers1_0>0+))$")]
        public string ExpandsCorrectly(string inputRegex)
        {
            var inputData = new MatcherData(inputRegex, new ModifierBuilder());
            var statMatchers = MockStatMatchers(true, inputData);
            var sut = CreateSut(statMatchers);

            var data = sut.First();

            return data.Regex;
        }

        [TestCase("({Matchers3})", ExpectedResult =
            "^((?<reference0_Matchers3_2>d ((?<reference0_0_Matchers1_1>[1-9])|(?<reference0_0_Matchers1_0>0+)) ((?<reference0_1_Matchers2_0>a)))" +
            "|(?<reference0_Matchers3_0>((?<reference0_0_Matchers2_0>a)))" +
            "|(?<reference0_Matchers3_1>c))$")]
        [TestCase("({Matchers4})", ExpectedResult = 
            "^((?<reference0_Matchers4_0>((?<reference0_0_Matchers2_0>a))))$")]
        [TestCase("({Matchers5})", ExpectedResult =
            "^((?<reference0_Matchers5_0>((?<reference0_0_Matchers1_1>[1-9])|(?<reference0_0_Matchers1_0>0+)))" +
            "|(?<reference0_Matchers5_1>((?<reference0_0_Matchers2_0>a)))" +
            "|(?<reference0_Matchers5_2>((?<reference0_0_Matchers3_2>d ((?<reference0_0_0_Matchers1_1>[1-9])|(?<reference0_0_0_Matchers1_0>0+)) ((?<reference0_0_1_Matchers2_0>a)))|(?<reference0_0_Matchers3_0>((?<reference0_0_0_Matchers2_0>a)))|(?<reference0_0_Matchers3_1>c)))" +
            "|(?<reference0_Matchers5_3>((?<reference0_0_Matchers4_0>((?<reference0_0_0_Matchers2_0>a))))))$")]
        [TestCase("({Matchers6})", ExpectedResult =
            "^((?<reference0_Matchers6_0>((?<reference0_0_Matchers2_0>a))" +
            " ((?<reference0_1_Matchers2_0>a))" +
            " ((?<reference0_2_Matchers2_0>a))" +
            " ((?<reference0_3_Matchers2_0>a))))$")]
        public string ExpandsReferencesRecursively(string inputRegex)
        {
            var inputData = new MatcherData(inputRegex, new ModifierBuilder());
            var statMatchers = MockStatMatchers(true, inputData);
            var referencedRegexes = Mock.Of<IReferencedRegexes>(r =>
                r.GetRegexes("Matchers1") == new[] { "0+", "[1-9]" } &&
                r.GetRegexes("Matchers2") == new[] { "a" } &&
                r.GetRegexes("Matchers3") == new[] { "({Matchers2})", "c", "d ({Matchers1}) ({Matchers2})" } &&
                r.GetRegexes("Matchers4") == new[] { "({Matchers2})" } &&
                r.GetRegexes("Matchers5") == new[] { "({Matchers1})", "({Matchers2})", "({Matchers3})", "({Matchers4})" } &&
                r.GetRegexes("Matchers6") == new[] { "({Matchers2}) ({Matchers2}) ({Matchers2}) ({Matchers2})" });
            var sut = CreateSut(statMatchers, referencedRegexes);

            var data = sut.First();

            return data.Regex;
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
            return mock.Object;
        }

        private static readonly IReferencedRegexes DefaultReferencedRegexes =
            Mock.Of<IReferencedRegexes>(r =>
                r.GetRegexes("Matchers1") == new[] { "0+", "[1-9]", "(01)+" } &&
                r.GetRegexes("Matchers2") == new[] { "a", "b", "c", "d" });

        private static IEnumerable<MatcherData> CreateSut(IStatMatchers statMatchers)
        {
            return CreateSut(statMatchers, DefaultReferencedRegexes);
        }

        private static IEnumerable<MatcherData> CreateSut(
            IStatMatchers statMatchers, IReferencedRegexes referencedRegexe)
        {
            return new StatMatcherRegexExpander(statMatchers, referencedRegexe, new RegexGroupService(null));
        }
    }
}