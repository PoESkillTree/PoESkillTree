using Moq;
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
            var inner = Mock.Of<IStringParser<string>>(p =>
                p.Parse("stat") == new StringParseResult<string>(innerSuccess, default, default));
            var sut = new StatNormalizingParser<string>(inner);

            var (actual, _, _) = sut.Parse("stat");

            return actual;
        }

        [Test]
        public void TryParsePassesRemainingd()
        {
            const string expected = "remaining";
            var inner = Mock.Of<IStringParser<string>>(p =>
                p.Parse("stat") == new StringParseResult<string>(default, expected, default));
            var sut = new StatNormalizingParser<string>(inner);

            var (_, actual, _) = sut.Parse("stat");

            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void TryParsePassesResul()
        {
            const string expected = "result";
            var inner = Mock.Of<IStringParser<string>>(p =>
                p.Parse("stat") == new StringParseResult<string>(default, default, expected));
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
            var innerMock = new Mock<IStringParser<string>>();
            var sut = new StatNormalizingParser<string>(innerMock.Object);

            sut.Parse(input);

            innerMock.Verify(p => p.Parse(expected));
        }
    }
}