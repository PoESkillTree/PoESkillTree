using Moq;
using NUnit.Framework;

namespace PoESkillTree.Computation.Parsing.Tests
{
    [TestFixture]
    public class CachingParserTest
    {
        private const string TrueStat = "true";
        private const string TrueRemaining = "trueRemaining";
        private const string TrueParsed = "trueParsed";
        private const string FalseStat = "false";
        private const string FalseRemaining = "falseRemaining";
        private const string FalseParsed = "falseParsed";

        private Mock<IParser<string>> _innerMock;
        private IParser<string> _inner;

        [SetUp]
        public void SetUp()
        {
            _innerMock = new Mock<IParser<string>>();
            var trueRemaining = TrueRemaining;
            var trueParsed = TrueParsed;
            var falseRemaining = FalseRemaining;
            var falseParsed = FalseParsed;
            _innerMock.Setup(p => p.TryParse(TrueStat, out trueRemaining, out trueParsed))
                .Returns(true);
            _innerMock.Setup(p => p.TryParse(FalseStat, out falseRemaining, out falseParsed))
                .Returns(false);
            _inner = _innerMock.Object;
        }

        [Test]
        public void IsIParserString()
        {
            var sut = new CachingParser<string>(_inner);

            Assert.IsInstanceOf<IParser<string>>(sut);
        }

        [Test]
        public void IsIParserInt()
        {
            var sut = new CachingParser<int>(Mock.Of<IParser<int>>());

            Assert.IsInstanceOf<IParser<int>>(sut);
        }

        [Test]
        public void TryParseReturnsTrueIfInjectedReturnsTrue()
        {
            var sut = new CachingParser<string>(_inner);

            var actual = sut.TryParse(TrueStat, out var _, out var _);

            Assert.True(actual);
        }

        [Test]
        public void TryParseReturnsFalseIfInjectedReturnsFalse()
        {
            var sut = new CachingParser<string>(_inner);

            var actual = sut.TryParse(FalseStat, out var _, out var _);

            Assert.False(actual);
        }

        [Test]
        public void TryParseOutputsInjectedOutput()
        {
            var sut = new CachingParser<string>(_inner);

            sut.TryParse(TrueStat, out var actualRemaining, out var actualParsed);

            Assert.AreEqual(TrueRemaining, actualRemaining);
            Assert.AreEqual(TrueParsed, actualParsed);
        }

        [Test]
        public void TryParseCachesSingleStat()
        {
            var sut = new CachingParser<string>(_inner);

            sut.TryParse(TrueStat, out var _, out var _);
            sut.TryParse(TrueStat, out var _, out var _);

            string _;
            _innerMock.Verify(p => p.TryParse(TrueStat, out _, out _), Times.Once);
        }

        [Test]
        public void TryParsesCachesMultipleStats()
        {
            var sut = new CachingParser<string>(_inner);

            sut.TryParse(TrueStat, out var _, out var _);
            sut.TryParse(FalseStat, out var _, out var _);
            sut.TryParse(FalseStat, out var _, out var _);
            sut.TryParse("whatever", out var _, out var _);
            sut.TryParse(TrueStat, out var _, out var _);
            sut.TryParse(TrueStat, out var _, out var _);
            sut.TryParse("whatever", out var _, out var _);

            string _;
            _innerMock.Verify(p => p.TryParse(TrueStat, out _, out _), Times.Once);
            _innerMock.Verify(p => p.TryParse(FalseStat, out _, out _), Times.Once);
            _innerMock.Verify(p => p.TryParse("whatever", out _, out _), Times.Once);
        }
    }
}