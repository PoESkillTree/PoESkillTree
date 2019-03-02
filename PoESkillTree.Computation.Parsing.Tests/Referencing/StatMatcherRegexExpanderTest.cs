using Moq;
using NUnit.Framework;
using PoESkillTree.Computation.Parsing.Referencing;

namespace PoESkillTree.Computation.Parsing.Tests.Referencing
{
    [TestFixture]
    public class StatMatcherRegexExpanderTest
    {
        [TestCase(true, ExpectedResult = "^" + DefaultRegex + "$")]
        [TestCase(false, ExpectedResult = StatMatcherRegexExpander.LeftDelimiterRegex + DefaultRegex + StatMatcherRegexExpander.RightDelimiterRegex)]
        public string RespectsMatchesWholeLineOnlyProperty(bool matchesWholeLineOnly)
        {
            var sut = CreateSut(matchesWholeLineOnly);

            return sut.Expand(DefaultRegex);
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
            var sut = CreateSut(true);

            return sut.Expand(inputRegex);
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
            var referencedRegexes = Mock.Of<IReferencedRegexes>(r =>
                r.GetRegexes("Matchers1") == new[] { "0+", "[1-9]" } &&
                r.GetRegexes("Matchers2") == new[] { "a" } &&
                r.GetRegexes("Matchers3") == new[] { "({Matchers2})", "c", "d ({Matchers1}) ({Matchers2})" } &&
                r.GetRegexes("Matchers4") == new[] { "({Matchers2})" } &&
                r.GetRegexes("Matchers5") == new[] { "({Matchers1})", "({Matchers2})", "({Matchers3})", "({Matchers4})" } &&
                r.GetRegexes("Matchers6") == new[] { "({Matchers2}) ({Matchers2}) ({Matchers2}) ({Matchers2})" });
            var sut = CreateSut(referencedRegexes, true);

            return sut.Expand(inputRegex);
        }

        private const string DefaultRegex = "test";

        private static readonly IReferencedRegexes DefaultReferencedRegexes =
            Mock.Of<IReferencedRegexes>(r =>
                r.GetRegexes("Matchers1") == new[] { "0+", "[1-9]", "(01)+" } &&
                r.GetRegexes("Matchers2") == new[] { "a", "b", "c", "d" });

        private static StatMatcherRegexExpander CreateSut(bool matchesWholeLineOnly)
            => CreateSut(DefaultReferencedRegexes, matchesWholeLineOnly);

        private static StatMatcherRegexExpander CreateSut(
            IReferencedRegexes referencedRegexe, bool matchesWholeLineOnly)
            => new StatMatcherRegexExpander(referencedRegexe, new RegexGroupService(null), matchesWholeLineOnly);
    }
}