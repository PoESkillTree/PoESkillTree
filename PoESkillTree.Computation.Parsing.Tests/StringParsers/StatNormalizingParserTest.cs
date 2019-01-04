using NUnit.Framework;
using PoESkillTree.Computation.Parsing.StringParsers;

namespace PoESkillTree.Computation.Parsing.Tests.StringParsers
{
    [TestFixture]
    public class StatNormalizingParserTest
    {
        [Test]
        public void IsIParser()
        {
            var sut = new StatNormalizingParser<string>(null);

            Assert.IsInstanceOf<IStringParser<string>>(sut);
        }

        [TestCase(true, ExpectedResult = true)]
        [TestCase(false, ExpectedResult = false)]
        public bool TryParsePassesSuccessfullyParsed(bool innerSuccess)
        {
            var inner = StringParserTestUtils.MockParser("stat", innerSuccess, default, "").Object;
            var sut = new StatNormalizingParser<string>(inner);

            var (actual, _, _) = sut.Parse("stat");

            return actual;
        }

        [Test]
        public void TryParsePassesRemainingd()
        {
            const string expected = "remaining";
            var inner = StringParserTestUtils.MockParser("stat", default, expected, "").Object;
            var sut = new StatNormalizingParser<string>(inner);

            var (_, actual, _) = sut.Parse("stat");

            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void TryParsePassesResul()
        {
            const string expected = "result";
            var inner = StringParserTestUtils.MockParser("stat", default, default, expected).Object;
            var sut = new StatNormalizingParser<string>(inner);

            var (_, _, actual) = sut.Parse("stat");

            Assert.AreEqual(expected, actual);
        }

        [TestCase("StAt In wEirD CaseS", "StAt In wEirD CaseS")]
        [TestCase(" white-space enclosed ", "white-space enclosed")]
        [TestCase("many\n  \t spaces", "many spaces")]
        [TestCase(" \n\t\r", "")]
        [TestCase(" A\n lot Of\t\t pointless   White-space\t",
            "A lot Of pointless White-space")]
        public void TryParseNormalizesStat(string input, string expected)
        {
            var innerMock = StringParserTestUtils.MockParser<string>();
            var sut = new StatNormalizingParser<string>(innerMock.Object);

            sut.Parse(input);

            innerMock.VerifyParse(expected);
        }
    }
}