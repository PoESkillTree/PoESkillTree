using System.Collections.Generic;
using Moq;
using NUnit.Framework;
using PoESkillTree.Computation.Parsing.Data;

namespace PoESkillTree.Computation.Parsing.Tests
{
    [TestFixture]
    public class StatReplacingParserTest
    {
        private IReadOnlyList<StatReplacerData> _statReplacers;

        [SetUp]
        public void SetUp()
        {
            _statReplacers = new[]
            {
                new StatReplacerData("Plain Stat", new[] { "stat1", "stat2", "stat3" }),
                new StatReplacerData("(grouped) (stat)", new[] { "$1", "$2", "$0" }),
                new StatReplacerData(".*[ck]omplex (.*) (regexp|regex)",
                    new[] { "complex", "$1", "$2" }),
                new StatReplacerData("removed", new string[0]), 
            };
        }

        [Test]
        public void IsIParserIReadOnlyList()
        {
            var sut = new StatReplacingParser<string>(null, _statReplacers);

            Assert.IsInstanceOf<IParser<IReadOnlyList<string>>>(sut);
        }

        [TestCase(true, ExpectedResult = true)]
        [TestCase(false, ExpectedResult = false)]
        public bool TryParseWithoutReplacementReturnsInnerReturn(bool innerReturn)
        {
            var innerMock = new Mock<IParser<string>>();
            string _;
            innerMock.Setup(p => p.TryParse("stat", out _, out _))
                .Returns(innerReturn);
            var sut = new StatReplacingParser<string>(innerMock.Object, _statReplacers);

            return sut.TryParse("stat", out var _, out var _);
        }

        [Test]
        public void TryParseWithoutReplacementOutputsInnerResult()
        {
            var innerMock = new Mock<IParser<string>>();
            var result = "result";
            string _;
            innerMock.Setup(p => p.TryParse("stat", out _, out result));
            var sut = new StatReplacingParser<string>(innerMock.Object, _statReplacers);

            sut.TryParse("stat", out var _, out var actualResult);

            Assert.That(actualResult, Has.Exactly(1).EqualTo(result));
        }

        [Test]
        public void TryParseWithoutReplacementOutputsInnerRemaining()
        {
            var innerMock = new Mock<IParser<string>>();
            var remaining = "remaining";
            string _;
            innerMock.Setup(p => p.TryParse("stat", out remaining, out _));
            var sut = new StatReplacingParser<string>(innerMock.Object, _statReplacers);

            sut.TryParse("stat", out var actualRemaining, out var _);

            Assert.AreEqual(remaining, actualRemaining);
        }

        [TestCase(true, true, true, ExpectedResult = true)]
        [TestCase(true, true, false, ExpectedResult = false)]
        [TestCase(true, false, true, ExpectedResult = false)]
        [TestCase(false, true, true, ExpectedResult = false)]
        [TestCase(false, false, false, ExpectedResult = false)]
        public bool TryParseWithManyReplacementsReturnsCorrectValue(params bool[] returns)
        {
            var innerMock = new Mock<IParser<string>>();
            string _;
            innerMock.Setup(p => p.TryParse("stat1", out _, out _))
                .Returns(returns[0]);
            innerMock.Setup(p => p.TryParse("stat2", out _, out _))
                .Returns(returns[1]);
            innerMock.Setup(p => p.TryParse("stat3", out _, out _))
                .Returns(returns[2]);
            var sut = new StatReplacingParser<string>(innerMock.Object, _statReplacers);

            return sut.TryParse("plain stat", out var _, out var _);
        }

        [Test]
        public void TryParseWithManyReplacementsOutputsListOfInnerResults()
        {
            var innerMock = new Mock<IParser<string>>();
            string _;
            var result1 = "r1";
            innerMock.Setup(p => p.TryParse("stat1", out _, out result1));
            var result2 = "r2";
            innerMock.Setup(p => p.TryParse("stat2", out _, out result2));
            var result3 = "r3";
            innerMock.Setup(p => p.TryParse("stat3", out _, out result3));
            var sut = new StatReplacingParser<string>(innerMock.Object, _statReplacers);

            sut.TryParse("plain stat", out var _, out var actual);

            CollectionAssert.AreEqual(new[] { result1, result2, result3 }, actual);
        }

