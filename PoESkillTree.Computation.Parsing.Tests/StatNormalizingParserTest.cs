using Moq;
using NUnit.Framework;

namespace PoESkillTree.Computation.Parsing.Tests
{
    [TestFixture]
    public class StatNormalizingParserTest
    {
        [Test]
        public void IsIParser()
        {
            var sut = new StatNormalizingParser<string>(null);

            Assert.IsInstanceOf<IParser<string>>(sut);
        }

        [Test]
        public void TryParsePassesResultOnUnchanged()
        {
            var innerMock = new Mock<IParser<string>>();
            var result = "result";
            string _;
            innerMock.Setup(p => p.TryParse("stat", out _, out result));
            var sut = new StatNormalizingParser<string>(innerMock.Object);

            sut.TryParse("stat", out var _, out var actual);

            Assert.AreEqual("result", actual);
        }

        [Test]
        public void TryParsePassesRemainingOnUnchanged()
        {
            var innerMock = new Mock<IParser<string>>();
            var remaining = "remaining";
            string _;
            innerMock.Setup(p => p.TryParse("stat", out remaining, out _));
            var sut = new StatNormalizingParser<string>(innerMock.Object);

            sut.TryParse("stat", out var actual, out var _);

            Assert.AreEqual("remaining", actual);
        }

        [TestCase(true, ExpectedResult = true)]
        [TestCase(false, ExpectedResult = false)]
        public bool TryParseReturnsInnerReturn(bool innerReturn)
        {
            var innerMock = new Mock<IParser<string>>();
            string _;
            innerMock.Setup(p => p.TryParse("stat", out _, out _))
                .Returns(innerReturn);
            var sut = new StatNormalizingParser<string>(innerMock.Object);

            var actual = sut.TryParse("stat", out var _, out var _);

            return actual;
        }

        [TestCase("StAt In wEirD CaseS", "stat in weird cases")]
        [TestCase(" white-space enclosed ", "white-space enclosed")]
        [TestCase("many\n  \t spaces", "many spaces")]
        [TestCase(" \n\t\r", "")]
        [TestCase(" A\n lot Of\t\t pOinTleSS   White-space\t",
            "a lot of pointless white-space")]
        public void TryParseNormalizesStat(string input, string expected)
        {
            var innerMock = new Mock<IParser<string>>();
            var sut = new StatNormalizingParser<string>(innerMock.Object);

            sut.TryParse(input, out var _, out var _);

            string _;
            innerMock.Verify(p => p.TryParse(expected, out _, out _));
        }
    }
}