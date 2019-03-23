using System.Collections.Generic;
using Moq;
using NUnit.Framework;
using PoESkillTree.Computation.Common.Builders.Modifiers;
using PoESkillTree.Computation.Common.Data;
using PoESkillTree.Utils;

namespace PoESkillTree.Computation.Parsing.StringParsers
{
    [TestFixture]
    public class MatcherDataParserTest
    {
        [TestCase("ac", ExpectedResult = true)]
        [TestCase("a c", ExpectedResult = true)]
        [TestCase("a b", ExpectedResult = true)]
        [TestCase("test a b stuff", ExpectedResult = true)]
        [TestCase("abc", ExpectedResult = true)]
        [TestCase("xabx", ExpectedResult = true)]
        [TestCase("a", ExpectedResult = false)]
        public bool TryParseReturnsCorrectSuccessfullyParsed(string stat)
        {
            var sut = CreateSut();

            var (actual, _, _) = sut.Parse(stat);

            return actual;
        }

        [TestCase("ac", ExpectedResult = "")]
        [TestCase("a c", ExpectedResult = "")]
        [TestCase("a b", ExpectedResult = "")]
        [TestCase("test a b stuff", ExpectedResult = "test  stuff")]
        [TestCase("abc", ExpectedResult = "ac")]
        [TestCase("xabx", ExpectedResult = "xax")]
        public string TryParseReturnsCorrectRemaining(string stat)
        {
            var sut = CreateSut();

            var (_, actual, _) = sut.Parse(stat);

            return actual;
        }

        [TestCase("ac", 0)]
        [TestCase("a c", 0)]
        [TestCase("a b", 1)]
        [TestCase("test a b stuff", 1)]
        [TestCase("abc", 2)]
        [TestCase("xabx", 2)]
        public void TryParseReturnsCorrectModififer(string stat, int matcherDataIndex)
        {
            var expected = DefaultMatcherData[matcherDataIndex].Modifier;
            var sut = CreateSut();

            var (_, _, result) = sut.Parse(stat);

            var actual = result.Modifier;
            Assert.AreEqual(expected, actual);
        }

        [TestCase("ac", new[] {"ac", "c"})]
        [TestCase("a c", new[] {"a c", " c"})]
        public void TryParseReturnsCorrectGroups(string stat, string[] groups)
        {
            var expected = new Dictionary<string, string>
            {
                ["0"] = groups[0],
                ["g1"] = groups[0],
                ["g2"] = groups[1]
            };
            var sut = CreateSut();

            var (_, _, result) = sut.Parse(stat);

            var actual = result.RegexGroups;
            CollectionAssert.AreEqual(expected, actual);
        }

        [TestCase("a b", "a b")]
        [TestCase("test a b stuff", "a b")]
        [TestCase("abc", "ab")]
        [TestCase("xabx", "ab")]
        public void TryParseReturnsCorrectFullGrouo(string stat, string fullGroup)
        {
            var sut = CreateSut();

            var (_, _, result) = sut.Parse(stat);

            var actual = result.RegexGroups;
            Assert.That(actual, Has.Exactly(1).Items.EqualTo(new KeyValuePair<string, string>("0", fullGroup)));
        }

        private static readonly MatcherData[] DefaultMatcherData =
        {
            CreateMatcherData("(?<g1>(a)(?<g2>[ c]+))"),
            CreateMatcherData("a b"),
            CreateMatcherData("ab", "a")
        };

        private static MatcherDataParser CreateSut()
            => MatcherDataParser.Create(DefaultMatcherData, Funcs.Identity);

        private static MatcherData CreateMatcherData(string regex, string matchSubstitution = "")
        {
            var modifierResult = Mock.Of<IIntermediateModifier>();
            return new MatcherData(regex, modifierResult, matchSubstitution);
        }
    }
}