        [Test]
        public void TryParseWithManyReplacementsOutputsJoinedInnerRemaining()
        {
            var innerMock = new Mock<IParser<string>>();
            string _;
            var remaining1 = "r1";
            innerMock.Setup(p => p.TryParse("stat1", out remaining1, out _));
            var remaining2 = "r2";
            innerMock.Setup(p => p.TryParse("stat2", out remaining2, out _));
            var remaining3 = "r3";
            innerMock.Setup(p => p.TryParse("stat3", out remaining3, out _));
            var sut = new StatReplacingParser<string>(innerMock.Object, _statReplacers);

            sut.TryParse("plain stat", out var actual, out var _);

            Assert.AreEqual(remaining1 + "\n" + remaining2 + "\n" + remaining3, actual);
        }

        [Test]
        public void TryParseWithEmptyReplacementReturnsTrue()
        {
            var sut = new StatReplacingParser<string>(null, _statReplacers);

            var actual = sut.TryParse("removed", out var _, out var _);

            Assert.True(actual);
        }

        [Test]
        public void TryParseWithEmptyReplacementOutputsEmptyListAsResult()
        {
            var sut = new StatReplacingParser<string>(null, _statReplacers);

            sut.TryParse("removed", out var _, out var actual);

            CollectionAssert.IsEmpty(actual);
        }

        [Test]
        public void TryParseWithEmptyReplacementOutputsEmptyStringAsRemaining()
        {
            var sut = new StatReplacingParser<string>(null, _statReplacers);

            sut.TryParse("removed", out var actual, out var _);

            CollectionAssert.IsEmpty(actual);
        }

        [TestCase("plain stat", new[] { "stat1", "stat2", "stat3" })]
        [TestCase("Plain Stat", new[] { "stat1", "stat2", "stat3" })]
        [TestCase("grouped stat", new[] { "grouped", "stat", "grouped stat" })]
        [TestCase("very complex asd regex", new[] { "complex", "asd", "regex" })]
        [TestCase("removed", new string[0])]
        [TestCase("unknown stat", new[] { "unknown stat" })]
        public void TryParseCallsInnerCorrectly(string stat, string[] expectedParts)
        {
            var innerMock = new Mock<IParser<string>>();
            var sut = new StatReplacingParser<string>(innerMock.Object, _statReplacers);

            sut.TryParse(stat, out var _, out var _);

            foreach (var expectedPart in expectedParts)
            {
                string _;
                innerMock.Verify(p => p.TryParse(expectedPart, out _, out _));
            }
        }

        [Test]
        public void TryParseIgnoresWhitespaceRemainings()
        {
            var innerMock = new Mock<IParser<string>>();
            string _;
            var remaining1 = "\t \n";
            innerMock.Setup(p => p.TryParse("stat1", out remaining1, out _));
            var remaining2 = "";
            innerMock.Setup(p => p.TryParse("stat2", out remaining2, out _));
            var remaining3 = "r3";
            innerMock.Setup(p => p.TryParse("stat3", out remaining3, out _));
            var sut = new StatReplacingParser<string>(innerMock.Object, _statReplacers);

            sut.TryParse("plain stat", out var actual, out var _);

            Assert.AreEqual(remaining3, actual);
        }

        [Test]
        public void TryParseMustFindFullMatchToReplaceStat()
        {
            var innerMock = new Mock<IParser<string>>();
            var sut = new StatReplacingParser<string>(innerMock.Object, _statReplacers);

            sut.TryParse("plain stat something", out var _, out var _);
            
            string _;
            innerMock.Verify(p => p.TryParse("plain stat something", out _, out _));
        }
    }
}